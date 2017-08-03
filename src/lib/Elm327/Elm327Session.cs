namespace DP.Tinast.Elm327
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Devices.Bluetooth.Rfcomm;
    using Windows.Devices.Enumeration;
    using Windows.Foundation;
    using Windows.Networking.Sockets;
    using Windows.Storage;
    using Windows.Storage.Streams;
    using Config;
    using Elm327;
    using Interfaces;
    using MetroLog;

    /// <summary>
    /// Represent an ELM 327 parser.
    /// </summary>
    public class Elm327Session
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<Elm327Session>();

        /// <summary>
        /// The connection
        /// </summary>
        private IElm327Connection connection;

        /// <summary>
        /// The read buffer
        /// </summary>
        private Windows.Storage.Streams.Buffer readBuffer = new Windows.Storage.Streams.Buffer(1 << 12);

        /// <summary>
        /// The debug data task
        /// </summary>
        private PidDebugData debugData = new PidDebugData(string.Empty, new string[0], TimeSpan.Zero);

        /// <summary>
        /// Initializes a new instance of the <see cref="Elm327Session"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public Elm327Session(IElm327Connection connection)
        {
            this.connection = connection;
        }

        /// <summary>
        /// Gets the debug data.
        /// </summary>
        /// <value>
        /// The debug data.
        /// </value>
        public PidDebugData DebugData
        {
            get
            {
                return this.debugData;
            }
        }

        /// <summary>
        /// Sends the command.
        /// </summary>
        /// <param name="commandString">The command string.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public async Task<string[]> SendCommandAsync(string commandString, CancellationToken token)
        {
            DateTime start = DateTime.Now;
            byte[] outBuf = Encoding.ASCII.GetBytes(commandString + "\r");
            await this.connection.OutputStream.WriteAsync(outBuf.AsBuffer());
            string[] ret = await this.ReadResponseAsync(token);
            this.debugData = new PidDebugData(commandString, ret, DateTime.Now - start);
            this.log.Trace(this.debugData.ToString());
            return ret;
        }

        /// <summary>
        /// Runs a PID against the ECU.
        /// </summary>
        /// <param name="pid">The PID string.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>An array of pid values.</returns>
        public async Task<List<int>> RunPidAsync(string pid, CancellationToken token)
        {
            List<int> pr = new List<int>();
            string[] r = await this.SendCommandAsync(pid, token);
            if (!r[r.Length - 1].Equals("UNABLE TO CONNECT") && !r[r.Length - 1].Equals("NO DATA"))
            {
                bool multiline = false;
                for (int i = 0; i < r.Length; ++i)
                {
                    if (r[i] != "SEARCHING...")
                    {
                        if (r[i].Length > 1)
                        {
                            int j = 0;
                            if (r[i][1] == ':')
                            {
                                // This is part of a line segment indicator...?
                                multiline = true;
                                j = 3;
                            }

                            while (j < r[i].Length)
                            {
                                int next = r[i].IndexOf(' ', j);
                                string str = r[i].Substring(j, next - j);
                                pr.Add(Convert.ToInt32(str, 16));
                                j = next + 1;
                            }
                        }
                    }
                }

                // In a multi-line response, the first digit is the number of bytes in the response.
                if (multiline)
                {
                    List<int> mPr = new List<int>(pr[0]);
                    for (int i = 1; i <= pr[0]; ++i)
                    {
                        mPr.Add(pr[i]);
                    }

                    pr = mPr;
                }
            }

            return pr;
        }

        /// <summary>
        /// Reads a line off the input socket.
        /// </summary>
        /// <returns></returns>
        private async Task<string[]> ReadResponseAsync(CancellationToken token)
        {
            using (CancellationTokenRegistration ctr = token.Register(async () => await this.connection.CancelAsync()))
            {
                List<char> cb = new List<char>();
                List<string> sr = new List<string>();
                for (;;)
                {
                    byte[] read = (await this.connection.InputStream.ReadAsync(this.readBuffer, this.readBuffer.Capacity, InputStreamOptions.Partial))
                                                .ToArray();
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < read.Length; ++i)
                    {
                        if (read[i] == '>')
                        {
                            int sCur = 0;
                            while (sCur < cb.Count)
                            {
                                int sEnd = cb.IndexOf('\r', sCur);
                                if (sEnd < 0)
                                {
                                    break;
                                }
                                else if (sEnd > sCur)
                                {
                                    int sLen = sEnd - sCur;
                                    char[] sa = new char[sLen];
                                    cb.CopyTo(sCur, sa, 0, sLen);
                                    string s = new string(sa, 0, sLen);
                                    if (s.Equals("STOPPED"))
                                    {
                                        throw new IOException("ELM327 device stopped.");
                                    }

                                    sr.Add(s);
                                }

                                sCur = sEnd + 1;
                            }

                            return sr.ToArray();
                        }
                        else
                        {
                            cb.Add((char)read[i]);
                        }
                    }
                }
            }
        }
    }
}
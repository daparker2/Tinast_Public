namespace DP.Tinast.Elm327
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Text;
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
    /// Represent the ELM327 driver for the gauge panel
    /// </summary>
    /// <seealso cref="DP.Tinast.Interfaces.IDisplayDriver" />
    public class Elm327Driver : IDisplayDriver
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<Elm327Driver>();

        /// <summary>
        /// The configuration
        /// </summary>
        private DisplayConfiguration config;

        /// <summary>
        /// The connection
        /// </summary>
        private IElm327Connection connection;

        /// <summary>
        /// The PID result
        /// </summary>
        private PidResult result = new PidResult();

        /// <summary>
        /// The PID table
        /// </summary>
        private PidTable pt = new PidTable();

        /// <summary>
        /// The read buffer
        /// </summary>
        private Windows.Storage.Streams.Buffer readBuffer = new Windows.Storage.Streams.Buffer(1 << 12);

        /// <summary>
        /// The debug data task
        /// </summary>
        private PidDebugData debugData = new PidDebugData(string.Empty, new string[0], TimeSpan.Zero);

        /// <summary>
        /// Initializes a new instance of the <see cref="Elm327Driver"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="connection">The ELM 327 connection instance.</param>
        public Elm327Driver(DisplayConfiguration config, IElm327Connection connection)
        {
            this.config = config;
            this.connection = connection;

            if (this.config.MaxPidsAtOnce < 1)
            {
                this.config.MaxPidsAtOnce = 1;
            }

            if (this.config.AfrPidType == PidType.Obd2)
            {
                this.pt.Add(new PidHandler(0x0134, PidRequest.Afr, 4, (pd) => this.result.Afr = (double)(pd[0] * 256 + pd[1]) / 32768.0 * 14.7));
            }
            else
            {
                throw new InvalidOperationException("Invalid AFR pid type");
            }

            if (this.config.BoostPidType == PidType.Obd2)
            {
                // We could calculate this based on barometric pressure PID but this is slightly faster.
                this.pt.Add(new PidHandler(0x010b, PidRequest.Boost, 1, (pd) => this.result.Boost = (double)pd[0] * 0.145037738007 + this.config.BoostOffset));
            }
            else
            {
                throw new InvalidOperationException("Invalid boost pid type");
            }

            if (this.config.LoadPidType == PidType.Obd2)
            {
                this.pt.Add(new PidHandler(0x0104, PidRequest.Load, 1, (pd) => this.result.Load = pd[0] * 100 / 255));
            }
            else
            {
                throw new InvalidOperationException("Invalid load pid type");
            }

            if (this.config.OilTempPidType == PidType.Obd2)
            {
                this.pt.Add(new PidHandler(0x015c, PidRequest.OilTemp, 1, (pd) => this.result.OilTemp = (int)this.CToF(pd[0] - 40)));
            }
            else if (this.config.OilTempPidType == PidType.Subaru)
            {
                this.pt.Add(new PidHandler(0x2101, PidRequest.OilTemp, 29, (pd) => this.result.OilTemp = (int)this.CToF(pd[28] - 40)));
            }
            else
            {
                throw new InvalidOperationException("Invalid AFR pid type");
            }

            if (this.config.CoolantTempPidType == PidType.Obd2)
            {
                this.pt.Add(new PidHandler(0x0105, PidRequest.CoolantTemp, 1, (pd) => this.result.CoolantTemp = (int)this.CToF(pd[0] - 40)));
            }
            else
            {
                throw new InvalidOperationException("Invalid AFR pid type");
            }

            if (this.config.IntakeTempPidType == PidType.Obd2)
            {
                this.pt.Add(new PidHandler(0x010f, PidRequest.IntakeTemp, 1, (pd) => this.result.IntakeTemp = (int)this.CToF(pd[0] - 40)));
            }
            else
            {
                throw new InvalidOperationException("Invalid AFR pid type");
            }
        }

        /// <summary>
        /// Tries connecting to the OBD2 ELM327 interface.
        /// </summary>
        /// <exception cref="ConnectFailedException">Occurs if the connection fails.</exception>
        public async Task OpenAsync()
        {
            try
            {
                await this.connection.OpenAsync();

                // Get some info about the device we just connected to.
                string elmDeviceDesc = (await this.SendCommand("atz")).FirstOrDefault();
                this.log.Trace("Connected to device: {0}", elmDeviceDesc ?? "<reconnected>");

                await this.SetDefaults();

                while (!(await this.SendCommand("atsp0")).Contains("OK")) ;

                this.log.Info("ELM327 device connected. ECU on.");
            }
            catch (Exception ex)
            {
                throw new ConnectFailedException("Connect failed.", ex);
            }
        }

        /// <summary>
        /// Closes the connection to the OBD2 ELM327 interface.
        /// </summary>
        /// <returns></returns>
        public void Close()
        {
            this.connection.Close();
        }

        /// <summary>
        /// Gets the last transaction information, which in most cases will be the command sent to GetPidResultAsync.
        /// </summary>
        /// <returns>
        /// A <see cref="PidDebugData" /> object representing the last transaction.
        /// </returns>
        public PidDebugData GetLastTransactionInfo()
        {
            return this.debugData;
        }

        /// <summary>
        /// Gets the PID result for the specific PID request from the ECU.
        /// </summary>
        /// <param name="request">The PID request.</param>
        /// <returns>A <see cref="PidResult"/> object.</returns>
        /// <exception cref="ConnectFailedException">Occurs if the connection fails.</exception>
        public async Task<PidResult> GetPidResultAsync(PidRequest request)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                int cPids = 0;
                int mode = 0;
                foreach (PidHandler ph in this.pt.GetHandlersForRequest(request))
                {
                    int curPid = ph.Mode;
                    int pidMode = (curPid & 0xFF00) >> 8;
                    if (pidMode != mode)
                    {
                        if (cPids > 0)
                        {
                            await this.UpdatePidResult(sb.ToString());
                            cPids = 0;
                        }

                        sb.Clear();
                        sb.AppendFormat("{0:X2}", pidMode);
                        mode = pidMode;
                    }

                    int pidValue = curPid & 0xFF;
                    sb.AppendFormat("{0:X2}", pidValue);
                    ++cPids;

                    if (cPids == this.config.MaxPidsAtOnce)
                    {
                        await this.UpdatePidResult(sb.ToString());
                        cPids = 0;
                        mode = 0;
                    }
                }

                if (cPids > 0)
                {
                    await this.UpdatePidResult(sb.ToString());
                }

                return this.result;
            }
            catch (Exception ex)
            {
                throw new ConnectFailedException("Pid request failed.", ex);
            }
        }

        /// <summary>
        /// Updates the <see cref="PidResult"/> object.
        /// </summary>
        /// <param name="pidRequest">The PID request.</param>
        /// <returns>A PID result.</returns>
        private async Task UpdatePidResult(string pidRequest)
        {
            List<int> pidResult = await this.RunPid(pidRequest);
            try
            {
                if (pidResult.Count > 0)
                {
                    int mode = pidResult[0] - 0x40;
                    for (int i = 1; i < pidResult.Count; ++i)
                    {
                        PidHandler ph = this.pt.GetHandler((mode << 8) | pidResult[i]);
                        i += ph.Handle(pidResult, i + 1);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException("Failed to get PID result.", ex);
            }
        }

        /// <summary>
        /// Sets the defaults scantool settings for the ELM 327 driver.
        /// </summary>
        /// <returns></returns>
        private async Task SetDefaults()
        {
            await this.SendCommand("ate0");
            await this.SendCommand("atsp0");

            // Only talk to ECU #1, which in most cases is the engine. That's the only one that we really care about.
            await this.SendCommand("atsh 7e0");

            if (this.config.AggressiveTiming)
            {
                await this.SendCommand("at2");
            }
        }

        /// <summary>
        /// Sends the command.
        /// </summary>
        /// <param name="commandString">The command string.</param>
        /// <returns></returns>
        private async Task<string[]> SendCommand(string commandString)
        {
            try
            {
                DateTime start = DateTime.Now;
                byte[] outBuf = Encoding.ASCII.GetBytes(commandString + "\r");
                await this.connection.OutputStream.WriteAsync(outBuf.AsBuffer());
                await this.connection.OutputStream.FlushAsync();
                string[] ret = await this.ReadResponse();
                this.debugData = new PidDebugData(commandString, ret, DateTime.Now - start);
                return ret;
            }
            catch (IOException ex)
            {
                this.log.Warn("Lost socket connection: {0}", ex.Message);
                this.debugData = new PidDebugData(commandString, new string[] { }, TimeSpan.MaxValue);
                throw;
            }
        }

        /// <summary>
        /// Reads a line off the input socket.
        /// </summary>
        /// <returns></returns>
        private async Task<string[]> ReadResponse()
        {
            try
            {
                List<char> cb = new List<char>();
                List<string> sr = new List<string>();
                for (;;)
                {
                    byte[] read = (await this.connection.InputStream.ReadAsync(this.readBuffer, this.readBuffer.Capacity, InputStreamOptions.Partial))
                                             .ToArray();
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
            catch (IOException ex)
            {
                this.log.Warn("Lost socket connection: {0}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Runs a PID against the ECU.
        /// </summary>
        /// <param name="pid">The PID string.</param>
        /// <param name="resultCount">The expected result count.</param>
        /// <returns>An array of pid values.</returns>
        private async Task<List<int>> RunPid(string pid)
        {
            List<int> pr = new List<int>();
            string[] r = await this.SendCommand(pid);
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
        /// Celsius to fahrenheit.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private double CToF(double v)
        {
            return v * 1.8 + 32.0;
        }
    }
}

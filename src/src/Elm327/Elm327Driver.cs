namespace DP.Tinast.Elm327
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Devices.Bluetooth.Rfcomm;
    using Windows.Devices.Enumeration;
    using Windows.Foundation;
    using Windows.Networking.Sockets;
    using Config;
    using Elm327;
    using Interfaces;
    using MetroLog;

    /// <summary>
    /// Represent the ELM327 driver for the gauge panel
    /// </summary>
    /// <seealso cref="DP.Tinast.Interfaces.IDisplayDriver" />
    /// <seealso cref="System.IDisposable" />
    class Elm327Driver : IDisplayDriver, IDisposable
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<Elm327Driver>();

        /// <summary>
        /// The disposed
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// The configuration
        /// </summary>
        private DisplayConfiguration config;

        /// <summary>
        /// The chat socket
        /// </summary>
        private StreamSocket socket;

        /// <summary>
        /// The chat service
        /// </summary>
        private RfcommDeviceService service;

        /// <summary>
        /// The reader
        /// </summary>
        private StreamReader reader;

        /// <summary>
        /// The writer
        /// </summary>
        private StreamWriter writer;

        /// <summary>
        /// The socket connected
        /// </summary>
        private bool socketConnected = false;

        /// <summary>
        /// The PID result
        /// </summary>
        private PidResult result = new PidResult();

        /// <summary>
        /// The PID table
        /// </summary>
        private PidTable pt = new PidTable();

        /// <summary>
        /// Initializes a new instance of the <see cref="Elm327Driver"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public Elm327Driver(DisplayConfiguration config)
        {
            this.config = config;

            if (this.config.MaxPidsAtOnce < 1)
            {
                this.config.MaxPidsAtOnce = 1;
            }

            if (this.config.AfrPidType == PidType.Obd2)
            {
                this.pt.Add(new PidHandler(0x0134, PidRequest.Afr, 2, (pd) => this.result.Afr = ((double)pd[0] * 256.0 + (double)pd[1]) / 32768.0 * 14.7));
            }
            else
            {
                throw new InvalidOperationException("Invalid AFR pid type");
            }

            if (this.config.BoostPidType == PidType.Obd2)
            {
                this.pt.Add(new PidHandler(0x010b, PidRequest.Boost, 1, (pd) => this.result.Boost = (int)((double)pd[0] * 0.000145037738007)));
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
                this.pt.Add(new PidHandler(0x015c, PidRequest.OilTemp, 1, (pd) => this.result.IntakeTemp = (int)this.CToF(pd[0] - 40)));
            }
            else
            {
                throw new InvalidOperationException("Invalid AFR pid type");
            }

            if (this.config.CoolantTempPidType == PidType.Obd2)
            {
                this.pt.Add(new PidHandler(0x0105, PidRequest.CoolantTemp, 1, (pd) => this.result.IntakeTemp = (int)this.CToF(pd[0] - 40)));
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
        /// Gets a value indicating whether this <see cref="IDisplayDriver"/> is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        public bool Connected
        {
            get
            {
                return this.socketConnected;
            }
        }

        /// <summary>
        /// Gets the last command.
        /// </summary>
        /// <value>
        /// The last command.
        /// </value>
        internal string LastCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the last response.
        /// </summary>
        /// <value>
        /// The last response.
        /// </value>
        internal List<string> LastResponse
        {
            get;
            private set;
        }

        /// <summary>
        /// Tries connecting to the OBD2 ELM327 interface.
        /// </summary>
        /// <returns>True if the connection was established.</returns>
        public async Task<bool> TryConnect()
        {
            if (!this.socketConnected)
            {
                if (this.service == null)
                {
                    DeviceInformationCollection serviceInfoCollection = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
                    if (serviceInfoCollection.Count != 1)
                    {
                        throw new InvalidOperationException("Only one paired RFComm device is supported at a time with this app and there must be at least one device.");
                    }

                    DeviceInformation deviceInfo = serviceInfoCollection.First();
                    this.log.Debug("BT device {0}", deviceInfo.Id);
                    this.service = await RfcommDeviceService.FromIdAsync(deviceInfo.Id);
                    if (this.service == null)
                    {
                        throw new InvalidOperationException("Access to the OBD2 device was denied.");
                    }
                }

                if (this.socket == null)
                {
                    this.socket = new StreamSocket();
                }

                this.log.Info("Connecting to {0};{1}", this.service.ConnectionHostName, this.service.ConnectionServiceName);
                try
                {
                    await this.socket.ConnectAsync(this.service.ConnectionHostName, this.service.ConnectionServiceName);
                }
                catch (Exception ex)
                {
                    this.log.Warn("Connect failed", ex);
                    if (this.socket != null)
                    {
                        this.socket.Dispose();
                        this.socket = null;
                    }

                    return false;
                }

                this.reader = new StreamReader(this.socket.InputStream.AsStreamForRead());
                this.writer = new StreamWriter(this.socket.OutputStream.AsStreamForWrite());

                // Get some info about the device we just connected to.
                string elmDeviceDesc = (await this.SendCommand("atz"))[1];
                this.log.Trace("Connected to device: {0}", elmDeviceDesc);

                await this.SendCommand("e0");
                await this.SendCommand("atsp0");
                if (this.config.AggressiveTiming)
                {
                    await this.SendCommand("at2");
                }

                while (!(await this.SendCommand("atsp0")).Contains("OK")) ;

                this.log.Info("ELM327 device connected. ECU on.");
                this.socketConnected = true;
            }

            return this.socketConnected;
        }

        /// <summary>
        /// Gets the PID result for the specific PID request from the ECU.
        /// </summary>
        /// <param name="request">The PID request.</param>
        /// <returns>A <see cref="PidResult"/> object.</returns>
        public async Task<PidResult> GetPidResult(PidRequest request)
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
                    for (int i = 0; i < pidResult.Count; ++i)
                    {
                        int mode = pidResult[i] - 0x40;
                        ++i;
                        PidHandler ph = this.pt.GetHandler((mode << 8) | pidResult[i]);
                        i += ph.Handle(pidResult, i);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException(string.Format("Failed to get PID result. PID: {0}. Last result:\n{1}", pidRequest, string.Join("\n", this.LastResponse)), ex);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disconnect the socket.
        /// </summary>
        public void Disconnect()
        {
            this.socketConnected = false;
            if (this.reader != null)
            {
                this.reader.Dispose();
                this.reader = null;
            }

            if (this.writer != null)
            {
                this.writer.Dispose();
                this.writer = null;
            }

            if (this.socket != null)
            {
                this.socket.Dispose();
                this.socket = null;
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;

                if (disposing)
                {
                    if (this.service != null)
                    {
                        this.service.Dispose();
                        this.service = null;
                    }

                    this.Disconnect();
                }
            }
        }

        /// <summary>
        /// Sends the command.
        /// </summary>
        /// <param name="commandString">The command string.</param>
        /// <returns></returns>
        private async Task<List<string>> SendCommand(string commandString)
        {
            try
            {
                this.LastCommand = commandString;
                await this.writer.WriteAsync(commandString);
                await this.writer.WriteAsync("\r");
                await this.writer.FlushAsync();
                return await this.ReadResponse();
            }
            catch (IOException ex)
            {
                this.log.Warn("Lost socket connection: {0}", ex.Message);
                this.Disconnect();
                throw;
            }
        }

        /// <summary>
        /// Reads a line off the input socket.
        /// </summary>
        /// <returns></returns>
        private async Task<List<string>> ReadResponse()
        {
            try
            {
                char[] buf = new char[1 << 8];
                List<char> cb = new List<char>();
                List<string> sr = new List<string>();
                for (;;)
                {
                    int len;
                    if ((len = await this.reader.ReadAsync(buf, 0, buf.Length)) > 0)
                    {
                        for (int i = 0; i < len; ++i)
                        {
                            if (buf[i] == '>')
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
                                        cb.CopyTo(sCur, buf, 0, sLen);
                                        string s = new string(buf, 0, sLen);
                                        if (s.Equals("STOPPED"))
                                        {
                                            throw new IOException("ELM327 device stopped.");
                                        }

                                        sr.Add(s);
                                    }

                                    sCur = sEnd + 1;
                                }

                                this.LastResponse = sr;
                                return sr;
                            }
                            else
                            {
                                cb.Add(buf[i]);
                            }
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                this.log.Warn("Lost socket connection: {0}", ex.Message);
                this.Disconnect();
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
            if (this.socketConnected)
            {
                List<int> pr = new List<int>();
                List<string> r = await this.SendCommand(pid);
                if (!r[r.Count - 1].Equals("UNABLE TO CONNECT") && !r[r.Count - 1].Equals("NO DATA"))
                {
                    for (int i = 1; i < r.Count; ++i)
                    {
                        if (r[i].Length > 1)
                        {
                            int j = 0;
                            if (r[i][1] == ':')
                            {
                                // This is part of a line segment indicator...?
                                j = 3;
                            }

                            for (; j < r[i].Length; j += 3)
                            {
                                pr.Add((this.ToDec(r[i][j]) << 4) | this.ToDec(r[i][j + 1]));
                            }
                        }
                    }
                }

                return pr;
            }

            return null;
        }

        /// <summary>
        /// Convert a character to decimal
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private int ToDec(char v)
        {
            if (v >= '0' && v <= '9')
            {
                return v - '0';
            }
            else if (v >= 'A' && v <= 'F')
            {
                return v - 'A';
            }
            else if (v >= 'a' && v <= 'f')
            {
                return v - 'a';
            }
            else
            {
                throw new FormatException("Invalid pid result. Last response: " + string.Join("\n", this.LastResponse));
            }
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

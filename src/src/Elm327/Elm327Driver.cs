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

    class Elm327Driver : IDisplayDriver, IDisposable
    {
        /// <summary>
        /// The connect timeout.
        /// </summary>
        const int ConnectTimeout = 30000;

        /// <summary>
        /// The write timeout.
        /// </summary>
        const int WriteTimeout = 5000;

        /// <summary>
        /// The read timeout.
        /// </summary>
        const int ReadTimeout = 5000;

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
        /// State variables
        /// </summary>
        private double boost, afr, load, oilTemp, coolantTemp, intakeTemp;

        /// <summary>
        /// Initializes a new instance of the <see cref="Elm327Driver"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public Elm327Driver(DisplayConfiguration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDisplayDriver"/> is resumed and can execute commands.
        /// </summary>
        /// <value>
        ///   <c>true</c> if resumed; otherwise, <c>false</c>.
        /// </value>
        public bool Resumed { get; private set; }

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
        /// Tries connecting to the OBD2 ELM327 interface.
        /// </summary>
        /// <returns>True if the connection was established.</returns>
        public async Task<bool> TryConnect()
        {
            if (this.Resumed && !this.socketConnected)
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
                Task connectTask = Task.Run(async () => await this.socket.ConnectAsync(this.service.ConnectionHostName, this.service.ConnectionServiceName));
                Task waited = await Task.WhenAny(connectTask, Task.Delay(ConnectTimeout));
                if (waited == connectTask)
                {
                    try
                    {
                        await connectTask;
                    }
                    catch (Exception ex)
                    {
                        this.log.Warn("Connect failed", ex);
                        return false;
                    }

                    this.reader = new StreamReader(this.socket.InputStream.AsStreamForRead());
                    this.writer = new StreamWriter(this.socket.OutputStream.AsStreamForWrite());

                    // Get some info about the device we just connected to.
                    this.log.Trace("Connected to device: {0}", await this.SendCommand("atz"));
                    while ((await this.SendCommand("atsp0")) != "OK") ;

                    this.log.Info("ELM327 device connected. ECU on.");
                    this.socketConnected = true;
                }
                else
                {
                    if (this.socket != null)
                    {
                        this.socket.Dispose();
                        this.socket = null;
                    }
                }
            }

            return this.socketConnected;
        }

        /// <summary>
        /// Gets the afr %.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public async Task<double> GetAfr()
        {
            if (this.Resumed && this.socketConnected)
            {
                if (this.config.AfrPidType == PidType.Obd2)
                {
                    int[] pidResult = await this.RunPid("0134", 2);
                    if (pidResult != null)
                    {
                        this.afr = ((double)pidResult[0] * 256.0 + (double)pidResult[1]) / 32768.0 * 14.7;
                    }
                }
            }

            return Math.Round(this.afr, 2);
        }

        /// <summary>
        /// Gets the boost in psi.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public async Task<int> GetBoost()
        {
            if (this.Resumed && this.socketConnected)
            {
                if (this.config.AfrPidType == PidType.Obd2)
                {
                    int[] pidResult = await this.RunPid("010B", 1);
                    if (pidResult != null)
                    {
                        // kpa -> psi
                        this.boost = (double)pidResult[0] * 0.000145037738007;
                    }
                }
            }

            return (int)this.boost;
        }

        /// <summary>
        /// Gets the load in %.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public async Task<int> GetLoad()
        {
            if (this.Resumed && this.socketConnected)
            {
                if (this.config.AfrPidType == PidType.Obd2)
                {
                    int[] pidResult = await this.RunPid("0104", 1);
                    if (pidResult != null)
                    {
                        this.load = (double)pidResult[0] * 100.0 / 255.0;
                    }
                }
            }

            return (int)this.load;
        }

        /// <summary>
        /// Gets the oil temp in F.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public async Task<int> GetOilTemp()
        {
            if (this.Resumed && this.socketConnected)
            {
                if (this.config.AfrPidType == PidType.Obd2)
                {
                    int[] pidResult = await this.RunPid("015C", 1);
                    if (pidResult != null)
                    {
                        this.oilTemp = this.CToF((double)pidResult[0] - 40.0);
                    }
                }
            }

            return (int)this.oilTemp;
        }

        /// <summary>
        /// Gets the coolant temp in F.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public async Task<int> GetCoolantTemp()
        {
            if (this.Resumed && this.socketConnected)
            {
                if (this.config.AfrPidType == PidType.Obd2)
                {
                    int[] pidResult = await this.RunPid("0105", 1);
                    if (pidResult != null)
                    {
                        this.coolantTemp = this.CToF((double)pidResult[0] - 40.0);
                    }
                }
            }

            return (int)this.coolantTemp;
        }

        /// <summary>
        /// Gets the intake temp in F.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public async Task<int> GetIntakeTemp()
        {
            if (this.Resumed && this.socketConnected)
            {
                if (this.config.AfrPidType == PidType.Obd2)
                {
                    int[] pidResult = await this.RunPid("010F", 1);
                    if (pidResult != null)
                    {
                        this.intakeTemp = this.CToF((double)pidResult[0] - 40.0);
                    }
                }
            }

            return (int)this.intakeTemp;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Resumes this driver instance.
        /// </summary>
        /// <returns></returns>
        public void Resume()
        {
            this.Resumed = true;
        }

        /// <summary>
        /// Suspends this driver instance.
        /// </summary>
        /// <returns></returns>
        public void Suspend()
        {
            this.Resumed = false;
            this.Disconnect();
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
        /// Disconnect the socket.
        /// </summary>
        private void Disconnect()
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
        /// Sends the command.
        /// </summary>
        /// <param name="commandString">The command string.</param>
        /// <returns></returns>
        private async Task<string> SendCommand(string commandString)
        {
            try
            {
                this.log.Trace("O: {0}", commandString);
                Task writeTask = Task.Run(async () =>
                {
                    await this.writer.WriteAsync(commandString + "\r");
                    await this.writer.FlushAsync();
                    await this.socket.OutputStream.FlushAsync();
                });

                Task waited = await Task.WhenAny(writeTask, Task.Delay(WriteTimeout));
                if (waited == writeTask)
                {
                    return await this.ReadResponse();
                }

                throw new IOException("ELM327 write timed out.");
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
        private async Task<string> ReadResponse()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                char[] buf = new char[1];
                for (;;)
                {
                    Task<int> readTask = this.reader.ReadAsync(buf, 0, 1);
                    Task waited = await Task.WhenAny(readTask, Task.Delay(ReadTimeout));
                    if (waited != readTask)
                    {
                        throw new IOException("ELM327 read timed out.");
                    }
                    else if (await readTask == 1)
                    {
                        if (buf[0] == '>')
                        {
                            string s = sb.ToString().Trim();
                            if (s.Contains("STOPPED"))
                            {
                                throw new IOException("ELM327 device stopped.");
                            }

                            sb.Clear();
                            if (!string.IsNullOrEmpty(s))
                            {
                                int iResponse = s.LastIndexOf('\r');
                                if (iResponse >= 0)
                                {
                                    s = s.Substring(iResponse + 1);
                                }

                                this.log.Trace("I: {0}", s);
                                return s;
                            }
                        }
                        else
                        {
                            sb.Append(buf[0]);
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
        private async Task<int[]> RunPid(string pid, int resultCount)
        {
            if (this.Connected)
            {
                string s1 = await this.SendCommand(pid);
                if (!s1.Equals("UNABLE TO CONNECT") && !s1.Equals("NO DATA"))
                {
                    List<int> pidValues = new List<int>();
                    try
                    {
                        string[] pidResult = s1.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 2; i < pidResult.Length; ++i)
                        {
                            pidValues.Add(Convert.ToInt32(pidResult[i], 16));
                        }

                        if (pidValues.Count != resultCount)
                        {
                            return null;
                        }
                    }
                    catch (FormatException ex)
                    {
                        this.Disconnect();
                        throw new IOException("Bad result for PID " + pid, ex);
                    }

                    return pidValues.ToArray();
                }
            }

            return null;
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

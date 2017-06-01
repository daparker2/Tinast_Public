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
        /// The chat service information collection
        /// </summary>
        private DeviceInformationCollection serviceInfoCollection;

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
        /// Tries connecting to the OBD2 ELM327 interface.
        /// </summary>
        /// <returns>True if the connection was established.</returns>
        public async Task<bool> TryConnect()
        {
            this.CheckIfSuspended();

            if (this.socketConnected)
            {
                return true;
            }

            DeviceInformation deviceInfo = null;
            if (this.serviceInfoCollection == null)
            {
                this.serviceInfoCollection = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
                foreach (DeviceInformation sppDeviceInfo in this.serviceInfoCollection)
                {
                    // Blargh. The BT device just shows up as "SPP"
                    this.log.Debug("BT device {0}", sppDeviceInfo.Id);
                    deviceInfo = sppDeviceInfo;
                    break;
                }
            }

            if (deviceInfo == null)
            {
                throw new InvalidOperationException("No compatible OBD2 devices found.");
            }

            if (this.service == null)
            {
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
                this.reader = new StreamReader(this.socket.InputStream.AsStreamForRead());
                this.writer = new StreamWriter(this.socket.OutputStream.AsStreamForWrite());

                // Get some info about the device we just connected to.
                await this.SendCommand("e0");
                string s = await this.SendCommand("atz");
                this.log.Info("ELM327 device info: {0}", s);
                this.socketConnected = true;
            }
            catch (Exception ex)
            {
                this.log.Error("Connect failed", ex);
            }

            return this.socketConnected;
        }

        /// <summary>
        /// Gets the afr %.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public async Task<double> GetAfr()
        {
            this.CheckIfSuspended();
            if (this.socketConnected)
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

            return this.afr;
        }

        /// <summary>
        /// Gets the boost in psi.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public async Task<double> GetBoost()
        {
            this.CheckIfSuspended();
            if (this.socketConnected)
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

            return this.boost;
        }

        /// <summary>
        /// Gets the load in %.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public async Task<double> GetLoad()
        {
            this.CheckIfSuspended();
            if (this.socketConnected)
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

            return this.load;
        }

        /// <summary>
        /// Gets the oil temp in F.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public async Task<double> GetOilTemp()
        {
            this.CheckIfSuspended();
            if (this.socketConnected)
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

            return this.oilTemp;
        }

        /// <summary>
        /// Gets the coolant temp in F.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public async Task<double> GetCoolantTemp()
        {
            this.CheckIfSuspended();
            if (this.socketConnected)
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

            return this.coolantTemp;
        }

        /// <summary>
        /// Gets the intake temp in F.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public async Task<double> GetIntakeTemp()
        {
            this.CheckIfSuspended();
            if (this.socketConnected)
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

            return this.intakeTemp;
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

            if (this.socket != null)
            {
                this.socket.Dispose();
                this.socket = null;
            }

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
        /// Checks if suspended.
        /// </summary>
        /// <exception cref="InvalidOperationException">Cannot perfor this command with the app suspended.</exception>
        private void CheckIfSuspended()
        {
            if (!this.Resumed)
            {
                throw new InvalidOperationException("Cannot perform this command with the app suspended.");
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
                await this.writer.WriteAsync(commandString + "\r");
                await this.writer.FlushAsync();
                await this.socket.OutputStream.FlushAsync();

                StringBuilder sb = new StringBuilder();
                int ch;
                while ((ch = this.reader.Read()) != '\r')
                {
                    sb.Append((char)ch);
                }

                string i = sb.ToString();
                sb = null;
                this.log.Trace("I: {0}", i);
                return i;
            }
            catch (IOException ex)
            {
                this.log.Warn("Lost socket connection: ", ex.Message);
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
            string result = await this.SendCommand(pid);
            List<int> pidValues = new List<int>();
            try
            {
                foreach (string pidValue in result.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    pidValues.Add(Convert.ToInt32(pidValue, 16));
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

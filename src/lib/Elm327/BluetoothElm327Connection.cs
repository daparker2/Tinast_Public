
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
    /// Represent a bluetooth-based ELM 327 connection.
    /// </summary>
    /// <seealso cref="DP.Tinast.Interfaces.IElm327Connection" />
    public class BluetoothElm327Connection : IElm327Connection, IDisposable
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<IElm327Connection>();

        /// <summary>
        /// The chat socket
        /// </summary>
        private StreamSocket socket;

        /// <summary>
        /// The device name
        /// </summary>
        private string deviceName;

        /// <summary>
        /// The socket connected
        /// </summary>
        private bool socketConnected = false;

        /// <summary>
        /// The disposed
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothElm327Connection"/> class.
        /// </summary>
        /// <param name="deviceName">Name of the device.</param>
        private BluetoothElm327Connection(string deviceName)
        {
            this.deviceName = deviceName;
        }

        /// <summary>
        /// Gets the device name.
        /// </summary>
        /// <value>
        /// The device name.
        /// </value>
        public string DeviceName
        {
            get
            {
                return this.deviceName;
            }
        }

        /// <summary>
        /// Gets the input stream.
        /// </summary>
        /// <value>
        /// The input stream.
        /// </value>
        public IInputStream InputStream
        {
            get
            {
                if (!this.socketConnected)
                {
                    return null;
                }

                return this.socket.InputStream;
            }
        }

        /// <summary>
        /// Gets the output stream.
        /// </summary>
        /// <value>
        /// The output stream.
        /// </value>
        public IOutputStream OutputStream
        {
            get
            {
                if (!this.socketConnected)
                {
                    return null;
                }

                return this.socket.OutputStream;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IElm327Connection" /> is opened.
        /// </summary>
        /// <value>
        ///   <c>true</c> if opened; otherwise, <c>false</c>.
        /// </value>
        public bool Opened
        {
            get
            {
                return this.socketConnected;
            }
        }

        /// <summary>
        /// Gets the available connections.
        /// </summary>
        /// <returns></returns>
        public static async Task<ICollection<BluetoothElm327Connection>> GetAvailableConnectionsAsync()
        {
            List<BluetoothElm327Connection> ret = new List<BluetoothElm327Connection>();
            DeviceInformationCollection serviceInfoCollection = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort), new string[] { "System.Devices.AepService.AepId" });
            foreach (DeviceInformation serviceInfo in serviceInfoCollection)
            {
                RfcommDeviceService service = await RfcommDeviceService.FromIdAsync(serviceInfo.Id);
                if (service != null)
                {
                    DeviceAccessStatus status = await service.RequestAccessAsync();
                    if (status == DeviceAccessStatus.Allowed)
                    {
                        DeviceInformation aepInfo = await DeviceInformation.CreateFromIdAsync((string)serviceInfo.Properties["System.Devices.AepService.AepId"]);
                        ret.Add(new BluetoothElm327Connection(aepInfo.Name));
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Opens the connection asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task OpenAsync()
        {
            if (this.socketConnected)
            {
                throw new InvalidOperationException("Socket already connected.");
            }

            DeviceInformationCollection serviceInfoCollection = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort), new string[] { "System.Devices.AepService.AepId" });
            foreach (DeviceInformation serviceInfo in serviceInfoCollection)
            {
                DeviceInformation aepInfo = await DeviceInformation.CreateFromIdAsync((string)serviceInfo.Properties["System.Devices.AepService.AepId"]);

                this.log.Debug("BT device {0}", aepInfo.Name);
                if (aepInfo.Name == this.deviceName)
                {
                    DevicePairingResult pairingResult = await aepInfo.Pairing.Custom.PairAsync(DevicePairingKinds.ConfirmOnly, DevicePairingProtectionLevel.None);
                    this.log.Debug("Device pairing status: {0}, protection level: {1}", pairingResult.Status, pairingResult.ProtectionLevelUsed);

                    RfcommDeviceService service = await RfcommDeviceService.FromIdAsync(serviceInfo.Id);
                    if (service == null)
                    {
                        throw new InvalidOperationException("Access to the OBD2 device was denied.");
                    }

                    this.socket = new StreamSocket();
                    this.socket.Control.NoDelay = true;
                    this.socket.Control.SerializeConnectionAttempts = true;

                    Exception connectException = null;
                    for (int i = 0; i < 3; ++i)
                    {
                        this.log.Info("Connecting to {0};{1}, attempt {2}", service.ConnectionHostName, service.ConnectionServiceName, i + 1);
                        try
                        {
                            await this.socket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName, SocketProtectionLevel.PlainSocket);
                            this.socketConnected = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            this.log.Error("Connect failed.", ex);
                            ex = connectException;
                        }
                    }

                    if (!this.socketConnected)
                    {
                        throw new ConnectFailedException("Failed to connect to the OBD2 adapter.", connectException);
                    }

                    return;
                }
            }

            throw new ConnectFailedException("Device not found. It may have been removed.");
        }

        /// <summary>
        /// Closes the connection asynchronously.
        /// </summary>
        /// <returns></returns>
        public void Close()
        {
            if (!this.socketConnected)
            {
                throw new InvalidOperationException("Socket not opened.");
            }

            this.socketConnected = false;
            this.socket.Dispose();
            this.socket = null;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} ({1})", base.ToString(), this.deviceName);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <returns></returns>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;
                if (disposing)
                {
                    if (this.socket != null)
                    {
                        this.socket.Dispose();
                        this.socket = null;
                    }
                }
            }
        }
    }
}

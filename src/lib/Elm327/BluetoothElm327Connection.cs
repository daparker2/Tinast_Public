﻿
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
    using System.Threading.Tasks;
    using Windows.Devices.Bluetooth.Rfcomm;
    using Windows.Devices.Enumeration;
    using Windows.Devices.Radios;
    using Windows.Foundation;
    using Windows.Networking;
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
        /// The host name
        /// </summary>
        private HostName hostName;

        /// <summary>
        /// The service name
        /// </summary>
        private string serviceName;

        /// <summary>
        /// The socket connected
        /// </summary>
        private bool socketConnected = false;

        /// <summary>
        /// Was the socket ever connected to apply our magic workaround
        /// </summary>
        private bool wasEverConnected = false;

        /// <summary>
        /// The disposed
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothElm327Connection"/> class.
        /// </summary>
        /// <param name="deviceName">Name of the device.</param>
        /// <param name="hostName">The connection hostname.</param>
        /// <param name="serviceName">The connection service name.</param>
        private BluetoothElm327Connection(string deviceName, HostName hostName, string serviceName)
        {
            this.deviceName = deviceName;
            this.hostName = hostName;
            this.serviceName = serviceName;
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
                using (RfcommDeviceService service = await RfcommDeviceService.FromIdAsync(serviceInfo.Id))
                {
                    if (service != null)
                    {
                        DeviceAccessStatus status = await service.RequestAccessAsync();
                        if (status == DeviceAccessStatus.Allowed)
                        {
                            DeviceInformation aepInfo = await DeviceInformation.CreateFromIdAsync((string)serviceInfo.Properties["System.Devices.AepService.AepId"]);
                            ret.Add(new BluetoothElm327Connection(aepInfo.Name, service.ConnectionHostName, service.ConnectionServiceName));
                        }
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
                this.socketConnected = false;
                this.socket.Dispose();
                this.socket = null;
            }

            this.log.Info("Connecting to '{0}'", this.deviceName);
            this.socket = new StreamSocket();
            try
            {
                await this.socket.ConnectAsync(this.hostName, this.serviceName);
                this.wasEverConnected = this.socketConnected = true;
            }
            catch (Exception ex)
            {
                this.log.Error("Socket connect failed", ex);
                if (this.wasEverConnected)
                {
                    this.log.Info("Trying magic BT workaround");
                    await this.ResetRadioAsync().ConfigureAwait(false);
                }

                throw;
            }
        }

        /// <summary>
        /// Resets the radio.
        /// </summary>
        /// <returns></returns>
        private async Task ResetRadioAsync()
        {
            foreach (Radio radio in await Radio.GetRadiosAsync())
            {
                if (radio.Kind == RadioKind.Bluetooth)
                {
                    this.log.Debug("Found BT radio '{0}', state {1}", radio.Name, radio.State);
                    if (radio.State == RadioState.On || radio.State == RadioState.Off)
                    {
                        RadioState originalState = radio.State;
                        this.log.Info("Toggling BT radio '{0}'", radio.Name);
                        RadioAccessStatus offStatus = await radio.SetStateAsync(RadioState.Off);
                        await Task.Delay(200).ConfigureAwait(false);
                        if (radio.State != RadioState.Off)
                        {
                            this.log.Debug("Failed to turn radio off. Reason: {0}", offStatus);
                        }

                        RadioAccessStatus onStatus = await radio.SetStateAsync(RadioState.On);
                        if (radio.State == RadioState.On)
                        {
                            this.log.Debug("Radio state toggled from {0} -> {1}", originalState, radio.State);
                            await Task.Delay(200).ConfigureAwait(false);
                        }
                        else
                        {
                            this.log.Debug("Failed to turn radio on. Reason: {0}", onStatus);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} ({1})", base.ToString(), this.deviceName);
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
                this.socketConnected = false;
                if (disposing)
                {
                    if (this.socket != null)
                    {
                        this.log.Info("Closing connection to '{0}'", this.deviceName);
                        this.socketConnected = false;
                        this.socket.Dispose();
                        this.socket = null;
                    }
                }
            }
        }
    }
}

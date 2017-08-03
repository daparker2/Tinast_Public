
namespace Break_Raspberry_Pi_Bluetooth
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
    using Windows.Foundation.Collections;
    using Windows.Networking;
    using Windows.Networking.Sockets;
    using Windows.Storage;
    using Windows.Storage.Streams;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Navigation;
    using MetroLog;

    /*
     * This was designed to show repeated failures to connect to an RFCOMM endpoint
     * which appear to be unrecoverable without rebooting the system or unpairing
     * and re-pairing to the bluetooth device.
     * 
     * The test will kick off about one second after the app loads, because my
     * Raspberry Pi doesn't have a touchscreen or mouse connected. While some
     * RFCOMM failures are expected, the nature of the failure I'm encountering
     * it here makes it difficult to work around. Exceptions get logged to the
     * debug stream and to a file via MetroLog's StreamingFileTarget.
     * 
     * This is the setup I used for a repro:
     * 1. Raspberry Pi 3 Model B
     * 2. Paired to BAFX Products 34t5 Bluetooth OBDII Scan Tool for Android Devices 
     * 3. ScanTool 602201 ECUsim 2000 ECU CAN Simulator for OBD-II Development 
     * (this is just equipment I could easily reproduce the issue I was having on)
     * 4. Current Insider build of IoT core.
     * 
     * This setup worked completely fine and did not throw any exceptions:
     * 1. My laptop, Dell Lattitude 3379
     * 2. Paired to BAFX Products 34t5 Bluetooth OBDII Scan Tool for Android Devices 
     * 3. ScanTool 602201 ECUsim 2000 ECU CAN Simulator for OBD-II Development 
     * 4. Win10 1703
     * 
     * Dunno if there is an issue at the hardware level with the Raspberry Pi 
     * (I reproduced this on 3 different Pi boards) or at the BSP level for IoT
     * core, but this seems like it should be working code to me.
     * 
     */

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<App>();
        DispatcherTimer timer;
        string lastMessage = null;

        public MainPage()
        {
            this.InitializeComponent();
            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromSeconds(1);
            this.timer.Tick += Timer_Tick;
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        private async void Timer_Tick(object sender, object e)
        {
            await UpdateStatus();
        }

        private async void OnBreakBluetooth(object sender, RoutedEventArgs e)
        {
            await UpdateStatus();
        }

        async Task UpdateStatus()
        {
            this.timer.Stop();
            Exception ex = await BreakBluetooth();
            if (ex == null)
            {
                status.Text = "It didn't break.";
            }
            else
            {
                status.Text = ex.ToString();
                if (lastMessage == status.Text)
                {
                    status.Text += " (same as last time)";
                }

                lastMessage = status.Text;
            }
        }

        async Task<Exception> BreakBluetooth()
        {
            // If this throws the same exception consecutively, connect retries probably aren't working.
            this.log.Info("In BreakBluetooth()");
            Exception ret = null;
            int fails = 0;
            for (int i = 0; i < 100; i++)
            {
                using (RfcommDeviceService deviceService = await GetRfCommService())
                using (StreamSocket socket = new StreamSocket())
                {
                    try
                    {
                        this.log.Info("Connecting");
                        await socket.ConnectAsync(deviceService.ConnectionHostName, deviceService.ConnectionServiceName);
                        this.log.Debug("ConnectAsync() succeeded");

                        for (int j = 0; j < 100; ++j)
                        {
                            // If it's not clear, I want to read with a timeout (e.g., only block for a second
                            // so if the connection is flakey, I can update my app UI and try to reconnect 
                            byte[] outBuf = Encoding.ASCII.GetBytes("0103\r");
                            this.log.Debug("Sending PID 0103");
                            await socket.OutputStream.WriteAsync(outBuf.AsBuffer());
                            byte[] inBuf = new byte[1];
                            do
                            {
                                Task readTask = socket.InputStream.ReadAsync(inBuf.AsBuffer(), 1, InputStreamOptions.None).AsTask();
                                Task waited = await Task.WhenAny(Task.Delay(1000), readTask);
                                if (waited != readTask)
                                {
                                    this.log.Warn("Timed out");
                                    throw new TimeoutException();
                                }
                            }
                            while (inBuf[0] != '>'); // This is an ELM327 response EOL...
                            this.log.Debug("Got a valid response");
                        }
                    }
                    catch (Exception ex)
                    {
                        ret = ex;
                        ++fails;
                        log.Error("Exception in BreakBluetooth()", ex);
                    }
                }
            }

            this.log.Info("Leaving BreakBluetooth(), {0} fails", fails);
            return ret;
        }

        async Task<RfcommDeviceService> GetRfCommService()
        {
            DeviceInformationCollection serviceInfoCollection = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort), new string[] { "System.Devices.AepService.AepId" });
            foreach (DeviceInformation serviceInfo in serviceInfoCollection)
            {
                RfcommDeviceService service = await RfcommDeviceService.FromIdAsync(serviceInfo.Id);
                if (service != null)
                {
                    DeviceAccessStatus status = await service.RequestAccessAsync();
                    if (status == DeviceAccessStatus.Allowed)
                    {
                        this.log.Debug("Getting RFCOMM service for host {0}", service.ConnectionHostName);
                        return service;
                    }
                }
            }

            throw new InvalidOperationException();
        }
    }
}

namespace DP.Tinast
{
    using Microsoft.HockeyApp;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.ApplicationModel.Core;
    using Windows.Foundation;
    using Windows.Foundation.Collections;
    using Windows.System;
    using Windows.System.Profile;
    using Windows.UI.Core;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Navigation;
    using Config;
    using Elm327;
    using Interfaces;
    using MetroLog;
    using MetroLog.Targets;
    using ViewModel;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger log;

        /// <summary>
        /// The view model
        /// </summary>
        private DisplayViewModel viewModel;

        /// <summary>
        /// The viewmodel tick task
        /// </summary>
        private Task tickTask;

        /// <summary>
        /// The configuration
        /// </summary>
        private DisplayConfiguration config;

        /// <summary>
        /// The driver
        /// </summary>
        private IDisplayDriver driver;

        /// <summary>
        /// The loaded
        /// </summary>
        private bool resumed;

        /// <summary>
        /// The was ever connected
        /// </summary>
        private bool wasEverConnected = false;

        /// <summary>
        /// The is iot
        /// </summary>
        private bool isIot = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            ((App)Application.Current).Faulted += MainPage_Faulted;
            this.Loaded += MainPage_Loaded;
            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;
        }

        /// <summary>
        /// Handles the Loaded event of the MainPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.log = LogManagerFactory.DefaultLogManager.GetLogger<App>();
            AnalyticsVersionInfo versionInfo = AnalyticsInfo.VersionInfo;
            this.log.Info("Device Family: {0}, Version: {1}", versionInfo.DeviceFamily, versionInfo.DeviceFamilyVersion);
            if (versionInfo.DeviceFamily == "Windows.IoT")
            {
                this.log.Debug("Device is IoT.");
                this.isIot = true;
            }

            this.config = await ((App)Application.Current).GetConfigAsync();
            this.driver = await ((App)Application.Current).GetDriverAsync();
            this.viewModel = new DisplayViewModel(this.driver, this.config);
            this.DataContext = this.viewModel;
            this.StartTicking();
        }

        /// <summary>
        /// Resumes execution.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Current_Resuming(object sender, object e)
        {
            StartTicking();
        }

        /// <summary>
        /// Handles the Suspending event of the Current control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SuspendingEventArgs"/> instance containing the event data.</param>
        private async void Current_Suspending(object sender, SuspendingEventArgs e)
        {
            this.resumed = false;
            await this.tickTask;
        }

        /// <summary>
        /// Handles the Faulted event of the MainPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MainPage_Faulted(object sender, EventArgs e)
        {
            if (this.viewModel != null)
            {
                this.viewModel.Fault();
            }
        }

        /// <summary>
        /// Starts the ticking again.
        /// </summary>
        private void StartTicking()
        {
            if (!this.resumed)
            {
                this.resumed = true;
                this.tickTask = this.TickLoopAsync();
            }
        }

        /// <summary>
        /// The async tick loop.
        /// </summary>
        /// <returns></returns>
        private async Task TickLoopAsync()
        {
            Random r = new Random();
            Task logDelay = Task.Delay(2000 + r.Next(-200, 200));
            while (this.resumed)
            {
                bool tickError = false;
                try
                {
                    await Task.Yield();
                    await this.viewModel.Tick()
                                        .TimeoutAfter(this.viewModel.GetTickDuration())
                                        .ConfigureAwait(false);
                    if (logDelay.IsCompleted)
                    {
                        await logDelay;
                        PidDebugData transactionResult = this.driver.GetLastTransactionInfo();
                        this.log.Trace("{0}; {1}", transactionResult.ToString().Replace('\n', ','), this.viewModel);
                        logDelay = Task.Delay(2000);
                    }

                    //// This is a brutal hack to work around intermittent connection failures on the Raspberry pi with our Bluetooth interface.
                    //// If we were ever connected to the OBD2 interface, and we become disconnected,
                    //// Show a toast for 5 seconds and then reboot the system.

                    if (!this.viewModel.Obd2Connecting)
                    {
                        if (!this.wasEverConnected)
                        {
                            this.log.Debug("OBD2 connected.");
                            this.wasEverConnected = true;
                        }
                    }
                    else
                    {
                        if (this.wasEverConnected && this.isIot)
                        {
                            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                            {
                                await this.RebootSystem();
                            });
                        }
                    }
                }
                catch (TimeoutException)
                {
                    this.log.Error("Tick update timed out.");
                    tickError = true;
                }
                catch (ConnectFailedException ex)
                {
                    this.log.Error("Tick update error.", ex);
                    tickError = true;
                }

                if (tickError)
                {
                    PidDebugData transactionResult = driver.GetLastTransactionInfo();
                    this.log.Debug("Last transaction: {0}; {1}", transactionResult.ToString().Replace('\n', ','), this.viewModel);
                }
            }
        }

        /// <summary>
        /// Reboots the system.
        /// </summary>
        private async Task RebootSystem()
        {
            this.log.Warn("OBD2 disconnected. About to reboot the system.");
            PidDebugData transactionResult = this.driver.GetLastTransactionInfo();
            this.log.Debug("Last transaction: {0}; {1}", transactionResult.ToString().Replace('\n', ','), this.viewModel);

            TimeSpan restartTimeout;
            if (Debugger.IsAttached)
            {
                restartTimeout = TimeSpan.FromSeconds(60);
            }
            else
            {
                restartTimeout = TimeSpan.FromSeconds(5);
            }

            try
            {
                ShutdownManager.BeginShutdown(ShutdownKind.Restart, restartTimeout);
            }
            catch (UnauthorizedAccessException ex)
            {
                this.log.Warn("Shutdown attempt failed.", ex);
                return;
            }

            MessageDialog dialog = new MessageDialog(string.Format("The system will reboot in {0} seconds.", restartTimeout.TotalSeconds));
            dialog.Commands.Add(new UICommand("Abort Shutdown") { Id = 0 });
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 0;
            IUICommand result = await dialog.ShowAsync();

            // Doesn't matter what the result is. We returned, so continue execution.
            this.log.Info("Canceling shutdown.");
            ShutdownManager.CancelShutdown();
        }
    }
}

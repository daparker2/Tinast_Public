namespace DP.Tinast.Pages
{
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

    // Just let the GC clean up the cancellation token source.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public abstract class MainPageBase : Page
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
        /// The tick CTS
        /// </summary>
        private CancellationTokenSource tickCts;

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
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPageBase()
        {
            ((ITinastApp)Application.Current).Faulted += MainPage_Faulted;
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
            this.log = LogManagerFactory.DefaultLogManager.GetLogger<MainPageBase>();
            AnalyticsVersionInfo versionInfo = AnalyticsInfo.VersionInfo;
            this.log.Info("Device Family: {0}, Version: {1}", versionInfo.DeviceFamily, versionInfo.DeviceFamilyVersion);
            if (versionInfo.DeviceFamily == "Windows.IoT")
            {
                this.log.Debug("Device is IoT.");
            }

            this.config = await ((ITinastApp)Application.Current).GetConfigAsync();
            this.driver = await ((ITinastApp)Application.Current).GetDriverAsync();
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
            this.tickCts.Cancel();
            try
            {
                await this.tickTask;
            }
            catch (OperationCanceledException)
            {
            }

            if (this.tickCts != null)
            {
                this.tickCts.Dispose();
            }
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
            if (this.tickCts == null)
            {
                this.tickCts = new CancellationTokenSource();
                this.tickTask = this.viewModel.UpdateViewModelAsync(this.tickCts.Token);
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

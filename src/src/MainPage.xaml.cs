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
    using Windows.Foundation;
    using Windows.Foundation.Collections;
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
        /// The loaded
        /// </summary>
        private bool resumed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            this.log = LogManagerFactory.DefaultLogManager.GetLogger<App>();
            this.viewModel = new DisplayViewModel(((App)Application.Current).Driver, ((App)Application.Current).Config);
            this.DataContext = this.viewModel;
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
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            StartTicking();
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
        private async void MainPage_Faulted(object sender, EventArgs e)
        {
            if (this.viewModel != null)
            {
                await this.viewModel.Fault();
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
                IDisplayDriver driver = ((App)Application.Current).Driver;
                this.tickTask = this.TickLoopAsync(driver);
            }
        }

        /// <summary>
        /// The async tick loop.
        /// </summary>
        /// <returns></returns>
        private async Task TickLoopAsync(IDisplayDriver driver)
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
                        PidDebugData transactionResult = driver.GetLastTransactionInfo();
                        this.log.Trace("{0}; {1}", transactionResult.ToString().Replace('\n', ','), this.viewModel);
                        logDelay = Task.Delay(2000 + r.Next(-200, 200));
                    }
                }
                catch (TimeoutException)
                {
                    this.log.Error("Tick update timed out.");
                    tickError = true;
                }
                catch (IOException ex)
                {
                    this.log.Error("Tick update error.", ex);
                    tickError = true;
                }

                if (tickError)
                {
                    driver.Disconnect();
                }
            }

            driver.Disconnect();
        }
    }
}

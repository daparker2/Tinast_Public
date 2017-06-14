namespace DP.Tinast
{
    using Microsoft.HockeyApp;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
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
        /// The debug task
        /// </summary>
        private Task debugTask;

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
            this.boostGauge.MaxLevel = ((App)Application.Current).Config.MaxBoost;
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
            if (this.tickTask != null)
            {
                await this.tickTask;
                this.tickTask = null;
            }

            if (this.debugTask != null)
            {
                await this.debugTask;
                this.debugTask = null;
            }
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
                this.debugTask = this.DebugLoopAsync(driver);
            }
        }

        /// <summary>
        /// The async tick loop.
        /// </summary>
        /// <returns></returns>
        private Task TickLoopAsync(IDisplayDriver driver)
        {
            return Task.Run(async () =>
            {
                while (this.resumed)
                {
                    bool tickError = false;
                    try
                    {
                        Task tick = this.viewModel.Tick();
                        if (tick.Wait(this.viewModel.GetTickDuration()))
                        {
                            await tick;
                        }
                        else
                        {
                            this.log.Error("Tick timed out.");
                            tickError = true;
                        }
                    }
                    catch (IOException ex)
                    {
                        this.log.Error("Tick update error.", ex);
                        tickError = true;
                    }

                    if (tickError)
                    {
                        Task<PidDebugData> transactionTask = driver.GetLastTransactionInfo();
                        if (transactionTask.IsCompleted)
                        {
                            PidDebugData data = await transactionTask;
                            this.log.Error("Tick data: {0}", data);
                        }

                        driver.Disconnect();
                    }
                }
            });
        }

        /// <summary>
        /// Periocally samples the driver for the last PID transaction, printing transactions if they have gotten a valid result already.
        /// </summary>
        /// <returns></returns>
        private Task DebugLoopAsync(IDisplayDriver driver)
        {
            return Task.Run(async () =>
            {
                while (this.resumed)
                {
                    Task<PidDebugData> transactionTask = driver.GetLastTransactionInfo();
                    if (transactionTask.Wait(this.viewModel.GetTickDuration()))
                    {
                        PidDebugData transactionResult = await transactionTask;
                        this.log.Trace("{0}; {1}", transactionResult.ToString().Replace('\n', ','), this.viewModel);
                        await Task.Delay(2000);
                    }
                }
            });
        }
    }
}

namespace DP.Tinast
{
    using Microsoft.HockeyApp;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
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
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application, IDisposable
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger log;

        /// <summary>
        /// The display configuration
        /// </summary>
        private DisplayConfiguration config;

        /// <summary>
        /// The ELM 327 driver
        /// </summary>
        private Elm327Driver driver;

        /// <summary>
        /// The update timer
        /// </summary>
        private DispatcherTimer updateTimer;

        /// <summary>
        /// The ticks
        /// </summary>
        private uint tick = 0;

        /// <summary>
        /// The disposed value
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Occurs when faulted.
        /// </summary>
        public event EventHandler Faulted;

        /// <summary>
        /// Occurs when the short tick for updating gauges occurs.
        /// </summary>
        public event EventHandler GaugeTick;

        /// <summary>
        /// Occurs when the long tick for blinking indicators occurs.
        /// </summary>
        public event EventHandler IndicatorTick;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            
            // setup the global crash handler...
            GlobalCrashHandler.Configure();

            this.log = LogManagerFactory.DefaultLogManager.GetLogger<App>();
            HockeyClient.Current.Configure("97e8a58ba9a74a2bb9a8b8d46a464b7b");
            this.log.Info("Starting application");
        }

        /// <summary>
        /// Gets the driver.
        /// </summary>
        /// <value>
        /// The driver.
        /// </value>
        public IDisplayDriver Driver
        {
            get
            {
                return this.driver;
            }
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public DisplayConfiguration Config
        {
            get
            {
                return this.config;
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
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            if (this.config == null)
            {
                this.config = await DisplayConfiguration.Load();
            }

            if (this.driver == null)
            {
                this.driver = new Elm327Driver(this.config);
            }

            if (this.updateTimer == null)
            {
                this.updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
                this.updateTimer.Tick += UpdateTimer_Tick;
                this.updateTimer.Start();
            }

            this.updateTimer.Start();

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                Window.Current.Activate();
            }
        }

        /// <summary>
        /// The main animation timer tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void UpdateTimer_Tick(object sender, object e)
        {
            EventArgs eventArgs = new EventArgs();
            if (tick++ % 16 == 0)
            {
                if (this.IndicatorTick != null)
                {
                    this.IndicatorTick(this, eventArgs);
                }
            }

            if (this.GaugeTick != null)
            {
                this.GaugeTick(this, eventArgs);
            }
        }

        /// <summary>
        /// The app specific unhandled exception handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            this.log.Error("Unhandled exception in app", e.Exception);
            if (this.Faulted != null)
            {
                this.Faulted(this, new EventArgs());
            }

            // So we can show the fault indicator
            e.Handled = !Debugger.IsAttached;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            this.log.Error("Failed to navigate to page " + e.SourcePageType.FullName);
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            try
            {
                this.updateTimer.Stop();

                if (this.driver != null)
                {
                    this.driver.Disconnect();
                }

                if (this.config != null)
                {
                    await this.config.Save();
                }
            }
            finally
            {
                deferral.Complete();
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                this.disposed = true;

                if (disposing)
                {
                    if (this.driver != null)
                    {
                        this.driver.Dispose();
                        this.driver = null;
                    }
                }
            }
        }
    }
}

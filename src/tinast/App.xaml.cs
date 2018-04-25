namespace DP.Tinast
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.Foundation;
    using Windows.Foundation.Collections;
    using Windows.System.Display;
    using Windows.UI.Popups;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Navigation;
    using Microsoft.HockeyApp;
    using Config;
    using Elm327;
    using Interfaces;
    using MetroLog;
    using MetroLog.Targets;
    using Pages;
    using ViewModel;

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger log;

        /// <summary>
        /// The update timer
        /// </summary>
        private DispatcherTimer updateTimer;

        /// <summary>
        /// The ticks
        /// </summary>
        private uint tick = 0;

        /// <summary>
        /// The display request
        /// </summary>
        private DisplayRequest displayRequest;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        /// <param name="pageType">The main page type.</param>
        public App()
        {
            this.Suspending += OnSuspending;
            HockeyClient.Current.Configure("97e8a58ba9a74a2bb9a8b8d46a464b7b");
            this.UnhandledException += UnhandledExceptionHandler;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

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
                if (this.log == null)
                {
                    // setup the global crash handler...
                    GlobalCrashHandler.Configure();

                    this.log = LogManagerFactory.DefaultLogManager.GetLogger<App>();
                }

                this.log.Info("Starting application");

                if (!Debugger.IsAttached)
                {
                    ApplicationView view = ApplicationView.GetForCurrentView();
                    if (view.TryEnterFullScreenMode())
                    {
                        ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
                    }
                }

                if (this.displayRequest == null)
                {
                    this.displayRequest = new DisplayRequest();
                }

                this.displayRequest.RequestActive();

                if (this.updateTimer == null)
                {
                    this.updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
                    this.updateTimer.Tick += UpdateTimer_Tick;
                    this.updateTimer.Start();
                }

                this.updateTimer.Start();

                if (rootFrame.Content == null)
                {
                    Type mainPageType = await GetMainpageType().ConfigureAwait(true);
                    this.log.Debug("Selected head unit type: {0}", mainPageType);
                    rootFrame.Navigate(mainPageType, e.Arguments);
                }

                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Gets the type of the mainpage.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Unsupported head unit.</exception>
        private static async Task<Type> GetMainpageType()
        {
            DisplayConfiguration displayConfig = await TinastGlobal.Current.GetConfigAsync()
                                                                           .ConfigureAwait(true);
            Type mainPageType = typeof(MainPage);
            if (displayConfig.HeadUnit == HeadUnitType.Head800x480)
            {
                mainPageType = typeof(MainPage800x480);
            }
            else if (displayConfig.HeadUnit != HeadUnitType.Default)
            {
                throw new InvalidOperationException("Unsupported head unit.");
            }

            return mainPageType;
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
                TinastGlobal.Current.OnIndicatorTick();
            }

            TinastGlobal.Current.OnGaugeTick();
        }

        /// <summary>
        /// The app specific unhandled exception handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private void UnhandledExceptionHandler(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            this.log.Fatal("Unhandled exception in app", e.Exception);
            TinastGlobal.Current.OnFaulted();

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
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            try
            {
                if (this.updateTimer != null)
                {
                    this.updateTimer.Stop();
                }

                TinastGlobal.Current.Suspend();

                if (this.displayRequest != null)
                {
                    this.displayRequest.RequestRelease();
                }
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}

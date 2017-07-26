namespace DP.Tinast.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices.WindowsRuntime;
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
    using MetroLog;
    using MetroLog.Targets;
    using Xunit.Runners.UI;

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : RunnerApplication
    {
        /// <summary>
        /// The log
        /// </summary>
        private ILogger log;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += UnhandledExceptionHandler;
            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Debug, LogLevel.Fatal, new StreamingFileTarget());
            this.log = LogManagerFactory.DefaultLogManager.GetLogger<App>();
        }

        /// <summary>
        /// Called when the test runner initializes.
        /// </summary>
        protected override void OnInitializeRunner()
        {
            this.log.Info("Inside App.OnInitializeRunner");

            // Make sure to declare dependent unit test classes here.
            this.AddTestAssembly(typeof(TestTests).GetTypeInfo().Assembly);
        }

        /// <summary>
        /// The app specific unhandled exception handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            this.log.Fatal("Unhandled exception in app", e.Exception);
        }
    }
}

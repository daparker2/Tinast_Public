namespace DP.Tinast.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.Foundation;
    using Windows.Foundation.Collections;
    using Windows.Networking;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;
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
    /// <seealso cref="Xunit.Runners.UI.RunnerApplication" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IXamlMetadataProvider" />
    sealed partial class App : RunnerApplication
    {
        /// <summary>
        /// The test results text file name.
        /// </summary>
        private const string TestResultsTxt = "TestResults.txt";

        /// <summary>
        /// The log
        /// </summary>
        private ILogger log;

        /// <summary>
        /// The result writer
        /// </summary>
        private TextWriter writer = new StringWriter();

        /// <summary>
        /// The listener
        /// </summary>
        private StreamSocketListener listener;

        /// <summary>
        /// The wait for run task
        /// </summary>
        private Task<string> waitForRunTask;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += UnhandledExceptionHandler;
            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Info, LogLevel.Fatal, new StreamingFileTarget());
            this.log = LogManagerFactory.DefaultLogManager.GetLogger<App>();
            this.AutoStart = true;
            this.Writer = writer;
            this.Suspending += App_Suspending;
        }

        /// <summary>
        /// Gets or sets the test result port.
        /// </summary>
        /// <value>
        /// The test result port.
        /// </value>
        public int TestResultPort { get; set; } = 8001;

        /// <summary>
        /// Gets or sets the test result timeout which should be at least longRunningTestSeconds.
        /// </summary>
        /// <value>
        /// The test result timeout.
        /// </value>
        public TimeSpan TestResultTimeout { get; set; } = TimeSpan.FromSeconds(2500);

        /// <summary>
        /// Called when the test runner initializes.
        /// </summary>
        protected override async void OnInitializeRunner()
        {
            this.log.Info("Inside App.OnInitializeRunner");

            // Make sure to declare dependent unit test classes here.
            this.AddTestAssembly(typeof(TestTests).GetTypeInfo().Assembly);

            // Set up a web service to post test results to when they're available.
            this.waitForRunTask = this.GetTestResultAsync();
            this.listener = new StreamSocketListener();
            this.listener.ConnectionReceived += Listener_ConnectionReceived;
            await this.listener.BindServiceNameAsync(this.TestResultPort.ToString());
            this.log.Debug("Listening on {0}", this.listener.Information.LocalPort);
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

        /// <summary>
        /// Gets the test result asynchronously.
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetTestResultAsync()
        {
            DateTime end = DateTime.Now + this.TestResultTimeout;
            for (;;)
            {
                if (DateTime.Now >= end)
                {
                    throw new TimeoutException("Test results not available within the configured timeout.");
                }

                string s = this.writer.ToString();
                await Task.Delay(1000);

                if (s.Contains("Tests run: "))
                {
                    this.log.Debug("Test run complete: {0}", s);
                    return s;
                }
            }
        }

        /// <summary>
        /// Handles the Suspending event of the App control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SuspendingEventArgs"/> instance containing the event data.</param>
        private void App_Suspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            try
            {
                if (this.listener != null)
                {
                    this.listener.Dispose();
                }
            }
            finally
            {
                deferral.Complete();
            }
        }

        /// <summary>
        /// Listens to the next request.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="StreamSocketListenerConnectionReceivedEventArgs"/> instance containing the event data.</param>
        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            using (StreamReader sr = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
            using (StreamWriter sw = new StreamWriter(args.Socket.OutputStream.AsStreamForWrite()))
            {
                string fileData;

                // Only one file we serve up, and that's the TestResults.txt file.
                try
                {
                    string fileName = sr.ReadLine();
                    if (fileName == TestResultsTxt)
                    {
                        fileData = await this.waitForRunTask;
                    }
                    else
                    {
                        fileData = "Not found.";
                    }
                }
                catch (TimeoutException)
                {
                    fileData = "Timed out.";
                }

                sw.WriteLine(fileData);
            }
        }
    }
}

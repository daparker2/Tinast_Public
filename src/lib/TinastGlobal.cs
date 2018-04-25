
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
    using Config;
    using Elm327;
    using Interfaces;
    using MetroLog;
    using MetroLog.Targets;
    using Pages;
    using ViewModel;

    /// <summary>
    /// The tinast global class.
    /// </summary>
    public class TinastGlobal : ITinastGlobal, IDisposable
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<TinastGlobal>();

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
        /// The display configuration
        /// </summary>
        private DisplayConfiguration config;

        /// <summary>
        /// The ELM 327 driver
        /// </summary>
        private Elm327Driver driver;

        /// <summary>
        /// The connection
        /// </summary>
        private BluetoothElm327Connection connection;
        
        /// <summary>
        /// The disposed.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Gets the global instance.
        /// </summary>
        public static ITinastGlobal Current { get; } = new TinastGlobal();

        /// <summary>
        /// Gets the driver asynchronously.
        /// </summary>
        /// <returns>A <see cref="IDisplayDriver"/> object.</returns>
        public async Task<IDisplayDriver> GetDriverAsync()
        {
            if (this.connection == null)
            {
                this.connection = (await BluetoothElm327Connection.GetAvailableConnectionsAsync().ConfigureAwait(true))
                                                                  .FirstOrDefault();
                if (this.connection == null)
                {
                    this.log.Error("App launch failed. Couldn't access the OBD2 scantool.");
#if !DEBUG
                    MessageDialog dialog = new MessageDialog("App launch failed for the following reason. You must pair the system with the OBD2 scantool before launching the app again. Press OK to quit the app.");
                    dialog.Commands.Add(new UICommand("OK") { Id = 0 });
                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 0;
                    IUICommand result = await dialog.ShowAsync();
                    Application.Current.Exit();
#endif
                    return null;
                }
            }

            if (this.driver == null)
            {
                this.driver = new Elm327Driver(await this.GetConfigAsync().ConfigureAwait(false), this.connection);
            }

            return this.driver;
        }

        /// <summary>
        /// Gets the configuration asynchronously.
        /// </summary>
        /// <returns>An <see cref="DisplayConfiguration"/> object.</returns>
        public async Task<DisplayConfiguration> GetConfigAsync()
        {
            if (this.config == null)
            {
                this.config = await DisplayConfiguration.Load().ConfigureAwait(false);
            }

            return this.config;
        }

        /// <summary>
        /// Invoked when the application is suspending.
        /// </summary>
        public void Suspend()
        {
            this.connection?.Dispose();
            this.connection = null;
            this.driver = null;
            this.config = null;
        }

        /// <summary>
        /// Occurs on an indicator tick.
        /// </summary>
        public void OnIndicatorTick()
        {
            this.IndicatorTick?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Occurs on a gauge tick.
        /// </summary>
        public void OnGaugeTick()
        {
            this.GaugeTick?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Occurs when faulted.
        /// </summary>
        public void OnFaulted()
        {
            this.Faulted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>s
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                this.disposed = true;

                if (disposing)
                {
                    this.connection?.Dispose();
                    this.connection = null;
                    this.driver = null;
                }
            }
        }
    }
}

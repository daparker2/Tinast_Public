
namespace DP.Tinast.ViewModel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Windows.ApplicationModel.Core;
    using Windows.UI.Core;
    using Config;
    using Interfaces;
    using MetroLog;

    /// <summary>
    /// Represent a view model for the display.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class DisplayViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<DisplayViewModel>();

        /// <summary>
        /// The ELM 227 driver.
        /// </summary>
        private IDisplayDriver driver;

        /// <summary>
        /// The display configuration
        /// </summary>
        private DisplayConfiguration config;

        /// <summary>
        /// The tick counter
        /// </summary>
        private ulong ticks = 0;

        /// <summary>
        /// The properties changed
        /// </summary>
        private ConcurrentBag<string> propertiesChanged = new ConcurrentBag<string>();

        /// <summary>
        /// The property task
        /// </summary>
        private Task propertyTask;

        /// <summary>
        /// The flashed gauges
        /// </summary>
        private bool flashedGauges = false;

        /// <summary>
        /// The property changed event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayViewModel"/> class.
        /// </summary>
        /// <param name="driver">The driver.</param>
        /// <param name="config">The configuration.</param>
        public DisplayViewModel(IDisplayDriver driver, DisplayConfiguration config)
        {
            this.driver = driver;
            this.config = config;
        }

        /// <summary>
        /// Gets the ticks.
        /// </summary>
        /// <value>
        /// The ticks.
        /// </value>
        public ulong Ticks
        {
            get
            {
                return this.ticks;
            }
        }

        /// <summary>
        /// Gets or sets the engine load.
        /// </summary>
        /// <value>
        /// The engine load.
        /// </value>
        public int EngineLoad { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the engine is in an idle load state.
        /// </summary>
        /// <value>
        ///   <c>true</c> if engine is in an idle load state; otherwise, <c>false</c>.
        /// </value>
        public bool IdleLoad { get; set; }

        /// <summary>
        /// Gets or sets the engine coolant temp.
        /// </summary>
        /// <value>
        /// The engine coolant temp.
        /// </value>
        public int EngineCoolantTemp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the coolant temperature warning is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if coolant temperature warning; otherwise, <c>false</c>.
        /// </value>
        public bool CoolantTempWarn { get; set; }

        /// <summary>
        /// Gets or sets the engine oil temp.
        /// </summary>
        /// <value>
        /// The engine oil temp.
        /// </value>
        public int EngineOilTemp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the oil temperature warning is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if oil temperature warning; otherwise, <c>false</c>.
        /// </value>
        public bool OilTempWarn { get; set; }

        /// <summary>
        /// Gets or sets the engine intake temp.
        /// </summary>
        /// <value>
        /// The engine intake temp.
        /// </value>
        public int EngineIntakeTemp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the intake temperature warning is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if intake temperature warning; otherwise, <c>false</c>.
        /// </value>
        public bool IntakeTempWarn { get; set; }

        /// <summary>
        /// Gets or sets the engine boost.
        /// </summary>
        /// <value>
        /// The engine boost.
        /// </value>
        public double EngineBoost { get; set; }

        /// <summary>
        /// Gets or sets the engine AFR.
        /// </summary>
        /// <value>
        /// The engine AFR.
        /// </value>
        public double EngineAfr { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the AFR warning is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the AFR warning is active; otherwise, <c>false</c>.
        /// </value>
        public bool AfrTooLean { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the AFR warning is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the AFR warning is active; otherwise, <c>false</c>.
        /// </value>
        public bool AfrTooRich { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a temperature value is out of range.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a temperature value is out of range; otherwise, <c>false</c>.
        /// </value>
        public bool TempWarning { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DisplayViewModel"/> is faulted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if faulted; otherwise, <c>false</c>.
        /// </value>
        public bool Faulted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the OBD2 driver is connecting.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the OBD2 driver is connecting; otherwise, <c>false</c>.
        /// </value>
        public bool Obd2Connecting { get; set; } = true;

        /// <summary>
        /// Updates the view-model asynchronously until the token is canceled.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public async Task UpdateViewModelAsync(CancellationToken token)
        {
            for (;;)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    await this.OpenConnectionAsync().ConfigureAwait(false);
                    if (this.driver != null)
                    {
                        for (; ; )
                        {
                            token.ThrowIfCancellationRequested();

                            // So basically, we want to share the bus time between boost+AFR (which always get updated every tick)
                            // and every other tick update one of the other 4 things: oil temp, coolant temp, intake temp, engine load
                            // This is to try and keep the frame-rate for boost and AFR as high as possible.

                            PidRequests request = PidRequests.Boost | PidRequests.Afr;
                            switch (this.ticks++ % 20)
                            {
                                case 4:
                                    request |= PidRequests.CoolantTemp;
                                    break;

                                case 8:
                                    request |= PidRequests.IntakeTemp;
                                    break;

                                case 12:
                                    request |= PidRequests.Load;
                                    break;

                                case 19:
                                    request |= PidRequests.OilTemp;
                                    break;

                                default:
                                    break;
                            }

                            PidResult result = await this.driver.GetPidResultAsync(request).TimeoutAfter(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

                            this.EngineBoost = this.SetProperty(nameof(this.EngineBoost), this.EngineBoost, result.Boost, out bool propertyChanged);
                            this.EngineAfr = this.SetProperty(nameof(this.EngineAfr), this.EngineAfr, Math.Round(result.Afr, 2), out propertyChanged);
                            if (propertyChanged)
                            {
                                this.AfrTooLean = this.SetProperty(nameof(this.AfrTooLean), this.AfrTooLean, this.EngineAfr > 18, out propertyChanged);
                                this.AfrTooRich = this.SetProperty(nameof(this.AfrTooRich), this.AfrTooRich, this.EngineAfr < 11, out propertyChanged);
                            }

                            if (request.HasFlag(PidRequests.OilTemp))
                            {
                                this.EngineOilTemp = this.SetProperty(nameof(this.EngineOilTemp), this.EngineOilTemp, result.OilTemp, out propertyChanged);
                                if (propertyChanged)
                                {
                                    this.OilTempWarn = this.SetProperty(nameof(this.OilTempWarn), this.OilTempWarn, !(this.EngineOilTemp >= this.config.OilTempMin && this.EngineOilTemp <= this.config.OilTempMax), out propertyChanged);
                                }
                            }
                            else if (request.HasFlag(PidRequests.CoolantTemp))
                            {
                                this.EngineCoolantTemp = this.SetProperty(nameof(this.EngineCoolantTemp), this.EngineCoolantTemp, result.CoolantTemp, out propertyChanged);
                                if (propertyChanged)
                                {
                                    this.CoolantTempWarn = this.SetProperty(nameof(this.CoolantTempWarn), this.CoolantTempWarn, !(this.EngineCoolantTemp >= this.config.CoolantTempMin && this.EngineCoolantTemp <= this.config.CoolantTempMax), out propertyChanged);
                                }
                            }
                            else if (request.HasFlag(PidRequests.IntakeTemp))
                            {
                                this.EngineIntakeTemp = this.SetProperty(nameof(this.EngineIntakeTemp), this.EngineIntakeTemp, result.IntakeTemp, out propertyChanged);
                                if (propertyChanged)
                                {
                                    this.IntakeTempWarn = this.SetProperty(nameof(this.IntakeTempWarn), this.IntakeTempWarn, !(this.EngineIntakeTemp >= this.config.IntakeTempMin && this.EngineIntakeTemp <= this.config.IntakeTempMax), out propertyChanged);
                                }
                            }
                            else if (request.HasFlag(PidRequests.Load))
                            {
                                this.EngineLoad = this.SetProperty(nameof(this.EngineLoad), this.EngineLoad, result.Load, out propertyChanged);
                                if (propertyChanged)
                                {
                                    this.IdleLoad = this.SetProperty(nameof(this.IdleLoad), this.IdleLoad, this.EngineLoad < this.config.MaxIdleLoad, out propertyChanged);
                                }
                            }

                            this.TempWarning = this.SetProperty(nameof(this.TempWarning), this.TempWarning, this.IntakeTempWarn || this.OilTempWarn || this.CoolantTempWarn, out propertyChanged);
                            await this.OnPropertiesChanged().ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    this.log.Error("ViewModel update failed", ex);
                    PidDebugData transactionResult = this.driver.GetLastTransactionInfo();
                    this.log.Debug("Last transaction: {0}", transactionResult.ToString().Replace('\n', ','));
                }
            }
        }

        /// <summary>
        /// Faults this instance.
        /// </summary>
        public void Fault()
        {
            if (!this.Faulted)
            {
                this.Faulted = true;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Faulted)));
            }
        }


        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Boost: {0}, Afr: {1}, AfrTooLean: {2}, AfrTooRich: {3}, Oil Temp: {4}, OilTempWarn: {5}, Coolant Temp: {6}, CoolantTempWarn: {7}, Intake Temp: {8}, IntakeTempWarn: {9}, Load: {10}, IdleLoad: {11}",
                this.EngineBoost, this.EngineAfr, this.AfrTooLean, this.AfrTooRich, this.EngineOilTemp, this.OilTempWarn, this.EngineCoolantTemp, this.CoolantTempWarn, this.EngineIntakeTemp, this.IntakeTempWarn, this.EngineLoad, this.IdleLoad);
        }

        /// <summary>
        /// Called when a set of properties change on the view model.
        /// </summary>
        /// <returns>A task object.</returns>
        protected virtual async Task OnPropertiesChanged()
        {
            HashSet<string> props = new HashSet<string>();
            while (this.propertiesChanged.TryTake(out string prop))
            {
                props.Add(prop);
            }

            if (this.propertyTask != null)
            {
                await this.propertyTask.ConfigureAwait(false);
            }

            if (props.Count > 0)
            {
                this.propertyTask = CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (this.PropertyChanged != null)
                    {
                        foreach (string propertyName in props)
                        {
                            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                        }
                    }
                }).AsTask();
            }
        }

        /// <summary>
        /// Flashes the gauges for the user so they know the gauges work.
        /// </summary>
        /// <returns></returns>
        private async Task FlashGauges()
        {
            Task tempFlash = this.FlashTempGauges();
            Task boostFlash = this.FlashBoostGauge();
            Task afrFlash = this.FlashAfrGauge();
            await Task.WhenAll(tempFlash, boostFlash, afrFlash).ConfigureAwait(false);
        }

        /// <summary>
        /// Flashes the temperature gauges for the user so they know the gauges work.
        /// </summary>
        /// <returns></returns>
        private async Task FlashTempGauges()
        {
            this.IntakeTempWarn = this.SetProperty(nameof(this.IntakeTempWarn), this.IntakeTempWarn, true, out bool propertyChanged);
            this.CoolantTempWarn = this.SetProperty(nameof(this.CoolantTempWarn), this.CoolantTempWarn, true, out propertyChanged);
            this.OilTempWarn = this.SetProperty(nameof(this.OilTempWarn), this.OilTempWarn, true, out propertyChanged);
            await this.OnPropertiesChanged().ConfigureAwait(false);

            for (int i = 150; i >= 0; i -= 50)
            {
                if (i == 100)
                {
                    this.IntakeTempWarn = this.SetProperty(nameof(this.IntakeTempWarn), this.IntakeTempWarn, false, out propertyChanged);
                    this.CoolantTempWarn = this.SetProperty(nameof(this.CoolantTempWarn), this.CoolantTempWarn, false, out propertyChanged);
                    this.OilTempWarn = this.SetProperty(nameof(this.OilTempWarn), this.OilTempWarn, false, out propertyChanged);
                    await this.OnPropertiesChanged().ConfigureAwait(false);
                }

                this.EngineIntakeTemp = this.SetProperty(nameof(this.EngineIntakeTemp), this.EngineIntakeTemp, i, out propertyChanged);
                this.EngineCoolantTemp = this.SetProperty(nameof(this.EngineCoolantTemp), this.EngineCoolantTemp, i, out propertyChanged);
                this.EngineOilTemp = this.SetProperty(nameof(this.EngineOilTemp), this.EngineOilTemp, i, out propertyChanged);
                await Task.WhenAll(Task.Delay(800), this.OnPropertiesChanged()).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Flashes the boost gauge for the user so they know the gauge works.
        /// </summary>
        /// <returns></returns>
        private async Task FlashBoostGauge()
        {
            bool propertyChanged;

            for (int i = (int)this.config.BoostOffset; i <= this.config.MaxBoost; ++i)
            {
                this.EngineBoost = this.SetProperty(nameof(this.EngineBoost), this.EngineBoost, i, out propertyChanged);
                await Task.WhenAll(Task.Delay(33), this.OnPropertiesChanged()).ConfigureAwait(false);
            }

            this.EngineBoost = this.SetProperty(nameof(this.EngineBoost), this.EngineBoost, this.config.MaxBoost, out propertyChanged);
            await Task.WhenAll(Task.Delay(300), this.OnPropertiesChanged()).ConfigureAwait(false);

            for (int i = (int)this.config.MaxBoost; i >= (int)this.config.BoostOffset; --i)
            {
                this.EngineBoost = this.SetProperty(nameof(this.EngineBoost), this.EngineBoost, i, out propertyChanged);
                await Task.WhenAll(Task.Delay(33), this.OnPropertiesChanged()).ConfigureAwait(false);
            }

            this.EngineBoost = this.SetProperty(nameof(this.EngineBoost), this.EngineBoost, this.config.BoostOffset, out propertyChanged);
            await this.OnPropertiesChanged().ConfigureAwait(false);
        }

        /// <summary>
        /// Flashes the afr gauge for the user so they know the gauge works.
        /// </summary>
        /// <returns></returns>
        private async Task FlashAfrGauge()
        {
            this.IdleLoad = this.SetProperty(nameof(this.IdleLoad), this.IdleLoad, false, out bool propertyChanged);
            this.AfrTooLean = this.SetProperty(nameof(this.AfrTooLean), this.AfrTooLean, false, out propertyChanged);
            await this.OnPropertiesChanged().ConfigureAwait(false);

            for (double i = 14.5; i <= 18; i += 0.5)
            {
                this.EngineAfr = this.SetProperty(nameof(this.EngineAfr), this.EngineAfr, i, out propertyChanged);
                if (i == 18)
                {
                    this.AfrTooLean = this.SetProperty(nameof(this.AfrTooLean), this.AfrTooLean, true, out propertyChanged);
                    await Task.WhenAll(Task.Delay(500), this.OnPropertiesChanged()).ConfigureAwait(false);
                }
                else
                {
                    await Task.WhenAll(Task.Delay(33), this.OnPropertiesChanged()).ConfigureAwait(false);
                }
            }

            this.AfrTooLean = this.SetProperty(nameof(this.AfrTooLean), this.AfrTooLean, false, out propertyChanged);
            this.AfrTooRich = this.SetProperty(nameof(this.AfrTooRich), this.AfrTooRich, false, out propertyChanged);
            await this.OnPropertiesChanged().ConfigureAwait(false);

            for (double i = 14.5; i >= 11; i -= 0.5)
            {
                this.EngineAfr = this.SetProperty(nameof(this.EngineAfr), this.EngineAfr, i, out propertyChanged);
                if (i == 11)
                {
                    this.AfrTooRich = this.SetProperty(nameof(this.AfrTooRich), this.AfrTooRich, true, out propertyChanged);
                    await Task.WhenAll(Task.Delay(500), this.OnPropertiesChanged()).ConfigureAwait(false);
                }
                else
                {
                    await Task.WhenAll(Task.Delay(33), this.OnPropertiesChanged()).ConfigureAwait(false);
                }
            }

            this.IdleLoad = this.SetProperty(nameof(this.IdleLoad), this.IdleLoad, true, out propertyChanged);
            this.EngineAfr = this.SetProperty(nameof(this.EngineAfr), this.EngineAfr, 11, out propertyChanged);
            this.AfrTooRich = this.SetProperty(nameof(this.AfrTooRich), this.AfrTooRich, true, out propertyChanged);
            this.AfrTooLean = this.SetProperty(nameof(this.AfrTooLean), this.AfrTooLean, false, out propertyChanged);
            await Task.WhenAll(Task.Delay(500), this.OnPropertiesChanged()).ConfigureAwait(false);
            this.EngineAfr = this.SetProperty(nameof(this.EngineAfr), this.EngineAfr, 18, out propertyChanged);
            this.AfrTooLean = this.SetProperty(nameof(this.AfrTooLean), this.AfrTooLean, true, out propertyChanged);
            this.AfrTooRich = this.SetProperty(nameof(this.AfrTooRich), this.AfrTooRich, false, out propertyChanged);
            await Task.WhenAll(Task.Delay(500), this.OnPropertiesChanged()).ConfigureAwait(false);
            this.EngineAfr = this.SetProperty(nameof(this.EngineAfr), this.EngineAfr, 14.7, out propertyChanged);
            this.AfrTooLean = this.SetProperty(nameof(this.AfrTooLean), this.AfrTooLean, false, out propertyChanged);
            this.AfrTooRich = this.SetProperty(nameof(this.AfrTooRich), this.AfrTooRich, false, out propertyChanged);
            await this.OnPropertiesChanged().ConfigureAwait(false);
        }

        /// <summary>
        /// Get if we should do the rest of the tick.
        /// </summary>
        /// <returns></returns>
        private async Task OpenConnectionAsync()
        {

            this.Obd2Connecting = this.SetProperty(nameof(this.Obd2Connecting), this.Obd2Connecting, true, out bool propertyChanged);
            await this.OnPropertiesChanged().ConfigureAwait(false);

            if (!this.flashedGauges)
            {
                this.flashedGauges = true;
                await this.FlashGauges().ConfigureAwait(false);
            }

            if (this.driver != null)
            {
                await this.driver.OpenAsync().TimeoutAfter(TimeSpan.FromSeconds(60)).ConfigureAwait(false);
            }

            this.Obd2Connecting = this.SetProperty(nameof(this.Obd2Connecting), this.Obd2Connecting, false, out propertyChanged);
            await this.OnPropertiesChanged().ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns></returns>
        private T SetProperty<T>(string propertyName, T propertyValue, T newValue, out bool propertyChanged) where T : struct
        {
            propertyChanged = false;
            if (!propertyValue.Equals(newValue))
            {
                this.propertiesChanged.Add(propertyName);
                propertyChanged = true;
                return newValue;
            }

            return propertyValue;
        }
    }
}

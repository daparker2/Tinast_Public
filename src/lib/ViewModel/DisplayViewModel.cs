
namespace DP.Tinast.ViewModel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
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
        /// Batch up property notifications up to 33 ms before sending them out.
        /// </summary>
        static readonly TimeSpan TickWindow = TimeSpan.FromMilliseconds(33);

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
                    await this.OpenConnectionAsync();

                    for (;;)
                    {
                        token.ThrowIfCancellationRequested();

                        // So basically, we want to share the bus time between boost+AFR (which always get updated every tick)
                        // and every other tick update one of the other 4 things: oil temp, coolant temp, intake temp, engine load
                        // This is to try and keep the frame-rate for boost and AFR as high as possible.

                        PidRequest request = PidRequest.Boost | PidRequest.Afr;
                        switch (this.ticks++ % 20)
                        {
                            case 4:
                                request |= PidRequest.CoolantTemp;
                                break;

                            case 8:
                                request |= PidRequest.IntakeTemp;
                                break;

                            case 12:
                                request |= PidRequest.Load;
                                break;

                            case 19:
                                request |= PidRequest.OilTemp;
                                break;

                            default:
                                break;
                        }

                        PidResult result = await this.driver.GetPidResultAsync(request).TimeoutAfter(TimeSpan.FromSeconds(5));

                        this.EngineBoost = this.SetProperty("EngineBoost", this.EngineBoost, result.Boost, out bool propertyChanged);
                        this.EngineAfr = this.SetProperty("EngineAfr", this.EngineAfr, Math.Round(result.Afr, 2), out propertyChanged);
                        if (propertyChanged)
                        {
                            this.AfrTooLean = this.SetProperty("AfrTooLean", this.AfrTooLean, this.EngineAfr > 18, out propertyChanged);
                            this.AfrTooRich = this.SetProperty("AfrTooRich", this.AfrTooRich, this.EngineAfr < 11, out propertyChanged);
                        }

                        if (request.HasFlag(PidRequest.OilTemp))
                        {
                            this.EngineOilTemp = this.SetProperty("EngineOilTemp", this.EngineOilTemp, result.OilTemp, out propertyChanged);
                            if (propertyChanged)
                            {
                                this.OilTempWarn = this.SetProperty("OilTempWarn", this.OilTempWarn, !(this.EngineOilTemp >= this.config.OilTempMin && this.EngineOilTemp <= this.config.OilTempMax), out propertyChanged);
                            }
                        }
                        else if (request.HasFlag(PidRequest.CoolantTemp))
                        {
                            this.EngineCoolantTemp = this.SetProperty("EngineCoolantTemp", this.EngineCoolantTemp, result.CoolantTemp, out propertyChanged);
                            if (propertyChanged)
                            {
                                this.CoolantTempWarn = this.SetProperty("CoolantTempWarn", this.CoolantTempWarn, !(this.EngineCoolantTemp >= this.config.CoolantTempMin && this.EngineCoolantTemp <= this.config.CoolantTempMax), out propertyChanged);
                            }
                        }
                        else if (request.HasFlag(PidRequest.IntakeTemp))
                        {
                            this.EngineIntakeTemp = this.SetProperty("EngineIntakeTemp", this.EngineIntakeTemp, result.IntakeTemp, out propertyChanged);
                            if (propertyChanged)
                            {
                                this.IntakeTempWarn = this.SetProperty("IntakeTempWarn", this.IntakeTempWarn, !(this.EngineIntakeTemp >= this.config.IntakeTempMin && this.EngineIntakeTemp <= this.config.IntakeTempMax), out propertyChanged);
                            }
                        }
                        else if (request.HasFlag(PidRequest.Load))
                        {
                            this.EngineLoad = this.SetProperty("EngineLoad", this.EngineLoad, result.Load, out propertyChanged);
                            if (propertyChanged)
                            {
                                this.IdleLoad = this.SetProperty("IdleLoad", this.IdleLoad, this.EngineLoad < this.config.MaxIdleLoad, out propertyChanged);
                            }
                        }

                        this.TempWarning = this.SetProperty("TempWarning", this.TempWarning, this.IntakeTempWarn || this.OilTempWarn || this.CoolantTempWarn, out propertyChanged);
                        await this.OnPropertiesChanged();
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
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Faulted"));
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
            return string.Format("Boost: {0}, Afr: {1}, AfrTooLean: {2}, AfrTooRich: {3}, Oil Temp: {4}, OilTempWarn: {5}, Coolant Temp: {6}, CoolantTempWarn: {7}, Intake Temp: {8}, IntakeTempWarn: {9}, Load: {10}, IdleLoad: {11}",
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
                await this.propertyTask;
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
            await Task.WhenAll(tempFlash, boostFlash, afrFlash);
        }

        /// <summary>
        /// Flashes the temperature gauges for the user so they know the gauges work.
        /// </summary>
        /// <returns></returns>
        private async Task FlashTempGauges()
        {
            this.IntakeTempWarn = this.SetProperty("IntakeTempWarn", this.IntakeTempWarn, true, out bool propertyChanged);
            this.CoolantTempWarn = this.SetProperty("CoolantTempWarn", this.CoolantTempWarn, true, out propertyChanged);
            this.OilTempWarn = this.SetProperty("OilTempWarn", this.OilTempWarn, true, out propertyChanged);
            await this.OnPropertiesChanged();

            for (int i = 150; i >= 0; i -= 50)
            {
                if (i == 100)
                {
                    this.IntakeTempWarn = this.SetProperty("IntakeTempWarn", this.IntakeTempWarn, false, out propertyChanged);
                    this.CoolantTempWarn = this.SetProperty("CoolantTempWarn", this.CoolantTempWarn, false, out propertyChanged);
                    this.OilTempWarn = this.SetProperty("OilTempWarn", this.OilTempWarn, false, out propertyChanged);
                    await this.OnPropertiesChanged();
                }

                this.EngineIntakeTemp = this.SetProperty("EngineIntakeTemp", this.EngineIntakeTemp, i, out propertyChanged);
                this.EngineCoolantTemp = this.SetProperty("EngineCoolantTemp", this.EngineCoolantTemp, i, out propertyChanged);
                this.EngineOilTemp = this.SetProperty("EngineOilTemp", this.EngineOilTemp, i, out propertyChanged);
                await Task.WhenAll(Task.Delay(100), this.OnPropertiesChanged());
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
                this.EngineBoost = this.SetProperty("EngineBoost", this.EngineBoost, i, out propertyChanged);
                await Task.WhenAll(Task.Delay(33), this.OnPropertiesChanged());
            }

            this.EngineBoost = this.SetProperty("EngineBoost", this.EngineBoost, this.config.MaxBoost, out propertyChanged);
            await Task.WhenAll(Task.Delay(300), this.OnPropertiesChanged());

            for (int i = (int)this.config.MaxBoost; i >= (int)this.config.BoostOffset; --i)
            {
                this.EngineBoost = this.SetProperty("EngineBoost", this.EngineBoost, i, out propertyChanged);
                await Task.WhenAll(Task.Delay(33), this.OnPropertiesChanged());
            }

            this.EngineBoost = this.SetProperty("EngineBoost", this.EngineBoost, this.config.BoostOffset, out propertyChanged);
            await this.OnPropertiesChanged();
        }

        /// <summary>
        /// Flashes the afr gauge for the user so they know the gauge works.
        /// </summary>
        /// <returns></returns>
        private async Task FlashAfrGauge()
        {
            this.IdleLoad = this.SetProperty("IdleLoad", this.IdleLoad, false, out bool propertyChanged);
            this.AfrTooLean = this.SetProperty("AfrTooLean", this.AfrTooLean, false, out propertyChanged);
            await this.OnPropertiesChanged();

            for (double i = 14.5; i <= 18; i += 0.5)
            {
                this.EngineAfr = this.SetProperty("EngineAfr", this.EngineAfr, i, out propertyChanged);
                if (i == 18)
                {
                    this.AfrTooLean = this.SetProperty("AfrTooLean", this.AfrTooLean, true, out propertyChanged);
                    await Task.WhenAll(Task.Delay(500), this.OnPropertiesChanged());
                }
                else
                {
                    await Task.WhenAll(Task.Delay(33), this.OnPropertiesChanged());
                }
            }

            this.AfrTooLean = this.SetProperty("AfrTooLean", this.AfrTooLean, false, out propertyChanged);
            this.AfrTooRich = this.SetProperty("AfrTooRich", this.AfrTooRich, false, out propertyChanged);
            await this.OnPropertiesChanged();

            for (double i = 14.5; i >= 11; i -= 0.5)
            {
                this.EngineAfr = this.SetProperty("EngineAfr", this.EngineAfr, i, out propertyChanged);
                if (i == 11)
                {
                    this.AfrTooRich = this.SetProperty("AfrTooRich", this.AfrTooRich, true, out propertyChanged);
                    await Task.WhenAll(Task.Delay(500), this.OnPropertiesChanged());
                }
                else
                {
                    await Task.WhenAll(Task.Delay(33), this.OnPropertiesChanged());
                }
            }

            this.IdleLoad = this.SetProperty("IdleLoad", this.IdleLoad, true, out propertyChanged);
            this.EngineAfr = this.SetProperty("EngineAfr", this.EngineAfr, 11, out propertyChanged);
            this.AfrTooRich = this.SetProperty("AfrTooRich", this.AfrTooRich, true, out propertyChanged);
            this.AfrTooLean = this.SetProperty("AfrTooLean", this.AfrTooLean, false, out propertyChanged);
            await Task.WhenAll(Task.Delay(500), this.OnPropertiesChanged());
            this.EngineAfr = this.SetProperty("EngineAfr", this.EngineAfr, 18, out propertyChanged);
            this.AfrTooLean = this.SetProperty("AfrTooLean", this.AfrTooLean, true, out propertyChanged);
            this.AfrTooRich = this.SetProperty("AfrTooRich", this.AfrTooRich, false, out propertyChanged);
            await Task.WhenAll(Task.Delay(500), this.OnPropertiesChanged());
            this.EngineAfr = this.SetProperty("EngineAfr", this.EngineAfr, 14.7, out propertyChanged);
            this.AfrTooLean = this.SetProperty("AfrTooLean", this.AfrTooLean, false, out propertyChanged);
            this.AfrTooRich = this.SetProperty("AfrTooRich", this.AfrTooRich, false, out propertyChanged);
            await this.OnPropertiesChanged();
        }

        /// <summary>
        /// Get if we should do the rest of the tick.
        /// </summary>
        /// <returns></returns>
        private async Task OpenConnectionAsync()
        {

            this.Obd2Connecting = this.SetProperty("Obd2Connecting", this.Obd2Connecting, true, out bool propertyChanged);
            await this.OnPropertiesChanged();

            if (!this.flashedGauges)
            {
                this.flashedGauges = true;
                await this.FlashGauges();
            }

            await this.driver.OpenAsync().TimeoutAfter(TimeSpan.FromSeconds(60));

            this.Obd2Connecting = this.SetProperty("Obd2Connecting", this.Obd2Connecting, false, out propertyChanged);
            await this.OnPropertiesChanged();
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

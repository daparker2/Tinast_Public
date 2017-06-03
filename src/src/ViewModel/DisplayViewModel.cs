﻿
namespace DP.Tinast.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
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
    class DisplayViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The last update
        /// </summary>
        private Task lastUpdate = null;

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
        List<string> propertiesChanged = new List<string>();

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
        public int EngineBoost { get; set; }

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
        /// Ticks an update of the display view model.
        /// </summary>
        /// <returns>A task object.</returns>
        public async Task Tick()
        {
            if (await this.ShouldTick())
            {
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
                        request |= PidRequest.OilTemp;
                        break;

                    case 19:
                        request |= PidRequest.OilTemp;
                        break;

                    default:
                        break;
                }

                PidResult result = await this.driver.GetPidResult(request);
                if (this.lastUpdate != null)
                {
                    await this.lastUpdate;
                    this.lastUpdate = null;
                }

                bool propertyChanged;
                this.EngineBoost = this.SetProperty("EngineBoost", this.EngineBoost, result.Boost, out propertyChanged);
                this.EngineAfr = this.SetProperty("EngineAfr", this.EngineAfr, Math.Round(result.Afr, 2), out propertyChanged);
                if (propertyChanged)
                {
                    this.AfrTooLean = this.SetProperty("AfrTooLean", this.AfrTooLean, this.EngineAfr > 16, out propertyChanged);
                    if (!propertyChanged)
                    {
                        this.AfrTooRich = this.SetProperty("AfrTooRich", this.AfrTooRich, this.EngineAfr < 12, out propertyChanged);
                    }
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

                this.lastUpdate = this.OnPropertiesChanged();
            }
        }

        /// <summary>
        /// Get if we should do the rest of the tick.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ShouldTick()
        {
            if (!this.driver.Resumed || !this.driver.Connected)
            {
                Task<bool> connectTask = this.driver.TryConnect();
                if (this.lastUpdate != null)
                {
                    await this.lastUpdate;
                    this.lastUpdate = null;
                }

                bool propertyChanged;
                this.Obd2Connecting = this.SetProperty("Obd2Connecting", await connectTask, true, out propertyChanged);
                this.lastUpdate = this.OnPropertiesChanged();
                return this.Obd2Connecting;
            }

            return true;
        }

        /// <summary>
        /// Faults this instance.
        /// </summary>
        /// <returns>A task object.</returns>
        public async Task Fault()
        {
            if (!this.Faulted)
            {
                if (this.lastUpdate != null)
                {
                    await this.lastUpdate;
                    this.lastUpdate = null;
                }

                bool propertyChanged;
                this.Faulted = this.SetProperty("Faulted", this.Faulted, true, out propertyChanged);
                this.lastUpdate = this.OnPropertiesChanged();
                await this.lastUpdate;
            }
        }

        /// <summary>
        /// Called when a set of properties change on the view model.
        /// </summary>
        /// <returns>A task object.</returns>
        protected virtual Task OnPropertiesChanged()
        {
            return CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (this.PropertyChanged != null && this.propertiesChanged.Count > 0)
                {
                    foreach (string propertyName in this.propertiesChanged)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    }
                }

                this.propertiesChanged.Clear();
            }).AsTask();
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

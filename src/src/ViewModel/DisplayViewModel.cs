
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
            this.Obd2Connecting = true;
        }

        /// <summary>
        /// Gets or sets the engine load.
        /// </summary>
        /// <value>
        /// The engine load.
        /// </value>
        public double EngineLoad
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the engine is in an idle load state.
        /// </summary>
        /// <value>
        ///   <c>true</c> if engine is in an idle load state; otherwise, <c>false</c>.
        /// </value>
        public bool IdleLoad
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the engine coolant temp.
        /// </summary>
        /// <value>
        /// The engine coolant temp.
        /// </value>
        public double EngineCoolantTemp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the coolant temperature warning is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if coolant temperature warning; otherwise, <c>false</c>.
        /// </value>
        public bool CoolantTempWarn
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the engine oil temp.
        /// </summary>
        /// <value>
        /// The engine oil temp.
        /// </value>
        public double EngineOilTemp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the oil temperature warning is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if oil temperature warning; otherwise, <c>false</c>.
        /// </value>
        public bool OilTempWarn
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the engine intake temp.
        /// </summary>
        /// <value>
        /// The engine intake temp.
        /// </value>
        public double EngineIntakeTemp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the intake temperature warning is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if intake temperature warning; otherwise, <c>false</c>.
        /// </value>
        public bool IntakeTempWarn
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the engine boost.
        /// </summary>
        /// <value>
        /// The engine boost.
        /// </value>
        public double EngineBoost
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the engine AFR.
        /// </summary>
        /// <value>
        /// The engine AFR.
        /// </value>
        public double EngineAfr
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the AFR warning is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the AFR warning is active; otherwise, <c>false</c>.
        /// </value>
        public bool AfrTooLean
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the AFR warning is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the AFR warning is active; otherwise, <c>false</c>.
        /// </value>
        public bool AfrTooRich
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DisplayViewModel"/> is faulted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if faulted; otherwise, <c>false</c>.
        /// </value>
        public bool Faulted
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the OBD2 driver is connecting.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the OBD2 driver is connecting; otherwise, <c>false</c>.
        /// </value>
        public bool Obd2Connecting
        {
            get;
            set;
        }

        /// <summary>
        /// Ticks an update of the display view model.
        /// </summary>
        /// <returns>A task object.</returns>
        public async Task Tick()
        {
            List<string> propertiesChanged = new List<string>();
            if (!this.driver.Resumed || !this.driver.TryConnect(30000))
            {
                if (!this.Obd2Connecting)
                {
                    propertiesChanged.Add("Obd2Connecting");
                    this.Obd2Connecting = true;
                    this.log.Warn("OBD2 connecting.");
                }
            }
            else
            {
                if (this.ticks % 100 == 0)
                {
                    this.log.Info("State: AFR={0]; Boost={1}; Oil Temp: {3}; Coolant Temp: {4}; Intake Temp: {5}; Load: {6}",
                        this.EngineAfr, this.EngineBoost, this.EngineOilTemp, this.EngineCoolantTemp, this.EngineIntakeTemp, this.EngineLoad);
                }

                if (this.Obd2Connecting)
                {
                    propertiesChanged.Add("Obd2Connecting");
                    this.Obd2Connecting = false;
                    this.log.Info("OBD2 connected.");
                }

                if (this.ticks % 8 == 0)
                {
                    this.EngineOilTemp = await this.driver.GetOilTemp();
                    propertiesChanged.Add("EngineOilTemp");
                    if (this.EngineOilTemp >= this.config.OilTempMin && this.EngineOilTemp >= this.config.OilTempMax)
                    {
                        if (this.OilTempWarn)
                        {
                            this.OilTempWarn = false;
                            propertiesChanged.Add("OilTempWarn");
                            this.log.Info("Oil temperature normal.");
                        }
                    }
                    else
                    {
                        if (!this.OilTempWarn)
                        {
                            this.OilTempWarn = true;
                            propertiesChanged.Add("OilTempWarn");
                            this.log.Warn("Oil temperature warning.");
                        }
                    }

                    this.EngineCoolantTemp = await this.driver.GetCoolantTemp();
                    propertiesChanged.Add("EngineCoolantTemp");
                    if (this.EngineCoolantTemp >= this.config.CoolantTempMin && this.EngineCoolantTemp >= this.config.CoolantTempMax)
                    {
                        if (this.CoolantTempWarn)
                        {
                            this.CoolantTempWarn = false;
                            propertiesChanged.Add("CoolantTempWarn");
                            this.log.Debug("Coolant temperature normal.");
                        }
                    }
                    else
                    {
                        if (!this.CoolantTempWarn)
                        {
                            this.CoolantTempWarn = true;
                            propertiesChanged.Add("CoolantTempWarn");
                            this.log.Warn("Coolant temperature warning.");
                        }
                    }

                    this.EngineIntakeTemp = await this.driver.GetIntakeTemp();
                    propertiesChanged.Add("EngineIntakeTemp");
                    if (this.EngineIntakeTemp >= this.config.IntakeTempMin && this.EngineIntakeTemp >= this.config.IntakeTempMax)
                    {
                        if (this.IntakeTempWarn)
                        {
                            this.IntakeTempWarn = false;
                            propertiesChanged.Add("IntakeTempWarn");
                            this.log.Debug("Intake temperature normal.");
                        }
                    }
                    else
                    {
                        if (!this.IntakeTempWarn)
                        {
                            this.IntakeTempWarn = true;
                            propertiesChanged.Add("IntakeTempWarn");
                            this.log.Warn("Intake temperature warning.");
                        }
                    }
                }

                if (this.ticks % 2 == 0)
                {
                    this.EngineLoad = await this.driver.GetLoad();
                    propertiesChanged.Add("EngineLoad");

                    if (this.EngineLoad < this.config.MaxIdleLoad)
                    { 
                        if (!this.IdleLoad)
                        {
                            this.IdleLoad = true;
                            this.log.Info("Engine idle.");
                            propertiesChanged.Add("IdleLoad");
                        }
                    }
                    else
                    {
                        if (this.IdleLoad)
                        {
                            this.IdleLoad = false;
                            this.log.Info("Engine under load.");
                            propertiesChanged.Add("IdleLoad");
                        }
                    }
                }

                // Always get the boost and AFR.
                this.EngineBoost = await this.driver.GetBoost();
                propertiesChanged.Add("EngineBoost");

                this.EngineAfr = await this.driver.GetAfr();
                propertiesChanged.Add("EngineAfr");

                if (this.EngineAfr > 12 && this.EngineAfr < 16)
                {
                    if (this.AfrTooLean || this.AfrTooRich)
                    {
                        this.AfrTooRich = this.AfrTooLean = false;
                        propertiesChanged.Add("AfrTooLean");
                        propertiesChanged.Add("AfrTooRich");
                        this.log.Info("AFR normal.");
                    }
                }
                else if (!this.IdleLoad && this.EngineAfr <= 12)
                {
                    if (!this.AfrTooRich)
                    {
                        this.AfrTooRich = true;
                        propertiesChanged.Add("AfrTooRich");
                        this.log.Warn("AFR too rich");
                    }
                }
                else if (!this.IdleLoad && this.EngineAfr >= 16)
                {
                    if (!this.AfrTooLean)
                    {
                        this.AfrTooLean = true;
                        propertiesChanged.Add("AfrTooLean");
                        this.log.Warn("AFR too lean");
                    }
                }

                ++this.ticks;
            }

            await this.OnPropertyChanged(propertiesChanged.ToArray());
        }

        /// <summary>
        /// Faults this instance.
        /// </summary>
        /// <returns>A task object.</returns>
        public async Task Fault()
        {
            this.Faulted = true;
            await this.OnPropertyChanged(new string[] { "Faulted" });
        }

        /// <summary>
        /// Called when a set of properties change on the view model.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns>A task object.</returns>
        protected virtual async Task OnPropertyChanged(string[] properties)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (string propertyName in properties)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            });
        }
    }
}

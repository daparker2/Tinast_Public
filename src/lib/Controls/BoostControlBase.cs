namespace DP.Tinast.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using Windows.Foundation;
    using Windows.Foundation.Collections;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Shapes;
    using Config;

    /// <summary>
    /// The boost gauge goes from the bottom left of the display to the top right of the display. At 0 psi, the gauge is empty. 
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.UserControl" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector2" />
    public abstract class BoostControlBase : UserControl
    {
        /// <summary>
        /// The temp level property
        /// </summary>
        public static readonly DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(double), typeof(BoostControlBase), new PropertyMetadata(default(double), new PropertyChangedCallback(OnLevelPropertyChanged)));

        /// <summary>
        /// The absolute offset maximum boost
        /// </summary>
        private double absMaxBoost;

        /// <summary>
        /// The boost offset from atmospheric pressure
        /// </summary>
        private double boostOffset;

        /// <summary>
        /// The maximum boost
        /// </summary>
        private double maxBoost;

        /// <summary>
        /// All leds
        /// </summary>
        private Polygon[] allLeds;

        /// <summary>
        /// The ticks
        /// </summary>
        private ulong ticks = 0;

        /// <summary>
        /// The blink
        /// </summary>
        private bool blink = false;

        /// <summary>
        /// The is blinking
        /// </summary>
        private bool isBlinking = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoostControl"/> class.
        /// </summary>
        public BoostControlBase()
        {
            this.DataContext = this;
            this.Loaded += BoostControl_Loaded;
        }

        /// <summary>
        /// Gets or sets the boost level.
        /// </summary>
        /// <value>
        /// The boost level.
        /// </value>
        [DesignerCategory("BoostControl")]
        [Description("The boost level.")]
        public double Level
        {
            get
            {
                return (double)this.GetValue(LevelProperty);
            }
            set
            {
                this.SetValue(LevelProperty, value);
                this.Redraw();
            }
        }

        /// <summary>
        /// Gets all the LEDs on the control in sequence.
        /// </summary>
        /// <returns>All the LEDs on the control in sequence.</returns>
        protected abstract Polygon[] GetAllLeds();

        /// <summary>
        /// Redraw the gauge.
        /// </summary>
        private void Redraw()
        {
            double boost = this.Level;
            double absBoost = boost - this.boostOffset;

            if (absBoost < 0)
            {
                absBoost = 0;
                this.blink = false;
            }
            else if (absBoost >= this.absMaxBoost)
            {
                absBoost = this.absMaxBoost;
                this.blink = true;
            }
            else
            {
                this.blink = false;
            }

            if (this.blink)
            {
                if (!this.isBlinking)
                {
                    this.isBlinking = true;
                    ((TinastApp)Application.Current).GaugeTick += UpdateTimer_Tick;
                }
            }
            else
            {
                if (this.isBlinking)
                {
                    this.isBlinking = false;
                    ((TinastApp)Application.Current).GaugeTick -= UpdateTimer_Tick;
                }
            }

            ++this.ticks;

            // Average to next value, minimum 1, until we hit it.
            int boostEnd = (int)(absBoost * this.allLeds.Length / this.absMaxBoost);
            for (int i = 0; i < this.allLeds.Length; ++i)
            {
                if (i < boostEnd)
                {
                    if (this.blink && (this.ticks % 2) == 0)
                    {
                        this.allLeds[i].Fill = ColorPalette.NeedleColor;
                    }
                    else
                    {
                        this.allLeds[i].Fill = ColorPalette.GaugeColor;
                    }

                    this.allLeds[i].Stroke = ColorPalette.OutlineColor;
                }
                else if (i == boostEnd)
                {
                    this.allLeds[i].Stroke = ColorPalette.OutlineColor;
                    this.allLeds[i].Fill = ColorPalette.NeedleColor;
                }
                else
                {
                    this.allLeds[i].Stroke = ColorPalette.InactiveColor;
                    this.allLeds[i].Fill = ColorPalette.InactiveColor;
                }
            }
        }

        /// <summary>
        /// Handles the Loaded event of the BoostControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void BoostControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.allLeds = this.GetAllLeds();

            foreach (Polygon led in this.allLeds)
            {
                led.Fill = ColorPalette.GaugeColor;
                led.Stroke = ColorPalette.OutlineColor;
            }

            DisplayConfiguration config = await ((TinastApp)Application.Current).GetConfigAsync();
            this.boostOffset = config.BoostOffset;
            this.maxBoost = config.MaxBoost;
            this.absMaxBoost = this.maxBoost - this.boostOffset;
        }

        /// <summary>
        /// Boost gauge update tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            this.Redraw();
        }

        /// <summary>
        /// Called when a boost level property changes..
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="dp">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnLevelPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs dp)
        {
            BoostControlBase boostControl = (BoostControlBase)obj;
            boostControl.Redraw();
        }
    }
}

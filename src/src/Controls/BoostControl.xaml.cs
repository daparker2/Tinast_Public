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

    /// <summary>
    /// The boost gauge goes from the bottom left of the display to the top right of the display. At 0 psi, the gauge is empty. 
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.UserControl" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector2" />
    public sealed partial class BoostControl : UserControl
    {
        /// <summary>
        /// The temp level property
        /// </summary>
        public static readonly DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(double), typeof(BoostControl), new PropertyMetadata(default(double)));

        /// <summary>
        /// The absolute offset maximum boost
        /// </summary>
        private readonly double absMaxBoost;

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
        /// Initializes a new instance of the <see cref="BoostControl"/> class.
        /// </summary>
        public BoostControl()
        {
            this.InitializeComponent();
            this.DataContext = this;
            ((App)Application.Current).GaugeTick += UpdateTimer_Tick;
            this.boostOffset = ((App)Application.Current).Config.BoostOffset;
            this.maxBoost = ((App)Application.Current).Config.MaxBoost;
            this.absMaxBoost = this.maxBoost - this.boostOffset;
            this.label.Foreground = ColorPalette.IndicatorColor;
            this.outline1.Stroke = ColorPalette.OutlineColor;
            this.outline2.Stroke = ColorPalette.OutlineColor;
            this.allLeds = new Polygon[]
            {
                this.led0,
                this.led1,
                this.led2,
                this.led3,
                this.led4,
                this.led5,
                this.led6,
                this.led7,
                this.led8,
                this.led9,
                this.led10,
                this.led11,
                this.led12,
                this.led13,
                this.led14,
                this.led15,
                this.led16,
                this.led17,
                this.led18,
                this.led19,
                this.led20,
                this.led21,
                this.led22,
                this.led23,
                this.led24,
                this.led25,
                this.led26,
                this.led27,
                this.led28,
                this.led29,
                this.led30,
                this.led31,
                this.led32,
                this.led33,
                this.led34,
                this.led35,
                this.led36,
                this.led37,
                this.led38,
                this.led39,
                this.led40,
                this.led41,
                this.led42,
                this.led43,
                this.led44,
                this.led45,
                this.led46,
                this.led47,
                this.led48,
                this.led49
            };

            foreach (Polygon led in this.allLeds)
            {
                led.Fill = ColorPalette.GaugeColor;
                led.Stroke = ColorPalette.OutlineColor;
            }
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
            }
        }

        /// <summary>
        /// Boost gauge update tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void UpdateTimer_Tick(object sender, EventArgs e)
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
    }
}

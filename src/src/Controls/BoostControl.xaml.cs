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
        public static readonly DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(int), typeof(BoostControl), new PropertyMetadata(default(int)));

        /// <summary>
        /// The temp level property
        /// </summary>
        public static readonly DependencyProperty MinLevelProperty = DependencyProperty.Register("MinLevel", typeof(int), typeof(BoostControl), new PropertyMetadata(default(int)));

        /// <summary>
        /// The temp level property
        /// </summary>
        public static readonly DependencyProperty MaxLevelProperty = DependencyProperty.Register("MaxLevel", typeof(int), typeof(BoostControl), new PropertyMetadata(default(int)));

        /// <summary>
        /// The last boost
        /// </summary>
        private int lastBoost = 0;

        /// <summary>
        /// All leds
        /// </summary>
        Polygon[] allLeds;

        /// <summary>
        /// The update timer
        /// </summary>
        private DispatcherTimer updateTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoostControl"/> class.
        /// </summary>
        public BoostControl()
        {
            this.InitializeComponent();
            this.Loaded += BoostControl_Loaded;
            this.Unloaded += BoostControl_Unloaded;
            this.DataContext = this;
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
                this.led49,
                this.led50,
                this.led51
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
        public int Level
        {
            get
            {
                return (int)this.GetValue(LevelProperty);
            }
            set
            {
                this.SetValue(LevelProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the minimum boost level.
        /// </summary>
        /// <value>
        /// The minimum boost level.
        /// </value>
        [DesignerCategory("BoostControl")]
        [Description("The minimum boost level.")]
        public int MinLevel
        {
            get
            {
                return (int)this.GetValue(MinLevelProperty);
            }
            set
            {
                this.SetValue(MinLevelProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum boost level.
        /// </summary>
        /// <value>
        /// The maximum boost level.
        /// </value>
        [DesignerCategory("BoostControl")]
        [Description("The maximum boost level.")]
        public int MaxLevel
        {
            get
            {
                return (int)this.GetValue(MaxLevelProperty);
            }
            set
            {
                this.SetValue(MaxLevelProperty, value);
            }
        }

        /// <summary>
        /// Handles the Loaded event of the BoostControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BoostControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
            this.updateTimer.Tick += UpdateTimer_Tick;
            this.updateTimer.Start();
        }

        /// <summary>
        /// Handles the Unloaded event of the BoostControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Windows.UI.Xaml.RoutedEventArgs" /> instance containing the event data.</param>
        private void BoostControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.updateTimer.Stop();
        }

        /// <summary>
        /// Boost gauge update tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void UpdateTimer_Tick(object sender, object e)
        {
            int boost = this.Level;
            if (boost < this.MinLevel)
            {
                boost = this.MinLevel;
            }
            else if (boost > this.MaxLevel)
            {
                boost = this.MaxLevel;
            }

            // Average to next value, minimum 1, until we hit it.
            int next = boost + (boost - this.lastBoost) / 2;
            if (next == 0)
            {
                if (boost > this.lastBoost)
                {
                    next = 1;
                }
                else if (boost < this.lastBoost)
                {
                    next = -1;
                }
            }

            int boostStep = this.allLeds.Length / (this.MaxLevel - this.MinLevel);
            int curBoost = this.MinLevel;
            for (int i = 0; i < this.allLeds.Length; ++i, curBoost += boostStep)
            {
                if (this.lastBoost < boost)
                {
                    if (curBoost <= this.lastBoost)
                    {
                        this.allLeds[i].Visibility = Visibility.Visible;
                        this.allLeds[i].Fill = ColorPalette.NeedleColor;
                    }
                    else if (curBoost <= next)
                    {
                        this.allLeds[i].Visibility = Visibility.Visible;
                        this.allLeds[i].Fill = ColorPalette.GaugeColor;
                    }
                    else
                    {
                        this.allLeds[i].Visibility = Visibility.Collapsed;
                    }
                }
                else if (this.lastBoost > boost)
                {
                    if (curBoost <= next)
                    {
                        this.allLeds[i].Visibility = Visibility.Visible;
                        this.allLeds[i].Fill = ColorPalette.GaugeColor;
                    }
                    else if (curBoost <= this.lastBoost)
                    {
                        this.allLeds[i].Visibility = Visibility.Visible;
                        this.allLeds[i].Fill = ColorPalette.NeedleColor;
                    }
                    else
                    {
                        this.allLeds[i].Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    if (curBoost <= boost)
                    {
                        this.allLeds[i].Visibility = Visibility.Visible;
                        this.allLeds[i].Fill = ColorPalette.GaugeColor;
                    }
                    else
                    {
                        this.allLeds[i].Visibility = Visibility.Collapsed;
                    }
                }
            }

            if (boost != this.lastBoost)
            {
                this.lastBoost = next;
            }
        }
    }
}

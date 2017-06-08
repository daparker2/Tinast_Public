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
    using Windows.UI.Xaml.Shapes;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Navigation;

    /// <summary>
    /// The AFR gauge is a radial gauge that occupies the center of the display. 
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.UserControl" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector2" />
    public sealed partial class AfrControl : UserControl
    {
        /// <summary>
        /// The temp level property
        /// </summary>
        public static readonly DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(double), typeof(AfrControl), new PropertyMetadata(default(double)));

        /// <summary>
        /// The too lean property
        /// </summary>
        public static readonly DependencyProperty TooLeanProperty = DependencyProperty.Register("TooLean", typeof(bool), typeof(AfrControl), new PropertyMetadata(default(bool)));

        /// <summary>
        /// The too rich property
        /// </summary>
        public static readonly DependencyProperty TooRichProperty = DependencyProperty.Register("TooRich", typeof(bool), typeof(AfrControl), new PropertyMetadata(default(bool)));

        /// <summary>
        /// The idle property
        /// </summary>
        public static readonly DependencyProperty IdleProperty = DependencyProperty.Register("Idle", typeof(bool), typeof(AfrControl), new PropertyMetadata(default(bool)));

        /// <summary>
        /// All leds
        /// </summary>
        Ellipse[] allLeds;

        /// <summary>
        /// The warning flash
        /// </summary>
        const int WarningFlash = 200;

        /// <summary>
        /// Control some animation properties of the gauge
        /// </summary>
        private bool warning;

        /// <summary>
        /// The state changed
        /// </summary>
        private bool stateChanged;

        /// <summary>
        /// The when warning
        /// </summary>
        private DateTime whenChanged = DateTime.Now;

        /// <summary>
        /// The update timer
        /// </summary>
        private DispatcherTimer updateTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AfrControl"/> class.
        /// </summary>
        public AfrControl()
        {
            this.InitializeComponent();
            this.Loaded += AfrControl_Loaded;
            this.Unloaded += AfrControl_Unloaded;
            this.DataContext = this;
            this.label.Foreground = ColorPalette.IndicatorColor;
            this.level.Foreground = ColorPalette.IndicatorColor;
            this.afrOutline.Stroke = ColorPalette.OutlineColor;
            this.allLeds = new Ellipse[]
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
                this.led47
            };

            foreach (Ellipse led in this.allLeds)
            {
                led.Fill = ColorPalette.GaugeColor;
                led.Stroke = ColorPalette.OutlineColor;
            }
        }

        /// <summary>
        /// Gets or sets the AFR level.
        /// </summary>
        /// <value>
        /// The AFR level.
        /// </value>
        [DesignerCategory("AfrControl")]
        [Description("The AFR level.")]
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
        /// Gets or sets the too rich property.
        /// </summary>
        /// <value>
        /// The too rich property.
        /// </value>
        [DesignerCategory("AfrControl")]
        [Description("The too rich property.")]
        public bool TooRich
        {
            get
            {
                return (bool)this.GetValue(TooRichProperty);
            }
            set
            {
                this.SetValue(TooRichProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the too lean property.
        /// </summary>
        /// <value>
        /// The too lean property.
        /// </value>
        [DesignerCategory("AfrControl")]
        [Description("The too lean property.")]
        public bool TooLean
        {
            get
            {
                return (bool)this.GetValue(TooLeanProperty);
            }
            set
            {
                this.SetValue(TooLeanProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the idle property.
        /// </summary>
        /// <value>
        /// The idle property.
        /// </value>
        [DesignerCategory("AfrControl")]
        [Description("The idle property.")]
        public bool Idle
        {
            get
            {
                return (bool)this.GetValue(TooLeanProperty);
            }
            set
            {
                this.SetValue(TooLeanProperty, value);
            }
        }

        /// <summary>
        /// Handles the Loaded event of the AfrControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AfrControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            this.updateTimer.Tick += UpdateTimer_Tick; ;
            this.updateTimer.Start();
        }

        /// <summary>
        /// Handles the Unloaded event of the AfrControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AfrControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.updateTimer.Stop();
        }

        /// <summary>
        /// AFR gauge update tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void UpdateTimer_Tick(object sender, object e)
        {
            // Color the AFR display appropriately
            bool shouldWarn = !this.Idle && (this.TooRich || this.TooLean);
            if (this.warning != shouldWarn)
            {
                // We're moving to some other warning state
                this.stateChanged = true;
                this.whenChanged = DateTime.Now;
                this.level.Foreground = ColorPalette.NeedleColor;
            }
            else if (this.stateChanged && DateTime.Now > this.whenChanged)
            {
                this.stateChanged = false;
                if (this.warning)
                {
                    this.level.Foreground = ColorPalette.WarningColor;
                }
                else
                {
                    this.level.Foreground = ColorPalette.IndicatorColor;
                }
            }

            // Now update and color the radial gauge. We just pick which ones should be visible and which should be hidden.
            double afrLevel = Math.Min(18.0, Math.Max(11.0, this.Level));
            int startIndex = this.allLeds.Length / 2; ;
            int endIndex;
            if (this.Idle)
            {
                startIndex = 0;
                endIndex = this.allLeds.Length;
            }
            else if (this.TooRich)
            {
                endIndex = 0;
            }
            else if (this.TooLean)
            {
                endIndex = this.allLeds.Length;
            }
            else
            {
                double offsetLambda = (afrLevel - 11.0) / 7.0;
                endIndex = (int)(Math.Ceiling((double)this.allLeds.Length * offsetLambda));
            }

            if (endIndex < startIndex)
            {
                int t = startIndex;
                startIndex = endIndex;
                endIndex = t;
            }

            for (int i = 0; i < this.allLeds.Length; ++i)
            {
                if (i >= startIndex && i < endIndex)
                {
                    this.allLeds[i].Visibility = Visibility.Visible;
                    if (this.warning != shouldWarn)
                    {
                        this.allLeds[i].Fill = ColorPalette.NeedleColor;
                        this.allLeds[i].Stroke = ColorPalette.OutlineColor;
                    }
                    if (shouldWarn)
                    {
                        this.allLeds[i].Stroke = ColorPalette.WarningColor;
                        this.allLeds[i].Fill = ColorPalette.WarningColor;
                    }
                    else
                    {
                        this.allLeds[i].Fill = ColorPalette.GaugeColor;
                        this.allLeds[i].Stroke = ColorPalette.OutlineColor;
                    }
                }
                else
                {
                    this.allLeds[i].Visibility = Visibility.Collapsed;
                }
            }

            this.warning = shouldWarn;
        }
    }
}

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
    using Windows.UI.Xaml.Navigation;
    using Interfaces;

    /// <summary>
    /// The oil temp, coolant temp, and intake temp indicators on the right side of the display will display the temperature levels as well as blink a warning indicator if one of the temperature levels is outside operating condition, by either being too low or too high.
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.UserControl" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector2" />
    public sealed partial class TemperatureControl : UserControl
    {
        /// <summary>
        /// The temp level property
        /// </summary>
        public static readonly DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(int), typeof(TemperatureControl), new PropertyMetadata(default(int)));

        /// <summary>
        /// The min level property
        /// </summary>
        public static readonly DependencyProperty WarningProperty = DependencyProperty.Register("Warning", typeof(bool), typeof(TemperatureControl), new PropertyMetadata(default(bool)));

        /// <summary>
        /// The max level property
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TemperatureControl), new PropertyMetadata("Temp:"));

        /// <summary>
        /// The last recorded level
        /// </summary>
        private int lastLevel = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureControl"/> class.
        /// </summary>
        public TemperatureControl()
        {
            this.InitializeComponent();
            ((ITinastApp)Application.Current).IndicatorTick += UpdateTimer_Tick;
            this.DataContext = this;
            this.temp.Foreground = ColorPalette.IndicatorColor;
            this.text.Foreground = ColorPalette.IndicatorColor;
        }

        /// <summary>
        /// Gets or sets the temperature level.
        /// </summary>
        /// <value>
        /// The temperature level.
        /// </value>
        [DesignerCategory("TemperatureControl")]
        [Description("The temperature level.")]
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
        /// Gets or sets the warning level.
        /// </summary>
        /// <value>
        /// The warning level.
        /// </value>
        [DesignerCategory("TemperatureControl")]
        [Description("The warning level.")]
        public bool Warning
        {
            get
            {
                return (bool)this.GetValue(WarningProperty);
            }
            set
            {
                this.SetValue(WarningProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        [DesignerCategory("TemperatureControl")]
        [Description("The text.")]
        public string Text
        {
            get
            {
                return (string)this.GetValue(TextProperty);
            }
            set
            {
                this.SetValue(TextProperty, value);
            }
        }

        /// <summary>
        /// Updates the timer tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            int level = this.Level;
            if (lastLevel != level)
            {
                this.temp.Foreground = ColorPalette.NeedleColor;
            }
            else if (this.Warning)
            {
                this.temp.Foreground = ColorPalette.WarningColor;
            }
            else
            {
                this.temp.Foreground = ColorPalette.IndicatorColor;
            }

            lastLevel = this.Level;
        }
    }
}

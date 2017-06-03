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

    /// <summary>
    /// If the connection to the scantool is lost, either due to a loss of ECU power or due to a loss of connectivity to the scantool, a CAN connection indicator will begin blinking in the lower right of the display.
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.UserControl" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector2" />
    public sealed partial class MalfunctionControl : UserControl
    {
        /// <summary>
        /// The on timer
        /// </summary>
        private DispatcherTimer onTimer;

        /// <summary>
        /// The off timer
        /// </summary>
        private DispatcherTimer offTimer;

        /// <summary>
        /// Gets or sets the malfunction indicator.
        /// </summary>
        /// <value>
        /// The malfunction indicator.
        /// </value>
        [Description("The malfunction indicator image source.")]
        public ImageSource LampSource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the system is malfunctioning.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the system is malfunctioning, otherwise false.
        /// </value>
        [Description("Whether the system is malfunctioning.")]
        public bool Malfunctioning { get; set; }

        /// <summary>
        /// Gets or sets the on interval in ms.
        /// </summary>
        /// <value>
        /// The on interval.
        /// </value>
        [Description("The indicator on interval.")]
        public int OnInterval { get; set; } = 400;

        /// <summary>
        /// Gets or sets the off interval in ms.
        /// </summary>
        /// <value>
        /// The on interval.
        /// </value>
        [Description("The indicator off interval.")]
        public int OffInterval { get; set; } = 600;

        /// <summary>
        /// Initializes a new instance of the <see cref="MalfunctionControl"/> class.
        /// </summary>
        public MalfunctionControl()
        {
            this.InitializeComponent();
            this.Loaded += MalfunctionControl_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the MalfunctionControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MalfunctionControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.onTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(this.OnInterval) };
            this.onTimer.Tick += OnTimer_Tick;
            this.offTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(this.OnInterval + this.OffInterval) };
            this.offTimer.Tick += OffTimer_Tick;
            this.onTimer.Start();
            this.offTimer.Start();
        }

        /// <summary>
        /// Called when the on timer ticks.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnTimer_Tick(object sender, object e)
        {
            this.onTimer.Interval = TimeSpan.FromMilliseconds(this.OnInterval);
            if (this.Malfunctioning)
            {
                this.lamp.Visibility = Visibility.Visible;
            }
            else
            {
                this.lamp.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Called when the of timer ticks.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OffTimer_Tick(object sender, object e)
        {
            this.offTimer.Interval = TimeSpan.FromMilliseconds(this.OnInterval + this.OffInterval);
            if (this.Malfunctioning)
            {
                this.lamp.Visibility = Visibility.Collapsed;
            }
        }
    }
}

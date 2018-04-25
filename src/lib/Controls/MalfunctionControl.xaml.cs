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
    /// If the connection to the scantool is lost, either due to a loss of ECU power or due to a loss of connectivity to the scantool, a CAN connection indicator will begin blinking in the lower right of the display.
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.UserControl" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector2" />
    public sealed partial class MalfunctionControl : UserControl
    {
        /// <summary>
        /// The lamp source property
        /// </summary>
        public static readonly DependencyProperty LampSourceProperty = DependencyProperty.Register("LampSource", typeof(ImageSource), typeof(MalfunctionControl), new PropertyMetadata(default(ImageSource)));

        /// <summary>
        /// The lamp source property
        /// </summary>
        public static readonly DependencyProperty MalfunctioningProperty = DependencyProperty.Register("Malfunctioning", typeof(bool), typeof(MalfunctionControl), new PropertyMetadata(default(bool)));

        /// <summary>
        /// The lamp source property
        /// </summary>
        public static readonly DependencyProperty OnIntervalProperty = DependencyProperty.Register("OnInterval", typeof(int), typeof(MalfunctionControl), new PropertyMetadata(400));

        /// <summary>
        /// The lamp source property
        /// </summary>
        public static readonly DependencyProperty OffIntervalProperty = DependencyProperty.Register("OffInterval", typeof(int), typeof(MalfunctionControl), new PropertyMetadata(600));

        /// <summary>
        /// The on timer
        /// </summary>
        private DispatcherTimer timer;

        /// <summary>
        /// The MIL mode duration
        /// </summary>
        private DateTime start = DateTime.Now;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MalfunctionControl"/> class.
        /// </summary>
        public MalfunctionControl()
        {
            this.InitializeComponent();
            this.Loaded += MalfunctionControl_Loaded;
            this.Unloaded += MalfunctionControl_Unloaded;
            this.DataContext = this;
        }

        /// <summary>
        /// Gets or sets the malfunction indicator.
        /// </summary>
        /// <value>
        /// The malfunction indicator.
        /// </value>
        [DesignerCategory("MalfunctionControl")]
        [Description("The malfunction indicator image source.")]
        public ImageSource LampSource
        {
            get
            {
                return (ImageSource)this.GetValue(LampSourceProperty);
            }
            set
            {
                this.SetValue(LampSourceProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the system is malfunctioning.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the system is malfunctioning, otherwise false.
        /// </value>
        [DesignerCategory("MalfunctionControl")]
        [Description("Whether the system is malfunctioning.")]
        public bool Malfunctioning
        {
            get
            {
                return (bool)this.GetValue(MalfunctioningProperty);
            }
            set
            {
                this.SetValue(MalfunctioningProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the on interval in ms.
        /// </summary>
        /// <value>
        /// The on interval.
        /// </value>
        [DesignerCategory("MalfunctionControl")]
        [Description("The indicator on interval.")]
        public int OnInterval
        {
            get
            {
                return (int)this.GetValue(OnIntervalProperty);
            }
            set
            {
                this.SetValue(OnIntervalProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the off interval in ms.
        /// </summary>
        /// <value>
        /// The on interval.
        /// </value>
        [DesignerCategory("MalfunctionControl")]
        [Description("The indicator off interval.")]
        public int OffInterval
        {
            get
            {
                return (int)this.GetValue(OffIntervalProperty);
            }
            set
            {
                this.SetValue(OffIntervalProperty, value);
            }
        }

        /// <summary>
        /// Handles the Loaded event of the MalfunctionControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MalfunctionControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            this.timer.Tick += Timer_Tick;
            this.timer.Start();
        }

        /// <summary>
        /// Handles the Unloaded event of the MalfunctionControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MalfunctionControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.timer.Stop();
        }


        /// <summary>
        /// Called when the on timer ticks.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Timer_Tick(object sender, object e)
        {
            this.timer.Interval = TimeSpan.FromMilliseconds(this.OnInterval);
            if (this.Malfunctioning)
            {
                DateTime now = DateTime.Now;
                int duration = (int)(now - this.start).TotalMilliseconds;
                if (this.lamp.Visibility == Visibility.Visible && duration > this.OnInterval)
                {
                    this.lamp.Visibility = Visibility.Collapsed;
                    this.start = now;
                }
                else if (duration > this.OffInterval)
                {
                    this.lamp.Visibility = Visibility.Visible;
                    this.start = now;
                }
            }
            else
            {
                this.lamp.Visibility = Visibility.Collapsed;
            }
        }
    }
}

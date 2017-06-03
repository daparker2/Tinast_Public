namespace DP.Tinast.Controls
{
    using System;
    using System.Collections.Generic;
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
    /// The oil temp, coolant temp, and intake temp indicators on the right side of the display will display the temperature levels as well as blink a warning indicator if one of the temperature levels is outside operating condition, by either being too low or too high.
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.UserControl" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector2" />
    public sealed partial class TemperatureControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureControl"/> class.
        /// </summary>
        public TemperatureControl()
        {
            this.InitializeComponent();
        }
    }
}

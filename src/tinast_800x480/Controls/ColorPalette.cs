namespace DP.Tinast.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.UI;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Media;

    /// <summary>
    /// Represent the standard display color palette
    /// </summary>
    static class ColorPalette
    {
        /// <summary>
        /// The gauge color
        /// </summary>
        public static readonly Brush GaugeColor = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));

        /// <summary>
        /// The inactive color
        /// </summary>
        public static readonly Brush InactiveColor = new SolidColorBrush(Color.FromArgb(0xff, 0x19, 0x0e, 0x00));

        /// <summary>
        /// The needle color
        /// </summary>
        public static readonly Brush NeedleColor = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0x00));

        /// <summary>
        /// The indicator color
        /// </summary>
        public static readonly Brush IndicatorColor = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));

        /// <summary>
        /// The warning color
        /// </summary>
        public static readonly Brush WarningColor = new SolidColorBrush(Color.FromArgb(0xff, 0x00, 0xff, 0xff));

        /// <summary>
        /// The outline color
        /// </summary>
        public static readonly Brush OutlineColor = new SolidColorBrush(Color.FromArgb(0xff, 0x7f, 0x7f, 0xff));
    }
}

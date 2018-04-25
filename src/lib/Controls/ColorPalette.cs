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
        public static readonly Brush GaugeColor = new SolidColorBrush(Color.FromArgb(0xff, 255, 255, 255));

        /// <summary>
        /// The inactive color
        /// </summary>
        public static readonly Brush InactiveColor = new SolidColorBrush(Color.FromArgb(0x77, 128, 255, 128));

        /// <summary>
        /// The needle color
        /// </summary>
        public static readonly Brush NeedleColor = new SolidColorBrush(Color.FromArgb(0xff, 0x77, 0xff, 0x77));

        /// <summary>
        /// The indicator color
        /// </summary>
        public static readonly Brush IndicatorColor = new SolidColorBrush(Color.FromArgb(0xff, 0, 0, 0));

        /// <summary>
        /// The indicator background
        /// </summary>
        public static readonly Brush IndicatorBackground = new SolidColorBrush(Color.FromArgb(0xdd, 0, 255, 0));

        /// <summary>
        /// The indicator background
        /// </summary>
        public static readonly Brush IndicatorWarningBackground = new SolidColorBrush(Color.FromArgb(0xff, 0, 0, 0));

        /// <summary>
        /// The warning color
        /// </summary>
        public static readonly Brush WarningColor = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));

        /// <summary>
        /// The outline color
        /// </summary>
        public static readonly Brush OutlineColor = new SolidColorBrush(Color.FromArgb(0xff, 128, 255, 128));
    }
}

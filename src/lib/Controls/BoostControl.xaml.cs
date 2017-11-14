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
    public sealed partial class BoostControl : BoostControlBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoostControl"/> class.
        /// </summary>
        public BoostControl()
        {
            this.InitializeComponent();
            this.label.Foreground = ColorPalette.IndicatorColor;
            this.outline1.Stroke = ColorPalette.OutlineColor;
            this.outline2.Stroke = ColorPalette.OutlineColor;
        }

        /// <summary>
        /// Gets all the LEDs for the Control.
        /// </summary>
        /// <returns></returns>
        protected override Polygon[] GetAllLeds()
        {
            return new Polygon[]
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
        }
    }
}

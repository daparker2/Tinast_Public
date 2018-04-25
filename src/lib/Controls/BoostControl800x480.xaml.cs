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
    /// Represent a boost control for 800x480 devices.
    /// </summary>
    public sealed partial class BoostControl800x480 : BoostControlBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoostControl"/> class.
        /// </summary>
        public BoostControl800x480()
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
                this.led51,
                this.led52,
                this.led53,
                this.led54,
                this.led55,
                this.led56,
                this.led57,
                this.led58,
                this.led59,
                this.led60,
                this.led61,
                this.led62,
                this.led63,
                this.led64,
                this.led65,
                this.led66,
                this.led67,
                this.led68,
                this.led69,
                this.led70,
                this.led71,
                this.led72,
                this.led73,
            };
        }
    }
}

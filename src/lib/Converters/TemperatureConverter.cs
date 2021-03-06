﻿namespace DP.Tinast.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.UI.Xaml.Data;

    /// <summary>
    /// Represent a temperature converter
    /// </summary>
    public class TemperatureConverter : IValueConverter
    {
        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(string))
            {
                string s = value.ToString() + "°";
                if (s.Length < 5)
                {
                    s = s.PadLeft(5, ' ');
                }

                return s;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(int))
            {
                return int.Parse(((string)value).Substring(0, ((string)value).Length - 1), CultureInfo.CurrentCulture);
            }

            throw new NotImplementedException();
        }
    }
}

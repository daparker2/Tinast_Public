﻿namespace DP.Tinast.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.UI.Xaml.Data;
    using DP.Tinast.Converters;
    using MetroLog;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// View converter tests.
    /// </summary>
    /// <seealso cref="DP.Tinast.Tests.TestBase{DP.Tinast.Tests.ConfigTests}" />
    public class ConverterTests : TestBase<ConfigTests>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConverterTests"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        /// <remarks>
        /// Use the <see cref="!:CreateLogger&lt;TTest&gt;(LoggingConfiguration)" /> method to access a suitable logging context for the test. Don't use <see cref="F:DP.Tinast.Tests.TestBase`1.outputHelper" /> directly.
        /// </remarks>
        public ConverterTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        /// <summary>
        /// Tests that the converter type can convert the given value correctly.
        /// </summary>
        /// <param name="converterType">Type of the converter.</param>
        /// <param name="value">The value.</param>
        /// <param name="expectedText">The expected text.</param>
        /// <returns></returns>
        [Theory]
        [InlineData(typeof(AfrConverter), -14.0, null, "en", "-14.0%")]
        [InlineData(typeof(AfrConverter), 14.0, null, "en", " 14.0%")]
        [InlineData(typeof(AfrConverter), 0.0, null, "en", "  0.0%")]
        [InlineData(typeof(BoostConverter), -14.0, null, "en", "-14.0")]
        [InlineData(typeof(BoostConverter), 14.0, null, "en", " 14.0")]
        [InlineData(typeof(BoostConverter), 0.0, null, "en", "  0.0")]
        [InlineData(typeof(TemperatureConverter), -14, null, "en", " -14°")]
        [InlineData(typeof(TemperatureConverter), 14, null, "en", "  14°")]
        [InlineData(typeof(TemperatureConverter), 0, null, "en", "   0°")]
        public void String_Converter_Of_Type_Converts_Value_Correctly(Type converterType, object value, object parameter, string language, string expectedText)
        {
            object converterObject = Activator.CreateInstance(converterType);
            Assert.IsAssignableFrom(typeof(IValueConverter), converterObject);
            MethodInfo convertMethod = converterType.GetMethod("Convert");
            MethodInfo convertBackMethod = converterType.GetMethod("ConvertBack");
            object text = convertMethod.Invoke(converterObject, new object[] { value, typeof(string), parameter, language });
            Assert.IsAssignableFrom(typeof(string), text);
            Assert.Equal(expectedText, text.ToString());
            object actualValue = convertBackMethod.Invoke(converterObject, new object[] { text.ToString(), value.GetType(), parameter, language });
            Assert.IsAssignableFrom(value.GetType(), actualValue);
            Assert.Equal(actualValue.ToString(), value.ToString());
        }
    }
}

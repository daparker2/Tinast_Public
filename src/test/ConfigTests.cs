

namespace DP.Tinast.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using DP.Tinast.Config;
    using MetroLog;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Tests for config namespace.
    /// </summary>
    /// <seealso cref="DP.Tinast.Tests.TestBase{DP.Tinast.Tests.ConfigTests}" />
    public class ConfigTests : TestBase<ConfigTests>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigTests"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        /// <remarks>
        /// Use the <see cref="!:CreateLogger&lt;TTest&gt;(LoggingConfiguration)" /> method to access a suitable logging context for the test. Don't use <see cref="F:DP.Tinast.Tests.TestBase`1.outputHelper" /> directly.
        /// </remarks>
        public ConfigTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        /// <summary>
        /// Displays the given configuration value is not mutated.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData("IntakeTempPidType", "BoostPidType", PidType.Subaru)]
        [InlineData("BoostPidType", "AfrPidType", PidType.Subaru)]
        [InlineData("LoadPidType", "BoostPidType", PidType.Subaru)]
        [InlineData("OilTempPidType", "LoadPidType", PidType.Subaru)]
        [InlineData("LoadPidType", "CoolantTempPidType", PidType.Subaru)]
        [InlineData("CoolantTempPidType", "IntakeTempPidType", PidType.Subaru)]
        [InlineData("BoostOffset", "IntakeTempMin", -10)]
        [InlineData("MaxBoost", "BoostOffset", -10)]
        [InlineData("MaxIdleLoad", "MaxBoost", -10)]
        [InlineData("OilTempMin", "MaxIdleLoad", -10)]
        [InlineData("OilTempMax", "OilTempMin", -10)]
        [InlineData("CoolantTempMin", "OilTempMax", -10)]
        [InlineData("IntakeTempMin", "CoolantTempMin", -10)]
        [InlineData("AggressiveTiming", "IntakeTempMin", 1)]
        [InlineData("MaxPidsAtOnce", "AggressiveTiming", true)]
        public async Task DisplayConfiguration_Value_Not_Mutated(string propertyName, string propertyName2, object value)
        {
            ILogger logger = this.CreateLogger();
            logger.Debug("Checking if '{0}' is mutated", propertyName);
            DisplayConfiguration config = new DisplayConfiguration();
            PropertyInfo propInfo = config.GetType().GetProperty(propertyName);
            PropertyInfo propInfo2 = config.GetType().GetProperty(propertyName2);
            object expected = propInfo.GetValue(config);
            propInfo2.SetValue(config, value);
            await config.Save();
            config = await DisplayConfiguration.Load();
            object actual = propInfo.GetValue(config);
            Assert.Equal(expected.ToString(), actual.ToString());
        }

        /// <summary>
        /// Tests that DisplayConfig stores the given parameter correctly.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <returns></returns>
        [Theory]
        [InlineData("BoostPidType", PidType.Subaru)]
        [InlineData("AfrPidType", PidType.Subaru)]
        [InlineData("LoadPidType", PidType.Subaru)]
        [InlineData("OilTempPidType", PidType.Subaru)]
        [InlineData("CoolantTempPidType", PidType.Subaru)]
        [InlineData("IntakeTempPidType", PidType.Subaru)]
        [InlineData("BoostPidType", PidType.Obd2)]
        [InlineData("AfrPidType", PidType.Obd2)]
        [InlineData("LoadPidType", PidType.Obd2)]
        [InlineData("OilTempPidType", PidType.Obd2)]
        [InlineData("CoolantTempPidType", PidType.Obd2)]
        [InlineData("IntakeTempPidType", PidType.Obd2)]
        [InlineData("BoostOffset", -10)]
        [InlineData("BoostOffset", 10)]
        [InlineData("MaxBoost", -10)]
        [InlineData("MaxBoost", 10)]
        [InlineData("MaxIdleLoad", -10)]
        [InlineData("MaxIdleLoad", 10)]
        [InlineData("OilTempMin", -10)]
        [InlineData("OilTempMin", 10)]
        [InlineData("OilTempMax", -10)]
        [InlineData("OilTempMax", 10)]
        [InlineData("CoolantTempMin", -10)]
        [InlineData("CoolantTempMax", 10)]
        [InlineData("IntakeTempMin", -10)]
        [InlineData("IntakeTempMax", 10)]
        [InlineData("AggressiveTiming", true)]
        [InlineData("AggressiveTiming", false)]
        [InlineData("MaxPidsAtOnce", 1)]
        [InlineData("MaxPidsAtOnce", 0)]
        public async Task DisplayConfiguration_Stores_Parameter_Correctly(string propertyName, object expectedValue)
        {
            ILogger logger = this.CreateLogger();
            logger.Debug("Checking of '{0}' can be set to {1}", expectedValue);
            DisplayConfiguration config = new DisplayConfiguration();
            PropertyInfo propInfo = config.GetType().GetProperty(propertyName);
            propInfo.SetValue(config, expectedValue);
            await config.Save();
            config = await DisplayConfiguration.Load();
            object actual = propInfo.GetValue(config);
            Assert.Equal(expectedValue.ToString(), actual.ToString());
        }
    }
}

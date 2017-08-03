namespace DP.Tinast.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.UI.Xaml.Data;
    using Config;
    using Elm327;
    using Interfaces;
    using ViewModel;
    using MetroLog;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Represent tests for the view model.
    /// </summary>
    public class ViewModelTests : TestBase<ConfigTests>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelTests"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        /// <remarks>
        /// Use the <see cref="!:CreateLogger&lt;TTest&gt;(LoggingConfiguration)" /> method to access a suitable logging context for the test. Don't use <see cref="F:DP.Tinast.Tests.TestBase`1.outputHelper" /> directly.
        /// </remarks>
        public ViewModelTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        /// <summary>
        /// Verifies the display view model will connect the driver on the first tick, and that the driver remains connected.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(20)]
        public async Task DisplayViewModel_Will_Connect_Driver_On_First_Tick_And_Stay_Connected(int numIterations)
        {
            MockDisplayDriver displayDriver = new MockDisplayDriver();
            Assert.False(displayDriver.Connected);
            DisplayViewModel viewModel = new DisplayViewModel(displayDriver, new DisplayConfiguration());
            Assert.True(viewModel.Obd2Connecting);
            displayDriver.SetPidResult(new PidResult());
            await viewModel.Tick();
            Assert.False(viewModel.Obd2Connecting);
            Assert.True(displayDriver.Connected);
            for (int i = 0; i < numIterations; ++i)
            {
                await viewModel.Tick();
                Assert.False(viewModel.Obd2Connecting);
                Assert.True(displayDriver.Connected);
            }
        }

        /// <summary>
        /// Verifies the display view model will fire a property changed notification for the given property after a certain amount of iterations occurs.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData("EngineCoolantTemp", "CoolantTemp", 25, 20)]
        [InlineData("EngineOilTemp", "OilTemp", 25, 20)]
        [InlineData("EngineIntakeTemp", "IntakeTemp", 25, 20)]
        [InlineData("EngineBoost", "Boost", 10, 20)]
        [InlineData("EngineAfr", "Afr", 14.7, 20)]
        [InlineData("EngineAfr", "Afr", 11, 20)]
        [InlineData("EngineLoad", "Load", 25, 20)]
        public async Task DisplayViewModel_Will_Fire_Value_Type_Events(string viewModelPropertyName, string pidResultPropertyName, object pidResultValue, int numIterations)
        {
            MockDisplayDriver displayDriver = new MockDisplayDriver();
            DisplayViewModel viewModel = new DisplayViewModel(displayDriver, new DisplayConfiguration());

            displayDriver.SetPidResult(new PidResult());
            await viewModel.Tick();

            PidResult nextResult = new PidResult();
            PropertyInfo pidResultPropertyInfo = nextResult.GetType().GetProperty(pidResultPropertyName);
            pidResultPropertyInfo.SetValue(nextResult, pidResultValue);
            bool propSet = false;
            viewModel.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == viewModelPropertyName)
                {
                    propSet = true;
                }
            };

            displayDriver.SetPidResult(nextResult);
            for (int i = 0; i < numIterations; ++i)
            {
                await viewModel.Tick();
            }

            Assert.True(propSet);
            PropertyInfo viewModelPropertyInfo = viewModel.GetType().GetProperty(viewModelPropertyName);
            object actual = viewModelPropertyInfo.GetValue(viewModel);
            Assert.Equal(pidResultValue.ToString(), actual.ToString());
        }

        /// <summary>
        /// Verifies the display view model will fire a property changed notification for the given property after a certain amount of iterations occurs.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData("CoolantTempWarn", "CoolantTemp", 125, 20, true, "CoolantTempMin", 20, "CoolantTempMax", 100)]
        [InlineData("CoolantTempWarn", "CoolantTemp", 15, 20, true, "CoolantTempMin", 20, "CoolantTempMax", 100)]
        [InlineData("CoolantTempWarn", "CoolantTemp", 50, 20, false, "CoolantTempMin", 20, "CoolantTempMax", 100)]
        [InlineData("OilTempWarn", "OilTemp", 125, 20, true, "OilTempMin", 20, "OilTempMax", 100)]
        [InlineData("OilTempWarn", "OilTemp", 15, 20, true, "OilTempMin", 20, "OilTempMax", 100)]
        [InlineData("OilTempWarn", "OilTemp", 50, 20, false, "OilTempMin", 20, "OilTempMax", 100)]
        [InlineData("IntakeTempWarn", "IntakeTemp", 125, 20, true, "IntakeTempMin", 20, "IntakeTempMax", 100)]
        [InlineData("IntakeTempWarn", "IntakeTemp", 15, 20, true, "IntakeTempMin", 20, "IntakeTempMax", 100)]
        [InlineData("TempWarning", "CoolantTemp", 125, 20, true, "CoolantTempMin", 20, "CoolantTempMax", 100)]
        [InlineData("TempWarning", "CoolantTemp", 15, 20, true, "CoolantTempMin", 20, "CoolantTempMax", 100)]
        [InlineData("TempWarning", "CoolantTemp", 50, 20, false, "CoolantTempMin", 20, "CoolantTempMax", 100)]
        [InlineData("TempWarning", "OilTemp", 125, 20, true, "OilTempMin", 20, "OilTempMax", 100)]
        [InlineData("TempWarning", "OilTemp", 15, 20, true, "OilTempMin", 20, "OilTempMax", 100)]
        [InlineData("TempWarning", "OilTemp", 50, 20, false, "OilTempMin", 20, "OilTempMax", 100)]
        [InlineData("TempWarning", "IntakeTemp", 125, 20, true, "IntakeTempMin", 20, "IntakeTempMax", 100)]
        [InlineData("TempWarning", "IntakeTemp", 15, 20, true, "IntakeTempMin", 20, "IntakeTempMax", 100)]
        [InlineData("TempWarning", "IntakeTemp", 50, 20, false, "IntakeTempMin", 20, "IntakeTempMax", 100)]
        [InlineData("AfrTooLean", "Afr", 19.0, 20, true, "MaxIdleLoad", 25, null, null)]
        [InlineData("AfrTooRich", "Afr", 10.0, 20, true, "MaxIdleLoad", 25, null, null)]
        [InlineData("AfrTooRich", "Afr", 14.7, 20, false, "MaxIdleLoad", 25, null, null)]
        [InlineData("IdleLoad", "Load", 1, 20, true, "MaxIdleLoad", 25, null, null)]
        [InlineData("IdleLoad", "Load", 50, 20, false, "MaxIdleLoad", 25, null, null)]
        public async Task DisplayViewModel_Will_Set_Warning_Property_When_Value_Out_Of_Range(string viewModelPropertyName, string pidResultPropertyName, object pidResultValue, int numIterations, bool expectedState, string configProperty1, object configValue1, string configProperty2, object configValue2)
        {
            DisplayConfiguration config = new DisplayConfiguration();
            PropertyInfo configPropInfo = config.GetType().GetProperty(configProperty1);
            configPropInfo.SetValue(config, configValue1);
            if (!string.IsNullOrEmpty(configProperty2))
            {
                PropertyInfo config2PropInfo = config.GetType().GetProperty(configProperty2);
                config2PropInfo.SetValue(config, configValue2);
            }

            MockDisplayDriver displayDriver = new MockDisplayDriver();
            DisplayViewModel viewModel = new DisplayViewModel(displayDriver, config);

            PropertyInfo viewModelPropertyInfo = viewModel.GetType().GetProperty(viewModelPropertyName);
            object warningStatus = viewModelPropertyInfo.GetValue(viewModel);
            Assert.Equal(false, warningStatus);

            PidResult nextResult = new PidResult();
            PropertyInfo pidResultPropertyInfo = nextResult.GetType().GetProperty(pidResultPropertyName);
            pidResultPropertyInfo.SetValue(nextResult, pidResultValue);
            displayDriver.SetPidResult(nextResult);
            bool propSet = false;
            viewModel.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == viewModelPropertyName)
                {
                    propSet = true;
                }
            };

            await viewModel.Tick();
            for (int i = 0; i < numIterations; ++i)
            {
                await viewModel.Tick();
            }

            warningStatus = viewModelPropertyInfo.GetValue(viewModel);
            Assert.Equal(expectedState, warningStatus);
            if (expectedState)
            {
                Assert.True(propSet);
            }
        }

        /// <summary>
        /// Verifies the display view model reports a fault.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DisplayViewModel_Reports_Fault()
        {
            MockDisplayDriver displayDriver = new MockDisplayDriver();
            DisplayViewModel viewModel = new DisplayViewModel(displayDriver, new DisplayConfiguration());
            displayDriver.SetPidResult(new PidResult());
            Assert.False(viewModel.Faulted);
            bool propSet = false;
            viewModel.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == "Faulted")
                {
                    propSet = true;
                }
            };

            viewModel.Fault();
            await viewModel.Tick();
            Assert.True(viewModel.Faulted);
            Assert.True(propSet);
        }

        /// <summary>
        /// Verifies the DisplayViewModel ticks against the OBD2 adapter for the given number of iterations.
        /// </summary>
        /// <param name="numIterations">The number iterations.</param>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task DisplayView_Model_Ticks_Against_Obd2_Adapter(int numIterations)
        {
            // Designed to work against the ScanTool.net ECUSIM 2000 simulator: https://www.scantool.net/scantool/downloads/101/ecusim_2000-ug.pdf
            using (BluetoothElm327Connection connection = (await BluetoothElm327Connection.GetAvailableConnectionsAsync()).FirstOrDefault())
            {
                DisplayConfiguration config = new DisplayConfiguration();
                Elm327Driver driver = new Elm327Driver(config, connection);
                DisplayViewModel viewModel = new DisplayViewModel(driver, config);
                for (int i = 0; i < numIterations; ++i)
                {
                    bool wasConnecting = viewModel.Obd2Connecting;
                    await viewModel.Tick();
                    if (wasConnecting)
                    {
                        Assert.False(viewModel.Obd2Connecting);
                    }
                }
            }
        }
    }
}

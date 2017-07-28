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
    using Interfaces;
    using Elm327;
    using MetroLog;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ELM 327 tests
    /// </summary>
    /// <seealso cref="DP.Tinast.Tests.TestBase{DP.Tinast.Tests.Elm327Tests}" />
    public class Elm327Tests : TestBase<Elm327Tests>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Elm327Tests"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        /// <remarks>
        /// Use the <see cref="!:CreateLogger&lt;TTest&gt;(LoggingConfiguration)" /> method to access a suitable logging context for the test. Don't use <see cref="F:DP.Tinast.Tests.TestBase`1.outputHelper" /> directly.
        /// </remarks>
        public Elm327Tests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        /// <summary>
        /// Verifies the elm327 connection discovers the bluetooth device.
        /// </summary>
        [Fact]
        public async Task BluetoothElm327Connection_Discovers_Bluetooth_Device()
        {
            ICollection<BluetoothElm327Connection> connections = await BluetoothElm327Connection.GetAvailableConnectionsAsync();
            Assert.NotNull(connections);
            Assert.InRange(connections.Count, 1, int.MaxValue);
            ILogger log = this.CreateLogger();
            log.Info("Found BT device '{0}'", connections.FirstOrDefault().DeviceName);
        }

        /// <summary>
        /// Verifies the elm327 connection connects and disconnects from the bluetooth device.
        /// </summary>
        /// <param name="numIterations">The number iterations.</param>
        /// <returns></returns>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task BluetoothElm327Connection_Connects_And_Disconnects_From_Device(int numIterations)
        {
            using (BluetoothElm327Connection connection = (await BluetoothElm327Connection.GetAvailableConnectionsAsync()).FirstOrDefault())
            {
                for (int i = 0; i < numIterations; ++i)
                {
                    Assert.False(connection.Opened);
                    await connection.OpenAsync();
                    Assert.True(connection.Opened);
                    connection.Close();
                }

                Assert.False(connection.Opened);
            }
        }

        /// <summary>
        /// Verifies the parser can evaluate the given obd2 adapter message.
        /// </summary>
        /// <param name="numIterations">The number iterations.</param>
        /// <param name="message">The message.</param>
        /// <param name="expectedResponse">The response.</param>
        /// <returns></returns>
        [Theory]
        [InlineData(1, "atz", new string[] { "atz", "ELM327 v1.5" })]
        [InlineData(5, "atz", new string[] { "atz", "ELM327 v1.5" })]
        [InlineData(10, "atz", new string[] { "atz", "ELM327 v1.5" })]
        [InlineData(100, "atz", new string[] { "atz", "ELM327 v1.5" })]
        public async Task Elm327Parser_Can_Evaluate_Obd2_Adapter_Message(int numIterations, string message, string[] expectedResponse)
        {
            using (BluetoothElm327Connection connection = (await BluetoothElm327Connection.GetAvailableConnectionsAsync()).FirstOrDefault())
            {
                Elm327Session session = new Elm327Session(connection);
                await connection.OpenAsync();
                for (int i = 0; i < numIterations; ++i)
                {
                    string[] actualResponse = await session.SendCommand(message);
                    Assert.True(expectedResponse.SequenceEqual(actualResponse), "Response mismatches expected.");
                }

                connection.Close();
            }
        }
    }
}

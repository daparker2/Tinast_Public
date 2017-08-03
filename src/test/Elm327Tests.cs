namespace DP.Tinast.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
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

        // The below stuff is expected to throw sometimes. We just verify that successive attempts don't produce the same failure.

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
                string lastExceptionMessage = null;
                Assert.False(connection.Opened);
                for (int i = 0; i < numIterations; ++i)
                {
                    try
                    {
                        await connection.OpenAsync();
                    }
                    catch (Exception ex)
                    {
                        Assert.NotEqual(lastExceptionMessage, ex.Message);
                        lastExceptionMessage = ex.Message;
                    }

                    Assert.True(connection.Opened);
                }
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
        [InlineData(2, "atz", new string[] { "ELM327 v1.5" })]
        [InlineData(100, "atz", new string[] { "ELM327 v1.5" })]
        public async Task Elm327Session_Can_Cancel_Send_Obd2_Command(int numIterations, string message, string[] expectedResponse)
        {
            using (BluetoothElm327Connection connection = (await BluetoothElm327Connection.GetAvailableConnectionsAsync()).FirstOrDefault())
            {
                Elm327Session session = new Elm327Session(connection);
                string lastExceptionMessage = null;
                try
                {
                    await connection.OpenAsync();

                    for (int i = 0; i < numIterations; ++i)
                    {
                        using (CancellationTokenSource cts = new CancellationTokenSource())
                        {
                            Task sendCommand = session.SendCommandAsync(message, cts.Token);
                            cts.Cancel();
                            try
                            {
                                await sendCommand;
                            }
                            catch (OperationCanceledException)
                            {
                                // Expected.
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Assert.NotEqual(lastExceptionMessage, ex.Message);
                    lastExceptionMessage = ex.Message;
                }
            }
        }

        /// <summary>
        /// Verifies the ELM 327 session can run a pid against the OBD2 adapter.
        /// </summary>
        /// <param name="numIterations">The number iterations.</param>
        /// <param name="messages">The messages.</param>
        /// <param name="pid">The pid.</param>
        /// <returns></returns>
        [Theory]
        [InlineData(10, new string[] { "atz", "ate0", "atsh 7e0", "atat2" }, "01010304")]
        [InlineData(10, new string[] { "atz", "ate0", "atsh 7e0", "atat2" }, "0104")]
        [InlineData(100, new string[] { "atz", "ate0", "atsh 7e0", "atat2" }, "0103")]
        public async Task Elm327Session_Can_Cancel_Run_Pid_Against_Obd2_Adapter(int numIterations, string[] messages, string pid)
        {
            // Designed to work against the ScanTool.net ECUSIM 2000 simulator: https://www.scantool.net/scantool/downloads/101/ecusim_2000-ug.pdf
            using (BluetoothElm327Connection connection = (await BluetoothElm327Connection.GetAvailableConnectionsAsync()).FirstOrDefault())
            {
                Elm327Session session = new Elm327Session(connection);
                string lastExceptionMessage = null;
                try
                {
                    await connection.OpenAsync();

                    if (messages != null)
                    {
                        foreach (string message in messages)
                        {
                            await session.SendCommandAsync(message, CancellationToken.None);
                        }
                    }

                    while (!(await session.SendCommandAsync("atsp0", CancellationToken.None)).Contains("OK")) ;

                    for (int i = 0; i < numIterations; ++i)
                    {
                        using (CancellationTokenSource cts = new CancellationTokenSource())
                        {
                            Task<List<int>> pidResponseTask = session.RunPidAsync(pid, CancellationToken.None);
                            cts.Cancel();
                            try
                            {
                                await pidResponseTask;
                            }
                            catch (OperationCanceledException)
                            {
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Assert.NotEqual(lastExceptionMessage, ex.Message);
                    lastExceptionMessage = ex.Message;
                }
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
        [InlineData(1, "atz", new string[] { "ELM327 v1.5" })]
        [InlineData(5, "atz", new string[] { "ELM327 v1.5" })]
        [InlineData(10, "atz", new string[] { "ELM327 v1.5" })]
        public async Task Elm327Session_Can_Evaluate_Obd2_Adapter_Message(int numIterations, string message, string[] expectedResponse)
        {
            using (BluetoothElm327Connection connection = (await BluetoothElm327Connection.GetAvailableConnectionsAsync()).FirstOrDefault())
            {
                Elm327Session session = new Elm327Session(connection);
                string lastExceptionMessage = null;
                try
                {
                    await connection.OpenAsync();

                    for (int i = 0; i < numIterations; ++i)
                    {
                        string[] actualResponse = await session.SendCommandAsync(message, CancellationToken.None);
                        int toSkip = actualResponse.Length - expectedResponse.Length;
                        Assert.True(expectedResponse.SequenceEqual(actualResponse.Skip(toSkip)), "Response mismatches expected.");
                    }
                }
                catch (Exception ex)
                {
                    Assert.NotEqual(lastExceptionMessage, ex.Message);
                    lastExceptionMessage = ex.Message;
                }
            }
        }

        /// <summary>
        /// Verifies the ELM 327 session can run a pid against the OBD2 adapter.
        /// </summary>
        /// <param name="numIterations">The number iterations.</param>
        /// <param name="messages">The messages.</param>
        /// <param name="pid">The pid.</param>
        /// <returns></returns>
        [Theory]
        [InlineData(1, new string[] { "atz", "ate0", "atsh 7e0", "atat2" }, "0101", new int[] { 0x41, 0x01, 0x00, 0x07, 0xef, 0x80 })]
        [InlineData(1, new string[] { "atz", "ate0", "atsh 7e0", "atat2" }, "0103", new int[] { 0x41, 0x03, 0x02, 0x01 })]
        [InlineData(1, new string[] { "atz", "ate0", "atsh 7e0", "atat2" }, "0104", new int[] { 0x41, 0x04, 0x32 })]
        [InlineData(1, new string[] { "atz", "ate0", "atsh 7e0", "atat2" }, "01010304", new int[] { 0x41, 0x01, 0x00, 0x07, 0xef, 0x80, 0x03, 0x02, 0x01, 0x04, 0x32 })]
        [InlineData(10, new string[] { "atz", "ate0", "atsh 7e0", "atat2" }, "0101", new int[] { 0x41, 0x01, 0x00, 0x07, 0xef, 0x80 })]
        [InlineData(10, new string[] { "atz", "ate0", "atsh 7e0", "atat2" }, "0103", new int[] { 0x41, 0x03, 0x02, 0x01 })]
        [InlineData(10, new string[] { "atz", "ate0", "atsh 7e0", "atat2" }, "0104", new int[] { 0x41, 0x04, 0x32 })]
        [InlineData(10, new string[] { "atz", "ate0", "atsh 7e0", "atat2" }, "01010304", new int[] { 0x41, 0x01, 0x00, 0x07, 0xef, 0x80, 0x03, 0x02, 0x01, 0x04, 0x32 })]
        [InlineData(100, new string[] { "atz", "ate0", "atsh 7e0", "atat2" }, "0101", new int[] { 0x41, 0x01, 0x00, 0x07, 0xef, 0x80 })]
        [InlineData(100, new string[] { "atz", "ate0", "atsh 7e0", "atat2" }, "0103", new int[] { 0x41, 0x03, 0x02, 0x01 })]
        [InlineData(100, new string[] { "atz", "ate0", "atsh 7e0", "atat2" }, "0104", new int[] { 0x41, 0x04, 0x32 })]
        [InlineData(100, new string[] { "atz", "ate0", "atsh 7e0", "atat2" }, "01010304", new int[] { 0x41, 0x01, 0x00, 0x07, 0xef, 0x80, 0x03, 0x02, 0x01, 0x04, 0x32 })]
        public async Task Elm327Session_Can_Run_Pid_Against_Obd2_Adapter(int numIterations, string[] messages, string pid, int[] expectedPidResponse)
        {
            // Designed to work against the ScanTool.net ECUSIM 2000 simulator: https://www.scantool.net/scantool/downloads/101/ecusim_2000-ug.pdf
            using (BluetoothElm327Connection connection = (await BluetoothElm327Connection.GetAvailableConnectionsAsync()).FirstOrDefault())
            {
                Elm327Session session = new Elm327Session(connection);
                string lastExceptionMessage = null;
                try
                {
                    await connection.OpenAsync();

                    if (messages != null)
                    {
                        foreach (string message in messages)
                        {
                            await session.SendCommandAsync(message, CancellationToken.None);
                        }
                    }

                    while (!(await session.SendCommandAsync("atsp0", CancellationToken.None)).Contains("OK")) ;

                    for (int i = 0; i < numIterations; ++i)
                    {
                        List<int> pidResponse = await session.RunPidAsync(pid, CancellationToken.None);
                        Assert.NotNull(pidResponse);
                        Assert.True(expectedPidResponse.SequenceEqual(pidResponse), "PID response does not match expected.");
                    }
                }
                catch (Exception ex)
                {
                    Assert.NotEqual(lastExceptionMessage, ex.Message);
                    lastExceptionMessage = ex.Message;
                }
            }
        }

        /// <summary>
        /// Verifies the ELM327 driver can construct without test mode.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Elm327_Driver_Can_Construct_With_Test_Mode_Setting(bool testMode)
        {
            // Designed to work against the ScanTool.net ECUSIM 2000 simulator: https://www.scantool.net/scantool/downloads/101/ecusim_2000-ug.pdf
            using (BluetoothElm327Connection connection = (await BluetoothElm327Connection.GetAvailableConnectionsAsync()).FirstOrDefault())
            {
                Elm327Driver driver = new Elm327Driver(new DisplayConfiguration(), connection, testMode);
                string lastExceptionMessage = null;
                try
                {
                    await driver.OpenAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Assert.NotEqual(lastExceptionMessage, ex.Message);
                    lastExceptionMessage = ex.Message;
                }
            }
        }

        /// <summary>
        /// Verifies the ELM 327 driver can execute commands and get PID results against the OBD2 adapter.
        /// </summary>
        /// <param name="numIterations">The number iterations.</param>
        /// <param name="testCommand">The test command.</param>
        /// <returns></returns>
        [Theory]
        [InlineData(1, PidRequest.Mode1Test1)]
        [InlineData(1, PidRequest.Mode1Test2)]
        [InlineData(1, PidRequest.Mode1Test3)]
        [InlineData(10, PidRequest.Mode1Test1)]
        [InlineData(100, PidRequest.Mode1Test2)]
        [InlineData(1, PidRequest.Mode1Test1 | PidRequest.Mode1Test2)]
        [InlineData(10, PidRequest.Mode1Test1 | PidRequest.Mode1Test2)]
        [InlineData(1, PidRequest.Mode1Test1 | PidRequest.Mode1Test2 | PidRequest.Mode1Test3)]
        [InlineData(1, PidRequest.Mode1Test2 | PidRequest.Mode1Test3)]
        [InlineData(1, PidRequest.Mode1Test1 | PidRequest.Mode1Test3)]
        [InlineData(100, PidRequest.Mode1Test1 | PidRequest.Mode1Test3)]
        [InlineData(1, PidRequest.Mode9Test1)]
        [InlineData(1, PidRequest.Mode9Test2)]
        [InlineData(1, PidRequest.Mode1Test1 | PidRequest.Mode9Test1)]
        [InlineData(1, PidRequest.Mode1Test2 | PidRequest.Mode9Test2)]
        [InlineData(1, PidRequest.Mode1Test1 | PidRequest.Mode1Test2 | PidRequest.Mode9Test2)]
        [InlineData(1, PidRequest.Mode1Test1 | PidRequest.Mode1Test2 | PidRequest.Mode1Test3 | PidRequest.Mode9Test1)]
        [InlineData(10, PidRequest.Mode1Test1 | PidRequest.Mode1Test2 | PidRequest.Mode1Test3 | PidRequest.Mode9Test1)]
        [InlineData(100, PidRequest.Mode1Test1 | PidRequest.Mode1Test2 | PidRequest.Mode1Test3 | PidRequest.Mode9Test1)]
        public async Task Elm327Driver_Can_Get_Pid_Results_Against_Obd2_Adapter(int numIterations, PidRequest testCommand)
        {
            // Designed to work against the ScanTool.net ECUSIM 2000 simulator: https://www.scantool.net/scantool/downloads/101/ecusim_2000-ug.pdf
            using (BluetoothElm327Connection connection = (await BluetoothElm327Connection.GetAvailableConnectionsAsync()).FirstOrDefault())
            {
                Elm327Driver driver = new Elm327Driver(new DisplayConfiguration(), connection, true);
                string lastExceptionMessage = null;
                try
                {
                    await driver.OpenAsync(CancellationToken.None);
                    for (int i = 0; i < numIterations; ++i)
                    {
                        PidResult result = await driver.GetPidResultAsync(testCommand, CancellationToken.None);
                        Assert.NotNull(result);
                        if (testCommand.HasFlag(PidRequest.Mode1Test1))
                        {
                            Assert.True(result.Mode1Test1Passed);
                        }

                        if (testCommand.HasFlag(PidRequest.Mode1Test2))
                        {
                            Assert.True(result.Mode1Test2Passed);
                        }

                        if (testCommand.HasFlag(PidRequest.Mode1Test3))
                        {
                            Assert.True(result.Mode1Test3Passed);
                        }

                        if (testCommand.HasFlag(PidRequest.Mode9Test1))
                        {
                            Assert.True(result.Mode9Test1Passed);
                        }

                        if (testCommand.HasFlag(PidRequest.Mode9Test2))
                        {
                            Assert.True(result.Mode9Test2Passed);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Assert.NotEqual(lastExceptionMessage, ex.Message);
                    lastExceptionMessage = ex.Message;
                }
            }
        }
    }
}

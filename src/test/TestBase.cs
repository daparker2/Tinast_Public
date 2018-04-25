
namespace DP.Tinast.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MetroLog;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Represent a base test object.
    /// </summary>
    /// <typeparam name="TTestClass">The type of test class being implemented.</typeparam>
    public abstract class TestBase<TTestClass>
    {
        /// <summary>
        /// The output helper
        /// </summary>
        private ITestOutputHelper outputHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestBase"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        /// <remarks>Use the <see cref="CreateLogger{TTest}(LoggingConfiguration)"/> method to access a suitable logging context for the test. Don't use <see cref="outputHelper"/> directly.</remarks>
        public TestBase(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

        /// <summary>
        /// Creates the logger.
        /// </summary>
        /// <param name="loggingConfig">The logging configuration.</param>
        /// <returns></returns>
        protected ILogger CreateLogger(LoggingConfiguration loggingConfig = null)
        {
            return LogManagerFactory.DefaultLogManager.GetLogger<TTestClass>(loggingConfig);
        }
    }
}

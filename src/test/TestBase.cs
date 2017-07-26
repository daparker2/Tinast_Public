
namespace DP.Tinast.Tests
{
    using System;
    using System.Collections.Generic;
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

        /// <summary>
        /// Represent a logging adapter.
        /// </summary>
        /// <seealso cref="MetroLog.ILogger" />
        class LoggerAdapter : ILogger
        {
            /// <summary>
            /// The base logger
            /// </summary>
            ILogger baseLogger;

            /// <summary>
            /// The output helper
            /// </summary>
            ITestOutputHelper outputHelper;

            /// <summary>
            /// Initializes a new instance of the <see cref="LoggerAdapter"/> class.
            /// </summary>
            /// <param name="baseLogger">The base logger.</param>
            /// <param name="outputHelper">The output helper.</param>
            public LoggerAdapter(ILogger baseLogger, ITestOutputHelper outputHelper)
            {
                this.baseLogger = baseLogger;
                this.outputHelper = outputHelper;
            }

            /// <summary>
            /// Gets a value indicating whether this instance is debug enabled.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is debug enabled; otherwise, <c>false</c>.
            /// </value>
            public bool IsDebugEnabled
            {
                get
                {
                    return this.baseLogger.IsDebugEnabled;
                }
            }

            /// <summary>
            /// Gets a value indicating whether this instance is error enabled.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is error enabled; otherwise, <c>false</c>.
            /// </value>
            public bool IsErrorEnabled
            {
                get
                {
                    return this.baseLogger.IsErrorEnabled;
                }
            }

            /// <summary>
            /// Gets a value indicating whether this instance is fatal enabled.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is fatal enabled; otherwise, <c>false</c>.
            /// </value>
            public bool IsFatalEnabled
            {
                get
                {
                    return this.baseLogger.IsFatalEnabled;
                }
            }

            /// <summary>
            /// Gets a value indicating whether this instance is information enabled.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is information enabled; otherwise, <c>false</c>.
            /// </value>
            public bool IsInfoEnabled
            {
                get
                {
                    return this.baseLogger.IsInfoEnabled;
                }
            }

            /// <summary>
            /// Gets a value indicating whether this instance is trace enabled.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is trace enabled; otherwise, <c>false</c>.
            /// </value>
            public bool IsTraceEnabled
            {
                get
                {
                    return this.baseLogger.IsTraceEnabled;
                }
            }

            /// <summary>
            /// Gets a value indicating whether this instance is warn enabled.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is warn enabled; otherwise, <c>false</c>.
            /// </value>
            public bool IsWarnEnabled
            {
                get
                {
                    return this.baseLogger.IsWarnEnabled;
                }
            }

            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name
            {
                get
                {
                    return this.baseLogger.Name;
                }
            }

            /// <summary>
            /// Debugs the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="ps">The ps.</param>
            public void Debug(string message, params object[] ps)
            {
                this.LogXunit(LogLevel.Debug, message, ps);
                this.baseLogger.Debug(message, ps);
            }

            /// <summary>
            /// Debugs the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="ex">The ex.</param>
            public void Debug(string message, Exception ex = null)
            {
                this.LogXunit(LogLevel.Debug, message, ex);
                this.baseLogger.Debug(message, ex);
            }

            /// <summary>
            /// Errors the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="ps">The ps.</param>
            public void Error(string message, params object[] ps)
            {
                this.LogXunit(LogLevel.Error, message, ps);
                this.baseLogger.Error(message, ps);
            }

            /// <summary>
            /// Errors the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="ex">The ex.</param>
            public void Error(string message, Exception ex = null)
            {
                this.LogXunit(LogLevel.Error, message, ex);
                this.baseLogger.Error(message, ex);
            }

            /// <summary>
            /// Fatals the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="ps">The ps.</param>
            public void Fatal(string message, params object[] ps)
            {
                this.LogXunit(LogLevel.Fatal, message, ps);
                this.baseLogger.Fatal(message, ps);
            }

            /// <summary>
            /// Fatals the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="ex">The ex.</param>
            public void Fatal(string message, Exception ex = null)
            {
                this.LogXunit(LogLevel.Fatal, message, ex);
                this.baseLogger.Fatal(message, ex);
            }

            /// <summary>
            /// Informations the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="ps">The ps.</param>
            public void Info(string message, params object[] ps)
            {
                this.LogXunit(LogLevel.Info, message, ps);
                this.baseLogger.Info(message, ps);
            }

            /// <summary>
            /// Informations the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="ex">The ex.</param>
            public void Info(string message, Exception ex = null)
            {
                this.LogXunit(LogLevel.Info, message, ex);
                this.baseLogger.Info(message, ex);
            }

            /// <summary>
            /// Determines whether the specified level is enabled.
            /// </summary>
            /// <param name="level">The level.</param>
            /// <returns>
            ///   <c>true</c> if the specified level is enabled; otherwise, <c>false</c>.
            /// </returns>
            public bool IsEnabled(LogLevel level)
            {
                return this.baseLogger.IsEnabled(level);
            }

            /// <summary>
            /// Logs the specified log level.
            /// </summary>
            /// <param name="logLevel">The log level.</param>
            /// <param name="message">The message.</param>
            /// <param name="ps">The ps.</param>
            public void Log(LogLevel logLevel, string message, params object[] ps)
            {
                this.LogXunit(logLevel, message, ps);
                this.baseLogger.Log(logLevel, message, ps);
            }

            /// <summary>
            /// Logs the specified log level.
            /// </summary>
            /// <param name="logLevel">The log level.</param>
            /// <param name="message">The message.</param>
            /// <param name="ex">The ex.</param>
            public void Log(LogLevel logLevel, string message, Exception ex)
            {
                this.LogXunit(logLevel, message, ex);
                this.baseLogger.Log(logLevel, message, ex);
            }

            /// <summary>
            /// Traces the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="ps">The ps.</param>
            public void Trace(string message, params object[] ps)
            {
                this.LogXunit(LogLevel.Trace, message, ps);
                this.baseLogger.Trace(message, ps);
            }

            /// <summary>
            /// Traces the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="ex">The ex.</param>
            public void Trace(string message, Exception ex = null)
            {
                this.LogXunit(LogLevel.Trace, message, ex);
                this.baseLogger.Trace(message, ex);
            }

            /// <summary>
            /// Warns the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="ps">The ps.</param>
            public void Warn(string message, params object[] ps)
            {
                this.LogXunit(LogLevel.Warn, message, ps);
                this.baseLogger.Warn(message, ps);
            }

            /// <summary>
            /// Warns the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="ex">The ex.</param>
            public void Warn(string message, Exception ex = null)
            {
                this.LogXunit(LogLevel.Warn, message, ex);
                this.baseLogger.Warn(message, ex);
            }

            /// <summary>
            /// Logs to the Xunit adapter.
            /// </summary>
            /// <param name="logLevel">The log level.</param>
            /// <param name="message">The message.</param>
            /// <param name="ex">The exception</param>
            private void LogXunit(LogLevel logLevel, string message, Exception ex = null)
            {
                if (this.IsEnabled(logLevel))
                {
                    this.outputHelper.WriteLine(string.Format("Test '{0}' {1} message: {2} {3}", this.Name, logLevel.ToString().ToLower(), message, ex != null ? string.Format(", exception={0}", ex) : string.Empty));
                }
            }

            /// <summary>
            /// Logs to the Xunit adapter.
            /// </summary>
            /// <param name="logLevel">The log level.</param>
            /// <param name="message">The message.</param>
            private void LogXunit(LogLevel logLevel, string fmt, params object[] ps)
            {
                if (this.IsEnabled(logLevel))
                {
                    this.LogXunit(logLevel, string.Format(fmt, ps));
                }
            }
        }
    }
}

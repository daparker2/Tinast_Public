
namespace DP.Tinast
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Foundation.Diagnostics;
    using MetroLog;
    using MetroLog.Targets;
    using MetroLog.Layouts;

    /// <summary>
    /// Represent a logging target for the <see cref="LoggingChannel"/> class.
    /// </summary>
    class LoggingChannelTarget : AsyncTarget
    {
        /// <summary>
        /// The channel
        /// </summary>
        private LoggingChannel channel;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingChannelTarget"/> class.
        /// </summary>
        /// <param name="layout">The layout.</param>
        public LoggingChannelTarget(LoggingChannel channel) 
            : base(new SingleLineLayout())
        {
            this.channel = channel;
        }

        /// <summary>
        /// Writes the asynchronous core.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected override Task<LogWriteOperation> WriteAsyncCore(LogWriteContext context, LogEventInfo entry)
        {

        }
    }
}

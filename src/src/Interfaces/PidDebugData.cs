
namespace DP.Tinast.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Represent data about the last pid transaction
    /// </summary>
    public class PidDebugData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PidDebugData"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="response">The response.</param>
        /// <param name="latency">The latency.</param>
        public PidDebugData(string command, string[] response, TimeSpan latency)
        {
            this.Command = command;
            this.Response = response;
            this.Latency = latency;
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public string Command
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        public string[] Response
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the latency.
        /// </summary>
        /// <value>
        /// The latency.
        /// </value>
        public TimeSpan Latency
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Command: '{0}' took {1}ms.\nResponse: {2}", this.Command, this.Latency.TotalMilliseconds, string.Join("\n", this.Response));
        }
    }
}

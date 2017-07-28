
namespace DP.Tinast.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A connection failed exception.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class ConnectFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectFailedException"/> class.
        /// </summary>
        public ConnectFailedException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectFailedException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ConnectFailedException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectFailedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public ConnectFailedException(string message, Exception inner) : base(message, inner) { }
    }
}

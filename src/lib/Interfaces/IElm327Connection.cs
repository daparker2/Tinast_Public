
namespace DP.Tinast.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Storage.Streams;

    /// <summary>
    /// Represent an interface to an ELM 327 connection.
    /// </summary>
    public interface IElm327Connection
    {
        /// <summary>
        /// Gets the input stream.
        /// </summary>
        /// <value>
        /// The input stream.
        /// </value>
        IInputStream InputStream { get; }

        /// <summary>
        /// Gets the output stream.
        /// </summary>
        /// <value>
        /// The output stream.
        /// </value>
        IOutputStream OutputStream { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IElm327Connection"/> is opened.
        /// </summary>
        /// <value>
        ///   <c>true</c> if opened; otherwise, <c>false</c>.
        /// </value>
        bool Opened { get; }

        /// <summary>
        /// Opens the connection asynchronously.
        /// </summary>
        /// <returns></returns>
        Task OpenAsync();

        /// <summary>
        /// Closes this instance.
        /// </summary>
        void Close();
    }
}

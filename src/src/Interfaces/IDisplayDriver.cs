
namespace DP.Tinast.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Represent an ELM 327 compatible driver interface.
    /// </summary>
    public interface IDisplayDriver
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IDisplayDriver"/> is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        bool Connected { get; }

        /// <summary>
        /// Tries connecting to the OBD2 ELM327 interface.
        /// </summary>
        /// <returns>True if the connection was established.</returns>
       Task<bool> TryConnectAsync();

        /// <summary>
        /// Disconnects from the OBD2 ELM327 interface.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Gets the pid result.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        Task<PidResult> GetPidResultAsync(PidRequest request);

        /// <summary>
        /// Gets the last transaction information, which in most cases will be the command sent to GetPidResultAsync.
        /// </summary>
        /// <returns>A <see cref="Task{PidDebugData}"/> object representing the last transaction.</returns>
        /// <remarks>This may block for a while, if no transactions are being performed due to a pending connect.</remarks>
        Task<PidDebugData> GetLastTransactionInfo();
    }
}

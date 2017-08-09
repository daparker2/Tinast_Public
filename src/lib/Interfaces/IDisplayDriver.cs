
namespace DP.Tinast.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represent an ELM 327 compatible driver interface.
    /// </summary>
    public interface IDisplayDriver
    {
        /// <summary>
        /// Connects to the OBD2 ELM327 interface.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>True if the connection was established.</returns>
        /// <exception cref="ConnectFailedException">Occurs if the connection fails.</exception>
        Task OpenAsync();

        /// <summary>
        /// Gets the pid result.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="token">The token.</param>
        /// <returns>The result of the PID request.</returns>
        /// <exception cref="ConnectFailedException">Occurs if the connection fails.</exception>
        Task<PidResult> GetPidResultAsync(PidRequest request);

        /// <summary>
        /// Gets the last transaction information, which in most cases will be the command sent to GetPidResultAsync.
        /// </summary>
        /// <returns>A <see cref="Task{PidDebugData}"/> object representing the last transaction.</returns>
        /// <remarks>This may block for a while, if no transactions are being performed due to a pending connect.</remarks>
        PidDebugData GetLastTransactionInfo();
    }
}

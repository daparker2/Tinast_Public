
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
    interface IDisplayDriver
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IDisplayDriver"/> is resumed and can execute commands.
        /// </summary>
        /// <value>
        ///   <c>true</c> if resumed; otherwise, <c>false</c>.
        /// </value>
        bool Resumed { get; }

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
        Task<bool> TryConnect();

        /// <summary>
        /// Gets the pid result.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        Task<PidResult> GetPidResult(PidRequest request);
    }
}

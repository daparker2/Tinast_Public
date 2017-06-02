
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
        /// Gets the afr %.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        Task<double> GetAfr();

        /// <summary>
        /// Gets the boost in psi.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        Task<int> GetBoost();

        /// <summary>
        /// Gets the load in %.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        Task<int> GetLoad();

        /// <summary>
        /// Gets the oil temp in F.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        Task<int> GetOilTemp();

        /// <summary>
        /// Gets the coolant temp in F.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        Task<int> GetCoolantTemp();

        /// <summary>
        /// Gets the intake temp in F.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        Task<int> GetIntakeTemp();
    }
}

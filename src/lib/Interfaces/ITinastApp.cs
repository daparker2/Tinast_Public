namespace DP.Tinast.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Config;

    /// <summary>
    /// Base class for applications wishing to implement control functionality.
    /// </summary>
    public interface ITinastApp
    {
        /// <summary>
        /// Occurs when faulted.
        /// </summary>
        event EventHandler Faulted;

        /// <summary>
        /// Occurs when the short tick for updating gauges occurs.
        /// </summary>
        event EventHandler GaugeTick;

        /// <summary>
        /// Occurs when the long tick for blinking indicators occurs.
        /// </summary>
        event EventHandler IndicatorTick;

        /// <summary>
        /// Gets the configuration asynchronously.
        /// </summary>
        /// <returns>An <see cref="DisplayConfiguration"/> object.</returns>
        Task<DisplayConfiguration> GetConfigAsync();

        /// <summary>
        /// Gets the driver asynchronously.
        /// </summary>
        /// <returns>A <see cref="IDisplayDriver"/> object.</returns>
        Task<IDisplayDriver> GetDriverAsync();
    }
}

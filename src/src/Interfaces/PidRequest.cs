namespace DP.Tinast.Interfaces
{
    using System;

    /// <summary>
    /// Represent a PID request
    /// </summary>
    [Flags]
    enum PidRequest
    {
        /// <summary>
        /// The boost
        /// </summary>
        Boost = 0x0,

        /// <summary>
        /// The afr
        /// </summary>
        Afr = 0x1,

        /// <summary>
        /// The load
        /// </summary>
        Load = 0x2,

        /// <summary>
        /// The intake temp
        /// </summary>
        IntakeTemp = 0x4,

        /// <summary>
        /// The coolant temp
        /// </summary>
        CoolantTemp = 0x8,

        /// <summary>
        /// The oil temp
        /// </summary>
        OilTemp = 0x10
    }
}

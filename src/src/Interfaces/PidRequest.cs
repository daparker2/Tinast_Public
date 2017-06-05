namespace DP.Tinast.Interfaces
{
    using System;

    /// <summary>
    /// Represent a PID request
    /// </summary>
    [Flags]
    public enum PidRequest
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0x0, 

        /// <summary>
        /// The boost
        /// </summary>
        Boost = 0x1,

        /// <summary>
        /// The afr
        /// </summary>
        Afr = 0x2,

        /// <summary>
        /// The load
        /// </summary>
        Load = 0x4,

        /// <summary>
        /// The intake temp
        /// </summary>
        IntakeTemp = 0x8,

        /// <summary>
        /// The coolant temp
        /// </summary>
        CoolantTemp = 0x10,

        /// <summary>
        /// The oil temp
        /// </summary>
        OilTemp = 0x20
    }
}

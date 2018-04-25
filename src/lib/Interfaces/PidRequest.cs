namespace DP.Tinast.Interfaces
{
    using System;

    /// <summary>
    /// Represent a PID request
    /// </summary>
    [Flags]
    public enum PidRequests
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
        OilTemp = 0x20,

        /// <summary>
        /// The scantool mode 1 test 1.
        /// </summary>
        Mode1Test1 = 0x1000,

        /// <summary>
        /// The scantool mode 1 test 2.
        /// </summary>
        Mode1Test2 = 0x2000,

        /// <summary>
        /// The scantool mode 1 test 3.
        /// </summary>
        Mode1Test3 = 0x4000,

        /// <summary>
        /// The scantool mode 9 test 1.
        /// </summary>
        Mode9Test1 = 0x8000,

        /// <summary>
        /// The scantool mode 9 test 2.
        /// </summary>
        Mode9Test2 = 0x10000
    }
}

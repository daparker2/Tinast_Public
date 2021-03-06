﻿
namespace DP.Tinast.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Represent a PID result
    /// </summary>
    public class PidResult
    {
        /// <summary>
        /// Gets or sets the afr.
        /// </summary>
        /// <value>
        /// The afr.
        /// </value>
        public double Afr { get; set; }

        /// <summary>
        /// Gets or sets the boost.
        /// </summary>
        /// <value>
        /// The boost.
        /// </value>
        public double Boost { get; set; }

        /// <summary>
        /// Gets or sets the load.
        /// </summary>
        /// <value>
        /// The load.
        /// </value>
        public int Load { get; set; }

        /// <summary>
        /// Gets or sets the oil temp
        /// </summary>
        /// <value>
        /// The load.
        /// </value>
        public int OilTemp { get; set; }

        /// <summary>
        /// Gets or sets the coolant temp
        /// </summary>
        /// <value>
        /// The load.
        /// </value>
        public int CoolantTemp { get; set; }

        /// <summary>
        /// Gets or sets the intake temp
        /// </summary>
        /// <value>
        /// The load.
        /// </value>
        public int IntakeTemp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the mode1 test passed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the mode1 test passed. otherwise, <c>false</c>.
        /// </value>
        public bool Mode1Test1Passed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the mode test passed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the mode test passed. otherwise, <c>false</c>.
        /// </value>
        public bool Mode1Test2Passed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the mode test passed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the mode test passed. otherwise, <c>false</c>.
        /// </value>
        public bool Mode1Test3Passed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the mode test passed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the mode test passed. otherwise, <c>false</c>.
        /// </value>
        public bool Mode9Test1Passed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the mode test passed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the mode test passed. otherwise, <c>false</c>.
        /// </value>
        public bool Mode9Test2Passed { get; set; }
    }
}

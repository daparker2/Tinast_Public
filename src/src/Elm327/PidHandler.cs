﻿
namespace DP.Tinast.Elm327
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Interfaces;

    /// <summary>
    /// Represent a PID handler
    /// </summary>
    class PidHandler
    {
        /// <summary>
        /// The action
        /// </summary>
        private Func<int[], Task> action;

        /// <summary>
        /// The number of parameters
        /// </summary>
        private int numParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="PidHandler"/> class.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="request">The request.</param>
        /// <param name="action">The action.</param>
        public PidHandler(int mode, PidRequest request, int numParameters, Func<int[],Task> action)
        {
            this.Mode = mode;
            this.Request = request;
            this.action = action;
            this.numParameters = numParameters;
        }

        /// <summary>
        /// Gets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public int Mode { get; private set; }

        /// <summary>
        /// Gets or sets the request.
        /// </summary>
        /// <value>
        /// The request.
        /// </value>
        public PidRequest Request { get; private set; }

        /// <summary>
        /// Handles the specified pid data.
        /// </summary>
        /// <param name="pidData">The pid data.</param>
        /// <param name="start">The start of pid data.</param>
        /// <returns>The number of PID data bytes consumed.</returns>
        /// >returns>A <see cref="Task{int}"/> object.</returns>
        public async Task<int> Handle(IList<int> pidData, int start)
        {
            if (pidData.Count - start < numParameters)
            {
                throw new IOException(string.Format("Invalid PID data size. Expected {0}; got {1} bytes instead.", numParameters, pidData.Count));
            }

            int[] pidParams = new int[numParameters];
            for (int i = 0; i < numParameters; ++i)
            {
                pidParams[i] = pidData[start + i];
            }

            await this.action(pidParams);
            return this.numParameters;
        }
    }
}

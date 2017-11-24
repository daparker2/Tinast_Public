namespace DP.Tinast.Elm327
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Interfaces;

    /// <summary>
    /// Represent a PID table.
    class PidTable
    {
        /// <summary>
        /// The ip
        /// </summary>
        private Dictionary<int, PidHandler> ip = new Dictionary<int, PidHandler>();

        /// <summary>
        /// The ip
        /// </summary>
        private Dictionary<PidRequest, PidHandler> pp = new Dictionary<PidRequest, PidHandler>();

        /// <summary>
        /// The keys
        /// </summary>
        private List<PidRequest> keys = new List<PidRequest>();

        /// <summary>
        /// Gets the PID handlers for request.
        /// </summary>
        /// <param name="request">The PID request.</param>
        /// <returns></returns>
        public IEnumerable<PidHandler> GetHandlersForRequest(PidRequest request)
        {
            foreach (PidRequest key in this.keys)
            {
                if (request.HasFlag(key))
                {
                    yield return this.GetHandler(key);
                }
            }
        }

        /// <summary>
        /// Adds the specified handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        public void Add(PidHandler handler)
        {
            int cBits = CountBits(handler.Request);
            if (cBits != 1)
            {
                throw new ArgumentException("Invalid PID request: " + handler.Request);
            }

            if (this.ip.ContainsKey(handler.Mode) || this.pp.ContainsKey(handler.Request))
            {
                throw new ArgumentException("PID request: " + handler.Request + " or handler: " + handler.Mode.ToString("X") + " already exists.");
            }

            this.ip[handler.Mode] = handler;
            this.pp[handler.Request] = handler;
            this.keys.Add(handler.Request);
        }

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <param name="pidMode">The pid mode.</param>
        /// <returns>The pid handler for the mode.</returns>
        public PidHandler GetHandler(int pidMode)
        {
            if (this.ip.TryGetValue(pidMode, out PidHandler ret))
            {
                return ret;
            }

            throw new IOException("Invalid pid mode: " + pidMode);
        }

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <param name="pidRequest">The pid request.</param>
        /// <returns>The pid handler for the mode.</returns>
        public PidHandler GetHandler(PidRequest pidRequest)
        {
            if (this.pp.TryGetValue(pidRequest, out PidHandler ret))
            {
                return ret;
            }

            throw new InvalidOperationException("Invalid pid request: " + pidRequest);
        }

        /// <summary>
        /// Counts the bits.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private static int CountBits(PidRequest request)
        {
            int cBits = 0;
            int sh = 32;
            int m = (int)request;
            while (--sh >= 0)
            {
                if ((m & (1 << sh)) >= 1)
                {
                    ++cBits;
                }
            }

            return cBits;
        }
    }
}

namespace DP.Tinast.Elm327
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Config;
    using Elm327;
    using Interfaces;
    using MetroLog;

    class Elm327Driver : IDisplayDriver, IDisposable
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<Elm327Driver>();

        /// <summary>
        /// The disposed
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// The configuration
        /// </summary>
        private DisplayConfiguration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="Elm327Driver"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public Elm327Driver(DisplayConfiguration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDisplayDriver"/> is resumed and can execute commands.
        /// </summary>
        /// <value>
        ///   <c>true</c> if resumed; otherwise, <c>false</c>.
        /// </value>
        public bool Resumed { get; private set; }

        /// <summary>
        /// Tries connecting to the OBD2 ELM327 interface.
        /// </summary>
        /// <param name="timeoutMilliseconds">The timeout in milliseconds.</param>
        /// <returns>True if the connection was established.</returns>
        public bool TryConnect(int timeoutMilliseconds)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the afr %.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public Task<double> GetAfr()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the boost in psi.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public Task<double> GetBoost()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the load in %.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public Task<double> GetLoad()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the oil temp in F.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public Task<double> GetOilTemp()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the coolant temp in F.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public Task<double> GetCoolantTemp()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the intake temp in F.
        /// </summary>
        /// <returns>A <see cref="Task{Double}"/> object.</returns>
        public Task<double> GetIntakeTemp()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Resumes this driver instance.
        /// </summary>
        /// <returns></returns>
        public Task Resume()
        {
            this.Resumed = true;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Suspends this driver instance.
        /// </summary>
        /// <returns></returns>
        public Task Suspend()
        {
            this.Resumed = false;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;

                if (disposing)
                {
                }
            }
        }
    }
}

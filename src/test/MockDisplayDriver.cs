
namespace DP.Tinast.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Interfaces;
    using MetroLog;
    using Xunit;
    using Xunit.Abstractions;

    class MockDisplayDriver : IDisplayDriver
    {
        private PidResult result;

        public bool Connected
        {
            get; set;
        }

        public async Task OpenAsync()
        {
#pragma warning disable CA2007 // Do not directly await a Task
            await Task.Delay(0);
#pragma warning restore CA2007 // Do not directly await a Task
            this.Connected = true;
        }

        public void Close()
        {
            this.Connected = false;
        }

        public PidDebugData GetLastTransactionInfo()
        {
#pragma warning disable CA1825 // Avoid zero-length array allocations.
            return new PidDebugData(string.Empty, new string[] { }, TimeSpan.Zero);
#pragma warning restore CA1825 // Avoid zero-length array allocations.
        }

        public async Task<PidResult> GetPidResultAsync(PidRequests request)
        {
#pragma warning disable CA2007 // Do not directly await a Task
            await Task.Delay(0);
#pragma warning restore CA2007 // Do not directly await a Task
            return this.result;
        }

        public void SetPidResult(PidResult result)
        {
            this.result = result;
        }

        public async Task<bool> TryConnectAsync()
        {
            this.Connected = true;
#pragma warning disable CA2007 // Do not directly await a Task
            await Task.Delay(0);
#pragma warning restore CA2007 // Do not directly await a Task
            return this.Connected;
        }
    }
}


namespace DP.Tinast.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
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

        public void Disconnect()
        {
            this.Connected = false;
        }

        public PidDebugData GetLastTransactionInfo()
        {
            return new PidDebugData(string.Empty, new string[] { }, TimeSpan.Zero);
        }

        public async Task<PidResult> GetPidResultAsync(PidRequest request)
        {
            await Task.Delay(0);
            return this.result;
        }

        public void SetPidResult(PidResult result)
        {
            this.result = result;
        }

        public async Task<bool> TryConnectAsync()
        {
            this.Connected = true;
            await Task.Delay(0);
            return this.Connected;
        }
    }
}

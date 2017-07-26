
namespace DP.Tinast.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MetroLog;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Test app tests.
    /// </summary>
    /// <seealso cref="DP.Tinast.Tests.TestBase" />
    public class TestTests : TestBase<TestTests>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestTests"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        public TestTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        /// <summary>
        /// Asserts that reality exists.
        /// </summary>
        [Fact]
        public void Reality_Exists()
        {
            Assert.True(true);
            Assert.False(false);
            ILogger logger = this.CreateLogger();
            logger.Info("Reality exists.");
        }

        /// <summary>
        /// Asserts that reality really exists.
        /// </summary>
        [Fact]
        public void Reality_Still_Exists()
        {
            Assert.True(true);
            Assert.False(false);
            ILogger logger = this.CreateLogger();
            logger.Info("Reality really exists.");
        }
    }
}

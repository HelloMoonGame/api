using Character.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using Serilog.Core;

namespace Character.IntegrationTests.Configuration
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void Logger_is_set_during_execution()
        {
            // Pre-assert
            Assert.IsNotInstanceOfType(Log.Logger, typeof(Logger));

            // Act
            _ = Program.Main(null);

            // Assert
            Assert.IsInstanceOfType(Log.Logger, typeof(Logger));
            
            // Cleanup
            Program.Stop();
        }
    }
}

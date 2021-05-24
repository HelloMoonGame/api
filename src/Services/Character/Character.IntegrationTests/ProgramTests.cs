using System;
using System.Threading;
using System.Threading.Tasks;
using Character.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Hosting;

namespace Character.IntegrationTests.Configuration
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void Logger_is_set_during_execution()
        {
            // Arrange
            var initialLogger = Log.Logger;
            var cancellationToken = new CancellationTokenSource();

            // Act
            _ = Program.RunWithLogging(() => Task.Delay(TimeSpan.FromMinutes(1), cancellationToken.Token));
            var loggerWhileExecuting = Log.Logger;
            cancellationToken.Cancel();
            var loggerAfterShutdown = Log.Logger;

            // Assert
            Assert.IsNotInstanceOfType(initialLogger, typeof(ReloadableLogger));
            Assert.IsInstanceOfType(loggerWhileExecuting, typeof(ReloadableLogger));
            Assert.IsNotInstanceOfType(loggerAfterShutdown, typeof(ReloadableLogger));
        }
    }
}

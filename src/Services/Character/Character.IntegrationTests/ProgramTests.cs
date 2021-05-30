using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Character.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using Serilog.Extensions.Hosting;

namespace Character.IntegrationTests
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
        
        [TestMethod]
        public async Task Exceptions_stopping_server_are_logged_to_console()
        {
            // Arrange
            var errorMessage = "This message should be in the console";
            await using var consoleText = new StringWriter();
            Console.SetOut(consoleText);
            
            // Act
            await Program.RunWithLogging(() => throw new Exception(errorMessage));
            var consoleOutput = consoleText.ToString();

            // Assert
            Assert.IsTrue(consoleOutput.Contains(errorMessage), $"Console output does not contain '{errorMessage}'");
        }
    }
}

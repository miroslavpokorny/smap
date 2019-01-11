using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.Integration
{
    public class IntegrationBase
    {
        // Tests should have Working directory set to ../bin/Debug/netcoreapp2.1 so we must go back by 3 directories
        protected const string ToProjectDir = "../../../";
        protected const string SmapProgram = "../../../../smap/bin/Release/netcoreapp2.1/smap.dll";
        
        protected static Task<int> RunProcessAsync(string fileName, string arguments)
        {
            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = { FileName = fileName, Arguments = arguments, WorkingDirectory = TestContext.CurrentContext.WorkDirectory, RedirectStandardError = true, RedirectStandardOutput = true},
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
    }
}
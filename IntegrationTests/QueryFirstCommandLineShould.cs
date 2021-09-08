using FluentAssertions;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Xunit;

namespace IntegrationTests
{
    public class QueryFirstCommandLineShould
    {
        [Fact]
        public void RegenerateAll_ShouldRegenerateAllAndBuildAndRun()
        {
            // Regenerate all
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var queryFirstDebugBuild = Path.Combine(assemblyPath, @"../../../../QueryFirst.CommandLine/bin/Debug/net5.0/QueryFirst.exe");
            var projectToRegenerate = Path.Combine(assemblyPath, @"../../../../Net5CmdLineTestTarget/");
            var result = RunProcess(queryFirstDebugBuild, projectToRegenerate);
            result.stdOut.Should().Contain("QueryFirst generated wrapper class for GetCustomers.sql");
            result.stdErr.Should().BeEmpty();

            // Build it
            var buildResult = RunProcess("dotnet", "build " + projectToRegenerate);
            buildResult.stdErr.Should().BeEmpty();
            buildResult.stdOut.Contains("�CHEC").Should().BeFalse();

            // Run it
            var runResult = RunProcess(Path.Combine(assemblyPath, @"../../../../Net5CmdLineTestTarget/bin/Debug/net5.0/Net5CmdLineTestTarget.exe"));
            runResult.stdErr.Should().BeEmpty();
        }
        private (string stdOut, string stdErr) RunProcess(string filename, string args = "")
        {
            var psi = new ProcessStartInfo(filename, args)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            // force english for MSBUILD (not working)
            if (psi.EnvironmentVariables.ContainsKey("VSLANG"))
                psi.EnvironmentVariables["VSLANG"] = "1033";
            else
                psi.EnvironmentVariables.Add("VSLANG", "1033");

            // force english for dotnet
            if (psi.EnvironmentVariables.ContainsKey("DOTNET_CLI_UI_LANGUAGE"))
                psi.EnvironmentVariables["DOTNET_CLI_UI_LANGUAGE"] = "en";
            else
                psi.EnvironmentVariables.Add("DOTNET_CLI_UI_LANGUAGE", "en");
            // necessary when using environment variables
            psi.UseShellExecute = false;
            using var process = Process.Start(psi);
            var stdOut = process.StandardOutput.ReadToEnd();
            var stdErr = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return (stdOut, stdErr);
        }
    }
}

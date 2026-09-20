using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BabisW.Execution
{
    class ExecutionHandler
    {
        private static DateTime LastPipeWarning = DateTime.MinValue;
        private static DateTime InjectionStartedAt = DateTime.MinValue;
        public static bool WrapperResponsive { get; private set; }
        public static bool InjectionInProgress { get; private set; }
        public static bool InjectionTimedOut { get; private set; }

        public static bool Inject()
        {
            if (SelectedAPI.API == "Selected API: WeAreDevs API")
            {
                InjectionInProgress = true;
                InjectionTimedOut = false;
                InjectionStartedAt = DateTime.UtcNow;

                try
                {
                // kill previous wrappers
                try
                {
                    foreach (Process proc in Process.GetProcessesByName("Babis-WWRDWrapper"))
                    {
                        proc.Kill();
                        proc.WaitForExit(5000);
                    }
                }
                catch { }
                try
                {
                    foreach (Process proc in Process.GetProcessesByName("WRDFakeServer"))
                    {
                        proc.Kill();
                        proc.WaitForExit(5000);
                    }
                }
                catch { }

                string wrapperPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Babis-WWRDWrapper.exe");
                if (!File.Exists(wrapperPath))
                {
                    MessageBox.Show($"The Babis-W wrapper is missing: {wrapperPath}", "Babis-W", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                var wrapperProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = wrapperPath,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    UseShellExecute = false
                });
                if (wrapperProcess == null)
                {
                    MessageBox.Show("The Babis-W wrapper could not be started.", "Babis-W", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                Thread.Sleep(500);
                if (wrapperProcess.HasExited)
                {
                    MessageBox.Show(
                        $"The Babis-W wrapper exited during startup (code {wrapperProcess.ExitCode}). Check the wrapper console for details.",
                        "Babis-W",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }
                try
                {
                    var Response = SelectedAPI.NewPipe.SendRequest<InjectionRequest>("Inject", new InjectionRequest{AdditionalData = "blank"}); // blank
                    Console.WriteLine($"injection result: {Response}");
                    if (Response.InjectionSuccessful == true)
                    {
                        return true;
                    }
                    else
                    {
                        MessageBox.Show($"WRD injection failed: {Response.AdditionalData}");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"error during injection: {ex.Message}");
                    return false;
                }
                finally
                {
                    InjectionInProgress = false;
                }
            }
            else
            {
                return false;
            }
        }

        public static void Execute(string script)
        {
            Process[] pname = Process.GetProcessesByName("RobloxPlayerBeta");
            if (pname.Length < 1) // If Roblox is not running
            {
                MessageBox.Show("Please run Roblox first before attempting to inject");
                return;
            }

            if (Execution.SelectedAPI.API == "Selected API: WeAreDevs API")
            {
                Process[] pname1 = Process.GetProcessesByName("Babis-WWRDWrapper");
                if (pname1.Length < 1) 
                {
                    MessageBox.Show("Please begin WRD injection first before attempting to execute a script");
                    return;
                }

                try
                {
                    var Response = SelectedAPI.NewPipe.SendRequest<ExecutionRequest>("Execute", new ExecutionRequest { Script = script });
                    Console.WriteLine($"execution result: {Response}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"The API is not ready to execute this script.\n\n{ex.Message}", "Babis-W", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        public static bool IsInjected()
        {   
            if (Execution.SelectedAPI.API == "Selected API: WeAreDevs API")
            {
                try
                {
                    var Response = SelectedAPI.NewPipe.SendRequest<IsInjectedRequest>("IsInjected", new IsInjectedRequest{ AdditionalData = "blank" }); // blank
                    WrapperResponsive = true;
                    if (!Response.IsInjected &&
                        InjectionStartedAt != DateTime.MinValue &&
                        (DateTime.UtcNow - InjectionStartedAt).TotalSeconds >= 60)
                    {
                        InjectionTimedOut = true;
                    }
                    return Response.IsInjected;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while checking for isinjected res: {ex}");
                    if ((DateTime.UtcNow - LastPipeWarning).TotalSeconds >= 10)
                    {
                        Console.WriteLine("The Babis-W wrapper is not responding. Start injection to reconnect.");
                        LastPipeWarning = DateTime.UtcNow;
                    }
                    WrapperResponsive = false;
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static void Stop()
        {
            if (Execution.SelectedAPI.API == "Selected API: WeAreDevs API")
            {
                try
                {
                    foreach (Process proc in Process.GetProcessesByName("Babis-WWRDWrapper"))
                    {
                        proc.Kill();
                    }
                }
                catch { }
            }    
        }
    }
}

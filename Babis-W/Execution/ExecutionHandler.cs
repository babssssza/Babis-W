using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using QuorumAPI;

namespace BabisW.Execution
{
    class ExecutionHandler
    {
        private static DateTime LastPipeWarning = DateTime.MinValue;
        private static DateTime InjectionStartedAt = DateTime.MinValue;
        public static bool WrapperResponsive { get; private set; }
        public static bool InjectionInProgress { get; private set; }
        public static bool InjectionTimedOut { get; private set; }
        public static bool NativeExecutionStarted { get; private set; }
        public static bool NativeHealthFailed { get; private set; }

        public static bool Inject()
        {
            if (SelectedAPI.API == "Selected API: Quorum API")
            {
                InjectionInProgress = true;
                InjectionTimedOut = false;
                NativeExecutionStarted = false;
                NativeHealthFailed = false;
                InjectionStartedAt = DateTime.UtcNow;
                try
                {
                    QuorumModule.UseAutoUpdate(true);
                    QuorumModule.SetAttachNotify(null);
                    return QuorumModule.AttachAPI();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Babis-W could not complete the injection: {ex.Message}\n\nPlease make sure Roblox is fully loaded and try again.",
                        "Babis-W",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
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

        public static async void Execute(string script)
        {
            Process[] pname = Process.GetProcessesByName("RobloxPlayerBeta");
            if (pname.Length < 1) // If Roblox is not running
            {
                MessageBox.Show("Please run Roblox first before attempting to inject");
                return;
            }

            if (Execution.SelectedAPI.API == "Selected API: Quorum API")
            {
                try
                {
                    if (!await QuorumModule.ExecuteScript(script))
                    {
                        throw new InvalidOperationException("Quorum API rejected the script.");
                    }
                }
                catch (Exception ex)
                {
                    NativeHealthFailed = true;
                    MessageBox.Show($"The API is not ready to execute this script.\n\n{ex.Message}", "Babis-W", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        public static bool IsInjected()
        {   
            if (Execution.SelectedAPI.API == "Selected API: Quorum API")
            {
                try
                {
                    var attached = QuorumModule.IsAttached();
                    WrapperResponsive = true;
                    if (!attached &&
                        InjectionStartedAt != DateTime.MinValue &&
                        (DateTime.UtcNow - InjectionStartedAt).TotalSeconds >= 60)
                    {
                        InjectionTimedOut = true;
                    }
                    return attached;
                }
                catch (Exception ex)
                {
                    NativeHealthFailed = true;
                    Console.WriteLine($"Error while checking for isinjected res: {ex}");
                    NativeExecutionStarted = false;
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
            NativeExecutionStarted = false;
        }
    }
}

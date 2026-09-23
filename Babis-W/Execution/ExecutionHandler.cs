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
        private static DateTime LastHealthWarning = DateTime.MinValue;
        private static DateTime InjectionStartedAt = DateTime.MinValue;
        public static bool WrapperResponsive { get; private set; }
        public static bool InjectionInProgress { get; private set; }
        public static bool InjectionTimedOut { get; private set; }
        public static bool NativeExecutionStarted { get; private set; }
        public static bool NativeHealthFailed { get; private set; }
        private static readonly SemaphoreSlim InjectionGate = new SemaphoreSlim(1, 1);
        private static int CommunicationStarted;

        public static async Task<bool> InjectAsync()
        {
            if (SelectedAPI.API != "Selected API: Quorum API" ||
                !await InjectionGate.WaitAsync(0))
            {
                return false;
            }

            InjectionInProgress = true;
            InjectionTimedOut = false;
            NativeExecutionStarted = false;
            NativeHealthFailed = false;
            WrapperResponsive = false;
            InjectionStartedAt = DateTime.UtcNow;
            try
            {
                var attached = await Task.Run(() =>
                {
                    QuorumModule.UseAutoUpdate(true);
                    return QuorumModule.AttachAPIAsync();
                });

                if (attached)
                {
                    EnsureCommunication();
                }

                return attached;
            }
            catch (Exception ex)
            {
                NativeHealthFailed = true;
                Console.WriteLine($"Quorum injection failed: {ex}");
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
                InjectionGate.Release();
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
                    if (!await QuorumModule.ExecuteScript(script, null))
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
                    EnsureCommunication();
                    var attached = QuorumModule.IsAttached();
                    WrapperResponsive = true;
                    if (attached)
                    {
                        NativeHealthFailed = false;
                        InjectionTimedOut = false;
                    }
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
                    Console.WriteLine($"Error while checking Quorum attachment state: {ex}");
                    NativeExecutionStarted = false;
                    if ((DateTime.UtcNow - LastHealthWarning).TotalSeconds >= 10)
                    {
                        Console.WriteLine("The Quorum API is not responding. Try injection again.");
                        LastHealthWarning = DateTime.UtcNow;
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
            if (Interlocked.Exchange(ref CommunicationStarted, 0) == 1)
            {
                try
                {
                    QuorumModule.StopCommunication();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping Quorum communication: {ex.Message}");
                }
            }
        }

        private static void EnsureCommunication()
        {
            if (Interlocked.CompareExchange(ref CommunicationStarted, 1, 0) == 0)
            {
                try
                {
                    QuorumModule.StartCommunication();
                }
                catch
                {
                    Interlocked.Exchange(ref CommunicationStarted, 0);
                    throw;
                }
            }
        }
    }
}

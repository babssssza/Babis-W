using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace BabisWWRDWrapper
{
    public class WRDAPI
    {
        private static readonly object NativeCallLock = new object();
        private static bool NativeInitialized;

        // BabisW already allocates a console

        
        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();
        

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); // show = 5, hide = 0


        // import exports from dlls
        [DllImport("wearedevs_exploit_api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte initialize();

        [DllImport("wearedevs_exploit_api.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool isAttached();

        [DllImport("wearedevs_exploit_api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void execute([MarshalAs(UnmanagedType.LPStr)] string script);


        private const string NativeLibraryName = "wearedevs_exploit_api.dll";
        private const string WRDLink = "https://wrdcdn.net/r/2/exploit%20api/wearedevs_exploit_api.dll";
        private const string ExpectedSha256 = "567C197658CB3FE2B1D5936B20D0DA5CCA7A5B505A9DA10D4003F5AF8B8D0705";

        public static void InitializeWithRetry()
        {
            EnsureNativeLibrary();
            Exception lastError = null;

            for (var attempt = 1; attempt <= 10; attempt++)
            {
                try
                {
                    lock (NativeCallLock)
                    {
                        if (!NativeInitialized)
                        {
                            initialize();
                            NativeInitialized = true;
                        }
                    }
                    return;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    if (attempt < 10)
                    {
                        Thread.Sleep(2000);
                    }
                }
            }

            throw new InvalidOperationException(
                "The WeAreDevs API could not initialize after 10 attempts. Make sure Roblox is fully loaded.",
                lastError);
        }

        public static Thread WRDInit()
        {
            AllocConsole();
            Thread initthread = new Thread(delegate ()
            {
                try
                {
                    InitializeWithRetry();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Native API initialization failed: {ex.Message}");
                }
                ShowConsole();
            });
            initthread.Start();
            return initthread;

        }

        private static void EnsureNativeLibrary()
        {
            var nativeLibraryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NativeLibraryName);
            if (IsTrustedNativeLibrary(nativeLibraryPath))
            {
                return;
            }

            try
            {
                using (var wc = new WebClient())
                {
                    var temporaryPath = nativeLibraryPath + ".download";
                    wc.DownloadFile(WRDLink, temporaryPath);
                    if (!IsTrustedNativeLibrary(temporaryPath))
                    {
                        File.Delete(temporaryPath);
                        throw new InvalidDataException("The downloaded WeAreDevs API failed integrity validation.");
                    }

                    if (File.Exists(nativeLibraryPath))
                    {
                        File.Delete(nativeLibraryPath);
                    }
                    File.Move(temporaryPath, nativeLibraryPath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("A trusted WeAreDevs API library is not available.", ex);
            }
        }

        private static bool IsTrustedNativeLibrary(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            using (var stream = File.OpenRead(path))
            using (var sha256 = SHA256.Create())
            {
                var hash = BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty);
                return string.Equals(hash, ExpectedSha256, StringComparison.OrdinalIgnoreCase);
            }
        }

        public static void Execute(String Script)
        {
            lock (NativeCallLock)
            {
                execute(Script);
            }
            ShowConsole();
        }

        public static bool IsInjected()
        {
            ShowConsole();
            lock (NativeCallLock)
            {
                return isAttached();
            }
        }

        public static bool WaitUntilAttached(int timeoutMs = 30000)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            var probeDelayMs = 1500;

            // Roblox and the native API need time to finish creating the script context.
            // Probing immediately causes the native API to scan an incomplete tree.
            Thread.Sleep(probeDelayMs);

            while (DateTime.UtcNow < deadline)
            {
                if (IsInjected())
                {
                    return true;
                }

                Thread.Sleep(probeDelayMs);
                probeDelayMs = Math.Min(probeDelayMs + 500, 4000);
            }

            return false;
        }

        private static void ShowConsole()
        {
            try
            {
                var consoleWindow = GetConsoleWindow();
                if (consoleWindow != IntPtr.Zero)
                {
                    ShowWindow(consoleWindow, 5);
                    Console.Title = "Babis-W WeAreDevs Wrapper";
                }
            }
            catch (IOException)
            {
                // The wrapper may be running without a valid console handle.
            }
            catch (InvalidOperationException)
            {
                // Console output is unavailable; execution itself can continue.
            }
        }
    }
}

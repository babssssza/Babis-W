using BabisWWRDWrapper;
using Microsoft.Win32;
using System;
using System.Drawing;

namespace BabisWWRDWrapper
{
    internal class Program
    {
        static string WrapperVersion = "1.1";
        static async Task Main(string[] args)
        {
            Console.Title = "BabisW WeAreDevs Wrapper";

            RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Babis-WWRDWrapper"); // From the settings we saved
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Babis-WWRDWrapper");
            key.SetValue("WrapperVersion", WrapperVersion);
            key.Close();

            var server = new PipeProcess("Babis-WWRDWrapper");
            Console.WriteLine("Starting pipe server, please don't close this window (literally don't)...");
            await server.StartAsync();
        }
    }
}
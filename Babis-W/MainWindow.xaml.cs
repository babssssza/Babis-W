/*
  __  __       _       _____        _     
 |  \/  |     (_)     |  __ \      | |    
 | \  / | __ _ _ _ __ | |  | | __ _| |__  
 | |\/| |/ _` | | '_ \| |  | |/ _` | '_ \ 
 | |  | | (_| | | | | | |__| | (_| | |_) |
 |_|  |_|\__,_|_|_| |_|_____/ \__,_|_.__/ 
                 
 https://github.com/Avaluate/BabisW

 You can join BabisW's Discord server at https://babis-w.org/discord (June 2025)
 or Telegram at https://telegram.me/babis-w (June 2025)
                                          
 BabisW, Main_EX (Avaluate)
 Discord: avaluate
 Telegram: t.me/avaluate

 WeAreDevs API obtained from https://wearedevs.net/d/Exploit%20API
*/

// References
using DiscordRPC;
using DiscordRPC.Logging;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using BabisW.Execution;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml;

namespace BabisW
{

    public partial class MainWindow : Window
    {
        // VARIABLES //
        private const string CurrentVersion = "Babis-W 15.2 SP3";
        private const string ExecutableName = "Babis-W.exe";
        private const string MainExecutableUrl = "https://raw.githubusercontent.com/babssssza/Babis-W/main/Babis-W/bin/Release/Babis-W.exe";

        // The default text editor text
        string DefaultTextEditorText = "--[[\r\nWelcome to BabisW!\r\nMake sure to join BabisW's Discord at babis-w.org/discord\r\nIf you need help, join our Discord!\r\n--]]\r\n-- Paste in your text below this comment.\r\n\r\nprint(\"BabisW Moment\")";

        // Variables relating to injection
        bool InjectionInProgress = false; // When injection is in progress, self explanatory

        // Animation variables
        private bool CloseCompleted = false; // Window fade-in

        // Scripthub stuff
        bool IsScriptHubOpened = false;
        bool IsGameHubOpened = false;

        // Theme
        bool IsDefaultTheme = true; // So it won't apply on startup smh
        bool IsAvalonLoaded = false; // Prevent more errors from occuring

        // Variables for the theming, you'll see it later on
        string CurrentLuaXSHDLocation = ""; // The path for the AvalonEdit syntax highlighting. Will be needed for future theming

        string LeftRGB = ""; // Top left gradient
        string RightRGB = ""; // Bottom right gradient
        string BGImageURL = ""; // Background image URL
        string BGTransparency = ""; // Background image transparency, on a 0 to 1 scale

        // Variables for the AvalonEdit background colours, there's probably a much better way
        static int AvalonEditBGA = 6; // A (Transparency)
        static int AvalonEditBGR = 47; // R
        static int AvalonEditBGG = 47; // G
        static int AvalonEditBGB = 49; // B

        string AvalonEditFont = "Consolas"; // Font to be used for editor, this is for future customisation options

        bool IsInjected = false;

        // Variables for custom icons (to be implemented in the future)
        Array scripts = Array.Empty<ScriptHub.ScriptData>();
        Array gamescripts = Array.Empty<ScriptHub.GameScriptData>();

        // WebClient Creation
        WebClient WebStuff = new WebClient(); // Create a new generally used WebClient

        // Console handle - https://stackoverflow.com/questions/3571627/show-hide-the-console-window-of-a-c-sharp-console-application
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); // show = 5, hide = 0

        // WINDOW INITILISATION //
        public MainWindow()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Console.WriteLine($"  __  __       _       _____        _     \r\n |  \\/  |     (_)     |  __ \\      | |    \r\n | \\  / | __ _ _ _ __ | |  | | __ _| |__  \r\n | |\\/| |/ _` | | '_ \\| |  | |/ _` | '_ \\ \r\n | |  | | (_| | | | | | |__| | (_| | |_) |\r\n |_|  |_|\\__,_|_|_| |_|_____/ \\__,_|_.__/\n\n${CurrentVersion}, by Avaluate (Main_EX) | babis-w.org/discord\n"); // i love babis-w

            InitializeComponent();
            MainWin.WindowStartupLocation = WindowStartupLocation.CenterScreen; // Center BabisW to the middle of the screen
            EnsureDesktopShortcut();
                                                                                // UPDATE SYSTEM //

            CheckForApplicationUpdate();

            // SETUP //


            // All these folders are needed for BabisW to run
            // This can be written in a shorter way, but I'll just leave it like this

            Console.WriteLine("Checking to see if directories exist");

            if (!Directory.Exists("Applications")) // Tools
            {
                Console.WriteLine("Created new directory: Applications");
                Directory.CreateDirectory("Applications");
            }
            if (!Directory.Exists("EditorThemes"))
            {
                Console.WriteLine("Created new directory: EditorThemes");
                Directory.CreateDirectory("EditorThemes");
            }
            if (!Directory.Exists("Scripts"))
            {
                Console.WriteLine("Created new directory: Scripts");
                Directory.CreateDirectory("Scripts");
            }
            if (!Directory.Exists("Themes"))
            {
                Console.WriteLine("Created new directory: Themes");
                Directory.CreateDirectory("Themes");
            }
            if (!Directory.Exists("Workspace"))
            {
                Console.WriteLine("Created new directory: Workspace");
                Directory.CreateDirectory("Workspace");
            }

            // Keep an existing wrapper installation. Missing files are downloaded below.
            Console.WriteLine("Checking to see if the Babis-W wrapper is installed");

            if (!File.Exists("Babis-WWRDWrapper.deps.json"))
            {
                Console.WriteLine("Downloading Babis-WWRDWrapper.deps.json, please wait...");
                WebStuff.DownloadFile("https://github.com/babssssza/Babis-W/raw/main/Babis-W/bin/x64/Debug/Babis-WWRDWrapper.deps.json", "Babis-WWRDWrapper.deps.json");
            }
            if (!File.Exists("Babis-WWRDWrapper.dll"))
            {
                Console.WriteLine("Downloading Babis-WWRDWrapper.dll, please wait...");
                WebStuff.DownloadFile("https://github.com/babssssza/Babis-W/raw/main/Babis-W/bin/x64/Debug/Babis-WWRDWrapper.dll", "Babis-WWRDWrapper.dll");
            }
            if (!File.Exists("Babis-WWRDWrapper.exe"))
            {
                Console.WriteLine("Downloading Babis-WWRDWrapper.exe, please wait...");
                WebStuff.DownloadFile("https://github.com/babssssza/Babis-W/raw/main/Babis-W/bin/x64/Debug/Babis-WWRDWrapper.exe", "Babis-WWRDWrapper.exe");
            }
            if (!File.Exists("Babis-WWRDWrapper.pdb"))
            {
                Console.WriteLine("Downloading Babis-WWRDWrapper.pdb, please wait...");
                WebStuff.DownloadFile("https://github.com/babssssza/Babis-W/raw/main/Babis-W/bin/x64/Debug/Babis-WWRDWrapper.pdb", "Babis-WWRDWrapper.pdb");
            }
            if (!File.Exists("Babis-WWRDWrapper.runtimeconfig.json"))
            {
                Console.WriteLine("Downloading Babis-WWRDWrapper.runtimeconfig.json, please wait...");
                WebStuff.DownloadFile("https://github.com/babssssza/Babis-W/raw/main/Babis-W/bin/x64/Debug/Babis-WWRDWrapper.runtimeconfig.json", "Babis-WWRDWrapper.runtimeconfig.json");
            }
            if (!File.Exists("WRDFakeServer.exe"))
            {
                Console.WriteLine("Downloading WRDFakeServer.exe, please wait (this will take some time)...");
                WebStuff.DownloadFile("https://github.com/babssssza/Babis-W/raw/main/Babis-W/bin/x64/Debug/WRDFakeServer.exe", "WRDFakeServer.exe");
            }
            if (!File.Exists("wearedevs_exploit_api.dll"))
            {
                try
                {
                    Console.WriteLine("Downloading wearedevs_exploit_api.dll, please wait...");
                    WebStuff.DownloadFile("https://wrdcdn.net/r/2/exploit%20api/wearedevs_exploit_api.dll", "wearedevs_exploit_api.dll");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to download original wearedevs_exploit_api.dll: " + ex + ", so now will try GitHub mirror");
                    try
                    {
                        WebStuff.DownloadFile("https://github.com/wcrddn/wcrddn.github.io/raw/main/wearedevs_exploit_api.dll", "wearedevs_exploit_api.dll");
                    }
                    catch (Exception ex1)
                    {
                        Console.WriteLine("Failed to download wearedevs_exploit_api.dll even from mirror: " + ex1 + "\nBabisW DEPENDS on the existence of this file, so BabisW cannot be ran. Please contact BabisW staff for help...");
                    }
                }
            }

            // OpenSSL req for cert signing
            Console.WriteLine("Checking to see if OpenSSL is downloaded...");

            if (!Directory.Exists("OpenSSL"))
            {
                Console.WriteLine("Created new directory: OpenSSL");
                Directory.CreateDirectory("OpenSSL");
            }
            if (!File.Exists("OpenSSL\\msys-2.0.dll"))
            {
                Console.WriteLine("Downloading msys-2.0.dll...");
                WebStuff.DownloadFile("https://raw.githubusercontent.com/babssssza/Babis-W/main/Babis-W/bin/x64/Debug/OpenSSL/msys-2.0.dll", "OpenSSL\\msys-2.0.dll");
            }
            if (!File.Exists("OpenSSL\\msys-crypto-3.dll"))
            {
                Console.WriteLine("Downloading msys-crypto-3.dll...");
                WebStuff.DownloadFile("https://raw.githubusercontent.com/babssssza/Babis-W/main/Babis-W/bin/x64/Debug/OpenSSL/msys-crypto-3.dll", "OpenSSL\\msys-crypto-3.dll");
            }
            if (!File.Exists("OpenSSL\\msys-ssl-3.dll"))
            {
                Console.WriteLine("Downloading msys-ssl-3.dll...");
                WebStuff.DownloadFile("https://raw.githubusercontent.com/babssssza/Babis-W/main/Babis-W/bin/x64/Debug/OpenSSL/msys-ssl-3.dll", "OpenSSL\\msys-ssl-3.dll");
            }
            if (!File.Exists("OpenSSL\\openssl.exe"))
            {
                Console.WriteLine("Downloading openssl.exe...");
                WebStuff.DownloadFile("https://raw.githubusercontent.com/babssssza/Babis-W/main/Babis-W/bin/x64/Debug/OpenSSL/openssl.exe", "OpenSSL\\openssl.exe");
            }

            // Theme checking for Avalon stuff, etc
            Console.WriteLine("Updating Avalon theme definitions (text editor syntax highlighting)");
            if (File.Exists("EditorThemes\\lua_md_default.xshd"))
            {
                File.Delete("EditorThemes\\lua_md_default.xshd"); // We want to update default theme regardless lol
            }
            string penis = WebStuff.DownloadString("https://raw.githubusercontent.com/babssssza/Babis-W/main/Babis-W/bin/x64/Debug/EditorThemes/lua_md_default.xshd");
            File.WriteAllText("EditorThemes\\lua_md_default.xshd", penis);

            CurrentLuaXSHDLocation = "EditorThemes\\lua_md_default.xshd";
            IsAvalonLoaded = true;

            // Finally, load optional script hub data. A missing or unavailable feed
            // must not prevent the main application from opening.
            Console.WriteLine("Loading script hub data...");
            try
            {
                scripts = ScriptHub.BabisWSC.GetSCData().GetAwaiter().GetResult();
                gamescripts = ScriptHub.BabisWGSC.GetGSCData().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                scripts = Array.Empty<ScriptHub.ScriptData>();
                gamescripts = Array.Empty<ScriptHub.GameScriptData>();
                Console.WriteLine($"Script hub data is unavailable: {ex.Message}");
            }


            Console.Title = "Babis-W";
            Console.WriteLine("All done!\n");
        }

        private void CheckForApplicationUpdate()
        {
            try
            {
                Console.WriteLine("Checking to see if Babis-W is up to date");
                string onlineVersion = WebStuff
                    .DownloadString("https://raw.githubusercontent.com/babssssza/Babis-W/main/UpdateStuff/Version")
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(onlineVersion) ||
                    string.Equals(CurrentVersion, onlineVersion.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                string installDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string executablePath = Path.Combine(installDirectory, ExecutableName);
                string pendingPath = executablePath + ".update";
                string scriptPath = Path.Combine(Path.GetTempPath(), "Babis-W-update-" + Guid.NewGuid().ToString("N") + ".cmd");

                Console.WriteLine("Babis-W update found, downloading...");
                WebStuff.DownloadFile(MainExecutableUrl, pendingPath);

                string escapedDirectory = installDirectory.TrimEnd('\\').Replace("%", "%%");
                string escapedExecutable = executablePath.Replace("%", "%%");
                string escapedPending = pendingPath.Replace("%", "%%");
                string escapedScript = scriptPath.Replace("%", "%%");
                File.WriteAllText(scriptPath,
                    "@echo off\r\n" +
                    "timeout /t 2 /nobreak >nul\r\n" +
                    ":retry\r\n" +
                    "move /y \"" + escapedPending + "\" \"" + escapedExecutable + "\" >nul 2>&1\r\n" +
                    "if not exist \"" + escapedPending + "\" goto launch\r\n" +
                    "timeout /t 1 /nobreak >nul\r\n" +
                    "goto retry\r\n" +
                    ":launch\r\n" +
                    "start \"\" /d \"" + escapedDirectory + "\" \"" + escapedExecutable + "\"\r\n" +
                    "del /q \"" + escapedScript + "\"\r\n");

                Process.Start(new ProcessStartInfo
                {
                    FileName = Environment.GetEnvironmentVariable("ComSpec"),
                    Arguments = "/c \"" + scriptPath + "\"",
                    WorkingDirectory = installDirectory,
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Automatic update unavailable: " + ex.Message);
            }
        }

        private static void EnsureDesktopShortcut()
        {
            try
            {
                string targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Babis-W.exe");
                if (!File.Exists(targetPath))
                {
                    return;
                }

                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null)
                {
                    return;
                }

                string shortcutPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    "Babis V-M.lnk");
                dynamic shell = Activator.CreateInstance(shellType);
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                shortcut.Description = "Launch Babis-W";
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Babis-W.ico");
                shortcut.IconLocation = File.Exists(iconPath) ? iconPath : targetPath + ",0";
                shortcut.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to create the desktop shortcut: {ex.Message}");
            }
        }

        // Make BabisW actually draggable
        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Draggable top window
            DragMove();
        }



        // Theme settings
        private void ThemeLoading(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BabisWTheme");
                if (SettingReg != null)
                {
                    string Thingy = SettingReg.GetValue("ISDEFAULT").ToString();
                    if (Thingy == "No")
                    {
                        string LeftRGBVal = SettingReg.GetValue("LEFTRGB").ToString();
                        string RightRGBVal = SettingReg.GetValue("RIGHTRGB").ToString();
                        string BGImageURLVal = SettingReg.GetValue("BGIMAGEURL").ToString();
                        string BGImageTransparencyVal = SettingReg.GetValue("BGTRANSPARENCY").ToString();
                        float transparencyval = float.Parse(BGImageTransparencyVal);
                        transparencyval = transparencyval / 100;

                        // Border
                        var conv = new ColorConverter();
                        LinearGradientBrush brushy = new LinearGradientBrush();
                        brushy.StartPoint = new Point(0, 0);
                        brushy.EndPoint = new Point(1, 1);
                        brushy.GradientStops.Add(new GradientStop((Color)conv.ConvertFrom(LeftRGBVal), 0.0));
                        brushy.GradientStops.Add(new GradientStop((Color)conv.ConvertFrom(RightRGBVal), 0.0));
                        WindowBorder.BorderBrush = brushy;

                        ImageBrush bb = new ImageBrush();
                        Image image = new Image();
                        image.Source = new BitmapImage(new Uri(BGImageURLVal));
                        image.Opacity = transparencyval;
                        bb.ImageSource = image.Source;
                        bb.Opacity = transparencyval;
                        MainGrid.Background = bb;
                        MainGrid.Background.Opacity = transparencyval;

                        IsDefaultTheme = false;
                    }
                }
            }
            catch
            {
                // Prevent themes from breaking BabisW on startup
            }

        }


        // TAB CREATION FUNCTIONS //
        // Honestly these functions were actually pasted off other sources A LONG TIME AGO
        // http://avalonedit.net/documentation/ for any other references
        // Note that these are also "Sentinel" tabs

        public TextEditor CreateNewTab()
        {
            TextEditor textEditor = new TextEditor // Here, we make a new Avalon editor
            {
                // Setup some settings
                LineNumbersForeground = new SolidColorBrush(Color.FromRgb(199, 197, 197)),
                ShowLineNumbers = true,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Background = new SolidColorBrush(Color.FromArgb((byte)AvalonEditBGA, (byte)AvalonEditBGR, (byte)AvalonEditBGB, (byte)AvalonEditBGG)),
                FontFamily = new FontFamily(AvalonEditFont),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                WordWrap = true
            };

            // Set some settings regarding the editor
            textEditor.Options.EnableEmailHyperlinks = false;
            textEditor.Options.EnableHyperlinks = false; // The URL looks ugly
            textEditor.Options.AllowScrollBelowDocument = false;

            // Loop until Avalon has loaded
            while (IsAvalonLoaded == false)
            {
                Thread.Sleep(100);
            }

            // This one is for the theming for Avalon
            Stream xshd_stream = File.OpenRead(CurrentLuaXSHDLocation);
            XmlTextReader xshd_reader = new XmlTextReader(xshd_stream);
            textEditor.SyntaxHighlighting = HighlightingLoader.Load(xshd_reader, HighlightingManager.Instance); // Now finally set it

            xshd_reader.Close();
            xshd_stream.Close();
            return textEditor;
        }

        // Get the text from the current texteditor
        // You can call this using CurrentTabWithStuff()
        // For example, to set the texteditor text, CurrentTabWithStuff().Text = "Text here";
        public TextEditor CurrentTabWithStuff()
        {
            return this.TabControl.SelectedContent as TextEditor;
        }

        // Create a new tab
        public TabItem CreateTab(string text = "", string title = "Tab")
        {
            title = title + " " + TabControl.Items.Count.ToString(); // Counts the amount of tabs
            bool loaded = false; // Some weird bugs have been occuring without this here

            // Calls the function CreateNewTab, which is found above
            TextEditor textEditor = CreateNewTab();
            textEditor.Text = text;
            TabItem tab = new TabItem
            {
                Content = textEditor,
                Style = TryFindResource("Tab2") as Style, // Declared in the XAML stuff, https://docs.microsoft.com/en-us/dotnet/api/system.windows.frameworkelement.tryfindresource?view=net-5.0
                AllowDrop = true,
                Header = title // From the function

            };

            tab.Loaded += delegate (object source, RoutedEventArgs e) // Function for when the tab is loaded
            {
                if (loaded)
                {
                    return;
                }

                loaded = true; // Prevents some weird bug from occuring, though I doubt this is needed
            };

            // This is the function for the "X" icon, basically close tab button
            tab.MouseDown += delegate (object sender, MouseButtonEventArgs farded)
            {
                if (farded.OriginalSource is Border) // First of all actually check it
                {
                    if (farded.MiddleButton == MouseButtonState.Pressed) // MiddleButton = Left click here
                    {
                        this.TabControl.Items.Remove(tab); // Remove the tab
                        return;
                    }
                }
            };

            // A second loaded function, in order to actually make the magic work
            tab.Loaded += delegate (object seggs, RoutedEventArgs daddy)
            {
                tab.GetTemplateItem<System.Windows.Controls.Button>("CloseButton").Click += delegate (object sjdfaskdfasklf, RoutedEventArgs efdsn)
                {
                    this.TabControl.Items.Remove(tab);
                };

                loaded = true;
            };

            // Now finally set the title
            string oldHeader = title;
            this.TabControl.SelectedIndex = this.TabControl.Items.Add(tab);

            // This is the text that we start off with
            CurrentTabWithStuff().Text = DefaultTextEditorText;
            return tab;

        }

        // Now when the TabControl is loaded
        private void TextEditorLoad(object sender, RoutedEventArgs e)
        {
            // Loop until Avalon has loaded
            while (IsAvalonLoaded == false)
            {
                Thread.Sleep(100);
            }

            // Now, let's load up the theme
            Stream input = File.OpenRead(CurrentLuaXSHDLocation);
            XmlTextReader xmlTextReader = new XmlTextReader(input);
            TextEditor.SyntaxHighlighting = HighlightingLoader.Load(xmlTextReader, HighlightingManager.Instance);

            // Now actually set it
            Stream nya = File.OpenRead(CurrentLuaXSHDLocation);
            XmlTextReader xml = new XmlTextReader(nya);
            TextEditor.SyntaxHighlighting = HighlightingLoader.Load(xml, HighlightingManager.Instance);

            CurrentTabWithStuff().Text = DefaultTextEditorText; // Scroll all the way up to the top of this source code to set it

            // The template is defined in the xaml code
            this.TabControl.GetTemplateItem<System.Windows.Controls.Button>("AddTabButton").Click += delegate (object s, RoutedEventArgs f)
            {
                this.CreateTab("", "Tab");
            };

            // More theming
            foreach (TabItem tab in TabControl.Items)
            {
                tab.GetTemplateItem<System.Windows.Controls.Button>("CloseButton").Width = 0;
            }
        }

        // This is similar to the function above, but it's actually for the texteditor that first spawns in
        private void TextEditor_Loaded(object sender, RoutedEventArgs e)
        {
            // Set some settings regarding the editor
            TextEditor.Options.EnableEmailHyperlinks = false;
            TextEditor.Options.EnableHyperlinks = false; // The URL looks ugly
            TextEditor.Options.AllowScrollBelowDocument = false;

            // Set some stuff first
            MainWin.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            // Loop until Avalon has loaded
            while (IsAvalonLoaded == false)
            {
                Thread.Sleep(100);
            }

            // Load file
            Stream input = File.OpenRead(CurrentLuaXSHDLocation);
            XmlTextReader xmlTextReader = new XmlTextReader(input);
            CurrentTabWithStuff().SyntaxHighlighting = HighlightingLoader.Load(xmlTextReader, HighlightingManager.Instance);
        }

        // ADDITIONAL FUNCTIONS //
        // These are where additional functions go

        // Run Discord's RPC, this will reflect in setting options later
        public void DiscordRPC()
        {
            // For usage examples, look at https://github.com/Lachee/discord-rpc-csharp/blob/master/DiscordRPC.Example/Program.cs

            DiscordRpcClient client;
            client = new DiscordRpcClient("1385682532326183085") // The ID of the client
            {
                Logger = new ConsoleLogger
                {
                    Level = LogLevel.Warning
                }
            };

            client.OnReady += delegate { };
            client.OnPresenceUpdate += delegate { };
            client.Initialize();
            client.SetPresence(new RichPresence()
            {
                Details = "Using " + CurrentVersion, // Status
                State = "Keyless Roblox Exploit (as in we bypass key systems)",

                Timestamps = new Timestamps
                {
                    Start = DateTime.UtcNow,
                },

                Assets = new Assets
                {
                    LargeImageKey = "icon_babis-w_v3",
                    LargeImageText = "BabisW Roblox Exploit",
                    SmallImageKey = "icon_babis-w_v3_side"
                },


                // These show the buttons in the Discord RPC status


                Buttons = new DiscordRPC.Button[]
                {
                    new DiscordRPC.Button() { Label = "Join BabisW's Discord", Url = "https://babis-w.org/discord" },
                    new DiscordRPC.Button() { Label = "Get BabisW (GitHub)", Url = "https://github.com/Avaluate/BabisW" }
                },

            });
        }

        // ANIMATIONS //
        // All of these animations are just basic fade in/out anims
        // Oh and these are found in the XAML code

        // Sidebar animations
        // Home icon animation
        private void HomePage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("HomeOpen") as Storyboard;
            sb.Begin();

            // This is pretty damn stupid to do but oh well
            HomeGrid.Visibility = Visibility.Visible;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;


        }

        // Execution icon animation
        private void ExecutorPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("ExecutionOpen") as Storyboard;
            sb.Begin();

            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Visible;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;

        }

        // Scripthub page icon
        private void ScriptHubPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("ScriptHubOpen") as Storyboard;
            sb.Begin();

            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Visible;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;

        }

        // Utilities page icon
        private void UtilitiesPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("ToolsOpen") as Storyboard;
            sb.Begin();

            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Visible;
            SettingsGrid.Visibility = Visibility.Hidden;

        }

        // Settings page icon
        private void SettingsPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("SettingsOpen") as Storyboard;
            sb.Begin();

            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Visible;

        }

        // Settings animations

        // API Selection button
        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("APISelection1") as Storyboard;
            sb.Begin();

            APISelection.Visibility = Visibility.Visible;
            GeneralOptions.Visibility = Visibility.Hidden;
            ThemeSelection.Visibility = Visibility.Hidden;
        }

        // General options button
        private void Border_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("GeneralOptions1") as Storyboard;
            sb.Begin();
            APISelection.Visibility = Visibility.Hidden;
            GeneralOptions.Visibility = Visibility.Visible;
            ThemeSelection.Visibility = Visibility.Hidden;
        }

        // Theme selection button
        private void Border_MouseDown_3(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("ThemeSelection1") as Storyboard;
            sb.Begin();
            APISelection.Visibility = Visibility.Hidden;
            GeneralOptions.Visibility = Visibility.Hidden;
            ThemeSelection.Visibility = Visibility.Visible;
        }

        // TOP BAR FUNCTIONS //

        // Minimise BabisW
        private void Mini(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // Exit functions
        // Now, since we want to play a fade out animation, we will need something extra
        private void ExitMD(object sender, MouseButtonEventArgs e)
        {
            this.Close(); // There will be a function that will run when the window closes, MainWin_Closing
        }
        // Closing function
        private void MainWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CloseCompleted)
            {
                FormFadeOut.Begin(); // Start the animation storyboard FormFadeOut
                e.Cancel = true;
            }
        }
        // Now when it's actually done, kill off BabisW process
        private void CloseWindow(object sender, EventArgs e)
        {
            CloseCompleted = true;
            // just in case
            try { foreach (Process proc in Process.GetProcessesByName("Babis-WWRDWrapper")) { proc.Kill(); } } catch { }
            try { foreach (Process proc in Process.GetProcessesByName("WRDFakeServer")) { proc.Kill(); } } catch { }
            Environment.Exit(0);
        }

        // Draggable top window
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Draggable top window
            DragMove();
        }

        // HOME GRID FUNCTIONS //

        // Join Discord button
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenExternalUrl("https://babis-w.org/discord");
        }

        // Get help button
        private void Help(object sender, MouseButtonEventArgs e)
        {
            OpenExternalUrl("https://github.com/babssssza/Babis-W"); // Babis-W website/repository
        }

        private static void OpenExternalUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open the web page.\n\n{ex.Message}", "Babis-W", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Set notice board text
        private void Notice(object sender, RoutedEventArgs e)
        {
            try
            {
                NoticeBoard.Text = WebStuff.DownloadString(
                    "https://raw.githubusercontent.com/babssssza/Babis-W/main/UpdateStuff/Notice");
            }
            catch (Exception ex)
            {
                NoticeBoard.Text = "No announcements are available right now.";
                Console.WriteLine("Notice unavailable: " + ex.Message);
            }
        }

        // Set changelog board text
        private void ChangelogBoard(object sender, RoutedEventArgs e)
        {
            try
            {
                Changelog.Text = WebStuff.DownloadString(
                    "https://raw.githubusercontent.com/babssssza/Babis-W/main/UpdateStuff/Changelog");
            }
            catch (Exception ex)
            {
                Changelog.Text = "No changelog is available right now.";
                Console.WriteLine("Changelog unavailable: " + ex.Message);
            }
        }

        // EXECUTION GRID FUNCTIONS //

        // Execute script icon
        private void Execute(object sender, MouseButtonEventArgs e)
        {
            Execution.ExecutionHandler.Execute(this.CurrentTabWithStuff().Text); // We have our own execution handler class
            // You can find this in the Execution folder
        }

        // Injection icon
        private void Inject(object sender, MouseButtonEventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("RobloxPlayerBeta");
            if (IsInjected)
            {
                MessageBox.Show("The API has already been injected. Attempting to inject twice will result in a crash");
            }
            else if (InjectionInProgress)
            {
                MessageBox.Show("Injection is already in process");
            }
            else if (pname.Length > 0) // If Roblox is running
            {
                InjectionInProgress = true;

                // wrd
                if (Execution.SelectedAPI.API == "Selected API: WeAreDevs API")
                {
                    ShowWindow(GetConsoleWindow(), 5);
                    // wrd
                    if (Execution.SelectedAPI.API == "Selected API: WeAreDevs API")
                    {
                        ShowWindow(GetConsoleWindow(), 5);
                        ExecutionHandler.Inject();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please open Roblox first before attempting to inject");
            }
        }

        // Clear icon
        private void Clear(object sender, MouseButtonEventArgs e)
        {
            CurrentTabWithStuff().Text = ""; // Clear the currently selected textbox
        }

        // Kill Roblox icon (so end process)
        private void KillRoblox(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to kill Roblox?", "BabisW", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                // Just a basic YesNo thing
            }
            else
            {
                try
                {
                    foreach (Process proc in Process.GetProcessesByName("RobloxPlayerBeta")) // We will loop though each process just to find it
                    // Not sure if there's a more efficient way but this does the job quickly
                    {
                        proc.Kill();
                        MessageBox.Show("Roblox process killed", "BabisW");
                    }

                }
                catch
                {
                    // Just in case the user is stupid or something
                    MessageBox.Show("Roblox process has already been killed, or Roblox isn't running.", "BabisW");
                }
            }
        }

        // Open file function itself
        public static OpenFileDialog openfiledialog = new OpenFileDialog // New OpenFileDialog thing
        {
            Filter = "Text Files and Lua Files (*.txt *.lua)|*.txt;*.lua|All files (*.*)|*.*", // Either any of those will work
            FilterIndex = 1,
            RestoreDirectory = true,
            Title = "Open File"
        };

        // Open file icon
        private void OpenFile(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog FL = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "Text Files and Lua Files (*.txt *.lua)|*.txt;*.lua|All files (*.*)|*.*" // Same filter
            };

            if (FL.ShowDialog() == true)
            {
                CurrentTabWithStuff().Text = File.ReadAllText(FL.FileName);
            }
        }

        // Save file icon
        private void SaveFile(object sender, MouseButtonEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Title = "Save File",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                Filter = "Text Files and Lua Files(*.txt *.lua) | *.txt; *.lua | All files(*.*) | *.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (saveDialog.ShowDialog() == true) File.WriteAllText(saveDialog.FileName, CurrentTabWithStuff().Text);
        }

        // Sets the selected API
        private void SetAPI(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BabisWData");
            if (key != null)
            {
                string apishouldbe = (key.GetValue("DLL").ToString());
                CurrentAPILabel.Content = apishouldbe;
                Execution.SelectedAPI.API = apishouldbe;
            }
            else
            {
                CurrentAPILabel.Content = "Using WeAreDevs API";
                Execution.SelectedAPI.API = "Selected API: WeAreDevs API";
            }
        }

        private void StatusCheck(Object o)
        {
            Process[] pname = Process.GetProcessesByName("RobloxPlayerBeta");
            if (pname.Length > 0) // If Roblox is running
            {
                if (Execution.SelectedAPI.API == "Selected API: WeAreDevs API")
                {
                    Process[] pname1 = Process.GetProcessesByName("Babis-WWRDWrapper");
                    if (pname1.Length > 0) // so injection at this point has started
                    {
                        IsInjected = Execution.ExecutionHandler.IsInjected();
                        if (IsInjected)
                        {
                            ShowWindow(GetConsoleWindow(), 5); // wrd wants to hide it; however we want to show it for debugging
                            this.Dispatcher.Invoke(() =>
                            {
                                InjectionStatus.Content = "WeAreDevs injected";
                                InjectionStatus.Foreground = new SolidColorBrush(Color.FromRgb(0, 192, 140));
                            });
                        }
                        else // presumably injection is occuring
                        {
                            ShowWindow(GetConsoleWindow(), 5); // wrd wants to hide it; however we want to show it for debugging
                            this.Dispatcher.Invoke(() =>
                            {
                                InjectionStatus.Content = "WeAreDevs injection in progress";
                                InjectionStatus.Foreground = new SolidColorBrush(Color.FromRgb(170, 192, 0));
                            });
                        }
                    }
                    else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            InjectionStatus.Content = "Awaiting injection";
                            InjectionStatus.Foreground = new SolidColorBrush(Color.FromRgb(192, 110, 0));
                        });
                    }
                }
            }
            else
            {
                // for WRD
                try { foreach (Process proc in Process.GetProcessesByName("Babis-WWRDWrapper")) { proc.Kill(); } } catch { }
                try { foreach (Process proc in Process.GetProcessesByName("WRDFakeServer")) { proc.Kill(); } } catch { }

                this.Dispatcher.Invoke(() =>
                {
                    InjectionStatus.Content = "Roblox not opened";
                    InjectionStatus.Foreground = new SolidColorBrush(Color.FromRgb(192, 0, 0));
                    InjectionInProgress = false;
                });
            }
        }

        private System.Threading.Timer StatusCheckTimer; // Create timer

        private void StartStatusCheck(object sender, RoutedEventArgs e) // Actual function
        {
            StatusCheckTimer = new System.Threading.Timer(StatusCheck, null, 1000, 1000); // Run the check every 10 seconds or so
        }


        // SCRIPTHUB GRID FUNCTIONS //

        // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/properties/dependency-properties-overview?view=netdesktop-6.0
        public static readonly DependencyProperty ScriptStringVal = DependencyProperty.Register("ScriptString", typeof(ScriptDetails), typeof(TabThingy));

        public struct ScriptDetails // String for the script itself
        {
            public string ScriptExecute;
        }

        // Get details of script
        public ScriptDetails SetStringValue
        {
            get => (ScriptDetails)GetValue(ScriptStringVal);
            set => SetValue(ScriptStringVal, value);
        }


        // When the scripthub panel loads
        private void WrapPanel_LoadedAsync(object sender, RoutedEventArgs e)
        {

        }

        // TOOLS GRID FUNCTIONS //

        // Multi Roblox Instance
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (File.Exists("Applications\\MultipleRobloxInstances.exe"))
            {
                Process.Start("Applications\\MultipleRobloxInstances.exe");
            }
            else
            {
                MessageBox.Show("Downloading Multiple Roblox Instances. Click OK to continue.", "BabisW");

                WebStuff.DownloadFile("https://github.com/Avaluate/MultipleRobloxInstances/releases/download/v2.0/MultipleRobloxInstances.exe", "Applications\\MultipleRobloxInstances.exe");
                WebStuff.Dispose();
                Process.Start("Applications\\MultipleRobloxInstances.exe");
                MessageBox.Show("Multiple Roblox downloaded and started!", "BabisW");
            }
        }

        // FPS unlocker
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (File.Exists("Applications\\rbxfpsunlocker.exe"))
            {
                Process.Start("Applications\\rbxfpsunlocker.exe");
            }
            else
            {
                MessageBox.Show("Downloading FPS Unlocker. Click OK to continue.", "BabisW");
                // Taken from https://github.com/axstin/rbxfpsunlocker
                WebStuff.DownloadFile("https://github.com/Avaluate/BabisWWeb/blob/master/rbxfpsunlocker.exe?raw=true", "Applications\\rbxfpsunlocker.exe");
                WebStuff.Dispose();
                Process.Start("Applications\\rbxfpsunlocker.exe");
                MessageBox.Show("FPS unlocker downloaded and started!", "BabisW");
            }
        }

        // SETTINGS GRID FUNCTIONS //
        // There are multiple tabs on this grid //

        // WeAreDevs API selection
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Execution.SelectedAPI.API = "Selected API: WeAreDevs API";
            CurrentAPILabel.Content = "Using WeAreDevs API";
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BabisWData");
            key.SetValue("DLL", "Selected API: WeAreDevs API");
            key.Close();
            MessageBox.Show("API set to WeAreDevs", "BabisW");
        }

        // Join Discord for help button
        private void DiscordLinkie(object sender, RoutedEventArgs e)
        {
            Process.Start("https://babis-w.org/discord");
        }

        // General options //
        // Most of the code here is pretty much self explanatory
        // Topmost enable/disable
        private void TopmostFunc(object sender, RoutedEventArgs e)
        {
            RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BabisWSettings"); // From the settings we saved
            if (SettingReg != null)
            {
                string TopMostSetting = SettingReg.GetValue("TOPMOST").ToString();
                if (TopMostSetting == "Yes")
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BabisWSettings");
                    key.SetValue("TOPMOST", "No");
                    key.Close();
                    TopMostButton.Background = new SolidColorBrush(Color.FromArgb(20, 33, 33, 33));
                    TopMostButton.Content = "Disabled";
                    Topmost = false;
                }
                else
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BabisWSettings");
                    key.SetValue("TOPMOST", "Yes");
                    key.Close();
                    TopMostButton.Background = new SolidColorBrush(Color.FromArgb(20, 11, 47, 199));
                    TopMostButton.Content = "Enabled";
                    Topmost = true;
                }
            }
            else
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BabisWSettings");
                key.SetValue("TOPMOST", "Yes");
                key.Close();
                TopMostButton.Background = new SolidColorBrush(Color.FromArgb(20, 11, 47, 199));
                TopMostButton.Content = "Enabled";
                Topmost = true;
            }

        }

        // Load the setting for it
        private void TopmostFuncSetting(object sender, RoutedEventArgs e)
        {
            RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BabisWSettings");
            if (SettingReg != null)
            {
                string TopMostSetting = SettingReg.GetValue("TOPMOST").ToString();
                if (TopMostSetting == "Yes")
                {
                    TopMostButton.Content = "Enabled";
                    TopMostButton.Background = new SolidColorBrush(Color.FromArgb(20, 11, 47, 199));
                    Topmost = true;
                }
            }

        }

        // Discord RPC enable/disable function
        private void DiscordRPCFunc(object sender, RoutedEventArgs e)
        {
            RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BabisWRPC");
            if (SettingReg != null)
            {
                string TopMostSetting = SettingReg.GetValue("DISCORDRPC").ToString();
                if (TopMostSetting == "Yes")
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BabisWRPC");
                    key.SetValue("DISCORDRPC", "No");
                    key.Close();
                    DiscordRPCButton.Background = new SolidColorBrush(Color.FromArgb(20, 33, 33, 33));
                    DiscordRPCButton.Content = "Disabled";
                    MessageBox.Show("Discord RPC disabled, BabisW will now close. Please reopen BabisW to apply settings.", "BabisW");
                    Environment.Exit(0);
                }
                else
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BabisWRPC");
                    key.SetValue("DISCORDRPC", "Yes");
                    key.Close();
                    DiscordRPCButton.Content = "Enabled";
                    DiscordRPCButton.Background = new SolidColorBrush(Color.FromArgb(20, 11, 47, 199));
                    MessageBox.Show("Discord RPC enabled, BabisW will now close. Please reopen BabisW to apply settings.", "BabisW");
                    Environment.Exit(0);
                }
            }
            else
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BabisWRPC");
                key.SetValue("DISCORDRPC", "Yes");
                key.Close();
                DiscordRPCButton.Content = "Enabled";
                DiscordRPCButton.Background = new SolidColorBrush(Color.FromArgb(30, 11, 47, 199));
                MessageBox.Show("Discord RPC enabled, BabisW will now close. Please reopen BabisW to apply settings.", "BabisW");
                Environment.Exit(0);
            }
        }

        // Load setting for Discord RPC
        private void DiscordRPCSetting(object sender, RoutedEventArgs e)
        {
            if (Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BabisWRPC") != null)
            {
                RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BabisWRPC");
                if (SettingReg != null)
                {
                    if (SettingReg.GetValue("DISCORDRPC").ToString() != null)
                    {
                        string TopMostSetting = SettingReg.GetValue("DISCORDRPC").ToString();
                        if (TopMostSetting == "Yes")
                        {
                            DiscordRPCButton.Content = "Enabled";
                            DiscordRPCButton.Background = new SolidColorBrush(Color.FromArgb(20, 11, 47, 199));
                            DiscordRPC();
                        }
                    }
                }
                else
                {
                    DiscordRPCButton.Background = new SolidColorBrush(Color.FromRgb(40, 195, 126));
                    DiscordRPCButton.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    DiscordRPC();
                }
            }
            else
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BabisWRPC");
                key.SetValue("DISCORDRPC", "Yes");
                key.Close();
                DiscordRPCButton.Background = new SolidColorBrush(Color.FromRgb(40, 195, 126));
                DiscordRPCButton.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                DiscordRPC();
            }
        }

        // Use custom theme function
        private void UseCustomTheme(object sender, RoutedEventArgs e)
        {
            RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BabisWTheme");
            if (SettingReg != null)
            {
                if (IsDefaultTheme == false)
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BabisWTheme"); // Save the settings in registry
                    key.SetValue("ISDEFAULT", "No");
                    key.SetValue("LEFTRGB", LeftRGB);
                    key.SetValue("RIGHTRGB", RightRGB);
                    key.SetValue("BGIMAGEURL", BGImageURL);
                    key.SetValue("BGTRANSPARENCY", BGTransparency);
                    key.Close();
                    MessageBox.Show("Theme settings saved!", "BabisW");
                }
                else
                {
                    // Do nothing lol
                    MessageBox.Show("Theme settings saved!", "BabisW");
                }
            }
            else
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BabisWTheme");
                key.SetValue("ISDEFAULT", "No");
                key.SetValue("LEFTRGB", LeftRGB);
                key.SetValue("RIGHTRGB", RightRGB);
                key.SetValue("BGIMAGEURL", BGImageURL);
                key.SetValue("BGTRANSPARENCY", BGTransparency);
                key.Close();
                MessageBox.Show("Theme settings saved!", "BabisW");
            }
        }

        // Reset to default theme function
        private void ResetTheme(object sender, RoutedEventArgs e)
        {
            RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BabisWTheme");
            if (SettingReg != null)
            {
                if (IsDefaultTheme == false)
                {
                    IsDefaultTheme = true;
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BabisWTheme");
                    key.SetValue("ISDEFAULT", "Yes");
                    key.Close();
                    MessageBox.Show("Theme settings reset! Please reopen BabisW.", "BabisW");
                }
                else
                {
                    // Do nothing lol
                    MessageBox.Show("Theme settings reset! Please reopen BabisW.", "BabisW");
                }
            }
            else
            {
                IsDefaultTheme = true;
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BabisWTheme");
                key.SetValue("ISDEFAULT", "Yes");
                key.Close();
                MessageBox.Show("Theme settings reset! Please reopen BabisW.", "BabisW");
            }
        }

        // Theme options //
        // This part took time to make as well btw

        // Apply theme button
        private void ThemeApply(object sender, RoutedEventArgs e)
        {
            try
            {
                // Border first
                var conv = new ColorConverter();
                var left = (Color)conv.ConvertFrom(LeftGradient.Text);
                var right = (Color)conv.ConvertFrom(RightGradient.Text);
                LinearGradientBrush brushy = new LinearGradientBrush();
                brushy.StartPoint = new Point(0, 0);
                brushy.EndPoint = new Point(1, 1);
                brushy.GradientStops.Add(new GradientStop(left, 0.0));
                brushy.GradientStops.Add(new GradientStop(right, 1.0));
                WindowBorder.BorderBrush = brushy;

                // Image now alongside transparency
                float transparencyval = float.Parse(ImageTransparency.Text);
                transparencyval = transparencyval / 100;
                ImageBrush bb = new ImageBrush();
                Image image = new Image();
                image.Source = new BitmapImage(
                    new Uri(
                       ImageLink.Text));
                image.Opacity = transparencyval;
                bb.ImageSource = image.Source;
                bb.Opacity = transparencyval;
                MainGrid.Background = bb;
                MainGrid.Background.Opacity = transparencyval;

                // If user wants to set it as default theme
                IsDefaultTheme = false;
                LeftRGB = LeftGradient.Text;
                RightRGB = RightGradient.Text;
                BGImageURL = ImageLink.Text;
                BGTransparency = ImageTransparency.Text;

                MessageBox.Show("Theme set!", "BabisW");
            }
            catch
            {
                MessageBox.Show("Did you put in the right hex code format and image url?", "BabisW");
            }

        }

        // To be removed soon
        private void ThemeDemo1(object sender, RoutedEventArgs e)
        {
            LeftGradient.Text = "#4C464646";
            RightGradient.Text = "#4C464646";
            ImageTransparency.Text = "20";
            CreatorName.Text = "Main_EX";
            ImageLink.Text = "https://art.pixilart.com/b7875a3999e9a79.gif";
            var conv = new BrushConverter();
            WindowBorder.BorderBrush = (Brush)conv.ConvertFrom("#4C464646");
            ImageBrush bb = new ImageBrush();
            Image image = new Image();
            image.Source = new BitmapImage(
                new Uri(
                   "https://art.pixilart.com/b7875a3999e9a79.gif"));
            image.Opacity = 0.2;
            bb.ImageSource = image.Source;
            bb.Opacity = 0.2;
            MainGrid.Background = bb;
            MainGrid.Background.Opacity = 0.2;

            IsDefaultTheme = false;
            LeftRGB = "#4C464646";
            RightRGB = "#4C464646";
            BGImageURL = "https://art.pixilart.com/b7875a3999e9a79.gif";
            BGTransparency = "20";
        }

        // I'm just keeping this function here for now
        private void JSON(object sender, RoutedEventArgs e)
        {
            try
            {
                // Deserialise first
                // Might be a bad way but works for such a small purpose
                var jsonfile = File.ReadAllText("Themes\\theme.json");
                var stuff = JObject.Parse(jsonfile);

                // Declare
                var conv = new ColorConverter();

                // Border setting first
                var left = (Color)conv.ConvertFrom(stuff.SelectToken("TopLeftBorderColour").Value<string>());
                var right = (Color)conv.ConvertFrom(stuff.SelectToken("BottomRightBorderColour").Value<string>());

                // Actual setting
                LinearGradientBrush brushy = new LinearGradientBrush();
                brushy.StartPoint = new Point(0, 0);
                brushy.EndPoint = new Point(1, 1);
                brushy.GradientStops.Add(new GradientStop(left, 0.0));
                brushy.GradientStops.Add(new GradientStop(right, 1.0));
                WindowBorder.BorderBrush = brushy;

                // Image transparency
                float transparencyval = float.Parse(stuff.SelectToken("BackgroundImageTransparency").Value<string>());
                transparencyval = transparencyval / 100;

                // Image stuff
                ImageBrush bb = new ImageBrush();
                Image image = new Image();

                image.Source = new BitmapImage(new Uri(stuff.SelectToken("BackgroundImageURL").Value<string>()));
                image.Opacity = transparencyval; // Just in case

                bb.ImageSource = image.Source;
                bb.Opacity = transparencyval; // Just in case

                MainGrid.Background = bb;
                MainGrid.Background.Opacity = transparencyval; // Just in case

                //MessageBox.Show("Theme set!", "BabisW");
            }
            catch
            {
                MessageBox.Show("Did you put in the right hex code format and image url?", "BabisW");
            }
        }

        // When listbox loaded
        private void ThemeListBox_Loaded(object sender, RoutedEventArgs e)
        {
            string[] fileEntries = Directory.GetFiles("Themes"); // Get stuff from folder
            foreach (string fileName in fileEntries)
                ThemeListBox.Items.Add(fileName.Remove(0, 7));
        }

        private void ListBoxSelect(object sender, SelectionChangedEventArgs e) // When something is selected on the list
        {
            object item = ThemeListBox.SelectedItem;

            if (item == null)
            {

            }
            else
            {
                string filepath = item.ToString();
                filepath = "Themes\\" + filepath;
                try
                {

                    // Deserialise first
                    // Might be a bad way but works for such a small purpose
                    // MessageBox.Show(filepath);

                    var jsonfile = File.ReadAllText(filepath);
                    var stuff = JObject.Parse(jsonfile);

                    // Declare
                    var conv = new ColorConverter();

                    // Name
                    CreatorName.Text = stuff.SelectToken("MadeBy").Value<string>();


                    // Border setting first
                    var left = (Color)conv.ConvertFrom(stuff.SelectToken("TopLeftBorderColour").Value<string>());
                    var right = (Color)conv.ConvertFrom(stuff.SelectToken("BottomRightBorderColour").Value<string>());

                    LeftGradient.Text = stuff.SelectToken("TopLeftBorderColour").Value<string>();
                    RightGradient.Text = stuff.SelectToken("BottomRightBorderColour").Value<string>();

                    // Actual setting
                    LinearGradientBrush brushy = new LinearGradientBrush();
                    brushy.StartPoint = new Point(0, 0);
                    brushy.EndPoint = new Point(1, 1);
                    brushy.GradientStops.Add(new GradientStop(left, 0.0));
                    brushy.GradientStops.Add(new GradientStop(right, 1.0));
                    WindowBorder.BorderBrush = brushy;

                    // Image transparency
                    float transparencyval = float.Parse(stuff.SelectToken("BackgroundImageTransparency").Value<string>());
                    ImageTransparency.Text = transparencyval.ToString();

                    transparencyval = transparencyval / 100;

                    // Image stuff
                    ImageBrush bb = new ImageBrush();
                    Image image = new Image();

                    image.Source = new BitmapImage(new Uri(stuff.SelectToken("BackgroundImageURL").Value<string>()));
                    image.Opacity = transparencyval; // Just in case
                    ImageLink.Text = stuff.SelectToken("BackgroundImageURL").Value<string>();

                    bb.ImageSource = image.Source;
                    bb.Opacity = transparencyval; // Just in case

                    MainGrid.Background = bb;
                    MainGrid.Background.Opacity = transparencyval; // Just in case

                    // Default theme
                    IsDefaultTheme = false;
                    LeftRGB = stuff.SelectToken("TopLeftBorderColour").Value<string>();
                    RightRGB = stuff.SelectToken("BottomRightBorderColour").Value<string>();
                    BGImageURL = stuff.SelectToken("BackgroundImageURL").Value<string>();
                    BGTransparency = stuff.SelectToken("BackgroundImageTransparency").Value<string>();

                    //MessageBox.Show("Theme set!", "BabisW");
                }
                catch
                {
                    // MessageBox.Show("The theme file is invalid!", "BabisW");
                }
            }
        }

        private void ThemeSave(object sender, RoutedEventArgs e)
        {
            // Instead of properly sterialising the json file properly, we can manually write it ourselves
            string FinalJson = ("{\n" +
                "  \"MadeBy\" : \"" + CreatorName.Text + "\",\n" +
                "  \"TopLeftBorderColour\" : \"" + LeftGradient.Text + "\",\n" +
                "  \"BottomRightBorderColour\" : \"" + RightGradient.Text + "\",\n" +
                "  \"BackgroundImageURL\" : \"" + ImageLink.Text + "\",\n" +
                "  \"BackgroundImageTransparency\" : \"" + ImageTransparency.Text + "\"\n" +
                "}"); // I didn't want to go though so much trouble figuring out steralisation, bear with this

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON file (*.json)|*.json";
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, FinalJson); // Save file

        }

        // Download themes button
        private void ThemeDownload(object sender, RoutedEventArgs e)
        {
            ThemeListBox.Items.Clear();
            DownloadMessage.Visibility = Visibility.Visible;
            if (!Directory.Exists("Themes"))
            {
                Directory.CreateDirectory("Themes"); // just checking, just checking.
            }
            string[] fileEntriesasdasd = Directory.GetFiles("Themes");
            foreach (string fileNameb in fileEntriesasdasd)
                ThemeListBox.Items.Add(fileNameb.Remove(0, 7));

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                this.Dispatcher.Invoke(() =>
                {
                    DownloadMessageTextBox.Text = "Downloading themes from theme repository...";

                    Storyboard sb = TryFindResource("FadeInListBox") as Storyboard;
                    sb.Begin();

                });
                var json = WebStuff.DownloadString("https://raw.githubusercontent.com/Avaluate/BabisWWeb/master/UpdateStuff/ThemeList.json");
                dynamic dsfadfasdf = JsonConvert.DeserializeObject(json);
                foreach (var item in dsfadfasdf)
                {
                    string FileName = item.filename;
                    string URL = item.themeurl;
                    if (File.Exists("Themes\\" + FileName))
                    {
                        File.Delete("Themes\\" + FileName); // Update themes
                    }
                    WebStuff.DownloadFile(URL, "Themes\\" + FileName);
                    this.Dispatcher.Invoke(() =>
                    {
                        if (ThemeListBox.Items.Contains(FileName))
                        {
                            // Do nothing
                        }
                        else
                        {
                            ThemeListBox.Items.Add(FileName);
                        }
                    });
                }
                this.Dispatcher.Invoke(() =>
                {
                    DownloadMessageTextBox.Text = "Download completed!";
                    Storyboard sb = TryFindResource("FadeOutListBox") as Storyboard;
                    sb.Completed += new EventHandler(Done);
                    sb.Begin();
                });
            }).Start();
        }

        private void Done(object sender, EventArgs e)
        {
            DownloadMessage.Visibility = Visibility.Hidden; // For the animation
        }

        // OTHER FUNCTIONS //

        // Originally we thought WeAreDevs would sponsor BabisW. Turns out not. I'm still salty about that.
        private void TextBlock_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://wearedevs.net/"); // As asked, the link is correct
        }

        private void LoadingScreenLoaded(object sender, RoutedEventArgs e)
        {

        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChangedAsync(object sender, TextChangedEventArgs e)
        {
            // This is likely a very inefficient method of searching, if there are better ways, I would like to know
            // The CPU usage rises while searching
            if (IsScriptHubOpened == true)
            {
                new Thread(() =>
                {
                    this.Dispatcher.Invoke(() => // Prevent error from this being done on "another thread"
                    {
                        WP.Children.Clear();

                        foreach (var scriptData in scripts ?? Array.Empty<ScriptHub.ScriptData>())
                        {
                            var obj = new TabThingy
                            {
                                Script = (ScriptHub.ScriptData)scriptData
                            };

                            if (GeneralScriptSearch.Text == "" || GeneralScriptSearch.Text == "Search for a script here" || GeneralScriptSearch.IsFocused == false || GeneralScriptSearch.IsKeyboardFocused == false)
                            {
                                // Functions for buttons
                                obj.Executed += (_, _) => Execution.ExecutionHandler.Execute(obj.Script.Script);
                                obj.CopyScript += (_, _) => Clipboard.SetText(obj.Script.Script);
                                WP.Children.Add(obj); // Add objects into scripthub panel
                            }

                            else if (obj.ScriptTitle.Content.ToString().ToLower().Contains(GeneralScriptSearch.Text.ToLower()) == true || obj.Description.Text.ToString().ToLower().Contains(GeneralScriptSearch.Text.ToLower()) == true || obj.Credit.Content.ToString().ToLower().Contains(GeneralScriptSearch.Text.ToLower()) == true)
                            {
                                // Functions for buttons
                                obj.Executed += (_, _) => Execution.ExecutionHandler.Execute(obj.Script.Script);
                                obj.CopyScript += (_, _) => Clipboard.SetText(obj.Script.Script);
                                WP.Children.Add(obj); // Add objects into scripthub panel
                            }

                        }
                        GC.Collect();

                    });

                })

                { }.Start();
            }
        }

        private void GeneralScriptSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (GeneralScriptSearch.Text == "Search for a script here")
            {
                GeneralScriptSearch.Text = "";
                GeneralScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(100, 137, 137, 137));
            }
        }
        private void GeneralScriptSearch_GotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (GeneralScriptSearch.Text == "Search for a script here")
            {
                GeneralScriptSearch.Text = "";
                GeneralScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(100, 137, 137, 137));
            }
        }

        private void GeneralScriptSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (GeneralScriptSearch.Text == "")
            {
                GeneralScriptSearch.Text = "Search for a script here";
                GeneralScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(0, 137, 137, 137));
            }
        }

        private void GeneralScriptSearch_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (GeneralScriptSearch.Text == "")
            {
                GeneralScriptSearch.Text = "Search for a script here";
                GeneralScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(0, 137, 137, 137));
            }
        }

        private void WP1_LoadedAsync(object sender, RoutedEventArgs e)
        {


        }

        private void GameScriptTextChanged(object sender, TextChangedEventArgs e)
        {


        }

        private void GameScriptSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (GameScriptSearch.Text == "Search for a script here")
            {
                GameScriptSearch.Text = "";
                GameScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(100, 137, 137, 137));
            }
        }

        private void GameScriptSearch_GotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (GameScriptSearch.Text == "Search for a script here")
            {
                GameScriptSearch.Text = "";
                GameScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(100, 137, 137, 137));
            }
        }

        private void GameScriptSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (GameScriptSearch.Text == "")
            {
                GameScriptSearch.Text = "Search for a script here";
                GameScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(0, 137, 137, 137));
            }
        }

        private void GameScriptSearch_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (GameScriptSearch.Text == "")
            {
                GameScriptSearch.Text = "Search for a script here";
                GameScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(0, 137, 137, 137));
            }
        }

        private void GameHubOpen(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("GameHubOpen") as Storyboard;
            sb.Begin();

            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Visible;
            ToolsGrid.Visibility = Visibility.Hidden;
            CustomisationGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void HomeRadioButtonClick(object sender, RoutedEventArgs e)
        {
            // This is pretty damn stupid to do but oh well
            HomeGrid.Visibility = Visibility.Visible;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            CustomisationGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void ExecutorRadioButtonClick(object sender, RoutedEventArgs e)
        {
            // This is pretty damn stupid to do but oh well
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Visible;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            CustomisationGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void ScriptHubRadioButtonClick(object sender, RoutedEventArgs e)
        {
            if (IsScriptHubOpened == false)
            {
                IsScriptHubOpened = true;
                WP.Children.Clear();
                foreach (var scriptData in scripts)
                {
                    var obj = new TabThingy
                    {
                        Script = (ScriptHub.ScriptData)scriptData
                    };
                    // Functions for buttons
                    obj.Executed += (_, _) => Execution.ExecutionHandler.Execute(obj.Script.Script);
                    obj.CopyScript += (_, _) => Clipboard.SetDataObject(obj.Script.Script);
                    WP.Children.Add(obj); // Add objects into scripthub panel
                }
            }


            // This is pretty damn stupid to do but oh well
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Visible;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            CustomisationGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void GameHubRadioButtonClick(object sender, RoutedEventArgs e)
        {
            if (IsGameHubOpened == false)
            {
                IsGameHubOpened = true;
                WP1.Children.Clear();
                foreach (var scriptData in gamescripts ?? Array.Empty<ScriptHub.GameScriptData>())
                {
                    var obj = new GameTab()
                    {
                        Script = (ScriptHub.GameScriptData)scriptData
                    };
                    // Functions for buttons
                    obj.Executed += (_, _) => Execution.ExecutionHandler.Execute(obj.Script.Script);
                    obj.CopyScript += (_, _) => Clipboard.SetText(obj.Script.Script);
                    WP1.Children.Add(obj); // Add objects into scripthub panel
                }
            }


            // This is pretty damn stupid to do but oh well
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Visible;
            ToolsGrid.Visibility = Visibility.Hidden;
            CustomisationGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void ToolsRadioButtonClick(object sender, RoutedEventArgs e)
        {
            // This is pretty damn stupid to do but oh well
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Visible;
            CustomisationGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void SettingsRadioButtonClick(object sender, RoutedEventArgs e)
        {
            // This is pretty damn stupid to do but oh well
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            CustomisationGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Visible;
        }

        private void HomeRadioButtonLoaded(object sender, RoutedEventArgs e)
        {
            HomeRadioButton.IsChecked = true;
        }

        private void SearchGameHub(object sender, RoutedEventArgs e)
        {
            // This is likely a very inefficient method of searching, if there are better ways, I would like to know
            // The CPU usage rises while searching

            new Thread(() =>
            {
                this.Dispatcher.Invoke(() => // Prevent error from this being done on "another thread"
                {
                    WP1.Children.Clear();

                    foreach (var scriptData in gamescripts)
                    {
                        var obj = new GameTab
                        {
                            Script = (ScriptHub.GameScriptData)scriptData
                        };

                        if (GameScriptSearch.Text == "" | GameScriptSearch.Text == "Search for a script here" | string.IsNullOrEmpty(GameScriptSearch.Text))
                        {
                            // Functions for buttons
                            obj.Executed += (_, _) => Execution.ExecutionHandler.Execute(obj.Script.Script);
                            obj.CopyScript += (_, _) => Clipboard.SetText(obj.Script.Script);
                            WP1.Children.Add(obj); // Add objects into scripthub panel
                        }

                        else if (obj.ScriptTitle.Content.ToString().ToLower().Contains(GameScriptSearch.Text.ToLower()) == true | obj.Description.Text.ToString().ToLower().Contains(GameScriptSearch.Text.ToLower()) == true | obj.Credit.Content.ToString().ToLower().Contains(GameScriptSearch.Text.ToLower()) == true)
                        {
                            // Functions for buttons
                            obj.Executed += (_, _) => Execution.ExecutionHandler.Execute(obj.Script.Script);
                            obj.CopyScript += (_, _) => Clipboard.SetText(obj.Script.Script);
                            WP1.Children.Add(obj); // Add objects into scripthub panel
                        }
                    }
                });
            })
            { }.Start();
        }

        private void MainWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.F5))
            {
                if (BlurTextbox.Visibility == Visibility.Visible)
                {
                    BlurTextbox.Visibility = Visibility.Hidden;
                }
                else
                {
                    BlurTextbox.Visibility = Visibility.Visible;
                }
            }
        }

        private void SearchScriptHub(object sender, RoutedEventArgs e)
        {
            // This is likely a very inefficient method of searching, if there are better ways, I would like to know
            // The CPU usage rises while searching
            if (IsScriptHubOpened == true)
            {
                new Thread(() =>
                {
                    this.Dispatcher.Invoke(() => // Prevent error from this being done on "another thread"
                    {
                        WP.Children.Clear();

                        foreach (var scriptData in scripts)
                        {
                            var obj = new TabThingy
                            {
                                Script = (ScriptHub.ScriptData)scriptData
                            };

                            if (GeneralScriptSearch.Text == "" | GeneralScriptSearch.Text == "Search for a script here" | string.IsNullOrEmpty(GeneralScriptSearch.Text))
                            {
                                // Functions for buttons
                                obj.Executed += (_, _) => Execution.ExecutionHandler.Execute(obj.Script.Script);
                                obj.CopyScript += (_, _) => Clipboard.SetText(obj.Script.Script);
                                WP.Children.Add(obj); // Add objects into scripthub panel
                            }

                            else if (obj.ScriptTitle.Content.ToString().ToLower().Contains(GeneralScriptSearch.Text.ToLower()) == true || obj.Description.Text.ToString().ToLower().Contains(GeneralScriptSearch.Text.ToLower()) == true || obj.Credit.Content.ToString().ToLower().Contains(GeneralScriptSearch.Text.ToLower()) == true)
                            {
                                // Functions for buttons
                                obj.Executed += (_, _) => Execution.ExecutionHandler.Execute(obj.Script.Script);
                                obj.CopyScript += (_, _) => Clipboard.SetText(obj.Script.Script);
                                WP.Children.Add(obj); // Add objects into scripthub panel
                            }

                        }
                        GC.Collect();

                    });

                })

                { }.Start();
            }
        }

        private void CustomisationRadioButtonClick(object sender, RoutedEventArgs e)
        {
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            CustomisationGrid.Visibility = Visibility.Visible;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void WRDStatus_Loaded_1(object sender, RoutedEventArgs e)
        {
            // wrd status checker

            string WRDStatusDl = WebStuff.DownloadString("https://cdn.wearedevs.net/software/jjsploit/tauri.json");




            dynamic WRDStatusFormat = JsonConvert.DeserializeObject(WRDStatusDl);
            bool IsWRDPatched = WRDStatusFormat.patched;
            WRDStatusTextFromWe.Text = $"Message from WRD: {WRDStatusFormat.serverMessage}";

            if (!IsWRDPatched) // not patch
            {
                WRDStatus.Fill = new SolidColorBrush(Color.FromRgb(40, 195, 126));
                WRDStatusText.Text = "Unpatched and working!";
            }
            else
            {
                WRDStatus.Fill = new SolidColorBrush(Color.FromRgb(255, 30, 30));
                WRDStatusText.Text = "Currently patched";
            }



        }

        private void JoinTelegramGroup(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://t.me/babis-wnow");
        }

        private void OpenGitHub(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/Avaluate/BabisW");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WpfAnimatedGif;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.Diagnostics;
using System.ComponentModel;

namespace BabisW_Bootstrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private const string ProductName = "Babis-W";
        private const string ExecutableName = "Babis-W.exe";
        private const string InstallDirectory = "Babis-W";
        private const string GitHubRepository = "babssssza/Babis-W";
        private const string LatestReleaseDownload = "https://raw.githubusercontent.com/" + GitHubRepository + "/main/Babis-W/bin/Release/" + ExecutableName;

        // WebClient Creation
        WebClient WebStuff = new WebClient(); // Create a new generally used WebClient

        public MainWindow()
        {
            InitializeComponent();
            Startup.Opacity = 0;
            
        }


        // Newer animation functions
        // StackOverflow saved my ass for this one

        public void Fade(DependencyObject ElementName, double Start, double End, double Time)
        {
            DoubleAnimation Anims = new DoubleAnimation()
            {
                From = Start,
                To = End,
                Duration = TimeSpan.FromSeconds(Time),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(Anims, ElementName);
            Storyboard.SetTargetProperty(Anims, new PropertyPath(OpacityProperty)); // well i don't actually think transparency has this effect
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(Anims);
            storyboard.Begin();
        }

        public void Move(DependencyObject ElementName, Thickness Origin, Thickness Location, double Time)
        {
            ThicknessAnimation Anims = new ThicknessAnimation()
            {
                From = Origin,
                To = Location,
                Duration = TimeSpan.FromSeconds(Time),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(Anims, ElementName);
            Storyboard.SetTargetProperty(Anims, new PropertyPath(MarginProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(Anims);
            storyboard.Begin();
        }
        public void Scaling(DependencyObject ElementName, double Before, double After, double Time)
        {
            DoubleAnimation ScalingX = new DoubleAnimation()
            {
                From = Before,
                To = After,
                Duration = TimeSpan.FromSeconds(Time),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(ScalingX, ElementName);
            Storyboard.SetTargetProperty(ScalingX, new PropertyPath("RenderTransform.Children[0].ScaleX"));
            Storyboard StoryboardX = new Storyboard();
            StoryboardX.Children.Add(ScalingX);

            DoubleAnimation ScalingY = new DoubleAnimation()
            {
                From = Before,
                To = After,
                Duration = TimeSpan.FromSeconds(Time),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(ScalingY, ElementName);
            Storyboard.SetTargetProperty(ScalingY, new PropertyPath("RenderTransform.Children[0].ScaleY"));
            Storyboard StoryboardY = new Storyboard();
            StoryboardY.Children.Add(ScalingY);

            StoryboardX.Begin();
            StoryboardY.Begin();
        }

        private async Task BackgroundImageAnimationAsync()
        {
            while(true)
            {
                await Task.Delay(1500);
                Fade(DownloadBG, 0.05, 0.01, 1.5);
                await Task.Delay(1500);
                Fade(DownloadBG, 0.01, 0.05, 1.5);
            }
        }

        private async void LoadedAsync(object sender, RoutedEventArgs e)
        {
            Startup.Visibility = Visibility.Visible;
            Startup.Opacity = 0;
            BabisWTitle.Opacity = 0;

            Fade(Startup, 0, 1, 0.5);
            await Task.Delay(1000);
 
            Move(BabisWIcon, BabisWIcon.Margin, new Thickness(127.2, 0, -0.8, 0), 0.5);
            Move(BabisWTitle, BabisWTitle.Margin, new Thickness(0, 189, 0, 0), 0.6);
            Scaling(BabisWIcon, 1, 0.79, 0.5);
            Fade(BabisWTitle, 0, 1, 0.5);

            await Task.Delay(1500);

            Move(BabisWTitle, BabisWTitle.Margin, new Thickness(0, 203, 0, 0), 0.6);
            Move(BabisWIcon, BabisWIcon.Margin, new Thickness(127.2, 0, 53.2, 0), 0.5);
            Fade(BabisWIcon, 1, 0, 0.5);
            Fade(BabisWTitle, 1, 0, 0.5);
            Scaling(BabisWIcon, 0.79, 0.89, 0.5);

            await Task.Delay(1000);

            Startup.Visibility = Visibility.Hidden;
            DownloadBabisW.Visibility = Visibility.Visible;

            WebStuff.Headers[HttpRequestHeader.UserAgent] = ProductName + " Downloader";
            LatestUpdate.Content = "Latest " + ProductName + " build: main branch";

            if (File.Exists(Path.Combine(InstallDirectory, ExecutableName)) || File.Exists(ExecutableName))
            {
                InstallUpdateText.Content = ProductName + " update found";
                InstallButton.Content = "Update " + ProductName;
            }

            Move(InstallUpdateText, InstallUpdateText.Margin, new Thickness(0, 111, -2.2, 0), 0.5);
            Move(InstallButton, InstallButton.Margin, new Thickness(216, 154, 0, 0), 0.6);
            Move(LatestUpdate, LatestUpdate.Margin, new Thickness(-2, 190, -0.2, 0), 0.7);
            Fade(InstallUpdateText, 0, 1, 0.5);
            Fade(InstallButton, 0, 1, 0.6);
            Fade(LatestUpdate, 0, 1, 0.7);
            
            await Task.Delay(1000);

            JoinDiscord.Visibility = Visibility.Visible;
            JoinOurDiscord.Visibility = Visibility.Visible;
            Fade(JoinDiscord, 0, 0.5, 1);
            Fade(JoinOurDiscord, 0, 1, 1);

            Fade(DownloadBG, 0, 0.05, 1.5);

            this.Dispatcher.Invoke(() =>
            {
                _ = BackgroundImageAnimationAsync();
            });
            
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove(); // Make window draggable
            }
            catch
            {
                // Apparently some other random mouse clicks can make this glitch
            }
           
        }

       
        // Download
    
        private async void InstallButton_MouseDown(object sender, RoutedEventArgs e)
        {
            InstallButton.IsEnabled = false;
            Move(InstallUpdateText, new Thickness(0, 111, -2.2, 0), new Thickness(0, 139, -2.2, 0), 0.5);
            Move(InstallButton, new Thickness(216, 154, 0, 0), new Thickness(216, 182, 0, 0), 0.6);
            Move(LatestUpdate, new Thickness(-2, 190, -0.2, 0), new Thickness(-2, 218, -0.2, 0), 0.7);

            Fade(InstallUpdateText, 1, 0, 0.5);
            Fade(InstallButton,1, 0, 0.6);
            Fade(LatestUpdate, 1, 0, 0.7);

            await Task.Delay(700);

            DownloadBabisW.Visibility = Visibility.Hidden;
            InstallBabisW.Visibility = Visibility.Visible;

            Fade(InstallBabisW, 0, 1, 0.5);
            Fade(RequirementCheck, 0, 1, 0.5);


            Fade(CheckingRequirementsLabel, 0, 1, 0.5);
            Fade(Gif1, 0, 0.8, 0.5);
            Move(CheckingRequirementsLabel, new Thickness(35, 101, 0, 0), new Thickness(35, 111, 0, 0), 0.5);
            Move(Gif1, new Thickness(246, 109, 0, 0), new Thickness(246, 119, 0, 0), 0.5);

            Fade(DeletingFilesLabel, 0, 1, 0.6);
            Fade(Gif2, 0, 0.8, 0.6);
            Move(DeletingFilesLabel, new Thickness(35, 130, 0, 0), new Thickness(35, 140, 0, 0), 0.6);
            Move(Gif2, new Thickness(246, 138, 0, 0), new Thickness(246, 148, 0, 0), 0.6);

            Fade(CreatingFoldersLabel, 0, 1, 0.7);
            Fade(Gif3, 0, 0.8, 0.7);
            Move(CreatingFoldersLabel, new Thickness(35, 159, 0, 0), new Thickness(35, 169, 0, 0), 0.7);
            Move(Gif3, new Thickness(246, 167, 0, 0), new Thickness(246, 177, 0, 0), 0.7);

            Fade(DownloadingBabisWLabel, 0, 1, 0.8);
            Fade(Gif4, 0, 0.8, 0.8);
            Move(DownloadingBabisWLabel, new Thickness(35 ,188, 0, 0), new Thickness(35, 198, 0, 0), 0.8);
            Move(Gif4, new Thickness(246, 196, 0, 0), new Thickness(246, 206, 0, 0), 0.8);

            HttpClient fd = new HttpClient();
            var GetWebsite = await fd.GetAsync("https://github.com");
            if (!GetWebsite.IsSuccessStatusCode)
            {
                // GitHub not accessable!
                MessageBox.Show(ProductName + "'s downloader cannot reach GitHub, which is required to download " + ProductName + ". Please check your firewall or router settings.\n\nYou can join the " + ProductName + " community at babis-w.org/discord if you need more help.", "Error connecting to GitHub");
                Environment.Exit(0);
            }

            await Task.Delay(2500);

            Fade(RequirementCheck, 1, 0, 0.5);
            Fade(Gif1, 0.8, 0, 0.5);
            await Task.Delay(600);

            Gif1.Visibility = Visibility.Hidden;
            Gif1Completed.Visibility = Visibility.Visible;

            RequirementCheck.Visibility = Visibility.Hidden;
            DeletingFiles.Visibility = Visibility.Visible;
            Fade(Gif1Completed, 0, 0.8, 0.5);
            Fade(DeletingFiles, 0, 1, 0.5);
            await Task.Delay(500);

            // Delete the existing application file.
            try
            {
                foreach (Process proc in Process.GetProcessesByName("Babis-W"))
                {
                    proc.Kill();
                }
            }
            catch { }

            await Task.Delay(2000);

            if (File.Exists(ExecutableName))
            {
                File.Delete(ExecutableName);
            }

            Fade(DeletingFiles, 1, 0, 0.5);
            Fade(Gif2, 0.8, 0, 0.5);
            await Task.Delay(600);

            Gif2.Visibility = Visibility.Hidden;
            Gif2Completed.Visibility = Visibility.Visible;

            DeletingFiles.Visibility = Visibility.Hidden;
            CreatingFolders.Visibility = Visibility.Visible;

            Fade(Gif2Completed, 0, 0.8, 0.5);
            Fade(CreatingFolders, 0, 1, 0.5);
            await Task.Delay(500);

            // Remove the previous installation folder.
            try
            {
                DirectoryInfo ruh = new DirectoryInfo(InstallDirectory);
                foreach (FileInfo file in ruh.GetFiles())
                {
                    file.Delete();
                }
                if (Directory.Exists(InstallDirectory))
                {
                    Directory.Delete(InstallDirectory);
                }
            }
            catch { }

            Directory.CreateDirectory(InstallDirectory);

            await Task.Delay(500);

            Fade(CreatingFolders, 1, 0, 0.5);
            Fade(Gif3, 0.8, 0, 0.5);
            await Task.Delay(600);

            Gif3.Visibility = Visibility.Hidden;
            Gif3Completed.Visibility = Visibility.Visible;

            CreatingFolders.Visibility = Visibility.Hidden;
            DownloadingBabisW.Visibility = Visibility.Visible;

            Fade(Gif3Completed, 0, 0.8, 0.5);
            Fade(DownloadingBabisW, 0, 1, 0.5);
            await Task.Delay(500);

            WebStuff.DownloadProgressChanged += new DownloadProgressChangedEventHandler(WebStuff_DownloadProgressChanged);
            WebStuff.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(WebStuff_DownloadCompleted);
            WebStuff.DownloadFileAsync(new Uri(LatestReleaseDownload), Path.Combine(InstallDirectory, ExecutableName));

            
        }

        void WebStuff_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                DownloadBar.Value = int.Parse(Math.Truncate(percentage).ToString());
            });
        }

        void WebStuff_DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.Dispatcher.Invoke(async () =>
            {
                if (e.Error != null)
                {
                    MessageBox.Show("Babis-W could not be downloaded: " + e.Error.Message, "Download error");
                    InstallButton.IsEnabled = true;
                    return;
                }

                await Task.Delay(500);
                Fade(DownloadingBabisW, 1, 0, 0.5);
                Fade(Gif4, 0.8, 0, 0.5);
                await Task.Delay(600);
                DownloadingBabisW.Visibility = Visibility.Hidden;
                Gif4.Visibility = Visibility.Hidden;
                Gif4Completed.Visibility = Visibility.Visible;
                ContinueToBabisW.Visibility = Visibility.Visible;
                Fade(Gif4Completed, 0, 0.8, 0.5);
                Fade(ContinueToBabisW, 0, 1, 0.5);
                
                string installedExecutable = Path.Combine(Environment.CurrentDirectory, InstallDirectory, ExecutableName);
                if (!File.Exists(installedExecutable))
                {
                    MessageBox.Show("The Babis-W executable was not found after the download completed.", "Installation error");
                    InstallButton.IsEnabled = true;
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = installedExecutable,
                    WorkingDirectory = Path.GetDirectoryName(installedExecutable),
                    UseShellExecute = true
                });

                await Task.Delay(2000);
                Fade(MainGrid, 1, 0, 0.5);
                await Task.Delay(501);
                Environment.Exit(0);
            });
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void InstallButton_Loaded(object sender, RoutedEventArgs e)
        {
           
        }
    }
}

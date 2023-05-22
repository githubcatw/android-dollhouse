using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Octokit;
using System.Net.Http;
using System.IO;
using Microsoft.Win32;
using System.Reflection;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        
        string VersionNumber = "v0.0.4-alpha";
        public static PluggedDevice adbDevice;
        public MAIN MAIN = new MAIN();
        public int settings;
        static readonly HttpClient downloader = new HttpClient();



        public MainWindow()
        {
            this.FontFamily = new FontFamily("Inter");
            InitializeComponent();
        }

        

        private void AcrylicWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                    var s = e.GetPosition(MainWindowBase);
                    this.Top = 0;
                    this.Left = s.X/2;
                }
                this.DragMove();
            }
        }

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Frame.Source = new Uri("SCRIPTS.xaml", UriKind.Relative);
        }
        private void Main_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

            Frame.Navigate(MAIN);
        }

        private void KillServer()
        {
            Process p = new Process();
            p.StartInfo.FileName = "./res/platform-tools/adb.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;

            p.StartInfo.Arguments = "kill-server";
            p.Start();
            string s = p.StandardError.ReadToEnd();
            s = p.StandardOutput.ReadToEnd();
            p.Kill();
            p.Dispose();
        }
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            KillServer();
            Environment.Exit(0);
        }

        private void AdvancedTabTextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Frame.Source = new Uri("ADVANCED.xaml", UriKind.Relative);
        }

        private void MainTabTextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            MainTabTextBlock.FontSize = 21;
        }

        private void ScriptsTabTextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            ScriptsTabTextBlock.FontSize = 21;
        }

        private void AdvancedTabTextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            AdvancedTabTextBlock.FontSize = 21;
        }

        private void MainTabTextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            MainTabTextBlock.FontSize = 20;

        }

        private void ScriptsTabTextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            ScriptsTabTextBlock.FontSize = 20;
        }

        private void AdvancedTabTextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            AdvancedTabTextBlock.FontSize = 20;
        }

        

        private async Task<usersettings> FetchSettings()
        {
            string data = "";
            int i = 0;
            usersettings usersettings = new usersettings();
            FieldInfo[] PI = usersettings.GetType().GetFields();

            if ( !File.Exists( Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\dollhouse\\settings.ini" ) )
            {
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\dollhouse"))
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\dollhouse");
                }
                using (FileStream fs = File.Create(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\dollhouse\\settings.ini"))
                {
                    foreach (FieldInfo property in PI)
                    {
                        data += property.Name + '=' + property.GetValue(usersettings) + '\n';
                    }
                    byte[] bytes = Encoding.UTF8.GetBytes(data);
                    await fs.WriteAsync(bytes);
                }
                
                
            }
            using (FileStream fs =  File.Open(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\dollhouse\\settings.ini", System.IO.FileMode.Open))
            {
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer);
                string s = Encoding.UTF8.GetString(buffer);
                List<string> sl = new List<string>(s.Split("\n"));
                List<FieldInfo> fl = new List<FieldInfo>(PI);
                foreach (var field in fl)
                {
                    
                }
            }
            return usersettings;
        }

        

        private async Task<Release> GetLatestVersion()
        {
            var client = new GitHubClient(new ProductHeaderValue("android-dollhouse"));
            var releases = await client.Repository.Release.GetAll("githubcatw", "android-dollhouse");
            var latestRelease = releases[0];
            return latestRelease;
        }
        private async Task<Release> GetLatestUpdaterVersion()
        {
            var client = new GitHubClient(new ProductHeaderValue("android-dollhouse-updater"));
            var releases = await client.Repository.Release.GetAll("dollscythe", "android-dollhouse-updater");
            var latestRelease = releases.Last();
            return latestRelease;
        }
        private async Task<Tuple<string, string>> AutoUpdate()
        {
            //latestbuild object
            Release latestBuild = await GetLatestVersion();
            Release latestUpdaterBuild = await GetLatestUpdaterVersion();

            string latestBuildVersionString = latestBuild.TagName;

            int currentBuildVersion = Int32.Parse(VersionNumber.Where(c => (Char.IsDigit(c))).ToArray());
            int latestBuildVersion = Int32.Parse(new string(latestBuild.TagName.Where(c => (Char.IsDigit(c))).ToArray()), CultureInfo.InvariantCulture);

            if (latestBuildVersion > currentBuildVersion)
            {
                if (MessageBox.Show("A new version is available. It will be downloaded and installed automatically. Proceed?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    //kill adb server so that it doesnt block us later
                    KillServer();

                    //start two processes, one for the updater, the other as a handle to the WPF app process.
                    Process p = new Process();
                    Process currentprocess = Process.GetCurrentProcess();

                    //updater StartInfo
                    p.StartInfo.FileName = "updaterframework.exe";
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.CreateNoWindow = false;

                    //send current ver num and PID to updater
                    p.StartInfo.Arguments = VersionNumber + ";" + currentprocess.Id;
                    try
                    {
                        p.Start();
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        try
                        {
                            using (Stream fileBytes = await downloader.GetStreamAsync(latestUpdaterBuild.Assets[0].BrowserDownloadUrl))
                            {
                                using (Stream streamToWriteTo = File.Open("./updaterframework.exe", System.IO.FileMode.Create))
                                {
                                    fileBytes.CopyTo(streamToWriteTo);
                                }
                            }
                        }
                        catch (HttpRequestException e)
                        {
                            Console.WriteLine("\nException Caught!");
                            Console.WriteLine("Message :{0} ", e.Message);
                        }
                        p.Start();
                    }
                }
            }

            return new Tuple<string, string>(VersionNumber, latestBuildVersionString);
        }

        private async void AcrylicWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await FetchSettings();
            try
            {
                Tuple<string, string> current_latest = await AutoUpdate();
                CurrentVersionText.Text = "Current: " + current_latest.Item1 + " - Latest: " + current_latest.Item2;
            }
            catch (System.Net.Http.HttpRequestException)
            {
            }
            
            
        }

        private void MainWindowBase_Closed(object sender, EventArgs e)
        {

        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Source = new Uri("settings.xaml", UriKind.Relative);
        }
    }
}

#nullable enable


using Microsoft.Win32;

using Microsoft.Windows.Themes;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for logcat.xaml
    /// </summary>
    public partial class logcat
    {

        Process p;
        List<string> buffer;
        bool filterActive;
        ListView temp;
        bool scrollactive;

        public logcat()
        {
            

            InitializeComponent();
            p = new Process();
            buffer = new List<string>();
            filterActive = false;
            temp = new ListView();
            p.StartInfo.FileName = "./res/platform-tools/adb.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.Arguments = "logcat";
            
        }

        private async void Logcat_Closed(object sender, EventArgs e)
        {
            p.OutputDataReceived -= P_OutputDataReceived;
            p.Kill();
            await p.WaitForExitAsync();
            p.Close();
            p.Dispose();
        }

        private void UpdateScrollBar(ListView listView)
        {
            if (listView != null && listView.Items.Count!= 0)
            {
                var border = (Border)VisualTreeHelper.GetChild(listView, 0);
                var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                if (scrollViewer.ScrollableHeight == scrollViewer.VerticalOffset)
                {
                    scrollViewer.ScrollToBottom();
                }
                
            }

        }
        public void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            buffer.Add(e.Data);
            Dispatcher.Invoke(() =>
            {
            if (e.Data.ToLower().Contains(FilterTextBox.Text.ToLower()) && filterActive == false)
             {
                    
              logcatListView.Items.Add(e.Data);
                    if (scrollactive == true)
                    {

                        UpdateScrollBar(logcatListView);
                    }

                }
            else if (filterActive==true)
            {
                            
            }
            
            
                
            });
            
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (FilterTextBox.Text.Length == 0)
            {
                filterActive = false;
            }
            else
            {
                filterActive = true;
            }
            logcatListView.ItemsSource = null;
            logcatListView.Items.Clear();

            foreach (string item in buffer.ToList<string>())
            {
                if (item.ToLower().Contains(FilterTextBox.Text.ToLower()))
                {
                    logcatListView.Items.Add(item);
                }
            }

            filterActive = false;
            UpdateScrollBar(logcatListView);
        }

        private async void AcrylicWindow_Loaded(object sender, RoutedEventArgs e)
        {
            p.OutputDataReceived += P_OutputDataReceived;

            p.Start();
            p.BeginOutputReadLine();
            await Task.Delay(1500);
            scrollactive = true;
            try
            {
                var border = (Border)VisualTreeHelper.GetChild(logcatListView, 0);
                var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
            catch (InvalidCastException)
            {
                var border = (ClassicBorderDecorator)VisualTreeHelper.GetChild(logcatListView, 0);
                var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }

        }

        private void SaveLogButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = "logcat";
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllLines(saveFileDialog.FileName, buffer);
            }
        }
    }
}

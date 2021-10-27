using System;
using System.Collections.Generic;
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
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for filesystem.xaml
    /// </summary>
    public partial class filesystem
    {
        public filesystem()
        {
            this.FontFamily = new FontFamily("Product Sans");
            InitializeComponent();
        }

        private void adbPullSaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "./res/platform-tools/adb.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;

            p.StartInfo.Arguments = "shell \"cat " + adbPullPathTextBox.Text.Trim() + " > /sdcard/temp\"" ;
            p.Start();
            p.WaitForExit();
            
            


            p.StartInfo.Arguments = "pull /sdcard/temp ./";
            p.Start();
            p.Dispose();
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                File.Move("./temp", saveFileDialog.FileName);
            }
        }
    }
}

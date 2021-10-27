using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for decrypt.xaml
    /// </summary>
    public partial class decrypt
    {
        public decrypt()
        {
            InitializeComponent();
        }

        private void twrpDecryptButton_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "./res/platform-tools/adb.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.Arguments = "shell \'twrp\' decrypt " + decryptTwrpPinTextBox.Text + " " + decryptTwrpUserTextBox.Text ;
            p.Start();
            p.WaitForExit();
            string output = p.StandardError.ReadToEnd();
            p.Kill();
            p.StartInfo.Arguments = "shell \'twrp\' remountrw system";
            p.Start();
            p.WaitForExit();
            p.Kill();
            p.Dispose();
            this.Close();
        }
    }
}

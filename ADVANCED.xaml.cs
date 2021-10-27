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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for ADVANCED.xaml
    /// </summary>
    public partial class ADVANCED : Page
    {
        public ADVANCED()
        {
            InitializeComponent();
        }

        private void cmdButton_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "C:/Windows/System32/cmd.exe";
            p.StartInfo.WorkingDirectory = new Uri("./res/platform-tools", UriKind.Relative).ToString();
            p.StartInfo.UseShellExecute = true ;
            p.StartInfo.CreateNoWindow = false;
            p.Start();
        }
    }
}

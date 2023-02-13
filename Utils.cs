using System.Diagnostics;

namespace WpfApp1 {
    internal class Utils {
        public static void RunFastboot(string command) {
            Process p = new Process();
            p.StartInfo.FileName = "./res/platform-tools/fastboot.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.Arguments = command;
            p.Start();
            p.WaitForExit();
            p.Kill();
            p.Dispose();
        }
    }
}
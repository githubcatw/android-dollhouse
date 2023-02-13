using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace WpfApp1
{
    public class PluggedDevice
    {
        public string Model = "No Device Detected.";
        public string Name ="";
        public string Storage = "";
        public string IMEI = "";
        public string Serial = "";
        [Obsolete("Please use CurrentMode instead")]
        public string CurrentModeString = "Unplugged";
        public PluggedDeviceMode CurrentMode = PluggedDeviceMode.None;
        public string CurrentSlot = "";
        public bool IsInBootloader = false;
        public string CurrentVendor = "";
        public string SELinuxStatus = "";
        public string OSVersion = "";
        public bool IsSamsung = false;
        public bool IsHMOS = false;
        public string KnoxBit = "";

        public PluggedDevice(Button b)
        {
            Process p = new Process();
            p.StartInfo.FileName = "./res/platform-tools/adb.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.Arguments = "devices -l";
            b.Content = "Refreshing";
            //Get current mode
            p.Start();
            p.WaitForExitAsync();
            string output = p.StandardOutput.ReadToEnd().Remove(0,26).Trim();
            output = Regex.Replace(output, @"\s+", " ");
            string[] data = output.Split(" ").ToArray();
            try {
                Serial = data[0];
                if (data[1] == "device") {
                    CurrentModeString = "System";
                    CurrentMode = PluggedDeviceMode.System;
                }
                else if (data[1] == "recovery") {
                    CurrentModeString = "Recovery";
                    CurrentMode = PluggedDeviceMode.Recovery;
                }
            }
            catch (IndexOutOfRangeException) {
                p.WaitForExit();
                p.Kill();

                p.StartInfo.FileName = "./res/platform-tools/fastboot.exe";
                p.StartInfo.Arguments = "devices";
                p.Start();

                output = p.StandardOutput.ReadToEnd().Trim();
                output = Regex.Replace(output, @"\s+", " ");
                data = output.Split(" ").ToArray();
                try {
                    Serial = data[0];
                    if (data[1] == "fastboot") {
                        CurrentMode = PluggedDeviceMode.Bootloader;
                    }

                }
                catch (IndexOutOfRangeException) {
                    Serial = "";
                    Model = "";
                    Name = "No Device Detected.";
                }

            }
            p.WaitForExitAsync();

            //get product.name(codename)
            p.StartInfo.FileName = "./res/platform-tools/adb.exe";
            p.StartInfo.Arguments = "shell getprop ro.product.name";
            p.Start();
            Name = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExitAsync();

            //get build.version
            p.StartInfo.FileName = "./res/platform-tools/adb.exe";
            p.StartInfo.Arguments = "shell getprop ro.build.version.release";
            p.Start();
            OSVersion = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExitAsync();

            //get SDK version
            p.StartInfo.FileName = "./res/platform-tools/adb.exe";
            p.StartInfo.Arguments = "shell getprop ro.build.version.sdk";
            p.Start();
            var sdk = p.StandardOutput.ReadToEnd().Trim();
            OSVersion += "(API " + sdk + ")";
            p.WaitForExitAsync();

            //get product.model

            p.StartInfo.Arguments = "shell getprop ro.product.model";
            p.Start();
            Model = p.StandardOutput.ReadToEnd().Trim();
            if (Model.StartsWith("SM-")) {
                IsSamsung = true;

                // Detect warranty bit
                var bit = GetProp("ro.boot.warranty_bit");
                if (bit == "") {
                    bit = GetProp("ro.warranty_bit");
                }
                KnoxBit = bit;
            }

            // TODO better HarmonyOS detection (I don't have a Huawei device)
            // This method assumes that every Huawei device running Android 10 is running HarmonyOS
            p.StartInfo.Arguments = "shell getprop ro.build.fingerprint";
            p.Start();
            var fp = p.StandardOutput.ReadToEnd().Trim();
            if (fp.StartsWith("HUAWEI") && sdk == "29") {
                IsHMOS = true;
            }

            if (CurrentMode != PluggedDeviceMode.Bootloader)
            {
                //get selinux
                p.StartInfo.FileName = "./res/platform-tools/adb.exe";
                p.StartInfo.Arguments = "shell";
                p.Start();
                p.StandardInput.WriteLine("getenforce");
                p.StandardInput.WriteLine("exit");
                output = p.StandardOutput.ReadToEnd().Trim();
                if (output == "") {
                    p.Start();
                    p.StandardInput.WriteLine("su");
                    p.StandardInput.WriteLine("getenforce");
                    p.StandardInput.WriteLine("exit");
                    output = p.StandardOutput.ReadToEnd().Trim();
                    SELinuxStatus = output == "" ? "Requires root" : output;
                }
                else {
                    SELinuxStatus = output;
                }
                p.WaitForExitAsync();

                //get imei
                if (CurrentMode == PluggedDeviceMode.Recovery) {
                    IMEI = "Inaccessible in TWRP";
                }
                else if (CurrentMode == PluggedDeviceMode.System) {
                    p.Start();
                    p.StandardInput.WriteLine("service call iphonesubinfo 1 | cut -c 52-66 | tr -d '.[:space:]'");
                    p.StandardInput.WriteLine("exit");
                    output = p.StandardOutput.ReadToEnd().Trim();
                    if (output == "')") IMEI = "Inaccessible";
                    else IMEI = output;
                    p.WaitForExitAsync();
                }
                
                

                //get storage
                p.StartInfo.FileName = "./res/platform-tools/adb.exe";
                p.StartInfo.Arguments = "shell";
                p.Start();
                p.StandardInput.WriteLine("df -h | grep '/data'");
                p.StandardInput.WriteLine("exit");
                output = p.StandardOutput.ReadToEnd().Trim();
                output = Regex.Replace(output, @"\s+", " ");
                data = output.Split(" ").ToArray();
                try {
                    Storage = data[1] + 'B';
                    Storage = new string(Storage.Where(Char.IsDigit).ToArray());
                    switch (int.Parse(Storage)) {
                        case <= 32:
                            Storage = "32GB";
                            break;
                        case <= 64 and > 32:
                            Storage = "64GB";
                            break;
                        case <= 128 and > 64:
                            Storage = "128GB";
                            break;
                        case <= 256 and > 128:
                            Storage = "256GB";
                            break;
                    }

                }
                catch (IndexOutOfRangeException) {
                    Storage = "Can't detect storage.";
                }
                p.WaitForExitAsync();


                //get boot slot
                if (CurrentMode != PluggedDeviceMode.Bootloader) {
                    p.StartInfo.FileName = "./res/platform-tools/adb.exe";
                    p.StartInfo.Arguments = "shell /bin/getprop ro.boot.slot_suffix";
                }
                else {
                    p.StartInfo.FileName = "./res/platform-tools/fastboot.exe";
                    p.StartInfo.Arguments = "getvar current-slot";
                }
                p.Start();
                output = p.StandardOutput.ReadToEnd().Trim().Replace("_", "");
                CurrentSlot = output;
                p.WaitForExitAsync();
            }
            else {
                p.StartInfo.FileName = "./res/platform-tools/fastboot.exe";
                p.StartInfo.Arguments = "getvar product";
                p.Start();
                output = p.StandardError.ReadLine().Trim();
                data = output.Split(" ").ToArray();
                Model = data[1];
                Name = "Bootloader Mode";
                p.WaitForExitAsync();

                //get slot in fastboot
                p.StartInfo.FileName = "./res/platform-tools/fastboot.exe";
                p.StartInfo.Arguments = "getvar current-slot";
                p.Start();
                output = p.StandardError.ReadToEnd().Trim().Replace("_", "");
                data = output.Replace("\r"," ").Replace("\n","").Split(" ").ToArray();
                
                CurrentSlot = data[1];
                p.WaitForExitAsync();
            }
            
            p.WaitForExitAsync();
            p.Kill();
            b.Content = "Refresh";
        }

        string GetProp(string prop) {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "./res/platform-tools/adb.exe";
            p.StartInfo.Arguments = "shell getprop " + prop;
            p.Start();
            var ret = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExitAsync();
            return ret;
        }


    }
}

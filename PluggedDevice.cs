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
        public string SDK = "";
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
                        CurrentModeString = "Bootloader";
                        CurrentMode = PluggedDeviceMode.Bootloader;
                    }

                }
                catch (IndexOutOfRangeException) {
                    Serial = "";
                    Model = "";
                    Name = "No Device Detected.";
                }

                // Reset the filename to ADB
                p.StartInfo.FileName = "./res/platform-tools/adb.exe";
            }
            p.WaitForExitAsync();

            // get product.name(codename)
            Name = GetProp("ro.product.name", p);

            // get build.version
            OSVersion = GetProp("ro.build.version.release", p);

            // get SDK version
            SDK = GetProp("ro.build.version.sdk", p);
            OSVersion += "(API " + SDK + ")";

            // get product.model
            Model = GetProp("ro.product.model", p);

            // get any additional props
            DetectVendorSpecificParams(p);

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

        private void DetectVendorSpecificParams(Process p) {
            // Detect Samsung devices
            if (Model.StartsWith("SM-")) {
                IsSamsung = true;

                // Detect warranty bit
                var bit = GetProp("ro.boot.warranty_bit", p);
                if (bit == "") {
                    bit = GetProp("ro.warranty_bit", p);
                }
                KnoxBit = bit;
            }

            // TODO better HarmonyOS detection (I don't have a Huawei device)
            // This method assumes that every Huawei device running Android 10 is running HarmonyOS
            var fp = GetProp("ro.build.fingerprint", p);
            if (fp.StartsWith("HUAWEI") && SDK == "29") {
                IsHMOS = true;
            }
        }

        string GetProp(string prop, Process p = null) {
            if (p == null) {
                p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "./res/platform-tools/adb.exe";
            }
            p.StartInfo.Arguments = "shell getprop " + prop;
            p.Start();
            var ret = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExitAsync();
            return ret;
        }
    }
}

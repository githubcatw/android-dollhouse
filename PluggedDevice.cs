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
        /// <summary>
        /// The device's model.
        /// </summary>
        public string Model = "No Device Detected.";
        /// <summary>
        /// The device's codename.
        /// </summary>
        public string Name = "";
        /// <summary>
        /// The device's vendor.
        /// </summary>
        public string CurrentVendor = "";

        /// <summary>
        /// The device's storage capacity.
        /// </summary>
        public string Storage = "";

        /// <summary>
        /// The device's IMEI, if available.
        /// </summary>
        public string IMEI = "";
        /// <summary>
        /// The device's serial number, if available.
        /// </summary>
        public string Serial = "";

        [Obsolete("Please use CurrentMode instead")]
        public string CurrentModeString = "Unplugged";

        /// <summary>
        /// The device's mode.
        /// </summary>
        public PluggedDeviceMode CurrentMode = PluggedDeviceMode.None;
        /// <summary>
        /// The device's Android version, if available.
        /// </summary>
        public string OSVersion = "";
        /// <summary>
        /// The device's Android SDK level, if available.
        /// </summary>
        public string SDK = "";

        /// <summary>
        /// The device's slot, if it uses A/B partitioning.
        /// </summary>
        public string CurrentSlot = "";
        /// <summary>
        /// The device's SELinux status.
        /// </summary>
        public string SELinuxStatus = "";

        /// <summary>
        /// Is this device a Samsung?
        /// </summary>
        public bool IsSamsung = false;
        /// <summary>
        /// Is this device running HarmonyOS?
        /// </summary>
        public bool IsHMOS = false;

        /// <summary>
        /// If this device is a Samsung, the device's Knox warranty bit.
        /// </summary>
        public string KnoxBit = "";

        public PluggedDevice(Button b)
        {
            Process p = new Process();
            p.StartInfo.FileName = "./res/platform-tools/adb.exe";
            SetupProcess(p);

            b.Content = "Refreshing";
            string output;
            string[] data;

            // Get current mode
            DetectMode(p, out output, out data);

            // Get product.name(codename)
            Name = GetProp("ro.product.name", p);

            // Get build.version
            OSVersion = GetProp("ro.build.version.release", p);

            // Get SDK version
            SDK = GetProp("ro.build.version.sdk", p);
            OSVersion += "(API " + SDK + ")";

            // Get product.model
            Model = GetProp("ro.product.model", p);

            // Get any additional props
            DetectVendorSpecificParams(p);

            // Reset the filename to ADB
            p.StartInfo.FileName = "./res/platform-tools/adb.exe";

            if (CurrentMode != PluggedDeviceMode.Bootloader) {
                //get selinux
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
                    int storageBytes = int.Parse(new string(Storage.Where(char.IsDigit).ToArray()));
                    switch (storageBytes) {
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
                p.StartInfo.Arguments = "getvar current-slot";
                p.Start();
                output = p.StandardError.ReadToEnd().Trim().Replace("_", "");
                data = output.Replace("\r", " ").Replace("\n", "").Split(" ").ToArray();

                CurrentSlot = data[1];
                p.WaitForExitAsync();
            }

            p.WaitForExitAsync();
            p.Kill();
            b.Content = "Refresh";
        }

        private void DetectMode(Process p, out string output, out string[] data) {
            p.StartInfo.Arguments = "devices -l";
            p.Start();
            p.WaitForExitAsync();
            output = p.StandardOutput.ReadToEnd().Remove(0, 26).Trim();
            output = Regex.Replace(output, @"\s+", " ");
            data = output.Split(" ").ToArray();
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
                p.WaitForExitAsync();

                // Reset the filename to ADB
                p.StartInfo.FileName = "./res/platform-tools/adb.exe";
            }
        }

        private static void SetupProcess(Process p)
        {
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
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

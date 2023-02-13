using Microsoft.Win32;
using SourceChord.FluentWPF;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MAIN.xaml
    /// </summary>
    public partial class MAIN : Page
    {

        public MAIN()
        {
            this.KeepAlive = true;
            this.FontFamily = new FontFamily("Product Sans");
            InitializeComponent();
            fetchData();
        }

        private async void fetchData()
        {
            MainWindow.adbDevice = new PluggedDevice(DeviceInfoRefresh);
            serial.Text = MainWindow.adbDevice.Serial;
            imei.Text = MainWindow.adbDevice.IMEI;
            storage.Text = MainWindow.adbDevice.Storage;
            model.Text = MainWindow.adbDevice.Model;
            name.Text = MainWindow.adbDevice.Name;
            selinux.Text = MainWindow.adbDevice.SELinuxStatus;

            if (MainWindow.adbDevice.CurrentMode == PluggedDeviceMode.None)
            {
                streamScreenButton.IsEnabled=false;

                deviceImage.Source = null;

                DeviceModeSwitchButton.IsEnabled = false;

                adbConnectButton.IsEnabled = true;
                adbPushButton.IsEnabled = false;
                adbPullButton.IsEnabled = false;
                adbInstallButton.IsEnabled = false;
                adbLogcatButton.IsEnabled = false;

                partitionFlashButton.IsEnabled = false;
                partitionWipeButton.IsEnabled = false;
                slotSwitchButton.IsEnabled = false;
                dataPartitionFormatButton.IsEnabled = false;
                bootImgButton.IsEnabled = false;
                slotASwitch.IsEnabled = false;
                slotBSwitch.IsEnabled = false;
                bootPartitionRadioButton.IsEnabled = false;
                systemPartitionRadioButton.IsEnabled = false;
                vendorPartitionRadioButton.IsEnabled = false;

                firmwareInstallButton.IsEnabled = false;


                currentSlotLabel.Text = "Current Slot:";
                storage.Text = "";
                selinux.Text = "";

                DecryptTwrpButton.IsEnabled = false;
                FlashZipTwrpButton.IsEnabled = false;
                FormatDataTwrpButton.IsEnabled = false;
                sideloadTwrpButton.IsEnabled = false;

            }

            if (MainWindow.adbDevice.CurrentMode == PluggedDeviceMode.System) {
                streamScreenButton.IsEnabled = true;

                updateImage();

                DeviceModeSwitchButton.IsEnabled = true;

                adbConnectButton.IsEnabled = true;
                adbPushButton.IsEnabled = true;
                adbPullButton.IsEnabled = true;
                adbInstallButton.IsEnabled = true;
                adbLogcatButton.IsEnabled = true;

                partitionFlashButton.IsEnabled = false;
                partitionWipeButton.IsEnabled = false;
                slotSwitchButton.IsEnabled = false;
                dataPartitionFormatButton.IsEnabled = false;
                slotASwitch.IsEnabled = false;
                slotBSwitch.IsEnabled = false;
                bootPartitionRadioButton.IsEnabled = false;
                systemPartitionRadioButton.IsEnabled = false;
                vendorPartitionRadioButton.IsEnabled = false;
                bootImgButton.IsEnabled = false;

                firmwareInstallButton.IsEnabled = false;

                DecryptTwrpButton.IsEnabled = false;
                FlashZipTwrpButton.IsEnabled = false;
                FormatDataTwrpButton.IsEnabled = false;
                sideloadTwrpButton.IsEnabled = false;
                if (MainWindow.adbDevice.IsSamsung)
                    bootloaderSwitch.Content = "Download Mode";
                else
                    bootloaderSwitch.Content = "Bootloader Mode";
            }

            else if (MainWindow.adbDevice.CurrentMode == PluggedDeviceMode.Bootloader)
            {
                streamScreenButton.IsEnabled = false;
                DeviceModeSwitchButton.IsEnabled = true;

                adbConnectButton.IsEnabled = false;
                adbPushButton.IsEnabled = false;
                adbPullButton.IsEnabled = false;
                adbInstallButton.IsEnabled = false;
                adbLogcatButton.IsEnabled = false;

                firmwareInstallButton.IsEnabled = false;

                partitionFlashButton.IsEnabled = true;
                partitionWipeButton.IsEnabled = true;
                slotSwitchButton.IsEnabled = true;
                dataPartitionFormatButton.IsEnabled = true;
                bootImgButton.IsEnabled = true;
                slotASwitch.IsEnabled = true;
                slotBSwitch.IsEnabled = true;
                bootPartitionRadioButton.IsEnabled = true;
                systemPartitionRadioButton.IsEnabled = true;
                vendorPartitionRadioButton.IsEnabled = true;

                DecryptTwrpButton.IsEnabled = false;
                FlashZipTwrpButton.IsEnabled = false;
                FormatDataTwrpButton.IsEnabled = false;
                sideloadTwrpButton.IsEnabled = false;

            }

            else if (MainWindow.adbDevice.CurrentMode == PluggedDeviceMode.Recovery)
            {
                streamScreenButton.IsEnabled = false;

                updateImage();

                DeviceModeSwitchButton.IsEnabled = true;

                adbConnectButton.IsEnabled = true;
                adbPushButton.IsEnabled = true;
                adbPullButton.IsEnabled = true;
                adbInstallButton.IsEnabled = true;
                adbLogcatButton.IsEnabled = true;

                partitionFlashButton.IsEnabled = false;
                partitionWipeButton.IsEnabled = false;
                slotSwitchButton.IsEnabled = false;
                dataPartitionFormatButton.IsEnabled = false;
                bootImgButton.IsEnabled = false;
                slotASwitch.IsEnabled = false;
                slotBSwitch.IsEnabled = false;
                bootPartitionRadioButton.IsEnabled = false;
                systemPartitionRadioButton.IsEnabled = false;
                vendorPartitionRadioButton.IsEnabled = false;

                firmwareInstallButton.IsEnabled = false;

                DecryptTwrpButton.IsEnabled = true;
                FlashZipTwrpButton.IsEnabled = true;
                FormatDataTwrpButton.IsEnabled = true;
                sideloadTwrpButton.IsEnabled = true;

            }

            if (MainWindow.adbDevice.CurrentSlot == "") {
                currentSlotLabel.Text = "Device is A-only";
                slotASwitch.Visibility = slotBSwitch.Visibility = slotSwitchButton.Visibility = Visibility.Collapsed;
            } else {
                if (MainWindow.adbDevice.CurrentSlot == "b")
                    currentSlotLabel.Text = "Current Slot: B";
                else if (MainWindow.adbDevice.CurrentSlot == "a")
                    currentSlotLabel.Text = "Current Slot: A";

                slotASwitch.Visibility = slotBSwitch.Visibility = slotSwitchButton.Visibility = Visibility.Visible;
            }

        }

        private void updateImage() {
            if (MainWindow.adbDevice.Name.Contains("marlin")
             || MainWindow.adbDevice.Name.Contains("sailfish")) {          /* P1/P1 XL */
                setImageOrGeneric("./res/pixel1.png");
            }
            else if (MainWindow.adbDevice.Name.Contains("walleye")) {    /* P2 */
                setImageOrGeneric("./res/walleye.png");
            }
            else if (MainWindow.adbDevice.Name.Contains("taimen")) {     /* P2 XL */
                setImageOrGeneric("./res/taimen.png");
            }
            else if (MainWindow.adbDevice.Name.Contains("blueline")) {   /* P3 */
                setImageOrGeneric("./res/pixel3.png");
            }
            else if (MainWindow.adbDevice.Name.Contains("crosshatch")) { /* P3 XL */
                setImageOrGeneric("./res/pixel3xl.png");
            }
            else if (MainWindow.adbDevice.Name.Contains("sargo")
                  || MainWindow.adbDevice.Name.Contains("bonito")) {     /* P3a/P3a XL */
                setImageOrGeneric("./res/pixel3a.png");
            }
            else if (MainWindow.adbDevice.Name.Contains("flame")) {      /* P4 */
                setImageOrGeneric("./res/pixel4.png");
            }
            else if (MainWindow.adbDevice.Name.Contains("coral")) {      /* P4 XL */
                setImageOrGeneric("./res/pixel4xl.png");
            }
            else if (MainWindow.adbDevice.Name.Contains("redfin")) {     /* P5 */
                setImageOrGeneric("./res/pixel5.png");
            }
            else if (MainWindow.adbDevice.Name.Contains("barbet")) {     /* P5a */
                setImageOrGeneric("./res/barbet.png");
            }
            else if (MainWindow.adbDevice.Name.Contains("oriole")) {     /* P6 */
                setImageOrGeneric("./res/oriole.png");
            }
            else if (MainWindow.adbDevice.Name.Contains("raven")) {      /* P6 Pro */
                setImageOrGeneric("./res/raven.png");
            }
            else if (MainWindow.adbDevice.Name.Contains("bluejay")) {    /* P6a */
                setImageOrGeneric("./res/bluejay.png");
            }
            else if (MainWindow.adbDevice.Name.Contains("cheetah")) {    /* P7 */
                setImageOrGeneric("./res/cheetah.png");
            }
            else if (MainWindow.adbDevice.Name.Contains("panther")) {    /* P7 Pro */
                setImageOrGeneric("./res/panther.png");
            }
            else setGeneric();
        }

        private void setImageOrGeneric(string path) {
            if (System.IO.File.Exists(path)) {
                deviceImage.Source = new BitmapImage(new Uri(path, UriKind.Relative));
            }
            else setGeneric();
        }

        private void setGeneric() {
            string uri;
            if (MainWindow.adbDevice.IsSamsung)
                uri = "./res/generic_samsung.png";
            else if (MainWindow.adbDevice.IsHMOS)
                uri = "./res/generic_hmos.png";
            else
                uri = "./res/generic.png";
            deviceImage.Source = new BitmapImage(new Uri(uri, UriKind.Relative));
        }

        private void DeviceInfoRefresh_Click(object sender, RoutedEventArgs e)
        {
            fetchData();
        }

        private void reboot_button_click(object sender, RoutedEventArgs e)
        {
            fetchData();
            if ((bool)recoverySwitch.IsChecked)
            {
                if (MainWindow.adbDevice.CurrentMode != PluggedDeviceMode.Bootloader)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "./res/platform-tools/adb.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.Arguments = "reboot recovery";
                    p.Start();
                    p.WaitForExit();
                    p.Kill();
                    p.Dispose();
                    return;
                }
                else
                {
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.FileName = "./res/platform-tools/fastboot.exe";
                    p.StartInfo.Arguments = "reboot recovery";
                    p.Start();
                    p.WaitForExit();
                    return;
                }
            }
            else if ((bool)bootloaderSwitch.IsChecked)
            {
                if (MainWindow.adbDevice.CurrentMode != PluggedDeviceMode.Bootloader)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "./res/platform-tools/adb.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.Arguments = "reboot bootloader";
                    p.Start();
                    p.WaitForExit();
                    p.Kill();
                    p.Close();
                    p.Dispose();
                    return;
                }
                else
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "./res/platform-tools/fastboot.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.Arguments = "reboot bootloader";
                    p.Start();
                    p.WaitForExit();
                    p.Kill();
                    p.Close();
                    p.Dispose();
                    return;
                }
            }
            else if ((bool)systemSwitch.IsChecked)
            {
                if (MainWindow.adbDevice.CurrentMode != PluggedDeviceMode.Bootloader)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "./res/platform-tools/adb.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.Arguments = "reboot system";
                    p.Start();
                    p.WaitForExit();
                    p.Kill();
                    p.Close();
                    p.Dispose();
                    return;
                }
                else
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "./res/platform-tools/fastboot.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.Arguments = "reboot";
                    p.Start();
                    p.WaitForExit();
                    p.Kill();
                    p.Close();
                    p.Dispose();
                    return;
                }
            }
        }

        private void slotSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            fetchData();
            if (MainWindow.adbDevice.CurrentMode != PluggedDeviceMode.Bootloader)
            {
                MessageBox.Show(Application.Current.MainWindow, "You must reboot to bootloader mode before using this.");
                return;
            }
            else
            {
                if ((bool)slotASwitch.IsChecked)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "./res/platform-tools/fastboot.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.Arguments = "--set-active=a";
                    p.Start();
                    p.WaitForExit();
                    fetchData();
                }
                else if ((bool)slotBSwitch.IsChecked)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "./res/platform-tools/fastboot.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.Arguments = "--set-active=b";
                    p.Start();
                    p.WaitForExit();
                    fetchData();
                }
                else
                {
                    MessageBox.Show(Application.Current.MainWindow,"Select a slot.");
                    return;
                }

            }
        }

        async private void adbConnectButton_Click(object sender, RoutedEventArgs e)
        {
            adbConnectButton.Content = "Restarting adb daemon...";
            adbConnectButton.IsEnabled = false;

            fetchData();
            if (MainWindow.adbDevice.CurrentMode != PluggedDeviceMode.Bootloader)
            {
                Process p = new Process();
                p.StartInfo.FileName = "./res/platform-tools/adb.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.Arguments = "kill-server";
                p.Start();
                await p.WaitForExitAsync();
                p.StartInfo.Arguments = "start-server";
                p.Start();
                await p.WaitForExitAsync();
                adbConnectButton.Content = "Connect";
                adbConnectButton.IsEnabled = true;
            }
            else
            {
                return;
            }
        }

        private void adbPushButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Process p = new Process();
                p.StartInfo.FileName = "./res/platform-tools/adb.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.Arguments = "push \"" + openFileDialog.FileName + "\" /sdcard";
                p.Start();
                p.WaitForExit();
                p.Kill();
                p.Dispose();
            }
        }



        private void adbPullButton_Click(object sender, RoutedEventArgs e)
        {
            filesystem filesystem = new();
            filesystem.Show();
        }



        private void adbLogcatButton_Click(object sender, RoutedEventArgs e)
        {
            logcat window = new logcat();
            window.Show();
        }

        async private void streamScreenButton_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "./res/platform-tools/scrcpy.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            await p.WaitForExitAsync();
            p.Kill();
            p.Dispose();
        }

        private void adbInstallButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Process p = new Process();
                p.StartInfo.FileName = "./res/platform-tools/adb.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.Arguments = "install -r -d \"" + openFileDialog.FileName + "\"";
                p.Start();
                p.WaitForExit();
                p.Kill();
                p.Dispose();
            }
        }

        private void partitionFlashButton_Click(object sender, RoutedEventArgs e)
        {
            if (systemPartitionRadioButton.IsChecked == true) {
                FastbootFlash("system");
            }
            else if (bootPartitionRadioButton.IsChecked == true) {
                FastbootFlash("boot");
            }
            else if (vendorPartitionRadioButton.IsChecked == true) {
                FastbootFlash("vendor");
            }
            else {
                MessageBox.Show(Application.Current.MainWindow,"Please select partition.", "Partition not selected");
                return;
            }
        }

        private static void FastbootFlash(string partition) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = $"Select {partition} image";
            if (openFileDialog.ShowDialog() == true) {
                Utils.RunFastboot($"flash {partition} \"{openFileDialog.FileName}\"");
            }
        }

        private void partitionWipeButton_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "./res/platform-tools/fastboot.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;

            if (MessageBox.Show("Are you sure? Partition cannot be recovered if erased.","Warning",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (systemPartitionRadioButton.IsChecked == true)
                {
                    p.StartInfo.Arguments = "erase system";
                    p.Start();
                    p.WaitForExit();
                    p.Kill();
                    p.Dispose();
                }
                else if (bootPartitionRadioButton.IsChecked == true)
                {
                    p.StartInfo.Arguments = "erase boot";
                    p.Start();
                    p.WaitForExit();
                    p.Kill();
                    p.Dispose();
                }
                else if (vendorPartitionRadioButton.IsChecked == true)
                {
                    p.StartInfo.Arguments = "erase vendor";
                    p.Start();
                    p.WaitForExit();
                    p.Kill();
                    p.Dispose();
                }
                else
                {
                    MessageBox.Show(Application.Current.MainWindow, "Please select partition.", "Partition not selected");
                    return;
                }
            }
            
        }

        private void dataPartitionFormatButton_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "./res/platform-tools/fastboot.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.Arguments = "-w";
            p.Start();
            p.WaitForExit();
            p.Kill();
            p.Dispose();
        }

        private void bootImgButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Process p = new Process();
                p.StartInfo.FileName = "./res/platform-tools/fastboot.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.Arguments = "boot \"" + openFileDialog.FileName + "\"";
                p.Start();
                p.WaitForExit();
                string output = p.StandardOutput.ReadToEnd();
                output = p.StandardError.ReadToEnd();
                p.Kill();
                p.Dispose();
            }
        }
        private void DecryptTwrpButton_Click(object sender, RoutedEventArgs e)
        {
            decrypt decrypt = new();
            decrypt.Show();
        }

        private void FlashZipTwrpButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Process p = new Process();
                p.StartInfo.FileName = "./res/platform-tools/adb.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.Arguments = "push \"" + openFileDialog.FileName + "\" /sdcard";
                p.Start();
                p.WaitForExit();
                string output = p.StandardOutput.ReadToEnd();
                output = p.StandardError.ReadToEnd();
                p.Kill();
                p.StartInfo.Arguments = "shell \'twrp\' install /sdcard/" + openFileDialog.SafeFileName;                
                p.Start();
                p.WaitForExit();
                output = p.StandardOutput.ReadToEnd();
                output = p.StandardError.ReadToEnd();
                p.Kill();
                
                p.Dispose();
            }
        }

        private void FormatDataTwrpButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Application.Current.MainWindow, "Are you sure you want to format your data partition? (This will format /sdcard too)", "Format data?",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Process p = new Process();
                p.StartInfo.FileName = "./res/platform-tools/adb.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.Arguments = "shell 'twrp' format data";
                p.Start();
                p.WaitForExit();
                string output = p.StandardOutput.ReadToEnd();
                output += p.StandardError.ReadToEnd();
                p.Kill();
                p.Dispose();
            }
        }

        private void sideloadTwrpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Application.Current.MainWindow, "Not working yet.");
        }
    }
}

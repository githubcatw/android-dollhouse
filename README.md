# Android Dollhouse
ADB &amp; Fastboot GUI/wrapper. Optimized for Google Pixel devices, but works great with any USB Debuggable android device.

This branch is an updated version of [the original](https://github.com/dollscythe/android-dollhouse) as it has been dead for a while.

## Download
Please note that:
1. You need to enable USB debugging on your Android device to be able to use this.
2. You will need .NET 6.0 x86 desktop runtimes.

Downloads: https://github.com/githubcatw/android-dollhouse/releases
## Building
Sorry about the spaghetti code.

Source has also been stripped of proprietary binaries, so if you're building from source you should copy them from the latest release or obtain them yourself:
- [scrcpy](https://github.com/Genymobile/scrcpy), version doesn't matter, to `res/platform-tools/`,
- [platform-tools](https://dl.google.com/android/repository/platform-tools-latest-windows.zip), version doesn't matter, to `res/platform-tools/`,
- [Magisk](https://github.com/topjohnwu/Magisk), version 23.0, to `res/platform-tools/Magisk-23.0.zip`,
- [dm-verity disabler](https://zackptg5.com/downloads/archive/Disable_Dm-Verity_ForceEncrypt_11.02.2020.zip), version 11.02.2020, to `res/platform-tools/Disable_Dm-Verity_ForceEncrypt_11.02.2020.zip`,
- optionally the proprietary Pixel device images from the latest release (if they're absent the program uses generic images).

Please open an issue if it doesn't function properly.

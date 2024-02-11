using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;

namespace 電車でGO
{
    public class DeviceFinder
    {
        public enum SupportedDevice
        {
            TCPP20011
        }

        public static UsbDeviceFinder FinderForDevice(SupportedDevice device)
        {
            int vid, pid;
            switch (device)
            {
                case SupportedDevice.TCPP20011:
                    vid = 新幹線専用コントローラ.Vid;
                    pid = 新幹線専用コントローラ.Pid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(device), device, null);
            }

            return new UsbDeviceFinder(vid, pid);
        }

        public static UsbDevice FindDevice(SupportedDevice device)
        {
            var usbDeviceFinder = FinderForDevice(device);

            return UsbDevice.OpenUsbDevice(usbDeviceFinder);
        }
    }
}

# 電車でGO LibUsbDotNet

LibUsbDotNet based access to Taito's 電車でGO新幹線 controller.

This project is based on [Tralsys](https://github.com/Tralsys/TR.BIDSsvMOD.dgoCtrlUSB)'s mod for a PC train simulator in order to add support for the 電車でGO新幹線, a train controller made by Taito for the Playstation 2 lucky enough to be built to use USB. [Tralsys](https://github.com/Tralsys/TR.BIDSsvMOD.dgoCtrlUSB)'s mod was originally based on a post and driver found here ([電車でGO新幹線コントローラ : 酔いどれ野郎の日記帳](http://www.mnw-i.net/blog/2013/02/go.html)) but whose driver is no longer available 😭

So here is a gussied up Windows dll/source interface to control the device with an example application included.

## How to Use

Because this interface relies on LibUsb (DotNet) one first has to device to play nice with Windows and libusb-win32/WinUSB, etc. I had luck with [Zadig](https://zadig.akeo.ie/).

Next pull the source, build and have fun!

## Plans

The plan is to eventually forward it to a useful general location like XInput or the like. Especially at that point it would be lovely to have this as a standalone installation, driver, etc.

## LICENSE

LGPL v2.1

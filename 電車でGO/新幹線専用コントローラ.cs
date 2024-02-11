using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Threading;

// based on TR.BIDSsvMOD.dgoCtrlUSB
namespace 電車でGO
{
    public class 新幹線専用コントローラ
    {
        public const int Vid = 0x0AE4;
        public const int Pid = 0x0005;

        private UsbDevice usbDevice;

        private enum ReadDataIndex
        {
            BrakeLever = 0,
            PowerLever = 1,

            FootPedal = 2,

            Direction = 3,
            Button = 4
        }

        private enum WriteDataIndex
        {
            LeftRumble = 0,
            RightRumble = 1,

            SmallSegmentBar = 2,
            LargeSegmentBar = 3,

            LowerSpeedDisplay = 4,
            UpperSpeedDisplay = 5,

            LowerATCDisplay = 6,
            UpperATCDisplay = 7,
        }

        // TODO: name small and large segment bar better
        public const int SmallSegmentBarMaximum = 10;
        public const int LargeSegmentBarMaximum = 23;

        private Thread updateThread;

        /** Automatically opens the device */
        public 新幹線専用コントローラ(UsbDevice usbDevice)
        {
            this.usbDevice = usbDevice;

            var iuDev = usbDevice as IUsbDevice;
            if (!ReferenceEquals(iuDev, null))
            {
                iuDev.SetConfiguration(1);
                iuDev.ClaimInterface(0);
            }
            usbDevice.Open();

            updateThread = new Thread(UpdateLoop);
            updateThread.Start();
        }

        private void UpdateLoop()
        {
            while (usbDevice.IsOpen)
            {
                var dataBuffer = new byte[64];
                var bytesRead = 0;

                using (var Reader = usbDevice.OpenEndpointReader(ReadEndpointID.Ep01))
                {
                    var errorCode = ErrorCode.None;
                    errorCode = Reader.Read(dataBuffer, 2000, out bytesRead);

                    if (errorCode == ErrorCode.IoTimedOut) continue;
                    if (errorCode == ErrorCode.IoCancelled) return;
                    if (errorCode != 0) throw new Exception("usbcom ReadCtrl Error : ErrorCode==" + errorCode.ToString() + "\n" + UsbDevice.LastErrorString);
                }

                if (bytesRead > 0)
                {
                    HandleReadData(dataBuffer, bytesRead);
                }

                Thread.Sleep(10);
            }
        }
        private void HandleReadData(byte[] data, int bytesRead)
        {
            // TODO: test bytesRead is enough

            if (OnReadData != null)
            {
                var onReadDataThread = new Thread(() => OnReadData.Invoke(null, new ReadDataEventArgs()
                {
                    BrakeLever = data[(int)ReadDataIndex.BrakeLever],
                    PowerLever = data[(int)ReadDataIndex.PowerLever],
                    FootPedal = data[(int)ReadDataIndex.FootPedal],
                    Direction = data[(int)ReadDataIndex.Direction],
                    Button = data[(int)ReadDataIndex.Button]
                }));
                onReadDataThread.Start();
            }
        }

        public class ReadDataEventArgs : EventArgs
        {
            public byte BrakeLever;
            public byte PowerLever;

            public byte FootPedal;

            public byte Direction;

            public byte Button;
        }
        public event EventHandler<ReadDataEventArgs> OnReadData;


        public struct WriteData
        {
            public byte LeftRumble;
            public byte RightRumble;

            public byte SmallSegmentBar;
            public byte LargeSegmentBar;

            public byte LowerSpeedDisplay;
            public byte UpperSpeedDisplay;

            public byte LowerATCDisplay;
            public byte UpperATCDisplay;
        }

        // TODO: better name, this contains rumble as well
        public int SendDisplayData(WriteData writeData)
        {
            var transfered = 0;
            var setupPacket = new UsbSetupPacket(0x40, 0x09, 0x0301, 0x0000, 8);

            var Data = new byte[8];

            Data[(int)WriteDataIndex.LeftRumble] = writeData.LeftRumble;
            Data[(int)WriteDataIndex.RightRumble] = writeData.RightRumble;
            Data[(int)WriteDataIndex.SmallSegmentBar] = writeData.SmallSegmentBar;
            Data[(int)WriteDataIndex.LargeSegmentBar] = writeData.LargeSegmentBar;
            Data[(int)WriteDataIndex.LowerSpeedDisplay] = writeData.LowerSpeedDisplay;
            Data[(int)WriteDataIndex.UpperSpeedDisplay] = writeData.UpperSpeedDisplay;
            Data[(int)WriteDataIndex.LowerATCDisplay] = writeData.LowerATCDisplay;
            Data[(int)WriteDataIndex.UpperATCDisplay] = writeData.UpperATCDisplay;

            usbDevice.ControlTransfer(ref setupPacket, Data, 8, out transfered);
            return transfered;
        }

        public void Close()
        {
            usbDevice.Close();
        }
    }
}

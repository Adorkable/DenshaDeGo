using LibUsbDotNet;
using System;
using System.Threading;

namespace 電車でGO
{
    public class 新幹線専用コントローライージィ
    {
        private 新幹線専用コントローラ controller;

        public 新幹線専用コントローライージィ(UsbDevice usbDevice)
        {
            controller = new 新幹線専用コントローラ(usbDevice);
            controller.OnReadData += HandleController_ReadData;
        }

        public class ReadStateEventArgs : EventArgs
        {
            public BrakeHandleState BrakeHandle;
            public PowerHandleState PowerHandle;
            public bool FootPedal;
            public DirectionState Direction;
            public bool AButton;
            public bool BButton;
            public bool CButton;
            public bool DButton;
            public bool SelectButton;
            public bool StartButton;

            public static string KeyStateToString(bool keyState)
            {
                if (keyState)
                {
                    return "DOWN";
                }
                else
                {
                    return "UP  ";
                }
            }

            public static string DirectionToString(DirectionState direction)
            {
                switch (direction)
                {
                    case DirectionState.UpperLeft:
                        return "UP-LEFT   ";
                    case DirectionState.Up:
                        return "UP        ";
                    case DirectionState.UpperRight:
                        return "UP-RIGHT  ";
                    case DirectionState.Right:
                        return "RIGHT     ";
                    case DirectionState.Down:
                        return "DOWN      ";
                    case DirectionState.Left:
                        return "LEFT      ";
                    case DirectionState.None:
                        return "NONE      ";
                    case DirectionState.DownLeft:
                        return "DOWN-LEFT ";
                    case DirectionState.DownRight:
                        return "DOWN-RIGHT";
                    default:
                        return "UNKNOWN   ";
                }
            }

            public override string ToString()
            {
                return base.ToString() + $"\nBrake Handle: {BrakeHandle}\nPower Handle: {PowerHandle}\nFoot Pedal:" + KeyStateToString(FootPedal) +
                " Direction: " + DirectionToString(Direction) +
                " A:" + KeyStateToString(AButton) +
                " B:" + KeyStateToString(BButton) +
                " C:" + KeyStateToString(CButton) +
                " D:" + KeyStateToString(DButton) +
                " Select:" + KeyStateToString(SelectButton) +
                " Start:" + KeyStateToString(StartButton);
            }
        }
        public event EventHandler<ReadStateEventArgs> OnReadState;

        private BrakeHandleState previousBrakeHandleState = new BrakeHandleState()
        {
            previousLevel = -1
        };
        private PowerHandleState previousPowerHandleState = new PowerHandleState()
        {
            previousLevel = -1
        };

        private void HandleController_ReadData(object sender, 新幹線専用コントローラ.ReadDataEventArgs eventArgs)
        {
            if (OnReadState != null)
            {
                (new Thread(() =>
                {
                    /** TODO: currently my controller flags "release" as 0xff (rather than 0x00) and in between as the same 0xff so there is no way to tell one from another */
                    var brakeHandleState = new BrakeHandleState()
                    {
                        level = eventArgs.BrakeLever != 0xff
                            ? (int)Math.Round(eventArgs.BrakeLever / 28.0, MidpointRounding.AwayFromZero) - 1
                            : -1,
                        inBetween = eventArgs.BrakeLever == 0xff,
                        previousLevel = previousBrakeHandleState.level
                    };

                    /** TODO: I am unable to test this, my controller's power handle seems broken :( */
                    var powerHandleState = new PowerHandleState()
                    {
                        level = eventArgs.PowerLever != 0xff
                            ? (int)Math.Round(eventArgs.PowerLever / 18.0, MidpointRounding.AwayFromZero) - 1
                            : -1,
                        inBetween = eventArgs.PowerLever == 0xff,
                        previousLevel = previousPowerHandleState.level
                    };

                    var buttonBuffer = eventArgs.Button;

                    OnReadState.Invoke(null, new ReadStateEventArgs()
                    {

                        BrakeHandle = brakeHandleState,
                        PowerHandle = powerHandleState,

                        FootPedal = eventArgs.FootPedal == 0,

                        Direction = (DirectionState)eventArgs.Direction,

                        AButton = TestKeyState(buttonBuffer, ButtonStateFlag.A),
                        BButton = TestKeyState(buttonBuffer, ButtonStateFlag.B),
                        CButton = TestKeyState(buttonBuffer, ButtonStateFlag.C),
                        DButton = TestKeyState(buttonBuffer, ButtonStateFlag.D),
                        SelectButton = TestKeyState(buttonBuffer, ButtonStateFlag.Select),
                        StartButton = TestKeyState(buttonBuffer, ButtonStateFlag.Start),
                    });

                    if (!brakeHandleState.inBetween)
                    {
                        previousBrakeHandleState = brakeHandleState;
                    }
                    if (!powerHandleState.inBetween)
                    {
                        previousPowerHandleState = powerHandleState;
                    }
                })).Start();
            }
        }

        private static bool TestKeyState(byte KeyData, ButtonStateFlag buttonDataFlag)
        {
            return ((ButtonStateFlag)KeyData & buttonDataFlag) == buttonDataFlag;
        }

        private 新幹線専用コントローラ.WriteData previousWriteData = new 新幹線専用コントローラ.WriteData()
        {
            LeftRumble = 0x00,
            RightRumble = 0x00,
            LargeSegmentBar = 0x00,
            SmallSegmentBar = 0x00,
            LowerSpeedDisplay = 0x00,
            UpperSpeedDisplay = 0x00,
            LowerATCDisplay = 0x00,
            UpperATCDisplay = 0x00
        };

        private byte booleanToRumble(bool enable)
        {
            return (byte)(enable ? 0xff : 0x00);
        }

        public void EnableLeftRumble(bool enable)
        {
            var writeData = previousWriteData;

            writeData.LeftRumble = booleanToRumble(enable);

            SendDisplayData(writeData);
        }

        public void EnableRightRumble(bool enable)
        {
            var writeData = previousWriteData;

            writeData.RightRumble = booleanToRumble(enable);

            SendDisplayData(writeData);
        }

        /** Values from 0 to 新幹線専用コントローラ.LargeSegmentBarMaximum */
        public void SetLargeSegmentBar(int largeSegmentBar)
        {
            var writeData = previousWriteData;

            writeData.LargeSegmentBar = (byte)(
                largeSegmentBar > 新幹線専用コントローラ.LargeSegmentBarMaximum
                ? 新幹線専用コントローラ.LargeSegmentBarMaximum
                : largeSegmentBar
            );

            SendDisplayData(writeData);
        }

        public void SetSmallSegmentBar(int value)
        {
            var writeData = previousWriteData;

            var normalizedValue = Convert.ToByte(
                value > 新幹線専用コントローラ.SmallSegmentBarMaximum
                ? 新幹線専用コントローラ.SmallSegmentBarMaximum
                : value
            );

            writeData.SmallSegmentBar = Convert.ToByte(Convert.ToByte(writeData.SmallSegmentBar & 0x80) | Convert.ToByte(normalizedValue & 0x0f));

            SendDisplayData(writeData);
        }

        public void EnableDoorsClosedLight(bool enable)
        {
            var writeData = previousWriteData;

            writeData.SmallSegmentBar = Convert.ToByte(Convert.ToByte(enable ? 0x80 : 0x00) | Convert.ToByte(writeData.SmallSegmentBar & 0x0f));

            SendDisplayData(writeData);
        }

        private static (byte, byte) splitThreePartDisplayValue(int value)
        {
            var textByte = value < 10 ? "0" + value : "" + value;

            return (
                Convert.ToByte("0x" + textByte.Substring(textByte.Length - 2, 2), 16),
                value > 99 ? Convert.ToByte("0x" + textByte.Substring(textByte.Length - 3, 1), 16) : (byte)0x0
             );
        }

        public void SetSpeedDisplay(int value)
        {
            var writeData = previousWriteData;

            var (lower, upper) = splitThreePartDisplayValue(value);

            writeData.LowerSpeedDisplay = lower;
            writeData.UpperSpeedDisplay = upper;

            SendDisplayData(writeData);
        }

        public void SetLowerSpeedDisplay(int value)
        {
            var writeData = previousWriteData;

            writeData.LowerSpeedDisplay = Convert.ToByte(value);

            SendDisplayData(writeData);
        }

        public void SetUpperSpeedDisplay(int value)
        {
            var writeData = previousWriteData;

            writeData.UpperSpeedDisplay = Convert.ToByte(value);

            SendDisplayData(writeData);
        }

        public void SetATCDisplay(int value)
        {
            var writeData = previousWriteData;

            var (lower, upper) = splitThreePartDisplayValue(value);

            writeData.LowerATCDisplay = lower;
            writeData.UpperATCDisplay = upper;

            SendDisplayData(writeData);
        }

        public void SetLowerATCDisplay(byte lowerATCDisplay)
        {
            var writeData = previousWriteData;

            writeData.LowerATCDisplay = lowerATCDisplay;

            SendDisplayData(writeData);
        }

        public void SetUpperATCDisplay(byte upperATCDisplay)
        {
            var writeData = previousWriteData;

            writeData.UpperATCDisplay = upperATCDisplay;

            SendDisplayData(writeData);
        }

        private void SendDisplayData(新幹線専用コントローラ.WriteData writeData)
        {
            controller.SendDisplayData(writeData);
            previousWriteData = writeData;
        }

        public void Dispose()
        {
            controller.OnReadData -= HandleController_ReadData;
        }
    }
}

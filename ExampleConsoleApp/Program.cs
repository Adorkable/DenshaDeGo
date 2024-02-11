using 電車でGO;

try
{
    var main = new Main();
}
catch (Exception exception)
{
    Console.WriteLine("Exception: " + exception);
}

public class Main
{
    public int Version => 100;

    private 新幹線専用コントローライージィ controller;

    private byte test = 0;

    public Main()
    {
        var usbDevice = DeviceFinder.FindDevice(DeviceFinder.SupportedDevice.TCPP20011);

        if (usbDevice == null)
        {
            throw new Exception("Device Not Found");
        }

        controller = new 新幹線専用コントローライージィ(usbDevice);
        controller.OnReadState += HandleController_OnReadState;

        Console.WriteLine("Press any key to close");

        Console.ReadKey();

        Environment.Exit(0);
    }

    private void HandleController_OnReadState(object sender, 新幹線専用コントローライージィ.ReadStateEventArgs eventArgs)
    {
        Console.Clear();
        Console.WriteLine(eventArgs.ToString());

        Console.WriteLine("\nPress any key to close");

        controller.EnableLeftRumble(eventArgs.BrakeHandle.level == BrakeHandleState.MaximumLevel);
        controller.EnableRightRumble(eventArgs.PowerHandle.level == PowerHandleState.MaximumLevel);

        var brakePercentageLevel = eventArgs.BrakeHandle.inBetween
            ? eventArgs.BrakeHandle.previousPercentageLevel
            : eventArgs.BrakeHandle.percentageLevel;

        var powerPercentageLevel = eventArgs.PowerHandle.inBetween
            ? eventArgs.PowerHandle.previousPercentageLevel
            : eventArgs.PowerHandle.percentageLevel;

        controller.SetLargeSegmentBar((int)Math.Round(brakePercentageLevel * 新幹線専用コントローラ.LargeSegmentBarMaximum));

        controller.SetSmallSegmentBar((byte)Math.Round(powerPercentageLevel * 新幹線専用コントローラ.SmallSegmentBarMaximum));

        if (eventArgs.Direction == DirectionState.Up)
        {
            test++;
        }
        else if (eventArgs.Direction == DirectionState.Down)
        {
            test--;
        }

        controller.EnableDoorsClosedLight(test % 2 == 0);

        controller.SetSpeedDisplay((int)Math.Round(brakePercentageLevel * 999));
        controller.SetATCDisplay((int)Math.Round(powerPercentageLevel * 999));
    }

    public void Dispose()
    {
        controller.OnReadState -= HandleController_OnReadState;
    }
}


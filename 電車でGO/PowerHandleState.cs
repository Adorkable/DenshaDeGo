namespace 電車でGO
{
    public struct PowerHandleState
    {
        public static int MaximumLevel = 13;

        public int level;
        public float percentageLevel { get { return (float)level / MaximumLevel; } }


        public bool cut { get { return level == 0; } }

        /** Flags when the brake handle is inbetween levels */
        public bool inBetween;
        /** Previous level before the inbetween state */
        public int previousLevel;
        public float previousPercentageLevel { get { return (float)previousLevel / MaximumLevel; } }

        public override string ToString()
        {
            return base.ToString() + $"\nLevel:{level} {percentageLevel * 100}% Cut: {cut} In-Between: {inBetween} Previous Level: {previousLevel}";
        }
    }
}

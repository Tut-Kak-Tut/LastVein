using System;

namespace LastVein.Core
{
    public static class BigNumberFormatter
    {
        static readonly string[] Suffixes = { "", "K", "M", "B", "T" };

        public static string Format(double value)
        {
            if (value < 1000) return Math.Floor(value).ToString("0");

            int magnitude = 0;
            while (value >= 1000 && magnitude < Suffixes.Length - 1)
            {
                value /= 1000.0;
                magnitude++;
            }

            return value.ToString("0.##") + Suffixes[magnitude];
        }
    }
}

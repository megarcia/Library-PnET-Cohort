using System;

namespace Landis.Library.PnETCohorts
{
    public class Calendar
    {
        public enum Months : int
        {
            January = 1,
            February = 2,
            March = 3,
            April = 4,
            May = 5,
            June = 6,
            July = 7,
            August = 8,
            September = 9,
            October = 10,
            November = 11,
            December = 12
        }

        public static int CalcDaySpan(int Month)
        {
            switch (Month)
            {
                case 1: return 31;
                case 2: return 28;
                case 3: return 31;
                case 4: return 30;
                case 5: return 31;
                case 6: return 30;
                case 7: return 31;
                case 8: return 31;
                case 9: return 30;
                case 10: return 31;
                case 11: return 30;
                case 12: return 31;
                default: throw new Exception("Month " + Month + " is not an integer between 1-12. Error assigning DaySpan");
            };
        }

        /// <summary>
        /// Nightlength in seconds
        /// </summary>
        /// <param name="Hrs"></param>
        /// <returns></returns>
        public static float CalcNightLength(float Hrs)
        {
            return Constants.SecondsPerHour * (24.0 - Hrs);
        }

        /// <summary>
        /// DayLength in seconds
        /// </summary>
        /// <param name="Hrs"></param>
        /// <returns></returns>
        public static float CalcDayLength(float Hrs)
        {
            return Constants.SecondsPerHour * Hrs;
        }

        /// <summary>
        /// Calculate hours of daylight
        /// NOTE: check unused calculation of r
        /// </summary>
        /// <param name="DOY"></param>
        /// <param name="Latitude"></param>
        /// <returns></returns>
        public static float CalcDaylightHrs(int DOY, double Latitude)
        {
            float LatRad = (float)Latitude * (2.0 * (float)Math.PI) / 360.0;
            float z = 0.39785 * (float)Math.Sin(4.868961 + 0.017203 * DOY + 0.033446 * (float)Math.Sin(6.224111 + 0.017202 * DOY));
            float decl;
            if ((float)Math.Abs(z) < 0.7)
                decl = (float)Math.Atan(z / ((float)Math.Sqrt(1.0 - z * z)));
            else
                decl = (float)Math.PI / 2.0 - (float)Math.Atan((float)Math.Sqrt(1.0 - z * z) / z);
            if ((float)Math.Abs(LatRad) >= (float)Math.PI / 2.0)
            {
                if (Latitude < 0)
                    LatRad = -1.0 * ((float)Math.PI / 2.0 - 0.01);
                else
                    LatRad = 1.0 * ((float)Math.PI / 2.0 - 0.01);
            }
            float z2 = -(float)Math.Tan(decl) * (float)Math.Tan(LatRad);
            float h;
            if (z2 >= 1.0)
                h = 0.0;
            else if (z2 <= -1.0)
                h = (float)Math.PI;
            else
            {
                float TA = (float)Math.Abs(z2);
                float AC;
                if (TA < 0.7)
                    AC = ((float)Math.PI / 2.0) - (float)Math.Atan(TA / (float)Math.Sqrt(1.0 - TA * TA));
                else
                    AC = (float)Math.Atan((float)Math.Sqrt(1.0 - TA * TA) / TA);
                if (z2 < 0)
                    h = (float)Math.PI - AC;
                else
                    h = AC;
            }
            float daylight_h = 2.0 * (h * 24.0) / (2.0 * (float)Math.PI);
            return daylight_h;
        }
    }
}

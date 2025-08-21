
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
                default: throw new System.Exception("Month " + Month + " is not an integer between 1-12. Error assigning DaySpan");
            };
        }

        /// <summary>
        /// Nightlength in seconds
        /// </summary>
        /// <param name="Hrs"></param>
        /// <returns></returns>
        public static float CalcNightLength(float Hrs)
        {
            return 60 * 60 * (24 - Hrs);
        }

        /// <summary>
        /// DayLength in seconds
        /// </summary>
        /// <param name="Hrs"></param>
        /// <returns></returns>
        public static float CalcDayLength(float Hrs)
        {
            return 60 * 60 * Hrs;
        }

        /// <summary>
        /// Calculate hours of daylight
        /// </summary>
        /// <param name="DOY"></param>
        /// <param name="Latitude"></param>
        /// <returns></returns>
        public static float CalcDaylightHrs(int DOY, double Latitude)
        {
            float TA;
            float AC;
            float LatRad;
            float r;
            float z;
            float decl;
            float z2;
            float h;
            LatRad = (float)Latitude * (2.0f * (float)Math.PI) / 360.0f;
            r = 1.0f - (0.0167f * (float)Math.Cos(0.0172f * (float)(DOY - 3)));
            z = 0.39785f * (float)Math.Sin(4.868961f + 0.017203f * (float)DOY + 0.033446f * (float)Math.Sin(6.224111f + 0.017202f * (float)DOY));
            if ((float)Math.Abs(z) < 0.7f)
                decl = (float)Math.Atan(z / ((float)Math.Sqrt(1.0f - z * z)));
            else
                decl = (float)Math.PI / 2.0f - (float)Math.Atan((float)Math.Sqrt(1.0f - z * z) / z);
            if ((float)Math.Abs(LatRad) >= (float)Math.PI / 2.0)
            {
                if (Latitude < 0)
                    LatRad = (-1.0f) * ((float)Math.PI / 2.0f - 0.01f);
                else
                    LatRad = 1.0f * ((float)Math.PI / 2.0f - 0.01f);
            }
            z2 = -(float)Math.Tan(decl) * (float)Math.Tan(LatRad);
            if (z2 >= 1.0)
                h = 0;
            else if (z2 <= -1.0)
                h = (float)Math.PI;
            else
            {
                TA = (float)Math.Abs(z2);
                if (TA < 0.7)
                    AC = 1.570796f - (float)Math.Atan(TA / (float)Math.Sqrt(1.0f - TA * TA));
                else
                    AC = (float)Math.Atan((float)Math.Sqrt(1.0f - TA * TA) / TA);
                if (z2 < 0)
                    h = 3.141593f - AC;
                else
                    h = AC;
            }
            return 2.0f * (h * 24.0f) / (2.0f * (float)Math.PI);
        }
    }
}

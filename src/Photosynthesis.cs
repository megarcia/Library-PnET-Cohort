namespace Landis.Library.PnETCohorts
{
    public class Photosynthesis
    {
        public static float CurvilinearPsnTempResponse(float Tday, float PsnTopt, float PsnTmin, float PsnTmax)
        {
            if (Tday < PsnTmin)
                return 0F;
            else if (Tday > PsnTopt)
                return 1F;
            else
                return (PsnTmax - Tday) * (Tday - PsnTmin) / (float)Math.Pow((PsnTmax - PsnTmin) / 2F, 2);
        }

        public static float DTempResponse(float Tday, float PsnTopt, float PsnTmin, float PsnTmax)
        {
            if (Tday < PsnTmin || Tday > PsnTmax)
                return 0F;
            else
            {
                if (Tday <= PsnTopt)
                {
                    float PsnTmaxEst = PsnTopt + (PsnTopt - PsnTmin);
                    return (float)Math.Max(0.0, (PsnTmaxEst - Tday) * (Tday - PsnTmin) / (float)Math.Pow((PsnTmaxEst - PsnTmin) / 2F, 2));
                }
                else
                {
                    float PsnTminEst = PsnTopt + (PsnTopt - PsnTmax);
                    return (float)Math.Max(0.0, (PsnTmax - Tday) * (Tday - PsnTminEst) / (float)Math.Pow((PsnTmax - PsnTminEst) / 2F, 2));
                }
            }
        }
    }
}

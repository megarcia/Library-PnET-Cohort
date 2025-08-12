namespace Landis.Library.PnETCohorts
{
    public class Photosynthesis
    {
        public static float CurvilinearPsnTempResponse(float tday, float psnTopt, float psnTmin, float psnTmax)
        {
            if (tday < psnTmin)
                return 0F;
            else if (tday > psnTopt)
                return 1F;
            else
                return (psnTmax - tday) * (tday - psnTmin) / (float)Math.Pow((psnTmax - psnTmin) / 2F, 2);
        }

        public static float DTempResponse(float tday, float psnTopt, float psnTmin, float psnTmax)
        {
            if (tday < psnTmin || tday > psnTmax)
                return 0F;
            else
            {
                if (tday <= psnTopt)
                {
                    float psnTmaxestimate = psnTopt + (psnTopt - psnTmin);
                    return (float)Math.Max(0.0, (psnTmaxestimate - tday) * (tday - psnTmin) / (float)Math.Pow((psnTmaxestimate - psnTmin) / 2F, 2));
                }
                else
                {
                    float psnTminestimate = psnTopt + (psnTopt - psnTmax);
                    return (float)Math.Max(0.0, (psnTmax - tday) * (tday - psnTminestimate) / (float)Math.Pow((psnTmax - psnTminestimate) / 2F, 2));
                }
            }
        }
    }
}

using System;

namespace Landis.Library.PnETCohorts
{
    public class Photosynthesis
    {
        /// <summary>
        /// Curvilinear response of photosynthesis to temperature
        /// </summary>
        /// <param name="Tday"></param>
        /// <param name="PsnTopt"></param>
        /// <param name="PsnTmin"></param>
        /// <param name="PsnTmax"></param>
        /// <returns></returns>
        public static float CurvilinearPsnTempResponse(float Tday, float PsnTopt, float PsnTmin, float PsnTmax)
        {
            if (Tday < PsnTmin)
                return 0F;
            else if (Tday > PsnTopt)
                return 1F;
            else
                return (PsnTmax - Tday) * (Tday - PsnTmin) / (float)Math.Pow((PsnTmax - PsnTmin) / 2F, 2);
        }

        /// <summary>
        /// Alternate response of photosynthesis to temperature
        /// </summary>
        /// <param name="Tday"></param>
        /// <param name="PsnTopt"></param>
        /// <param name="PsnTmin"></param>
        /// <param name="PsnTmax"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Calculate DVPD, gradient of effect of vapor pressure deficit 
        /// on growth
        /// </summary>
        /// <param name="VPD"></param>
        /// <param name="DVPD1"></param>
        /// <param name="DVPD2"></param>
        /// <returns></returns>
        public static float CalcDVPD(float VPD, float DVPD1, float DVPD2)
        {
            float DVPD = Math.Max(0, 1.0F - DVPD1 * (float)Math.Pow(VPD, DVPD2));
            return DVPD;
        }

        /// <summary>
        /// Calculate JH20
        /// </summary>
        /// <param name="Tmin"></param>
        /// <param name="VPD"></param>
        /// <returns></returns>
        public static float CalcJH2O(float Tmin, float VPD)
        {
            float JH2O = (float)(Constants.CalperJ * (VPD / (Constants.GasConst_JperkmolK * (Tmin + Constants.Tref_K))));
            return JH2O;
        }

        /// <summary>
        /// Modify AmaxB based on CO2 level using linear interpolation
        /// uses 2 known points: (350, AmaxB) and (550, AmaxB * CO2AmaxBEff)
        /// </summary>
        /// <param name="CO2"></param>
        /// <param name="AmaxB"></param>
        /// <param name="CO2AMaxBEff"></param>
        /// <returns></returns>
        public static float CalcAmaxB_CO2(float CO2, float AmaxB, float CO2AMaxBEff)
        {
            // AmaxB_slope = [(AmaxB * CO2AMaxBEff) - AmaxB] / [550 - 350]
            float AmaxB_slope = (float)((CO2AMaxBEff - 1.0) * AmaxB / 200.0F);
            // AmaxB_intercept = AmaxB - (AmaxB_slope * 350)
            float AmaxB_intercept = (float)(-1.0 * (((CO2AMaxBEff - 1.0) * 1.75) - 1.0) * AmaxB);
            float AmaxB_CO2 = (float)(AmaxB_slope * CO2 + AmaxB_intercept);
            return AmaxB_CO2;
        }
    }
}

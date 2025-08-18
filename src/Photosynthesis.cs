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
        /// uses 2 known points: (350, AmaxB) and (550, AmaxB * AMaxBFCO2)
        /// </summary>
        /// <param name="CO2"></param>
        /// <param name="AmaxB"></param>
        /// <param name="AMaxBFCO2"></param>
        /// <returns></returns>
        public static float CalcAmaxB_CO2(float CO2, float AmaxB, float AMaxBFCO2)
        {
            // AmaxB_slope = [(AmaxB * AMaxBFCO2) - AmaxB] / [550 - 350]
            float AmaxB_slope = (float)((AMaxBFCO2 - 1.0) * AmaxB / 200.0F);
            // AmaxB_intercept = AmaxB - (AmaxB_slope * 350)
            float AmaxB_intercept = (float)(-1.0 * (((AMaxBFCO2 - 1.0) * 1.75) - 1.0) * AmaxB);
            float AmaxB_CO2 = (float)(AmaxB_slope * CO2 + AmaxB_intercept);
            return AmaxB_CO2;
        }

        /// <summary>
        /// Calculate CiModifier as a function of leaf O3 tolerance
        /// Regression coefs estimated from New 3 algorithm for Ozone drought.xlsx
        /// https://usfs.box.com/s/eksrr4d7fli8kr9r4knfr7byfy9r5z0i
        /// Uses data provided by Yasutomo Hoshika and Elena Paoletti
        /// </summary>
        /// <param name="CumulativeO3"></param>
        /// <param name="StomataO3Sensitivity"></param>
        /// <param name="FWaterOzone"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static float CalcCiModifier(float CumulativeO3, string StomataO3Sens, float FWaterOzone)
        {
            float CiModifier = 1.0f; // if no ozone, ciModifier defaults to 1
            if (CumulativeO3 > 0)
            {
                if (StomataO3Sens == "Sensitive" || StomataO3Sens == "Sens")
                    CiModifier = (float)(FWaterOzone + (-0.0176 * FWaterOzone + 0.0118) * CumulativeO3);
                else if (StomataO3Sens == "Intermediate" || StomataO3Sens == "Int")
                    CiModifier = (float)(FWaterOzone + (-0.0148 * FWaterOzone + 0.0062) * CumulativeO3);
                else if (StomataO3Sens == "Tolerant" || StomataO3Sens == "Tol")
                    CiModifier = (float)(FWaterOzone + (-0.021 * FWaterOzone + 0.0087) * CumulativeO3);
                else
                    throw new Exception("O3 data provided, but species StomataO3Sensitivity is not set to Sensitive, Intermediate, or Tolerant");
            }
            CiModifier = Math.Max(0.00001F, Math.Min(CiModifier, 1.0F));
            return CiModifier;
        }

        /// <summary>
        /// Radiative (light) effect on photosynthesis
        /// </summary>
        /// <param name="Radiation"></param>
        /// <param name="HalfSat"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static float CalcFRad(float Radiation, float HalfSat)
        {
            if (HalfSat > 0)
                return (float)(1.0 - Math.Exp(-1.0 * Radiation * Math.Log(2.0) / HalfSat));
            else
                throw new Exception("HalfSat <= 0. Cannot calculate fRad.");
        }

        /// <summary>
        /// Soil water effect on photosynthesis
        /// </summary>
        /// <param name="H1"></param>
        /// <param name="H2"></param>
        /// <param name="H3"></param>
        /// <param name="H4"></param>
        /// <param name="pressureHead"></param>
        /// <returns></returns>
        public static float CalcFWater(float H1, float H2, float H3, float H4, float pressureHead)
        {
            float minThreshold = H1;
            if (H2 <= H1)
                minThreshold = H2;
            // Calculate water stress
            if (pressureHead <= H1)
                return 0F;
            else if (pressureHead < minThreshold || pressureHead >= H4)
                return 0F;
            else if (pressureHead > H3)
                return 1F - ((pressureHead - H3) / (H4 - H3));
            else if (pressureHead < H2)
                return 1F / (H2 - H1) * pressureHead - (H1 / (H2 - H1));
            else
                return 1F;
        }

        /// <summary>
        /// O3 effect on photosynthesis
        /// </summary>
        /// <param name="O3"></param>
        /// <param name="Layer"></param>
        /// <param name="nLayers"></param>
        /// <param name="FolMass"></param>
        /// <param name="LastFOzone"></param>
        /// <param name="WVConductance"></param>
        /// <param name="O3Coeff"></param>
        /// <returns></returns>
        public static float CalcFOzone(float O3, int Layer, int nLayers, float FolMass, float LastFOzone, float WVConductance, float O3Coeff)
        {
            float DroughtO3Frac = 1.0F; // Not using DroughtO3Frac from PnET code per M. Kubiske and A. Chappelka
            float kO3Eff = 0.0026F * O3Coeff;  // Scaled by species using input parameters
            float O3Prof = (float)(0.6163F + (0.00105F * FolMass));
            float RelLayer = Layer / (float)nLayers;
            float RelO3 = Math.Min(1F, 1F - RelLayer * O3Prof * Math.Pow((RelLayer * O3Prof),2));
            // Kubiske method (using water vapor conductance in place of conductance
            float FOzone = (float)Math.Min(1F, (LastFOzone * DroughtO3Frac) + (kO3Eff * WVConductance * O3 * RelO3));
            return FOzone;
        }
    }
}

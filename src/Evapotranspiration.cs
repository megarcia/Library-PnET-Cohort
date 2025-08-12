
namespace Landis.Library.PnETCohorts
{
    public class Evapotranspiration
    {
        /// <summary>
        /// PE calculations based on Stewart & Rouse 1976 and Cabrera et al. 2016
        /// </summary>
        /// <param name="par">Daytime solar radiation (PAR) (micromol/m2.s)</param>
        /// <param name="tair">Daytime air temperature (°C) [Tday]</param>
        /// <param name="daySpan">Days in the month</param>
        /// <param name="dayLength">Length of daylight (s)</param>
        /// <returns></returns>
        static float CalcPotentialEvaporation_umol(double par, double tair, float daySpan, float dayLength)
        {
            // convert PAR (umol/m2.s) to total solar radiation (W/m2) (Reis and Ribeiro, 2019, eq. 39)  
            // convert Rs_W (W/m2) to Rs (MJ/m2.d) (Reis and Ribeiro, 2019, eq. 13)
            float Rs = (float)par / 2.02F * Constants.SecondsPerDay / 1000000F;
            // get slope of vapor pressure curve at Tair
            float VPSlope = CalcVaporPressureCurveSlope((float)tair);
            // calculate potential evaporation (Stewart & Rouse, 1976, eq. 11)
            float PotentialEvaporation_MJ = VPSlope / (VPSlope + Constants.PsychrometricCoeff) * (1.624F + 0.9265F * Rs); // MJ/m2.day 
            // convert MJ/m2.day to mm/day (http://www.fao.org/3/x0490e/x0490e0i.htm)
            float PotentialEvaporation = PotentialEvaporation_MJ * 0.408F;
            return PotentialEvaporation * daySpan;  // mm/month 
        }

        /// <summary>
        /// PET calculations via Priestley-Taylor
        /// </summary>
        /// <param name="aboveCanopyPAR">Daytime PAR (umol/m2.s) at top of canopy</param>
        /// <param name="subCanopyPAR">Daytime PAR (umol/m2.s) at bottom of canopy</param>
        /// <param name="dayLength">Daytime length (s)</param>
        /// <param name="T">Average monthly temperature (C)</param>
        /// <param name="daySpan">Days in the month</param>
        /// <returns></returns>
        public float CalcPotentialGroundET_Radiation_umol(float aboveCanopyPAR, float subCanopyPAR, float dayLength, float T, float daySpan)
        {
            // convert daytime PAR (umol/m2*s) to total daily PAR (umol/m2*s)
            float Rs_daily = (float)(aboveCanopyPAR / Constants.SecondsPerDay / dayLength); 
            // convert daily PAR (umol/m2*s) to total solar radiation (W/m2)
            //     Reis and Ribeiro 2019 (Consants and Values)  
            float Rs_W = (float)(Rs_daily / 2.02f); 
            // Back-calculate LAI from aboveCanopyPAR and subCanopyPAR
            float k = 0.3038f;
            float LAI = (float)Math.Log(subCanopyPAR / aboveCanopyPAR) / (-1.0f * k);
            float aboveCanopyNetRad = 0f;
            if (LAI < 2.4)
                aboveCanopyNetRad = -26.8818f + 0.693066f * Rs_W;
            else
                aboveCanopyNetRad = -33.2467f + 0.741644f * Rs_W;
            float subCanopyNetRad = aboveCanopyNetRad * (float)Math.Exp(-1.0f * k * LAI);
            float alpha = 1.0f;
            float VPSlope = CalcVaporPressureCurveSlope((float)T);
            // conversion W/m2 to MJ/m2.d
            float PotentialET_ground = alpha * (VPSlope / (VPSlope + Constants.PsychrometricCoeff)) / Constants.LatentHeatVaporWater * subCanopyNetRad * Constants.SecondsPerDay / 1000000F; // m/day
            return PotentialET_ground * 1000 * daySpan; //mm/month
        }

        /// <summary>
        /// Reference ET calculations via Hamon
        /// </summary>
        /// <param name="T">Average monthly temperature (C)</param>
        /// <param name="dayLength">Daytime length (s)</param>
        /// <returns></returns>
        public float CalcReferenceET_Hamon(float T, float dayLength)
        {
            if (T < 0)
                return 0f;
            float k = 1.2f;   // proportionality coefficient
            float es = CalcVaporPressure(T);
            float N = dayLength / Constants.SecondsPerHour / 12f;
            float ReferenceET = k * 0.165f * 216.7f * N * (10f * es / (T + 273.3f)); // TODO: verify the 10x factor
            return ReferenceET; // mm/day
        }

        /// <summary>
        /// Potential ET given LAI via WATER (???)
        /// </summary>
        /// <param name="LAI">Total canopy LAI</param>
        /// <param name="T">Average monthly temperature (C)</param>
        /// <param name="dayLength">Daytime length (s)</param>
        /// <param name="daySpan">Days in the month</param>
        /// <returns></returns>
        public float CalcPotentialGroundET_LAI_WATER(float LAI, float T, float dayLength, float daySpan)
        {
            float ReferenceET = CalcReferenceET_Hamon(T, dayLength); // mm/day
            float Egp = 0.8f * ReferenceET * (float)Math.Exp(-0.695f * LAI); // mm/day
            return Egp * daySpan; //mm/month
        }

        /// <summary>
        /// Potential ET given LAI via WEPP (???)
        /// </summary>
        /// <param name="LAI">Total canopy LAI</param>
        /// <param name="T">Average monthly temperature (C)</param>
        /// <param name="dayLength">Daytime length (s)</param>
        /// <param name="daySpan">Days in the month</param>
        /// <returns></returns>
        public float CalcPotentialGroundET_LAI_WEPP(float LAI, float T, float dayLength, float daySpan)
        {
            float ReferenceET = CalcReferenceET_Hamon(T, dayLength); // mm/day
            float Egp = ReferenceET * (float)Math.Exp(-0.4f * LAI); // mm/day
            return Egp * daySpan; // mm/month
        }

        /// <summary>
        /// Potential ET given LAI and a given crop coefficient
        /// </summary>
        /// <param name="LAI">Total canopy LAI</param>
        /// <param name="T">Average monthly temperature (C)</param>
        /// <param name="dayLength">Daytime length (s)</param>
        /// <param name="daySpan">Days in the month</param>
        /// <param name="k">LAI extinction coefficient</param>
        /// <param name="cropCoeff">Crop coefficient (scalar adjustment)</param>
        /// <returns></returns>
        public float CalcPotentialGroundET_LAI(float LAI, float T, float dayLength, float daySpan, float k)
        {
            float cropCoeff = ((Parameter<float>)Names.GetParameter("ReferenceETCropCoeff")).Value;
            float ReferenceET = CalcReferenceET_Hamon(T, dayLength); // mm/day
            float Egp = cropCoeff * ReferenceET * (float)Math.Exp(-k * LAI); // mm/day
            return Egp * daySpan; // mm/month
        }
    }
}
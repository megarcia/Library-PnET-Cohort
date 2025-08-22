using System;
using System.Data;

namespace Landis.Library.PnETCohorts
{
    public class Evapotranspiration
    {
        /// <summary>
        /// PE calculations based on Stewart and Rouse 1976 and Cabrera et al. 2016
        /// NOTE: apparently unreferenced in PnET-Cohort library
        /// </summary>
        /// <param name="PAR">Daytime solar radiation (PAR) (micromol/m2.s)</param>
        /// <param name="Tair">Daytime air temperature (°C) [Tday]</param>
        /// <param name="DaySpan">Days in the month</param>
        /// <param name="DayLength">Length of daylight (s)</param>
        /// <returns></returns>
        public static float CalcPotentialEvaporation_umol(double PAR, double Tair, float DaySpan, float DayLength)
        {
            // convert PAR (umol/m2.s) to total solar radiation (W/m2) (Reis and Ribeiro, 2019, eq. 39)  
            // convert Rs_W (W/m2) to Rs (MJ/m2.d) (Reis and Ribeiro, 2019, eq. 13)
            float Rs = (float)PAR / 2.02F * Constants.SecondsPerDay / 1000000F;
            // get slope of vapor pressure curve at Tair
            float VPSlope = Weather.CalcVaporPressureCurveSlope((float)Tair);
            // calculate potential evaporation (Stewart & Rouse, 1976, eq. 11)
            float PotentialEvaporation_MJ = VPSlope / (VPSlope + Constants.PsychrometricCoeff) * (1.624F + 0.9265F * Rs); // MJ/m2.day 
            // convert MJ/m2.day to mm/day (http://www.fao.org/3/x0490e/x0490e0i.htm)
            float PotentialEvaporation = PotentialEvaporation_MJ * 0.408F;
            return PotentialEvaporation * DaySpan;  // mm/month 
        }

        /// <summary>
        /// PET calculations via Priestley-Taylor
        /// NOTE: apparently unreferenced in PnET-Cohort library
        /// </summary>
        /// <param name="AboveCanopyPAR">Daytime PAR (umol/m2.s) at top of canopy</param>
        /// <param name="SubCanopyPAR">Daytime PAR (umol/m2.s) at bottom of canopy</param>
        /// <param name="DayLength">Daytime length (s)</param>
        /// <param name="T">Average monthly temperature (C)</param>
        /// <param name="DaySpan">Days in the month</param>
        /// <returns></returns>
        public static float CalcPotentialGroundET_Radiation_umol(float AboveCanopyPAR, float SubCanopyPAR, float DayLength, float T, float DaySpan)
        {
            // convert daytime PAR (umol/m2*s) to total daily PAR (umol/m2*s)
            float Rs_daily = (float)(AboveCanopyPAR / Constants.SecondsPerDay / DayLength); 
            // convert daily PAR (umol/m2*s) to total solar radiation (W/m2)
            //     Reis and Ribeiro 2019 (Consants and Values)  
            float Rs_W = (float)(Rs_daily / 2.02f); 
            // Back-calculate LAI from aboveCanopyPAR and subCanopyPAR
            float k = 0.3038f;
            float LAI = (float)Math.Log(SubCanopyPAR / AboveCanopyPAR) / (-1.0f * k);
            float AboveCanopyNetRad = 0f;
            if (LAI < 2.4)
                AboveCanopyNetRad = -26.8818f + 0.693066f * Rs_W;
            else
                AboveCanopyNetRad = -33.2467f + 0.741644f * Rs_W;
            float SubCanopyNetRad = AboveCanopyNetRad * (float)Math.Exp(-1.0f * k * LAI);
            float alpha = 1.0f;
            float VPSlope = Weather.CalcVaporPressureCurveSlope((float)T);
            // conversion W/m2 to MJ/m2.d
            float PotentialET_ground = alpha * (VPSlope / (VPSlope + Constants.PsychrometricCoeff)) / Constants.LatentHeatVaporWater * SubCanopyNetRad * Constants.SecondsPerDay / 1000000F; // m/day
            return PotentialET_ground * 1000 * DaySpan; //mm/month
        }

        /// <summary>
        /// Reference ET calculations via Hamon
        /// NOTE: has interface entry
        /// </summary>
        /// <param name="T">Average monthly temperature (C)</param>
        /// <param name="DayLength">Daytime length (s)</param>
        /// <returns></returns>
        public static float CalcReferenceET_Hamon(float T, float DayLength)
        {
            if (T < 0)
                return 0f;
            float k = 1.2f;   // proportionality coefficient
            float es = Weather.CalcVaporPressure(T);
            float N = DayLength / Constants.SecondsPerHour / 12f;
            float ReferenceET = k * 0.165f * 216.7f * N * (10f * es / (T + 273.3f)); // TODO: verify the 10x factor
            return ReferenceET; // mm/day
        }

        /// <summary>
        /// Potential ET given LAI via WATER (???)
        /// NOTE: apparently unreferenced in PnET-Cohort library
        /// </summary>
        /// <param name="LAI">Total canopy LAI</param>
        /// <param name="T">Average monthly temperature (C)</param>
        /// <param name="DayLength">Daytime length (s)</param>
        /// <param name="DaySpan">Days in the month</param>
        /// <returns></returns>
        public static float CalcPotentialGroundET_LAI_WATER(float LAI, float T, float DayLength, float DaySpan)
        {
            float ReferenceET = CalcReferenceET_Hamon(T, DayLength); // mm/day
            float Egp = 0.8f * ReferenceET * (float)Math.Exp(-0.695f * LAI); // mm/day
            return Egp * DaySpan; //mm/month
        }

        /// <summary>
        /// Potential ET given LAI via WEPP (???)
        /// NOTE: apparently unreferenced in PnET-Cohort library
        /// </summary>
        /// <param name="LAI">Total canopy LAI</param>
        /// <param name="T">Average monthly temperature (C)</param>
        /// <param name="DayLength">Daytime length (s)</param>
        /// <param name="DaySpan">Days in the month</param>
        /// <returns></returns>
        public static float CalcPotentialGroundET_LAI_WEPP(float LAI, float T, float DayLength, float DaySpan)
        {
            float ReferenceET = CalcReferenceET_Hamon(T, DayLength); // mm/day
            float Egp = ReferenceET * (float)Math.Exp(-0.4f * LAI); // mm/day
            return Egp * DaySpan; // mm/month
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
        public static float CalcPotentialGroundET_LAI(float LAI, float T, float DayLength, float DaySpan, float k)
        {
            float CropCoeff = ((Parameter<float>)Names.GetParameter("ReferenceETCropCoeff")).Value;
            float ReferenceET = CalcReferenceET_Hamon(T, DayLength); // mm/day
            float Egp = CropCoeff * ReferenceET * (float)Math.Exp(-k * LAI); // mm/day
            return Egp * DaySpan; // mm/month
        }

        /// <summary>
        /// Calculate water vapor conductance
        /// </summary>
        /// <param name="CO2"></param>
        /// <param name="Tavg"></param>
        /// <param name="CiElev"></param>
        /// <param name="netPsn"></param>
        /// <returns></returns>
        public static float CalcWVConductance(float CO2, float Tavg, float CiElev, float netPsn)
        {
            float Ca_Ci = CO2 - CiElev;
            float conductance_mol = (float)(netPsn / Ca_Ci * 1.6 * 1000);
            float conductance = (float)(conductance_mol / (444.5 - 1.3667 * Tavg) * 10);
            return conductance;
        }

        /// <summary>
        /// Calculate water use efficiency using photosynthesis,
        /// canopy layer fraction, and transpiration  
        /// </summary>
        /// <param name="grossPsn"></param>
        /// <param name="canopyLayerFrac"></param>
        /// <param name="transpiration"></param>
        /// <returns></returns>
        public static float CalcWUE(float grossPsn, float canopyLayerFrac, float transpiration)
        {
            float JCO2_JH2O = 0;
            if (transpiration > 0)
                JCO2_JH2O = (float)(0.0015f * grossPsn * canopyLayerFrac / transpiration);
            float WUE = JCO2_JH2O * Constants.MCO2_MC;
            return WUE;
        }
    }
}

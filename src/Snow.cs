using System;

namespace Landis.Library.PnETCohorts
{
    public class Snow
    {
        /// <summary>
        /// Calculate snow density (kg/m3, function of daysOfWinter) 
        /// </summary>
        /// <param name="DaysOfWinter"></param>
        /// <returns></returns>
        public static float CalcDensity(int DaysOfWinter)
        {
            return Constants.DensitySnow_intercept + (Constants.DensitySnow_slope * DaysOfWinter);
        }

        /// <summary>
        /// Calculate snow depth (m)
        /// </summary>
        /// <param name="DaysOfWinter"></param>
        /// <param name="Snowpack"></param>
        /// <returns></returns>
        public static float CalcDepth(float DensitySnow_kg_m3, float Snowpack)
        {
            return Constants.DensityWater * Snowpack / DensitySnow_kg_m3 / 1000F;
        }

        /// <summary>
        /// Calculate thermal conductivity of snow (kJ/m.d.K) 
        /// includes unit conversion from W to kJ
        /// based on CLM model - https://escomp.github.io/ctsm-docs/doc/build/html/tech_note/Soil_Snow_Temperatures/CLM50_Tech_Note_Soil_Snow_Temperatures.html#soil-and-snow-thermal-properties
        /// Eq. 85 in Jordan (1991)
        /// </summary>
        /// <param name="DensitySnow_kg_m3"></param>
        /// <returns></returns>
        public static float CalcThermalConductivity(float DensitySnow_kg_m3)
        {
            return (float)(Constants.ThermalConductivityAir_Watts + ((0.0000775 * DensitySnow_kg_m3) + (0.000001105 * Math.Pow(DensitySnow_kg_m3, 2))) * (Constants.ThermalConductivityIce_Watts - Constants.ThermalConductivityAir_Watts)) * 3.6F * 24F;
        }

        /// <summary>
        /// Calculate snow thermal damping coefficient
        /// based on CLM model - https://escomp.github.io/ctsm-docs/doc/build/html/tech_note/Soil_Snow_Temperatures/CLM50_Tech_Note_Soil_Snow_Temperatures.html#soil-and-snow-thermal-properties
        /// Eq. 85 in Jordan (1991)
        /// </summary>
        /// <param name="ThermalConductivity_Snow"></param>
        /// <returns></returns>
        public static float CalcThermalDamping(float ThermalConductivity_Snow)
        {
            return (float)Math.Sqrt(Constants.omega / (2.0F * ThermalConductivity_Snow));
        }

        /// <summary>
        /// Thermal damping ratio for snow
        /// adapted from Kang et al. (2000) and Liang et al. (2014)
        /// based on CLM model - https://escomp.github.io/ctsm-docs/doc/build/html/tech_note/Soil_Snow_Temperatures/CLM50_Tech_Note_Soil_Snow_Temperatures.html#soil-and-snow-thermal-properties
        /// Eq. 85 in Jordan (1991)
        /// </summary>
        /// <param name="SnowDepth"></param>
        /// <param name="ThermalDamping"></param>
        /// <returns></returns>
        public static float CalcDampingRatio(float SnowDepth, float ThermalDamping)
        {
            return (float)Math.Exp(-1.0F * SnowDepth * ThermalDamping);
        }

        /// <summary>
        /// Snowmelt rate can range between 1.6 to 6.0 mm/degree day, and default should be 2.74 according to NRCS Part 630 Hydrology National Engineering Handbook (Chapter 11: Snowmelt)
        /// </summary>
        /// <param name="Tavg"></param>
        /// <param name="DaySpan"></param>
        /// <returns></returns>
        public static float CalcMaxSnowMelt(float Tavg, float DaySpan)
        {
            return (float)2.74f * Math.Max(0F, Tavg) * DaySpan;
        }

        /// <summary>
        /// Calculate actual snow melt
        /// </summary>
        /// <param name="Snowpack"></param>
        /// <param name="Tavg"></param>
        /// <param name="DaySpan"></param>
        /// <param name="Name"></param>
        /// <param name="Location"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static float CalcMelt(float Snowpack, float Tavg, float DaySpan, string Name, string Location)
        {
            float Snowmelt = Math.Min(Snowpack, CalcMaxSnowMelt(Tavg, DaySpan)); // mm
            if (Snowmelt < 0)
                throw new Exception("Error, snowmelt = " + Snowmelt + "; ecoregion = " + Name + "; site = " + Location);
            return Snowmelt;
        }

        /// <summary>
        /// Snow fraction of ground cover
        /// </summary>
        /// <param name="Tavg"></param>
        /// <returns></returns>
        public static float CalcSnowFrac(float Tavg)
        {
            return (float)Math.Max(0F, Math.Min(1F, (Tavg - 2F) / -7F));
        }

        // Calculate depth of new snow
        public static float CalcNewSnowDepth(float Tavg, float Prec, float SublimationFrac)
        {
            float NewSnow = CalcSnowFrac(Tavg) * Prec;
            float NewSnowDepth = NewSnow * (1 - SublimationFrac); // (mm) Account for sublimation here
            if (NewSnowDepth < 0 || NewSnowDepth > Prec)
                throw new Exception("Error, newSnowDepth = " + NewSnowDepth + " availablePrecipitation = " + Prec);
            return NewSnowDepth;
        }
    }
}

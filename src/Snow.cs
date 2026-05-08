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
            float density = Constants.DensitySnow_intercept + (Constants.DensitySnow_slope * DaysOfWinter);
            return density;
        }

        /// <summary>
        /// Calculate snow depth (m)
        /// </summary>
        /// <param name="DensitySnow_kg_m3"></param>
        /// <param name="Snowpack"></param>
        /// <returns></returns>
        public static float CalcDepth(float DensitySnow_kg_m3, float Snowpack)
        {
            float depth = Constants.DensityWater * Snowpack / DensitySnow_kg_m3 / 1000F;
            return depth;
        }

        /// <summary>
        /// Calculate volumetric heat capacity of snow (kJ/m3/K) 
        /// </summary>
        /// <param name="DensitySnow_kg_m3"></param>
        /// <returns></returns>
        public static float CalcHeatCapacity(float DensitySnow_kg_m3)
        {
            float heatcapacity = Constants.snowHeatCapacity * DensitySnow_kg_m3 / 1000F;
            return heatcapacity;
        }

        /// <summary>
        /// Calculate thermal conductivity of snow (kJ/m.d.K) 
        /// includes unit conversion from W to kJ
        /// </summary>
        /// <param name="DensitySnow_kg_m3"></param>
        /// <returns></returns>
        public static float CalcThermalConductivity(float DensitySnow_kg_m3)
        {
            float conductivity = (float)(Constants.ThermalConductivityAir_Watts + ((0.0000775 * DensitySnow_kg_m3) + (0.000001105 * Math.Pow(DensitySnow_kg_m3, 2))) * (Constants.ThermalConductivityIce_Watts - Constants.ThermalConductivityAir_Watts)) * 3.6F * 24F;
            return conductivity;
        }

        /// <summary>
        /// Calculate thermal diffusivity of snow (mm2/s) 
        /// </summary>
        /// <param name="DensitySnow_kg_m3"></param>
        /// <returns></returns>
        public static float CalcThermalDiffusivity(float DensitySnow_kg_m3)
        {
            float diffusivity = (float)Constants.Million / (float)Constants.SecondsPerDay * CalcThermalConductivity(DensitySnow_kg_m3) / CalcHeatCapacity(DensitySnow_kg_m3);
            return diffusivity;
        }

        /// <summary>
        /// Calculate snow thermal damping coefficient
        /// based on Ochsner, 2019: Rain or Shine (textbook), Eq. 13-5
        /// https://open.library.okstate.edu/rainorshine/chapter/13-4-sub-surface-soil-temperatures/
        /// </summary>
        /// <param name="DensitySnow_kg_m3"></param>
        /// <returns></returns>
        public static float CalcThermalDamping(float DensitySnow_kg_m3)
        {
            float damping = (float)Math.Sqrt(2.0F * CalcThermalDiffusivity(DensitySnow_kg_m3) / Constants.omega);
            return damping;
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
            float dampingratio = (float)Math.Exp(-1.0F * SnowDepth * ThermalDamping);
            return dampingratio;
        }

        /// <summary>
        /// Snowmelt rate can range between 1.6 to 6.0 mm/degree day, and default should be 2.74 according to NRCS Part 630 Hydrology National Engineering Handbook (Chapter 11: Snowmelt)
        /// </summary>
        /// <param name="Tavg"></param>
        /// <param name="DaySpan"></param>
        /// <returns></returns>
        public static float CalcMaxSnowMelt(float Tavg, float DaySpan)
        {
            float maxsnowmelt = (float)2.74f * Math.Max(0F, Tavg) * DaySpan;
            return maxsnowmelt;
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
            float snowmelt = Math.Min(Snowpack, CalcMaxSnowMelt(Tavg, DaySpan)); // mm
            if (snowmelt < 0)
                throw new Exception("Error, snowmelt = " + snowmelt + "; ecoregion = " + Name + "; site = " + Location);
            return snowmelt;
        }

        /// <summary>
        /// Snow fraction of ground cover
        /// </summary>
        /// <param name="Tavg"></param>
        /// <returns></returns>
        public static float CalcSnowFrac(float Tavg)
        {
            float snowfrac = (float)Math.Max(0F, Math.Min(1F, (Tavg - 2F) / -7F));
            return snowfrac;
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

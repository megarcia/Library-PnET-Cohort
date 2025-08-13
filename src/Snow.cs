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
            float DensitySnow_kg_m3 = Constants.DensitySnow_intercept + (Constants.DensitySnow_slope * DaysOfWinter);
            return DensitySnow_kg_m3;
        }

        /// <summary>
        /// Calculate snow depth (m)
        /// </summary>
        /// <param name="DaysOfWinter"></param>
        /// <param name="Snowpack"></param>
        /// <returns></returns>
        public static float CalcDepth(float DensitySnow_kg_m3, float Snowpack)
        {
            float DepthSnow = Constants.DensityWater * Snowpack / DensitySnow_kg_m3 / 1000;
            return DepthSnow;
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
            float ThermalConductivity = (float)(Constants.ThermalConductivityAir_Watts + ((0.0000775 * DensitySnow_kg_m3) + (0.000001105 * Math.Pow(DensitySnow_kg_m3, 2))) * (Constants.ThermalConductivityIce_Watts - Constants.ThermalConductivityAir_Watts)) * 3.6F * 24F;
            return ThermalConductivity;
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
            float ThermalDamping = (float)Math.Sqrt(Constants.omega / (2.0F * ThermalConductivity_Snow));
            return ThermalDamping;
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
            float DampingRatio = (float)Math.Exp(-1.0F * SnowDepth * ThermalDamping);
            return DampingRatio;
        }
    }
}

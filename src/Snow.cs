using System;

namespace Landis.Library.PnETCohorts
{
    public class Snow
    {
        /// <summary>
        /// Calculate snow density (kg/m3, function of daysOfWinter) 
        /// </summary>
        /// <param name="daysOfWinter"></param>
        /// <returns></returns>
        public static float CalcDensity(int daysOfWinter)
        {
            float densitySnow_kg_m3 = Constants.DensitySnow_intercept + (Constants.DensitySnow_slope * daysOfWinter);
            return densitySnow_kg_m3;
        }

        /// <summary>
        /// Calculate snow depth (m)
        /// </summary>
        /// <param name="daysOfWinter"></param>
        /// <param name="snowpack"></param>
        /// <returns></returns>
        public static float CalcDepth(float densitySnow_kg_m3, float snowpack)
        {
            float depthSnow = Constants.DensityWater * snowpack / densitySnow_kg_m3 / 1000;
            return depthSnow;
        }

        /// <summary>
        /// Calculate thermal conductivity of snow (kJ/m.d.K) 
        /// includes unit conversion from W to kJ
        /// based on CLM model - https://escomp.github.io/ctsm-docs/doc/build/html/tech_note/Soil_Snow_Temperatures/CLM50_Tech_Note_Soil_Snow_Temperatures.html#soil-and-snow-thermal-properties
        /// Eq. 85 in Jordan (1991)
        /// </summary>
        /// <param name="densitySnow_kg_m3"></param>
        /// <returns></returns>
        public static float CalcThermalConductivity(float densitySnow_kg_m3)
        {
            float thermalConductivity = (float)(Constants.ThermalConductivityAir_Watts + ((0.0000775 * densitySnow_kg_m3) + (0.000001105 * Math.Pow(densitySnow_kg_m3, 2))) * (Constants.ThermalConductivityIce_Watts - Constants.ThermalConductivityAir_Watts)) * 3.6F * 24F;
            return thermalConductivity;
        }

        /// <summary>
        /// Calculate snow thermal damping coefficient
        /// based on CLM model - https://escomp.github.io/ctsm-docs/doc/build/html/tech_note/Soil_Snow_Temperatures/CLM50_Tech_Note_Soil_Snow_Temperatures.html#soil-and-snow-thermal-properties
        /// Eq. 85 in Jordan (1991)
        /// </summary>
        /// <param name="thermalConductivity_Snow"></param>
        /// <returns></returns>
        public static float CalcThermalDamping(float thermalConductivity_Snow)
        {
            float thermalDamping = (float)Math.Sqrt(Constants.omega / (2.0F * thermalConductivity_Snow));
            return thermalDamping;
        }

        /// <summary>
        /// Damping ratio for snow
        /// adapted from Kang et al. (2000) and Liang et al. (2014)
        /// based on CLM model - https://escomp.github.io/ctsm-docs/doc/build/html/tech_note/Soil_Snow_Temperatures/CLM50_Tech_Note_Soil_Snow_Temperatures.html#soil-and-snow-thermal-properties
        /// Eq. 85 in Jordan (1991)
        /// </summary>
        /// <param name="snowDepth"></param>
        /// <param name="thermalDamping"></param>
        /// <returns></returns>
        public static float CalcDampingRatio(float snowDepth, float thermalDamping)
        {
            float dampingRatio = (float)Math.Exp(-1.0F * snowDepth * thermalDamping);
            return dampingRatio;
        }
    }
}

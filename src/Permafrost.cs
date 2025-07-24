using System;
using System.Collections.Generic;
using System.Text;

namespace Landis.Library.PnETCohorts
{
    public class Permafrost
    {
        public static SortedList<float, float> CalcMonthlySoilTemps(SortedList<float, float> depthTempDict, IEcoregionPnET Ecoregion, int daysOfWinter, float snowpack, IHydrology hydrology, float lastTempBelowSnow)
        {
            float[] snowResults = CalcSnowDepth(daysOfWinter, snowpack);
            float snowDepth = snowResults[0];
            float DensitySnow_kg_m3 = snowResults[1];
            if (Ecoregion.Variables.Tavg >= 0)
            {
                float fracAbove0 = Ecoregion.Variables.Tmax / (Ecoregion.Variables.Tmax - Ecoregion.Variables.Tmin);
                snowDepth = snowDepth * fracAbove0;
            }
            // based on CLM model - https://escomp.github.io/ctsm-docs/doc/build/html/tech_note/Soil_Snow_Temperatures/CLM50_Tech_Note_Soil_Snow_Temperatures.html#soil-and-snow-thermal-properties
            // Eq. 85 in Jordan (1991)
            // Damping ratio for snow - adapted from Kang et al. (2000) and Liang et al. (2014)
            float snowThermalDamping = CalcSnowThermalDamping(DensitySnow_kg_m3);
            float DRz_snow = (float)Math.Exp(-1.0F * snowDepth * snowThermalDamping); 
            // Frozen soil calculations - from "Soil thawing worksheet.xlsx"
            float soilPorosity = Ecoregion.Porosity / Ecoregion.RootingDepth;  //m3/m3
            float soilWaterContent = hydrology.SoilWaterContent / Ecoregion.RootingDepth;  //m3/m3
            float ga = 0.035F + 0.298F * (soilWaterContent / soilPorosity);
            float Fa = (2.0F / 3.0F / (1.0F + ga * ((Constants.ThermalConductivityAir_kJperday / Constants.ThermalConductivityWater_kJperday) - 1.0F))) + (1.0F / 3.0F / (1.0F + (1.0F - 2.0F * ga) * ((Constants.ThermalConductivityAir_kJperday / Constants.ThermalConductivityWater_kJperday) - 1.0F))); // ratio of air temp gradient
            float Fs = Hydrology_SaxtonRawls.GetFs(Ecoregion.SoilType);
            float ThermalConductivitySoil = Hydrology_SaxtonRawls.GetThermalConductivitySoil(Ecoregion.SoilType);
            float ThermalConductivity_theta = (Fs * (1.0F - soilPorosity) * ThermalConductivitySoil + Fa * (soilPorosity - soilWaterContent) * Constants.ThermalConductivityAir_kJperday + soilWaterContent * Constants.ThermalConductivityWater_kJperday) / (Fs * (1.0F - soilPorosity) + Fa * (soilPorosity - soilWaterContent) + soilWaterContent); //soil thermal conductivity (kJ/m/d/K)
            float D = ThermalConductivity_theta / Hydrology_SaxtonRawls.GetCTheta(Ecoregion.SoilType);  //m2/day
            float Dmonth = D * Ecoregion.Variables.DaySpan; // m2/month
            float ks = Dmonth * 1000000F / (Ecoregion.Variables.DaySpan * Constants.SecondsPerDay); // mm2/s
            float d = (float)Math.Pow(Constants.omega / (2.0F * Dmonth), 0.5);
            float maxDepth = Ecoregion.RootingDepth + Ecoregion.LeakageFrostDepth;
            float freezeDepth = maxDepth;
            float testDepth = 0;
            float tempBelowSnow = Ecoregion.Variables.Tavg;
            if (snowDepth > 0)
                tempBelowSnow = lastTempBelowSnow + (Ecoregion.Variables.Tavg - lastTempBelowSnow) * DRz_snow;
            lastTempBelowSnow = tempBelowSnow;
            while (testDepth <= (maxDepth / 1000.0))
            {
                float DRz = (float)Math.Exp(-1.0F * testDepth * d); // adapted from Kang et al. (2000) and Liang et al. (2014)
                float zTemp = depthTempDict[testDepth] + (tempBelowSnow - depthTempDict[testDepth]) * DRz;
                depthTempDict[testDepth] = zTemp;
                if (testDepth == 0F)
                    testDepth = 0.10F;
                else if (testDepth == 0.10F)
                    testDepth = 0.25F;
                else
                    testDepth += 0.25F;
            }
            if (maxDepth < 100) // mm
                depthTempDict[0.1F] = depthTempDict[0];
            return depthTempDict;
        }

        /// <summary>
        /// Calculate snow density (kg/m3, function of daysOfWinter) 
        /// and snow depth (m)
        /// </summary>
        /// <param name="daysOfWinter"></param>
        /// <param name="snowpack"></param>
        /// <returns></returns>
        public static float[] CalcSnowDepth(int daysOfWinter, float snowpack)
        {
            float DensitySnow_kg_m3 = Constants.DensitySnow_intercept + (Constants.DensitySnow_slope * daysOfWinter);
            float snowDepth = Constants.DensityWater * snowpack / DensitySnow_kg_m3 / 1000;
            float[] snowArray = new float[2];
            snowArray[0] = snowDepth;
            snowArray[1] = DensitySnow_kg_m3;
            return snowArray;
        }

        /// <summary>
        /// Calculate snow thermal damping coefficient
        /// based on CLM model - https://escomp.github.io/ctsm-docs/doc/build/html/tech_note/Soil_Snow_Temperatures/CLM50_Tech_Note_Soil_Snow_Temperatures.html#soil-and-snow-thermal-properties
        /// Eq. 85 in Jordan (1991)
        /// ThermalConductivity_Snow (kJ/m.d.K) includes unit conversion from W to kJ
        /// </summary>
        /// <param name="DensitySnow_kg_m3"></param>
        /// <returns></returns>
        public static float CalcSnowThermalDamping(float DensitySnow_kg_m3)
        {
            float ThermalConductivity_Snow = (float)(Constants.ThermalConductivityAir_Watts + ((0.0000775 * DensitySnow_kg_m3) + (0.000001105 * Math.Pow(DensitySnow_kg_m3, 2))) * (Constants.ThermalConductivityIce_Watts - Constants.ThermalConductivityAir_Watts)) * 3.6F * 24F;
            float SnowThermalDamping = (float)Math.Sqrt(Constants.omega / (2.0F * ThermalConductivity_Snow));
            return SnowThermalDamping;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Landis.Library.PnETCohorts
{
    public class FrozenSoils
    {
        public static SortedList<float, float> CalcMonthlySoilTemps(SortedList<float, float> depthTempDict, IPnETEcoregionData Ecoregion, int daysOfWinter, float snowpack, IHydrology hydrology, float lastTempBelowSnow)
        {
            //
            // Snow calculations, now handled in Snow class
            float densitySnow_kg_m3 = Snow.CalcDensity(daysOfWinter);
            float snowDepth = Snow.CalcDepth(densitySnow_kg_m3, snowpack);
            if (Ecoregion.Variables.Tavg >= 0)
            {
                float fracAbove0 = Ecoregion.Variables.Tmax / (Ecoregion.Variables.Tmax - Ecoregion.Variables.Tmin);
                snowDepth *= fracAbove0;
            }
            float snowThermalConductivity = Snow.CalcThermalConductivity(densitySnow_kg_m3);
            float snowThermalDamping = Snow.CalcThermalDamping(snowThermalConductivity);
            float snowDampingRatio = Snow.CalcDampingRatio(snowDepth, snowThermalDamping);
            //
            // Frozen soil calculations
            float soilPorosity = Ecoregion.Porosity / Ecoregion.RootingDepth;  //m3/m3
            float soilWaterContent = hydrology.SoilWaterContent / Ecoregion.RootingDepth;  //m3/m3
            float ga = 0.035F + 0.298F * (soilWaterContent / soilPorosity);
            float Fa = (2.0F / 3.0F / (1.0F + ga * ((Constants.ThermalConductivityAir_kJperday / Constants.ThermalConductivityWater_kJperday) - 1.0F))) + (1.0F / 3.0F / (1.0F + (1.0F - 2.0F * ga) * ((Constants.ThermalConductivityAir_kJperday / Constants.ThermalConductivityWater_kJperday) - 1.0F))); // ratio of air temp gradient
            float Fs = Hydrology_SaxtonRawls.GetFs(Ecoregion.SoilType);
            float ThermalConductivitySoil = Hydrology_SaxtonRawls.GetThermalConductivitySoil(Ecoregion.SoilType);
            float ThermalConductivity_theta = (Fs * (1.0F - soilPorosity) * ThermalConductivitySoil + Fa * (soilPorosity - soilWaterContent) * Constants.ThermalConductivityAir_kJperday + soilWaterContent * Constants.ThermalConductivityWater_kJperday) / (Fs * (1.0F - soilPorosity) + Fa * (soilPorosity - soilWaterContent) + soilWaterContent); //soil thermal conductivity (kJ/m/d/K)
            float D = ThermalConductivity_theta / Hydrology_SaxtonRawls.GetCTheta(Ecoregion.SoilType);  //m2/day
            float Dmonth = D * Ecoregion.Variables.DaySpan; // m2/month
            float d = (float)Math.Pow(Constants.omega / (2.0F * Dmonth), 0.5);
            float maxDepth = Ecoregion.RootingDepth + Ecoregion.LeakageFrostDepth;
            float testDepth = 0;
            float tempBelowSnow = Ecoregion.Variables.Tavg;
            if (snowDepth > 0)
                tempBelowSnow = lastTempBelowSnow + (Ecoregion.Variables.Tavg - lastTempBelowSnow) * snowDampingRatio;
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
    }
}

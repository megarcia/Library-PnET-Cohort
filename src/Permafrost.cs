using System;
using System.Collections.Generic;
using System.Text;

namespace Landis.Library.PnETCohorts
{
    public class Permafrost
    {
        public static SortedList<float, float> CalcMonthlySoilTemps(SortedList<float, float> depthTempDict,IEcoregionPnET Ecoregion, int daysOfWinter, float snowPack, IHydrology hydrology, float lastTempBelowSnow)
        {
            float[] snowResults = CalcSnowDepth(daysOfWinter, snowPack);
            float snowDepth = snowResults[0];
            float DensitySnow_kg_m3 = snowResults[1];
            if (Ecoregion.Variables.Tavg >= 0)
            {
                float fracAbove0 = Ecoregion.Variables.Tmax / (Ecoregion.Variables.Tmax - Ecoregion.Variables.Tmin);
                snowDepth = snowDepth * fracAbove0;
            }
            // from CLM model - https://escomp.github.io/ctsm-docs/doc/build/html/tech_note/Soil_Snow_Temperatures/CLM50_Tech_Note_Soil_Snow_Temperatures.html#soil-and-snow-thermal-properties
            // Eq. 85 - Jordan (1991)
            float damping = CalcSnowDamping(DensitySnow_kg_m3);
            float DRz_snow = (float)Math.Exp(-1.0F * snowDepth * damping); // Damping ratio for snow - adapted from Kang et al. (2000) and Liang et al. (2014)
            // Permafrost calculations - from "Soil thawing worksheet.xlsx"
            float porosity = Ecoregion.Porosity / Ecoregion.RootingDepth;  //m3/m3
            float waterContent = hydrology.Water / Ecoregion.RootingDepth;  //m3/m3
            float ga = 0.035F + 0.298F * (waterContent / porosity);
            float Fa = (2.0F / 3.0F / (1.0F + ga * ((Constants.ThermalConductivityAir_kJperday / Constants.ThermalConductivityWater_kJperday) - 1.0F))) + (1.0F / 3.0F / (1.0F + (1.0F - 2.0F * ga) * ((Constants.ThermalConductivityAir_kJperday / Constants.ThermalConductivityWater_kJperday) - 1.0F))); // ratio of air temp gradient
            float Fs = PressureHeadSaxton_Rawls.GetFs(Ecoregion.SoilType);
            float ThermalConductivitySoil = PressureHeadSaxton_Rawls.GetLambda_s(Ecoregion.SoilType);
            float ThermalConductivity_theta = (Fs * (1.0F - porosity) * ThermalConductivitySoil + Fa * (porosity - waterContent) * Constants.ThermalConductivityAir_kJperday + waterContent * Constants.ThermalConductivityWater_kJperday) / (Fs * (1.0F - porosity) + Fa * (porosity - waterContent) + waterContent); //soil thermal conductivity (kJ/m/d/K)
            float D = ThermalConductivity_theta / PressureHeadSaxton_Rawls.GetCTheta(Ecoregion.SoilType);  //m2/day
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
                if (testDepth == 0f)
                    testDepth = 0.10f;
                else if (testDepth == 0.10f)
                    testDepth = 0.25f;
                else
                    testDepth += 0.25F;
            }
            if (maxDepth < 100)
                depthTempDict[0.1f] = depthTempDict[0];
            return depthTempDict;
        }

        public static float[] CalcSnowDepth(int daysOfWinter, float snowPack)
        {
            float DensitySnow_kg_m3 = Constants.DensitySnow_intercept + (Constants.DensitySnow_slope * daysOfWinter); //kg/m3
            float snowDepth = Constants.DensityWater * snowPack / DensitySnow_kg_m3 / 1000; //m
            float[] snowArray = new float[2];
            snowArray[0] = snowDepth;
            snowArray[1] = DensitySnow_kg_m3;
            return snowArray;
        }

        public static float CalcSnowDamping(float DensitySnow_kg_m3)
        {
            // from CLM model - https://escomp.github.io/ctsm-docs/doc/build/html/tech_note/Soil_Snow_Temperatures/CLM50_Tech_Note_Soil_Snow_Temperatures.html#soil-and-snow-thermal-properties
            // Eq. 85 - Jordan (1991)
            // ThermalConductivity_Snow in kJ/m.d.K includes unit conversion from W to kJ
            float ThermalConductivity_Snow = (float)(Constants.ThermalConductivityAir_Watts + ((0.0000775 * DensitySnow_kg_m3) + (0.000001105 * Math.Pow(DensitySnow_kg_m3, 2))) * (Constants.ThermalConductivityIce_Watts - Constants.ThermalConductivityAir_Watts)) * 3.6F * 24F;
            float damping = (float)Math.Sqrt(Constants.omega / (2.0F * ThermalConductivity_Snow));
            return damping;
        }
    }
}

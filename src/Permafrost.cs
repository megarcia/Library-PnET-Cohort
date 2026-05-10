using System;
using System.Collections.Generic;
using System.Text;


namespace Landis.Library.PnETCohorts
{
    public class Permafrost
    {
        public static SortedList<float, float> CalculateMonthlySoilTemps(SortedList<float, float> depthTempDict,IEcoregionPnET Ecoregion, int daysOfWinter, float snowpack, IHydrology hydrology, float lastTempBelowSnow)
        {
            float snowDensity = Snow.CalcDensity(daysOfWinter);
            float[] snowResults = Snow.CalcDepth(snowDensity, snowpack);
            float sno_dep = snowResults[0];
            float Psno_kg_m3 = snowResults[1];
            if (Ecoregion.Variables.Tave >= 0)
            {
                float fracAbove0 = Ecoregion.Variables.Tmax / (Ecoregion.Variables.Tmax - Ecoregion.Variables.Tmin);
                sno_dep = sno_dep * fracAbove0;
            }
            float damping = Snow.CalcThermalDamping(Psno_kg_m3);
            float DRz_snow = Snow.CalcDampingRatio(sno_dep, damping);
            // Permafrost calculations - from "Soil thawing worksheet.xlsx"
            // NOTE by MGarcia on 20260510: if you don't provide the worksheet/file as a 
            // supplement to the Development Guide, don't reference the worksheet. 
            float porosity = Ecoregion.Porosity / Ecoregion.RootingDepth;  // m3/m3
            float waterContent = hydrology.Water / Ecoregion.RootingDepth;  // m3/m3
            float ga = 0.035F + 0.298F * (waterContent / porosity);
            float Fa = (2.0F / 3.0F / (1.0F + ga * ((Constants.lambda_a / Constants.lambda_w) - 1.0F))) + (1.0F / 3.0F / (1.0F + (1.0F - 2.0F * ga) * ((Constants.lambda_a / Constants.lambda_w) - 1.0F))); // ratio of air temp gradient
            float Fs = PressureHeadSaxton_Rawls.GetFs(Ecoregion.SoilType);
            float lambda_s = PressureHeadSaxton_Rawls.GetLambda_s(Ecoregion.SoilType);
            float lambda_theta = (Fs * (1.0F - porosity) * lambda_s + Fa * (porosity - waterContent) * Constants.lambda_a + waterContent * Constants.lambda_w) / (Fs * (1.0F - porosity) + Fa * (porosity - waterContent) + waterContent); //soil thermal conductivity (kJ/m/d/K)
            float D = lambda_theta / PressureHeadSaxton_Rawls.GetCTheta(Ecoregion.SoilType);  // m2/day
            float Dmonth = D * Ecoregion.Variables.DaySpan; // m2/month
            float ks = Dmonth * Constants.Million / (Ecoregion.Variables.DaySpan * Constants.SecondsPerDay); // mm2/s
            float d = (float)Math.Pow(Constants.omega / (2.0F * Dmonth), 0.5);  // source for this equation?
            float maxDepth = Ecoregion.RootingDepth + Ecoregion.LeakageFrostDepth;
            float freezeDepth = maxDepth;
            float tempBelowSnow = Ecoregion.Variables.Tave;
            if (sno_dep > 0)
                tempBelowSnow = lastTempBelowSnow + (Ecoregion.Variables.Tave - lastTempBelowSnow) * DRz_snow;
            lastTempBelowSnow = tempBelowSnow;
            float testDepth = 0;
            while (testDepth <= (maxDepth / 1000.0))
            {
                float DRz = Snow.CalcDampingRatio(testDepth, d);
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
    }
}

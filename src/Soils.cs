using System;
using System.Collections.Generic;

namespace Landis.Library.PnETCohorts
{
    public class Soils
    {
        public static SortedList<float, float> CalcMonthlySoilTemps(SortedList<float, float> depthTempDict, IPnETEcoregionData Ecoregion, int daysOfWinter, float snowpack, IHydrology hydrology, float lastTempBelowSnow)
        {
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
            // Soil thermal conductivity via De Vries model (convert to kJ/m.d.K)
            float ThermalConductivity_theta = CalcThermalConductivitySoil_Watts(hydrology.SoilWaterContent, Ecoregion.Porosity, Ecoregion.SoilType) / Constants.Convert_kJperday_to_Watts;
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
                // adapted from Kang et al. (2000) and Liang et al. (2014)
                float DRz = (float)Math.Exp(-1.0F * testDepth * d);
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
        /// Calculate ga term in De Vries model of soil thermal conductivity
        /// </summary>
        /// <param name="WaterContent"></param>
        /// <param name="Porosity"></param>
        /// <returns></returns>
        public static float CalcAirShapeFactor(float WaterContent, float Porosity)
        {
            float ga = 0.035F + 0.298F * (WaterContent / Porosity);
            return ga;
        }

        /// <summary>
        /// Calculate thermal conductivity of De Vries "fluid"
        /// </summary>
        /// <param name="WaterContent"></param>
        /// <param name="ClayFrac"></param>
        /// <returns></returns>
        public static float CalcThermalConductivityFluid(float WaterContent, float ClayFrac)
        {
            float theta0 = 0.33f * ClayFrac + 0.078f;
            float ratio = WaterContent / theta0;
            float q = 7.25f * ClayFrac + 2.52f;
            float fw = 1F / (1F + (float)Math.Pow(ratio, -q));
            return fw;
        }

        /// <summary>
        /// Calculate weights in De Vries model of soil thermal conductivity
        /// </summary>
        /// <param name="ga"></param>
        /// <param name="gc"></param>
        /// <param name="Numerator"></param>
        /// <param name="ThermalConductivityFluid"></param>
        /// <returns></returns>
        public static float CalcDeVriesWeight(float ga, float Numerator, float ThermalConductivityFluid)
        {
            float gc = 1F - 2F * ga;
            float term1 = 2F / 3F / (1F + ga * ((Numerator / ThermalConductivityFluid) - 1F));
            float term2 = 1F / 3F / (1F + gc * ((Numerator / ThermalConductivityFluid) - 1F));
            float weight = term1 + term2;
            return weight;
        }

        /// <summary>
        /// Calculate thermal conductivity of moist/wet soil via De Vries model
        /// (De Vries, 1963) summarized in (Campbell et al., 1994; Tong et al., 2016) 
        /// </summary>
        /// <param name="WaterContent"></param>
        /// <param name="Porosity"></param>
        /// <param name="SoilType"></param>
        /// <returns></returns>
        public static float CalcThermalConductivitySoil_Watts(float WaterContent, float Porosity, string SoilType)
        {
            float ga = CalcAirShapeFactor(WaterContent, Porosity);
            float ClayFrac = Hydrology_SaxtonRawls.GetClayFrac(SoilType);
            float ThermalConductivityFluid = CalcThermalConductivityFluid(WaterContent, ClayFrac);
            float Kair = CalcDeVriesWeight(ga, Constants.ThermalConductivityAir_Watts, ThermalConductivityFluid);
            float ThermalConductivitySoil_Watts = Hydrology_SaxtonRawls.GetThermalConductivitySoil(SoilType) * Constants.Convert_kJperday_to_Watts;
            float Ksoil = CalcDeVriesWeight(ga, ThermalConductivitySoil_Watts, ThermalConductivityFluid);
            float Kwater = CalcDeVriesWeight(ga, Constants.ThermalConductivityWater_Watts, ThermalConductivityFluid);
            float AirContent = Porosity - WaterContent;
            float numerator_air = Kair * AirContent * Constants.ThermalConductivityAir_Watts;
            float numerator_soil = Ksoil * (1F - Porosity) * ThermalConductivitySoil_Watts;
            float numerator_water = Kwater * WaterContent * Constants.ThermalConductivityWater_Watts;
            float numerator = numerator_air + numerator_soil + numerator_water;
            float denominator = Kair * AirContent + Ksoil * (1F - Porosity) + Kwater * WaterContent;
            float ThermalConductivitySoil = numerator / denominator;
            return ThermalConductivitySoil;
        }
    }
}

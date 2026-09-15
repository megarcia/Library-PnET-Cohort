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
            float snowThermalDamping = Snow.CalcThermalDamping(densitySnow_kg_m3);
            float snowDampingRatio = Snow.CalcDampingRatio(snowDepth, snowThermalDamping);
            // Soil thermal conductivity via De Vries model (converted to kJ/m.d.K) (see below)
            float ThermalConductivity_theta = CalcThermalConductivitySoil_Watts(hydrology.SoilWaterContent, Ecoregion.Porosity, Ecoregion.SoilType) / Constants.Convert_kJperday_to_Watts;
            float Diffusivity = ThermalConductivity_theta / Hydrology_SaxtonRawls.GetCTheta(Ecoregion.SoilType);  //m2/day
            float Diffusivity_month = Diffusivity * Ecoregion.Variables.DaySpan; // m2/month
            float damping = (float)Math.Sqrt(2.0f * Diffusivity_month / Constants.omega);  //MG20260915 corrected
            float maxDepth = Ecoregion.RootingDepth + Ecoregion.LeakageFrostDepth;
            float testDepth = 0f;
            float tempBelowSnow = Ecoregion.Variables.Tavg;
            if (snowDepth > 0)
                tempBelowSnow = lastTempBelowSnow + (Ecoregion.Variables.Tavg - lastTempBelowSnow) * snowDampingRatio;
            while (testDepth <= (maxDepth / 1000.0f))
            {
                float DRz = (float)Math.Exp(-1.0f * testDepth / damping);  //MG20260915 corrected
                float zTemp = depthTempDict[testDepth] + (tempBelowSnow - depthTempDict[testDepth]) * DRz;
                depthTempDict[testDepth] = zTemp;
                if (testDepth == 0.0)
                    testDepth = 0.10f;
                else if (testDepth == 0.10)
                    testDepth = 0.25f;
                else
                    testDepth += 0.25f;
            }
            if (maxDepth < 100.0) // mm
                depthTempDict[0.1f] = depthTempDict[0];
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
            float ga = 0.035f + 0.298f * (WaterContent / Porosity);
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
            float fw = 1.0f / (1.0f + (float)Math.Pow(ratio, -q));
            return fw;
        }

        /// <summary>
        /// Calculate weights in De Vries model of soil thermal conductivity
        /// </summary>
        /// <param name="ga"></param>
        /// <param name="Numerator"></param>
        /// <param name="ThermalConductivityFluid"></param>
        /// <returns></returns>
        public static float CalcDeVriesWeight(float ga, float Numerator, float ThermalConductivityFluid)
        {
            float gc = 1.0f - 2.0f * ga;
            float term1 = 2.0f / 3.0f / (1.0f + ga * ((Numerator / ThermalConductivityFluid) - 1.0f));
            float term2 = 1.0f / 3.0f / (1.0f + gc * ((Numerator / ThermalConductivityFluid) - 1.0f));
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
            float numerator_soil = Ksoil * (1.0f - Porosity) * ThermalConductivitySoil_Watts;
            float numerator_water = Kwater * WaterContent * Constants.ThermalConductivityWater_Watts;
            float numerator = numerator_air + numerator_soil + numerator_water;
            float denominator = Kair * AirContent + Ksoil * (1.0f - Porosity) + Kwater * WaterContent;
            float ThermalConductivitySoil = numerator / denominator;
            return ThermalConductivitySoil;
        }
    }
}

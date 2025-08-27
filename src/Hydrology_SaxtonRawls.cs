using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Library.PnETCohorts
{
    public class Hydrology_SaxtonRawls
    {
        public const string SaxtonAndRawlsParameters = "SaxtonAndRawlsParameters";

        public static List<string> ParameterNames
        {
            get
            {
                return typeof(Hydrology_SaxtonRawls).GetFields().Select(x => x.Name).ToList();
            }
        }

        public static Parameter<string> Sand;
        public static Parameter<string> Clay;
        public static Parameter<string> PctOM;
        public static Parameter<string> DensFactor;
        public static Parameter<string> Gravel;
        static Dictionary<string, float> tensionA = new Dictionary<string, float>();
        static Dictionary<string, float> tensionB = new Dictionary<string, float>();
        static Dictionary<string, float> soilPorosity_OM_comp = new Dictionary<string, float>();
        static Dictionary<string, float> clayFrac = new Dictionary<string, float>();
        static Dictionary<string, float> cTheta = new Dictionary<string, float>();
        static Dictionary<string, float> ThermalConductivitySoil = new Dictionary<string, float>();
        static Dictionary<string, float> Fs = new Dictionary<string, float>();
        Library.Parameters.Ecoregions.AuxParm<float[]> table = new Library.Parameters.Ecoregions.AuxParm<float[]>(Globals.ModelCore.Ecoregions);

        /// <summary>
        /// mm/m of active soil
        /// </summary>
        /// <param name="soilType"></param>
        /// <returns></returns>
        public float GetSoilPorosity(string soilType)
        {
            return soilPorosity_OM_comp[soilType];
        }

        public static float GetClayFrac(string soilType)
        {
            return clayFrac[soilType];
        }

        public static float GetFs(string soilType)
        {
            return Fs[soilType];
        }

        public static float GetThermalConductivitySoil(string soilType)
        {
            return ThermalConductivitySoil[soilType];
        }

        public static float GetCTheta(string soilType)
        {
            return cTheta[soilType];
        }

        public float this[IPnETEcoregionData pnetecoregion, int soilWaterContent]
        {
            get
            {
                try
                {
                    if (soilWaterContent >= table[pnetecoregion].Length)
                        return 0;
                    return table[pnetecoregion][soilWaterContent];
                }
                catch (Exception)
                {
                    throw new Exception("Cannot get pressure head for soil water content " + soilWaterContent);
                }
            }
        }

        /// <summary>
        /// tension = pressurehead (kPA) 
        /// </summary>
        /// <param name="soilWaterContent"></param>
        /// <param name="soilType"></param>
        /// <returns></returns>
        public float CalcSoilWaterPressureHead(double soilWaterContent, string soilType)
        {
            double tension = 0.0;
            if (soilWaterContent <= soilPorosity_OM_comp[soilType])
                tension = tensionA[soilType] * Math.Pow(soilWaterContent, -tensionB[soilType]);
            float pressureHead;
            if (double.IsInfinity(tension))
                pressureHead = float.MaxValue;
            else
            {
                pressureHead = (float)(tension * 0.1019977334);
                if (pressureHead > float.MaxValue)
                    pressureHead = float.MaxValue;
                else
                    pressureHead = (float)Math.Round(pressureHead, 2);
            }
            return pressureHead;
        }

        /// <summary>
        /// Calculate volumetric soil water content (m3 H2O/m3 soil)
        /// </summary>
        /// <param name="tension">float in kPa</param>
        /// <param name="soilType">string</param>
        /// <returns></returns>
        public float CalcSoilWaterContent(float tension, string soilType)
        {
            float soilWaterContent = (float)Math.Pow(tension / tensionA[soilType], 1.0 / -tensionB[soilType]);
            return soilWaterContent;
        }

        public Hydrology_SaxtonRawls()
        {
            Library.Parameters.Ecoregions.AuxParm<string> SoilType = (Library.Parameters.Ecoregions.AuxParm<string>)Names.GetParameter(Names.SoilType);
            Library.Parameters.Ecoregions.AuxParm<float> RootingDepth = (Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)Names.GetParameter(Names.RootingDepth, 0, float.MaxValue);
            table = new Library.Parameters.Ecoregions.AuxParm<float[]>(Globals.ModelCore.Ecoregions);
            Sand = Names.GetParameter("sand");
            Clay = Names.GetParameter("clay");
            PctOM = Names.GetParameter("pctOM");
            DensFactor = Names.GetParameter("densFactor");
            Gravel = Names.GetParameter("gravel");
            foreach (IPnETEcoregionData pnetecoregion in Globals.ModelCore.Ecoregions)
            {
                if (pnetecoregion.Active)
                {
                    List<float> PressureHead = new List<float>();
                    if (tensionB.ContainsKey(SoilType[pnetecoregion]) == false)
                    {
                        double sand = double.Parse(Sand[SoilType[pnetecoregion]]);
                        double clay = double.Parse(Clay[SoilType[pnetecoregion]]);
                        double pctOM = double.Parse(PctOM[SoilType[pnetecoregion]]);
                        double densFactor = double.Parse(DensFactor[SoilType[pnetecoregion]]);
                        double gravel = double.Parse(Gravel[SoilType[pnetecoregion]]);
                        // Moisture at wilting point 
                        double predMoist1500 = -0.024 * sand + 0.487 * clay + 0.006 * pctOM + 0.005 * sand * pctOM - 0.013 * clay * pctOM + 0.068 * sand * clay + 0.031;
                        double predMoist1500adj = predMoist1500 + 0.14 * predMoist1500 - 0.02;
                        // Moisture at field capacity
                        double predMoist33 = -0.251 * sand + 0.195 * clay + 0.011 * pctOM + 0.006 * sand * pctOM - 0.027 * clay * pctOM + 0.452 * sand * clay + 0.299;
                        double predMoist33Adj = predMoist33 + (1.283 * predMoist33 * predMoist33 - 0.374 * predMoist33 - 0.015);
                        double porosMoist33 = 0.278 * sand + 0.034 * clay + 0.022 * pctOM - 0.018 * sand * pctOM - 0.027 * clay * pctOM - 0.584 * sand * clay + 0.078;
                        double porosMoist33Adj = porosMoist33 + (0.636 * porosMoist33 - 0.107);
                        double satPor33 = porosMoist33Adj + predMoist33Adj;
                        double satSandAdj = -0.097 * sand + 0.043;
                        double sandAdjSat = satPor33 + satSandAdj;
                        double density_OM = (1.0 - sandAdjSat) * 2.65;
                        double density_comp = density_OM * densFactor;
                        soilPorosity_OM_comp.Add(SoilType[pnetecoregion], (float)(1.0 - (density_comp / 2.65)));
                        double soilPorosity_change_comp = 1.0 - density_comp / 2.65 - (1.0 - density_OM / 2.65);
                        double moist33_comp = predMoist33Adj + 0.2 * soilPorosity_change_comp;
                        double soilPorosity_moist33_comp = soilPorosity_OM_comp[SoilType[pnetecoregion]] - moist33_comp;
                        double ThermalConductivity = (Math.Log(moist33_comp) - Math.Log(predMoist1500adj)) / (Math.Log(1500) - Math.Log(33));
                        double gravel_red_sat_cond = (1.0 - gravel) / (1.0 - gravel * (1.0 - 1.5 * (density_comp / 2.65)));
                        double satcond_mmhr = 1930 * Math.Pow(soilPorosity_moist33_comp, 3.0 - ThermalConductivity) * gravel_red_sat_cond;
                        double gravels_vol = density_comp / 2.65 * gravel / (1 - gravel * (1 - density_comp / 2.65));
                        double bulk_density = gravels_vol * 2.65 + (1 - gravels_vol) * density_comp; // g/cm3                      
                        tensionB.Add(SoilType[pnetecoregion], (float)((Math.Log(1500) - Math.Log(33.0)) / (Math.Log(moist33_comp) - Math.Log(predMoist1500adj))));
                        tensionA.Add(SoilType[pnetecoregion], (float)Math.Exp(Math.Log(33.0) + (tensionB[SoilType[pnetecoregion]] * Math.Log(moist33_comp))));
                        // For frozen soil
                        clayFrac.Add(SoilType[pnetecoregion], (float)clay);
                        double cTheta_temp = Constants.HeatCapacitySoil * (1.0 - soilPorosity_OM_comp[SoilType[pnetecoregion]]) + Constants.HeatCapacityWater * soilPorosity_OM_comp[SoilType[pnetecoregion]];  //specific heat of soil	kJ/m3/K
                        cTheta.Add(SoilType[pnetecoregion], (float)cTheta_temp);
                        double ThermalConductivitySoil_temp = (1.0 - clay) * Constants.ThermalConductivitySandstone + clay * Constants.ThermalConductivityClay;   //thermal conductivity soil	kJ/m/d/K
                        ThermalConductivitySoil.Add(SoilType[pnetecoregion], (float)ThermalConductivitySoil_temp);
                        double Fs_temp = (2.0 / 3.0 / (1.0 + Constants.gs * ((ThermalConductivitySoil_temp / Constants.ThermalConductivityWater_kJperday) - 1.0))) + (1.0 / 3.0 / (1.0 + (1.0 - 2.0 * Constants.gs) * ((ThermalConductivitySoil_temp / Constants.ThermalConductivityWater_kJperday) - 1.0)));  //ratio of solid temp gradient
                        Fs.Add(SoilType[pnetecoregion], (float)Fs_temp);
                    }
                    double soilWaterContent = 0.0;
                    float pressureHead = float.MaxValue;
                    while (pressureHead > 0.01)
                    {
                        pressureHead = CalcSoilWaterPressureHead(soilWaterContent, SoilType[pnetecoregion]);
                        PressureHead.Add(pressureHead);
                        soilWaterContent += 0.01;
                    }
                    table[pnetecoregion] = PressureHead.ToArray();
                }
            }
        }
    }
}

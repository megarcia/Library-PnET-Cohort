using System;

namespace Landis.Library.PnETCohorts
{
    public class Hydrology : IHydrology
    {
        // References:
        //     Cabrera et al. 2016: Performance of evaporation estimation methods compared with standard 20 m2 tank https://doi.org/10.1590/1807-1929/agriambi.v20n10p874-879
        //     Reis, M. G. dos, and A. Ribeiro, 2020: Conversion factors and general equations applied in agricultural and forest meteorology. Agrometeoros, 27(2). https://doi.org/10.31062/agrom.v27i2.26527
        //     Robock, A., K.Y. Vinnikov, C.A. Schlosser, N.A. Speranskaya, and Y. Xue, 1995: Use of midlatitude soil moisture and meteorological observations to validate soil moisture simulations with biosphere and bucket models. Journal of Climate, 8(1), 15-35.

        private float soilWaterContent;
        private float frozenSoilWaterContent;
        private float frozenSoilDepth;
        public static Hydrology_SaxtonRawls pressureHeadTable;

        /// <summary>
        /// soil volumetric water content (mm/m)
        /// </summary>
        public float SoilWaterContent
        {
            get
            {
                return soilWaterContent;
            }
        }

        /// <summary>
        /// frozen soil volumetric water content (mm/m)
        /// </summary>
        public float FrozenSoilWaterContent
        {
            get
            {
                return frozenSoilWaterContent;
            }
        }

        /// <summary>
        /// Depth at which soil is frozen (mm)
        /// Rooting zone soil below this depth is frozen
        /// </summary>
        public float FrozenSoilDepth
        {
            get
            {
                return frozenSoilDepth;
            }
        }

        public Hydrology_SaxtonRawls PressureHeadTable
        {
            get
            {
                return pressureHeadTable;
            }
        }

        /// <summary>
        /// Actual Evaporation (mm)
        /// </summary>
        public float Evaporation;

        /// <summary>
        /// Leakage to groundwater (mm)
        /// </summary>
        public float Leakage;

        /// <summary>
        /// Runoff (mm)
        /// </summary>
        public float Runoff;

        /// <summary>
        /// Potential Evaporation (mm)
        /// </summary>
        public float PotentialEvaporation;

        /// <summary>
        /// Potential Evapotranspiration (mm)
        /// </summary>
        public float PotentialET;

        /// <summary>
        /// Volume of water captured on the surface
        /// </summary>
        public float SurfaceWater = 0;

        /// <summary>
        /// ???
        /// </summary>
        public static float DeliveryPotential;

        /// <summary>
        /// thread lock utility
        /// </summary>
        public static readonly object threadLock = new object();

        /// <summary>
        /// Get the pressure head (mmH2O) for the current soil water content
        /// (converted from fraction to percent)
        /// </summary>
        /// <param name="ecoregion"></param>
        /// <returns></returns>
        public float GetPressureHead(IPnETEcoregionData ecoregion)
        {
            return pressureHeadTable[ecoregion, (int)Math.Round(soilWaterContent * 100.0)];
        }

        /// <summary>
        /// Get the pressure head (mmH2O) for a specified soil water content value
        /// (converted from fraction to percent)
        /// </summary>
        /// <param name="ecoregion"></param>
        /// <param name="_soilWaterContent"></param>
        /// <returns></returns>
        public float GetPressureHead(IPnETEcoregionData ecoregion, float _soilWaterContent)
        {
            return pressureHeadTable[ecoregion, (int)Math.Round(_soilWaterContent * 100.0)];
        }

        /// <summary>
        /// Add mm water to volumetric soil water content (mm/m) (considering activeSoilDepth - frozen soil cannot accept water)
        /// </summary>
        /// <param name="addWater"></param>
        /// <param name="activeSoilDepth"></param>
        /// <returns></returns>
        public bool AddWater(float addWater, float activeSoilDepth)
        {
            float adjSoilWaterContent = 0;
            if (activeSoilDepth > 0)
                adjSoilWaterContent = addWater / activeSoilDepth;
            soilWaterContent += adjSoilWaterContent;
            // Note 20250721 MG: always returns true because of this value reset
            if (soilWaterContent < 0)
                soilWaterContent = 0;
            // end Note
            if (soilWaterContent >= 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Add mm water to volumetric soil water content (mm/m) (considering activeSoilDepth - frozen soil cannot accept water)
        /// </summary>
        /// <param name="currentWater"></param>
        /// <param name="addWater"></param>
        /// <param name="activeSoilDepth"></param>
        /// <returns></returns>
        public float AddWater(float currentWater, float addWater, float activeSoilDepth)
        {
            float adjSoilWaterContent = 0;
            if (activeSoilDepth > 0)
                adjSoilWaterContent = addWater / activeSoilDepth;
            currentWater += adjSoilWaterContent;
            if (currentWater < 0)
                currentWater = 0;
            return currentWater;
        }

        public Hydrology(float soilWaterContent)
        {
            // mm of water per m of active soil (volumetric content)
            this.soilWaterContent = soilWaterContent;
        }

        /// <summary>
        /// volumetric water content (mm/m) of the frozen soil
        /// </summary>
        /// <param name="soilWaterContent"></param>
        /// <returns></returns>
        public bool SetFrozenSoilWaterContent(float soilWaterContent)
        {
            this.frozenSoilWaterContent = soilWaterContent;
            if (soilWaterContent >= 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Depth at which soil is frozen (mm); Rooting zone soil below this depth is frozen
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public bool SetFrozenSoilDepth(float depth)
        {
            this.frozenSoilDepth = depth;
            if (depth >= 0)
                return true;
            else
                return false;
        }

        public static void Initialize()
        {
            Parameter<string> PressureHeadCalculationMethod = null;
            if (Names.TryGetParameter(Names.PressureHeadCalculationMethod, out PressureHeadCalculationMethod))
            {
                Parameter<string> p = Names.GetParameter(Names.PressureHeadCalculationMethod);
                pressureHeadTable = new Hydrology_SaxtonRawls();
            }
            else
            {
                string msg = "Missing method for calculating pressurehead, expected keyword " + Names.PressureHeadCalculationMethod + " in " + Names.GetParameter(Names.PnETGenericParameters).Value + " or in " + Names.GetParameter(Names.ExtensionName).Value;
                throw new Exception(msg);
            }
            Globals.ModelCore.UI.WriteLine("Eco\tSoiltype\tWiltingPoint\t\tFieldCapacity\tFC-WP\t\tPorosity");
            foreach (IPnETEcoregionData ecoregion in PnETEcoregionData.Ecoregions) if (ecoregion.Active)
                {
                    // Volumetric soil water content (mm/m) at field capacity
                    ecoregion.FieldCapacity = (float)pressureHeadTable.CalcSoilWaterContent(-Constants.FieldCapacity_kPa, ecoregion.SoilType);
                    // Volumetric soil water content (mm/m) at wilting point
                    ecoregion.WiltingPoint = (float)pressureHeadTable.CalcSoilWaterContent(-Constants.WiltingPoint_kPa, ecoregion.SoilType);
                    // Volumetric soil water content (mm/m) at porosity
                    ecoregion.Porosity = (float)pressureHeadTable.SoilPorosity(ecoregion.SoilType);
                    float f = ecoregion.FieldCapacity - ecoregion.WiltingPoint;
                    Globals.ModelCore.UI.WriteLine(ecoregion.Name + "\t" + ecoregion.SoilType + "\t\t" + ecoregion.WiltingPoint + "\t" + ecoregion.FieldCapacity + "\t" + f + "\t" + ecoregion.Porosity);
                }
        }

        /// <summary>
        /// Calculate surface evaporation
        /// </summary>
        /// <param name="ecoregion"></param>
        /// <param name="potentialET"></param>
        /// <returns></returns>
        public float CalcEvaporation(IPnETEcoregionData ecoregion, float potentialET)
        {
            lock (threadLock)
            {
                // frozen soils
                float frostFreeSoilDepth = ecoregion.RootingDepth - FrozenSoilDepth;
                float frostFreeFrac = Math.Min(1.0F, frostFreeSoilDepth / ecoregion.RootingDepth);
                // Evaporation is limited to frost free soil above EvapDepth
                float evapSoilDepth = Math.Min(ecoregion.RootingDepth * frostFreeFrac, ecoregion.EvapDepth);
                // Maximum actual evaporation = Potential ET
                float AEmax = potentialET; // Modified 11/4/22 in v 5.0-rc19; remove access limitation and only use physical limit at wilting point below
                // Evaporation cannot remove water below wilting point           
                float evaporationEvent = Math.Min(AEmax, (soilWaterContent - ecoregion.WiltingPoint) * evapSoilDepth); // mm/month
                evaporationEvent = Math.Max(0f, evaporationEvent);  // evaporation cannot be negative
                return evaporationEvent; // mm/month
            }
        }

        /// <summary>
        /// Evaporation from soil water content at soil surface.
        /// TODO: evaporate from available captured surface water first, 
        /// then take water from soil column.   
        /// </summary>
        /// <param name="hydrology"></param>
        /// <param name="ecoregion"></param>
        /// <param name="snowpack"></param>
        /// <param name="fracRootAboveFrost"></param>
        /// <param name="PET"></param>
        /// <param name="location"></param>
        /// <exception cref="Exception"></exception>
        public void CalcSoilEvaporation(Hydrology hydrology, IPnETEcoregionData ecoregion, float snowpack, float fracRootAboveFrost, float potentialET, string location)
        {
            float EvaporationEvent = 0;
            if (fracRootAboveFrost > 0 && snowpack == 0)
                EvaporationEvent = hydrology.CalcEvaporation(ecoregion, potentialET); // mm
            bool success = hydrology.AddWater(-1 * EvaporationEvent, ecoregion.RootingDepth * fracRootAboveFrost);
            if (!success)
                throw new Exception("Error adding water, evaporation = " + EvaporationEvent + "; soilWaterContent = " + hydrology.SoilWaterContent + "; ecoregion = " + ecoregion.Name + "; site = " + location);
            hydrology.Evaporation += EvaporationEvent;
        }

        /// <summary>
        /// Calculate runoff from input in excess of surface capture 
        /// and/or available soil capacity. Includes infiltration.
        /// </summary>
        /// <param name="hydrology"></param>
        /// <param name="ecoregion"></param>
        /// <param name="inputWater"></param>
        /// <param name="fracRootAboveFrost"></param>
        /// <param name="location"></param>
        /// <exception cref="Exception"></exception>
        public void CalcRunoff(Hydrology hydrology, IPnETEcoregionData ecoregion, float inputWater, float fracRootAboveFrost, string location)
        {
            if (ecoregion.RunoffCapture > 0)
            {
                float capturedInput = Math.Min(inputWater, Math.Max(ecoregion.RunoffCapture - hydrology.SurfaceWater, 0));
                hydrology.SurfaceWater += capturedInput;
                inputWater -= capturedInput;
            }
            float availableSoilCapacity = Math.Max(ecoregion.Porosity - hydrology.SoilWaterContent, 0) * ecoregion.RootingDepth * fracRootAboveFrost; // mm
            float runoff = Math.Max(inputWater - availableSoilCapacity, 0);
            bool success = hydrology.AddWater(inputWater - runoff, ecoregion.RootingDepth * fracRootAboveFrost);
            if (!success)
                throw new Exception("Error adding water, InputWater = " + inputWater + "; soilWaterContent = " + hydrology.SoilWaterContent + "; Runoff = " + runoff + "; ecoregion = " + ecoregion.Name + "; site = " + location);
            hydrology.Runoff += runoff;
        }

        /// <summary>
        /// Calculate soil infiltration purely from surface captured water.
        /// </summary>
        /// <param name="hydrology"></param>
        /// <param name="ecoregion"></param>
        /// <param name="fracRootAboveFrost"></param>
        /// <param name="location"></param>
        /// <exception cref="Exception"></exception>
        public void CalcInfiltration(Hydrology hydrology, IPnETEcoregionData ecoregion, float fracRootAboveFrost, string location)
        {
            float SurfaceInput = Math.Min(hydrology.SurfaceWater, (ecoregion.Porosity - hydrology.SoilWaterContent) * ecoregion.RootingDepth * fracRootAboveFrost);
            bool success = hydrology.AddWater(SurfaceInput, ecoregion.RootingDepth * fracRootAboveFrost);
            if (!success)
                throw new Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; soilWaterContent = " + hydrology.SoilWaterContent + "; ecoregion = " + ecoregion.Name + "; site = " + location);
            hydrology.SurfaceWater -= SurfaceInput;
        }

        /// <summary>
        /// Calculate leakage of soil water to "groundwater."
        /// </summary>
        /// <param name="hydrology"></param>
        /// <param name="ecoregion"></param>
        /// <param name="leakageFrac"></param>
        /// <param name="fracRootAboveFrost"></param>
        /// <param name="location"></param>
        /// <exception cref="Exception"></exception>
        public void CalcLeakage(Hydrology hydrology, IPnETEcoregionData ecoregion, float leakageFrac, float fracRootAboveFrost, string location)
        {
            float leakage = Math.Max(leakageFrac * (hydrology.SoilWaterContent - ecoregion.FieldCapacity), 0) * ecoregion.RootingDepth * fracRootAboveFrost; //mm
            hydrology.Leakage += leakage;
            bool success = hydrology.AddWater(-1 * leakage, ecoregion.RootingDepth * fracRootAboveFrost);
            if (!success)
                throw new Exception("Error adding water, Hydrology.Leakage = " + hydrology.Leakage + "; soilWaterContent = " + hydrology.SoilWaterContent + "; ecoregion = " + ecoregion.Name + "; site = " + location);
        }

        /// <summary>
        /// Account for transpiration
        /// </summary>
        /// <param name="hydrology"></param>
        /// <param name="ecoregion"></param>
        /// <param name="transpiration"></param>
        /// <param name="fracRootAboveFrost"></param>
        /// <param name="location"></param>
        /// <exception cref="Exception"></exception>
        public void SubtractTranspiration(Hydrology hydrology, IPnETEcoregionData ecoregion, float transpiration, float fracRootAboveFrost, string location)
        {
            bool success = hydrology.AddWater(-1 * transpiration, ecoregion.RootingDepth * fracRootAboveFrost);
            if (!success)
                throw new Exception("Error adding water, Transpiration = " + transpiration + " soilWaterContent = " + hydrology.SoilWaterContent + "; ecoregion = " + ecoregion.Name + "; site = " + location);
        }
    }
}

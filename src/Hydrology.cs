using System;

namespace Landis.Library.PnETCohorts
{
    public class Hydrology : IHydrology
    {
        private float water;
        private float frozenWaterContent;
        private float frozenDepth;

        // volumetric water (mm/m)
        public float Water
        {
            get
            {
                return water;
            }
        }

        // volumetric water content (mm/m) of the frozen soil
        public float FrozenWaterContent
        {
            get
            {
                return frozenWaterContent;
            }
        }

        // Depth at which soil is frozen (mm); Rooting zone soil below this depth is frozen
        public float FrozenDepth
        {
            get
            {
                return frozenDepth;
            }
        }

        public static Hydrology_SaxtonRawls pressureheadtable;

        public Hydrology_SaxtonRawls PressureHeadTable
        {
            get
            {
                return pressureheadtable;
            }
        }

        // Get the pressurehead (mmH2O) for the current water content (converted from proportion to percent)
        public float GetPressureHead(IEcoregionPnET ecoregion)
        {
            return pressureheadtable[ecoregion, (int)Math.Round(water * 100.0)];
        }

        // Get the pressurehead (mmH2O) for a provided water content (converted from proportion to percent)
        public float GetPressureHead(IEcoregionPnET ecoregion, float temp_water)
        {
            return pressureheadtable[ecoregion, (int)Math.Round(temp_water * 100.0)];
        }

        public float Evaporation;
        public float Leakage;
        public float RunOff;
        public float PE;  // Potential Evaporation (mm)
        public float PET;  // Potential Evapotranspiration (mm)
        public static float DeliveryPotential;
        public static readonly object threadLock = new object();
        public float SurfaceWater = 0; // Volume of water captured above saturatino on the surface

        // Add mm water to volumetric water content (mm/m) (considering activeSoilDepth - frozen soil cannot accept water)
        public bool AddWater(float addwater, float activeSoilDepth)
        {
            float adjWater = 0;
            if (activeSoilDepth > 0)
                adjWater = addwater / activeSoilDepth;
            water += adjWater;
            if (water < 0)
                water = 0;
            if (water >= 0)
                return true;
            else
                return false;
        }

        // Add mm water to volumetric water content (mm/m) (considering activeSoilDepth - frozen soil cannot accept water)
        public float AddWater(float currentWater, float addwater, float activeSoilDepth)
        {
            float adjWater = 0;
            if (activeSoilDepth > 0)
                adjWater = addwater / activeSoilDepth;
            currentWater += adjWater;
            if (currentWater < 0)
                currentWater = 0;
            return currentWater;            
        }

        // mm of water per m of active soil (volumetric content)
        public Hydrology(float water)
        {
            this.water = water;
        }

        // volumetric water content (mm/m) of the frozen soil
        public bool SetFrozenWaterContent (float water)
        {
            this.frozenWaterContent = water;
            if (water >= 0)
                return true;
            else
                return false;
        }

        // Depth at which soil is frozen (mm); Rooting zone soil below this depth is frozen
        public bool SetFrozenDepth(float depth)
        {
            this.frozenDepth = depth;
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
                pressureheadtable = new Hydrology_SaxtonRawls();
            }
            else
            {
                string msg = "Missing method for calculating pressurehead, expected keyword " + Names.PressureHeadCalculationMethod + " in " + Names.GetParameter(Names.PnETGenericParameters).Value + " or in " + Names.GetParameter(Names.ExtensionName).Value; 
                throw new System.Exception(msg);
            }
            Globals.ModelCore.UI.WriteLine("Eco\tSoiltype\tWiltPnt\t\tFieldCap\tFC-WP\t\tPorosity");
            foreach (IEcoregionPnET eco in EcoregionData.Ecoregions) if (eco.Active)
            {
                // Volumetric water content (mm/m) at field capacity
                //  −33 kPa (or −0.33 bar)        
                // Convert kPA to mH2o (/9.804139432) = 3.37
                eco.FieldCap = (float)pressureheadtable.CalculateWaterContent(33, eco.SoilType);
                // Volumetric water content (mm/m) at wilting point
                //  −1500 kPa (or −15 bar)  
                // Convert kPA to mH2o (/9.804139432) = 153.00
                eco.WiltPnt = (float)pressureheadtable.CalculateWaterContent(1500, eco.SoilType);
                // Volumetric water content (mm/m) at porosity
                eco.Porosity = (float)pressureheadtable.Porosity(eco.SoilType);
                float f = eco.FieldCap - eco.WiltPnt;
                Globals.ModelCore.UI.WriteLine(eco.Name + "\t" + eco.SoilType + "\t\t" + eco.WiltPnt + "\t" + eco.FieldCap + "\t" + f + "\t" + eco.Porosity );
            }
        }

        public float CalculateEvaporation(SiteCohorts sitecohorts, float PET)
        {
            lock (threadLock)
            {
                // permafrost
                float frostFreeSoilDepth = sitecohorts.Ecoregion.RootingDepth - FrozenDepth;
                float frostFreeProp = Math.Min(1.0F, frostFreeSoilDepth / sitecohorts.Ecoregion.RootingDepth);
                // Evaporation is limited to frost free soil above EvapDepth
                float evapSoilDepth = Math.Min(sitecohorts.Ecoregion.RootingDepth * frostFreeProp, sitecohorts.Ecoregion.EvapDepth);
                float AEmax = PET; // Modified 11/4/22 in v 5.0-rc19; remove access limitation and only use physical limit at wilting point below
                // Evaporation cannot remove water below wilting point           
                float evaporationEvent = Math.Min(AEmax, (Water - sitecohorts.Ecoregion.WiltPnt) * evapSoilDepth);// mm/month
                evaporationEvent = Math.Max(0f, evaporationEvent);  // evap cannot be negative
                return evaporationEvent; //mm/month
            }
        }
    }
}

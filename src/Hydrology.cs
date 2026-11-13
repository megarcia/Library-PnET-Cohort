using System;

namespace Landis.Library.PnETCohorts
{
    public class Hydrology : IHydrology
    {
        private float water;
        private float frozenWaterContent;
        private float frozenDepth;
        public static Hydrology_SaxtonRawls pressureheadtable;
        public float Evaporation;
        public float Leakage;
        public float Runoff;
        public float PE;  // Potential Evaporation (mm)
        public float PET;  // Potential Evapotranspiration (mm)
        public static float DeliveryPotential;
        public float SurfaceWater = 0; // Volume of water captured above saturation on the surface
        public static readonly object threadLock = new object();

        // volumetric liquid water content (mm/m)
        public float Water
        {
            get
            {
                return water;
            }
        }

        // volumetric frozen water content (mm/m)
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
            return pressureheadtable[ecoregion, (int)Math.Round(water * 100.0f)];
        }

        // Get the pressurehead (mmH2O) for a provided water content (converted from proportion to percent)
        public float GetPressureHead(IEcoregionPnET ecoregion, float water)
        {
            return pressureheadtable[ecoregion, (int)Math.Round(water * 100.0f)];
        }

        // Add mm water to volumetric water content (mm/m)
        // considering activeSoilDepth - frozen soil cannot accept water
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

        // Add mm water to volumetric water content (mm/m) 
        // considering activeSoilDepth - frozen soil cannot accept water
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

        public Hydrology(float water)
        {
            this.water = water;
        }

        // volumetric water content (mm/m) of the frozen soil
        public bool SetFrozenWaterContent (float water)
        {
            frozenWaterContent = water;
            if (water >= 0)
                return true;
            else
                return false;
        }

        // Depth at which soil is frozen (mm); Rooting zone soil below this depth is frozen
        public bool SetFrozenDepth(float depth)
        {
            frozenDepth = depth;
            if (depth >= 0)
                return true;
            else
                return false;
        }

        //
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
                throw new Exception(msg);
            }
            Globals.ModelCore.UI.WriteLine("Eco\tSoiltype\tWiltPnt\t\tFieldCap\tFC-WP\t\tPorosity");
            foreach (IEcoregionPnET eco in EcoregionData.Ecoregions) if (eco.Active)
            {
                // Volumetric water content (mm/m) at field capacity
                //  −33 kPa (or −0.33 bar)        
                eco.FieldCap = (float)pressureheadtable.CalculateWaterContent(33, eco.SoilType);

                // Volumetric water content (mm/m) at wilting point
                //  −1500 kPa (or −15 bar)  
                eco.WiltPnt = (float)pressureheadtable.CalculateWaterContent(1500, eco.SoilType);

                // Volumetric water content (mm/m) at porosity
                eco.Porosity = (float)pressureheadtable.Porosity(eco.SoilType);

                float f = eco.FieldCap - eco.WiltPnt;
                Globals.ModelCore.UI.WriteLine(eco.Name + "\t" + eco.SoilType + "\t\t" + eco.WiltPnt + "\t" + eco.FieldCap + "\t" + f + "\t" + eco.Porosity );
            }
        }

        // Calculations based on Stewart & Rouse 1976 and Cabrera et al. 2016
        //double _Rads                  // Daytime Solar Radiation (PAR) (micromol/m2/s)
        //double _Tair                  // Daytime air temperature (°C) [Tday]
        //float _daySpan                // Number of days in the month
        //float _daylength              // Length of daylight in seconds
        static float Calculate_PotentialEvaporation_umol(double _Rads, double _Tair, float _daySpan, float _daylength)
        {
            float PE = 0.0f; //mm/month
            float Rs_Watts = (float)(_Rads / 2.02f); // convert PAR (umol/m2*s) to total solar radiation (W/m2) [Reis and Ribeiro 2019 (Consants and Values)]  
            float Rs = Rs_Watts * Constants.SecondsPerDay / Constants.Million; // convert Rs_W (W/m2) to Rs (MJ/m2*d) [Reis and Ribeiro 2019 (eq. 13)]
            float es = Weather.CalcVaporPressure((float)_Tair); // water vapor saturation pressure (kPa); [Cabrera et al. 2016 (Table 1)]
            float S = Weather.CalcVaporPressureCurveSlope((float)_Tair); // slope of curve of water pressure and air temp; [Cabrera et al. 2016 (Table 1)]
            float PEMJ = S / (S + Constants.PsychrometricCoeff) * (1.624f + 0.9265f * Rs); // MJ/m2 day; Stewart & Rouse 1976 (eq. 11)
            PE = PEMJ * 0.408f; // convert MJ/m2 day to mm/day http://www.fao.org/3/x0490e/x0490e0i.htm
            return PE * _daySpan;  //mm/month 
        }

        public float CalculateEvaporation(SiteCohorts sitecohorts, float PET)
        {
            lock (threadLock)
            {
                // Frozen soils
                float frostFreeSoilDepth = sitecohorts.Ecoregion.RootingDepth - FrozenDepth;
                float frostFreeProp = Math.Min(1.0f, frostFreeSoilDepth / sitecohorts.Ecoregion.RootingDepth);
                // Evaporation is limited to frost free soil above EvapDepth
                float evapSoilDepth = Math.Min(sitecohorts.Ecoregion.RootingDepth * frostFreeProp, sitecohorts.Ecoregion.EvapDepth);
                float AEmax = PET; // Modified 11/4/22 in v 5.0-rc19; remove access limitation and only use physical limit at wilting point below
                // Evaporation cannot remove water below wilting point           
                float evaporationEvent = Math.Min(AEmax, (Water - sitecohorts.Ecoregion.WiltPnt) * evapSoilDepth);// mm/month
                evaporationEvent = Math.Max(0.0f, evaporationEvent);  // evap cannot be negative
                return evaporationEvent; //mm/month
            }
        }

        // Priestley-Taylor
        // aboveCanopyPAR   daytime PAR (umol/m2/s) at top of canopy
        // subCanopyPAR     daytime PAR (umol/m2/s) at bottom of canopy
        // daylength        daytime length in seconds (s)
        // T                average monthly temperature (C)
        // daySpan          number of days in the month
        public float Calculate_PotentialGroundET_Radiation_umol(float aboveCanopyPAR,float subCanopyPAR, float daylength, float T, float daySpan)            
        {
            float Rs_daily = (float)(aboveCanopyPAR / (Constants.SecondsPerDay / daylength)); // convert daytime PAR (umol/m2*s) to total daily PAR (umol/m2*s)
            float Rs_W = (float)(Rs_daily / 2.02f); // convert daily PAR (umol/m2*s) to total solar radiation (W/m2) [Reis and Ribeiro 2019 (Consants and Values)]  
            // Back-calculate LAI from aboveCanopyPAR and subCanopyPAR
            float k = 0.3038f;
            float LAI = (float)Math.Log(subCanopyPAR / aboveCanopyPAR) / (-1.0f * k);
            float aboveCanopyNetRad = 0f;
            if (LAI < 2.4)
                aboveCanopyNetRad = -26.8818f + 0.693066f * Rs_W;
            else
                aboveCanopyNetRad = -33.2467f + 0.741644f * Rs_W;
            float subCanopyNetRad = aboveCanopyNetRad * (float)Math.Exp(-1.0f * k * LAI);
            float alpha = 1.0f;
            float es = Weather.CalcVaporPressure(T); // water vapor saturation pressure (kPa); [Cabrera et al. 2016 (Table 1)]
            float S = Weather.CalcVaporPressureCurveSlope(T); // slope of curve of water pressure and air temp; [Cabrera et al. 2016 (Table 1)]
            float PET_ground = alpha * (S / (S + Constants.PsychrometricCoeff)) / Constants.LatentHeatVaporWater * subCanopyNetRad * 0.0864F; //m/day  (0.0864 conversion W/m2 to MJ/m2*d)
            return PET_ground * 1000 * daySpan; //mm/month
        }

        // T            average monthly temperature (C)
        // daylength    daytime length in seconds (s)
        public float Calculate_RET_Hamon(float T, float dayLength)
        {
            if (T < 0)
                return 0.0f;
            else
            {
                float k = 1.2f;   // proportionality coefficient
                float es = Weather.CalcVaporPressure(T);
                float N = dayLength / (float)Constants.SecondsPerHour / 12f;
                float PET = k * 0.165f * 216.7f * N * (es / (T + 273.3f));
                return PET; // mm/day
            }
        }

        // LAI          Total Canopy LAI
        // T            average monthly temperature (C)
        // daylength    daytime length in seconds (s)
        // daySpan      number of days in the month
        public float Calculate_PotentialGroundET_LAI_WATER(float LAI, float T, float dayLength, float daySpan)
        {
            float RET = Calculate_RET_Hamon(T, dayLength); //mm/day
            float Egp = 0.8f * RET * (float)Math.Exp(-0.695f * LAI); //mm/day
            return Egp * daySpan; //mm/month
        }

        // LAI          Total Canopy LAI
        // T            average monthly temperature (C)
        // daylength    daytime length in seconds (s)
        // daySpan      number of days in the month
        public float Calculate_PotentialGroundET_LAI_WEPP(float LAI, float T, float dayLength, float daySpan)
        {
            float RET = Calculate_RET_Hamon(T, dayLength); //mm/day
            float Egp = RET * (float)Math.Exp(-0.4f * LAI); //mm/day
            return Egp * daySpan; //mm/month
        }

        // LAI          Total Canopy LAI
        // T            average monthly temperature (C)
        // daylength    daytime length in seconds (s)
        // daySpan      number of days in the month
        // k            extinction coefficient
        // cropCoeff    crop coefficient (scalar adjustment)
        public float Calculate_PotentialGroundET_LAI(float LAI, float T, float dayLength, float daySpan, float k, float cropCoeff = 1f)
        {
            cropCoeff = ((Parameter<float>)Names.GetParameter("RETCropCoeff")).Value;
            float RET = Calculate_RET_Hamon(T, dayLength); //mm/day
            float Egp = cropCoeff  * RET * (float)Math.Exp(-k * LAI); //mm/day
            return Egp * daySpan; //mm/month
        }
    }
}
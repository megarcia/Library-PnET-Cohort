using System;

namespace Landis.Library.PnETCohorts
{
    public class Hydrology : IHydrology
    {
        private float water;
        private float frozenWaterContent;
        private float frozenDepth;

        /// <summary>
        /// volumetric water (mm/m)
        /// </summary>
        public float Water
        {
            get
            {
                return water;
            }
        }

        /// <summary>
        /// volumetric water content (mm/m) of the frozen soil
        /// </summary>
        public float FrozenWaterContent
        {
            get
            {
                return frozenWaterContent;
            }
        }

        /// <summary>
        /// Depth at which soil is frozen (mm); Rooting zone soil below this depth is frozen
        /// </summary>
        public float FrozenDepth
        {
            get
            {
                return frozenDepth;
            }
        }

        public static PressureHeadSaxton_Rawls pressureheadtable;

        public PressureHeadSaxton_Rawls PressureHeadTable
        {
            get
            {
                return pressureheadtable;
            }
        }

        /// <summary>
        /// Get the pressurehead (mmH2O) for the current water content (converted from fraction to percent)
        /// </summary>
        /// <param name="ecoregion"></param>
        /// <returns></returns>
        public float GetPressureHead(IEcoregionPnET ecoregion)
        {
            return pressureheadtable[ecoregion, (int)Math.Round(water * 100.0)];
        }

        /// <summary>
        /// Get the pressurehead (mmH2O) for a provided water content (converted from fraction to percent)
        /// </summary>
        /// <param name="ecoregion"></param>
        /// <param name="temp_water"></param>
        /// <returns></returns>
        public float GetPressureHead(IEcoregionPnET ecoregion, float temp_water)
        {
            return pressureheadtable[ecoregion, (int)Math.Round(temp_water * 100.0)];
        }

        public float Evaporation;
        public float Leakage;
        public float RunOff;

        /// <summary>
        /// Potential Evaporation (mm)
        /// </summary>
        public float PE;

        /// <summary>
        /// Potential Evapotranspiration (mm)
        /// </summary>
        public float PET;

        public static float DeliveryPotential;
        public static readonly object threadLock = new object();
        public float SurfaceWater = 0; // Volume of water captured above saturatino on the surface

        /// <summary>
        /// Add mm water to volumetric water content (mm/m) (considering activeSoilDepth - frozen soil cannot accept water)
        /// </summary>
        /// <param name="addwater"></param>
        /// <param name="activeSoilDepth"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Add mm water to volumetric water content (mm/m) (considering activeSoilDepth - frozen soil cannot accept water)
        /// </summary>
        /// <param name="currentWater"></param>
        /// <param name="addwater"></param>
        /// <param name="activeSoilDepth"></param>
        /// <returns></returns>
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
            // mm of water per m of active soil (volumetric content)
            this.water = water;
        }

        /// <summary>
        /// volumetric water content (mm/m) of the frozen soil
        /// </summary>
        /// <param name="water"></param>
        /// <returns></returns>
        public bool SetFrozenWaterContent(float water)
        {
            this.frozenWaterContent = water;
            if (water >= 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Depth at which soil is frozen (mm); Rooting zone soil below this depth is frozen
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
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
                pressureheadtable = new PressureHeadSaxton_Rawls();
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
                    eco.FieldCap = (float)pressureheadtable.CalcWaterContent(33, eco.SoilType);
                    // Volumetric water content (mm/m) at wilting point
                    //  −1500 kPa (or −15 bar)  
                    // Convert kPA to mH2o (/9.804139432) = 153.00
                    eco.WiltPnt = (float)pressureheadtable.CalcWaterContent(1500, eco.SoilType);
                    // Volumetric water content (mm/m) at porosity
                    eco.Porosity = (float)pressureheadtable.Porosity(eco.SoilType);
                    float f = eco.FieldCap - eco.WiltPnt;
                    Globals.ModelCore.UI.WriteLine(eco.Name + "\t" + eco.SoilType + "\t\t" + eco.WiltPnt + "\t" + eco.FieldCap + "\t" + f + "\t" + eco.Porosity);
                }
        }

        /// <summary>
        /// PE calculations based on Stewart & Rouse 1976 and Cabrera et al. 2016
        /// </summary>
        /// <param name="_Rads"></param>
        /// <param name="_Tair"></param>
        /// <param name="_daySpan"></param>
        /// <param name="_dayLength"></param>
        /// <returns></returns>
        static float CalcPotentialEvaporation_umol(double _Rads, double _Tair, float _daySpan, float _dayLength)
        {
            //double _Rads                  // Daytime Solar Radiation (PAR) (micromol/m2/s)
            //double _Tair                  // Daytime air temperature (°C) [Tday]
            //float _daySpan                // Number of days in the month
            //float _dayLength              // Length of daylight in seconds
            float Rs_W = (float)(_Rads / 2.02f); // convert PAR (umol/m2*s) to total solar radiation (W/m2) [Reis and Ribeiro 2019 (Consants and Values)]  
            float Rs = Rs_W * 0.0864F; // convert Rs_W (W/m2) to Rs (MJ/m2*d) [Reis and Ribeiro 2019 (eq. 13)]
            float Gamma = 0.062F; // kPa/C; [Cabrera et al. 2016 (Table 1)]
            float es = 0.6108F * (float)Math.Pow(10, 7.5 * _Tair / (237.3 + _Tair)); // water vapor saturation pressure (kPa); [Cabrera et al. 2016 (Table 1)]
            float S = 4098F * es / (float)Math.Pow(_Tair + 237.3, 2); // slope of curve of water pressure and air temp; [Cabrera et al. 2016 (Table 1)]
            float PEMJ = S / (S + Gamma) * (1.624F + 0.9265F * Rs); // MJ/m2 day; Stewart & Rouse 1976 (eq. 11)
            float PE = PEMJ * 0.408F; // convert MJ/m2 day to mm/day http://www.fao.org/3/x0490e/x0490e0i.htm

            return PE * _daySpan;  //mm/month 
        }

        public float CalcEvaporation(SiteCohorts sitecohorts, float PET)
        {
            lock (threadLock)
            {
                // permafrost
                float frostFreeSoilDepth = sitecohorts.Ecoregion.RootingDepth - FrozenDepth;
                float frostFreeFrac = Math.Min(1.0F, frostFreeSoilDepth / sitecohorts.Ecoregion.RootingDepth);
                // Evaporation is limited to frost free soil above EvapDepth
                float evapSoilDepth = Math.Min(sitecohorts.Ecoregion.RootingDepth * frostFreeFrac, sitecohorts.Ecoregion.EvapDepth);
                // Evaporation begins to decline at 75% of field capacity (Robock et al. 1995)
                // Robock, A., Vinnikov, K. Y., Schlosser, C. A., Speranskaya, N. A., & Xue, Y. (1995). Use of midlatitude soil moisture and meteorological observations to validate soil moisture simulations with biosphere and bucket models. Journal of Climate, 8(1), 15-35.
                float evapCritWater = sitecohorts.Ecoregion.FieldCap * 0.75f;
                float evapCritWaterPH = pressureheadtable[sitecohorts.Ecoregion, (int)Math.Round(evapCritWater * 100.0)];
                // Delivery potential is 1 if pressurehead < evapCritWater, and declines to 0 at wilting point (153 mH2O)
                DeliveryPotential = Cohort.CalcFWater(-1, -1, evapCritWaterPH, 153, pressurehead);
                float AEmax = PET; // Modified 11/4/22 in v 5.0-rc19; remove access limitation and only use physical limit at wilting point below
                // Evaporation cannot remove water below wilting point           
                float evaporationEvent = Math.Min(AEmax, (Water - sitecohorts.Ecoregion.WiltPnt) * evapSoilDepth);// mm/month
                evaporationEvent = Math.Max(0f, evaporationEvent);  // evap cannot be negative

                return evaporationEvent; //mm/month
            }
        }

        /// <summary>
        /// Priestley-Taylor
        /// </summary>
        /// <param name="aboveCanopyPAR"></param>
        /// <param name="subCanopyPAR"></param>
        /// <param name="dayLength"></param>
        /// <param name="T"></param>
        /// <param name="daySpan"></param>
        /// <returns></returns>
        public float CalcPotentialGroundET_Radiation_umol(float aboveCanopyPAR, float subCanopyPAR, float dayLength, float T, float daySpan)
        {
            // aboveCanopyPAR   daytime PAR (umol/m2/s) at top of canopy
            // subCanopyPAR     daytime PAR (umol/m2/s) at bottom of canopy
            // dayLength        daytime length in seconds (s)
            // T                average monthly temperature (C)
            // daySpan          number of days in the month
            float Rs_daily = (float)(aboveCanopyPAR / (24 * Constants.SecondsPerHour / dayLength)); // convert daytime PAR (umol/m2*s) to total daily PAR (umol/m2*s)
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
            float gamma = 0.066f;    // kPA/C
            float L = 2453f;    // MJ/m3 - latent heat of vaporization
            float es = 0.6108F * (float)Math.Pow(10, 7.5 * T / (237.3 + T)); // water vapor saturation pressure (kPa); [Cabrera et al. 2016 (Table 1)]
            float S = 4098F * es / (float)Math.Pow(T + 237.3, 2); // slope of curve of water pressure and air temp; [Cabrera et al. 2016 (Table 1)]
            float PET_ground = alpha * (S / (S + gamma)) / L * subCanopyNetRad * 0.0864F; //m/day  (0.0864 conversion W/m2 to MJ/m2*d)

            return PET_ground * 1000 * daySpan; //mm/month
        }

        public float CalcRET_Hamon(float T, float dayLength)
        {
            // T            average monthly temperature (C)
            // dayLength    daytime length in seconds (s)
            if (T < 0)
                return 0f;
            else
            {
                float k = 1.2f;   // proportionality coefficient
                float es = 6.108f * (float)Math.Exp(17.27f * T / (T + 237.3f));
                float N = dayLength / (float)Constants.SecondsPerHour / 12f;
                float PET = k * 0.165f * 216.7f * N * (es / (T + 273.3f));
                return PET; // mm/day
            }
        }

        public float CalcPotentialGroundET_LAI_WATER(float LAI, float T, float dayLength, float daySpan)
        {
            // LAI          Total Canopy LAI
            // T            average monthly temperature (C)
            // dayLength    daytime length in seconds (s)
            // daySpan          number of days in the month
            float RET = CalcRET_Hamon(T, dayLength); //mm/day
            float Egp = 0.8f * RET * (float)Math.Exp(-0.695f * LAI); //mm/day

            return Egp * daySpan; //mm/month
        }

        public float CalcPotentialGroundET_LAI_WEPP(float LAI, float T, float dayLength, float daySpan)
        {
            // LAI          Total Canopy LAI
            // T            average monthly temperature (C)
            // dayLength    daytime length in seconds (s)
            // daySpan          number of days in the month
            float RET = CalcRET_Hamon(T, dayLength); //mm/day
            float Egp = RET * (float)Math.Exp(-0.4f * LAI); //mm/day

            return Egp * daySpan; //mm/month
        }

        public float CalcPotentialGroundET_LAI(float LAI, float T, float dayLength, float daySpan, float k, float cropCoeff = 1f)
        {
            // LAI          Total Canopy LAI
            // T            average monthly temperature (C)
            // dayLength    daytime length in seconds (s)
            // daySpan      number of days in the month
            // k            extinction coefficient
            // cropCoeff    crop coefficient (scalar adjustment)
            cropCoeff = ((Parameter<float>)Names.GetParameter("RETCropCoeff")).Value;
            float RET = CalcRET_Hamon(T, dayLength); //mm/day
            float Egp = cropCoeff * RET * (float)Math.Exp(-k * LAI); //mm/day

            return Egp * daySpan; //mm/month
        }
    }
}

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
        public float GetPressureHead(IEcoregionPnET ecoregion)
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
        public float GetPressureHead(IEcoregionPnET ecoregion, float _soilWaterContent)
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
            foreach (IEcoregionPnET ecoregion in EcoregionData.Ecoregions) if (ecoregion.Active)
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
        /// Calculates vapor pressure at temperature (T) via the Tetens
        /// equations for water and ice   
        ///     see https://en.wikipedia.org/wiki/Tetens_equation
        /// </summary>
        /// <param name="T">Air temperature (°C)</param>
        private static float CalcVaporPressure(float T)
        {
            float es;
            if (T >= 0f)
                // above freezing point -- vapor pressure over water
                es = 0.61078f * (float)Math.Exp(17.26939f * T / (T + 237.3f));
            else
                // below freezing point -- vapor pressure over ice
                es = 0.61078f * (float)Math.Exp(21.87456f * T / (T + 265.5f));
            return es;
        }

        /// <summary>
        /// Slope of curve of water pressure and air temp
        ///     Cabrera et al. 2016 (Table 1)
        /// </summary>
        /// <param name="T">Temperature (C)</param>
        /// <returns></returns>
        private static float CalcVaporPressureCurveSlope(float T)
        {
            float slope = 4098F * CalcVaporPressure(T) / (float)Math.Pow(T + 237.3, 2);
            return slope;
        }

        /// <summary>
        /// PE calculations based on Stewart & Rouse 1976 and Cabrera et al. 2016
        /// </summary>
        /// <param name="par">Daytime solar radiation (PAR) (micromol/m2.s)</param>
        /// <param name="tair">Daytime air temperature (°C) [Tday]</param>
        /// <param name="daySpan">Days in the month</param>
        /// <param name="dayLength">Length of daylight (s)</param>
        /// <returns></returns>
        static float CalcPotentialEvaporation_umol(double par, double tair, float daySpan, float dayLength)
        {
            // convert PAR (umol/m2.s) to total solar radiation (W/m2) (Reis and Ribeiro, 2019, eq. 39)  
            // convert Rs_W (W/m2) to Rs (MJ/m2.d) (Reis and Ribeiro, 2019, eq. 13)
            float Rs = (float)par / 2.02F * Constants.SecondsPerDay / 1000000F;
            // get slope of vapor pressure curve at Tair
            float VPSlope = CalcVaporPressureCurveSlope((float)tair);
            // calculate potential evaporation (Stewart & Rouse, 1976, eq. 11)
            float PotentialEvaporation_MJ = VPSlope / (VPSlope + Constants.PsychrometricCoeff) * (1.624F + 0.9265F * Rs); // MJ/m2.day 
            // convert MJ/m2.day to mm/day (http://www.fao.org/3/x0490e/x0490e0i.htm)
            float PotentialEvaporation = PotentialEvaporation_MJ * 0.408F;
            return PotentialEvaporation * daySpan;  // mm/month 
        }

        public float CalcEvaporation(SiteCohorts sitecohorts, float PotentialET)
        {
            lock (threadLock)
            {
                // frozen soils
                float frostFreeSoilDepth = sitecohorts.Ecoregion.RootingDepth - FrozenSoilDepth;
                float frostFreeFrac = Math.Min(1.0F, frostFreeSoilDepth / sitecohorts.Ecoregion.RootingDepth);
                // Evaporation is limited to frost free soil above EvapDepth
                float evapSoilDepth = Math.Min(sitecohorts.Ecoregion.RootingDepth * frostFreeFrac, sitecohorts.Ecoregion.EvapDepth);
                // Maximum actual evaporation = Potential ET
                float AEmax = PotentialET; // Modified 11/4/22 in v 5.0-rc19; remove access limitation and only use physical limit at wilting point below
                // Evaporation cannot remove water below wilting point           
                float evaporationEvent = Math.Min(AEmax, (soilWaterContent - sitecohorts.Ecoregion.WiltingPoint) * evapSoilDepth); // mm/month
                evaporationEvent = Math.Max(0f, evaporationEvent);  // evaporation cannot be negative
                return evaporationEvent; // mm/month
            }
        }

        /// <summary>
        /// Priestley-Taylor
        /// </summary>
        /// <param name="aboveCanopyPAR">Daytime PAR (umol/m2.s) at top of canopy</param>
        /// <param name="subCanopyPAR">Daytime PAR (umol/m2.s) at bottom of canopy</param>
        /// <param name="dayLength">Daytime length (s)</param>
        /// <param name="T">Average monthly temperature (C)</param>
        /// <param name="daySpan">Days in the month</param>
        /// <returns></returns>
        public float CalcPotentialGroundET_Radiation_umol(float aboveCanopyPAR, float subCanopyPAR, float dayLength, float T, float daySpan)
        {
            // convert daytime PAR (umol/m2*s) to total daily PAR (umol/m2*s)
            float Rs_daily = (float)(aboveCanopyPAR / Constants.SecondsPerDay / dayLength); 
            // convert daily PAR (umol/m2*s) to total solar radiation (W/m2)
            //     Reis and Ribeiro 2019 (Consants and Values)  
            float Rs_W = (float)(Rs_daily / 2.02f); 
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
            float VPSlope = CalcVaporPressureCurveSlope((float)T);
            // conversion W/m2 to MJ/m2.d
            float PotentialET_ground = alpha * (VPSlope / (VPSlope + Constants.PsychrometricCoeff)) / Constants.LatentHeatVaporWater * subCanopyNetRad * Constants.SecondsPerDay / 1000000F; // m/day
            return PotentialET_ground * 1000 * daySpan; //mm/month
        }

        /// <summary>
        /// Reference evapotranspiration by Hamon method
        /// </summary>
        /// <param name="T">Average monthly temperature (C)</param>
        /// <param name="dayLength">Daytime length (s)</param>
        /// <returns></returns>
        public float CalcReferenceET_Hamon(float T, float dayLength)
        {
            if (T < 0)
                return 0f;
            float k = 1.2f;   // proportionality coefficient
            float es = CalcVaporPressure(T);
            float N = dayLength / Constants.SecondsPerHour / 12f;
            float ReferenceET = k * 0.165f * 216.7f * N * (10f * es / (T + 273.3f)); // TODO: verify the 10x factor
            return ReferenceET; // mm/day
        }

        /// <summary>
        /// Potential ET given LAI via WATER (???)
        /// </summary>
        /// <param name="LAI">Total canopy LAI</param>
        /// <param name="T">Average monthly temperature (C)</param>
        /// <param name="dayLength">Daytime length (s)</param>
        /// <param name="daySpan">Days in the month</param>
        /// <returns></returns>
        public float CalcPotentialGroundET_LAI_WATER(float LAI, float T, float dayLength, float daySpan)
        {
            float ReferenceET = CalcReferenceET_Hamon(T, dayLength); // mm/day
            float Egp = 0.8f * ReferenceET * (float)Math.Exp(-0.695f * LAI); // mm/day
            return Egp * daySpan; //mm/month
        }

        /// <summary>
        /// Potential ET given LAI via WEPP (???)
        /// </summary>
        /// <param name="LAI">Total canopy LAI</param>
        /// <param name="T">Average monthly temperature (C)</param>
        /// <param name="dayLength">Daytime length (s)</param>
        /// <param name="daySpan">Days in the month</param>
        /// <returns></returns>
        public float CalcPotentialGroundET_LAI_WEPP(float LAI, float T, float dayLength, float daySpan)
        {
            float ReferenceET = CalcReferenceET_Hamon(T, dayLength); // mm/day
            float Egp = ReferenceET * (float)Math.Exp(-0.4f * LAI); // mm/day
            return Egp * daySpan; // mm/month
        }

        /// <summary>
        /// Potential ET given LAI and a crop coefficient
        /// </summary>
        /// <param name="LAI">Total canopy LAI</param>
        /// <param name="T">Average monthly temperature (C)</param>
        /// <param name="dayLength">Daytime length (s)</param>
        /// <param name="daySpan">Days in the month</param>
        /// <param name="k">LAI extinction coefficient</param>
        /// <param name="cropCoeff">Crop coefficient (scalar adjustment)</param>
        /// <returns></returns>
        public float CalcPotentialGroundET_LAI(float LAI, float T, float dayLength, float daySpan, float k)
        {
            float cropCoeff = ((Parameter<float>)Names.GetParameter("ReferenceETCropCoeff")).Value;
            float ReferenceET = CalcReferenceET_Hamon(T, dayLength); // mm/day
            float Egp = cropCoeff * ReferenceET * (float)Math.Exp(-k * LAI); // mm/day
            return Egp * daySpan; // mm/month
        }
    }
}

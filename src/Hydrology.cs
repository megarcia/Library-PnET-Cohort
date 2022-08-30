using System;

namespace Landis.Library.PnETCohorts
{
    public class Hydrology : IHydrology
    {
        private float water;
        private float frozenWaterContent;
        private float frozenDepth;
        //---------------------------------------------------------------------
        // volumetric water (mm/m)
        public float Water
        {
            get
            {
                return water;
            }
        }

        //---------------------------------------------------------------------
        // volumetric water content (mm/m) of the frozen soil
        public float FrozenWaterContent
        {
            get
            {
                return frozenWaterContent;
            }
        }

        //---------------------------------------------------------------------
        // Depth at which soil is frozen (mm); Rooting zone soil below this depth is frozen
        public float FrozenDepth
        {
            get
            {
                return frozenDepth;
            }
        }

        //---------------------------------------------------------------------
        private static PressureHeadSaxton_Rawls pressureheadtable;
        //---------------------------------------------------------------------
        // Get the pressurehead (mmH2O) for the current water content (converted from proportion to percent)
        public float GetPressureHead(IEcoregionPnET ecoregion)
        {
            return pressureheadtable[ecoregion, (int)Math.Round(water * 100.0)];
        }

        public float Evaporation;
        public float Leakage;
        public float RunOff;
        public float PE;  // Potential Evaporation
        public static float DeliveryPotential;
        public static readonly object threadLock = new object();
        public float SurfaceWater = 0; // Volume of water captured above saturatino on the surface

        /*// Add mm water
        public bool AddWater(float addwater)
        {
            water += addwater;

            if (water >= 0) return true;
            else return false;
        }*/
        //---------------------------------------------------------------------
        // Add mm water to volumetric water content (mm/m) (considering activeSoilDepth - frozen soil cannot accept water)
        public bool AddWater(float addwater, float activeSoilDepth)
        {
            float adjWater = 0;
            if (activeSoilDepth > 0)
            {
                adjWater = addwater / activeSoilDepth;
            }
            water += adjWater;
            if (water < 0)
                water = 0;

            if (water >= 0)
                return true;
            else
            {
                return false;
            }
        }
        //---------------------------------------------------------------------
        // mm of water per m of active soil (volumetric content)
        public Hydrology(float water)
        {
            this.water = water;
        }

        //---------------------------------------------------------------------
        // volumetric water content (mm/m) of the frozen soil
        public bool SetFrozenWaterContent (float water)
        {
            this.frozenWaterContent = water;
            if (water >= 0) return true;
            else return false;
        }
        //---------------------------------------------------------------------
        // Depth at which soil is frozen (mm); Rooting zone soil below this depth is frozen
        public bool SetFrozenDepth(float depth)
        {
            this.frozenDepth = depth;
            if (depth >= 0) return true;
            else return false;
        }
        //---------------------------------------------------------------------
        //
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
        //---------------------------------------------------------------------
        // Old function - no longer used - unable to verify equations
        static double Calculate_PotentialEvapotranspiration(double _Rads, double _Tair, float _dayLength,float _daySpan, double Altitude = 0)
        {
            //================================================================================
            //----  Computes the potential evapotranspiration as the value under minimum
            //----  advection according to Priestley and Taylor (1972) as discussed in
            //----  Brutsaert (1982, p. 217).
            //
            //----  Pierluigi Calanca, 23.06.2006 (PROGRASS)
            //================================================================================
            //double _Rads                  // Solar Radiation (micromol(PAR)/m2/s)
            //double _Tair                  // Daytime air temperature (°C) [Tday]
            //float _daylength              // Length of daytime in seconds
            //float _daySpan                // Number of days in the month
			 
            double Lv = 2.5e6;				 // Specific heat of vaporisation (J/kg)
	        double Cpd = 1004;				 // Joules/°C/kg (Specific heat at constant pressure)
	        double eps = 0.622;				 // Mol Mass Water (18)/Mol mass air (28.9)
	        double alphaPT = 1.35;			 // Priestley Taylor constant (parameter)

            int sec_per_day = (int) Math.Round(_dayLength);
            const int JoulesPerMJ = 1000000;
                        
	        // Atmospheric pressure (unit of vapour pressure kPa, depends on altitude)
	        //http://www.fao.org/docrep/x0490e/x0490e07.htm#TopOfPage
	        double press = 101.3 * Math.Pow(((293 -0.0065 * Altitude)/293),5.26);
           
	        // Psychrometric constant [kPa °C-1]
            double gamE  = Cpd*press/(eps*Lv); 

	        // Angle of the curve [-]
            double delta = (6.112 * Math.Exp(17.67 * _Tair / (_Tair + 243.5))) * 17.67 * 243.5 / Math.Pow((_Tair + 243.5), 2);

            // RADs coming in as micromol(PAR)/m2/s
            double Rad_day = _Rads * sec_per_day;   // umol/m2/day
            double Radn = Math.Max(-15 + 0.6 * Rad_day / JoulesPerMJ, 0); // (MJ/m2/day)
            double RadnMJM2 = Radn * sec_per_day / JoulesPerMJ;  // Radn should have unit MJ/m2
            //double RadnMJM2 = _Rads * sec_per_day / 2.0513; //(MJ/m2/day) http://www.pnet.sr.unh.edu/subpages/radconvert.html

            double PET = 0;
	        if (RadnMJM2 > 0)PET = (alphaPT/Lv) * delta / (delta + gamE) * RadnMJM2 * JoulesPerMJ; //BRM - unable to verify this equation and proper units
	        else PET= 0.0;

            return PET * _daySpan;  //mm/month
        }
        //---------------------------------------------------------------------
        static float Calculate_PotentialEvaporation_umol(double _Rads, double _Tair, float _daySpan, float _daylength)
        {
            //double _Rads                  // Daytime Solar Radiation (PAR) (micromol/m2/s)
            //double _Tair                  // Daytime air temperature (°C) [Tday]
            //float _daySpan                // Number of days in the month
            //float _daylength              // Length of daylight in seconds

            // Caculations based on Stewart & Rouse 1976 and Cabrera et al. 2016
            float PE = 0; //mm/month

            //float Rs_W = (float)(_Rads / (2.02 * 24 * Constants.SecondsPerHour / _daylength)); // convert daytime PAR (umol/m2*s) to total daily solar radiation (W/m2) [Reis and Ribeiro 2019 (Consants and Values)]  
            float Rs_W = (float)(_Rads / (2.02f)); // convert PAR (umol/m2*s) to total solar radiation (W/m2) [Reis and Ribeiro 2019 (Consants and Values)]  
            float Rs = Rs_W * 0.0864F; // convert Rs_W (W/m2) to Rs (MJ/m2*d) [Reis and Ribeiro 2019 (eq. 13)]
            float Gamma = 0.062F; // kPa/C; [Cabrera et al. 2016 (Table 1)]
            float es = 0.6108F * (float)Math.Pow(10, (7.5 * _Tair) / (237.3 + _Tair)); // water vapor saturation pressure (kPa); [Cabrera et al. 2016 (Table 1)]
            float S = (4098F * es) / (float)(Math.Pow((_Tair + 237.3), 2)); // slope of curve of water pressure and air temp; [Cabrera et al. 2016 (Table 1)]
            //float PETmm = (S / (S + Gamma)) * (0.4755F + 0.3773F * Rs); // Stewart & Rouse 1976 (mm/d); [Cabrera et al. 2016 (Table 1)]
            float PEMJ = (S / (S + Gamma)) * (1.624F + 0.9265F * Rs); // MJ/m2 day; Stewart & Rouse 1976 (eq. 11)
            PE = PEMJ * 0.408F; // convert MJ/m2 day to mm/day http://www.fao.org/3/x0490e/x0490e0i.htm

            return PE * _daySpan;  //mm/month 
        }
        //---------------------------------------------------------------------
        public float CalculateEvaporation(SiteCohorts sitecohorts, IEcoregionPnETVariables variables)
        {
            lock (threadLock)
            {
                // permafrost
                float frostFreeSoilDepth = sitecohorts.Ecoregion.RootingDepth - FrozenDepth;
                float frostFreeProp = Math.Min(1.0F, frostFreeSoilDepth / sitecohorts.Ecoregion.RootingDepth);
                // Evaporation is limited to frost free soil above EvapDepth
                float evapSoilDepth = Math.Min(sitecohorts.Ecoregion.RootingDepth * frostFreeProp, sitecohorts.Ecoregion.EvapDepth);


                float umolPAR = sitecohorts.SubcanopyPAR;
                if (((Parameter<string>)Names.GetParameter(Names.PARunits)).Value == "W/m2")
                    umolPAR = (sitecohorts.SubcanopyPAR * 2.02f); // convert daytime solar radiation (W/m2) to daytime PAR (umol/m2*s) [Reis and Ribeiro 2019 (Consants and Values)]  

                // mm/month
                if (((Parameter<string>)Names.GetParameter(Names.ETMethod)).Value == "Original")
                {
                    PE = (float)Calculate_PotentialEvaporation_umol(umolPAR, variables.Tday, variables.DaySpan, variables.Daylength) * evapSoilDepth / sitecohorts.Ecoregion.RootingDepth;
                }
                else if (((Parameter<string>)Names.GetParameter(Names.ETMethod)).Value == "Radiation")
                {
                    PE = Calculate_PotentialGroundET_Radiation_umol(umolPAR, variables.Daylength, variables.Tday, variables.DaySpan) * evapSoilDepth / sitecohorts.Ecoregion.RootingDepth;
                }
                else if (((Parameter<string>)Names.GetParameter(Names.ETMethod)).Value == "WATER")
                {
                    PE = Calculate_PotentialGroundET_LAI_WATER(sitecohorts.CanopyLAImax, variables.Tave, variables.Daylength, variables.DaySpan) * evapSoilDepth / sitecohorts.Ecoregion.RootingDepth;
                }
                else if (((Parameter<string>)Names.GetParameter(Names.ETMethod)).Value == "WEPP")
                {
                    PE = Calculate_PotentialGroundET_LAI_WEPP(sitecohorts.CanopyLAImax, variables.Tave,  variables.Daylength, variables.DaySpan) * evapSoilDepth / sitecohorts.Ecoregion.RootingDepth;
                }
                    SiteVars.AnnualPE[sitecohorts.Site] += PE;
                float pressurehead = pressureheadtable[sitecohorts.Ecoregion, (int)Math.Round(Water * 100)];

                // Evaporation begins to decline at 75% of field capacity (Robock et al. 1995)
                // Robock, A., Vinnikov, K. Y., Schlosser, C. A., Speranskaya, N. A., & Xue, Y. (1995). Use of midlatitude soil moisture and meteorological observations to validate soil moisture simulations with biosphere and bucket models. Journal of Climate, 8(1), 15-35.
                float evapCritWater = sitecohorts.Ecoregion.FieldCap * 0.75f;
                float evapCritWaterPH = pressureheadtable[sitecohorts.Ecoregion, (int)Math.Round(evapCritWater * 100.0)];

                // Delivery potential is 1 if pressurehead < evapCritWater, and declines to 0 at wilting point (153 mH2O)
                DeliveryPotential = Cohort.ComputeFWater(-1, -1, evapCritWaterPH, 153, pressurehead);
          
                float AEmax = DeliveryPotential * PE;  // Actual Evaporation max mm/month
               
                // Evaporation  cannot be negative
                // Transpiration is assumed to replace evaporation
                //Evaporation = (float)Math.Max(0, AET - (double)sitecohorts.Transpiration);

                // Change in assumption: evaporation and transpiration are additive because evap already accounts for shading
                // Evaporation cannot remove water below wilting point           
                Evaporation = Math.Min(AEmax, (Water - sitecohorts.Ecoregion.WiltPnt) * evapSoilDepth);// mm/month

                float AET = Evaporation + sitecohorts.Transpiration;
                sitecohorts.SetAet(AET, variables.Month);                
                float PET = PE + sitecohorts.PotentialTranspiration;
                sitecohorts.SetPET(PET);
                SiteVars.ClimaticWaterDeficit[sitecohorts.Site] += (PET - AET);
                return Evaporation; //mm/month
            }
        }
        //---------------------------------------------------------------------
        public float Calculate_PotentialGroundET_Radiation_umol(float subCanopyPAR, float daylength, float T, float daySpan)            
        {
            // Priestley-Taylor
            // subCanopyPAR     daytime PAR (umol/m2/s) at ground level
            // daylength        daytime length in seconds (s)
            // T                average monthly temperature (C)
            // daySpan          number of days in the month

//            float Rs_W = (float)(subCanopyPAR / (2.02 * 24 * Constants.SecondsPerHour / daylength)); // convert daytime PAR (umol/m2*s) to total daily solar radiation (W/m2) [Reis and Ribeiro 2019 (Consants and Values)]  
            float Rs_W = (float)(subCanopyPAR / (2.02f )); // convert PAR (umol/m2*s) to total solar radiation (W/m2) [Reis and Ribeiro 2019 (Consants and Values)]  
            float Rs = Rs_W * 0.0864F; // convert Rs_W (W/m2) to Rs (MJ/m2*d) [Reis and Ribeiro 2019 (eq. 13)]
            float alpha = 1.26f;
            float gamma = 0.066f;    // kPA/C
            float L = 2453f;    // MJ/m3 - latent heat of vaporization
            float Ta = T + 273.15f;
            float es = (float)(Math.Pow(Ta / 273.16, -4.811) * Math.Exp(24.134 - (6726.73 / Ta)));  // UNITS = kPa, (note: Ta here is in Kelvin, so Ta = TdegC + 273.15)
            float S = (es / (T + 273.15f)) * ((6726.73f / (T + 273.15f)) - 4.811f);     // UNITS Pressure = kPa, T -> degC

            float PET_ground = alpha * (S/(S+gamma))*Rs / L; //m/day
            return PET_ground * 1000 * daySpan; //mm/month
        }
        //---------------------------------------------------------------------
        public float Calculate_RET_Hamon(float T, float dayLength)
        {
            // T            average monthly temperature (C)
            // daylength    daytime length in seconds (s)

            float k = 1f;   // proportionality coefficient
            float es = 6.108f * (float)Math.Exp((17.27f * T) / (T + 237.3f));
            float N = (dayLength / (float)Constants.SecondsPerHour) / 12f;
            float PET = k * 0.165f * 216.7f * N * (es / (T + 273.3f));
            return PET; // mm/day
        }
        //---------------------------------------------------------------------
        public float Calculate_PotentialGroundET_LAI_WATER(float LAI, float T, float dayLength, float daySpan)
        {
            // LAI          Total Canopy LAI
            // T            average monthly temperature (C)
            // daylength    daytime length in seconds (s)
            // daySpan          number of days in the month

            float RET = Calculate_RET_Hamon(T, dayLength);
            float Egp = 0.8f * RET * (float)Math.Exp(-0.695f * LAI); //m/day
            return Egp * 1000 * daySpan; //mm/month
        }
        //---------------------------------------------------------------------
        public float Calculate_PotentialGroundET_LAI_WEPP(float LAI, float T, float dayLength, float daySpan)
        {
            // LAI          Total Canopy LAI
            // T            average monthly temperature (C)
            // daylength    daytime length in seconds (s)
            // daySpan          number of days in the month

            float RET = Calculate_RET_Hamon(T, dayLength);
            float Egp = RET * (float)Math.Exp(-0.4f * LAI); //m/day
            return Egp * 1000 * daySpan; //mm/month
        }
    }
}
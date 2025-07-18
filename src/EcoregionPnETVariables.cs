using System;
using System.Collections.Generic;

namespace Landis.Library.PnETCohorts
{
    public class EcoregionPnETVariables : IEcoregionPnETVariables
    {
        private DateTime _date;
        private IObservedClimate obs_clim;
        private float _vpd;
        private float _dayspan;
        private float _tavg;
        private float _tday;
        private float _daylength;

        public float VPD
        {
            get
            {
                return _vpd;
            }
        }

        public byte Month
        {
            get
            {
                return (byte)_date.Month;
            }
        }

        public float Tday
        {
            get
            {
                return _tday;
            }
        }

        public float Prec
        {
            get
            {
                return obs_clim.Prec;
            }
        }

        public float O3
        {
            get
            {
                return obs_clim.O3;
            }
        }

        public float CO2
        {
            get
            {
                return obs_clim.CO2;
            }
        }

        public float SPEI
        {
            get
            {
                return obs_clim.SPEI;
            }
        }

        public float PAR0
        {
            get
            {
                return obs_clim.PAR0;
            }
        }

        public DateTime Date
        {
            get
            {
                return _date;
            }
        }

        /// <summary>
        /// Number of days in the month
        /// </summary>
        public float DaySpan
        {
            get
            {
                return _dayspan;
            }
        }

        /// <summary>
        /// Year
        /// </summary>
        public int Year
        {
            get
            {
                return _date.Year;
            }
        }

        /// <summary>
        /// Time (decimal year)
        /// </summary>
        public float Time
        {
            get
            {
                return _date.Year + 1F / 12F * (_date.Month - 1);
            }
        }

        public float Tavg
        {
            get
            {
                return _tavg;
            }
        }

        public float Tmin
        {
            get
            {
                return obs_clim.Tmin;
            }
        }

        public float Tmax
        {
            get
            {
                return obs_clim.Tmax;
            }
        }

        public float Daylength
        {
            get
            {
                return _daylength;
            }
        }

        #region static computation functions
        public static int CalcDaySpan(int Month)
        {
            return Month switch
            {
                1 => 31,
                2 => 28,
                3 => 31,
                4 => 30,
                5 => 31,
                6 => 30,
                7 => 31,
                8 => 31,
                9 => 30,
                10 => 31,
                11 => 30,
                12 => 31,
                _ => throw new System.Exception("Month " + Month + " is not an integer between 1-12. Error assigning DaySpan"),
            };
        }

        /// <summary>
        /// Calculate average temperature (ºC)
        /// </summary>
        /// <param name="Tmin"></param>
        /// <param name="Tmax"></param>
        /// <returns></returns>
        public static float CalcTavg(float Tmin, float Tmax)
        {
            return (float)((Tmin + Tmax) / 2.0f);
        }

        /// <summary>
        /// Calculate daytime average temperature (ºC)
        /// </summary>
        /// <param name="Tavg"></param>
        /// <param name="Tmax"></param>
        /// <returns></returns>
        public static float CalcTday(float Tavg, float Tmax)
        {
            return (float)((Tavg + Tmax) / 2.0f);
        }

        private static float CalcVP(float a, float b, float c, float T)
        {
            // Calculates vapor pressure at temperature (T)
            // a,b,c are coefficients
            // Equation from PnET-II
            return a * (float)Math.Exp(b * T / (T + c));
        }

        public static float CalcVPD(float Tday, float Tmin)
        {
            float emean;
            // saturated vapor pressure
            float es = CalcVP(0.61078f, 17.26939f, 237.3f, Tday);
            if (Tday < 0)
                es = CalcVP(0.61078f, 21.87456f, 265.5f, Tday);
            emean = CalcVP(0.61078f, 17.26939f, 237.3f, Tmin);
            if (Tmin < 0)
                emean = CalcVP(0.61078f, 21.87456f, 265.5f, Tmin);
            return es - emean;
        }

        public static float CurvilinearPsnTempResponse(float tday, float PsnTopt, float PsnTmin, float PsnTmax)
        {
            if (tday < PsnTmin)
                return 0;
            else if (tday > PsnTopt)
                return 1;
            else
                return (PsnTmax - tday) * (tday - PsnTmin) / (float)Math.Pow((PsnTmax - PsnTmin) / 2, 2);
        }

        public static float DTempResponse(float tday, float PsnTopt, float PsnTmin, float PsnTmax)
        {
            if (tday < PsnTmin)
                return 0;
            else if (tday > PsnTmax)
                return 0;
            else
            {
                if (tday <= PsnTopt)
                {
                    float PsnTmaxestimate = PsnTopt + (PsnTopt - PsnTmin);
                    return (float)Math.Max(0.0, (PsnTmaxestimate - tday) * (tday - PsnTmin) / (float)Math.Pow((PsnTmaxestimate - PsnTmin) / 2, 2));
                }
                else
                {
                    float PsnTminestimate = PsnTopt + (PsnTopt - PsnTmax);
                    return (float)Math.Max(0.0, (PsnTmax - tday) * (tday - PsnTminestimate) / (float)Math.Pow((PsnTmax - PsnTminestimate) / 2, 2));
                }
            }
        }

        /// <summary>
        /// Nightlength in seconds
        /// </summary>
        /// <param name="hr"></param>
        /// <returns></returns>
        public static float CalcNightLength(float hr)
        {
            return 60 * 60 * (24 - hr);
        }

        /// <summary>
        /// Daylength in seconds
        /// </summary>
        /// <param name="hr"></param>
        /// <returns></returns>
        public static float CalcDayLength(float hr)
        {
            return 60 * 60 * hr;
        }

        /// <summary>
        /// Calculate hours of daylight
        /// </summary>
        /// <param name="DOY"></param>
        /// <param name="Latitude"></param>
        /// <returns></returns>
        public static float CalcDaylightHrs(int DOY, double Latitude)
        {
            float TA;
            float AC;
            float LatRad;
            float r;
            float z;
            float decl;
            float z2;
            float h;
            LatRad = (float)Latitude * (2.0f * (float)Math.PI) / 360.0f;
            r = 1.0f - (0.0167f * (float)Math.Cos(0.0172f * (float)(DOY - 3)));
            z = 0.39785f * (float)Math.Sin(4.868961f + 0.017203f * (float)DOY + 0.033446f * (float)Math.Sin(6.224111f + 0.017202f * (float)DOY));
            if ((float)Math.Abs(z) < 0.7f)
                decl = (float)Math.Atan(z / ((float)Math.Sqrt(1.0f - z * z)));
            else
                decl = (float)Math.PI / 2.0f - (float)Math.Atan((float)Math.Sqrt(1.0f - z * z) / z);
            if ((float)Math.Abs(LatRad) >= (float)Math.PI / 2.0)
            {
                if (Latitude < 0)
                    LatRad = (-1.0f) * ((float)Math.PI / 2.0f - 0.01f);
                else
                    LatRad = 1.0f * ((float)Math.PI / 2.0f - 0.01f);
            }
            z2 = -(float)Math.Tan(decl) * (float)Math.Tan(LatRad);
            if (z2 >= 1.0)
                h = 0;
            else if (z2 <= -1.0)
                h = (float)Math.PI;
            else
            {
                TA = (float)Math.Abs(z2);
                if (TA < 0.7)
                    AC = 1.570796f - (float)Math.Atan(TA / (float)Math.Sqrt(1.0f - TA * TA));
                else
                    AC = (float)Math.Atan((float)Math.Sqrt(1.0f - TA * TA) / TA);
                if (z2 < 0)
                    h = 3.141593f - AC;
                else
                    h = AC;
            }
            return 2.0f * (h * 24.0f) / (2.0f * (float)Math.PI);
        }
        #endregion

        private Dictionary<string, SpeciesPnETVariables> speciesVariables;

        public SpeciesPnETVariables this[string species]
        {
            get
            {
                return speciesVariables[species];
            }
        }

        public EcoregionPnETVariables(IObservedClimate climate_dataset, DateTime Date, bool Wythers, bool DTemp, List<ISpeciesPnET> Species, float Latitude)
        {
            this._date = Date;
            this.obs_clim = climate_dataset;
            speciesVariables = new Dictionary<string, SpeciesPnETVariables>();
            _tavg = CalcTavg(climate_dataset.Tmin, climate_dataset.Tmax);
            _dayspan = CalcDaySpan(Date.Month);
            float hr = CalcDaylightHrs(Date.DayOfYear, Latitude); //hours of daylight
            _daylength = CalcDayLength(hr);
            float nightlength = CalcNightLength(hr);
            _tday = CalcTday(Tavg, climate_dataset.Tmax);
            _vpd = CalcVPD(Tday, climate_dataset.Tmin);
            foreach (ISpeciesPnET spc in Species)
            {
                SpeciesPnETVariables speciespnetvars = GetSpeciesVariables(ref climate_dataset, Wythers, DTemp, Daylength, nightlength, spc);
                speciesVariables.Add(spc.Name, speciespnetvars);
            }
        }

        private SpeciesPnETVariables GetSpeciesVariables(ref IObservedClimate climate_dataset, bool Wythers, bool DTemp, float daylength, float nightlength, ISpeciesPnET spc)
        {
            // Class that contains species specific PnET variables for a certain month
            SpeciesPnETVariables speciespnetvars = new SpeciesPnETVariables();
            // Gradient of effect of vapour pressure deficit on growth. 
            speciespnetvars.DVPD = Math.Max(0, 1.0f - spc.DVPD1 * (float)Math.Pow(VPD, spc.DVPD2));
            // ** CO2 effect on growth **
            // M. Kubiske method for wue calculation:  Improved methods for calculating WUE and Transpiration in PnET.
            float JH2O = (float)(0.239 * (VPD / (8314.47 * (climate_dataset.Tmin + 273f))));
            speciespnetvars.JH2O = JH2O;
            // GROSSPsn gross photosynthesis
            // Modify AmaxB based on CO2 level
            // Equations solved from 2 known points: (350, AmaxB) and (550, AmaxB * CO2AmaxBEff)
            float AmaxB_slope = (float)((spc.CO2AMaxBEff - 1.0) * spc.AmaxB / 200.0);  // Derived from m = [(AmaxB*CO2AMaxBEff) - AmaxB]/[550 - 350]
            float AmaxB_int = (float)(-1.0 * (((spc.CO2AMaxBEff - 1.0) * 1.75) - 1.0) * spc.AmaxB);  // Derived from b = AmaxB - (AmaxB_slope * 350)
            float AmaxB_CO2 = AmaxB_slope * climate_dataset.CO2 + AmaxB_int;
            speciespnetvars.AmaxB_CO2 = AmaxB_CO2;
            // PsnFTemp (public for output file)
            if (DTemp)
                speciespnetvars.PsnFTemp = DTempResponse(Tday, spc.PsnTopt, spc.PsnTmin, spc.PsnTmax);
            else
                speciespnetvars.PsnFTemp = CurvilinearPsnTempResponse(Tday, spc.PsnTopt, spc.PsnTmin, spc.PsnTmax); // Modified 051216(BRM)
            // Respiration gC/timestep (RespTempResponses[0] = day respiration factor)
            // Respiration acclimation subroutine From: Tjoelker, M.G., Oleksyn, J., Reich, P.B. 1999.
            // Acclimation of respiration to temperature and C02 in seedlings of boreal tree species
            // in relation to plant size and relative growth rate. Global Change Biology. 49:679-691,
            // and Tjoelker, M.G., Oleksyn, J., Reich, P.B. 2001. Modeling respiration of vegetation:
            // evidence for a general temperature-dependent Q10. Global Change Biology. 7:223-230.
            // This set of algorithms resets the veg parameter "BaseFoliarRespirationFrac" from
            // the static vegetation parameter, then recalculates BaseFoliarRespiration based on the adjusted
            // BaseFoliarRespirationFrac
            // Base foliage respiration 
            float BaseFoliarRespirationFrac;
            // Base parameter in Q10 temperature dependency calculation
            float Q10base;
            if (Wythers == true)
            {
                //Calculate Base foliar respiration based on temp; this is species-level
                BaseFoliarRespirationFrac = 0.138071F - 0.0024519F * Tavg;
                //Midpoint between Tavg and Optimal Temp; this is also species-level
                float Tmidpoint = (Tavg + spc.PsnTopt) / 2F;
                // Base parameter in Q10 temperature dependency calculation in current temperature
                Q10base = 3.22F - 0.046F * Tmidpoint;
            }
            else
            {
                // The default PnET setting 
                BaseFoliarRespirationFrac = spc.BaseFoliarRespiration;
                Q10base = spc.Q10;
            }
            speciespnetvars.BaseFoliarRespirationFrac = BaseFoliarRespirationFrac;
            // Respiration Q10 factor
            speciespnetvars.FQ10 = CalcPsnFQ10(Q10base, Tavg, spc.PsnTopt);
            // Dday  maintenance respiration factor (scaling factor of actual vs potential respiration applied to daily temperature)
            float fTempRespDay = CalcPsnFQ10(Q10base, Tday, spc.PsnTopt);
            // Night maintenance respiration factor (scaling factor of actual vs potential respiration applied to night temperature)
            float fTempRespNight = CalcPsnFQ10(Q10base, Tmin, spc.PsnTopt);
            // Unitless respiration adjustment: public for output file only
            float FTempRespWeightedDayAndNight = (float)Math.Min(1.0, (fTempRespDay * daylength + fTempRespNight * nightlength) / ((float)daylength + (float)nightlength));
            speciespnetvars.FTempRespWeightedDayAndNight = FTempRespWeightedDayAndNight;
            // Scaling factor of respiration given day and night temperature and day and night length
            speciespnetvars.MaintRespFTempResp = spc.MaintResp * FTempRespWeightedDayAndNight;
            return speciespnetvars;
        }

        /// <summary>
        /// Generic computation for a Q10 reduction factor used for respiration calculations
        /// </summary>
        /// <param name="Q10"></param>
        /// <param name="Tday"></param>
        /// <param name="PsnTopt"></param>
        /// <returns></returns>
        public static float CalcPsnFQ10(float Q10, float Tday, float PsnTopt)
        {
            float fQ10 = (float)Math.Pow(Q10, (Tday - PsnTopt) / 10);
            return fQ10;
        }
    }
}

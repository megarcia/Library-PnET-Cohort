using System;
using System.Collections.Generic;
using System.Globalization;

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
        private float _dayLength;

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

        public float DayLength
        {
            get
            {
                return _dayLength;
            }
        }

        #region static computation functions

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

        #endregion

        private Dictionary<string, PnETSpeciesVariables> speciesVariables;

        public PnETSpeciesVariables this[string species]
        {
            get
            {
                return speciesVariables[species];
            }
        }

        public EcoregionPnETVariables(IObservedClimate climate_dataset, DateTime Date, bool Wythers, bool DTemp, List<IPnETSpecies> Species, float Latitude)
        {
            this._date = Date;
            this.obs_clim = climate_dataset;
            speciesVariables = new Dictionary<string, PnETSpeciesVariables>();
            _tavg = CalcTavg(climate_dataset.Tmin, climate_dataset.Tmax);
            _dayspan = Calendar.CalcDaySpan(Date.Month);
            float hr = Calendar.CalcDaylightHrs(Date.DayOfYear, Latitude); //hours of daylight
            _dayLength = Calendar.CalcDayLength(hr);
            float nightLength = Calendar.CalcNightLength(hr);
            _tday = CalcTday(Tavg, climate_dataset.Tmax);
            _vpd = CalcVPD(Tday, climate_dataset.Tmin);
            foreach (IPnETSpecies spc in Species)
            {
                PnETSpeciesVariables speciespnetvars = GetSpeciesVariables(ref climate_dataset, Wythers, DTemp, DayLength, nightLength, spc);
                speciesVariables.Add(spc.Name, speciespnetvars);
            }
        }

        private PnETSpeciesVariables GetSpeciesVariables(ref IObservedClimate climate_dataset, bool Wythers, bool DTemp, float dayLength, float nightLength, IPnETSpecies spc)
        {
            // Class that contains species specific PnET variables for a certain month
            PnETSpeciesVariables speciespnetvars = new PnETSpeciesVariables();
            // Gradient of effect of vapour pressure deficit on growth. 
            speciespnetvars.DVPD = Math.Max(0, 1.0f - spc.DVPD1 * (float)Math.Pow(VPD, spc.DVPD2));
            // ** CO2 effect on growth **
            // M. Kubiske method for wue calculation:  Improved methods for calculating WUE and Transpiration in PnET.
            float JH2O = (float)(Constants.CalperJ * (VPD / (Constants.GasConst_JperkmolK * (climate_dataset.Tmin + Constants.Tref_K))));
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
            speciespnetvars.RespirationFQ10 = CalcRespirationFQ10(Q10base, Tavg, spc.PsnTopt);
            // Dday  maintenance respiration factor (scaling factor of actual vs potential respiration applied to daily temperature)
            float fTempRespDay = CalcRespirationFQ10(Q10base, Tday, spc.PsnTopt);
            // Night maintenance respiration factor (scaling factor of actual vs potential respiration applied to night temperature)
            float fTempRespNight = CalcRespirationFQ10(Q10base, Tmin, spc.PsnTopt);
            // Unitless respiration adjustment: public for output file only
            float RespirationFTemp = (float)Math.Min(1.0, (fTempRespDay * dayLength + fTempRespNight * nightLength) / ((float)dayLength + (float)nightLength));
            speciespnetvars.RespirationFTemp = RespirationFTemp;
            // Scaling factor of respiration given day and night temperature and day and night length
            speciespnetvars.MaintenanceRespirationFTemp = spc.MaintResp * RespirationFTemp;
            return speciespnetvars;
        }

        /// <summary>
        /// Generic computation for a Q10 reduction factor used for respiration calculations
        /// </summary>
        /// <param name="Q10"></param>
        /// <param name="Tday"></param>
        /// <param name="PsnTopt"></param>
        /// <returns></returns>
        public static float CalcRespirationFQ10(float Q10, float Tday, float PsnTopt)
        {
            float RespirationFQ10 = (float)Math.Pow(Q10, (Tday - PsnTopt) / 10);
            return RespirationFQ10;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Landis.Library.PnETCohorts
{
    public class PnETEcoregionVars : IPnETEcoregionVars
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

        private Dictionary<string, PnETSpeciesVars> speciesVariables;

        public PnETSpeciesVars this[string species]
        {
            get
            {
                return speciesVariables[species];
            }
        }

        public PnETEcoregionVars(IObservedClimate climate_dataset, DateTime Date, bool Wythers, bool DTemp, List<IPnETSpecies> Species, float Latitude)
        {
            _date = Date;
            obs_clim = climate_dataset;
            speciesVariables = new Dictionary<string, PnETSpeciesVars>();
            _tavg = Weather.CalcTavg(climate_dataset.Tmin, climate_dataset.Tmax);
            _dayspan = Calendar.CalcDaySpan(Date.Month);
            float hr = Calendar.CalcDaylightHrs(Date.DayOfYear, Latitude); //hours of daylight
            _dayLength = Calendar.CalcDayLength(hr);
            float nightLength = Calendar.CalcNightLength(hr);
            _tday = Weather.CalcTday(Tavg, climate_dataset.Tmax);
            _vpd = Weather.CalcVPD(Tday, climate_dataset.Tmin);
            foreach (IPnETSpecies pnetspecies in Species)
            {
                PnETSpeciesVars pnetspeciesvars = GetSpeciesVariables(ref climate_dataset, Wythers, DTemp, DayLength, nightLength, pnetspecies);
                speciesVariables.Add(pnetspecies.Name, pnetspeciesvars);
            }
        }

        private PnETSpeciesVars GetSpeciesVariables(ref IObservedClimate climate_dataset, bool Wythers, bool DTemp, float dayLength, float nightLength, IPnETSpecies spc)
        {
            // Class that contains species specific PnET variables for a certain month
            PnETSpeciesVars pnetspeciesvars = new PnETSpeciesVars();
            pnetspeciesvars.DVPD = Photosynthesis.CalcDVPD(VPD, spc.DVPD1, spc.DVPD2);
            pnetspeciesvars.JH2O = Photosynthesis.CalcJH2O(climate_dataset.Tmin, VPD);
            pnetspeciesvars.AmaxB_CO2 = Photosynthesis.CalcAmaxB_CO2(climate_dataset.CO2, spc.AmaxB, spc.AMaxBFCO2);
            if (DTemp)
                pnetspeciesvars.PsnFTemp = Photosynthesis.DTempResponse(Tday, spc.PsnTopt, spc.PsnTmin, spc.PsnTmax);
            else
                pnetspeciesvars.PsnFTemp = Photosynthesis.CurvilinearPsnTempResponse(Tday, spc.PsnTopt, spc.PsnTmin, spc.PsnTmax); // Modified 051216(BRM)
            // Respiration gC/timestep (RespTempResponses[0] = day respiration factor)
            // Respiration acclimation subroutine From: Tjoelker, M.G., Oleksyn, J., Reich, P.B. 1999.
            // Acclimation of respiration to temperature and C02 in seedlings of boreal tree species
            // in relation to plant size and relative growth rate. Global Change Biology. 49:679-691,
            // and Tjoelker, M.G., Oleksyn, J., Reich, P.B. 2001. Modeling respiration of vegetation:
            // evidence for a general temperature-dependent Q10. Global Change Biology. 7:223-230.
            // This set of algorithms resets the veg parameter "BaseFoliarRespirationFrac" from
            // the static vegetation parameter, then recalculates BaseFoliarRespiration based on the adjusted
            // BaseFoliarRespirationFrac
            //
            // Base parameter in Q10 temperature dependency calculation
            float Q10base;
            if (Wythers)
            {
                pnetspeciesvars.BaseFoliarRespirationFrac = Respiration.CalcBaseFolRespFrac_Wythers(Tavg);
                Q10base = Respiration.CalcQ10_Wythers(Tavg, spc.PsnTopt);
            }
            else
            {
                pnetspeciesvars.BaseFoliarRespirationFrac = spc.BaseFoliarRespiration;
                Q10base = spc.Q10;
            }
            // Respiration Q10 factor
            pnetspeciesvars.RespirationFQ10 = Respiration.CalcFQ10(Q10base, Tavg, spc.PsnTopt);
            // Respiration adjustment for temperature
            float RespFTemp = Respiration.CalcFTemp(Q10base, Tday, Tmin, spc.PsnTopt, dayLength, nightLength);
            pnetspeciesvars.RespirationFTemp = RespFTemp;
            // Scaling factor of respiration given day and night temperature and day and night length
            pnetspeciesvars.MaintenanceRespirationFTemp = spc.MaintResp * RespFTemp;
            return pnetspeciesvars;
        }
    }
}

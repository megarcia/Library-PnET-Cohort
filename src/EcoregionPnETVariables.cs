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

        // Number of days in the month
        public float DaySpan
        {
            get
            {
                return _dayspan;
            }
        }

        // Year
        public int Year
        {
            get
            {
                return _date.Year;
            }
        }

        // Time (decimal year)
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
            obs_clim = climate_dataset;
            _date = Date;
            _tavg = Weather.CalcTavg(climate_dataset.Tmin, climate_dataset.Tmax);
            _dayspan = Calendar.CalcDaySpan(Date.Month);
            _daylength = Calendar.CalcDayLength(hr);
            _tday = Weather.CalcTday(_tavg, climate_dataset.Tmax);
            _vpd = Weather.CalcVPD(Tday, climate_dataset.Tmin);
            float hr = Calendar.CalcDaylightHrs(Date.DayOfYear, Latitude);
            float nightlength = Calendar.CalcNightLength(hr);
            speciesVariables = new Dictionary<string, SpeciesPnETVariables>();
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
            float JH2O = (float)(0.239 * (VPD / (Constants.GasConst_JperkmolK * (climate_dataset.Tmin + Constants.Tref_K))));
            speciespnetvars.JH2O = JH2O;
            // GROSSPSN gross photosynthesis
            // Modify AmaxB based on CO2 level
            // Equations solved from 2 known points: (350, AmaxB) and (550, AmaxB * CO2AmaxBEff)
            float AmaxB_slope = (float)((spc.CO2AMaxBEff - 1.0) * spc.AmaxB / 200.0);  // Derived from m = [(AmaxB*CO2AMaxBEff) - AmaxB]/[550 - 350]
            float AmaxB_int = (float)(-1.0 * (((spc.CO2AMaxBEff - 1.0) * 1.75) - 1.0) * spc.AmaxB);  // Derived from b = AmaxB - (AmaxB_slope * 350)
            float AmaxB_CO2 = AmaxB_slope * climate_dataset.CO2 + AmaxB_int;
            speciespnetvars.AmaxB_CO2 = AmaxB_CO2;
            if (DTemp)
                speciespnetvars.FTempPSN = Photosynthesis.DTempResponse(Tday, spc.PsnTOpt, spc.PsnTMin, spc.PsnTMax);
            else
                speciespnetvars.FTempPSN = Photosynthesis.CurvilinearPsnTempResponse(Tday, spc.PsnTOpt, spc.PsnTMin, spc.PsnTMax); // Modified 051216(BRM)
            // Respiration gC/timestep (RespTempResponses[0] = day respiration factor)
            // Respiration acclimation subroutine From: Tjoelker, M.G., Oleksyn, J., Reich, P.B. 1999.
            // Acclimation of respiration to temperature and C02 in seedlings of boreal tree species
            // in relation to plant size and relative growth rate. Global Change Biology. 49:679-691,
            // and Tjoelker, M.G., Oleksyn, J., Reich, P.B. 2001. Modeling respiration of vegetation:
            // evidence for a general temperature-dependent Q10. Global Change Biology. 7:223-230.
            // This set of algorithms resets the veg parameter "BaseFolRespFrac" from
            // the static vegetation parameter, then recalculates BaseFolResp based on the adjusted
            // BaseFolRespFrac
            // Base foliage respiration 
            float BaseFolRespFrac;
            // Base parameter in Q10 temperature dependency calculation
            float Q10base;
            if (wythers)
            {
                speciespnetvars.BaseFoliarRespirationFrac = Respiration.CalcBaseFolRespFrac_Wythers(Tavg);
                Q10base = Respiration.CalcQ10_Wythers(Tavg, spc.PsnTopt);
            }
            else
            {
                speciespnetvars.BaseFoliarRespirationFrac = spc.BaseFoliarRespiration;
                Q10base = spc.Q10;
            }
            // Respiration Q10 factor
            speciespnetvars.Q10Factor = Respiration.CalcFQ10(Q10base, Tavg, spc.PsnTopt);
            // Respiration adjustment for temperature
            float RespFTemp = Respiration.CalcFTemp(Q10base, Tday, Tmin, spc.PsnTopt, dayLength, nightLength);
            speciespnetvars.FTempRespWeightedDayAndNight = RespFTemp;
            // Scaling factor of respiration given day and night temperature and day and night length
            speciespnetvars.MaintRespFTempResp = spc.MaintResp * RespFTemp;
            return speciespnetvars;
        }
    }
}

// John McNabb: This is a copy of EcoregionPnETVariables substituting
// MonthlyClimateRecord _monthlyClimateRecord for IObservedClimate obs_clim

using System;
using System.Collections.Generic;
using System.Text;

namespace Landis.Library.PnETCohorts
{
    public class ClimateRegionPnETVariables : IEcoregionPnETVariables
    {
        private Dictionary<string, SpeciesPnETVariables> speciesVariables;
        private MonthlyClimateRecord _monthlyClimateRecord;
        private DateTime _date;
        private float _vpd;
        private float _dayspan;
        private float _tave;
        private float _tday;
        private float _daylength;

        public SpeciesPnETVariables this[string species] => speciesVariables[species];

        // public float O3 => (float)_monthlyClimateRecord.O3;
        public float O3
        {
            get
            {
                return (float)_monthlyClimateRecord.O3;
            }
        }

        // public float CO2 => (float)_monthlyClimateRecord.CO2;
        public float CO2
        {
            get
            {
                return (float)_monthlyClimateRecord.CO2;
            }
        }

        // public float PAR0 => (float)_monthlyClimateRecord.PAR0;
        public float PAR0
        {
            get
            {
                return (float)_monthlyClimateRecord.PAR0;
            }
        }

        // public float Prec => (float)_monthlyClimateRecord.Prec;
        public float Prec
        {
            get
            {
                return (float)_monthlyClimateRecord.Prec;
            }
        }

        // public float Tmax => (float)_monthlyClimateRecord.Tmax;
        public float Tmax
        {
            get
            {
                return (float)_monthlyClimateRecord.Tmax;
            }
        }

        // public float Tmin => (float)_monthlyClimateRecord.Tmin;
        public float Tmin
        {
            get
            {
                return (float)_monthlyClimateRecord.Tmin;
            }
        }

        // public float SPEI => (float)_monthlyClimateRecord.SPEI;
        public float SPEI
        {
            get
            {
                return (float)_monthlyClimateRecord.SPEI;
            }
        }

        // public DateTime Date => _date;
        public DateTime Date
        {
            get
            {
                return _date;
            }
        }

        // public int Year => _date.Year;
        public int Year
        {
            get
            {
                return _date.Year;
            }
        }

        // public byte Month => (byte)_date.Month;
        public byte Month
        {
            get
            {
                return (byte)_date.Month;
            }
        }

        // public float Time => _date.Year + 1.0f / 12.0f * (_date.Month - 1);
        public float Time
        {
            get
            {
                return _date.Year + 1.0f / 12.0f * (_date.Month - 1);
            }
        }

        // public float VPD => _vpd;
        public float VPD
        {
            get
            {
                return _vpd;
            }
        }

        // public float DaySpan => _dayspan;
        public float DaySpan
        {
            get
            {
                return _dayspan;
            }
        }

        // public float Tave => _tave;
        public float Tave
        {
            get
            {
                return _tave;
            }
        }

        // public float Tday => _tday;
        public float Tday
        {
            get
            {
                return Tday;
            }
        }

        // public float Daylength => _daylength;
        public float Daylength
        {
            get
            {
                return _daylength;
            }
        }

        public ClimateRegionPnETVariables(MonthlyClimateRecord monthlyClimateRecord, DateTime date, bool wythers, bool dTemp, List<ISpeciesPnET> Species, float latitude)
        {
            _monthlyClimateRecord = monthlyClimateRecord;
            _date = date;
            speciesVariables = new Dictionary<string, SpeciesPnETVariables>();
            _tave = Weather.CalcTavg((float)monthlyClimateRecord.Tmin, (float)monthlyClimateRecord.Tmax);
            _dayspan = Calendar.CalcDaySpan(date.Month);
            float hr = Calendar.CalcDaylightHrs(date.DayOfYear, latitude);
            _daylength = Calendar.CalcDayLength(hr);
            float nightlength = Calendar.CalcNightLength(hr);
            _tday = Weather.CalcTday(_tave, (float)monthlyClimateRecord.Tmax);
            _vpd = Weather.CalcVPD(_tday, (float)monthlyClimateRecord.Tmin);
            foreach (ISpeciesPnET spc in Species)
            {
                SpeciesPnETVariables speciespnetvars = GetSpeciesVariables(monthlyClimateRecord, wythers, dTemp, Daylength, nightlength, spc);
                speciesVariables.Add(spc.Name, speciespnetvars);
            }
        }

        private SpeciesPnETVariables GetSpeciesVariables(MonthlyClimateRecord monthlyClimateRecord, bool wythers, bool dTemp, float daylength, float nightlength, ISpeciesPnET spc)
        {
            // Class that contains species specific PnET variables for a certain month
            SpeciesPnETVariables speciespnetvars = new SpeciesPnETVariables();

            // Gradient of effect of vapor pressure deficit on growth 
            speciespnetvars.DVPD = Photosynthesis.CalcDVPD(VPD, spc.DVPD1, spc.DVPD2);
            speciespnetvars.JH2O = Photosynthesis.CalcJH2O(monthlyClimateRecord.Tmin, VPD);
            // Modify AmaxB based on CO2 level
            speciespnetvars.AmaxB_CO2 = Photosynthesis.CalcAmaxB_CO2(monthlyClimateRecord.CO2, spc.AmaxB, spc.CO2AMaxBEff);
            // FTempPsn: reduction factor due to temperature (public for output file)
            if (dTemp)
                speciespnetvars.FTempPsn = Photosynthesis.DTempResponse(Tday, spc.PsnTOpt, spc.PsnTMin, spc.PsnTMax);
            else
                speciespnetvars.FTempPsn = Photosynthesis.CurvilinearTempResponse(Tday, spc.PsnTOpt, spc.PsnTMin, spc.PsnTMax);

            // Respiration gC/timestep (RespTempResponses[0] = day respiration factor)
            // Respiration acclimation subroutine From: Tjoelker, M.G., Oleksyn, J., Reich, P.B. 1999.
            // Acclimation of respiration to temperature and C02 in seedlings of boreal tree species
            // in relation to plant size and relative growth rate. Global Change Biology. 49:679-691,
            // and Tjoelker, M.G., Oleksyn, J., Reich, P.B. 2001. Modeling respiration of vegetation:
            // evidence for a general temperature-dependent Q10. Global Change Biology. 7:223-230.
            // This set of algorithms resets the veg parameter "BaseFolRespFrac" from
            // the static vegetation parameter, then recalculates BaseFolResp based on the adjusted
            // BaseFolRespFrac

            // Base foliar respiration 
            float BaseFolRespFrac;
            // Base Q10 parameter in temperature dependency calculation
            float Q10base;
            if (wythers)
            {
                // Computed Base foliar respiration based on temp
                BaseFolRespFrac = Respiration.CalcBaseFolRespFrac_Wythers(Tavg);
                // Base parameter in Q10 temperature dependency calculation
                Q10base = Respiration.CalcQ10_Wythers(Tave, spc.PsnTOpt);
            }
            else
            {
                // The default PnET settings
                BaseFolRespFrac = spc.BFolResp;
                Q10base = spc.Q10;
            }
            speciespnetvars.BaseFolRespFrac = BaseFolRespFrac;
            speciespnetvars.Q10Factor = Respiration.CalcFQ10(Q10base, Tave, spc.PsnTOpt);
            // Unitless respiration adjustment based on temperature: public for output file only
            float FTempRespWeightedDayAndNight = Respiration.CalcWeightedFTemp(Q10base, Tday, Tmin, spc.PsnTOpt, daylength, nightlength);
            speciespnetvars.FTempRespWeightedDayAndNight = FTempRespWeightedDayAndNight;
            // Scaling factor of respiration given day and night temperature and day and night length
            speciespnetvars.MaintRespFTempResp = spc.MaintResp * FTempRespWeightedDayAndNight;
            return speciespnetvars;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Landis.Library.PnETCohorts
{
    /// <summary>John McNabb: This is a copy of EcoregionPnETVariables substituting MonthlyClimateRecord _monthlyClimateRecord for IObservedClimate obs_clim</summary>
    public class ClimateRegionPnETVariables : IEcoregionPnETVariables
    {
        #region private fields

        private MonthlyClimateRecord _monthlyClimateRecord;
        private DateTime _date;
        private float _vpd;
        private float _dayspan;
        private float _tavg;
        private float _tday;
        private float _daylength;
        private Dictionary<string, SpeciesPnETVariables> speciesVariables;

        #endregion

        #region constructor

        public ClimateRegionPnETVariables(MonthlyClimateRecord monthlyClimateRecord, DateTime date, bool wythers, bool dTemp, List<ISpeciesPnET> Species, float latitude)
        {
            _monthlyClimateRecord = monthlyClimateRecord;
            _date = date;
            _tavg = Weather.CalcTavg(monthlyClimateRecord.Tmin, monthlyClimateRecord.Tmax);
            _dayspan = Calendar.CalcDaySpan(date.Month);
            _daylength = Calendar.CalcDayLength(hr);
            _tday = Weather.CalcTday(_tavg, monthlyClimateRecord.Tmax);
            _vpd = Weather.CalcVPD(Tday, monthlyClimateRecord.Tmin);
            float hr = Calendar.CalcDaylightHrs(date.DayOfYear, latitude);
            float nightlength = Calendar.CalcNightLength(hr);
            speciesVariables = new Dictionary<string, SpeciesPnETVariables>();
            foreach (ISpeciesPnET spc in Species)
            {
                SpeciesPnETVariables speciespnetvars = GetSpeciesVariables(monthlyClimateRecord, wythers, dTemp, Daylength, nightlength, spc);
                speciesVariables.Add(spc.Name, speciespnetvars);
            }
        }

        #endregion

        #region properties

        public float VPD => _vpd;
        public byte Month => (byte)_date.Month;
        public float Tday => _tday;
        public float Prec => (float)_monthlyClimateRecord.Prec;
        public float O3 => (float)_monthlyClimateRecord.O3;
        public float CO2 => (float)_monthlyClimateRecord.CO2;
        public float PAR0 => (float)_monthlyClimateRecord.PAR0;
        public DateTime Date => _date;
        public float DaySpan => _dayspan;
        public float Time => _date.Year + 1F / 12F * (_date.Month - 1);
        public int Year => _date.Year;
        public float Tavg => _tavg;
        public float Tmin => (float)_monthlyClimateRecord.Tmin;
        public float Tmax => (float)_monthlyClimateRecord.Tmax;
        public float Daylength => _daylength;
        public float SPEI => (float)_monthlyClimateRecord.SPEI;
        public SpeciesPnETVariables this[string species] => speciesVariables[species];

        #endregion

        #region private methods

        private SpeciesPnETVariables GetSpeciesVariables(MonthlyClimateRecord monthlyClimateRecord, bool wythers, bool dTemp, float daylength, float nightlength, ISpeciesPnET spc)
        {
            // Class that contains species specific PnET variables for a certain month
            SpeciesPnETVariables speciespnetvars = new SpeciesPnETVariables();
            // Gradient of effect of vapour pressure deficit on growth. 
            speciespnetvars.DVPD = Math.Max(0, 1 - spc.DVPD1 * (float)Math.Pow(VPD, spc.DVPD2));
            // ** CO2 effect on growth **
            // M. Kubiske method for wue calculation:  Improved methods for calculating WUE and Transpiration in PnET.
            float JH2O = (float)(0.239 * (VPD / (Constants.GasConst_JperkmolK * (monthlyClimateRecord.Tmin + Constants.Tref_K))));
            speciespnetvars.JH2O = JH2O;
            // NETPSN net photosynthesis
            // Modify AmaxB based on CO2 level
            // Equations solved from 2 known points: (350, AmaxB) and (550, AmaxB * CO2AmaxBEff)
            float AmaxB_slope = (float)((spc.CO2AMaxBEff - 1.0) * spc.AmaxB / 200.0);  // Derived from m = [(AmaxB*CO2AMaxBEff) - AmaxB]/[550 - 350]
            float AmaxB_int = (float)(-1.0 * (((spc.CO2AMaxBEff - 1.0) * 1.75) - 1.0) * spc.AmaxB);  // Derived from b = AmaxB - (AmaxB_slope * 350)
            float AmaxB_CO2 = (float)(AmaxB_slope * monthlyClimateRecord.CO2 + AmaxB_int);
            speciespnetvars.AmaxB_CO2 = AmaxB_CO2;
            // FTempPSN: reduction factor due to temperature (public for output file)
            if (dTemp)
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

        #endregion
    }
}

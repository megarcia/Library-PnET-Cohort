/// <summary>
/// John McNabb: This is a copy of EcoregionPnETVariables substituting MonthlyClimateRecord _monthlyClimateRecord for IObservedClimate obs_clim
/// </summary>

using System;
using System.Collections.Generic;
using System.Text;

namespace Landis.Library.PnETCohorts
{
    public class ClimateRegionPnETVariables : IEcoregionPnETVariables
    {
        #region private fields
        private MonthlyClimateRecord _monthlyClimateRecord;
        private DateTime _date;
        private float _vpd;
        private float _dayspan;
        private float _tavg;
        private float _tday;
        private float _dayLength;
        private Dictionary<string, PnETSpeciesVariables> speciesVariables;
        #endregion

        #region constructor
        public ClimateRegionPnETVariables(MonthlyClimateRecord monthlyClimateRecord, DateTime date, bool wythers, bool dTemp, List<IPnETSpecies> Species, float latitude)
        {
            _monthlyClimateRecord = monthlyClimateRecord;
            _date = date;
            speciesVariables = new Dictionary<string, PnETSpeciesVariables>();
            _tavg = Weather.CalcTavg((float)monthlyClimateRecord.Tmin, (float)monthlyClimateRecord.Tmax);
            _dayspan = Calendar.CalcDaySpan(date.Month);
            float hr = Calendar.CalcDaylightHrs(date.DayOfYear, latitude);
            _dayLength = Calendar.CalcDayLength(hr);
            float nightLength = Calendar.CalcNightLength(hr);
            _tday = Weather.CalcTavg(_tavg, (float)monthlyClimateRecord.Tmax);
            _vpd = Weather.CalcVPD(Tday, (float)monthlyClimateRecord.Tmin);
            foreach (IPnETSpecies spc in Species)
            {
                PnETSpeciesVariables speciespnetvars = GetSpeciesVariables(monthlyClimateRecord, wythers, dTemp, dayLength, nightLength, spc);
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
        public float DayLength => _dayLength;
        public float SPEI => (float)_monthlyClimateRecord.SPEI;
        public PnETSpeciesVariables this[string species] => speciesVariables[species];
        #endregion

        #region private methods
        private PnETSpeciesVariables GetSpeciesVariables(MonthlyClimateRecord monthlyClimateRecord, bool wythers, bool dTemp, float dayLength, float nightLength, IPnETSpecies spc)
        {
            // Class that contains species specific PnET variables for a certain month
            PnETSpeciesVariables speciespnetvars = new PnETSpeciesVariables();
            speciespnetvars.DVPD = Photosynthesis.CalcDVPD(VPD, spc.DVPD1, spc.DVPD2);
            speciespnetvars.JH2O = Photosynthesis.CalcJH2O((float)monthlyClimateRecord.Tmin, VPD);
            speciespnetvars.AmaxB_CO2 = Photosynthesis.CalcAmaxB_CO2((float)monthlyClimateRecord.CO2, spc.AmaxB, spc.AMaxBFCO2);
            if (dTemp)
                speciespnetvars.PsnFTemp = Photosynthesis.DTempResponse(Tday, spc.PsnTopt, spc.PsnTmin, spc.PsnTmax);
            else
                speciespnetvars.PsnFTemp = Photosynthesis.CurvilinearPsnTempResponse(Tday, spc.PsnTopt, spc.PsnTmin, spc.PsnTmax); // Modified 051216(BRM)
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
            speciespnetvars.RespirationFQ10 = Respiration.CalcFQ10(Q10base, Tavg, spc.PsnTopt);
            // Respiration adjustment for temperature
            float RespFTemp = Respiration.CalcFTemp(Q10base, Tday, Tmin, spc.PsnTopt, dayLength, nightLength);
            speciespnetvars.RespirationFTemp = RespFTemp;
            // Scaling factor of respiration given day and night temperature and day and night length
            speciespnetvars.MaintenanceRespirationFTemp = spc.MaintResp * RespFTemp;
            return speciespnetvars;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Landis.Library.PnETCohorts
{
    public class ClimateRegionPnETVariables : IClimateRegionPnETVariables
    {
        private Dictionary<string, SpeciesPnETVariables> speciesVariables;
        private MonthlyClimateRecord monthlyClimRec;
        private DateTime date;
        private float vpd;
        private float dayspan;
        private float tave;
        private float tday;
        private float daylength;

        public SpeciesPnETVariables this[string species]
        {
            get
            {
                return speciesVariables[species];
            }
        }

        public float O3
        {
            get
            {
                return (float)monthlyClimRec.O3;
            }
        }

        public float CO2
        {
            get
            {
                return (float)monthlyClimRec.CO2;
            }
        }

        public float PAR0
        {
            get
            {
                return (float)monthlyClimRec.PAR0;
            }
        }

        public float Prec
        {
            get
            {
                return (float)monthlyClimRec.Prec;
            }
        }

        public float Tmax
        {
            get
            {
                return (float)monthlyClimRec.Tmax;
            }
        }

        public float Tmin
        {
            get
            {
                return (float)monthlyClimRec.Tmin;
            }
        }

        public float SPEI
        {
            get
            {
                return (float)monthlyClimRec.SPEI;
            }
        }

        public DateTime Date
        {
            get
            {
                return date;
            }
        }

        public int Year
        {
            get
            {
                return date.Year;
            }
        }

        public byte Month
        {
            get
            {
                return (byte)date.Month;
            }
        }

        public float Time
        {
            get
            {
                return Year + 1.0f / 12.0f * (Month - 1);
            }
        }

        public float VPD
        {
            get
            {
                return vpd;
            }
        }

        public float DaySpan
        {
            get
            {
                return dayspan;
            }
        }

        public float Tave
        {
            get
            {
                return tave;
            }
        }

        public float Tday
        {
            get
            {
                return tday;
            }
        }

        public float Daylength
        {
            get
            {
                return daylength;
            }
        }

        public ClimateRegionPnETVariables(MonthlyClimateRecord monthlyClimateRecord, DateTime modeldate, bool wythers, bool dTemp, List<ISpeciesPnET> Species, float latitude)
        {
            monthlyClimRec = monthlyClimateRecord;
            date = modeldate;
            speciesVariables = new Dictionary<string, SpeciesPnETVariables>();
            tave = Weather.CalcTavg(monthlyClimateRecord.Tmin, monthlyClimateRecord.Tmax);
            dayspan = Calendar.CalcDaySpan(date.Month);
            float hr = Calendar.CalcDaylightHrs(date.DayOfYear, latitude);
            daylength = Calendar.CalcDayLength(hr);
            float nightlength = Calendar.CalcNightLength(hr);
            tday = Weather.CalcTday(tave, monthlyClimateRecord.Tmax);
            vpd = Weather.CalcVPD(tday, monthlyClimateRecord.Tmin);
            foreach (ISpeciesPnET spc in Species)
            {
                SpeciesPnETVariables speciespnetvars = GetSpeciesVariables(monthlyClimateRecord, wythers, dTemp, daylength, nightlength, spc);
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
                BaseFolRespFrac = Respiration.CalcBaseFolRespFrac_Wythers(Tave);
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
            float fTempRespWeightedDayAndNight = Respiration.CalcWeightedFTemp(Q10base, Tday, Tmin, spc.PsnTOpt, daylength, nightlength);
            speciespnetvars.FTempRespWeightedDayAndNight = fTempRespWeightedDayAndNight;
            // Scaling factor of respiration given day and night temperature and day and night length
            speciespnetvars.MaintRespFTempResp = spc.MaintResp * fTempRespWeightedDayAndNight;
            return speciespnetvars;
        }
    }
}

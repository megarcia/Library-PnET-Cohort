using System;
using Landis.Core;

namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// John McNabb: This is a record of monthly climate data based on the climate library.
    /// </summary>
    public class MonthlyClimateRecord : IMonthlyClimateRecord
    {
        private float o3;
        private float co2;
        private float par0;
        private float prec;
        private float tmax;
        private float tmin;
        private float spei;

        public float O3
        {
            get
            {
                return o3;
            }
        }

        public float CO2
        {
            get
            {
                return co2;
            }
        }

        public float PAR0
        {
            get
            {
                return par0;
            }
        }

        public float Prec
        {
            get
            {
                return prec;
            }
        }

        public float Tmax
        {
            get
            {
                return tmax;
            }
        }

        public float Tmin
        {
            get
            {
                return tmin;
            }
        }

        public float SPEI
        {
            get
            {
                return spei;
            }
        }

        /// <summary>
        /// Notes: Climate library month is zero-based, while DateTime.Month is one-based
        /// Notes: Climate library gives precipitation in cm, but PnET expects precipitation in mm
        /// </summary>
        /// <param name="ecoregion"></param>
        /// <param name="date"></param>
        public MonthlyClimateRecord(Landis.Core.IEcoregion ecoregion, DateTime date)
        {
            int month = date.Month - 1;
            o3 = (float)ClimateRegionData.AnnualClimate[ecoregion].MonthlyOzone[month];
            co2 = (float)ClimateRegionData.AnnualClimate[ecoregion].MonthlyCO2[month];
            par0 = (float)ClimateRegionData.AnnualClimate[ecoregion].MonthlyPAR[month];
            prec = (float)ClimateRegionData.AnnualClimate[ecoregion].MonthlyPrecip[month] * 10.0f; 
            tmax = (float)ClimateRegionData.AnnualClimate[ecoregion].MonthlyMaxTemp[month];
            tmin = (float)ClimateRegionData.AnnualClimate[ecoregion].MonthlyMinTemp[month];
            spei = (float)ClimateRegionData.AnnualClimate[ecoregion].MonthlySpei[month];
        }
    }
}

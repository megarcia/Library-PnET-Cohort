using Landis.Core;
using System;

namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// John McNabb: This is a record of monthly climate data based on the climate library.
    /// </summary>
    public class MonthlyClimateRecord
    {
        public MonthlyClimateRecord(IEcoregion ecoregion, DateTime date)
        {
            var month = date.Month - 1; // climate library month is zero-based, while DateTime.Month is one-based
            O3 = ClimateRegionData.AnnualClimate[ecoregion].MonthlyOzone[month];
            CO2 = ClimateRegionData.AnnualClimate[ecoregion].MonthlyCO2[month];
            PAR0 = ClimateRegionData.AnnualClimate[ecoregion].MonthlyPAR[month];
            Prec = ClimateRegionData.AnnualClimate[ecoregion].MonthlyPrecip[month] * 10.0; // The climate library gives precipitation in cm, but PnET expects precipitation in mm, so multiply by 10.
            Tmax = ClimateRegionData.AnnualClimate[ecoregion].MonthlyMaxTemp[month];
            Tmin = ClimateRegionData.AnnualClimate[ecoregion].MonthlyMinTemp[month];
            SPEI = ClimateRegionData.AnnualClimate[ecoregion].MonthlySpei[month];
        }

        public double O3 { get; }
        public double CO2 { get; }
        public double PAR0 { get; }
        public double Prec { get; }
        public double Tmax { get; }
        public double Tmin { get; }
        public double SPEI { get; }
    }
}

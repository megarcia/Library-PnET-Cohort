using System;
using Landis.Core;

namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// John McNabb: This is a record of monthly climate data based on the climate library.
    /// </summary>
    public class MonthlyClimateRecord
    {
        // Atmospheric O3 concentration, acumulated during growing season (AOT40) (ppb h)
        float O3 { get; }
        // Atmospheric CO2 concentration (ppm)
        float CO2 { get; } 
        // Photosynthetically active radiation, average daily during the month (W/m2)
        float PAR0 { get; }
        // Precipitation (mm/mo)
        float Prec { get; }
        // Maximum daily temperature
        float Tmax { get; } 
        // Minimum daily temperature
        float Tmin { get; }
        // Standardized Precipitation-Evapotranspiration Index
        float SPEI { get; } 

        /// <summary>
        /// Notes: Climate library month is zero-based, while DateTime.Month is one-based
        /// Notes: Climate library gives precipitation in cm, but PnET expects precipitation in mm
        /// </summary>
        /// <param name="ecoregion"></param>
        /// <param name="date"></param>
        public MonthlyClimateRecord(IEcoregion ecoregion, DateTime date)
        {
            var month = date.Month - 1;
            O3 = ClimateRegionData.AnnualClimate[ecoregion].MonthlyOzone[month];
            CO2 = ClimateRegionData.AnnualClimate[ecoregion].MonthlyCO2[month];
            PAR0 = ClimateRegionData.AnnualClimate[ecoregion].MonthlyPAR[month];
            Prec = ClimateRegionData.AnnualClimate[ecoregion].MonthlyPrecip[month] * 10.0; 
            Tmax = ClimateRegionData.AnnualClimate[ecoregion].MonthlyMaxTemp[month];
            Tmin = ClimateRegionData.AnnualClimate[ecoregion].MonthlyMinTemp[month];
            SPEI = ClimateRegionData.AnnualClimate[ecoregion].MonthlySpei[month];
        }
    }
}

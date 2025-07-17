
namespace Landis.Library.PnETCohorts
{
    public interface IEcoregionPnETVariables
    {
        /// <summary>
        /// Photosynthetically active radiation, average daily during the month (umol/m2*s)
        /// </summary>
        float PAR0 { get; } 

        /// <summary>
        /// Precipitation (mm/mo)
        /// </summary>
        float Prec { get; } 

        /// <summary>
        /// Monthly average daytime air temp: (Tmax + Tave)/2
        /// </summary>
        float Tday { get; } 

        /// <summary>
        /// Vapor pressure deficit
        /// </summary>
        float VPD { get; } 

        /// <summary>
        /// Decimal year and month
        /// </summary>
        float Time { get; } 

        int Year { get; }

        /// <summary>
        /// Number of days in the month
        /// </summary>
        float DaySpan { get; }

        /// <summary>
        /// Length of daylight in seconds
        /// </summary>
        float Daylength { get; } 

        /// <summary>
        /// Numeric month
        /// </summary>
        byte Month { get; }  

        /// <summary>
        /// Monthly average air temp: (Tmin + Tmax)/2
        /// </summary>
        float Tave { get; } 

        /// <summary>
        /// Monthly min air temp
        /// </summary>
        float Tmin { get; } 

        /// <summary>
        /// Monthly max air temp
        /// </summary>
        float Tmax { get; } 

        /// <summary>
        /// Atmospheric CO2 concentration (ppm)
        /// </summary>
        float CO2 { get; } 

        /// <summary>
        /// Atmospheric O3 concentration, acumulated during growing season (AOT40) (ppb h)
        /// </summary>
        float O3 { get; } 

        /// <summary>
        /// SPEI
        /// </summary>
        float SPEI { get; }  

        SpeciesPnETVariables this[string species] { get; }
    }
}

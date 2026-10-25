
namespace Landis.Library.PnETCohorts
{
    public interface IMonthlyClimateRecord
    {
        float O3 { get; } // Atmospheric O3 concentration, acumulated during growing season (AOT40) (ppb h)
        float CO2 { get; } // Atmospheric CO2 concentration (ppm)
        float PAR0 { get; } // Photosynthetically active radiation, average daily during the month (umol/m2*s)
        float Prec { get; } // Precipitation (mm/mo)
        float Tmax { get; } // Monthly max air temp
        float Tmin { get; } // Monthly min air temp
        float SPEI { get; }  // SPEI
    }
}

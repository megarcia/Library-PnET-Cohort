using System.Collections.Generic;

namespace Landis.Library.PnETCohorts 
{
    public interface IObservedClimate: IEnumerable<ObservedClimate>  
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

        // SPEI
        float SPEI { get; } 
    }
}

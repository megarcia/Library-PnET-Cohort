using System.Collections.Generic;

namespace Landis.Library.PnETCohorts 
{
    public interface IObservedClimate: IEnumerable<ObservedClimate>  
    {
        /// <summary>
        /// Atmospheric O3 concentration, acumulated during growing season (AOT40) (ppb h)
        /// </summary>
        float O3 { get; }

        /// <summary>
        /// Atmospheric CO2 concentration (ppm)
        /// </summary>
        float CO2 { get; } 

        /// <summary>
        /// Photosynthetically active radiation, average daily during the month (W/m2)
        /// </summary>
        float PAR0 { get; }

        /// <summary>
        /// Precipitation (mm/mo)
        /// </summary>
        float Prec { get; }

        /// <summary>
        /// Maximum daily temperature
        /// </summary>
        float Tmax { get; } 

        /// <summary>
        /// Minimum daily temperature
        /// </summary>
        float Tmin { get; }

        /// <summary>
        /// SPEI
        /// </summary>
        float SPEI { get; } 
    }
}

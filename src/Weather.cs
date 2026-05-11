using System;

namespace Landis.Library.PnETCohorts
{
    public class Weather
    {
        /// <summary>
        /// Calculate average temperature (ºC)
        /// </summary>
        /// <param name="Tmin"></param>
        /// <param name="Tmax"></param>
        /// <returns></returns>
        public static float CalcTavg(float Tmin, float Tmax)
        {
            return (float)((Tmin + Tmax) / 2.0);
        }

        /// <summary>
        /// Calculate daytime average temperature (ºC)
        /// </summary>
        /// <param name="Tavg"></param>
        /// <param name="Tmax"></param>
        /// <returns></returns>
        public static float CalcTday(float Tavg, float Tmax)
        {
            return (float)((Tavg + Tmax) / 2.0);
        }

        /// <summary>
        /// Calculates vapor pressure at temperature T (C) via the Tetens
        /// equations for water or ice; result is in kPa.
        ///     see https://en.wikipedia.org/wiki/Tetens_equation
        /// </summary>
        /// <param name="T">Temperature (C)</param>
        public static float CalcVaporPressure(float T)
        {
            float coeff = 0.61078;
            if (T >= 0.0)  // above freezing point -- vapor pressure over water
            {
                t_coeff = 17.26939; 
                t_offset = 237.3;
            }
            else  // below freezing point -- vapor pressure over ice
            {
                t_coeff = 21.87456;
                t_offset = 265.5;  
            }
            float es = coeff * (float)Math.Exp(t_coeff * T / (T + t_offset));
            return es;
        }

        /// <summary>
        /// Calculate vapor pressure deficit for daytime temperature Tday
        /// </summary>
        /// <param name="Tday">Daytime-average temperature (C)</param>
        /// <param name="Tmin">Daily minimum temperature (C)</param>
        public static float CalcVPD(float Tday, float Tmin)
        {
            float es_day = CalcVaporPressure(Tday);
            float es_night = CalcVaporPressure(Tmin);
            float VPD = es_day - es_night;
            return VPD;
        }
       
        /// <summary>
        /// Slope of vapor pressure curve at temperature T (C)
        ///     Cabrera et al. 2016 (Table 1)
        /// </summary>
        /// <param name="T">Temperature (C)</param>
        public static float CalcVaporPressureCurveSlope(float T)
        {
            float t_offset = 237.3;
            float Slope = 4098.0 * CalcVaporPressure(T) / (float)Math.Pow(T + t_offset, 2);
            return Slope;
        }
    }
}

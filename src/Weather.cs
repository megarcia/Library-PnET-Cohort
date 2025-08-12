
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
            return (float)((Tmin + Tmax) / 2.0f);
        }

        /// <summary>
        /// Calculate daytime average temperature (ºC)
        /// </summary>
        /// <param name="Tavg"></param>
        /// <param name="Tmax"></param>
        /// <returns></returns>
        public static float CalcTday(float Tavg, float Tmax)
        {
            return (float)((Tavg + Tmax) / 2.0f);
        }

        /// <summary>
        /// Calculates vapor pressure at temperature (T) via the Tetens
        /// equations for water and ice   
        ///     see https://en.wikipedia.org/wiki/Tetens_equation
        /// </summary>
        /// <param name="T">Air temperature (°C)</param>
        private static float CalcVaporPressure(float T)
        {
            float es;
            if (T >= 0f)
                // above freezing point -- vapor pressure over water
                es = 0.61078f * (float)Math.Exp(17.26939f * T / (T + 237.3f));
            else
                // below freezing point -- vapor pressure over ice
                es = 0.61078f * (float)Math.Exp(21.87456f * T / (T + 265.5f));
            return es;
        }

        /// <summary>
        /// Slope of curve of water pressure and air temp
        ///     Cabrera et al. 2016 (Table 1)
        /// </summary>
        /// <param name="T">Temperature (C)</param>
        /// <returns></returns>
        private static float CalcVaporPressureCurveSlope(float T)
        {
            float slope = 4098F * CalcVaporPressure(T) / (float)Math.Pow(T + 237.3, 2);
            return slope;
        }

        /// <summary>
        /// Calculate vapor pressure deficit for daytime temperature
        /// </summary>
        public static float CalcVPD(float Tday, float Tmin)
        {
            float es = CalcVaporPressure(Tday);
            float emean = CalcVaporPressure(Tmin);
            return es - emean;
        }
    }
}

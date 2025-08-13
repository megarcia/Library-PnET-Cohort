
namespace Landis.Library.PnETCohorts
{
    public class Respiration
    {
        /// <summary>
        /// Calculate base foliar respiration fraction via Wythers
        /// </summary>
        /// <param name="Tavg"></param>
        /// <returns></returns>
        public static float CalcBaseFolRespFrac_Wythers(float Tavg)
        {
            float BaseFolRespFrac = 0.138071F - 0.0024519F * Tavg;;
            return BaseFolRespFrac;
        }

        /// <summary>
        /// Calculate respiration Q10 factor via Wythers
        /// </summary>
        /// <param name="Tavg"></param>
        /// <param name="PsnTopt"></param>
        /// <returns></returns>
        public static float CalcQ10_Wythers(float Tavg, float PsnTopt)
        {
            // Midpoint between Tavg and optimal T for photosynthesis
            float Tmid = (Tavg + PsnTopt) / 2F;
            float Q10 = 3.22F - 0.046F * Tmidpoint;
            return Q10;
        }

        /// <summary>
        /// Calculate respiration Q10 reduction factor
        /// </summary>
        /// <param name="Q10"></param>
        /// <param name="Tday"></param>
        /// <param name="PsnTopt"></param>
        /// <returns></returns>
        public static float CalcFQ10(float Q10, float Tday, float PsnTopt)
        {
            float FQ10 = (float)Math.Pow(Q10, (Tday - PsnTopt) / 10F);
            return FQ10;
        }

        /// <summary>
        /// Calculate temperature-dependent factor for respiration
        /// </summary>
        /// <param name="Q10"></param>
        /// <param name="Tday"></param>
        /// <param name="Tmin"></param>
        /// <param name="PsnTopt"></param>
        /// <param name="DayLength"></param>
        /// <param name="NightLength"></param>
        /// <returns></returns>
        public static float CalcFTemp(float Q10, float Tday, float Tmin, float PsnTopt, float DayLength, float NightLength)
        {
            // Daytime maintenance respiration factor (scaling factor of actual vs potential respiration applied to daytime average temperature)
            float FTempDay = CalcFQ10(Q10, Tday, PsnTopt);
            // Night maintenance respiration factor (scaling factor of actual vs potential respiration applied to nighttime minimum temperature)
            float FTempNight = CalcFQ10(Q10, Tmin, PsnTopt);
            // Unitless respiration adjustment
            float FTemp = (float)Math.Min(1.0, (FTempDay * DayLength + FTempNight * NightLength) / (DayLength + NightLength));
            return FTemp;
        }
    }
}

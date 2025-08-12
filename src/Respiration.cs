
namespace Landis.Library.PnETCohorts
{
    public class Respiration
    {
        /// <summary>
        /// Generic computation for a Q10 reduction factor
        /// used for respiration calculations
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
    }
}


namespace Landis.Library.PnETCohorts
{
    public interface IEvapotranspiration
    {
        /// <summary>
        /// Calculate reference ET
        /// </summary>
        /// <param name="T"></param>
        /// <param name="DayLength"></param>
        /// <returns></returns>
        float CalcReferenceET_Hamon(float T, float DayLength);
    }
}


namespace Landis.Library.PnETCohorts
{
    public interface IHydrology
    {
        /// <summary>
        /// volumetric water (mm/m)
        /// </summary>
        float Water { get; } 

        /// <summary>
        /// Get the pressurehead (mmH2O) for the current water content
        /// </summary>
        /// <param name="ecoregion"></param>
        /// <returns></returns>
        float GetPressureHead(IEcoregionPnET ecoregion); 

        /// <summary>
        /// Add mm water to volumetric water content (mm/m) (considering activeSoilDepth - frozen soil cannot accept water)
        /// </summary>
        /// <param name="water"></param>
        /// <param name="activeSoilDepth"></param>
        /// <returns></returns>
        bool AddWater(float water, float activeSoilDepth); 

        float CalcEvaporation(SiteCohorts sitecohorts, float PET);

        /// <summary>
        /// volumetric water content (mm/m) of the frozen soil
        /// </summary>
        float FrozenWaterContent { get; } 

        /// <summary>
        /// Depth at which soil is frozen (mm); Rooting zone soil below this depth is frozen
        /// </summary>
        float FrozenDepth { get; } 

        /// <summary>
        /// Change FrozenWaterContent
        /// </summary>
        /// <param name="water"></param>
        /// <returns></returns>
        bool SetFrozenWaterContent(float water);  

        /// <summary>
        /// Change FrozenDepth
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        bool SetFrozenDepth(float depth); 

        /// <summary>
        /// Calculate reference ET
        /// </summary>
        /// <param name="T"></param>
        /// <param name="dayLength"></param>
        /// <returns></returns>
        float CalcRET_Hamon(float T, float dayLength); 

        /// <summary>
        /// Get the PressureHeadTable object
        /// </summary>
        PressureHeadSaxton_Rawls PressureHeadTable { get; } 
    }
}

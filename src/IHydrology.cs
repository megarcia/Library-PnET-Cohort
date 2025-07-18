
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

        float CalcEvaporation(SiteCohorts sitecohorts, float PotentialET);

        /// <summary>
        /// volumetric water content (mm/m) of the frozen soil
        /// </summary>
        float FrozenSoilWaterContent { get; } 

        /// <summary>
        /// Depth at which soil is frozen (mm); Rooting zone soil below this depth is frozen
        /// </summary>
        float FrozenSoilDepth { get; } 

        /// <summary>
        /// Change FrozenSoilWaterContent
        /// </summary>
        /// <param name="water"></param>
        /// <returns></returns>
        bool SetFrozenSoilWaterContent(float water);  

        /// <summary>
        /// Change FrozenSoilDepth
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        bool SetFrozenSoilDepth(float depth); 

        /// <summary>
        /// Calculate reference ET
        /// </summary>
        /// <param name="T"></param>
        /// <param name="dayLength"></param>
        /// <returns></returns>
        float CalcReferenceET_Hamon(float T, float dayLength); 

        /// <summary>
        /// Get the PressureHeadTable object
        /// </summary>
        PressureHeadSaxton_Rawls PressureHeadTable { get; } 
    }
}

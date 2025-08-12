
namespace Landis.Library.PnETCohorts
{
    public interface IHydrology
    {
        /// <summary>
        /// volumetric soilWaterContent (mm/m)
        /// </summary>
        float SoilWaterContent { get; } 

        /// <summary>
        /// Get the pressurehead (mmH2O) for the current soil water content
        /// </summary>
        /// <param name="ecoregion"></param>
        /// <returns></returns>
        float GetPressureHead(IEcoregionPnET ecoregion); 

        /// <summary>
        /// Add mm water to volumetric soil water content (mm/m) (considering activeSoilDepth - frozen soil cannot accept water)
        /// </summary>
        /// <param name="soilWaterContent"></param>
        /// <param name="activeSoilDepth"></param>
        /// <returns></returns>
        bool AddWater(float soilWaterContent, float activeSoilDepth); 

        float CalcEvaporation(SiteCohorts sitecohorts, float PotentialET);

        /// <summary>
        /// volumetric soil water content (mm/m) of the frozen soil
        /// </summary>
        float FrozenSoilWaterContent { get; } 

        /// <summary>
        /// Depth at which soil is frozen (mm); Rooting zone soil below this depth is frozen
        /// </summary>
        float FrozenSoilDepth { get; } 

        /// <summary>
        /// Change FrozenSoilWaterContent
        /// </summary>
        /// <param name="soilWaterContent"></param>
        /// <returns></returns>
        bool SetFrozenSoilWaterContent(float soilWaterContent);  

        /// <summary>
        /// Change FrozenSoilDepth
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        bool SetFrozenSoilDepth(float depth); 

        /// <summary>
        /// Get the PressureHeadTable object
        /// </summary>
        Hydrology_SaxtonRawls PressureHeadTable { get; } 
    }
}

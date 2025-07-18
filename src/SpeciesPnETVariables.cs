
namespace Landis.Library.PnETCohorts
{
    public class SpeciesPnETVariables
    {
        /// <summary>
        /// Unitless respiration adjustment based on temperature: for output only
        /// </summary>
        public float FTempRespWeightedDayAndNight; 

        /// <summary>
        /// Scaling factor of respiration given day and night temperature and day and night length
        /// </summary>
        public float MaintRespFTempResp; 

        /// <summary>
        /// Respiration Q10 factor
        /// </summary>
        public float Q10Factor;  

        /// <summary>
        /// Base foliar respiration fraction (using Wythers when selected)
        /// </summary>
        public float BaseFolRespFrac; 

        /// <summary>
        /// Photosynthesis reduction factor due to temperature: for output only
        /// </summary>
        public float FTempPsn; 

        /// <summary>
        /// Adjustment to Amax based on CO2: for output only
        /// </summary>
        public float DelAmax;  

        public float JH2O;

        public float AmaxB_CO2;

        /// <summary>
        /// Gradient of effect of vapor pressure deficit on growth
        /// </summary>
        public float DVPD; 
    }
}

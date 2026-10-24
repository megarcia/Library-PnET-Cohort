using System.Collections.Generic;
using Landis.Core;
using Landis.Library.Parameters;
using Landis.Library.UniversalCohorts;

namespace Landis.Library.PnETCohorts 
{
    /// <summary>
    /// PnET SiteCohort interface
    /// Inherits from UniversalCohorts ISiteCohort interface
    /// </summary>
    public interface ISiteCohorts : UniversalCohorts.ISiteCohorts
    {
        float[] NetPsn { get; }
        float[] MaintResp{ get; }
        float[] GrossPsn{ get; }
        float[] FolResp { get; }
        float[] AverageAlbedo { get; }
        float[] ActiveLayerDepth { get; }
        float[] FrostDepth { get; }
        float[] MonthlyAvgSnowPack { get; }
        float[] MonthlyAvgWater { get; }
        float[] MonthlyAvgLAI { get; }
        float[] MonthlyEvap { get; }
        float[] MonthlyActualTrans { get; }
        float[] MonthlyLeakage { get; }
        float[] MonthlyInterception { get; }
        float[] MonthlyRunoff { get; }
        float[] MonthlyAET { get; }
        float[] MonthlyPotentialEvap { get; }
        float[] MonthlyPotentialTrans { get; }
        float CanopyLAImax{get;}
        float SiteMossDepth { get; }
        int AverageAge { get; }
        float BiomassSum { get; }
        float AbovegroundBiomassSum { get; }
        float WoodBiomassSum { get; }
        float WoodySenescenceSum { get; }
        float FoliageSenescenceSum { get; }
        int CohortCount { get; }
        float JulySubcanopyPAR { get; }
        float SubcanopyPARmax { get; }
        double Litter{ get; }
        double WoodyDebris { get; }
        int AgeMax { get; }
        float WaterAvg { get; }
        float BelowGroundBiomassSum { get; }
        float FoliageSum { get; }
        float NSCSum { get; }
        float AETSum { get; } //mm
        float NetPsnSum { get; }
        float PET { get; }
        IEstablishmentProbability EstablishmentProbability { get; }
        Library.Parameters.Species.AuxParm<int> CohortCountPerSpecies { get; }
        Library.Parameters.Species.AuxParm<bool> SpeciesPresent { get; }
        Library.Parameters.Species.AuxParm<int> MaxFoliageYearPerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> BiomassPerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> AbovegroundBiomassPerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> WoodBiomassPerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> BelowGroundBiomassPerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> FoliageBiomassPerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> NSCPerSpecies { get; }
        Library.Parameters.Species.AuxParm<float> LAIPerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> WoodySenescencePerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> FoliageSenescencePerSpecies { get; }
        Library.Parameters.Species.AuxParm<List<ushort>> CohortAges { get; }
        List<Landis.Core.ISpecies> SpeciesByPlant { get; set; }
        List<Landis.Core.ISpecies> SpeciesBySerotiny { get; set; }
        List<Landis.Core.ISpecies> SpeciesByResprout { get; set; }
        List<Landis.Core.ISpecies> SpeciesBySeed { get; set; }
        List<int> CohortsBySuccession { get; set; }
        List<int> CohortsByCold { get; set; }
        List<int> CohortsByHarvest { get; set; }
        List<int> CohortsByFire { get; set; }
        List<int> CohortsByWind { get; set; }
        List<int> CohortsByOther { get; set; }
    }
}

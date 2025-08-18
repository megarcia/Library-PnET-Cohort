using System.Collections.Generic;
using Landis.Core;

namespace Landis.Library.PnETCohorts 
{
    public interface ISiteCohorts : Landis.Library.UniversalCohorts.ISiteCohorts
    {
        float[] NetPsn { get; }
        float[] MaintResp{ get; }
        float[] GrossPsn{ get; }
        float[] FoliarRespiration { get; }
        float[] AverageAlbedo { get; }
        float[] ActiveLayerDepth { get; }
        float[] FrostDepth { get; }
        float[] MonthlyAvgSnowpack { get; }
        float[] MonthlyAvgWater { get; }
        float[] MonthlyAvgLAI { get; }
        float[] MonthlyEvap { get; }
        float[] MonthlyActualTrans { get; }
        float[] MonthlyLeakage { get; }
        float[] MonthlyInterception { get; }
        float[] MonthlyRunoff { get; }
        float[] MonthlyActualET { get; }
        float[] MonthlyPotentialEvap { get; }
        float[] MonthlyPotentialTrans { get; }
        float CanopyLAImax{ get; }
        float SiteMossDepth { get; }
        int AverageAge { get; }
        Landis.Library.Parameters.Species.AuxParm<int> CohortCountPerSpecies { get; }
        Landis.Library.Parameters.Species.AuxParm<bool> SpeciesPresent { get; }
        IProbEstablishment ProbEstablishment { get; }
        Landis.Library.Parameters.Species.AuxParm<int> MaxFoliageYearPerSpecies { get; }
        Landis.Library.Parameters.Species.AuxParm<int> BiomassPerSpecies { get; }
        Landis.Library.Parameters.Species.AuxParm<int> AbovegroundBiomassPerSpecies { get; }
        Landis.Library.Parameters.Species.AuxParm<int> WoodBiomassPerSpecies { get; }
        Landis.Library.Parameters.Species.AuxParm<int> BelowGroundBiomassPerSpecies { get; }
        Landis.Library.Parameters.Species.AuxParm<int> FoliageBiomassPerSpecies { get; }
        Landis.Library.Parameters.Species.AuxParm<int> NSCPerSpecies { get; }
        Landis.Library.Parameters.Species.AuxParm<float> LAIPerSpecies { get; }
        Landis.Library.Parameters.Species.AuxParm<int> WoodSenescencePerSpecies { get; }
        Landis.Library.Parameters.Species.AuxParm<int> FolSenescencePerSpecies { get; }
        Landis.Library.Parameters.Species.AuxParm<List<ushort>> CohortAges { get; }
        float BiomassSum { get; }
        float AbovegroundBiomassSum { get; }
        float WoodBiomassSum { get; }
        float WoodSenescenceSum { get; }
        float FolSenescenceSum { get; }
        int CohortCount { get; }
        float JulySubCanopyPar { get; }
        float SubCanopyParMAX { get; }
        double LeafLitter{ get; }
        double WoodDebris { get; }
        int AgeMax { get; }
        float AvgSoilWaterContent { get; }
        float BelowGroundBiomassSum { get; }
        float FoliageSum { get; }
        float NSCSum { get; }
        float ActualETSum { get; } //mm
        float NetPsnSum { get; }
        float PotentialET { get; }

        List<ISpecies> SpeciesByPlant { get; set; }
        List<ISpecies> SpeciesBySerotiny { get; set; }
        List<ISpecies> SpeciesByResprout { get; set; }
        List<ISpecies> SpeciesBySeed { get; set; }
        List<int> CohortsBySuccession { get; set; }
        List<int> CohortsByCold { get; set; }
        List<int> CohortsByHarvest { get; set; }
        List<int> CohortsByFire { get; set; }
        List<int> CohortsByWind { get; set; }
        List<int> CohortsByOther { get; set; }
    }
}

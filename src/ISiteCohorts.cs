using System.Collections.Generic;

namespace Landis.Library.PnETCohorts 
{
    public interface ISiteCohorts : Library.UniversalCohorts.ISiteCohorts
    {
        float[] NetPsn { get; }
        float[] MaintResp { get; }
        float[] GrossPsn { get; }
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
        float CanopyLAImax { get; }
        float SiteMossDepth { get; }
        int AverageAge { get; }
        Library.Parameters.Species.AuxParm<int> CohortCountPerSpecies { get; }
        Library.Parameters.Species.AuxParm<bool> SpeciesPresent { get; }
        IProbEstablishment ProbEstablishment { get; }
        Library.Parameters.Species.AuxParm<int> MaxFoliageYearPerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> BiomassPerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> AGBiomassPerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> WoodBiomassPerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> BGBiomassPerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> FoliageBiomassPerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> NSCPerSpecies { get; }
        Library.Parameters.Species.AuxParm<float> LAIPerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> WoodSenescencePerSpecies { get; }
        Library.Parameters.Species.AuxParm<int> FolSenescencePerSpecies { get; }
        Library.Parameters.Species.AuxParm<List<ushort>> CohortAges { get; }
        float BiomassSum { get; }
        float AGBiomassSum { get; }
        float WoodBiomassSum { get; }
        float WoodSenescenceSum { get; }
        float FolSenescenceSum { get; }
        int CohortCount { get; }
        float JulySubCanopyPar { get; }
        float SubCanopyParMAX { get; }
        double LeafLitter { get; }
        double WoodDebris { get; }
        int AgeMax { get; }
        float AvgSoilWaterContent { get; }
        float BGBiomassSum { get; }
        float FolBiomassSum { get; }
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

// NOTE: ISpecies --> Landis.Core

using System.Collections.Generic;
using Landis.Core;
using Landis.Library.Parameters;
using Landis.Library.UniversalCohorts;

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
        float JulySubCanopyPAR { get; }
        float MaxSubCanopyPAR { get; }
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

        List<ISpecies> SpeciesEstByPlanting { get; set; }
        List<ISpecies> SpeciesEstBySerotiny { get; set; }
        List<ISpecies> SpeciesEstByResprout { get; set; }
        List<ISpecies> SpeciesEstBySeeding { get; set; }
        List<int> CohortsDiedBySuccession { get; set; }
        List<int> CohortsDiedByCold { get; set; }
        List<int> CohortsDiedByHarvest { get; set; }
        List<int> CohortsDiedByFire { get; set; }
        List<int> CohortsDiedByWind { get; set; }
        List<int> CohortsDiedByOther { get; set; }
    }
}

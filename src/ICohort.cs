using System.Collections.Generic;
using Landis.Library.UniversalCohorts;

namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// PnET's Cohort Interface
    /// </summary>
    public interface ICohort : Library.UniversalCohorts.ICohort
    {
        new CohortData Data { get; }
        ushort SuccessionTimestep { get; }
        int AGBiomass { get; }
        int TotalBiomass { get; }
        bool IsLeafOn { get; }
        float MaxBiomass { get; }
        float Fol { get; }
        float MaxFolYear { get; }
        float NSC { get; }
        float DefoliationFrac { get; }
        float LastWoodSenescence { get; }
        float LastFolSenescence { get; }
        float LastFRad { get; }
        List<float> LastSeasonFRad { get; }
        float adjFolBiomassFrac { get; }
        float AdjHalfSat { get; }
        float adjFolN { get; }
        int ColdKill { get; }
        byte Layer { get; }
        float[] LAI { get; }
        float LastLAI { get; }
        float LastAGBio { get; }
        float[] GrossPsn { get; }
        float[] FoliarRespiration { get; }
        float[] NetPsn { get; }
        float[] MaintenanceRespiration { get; }
        float[] Transpiration { get; }
        float[] PotentialTranspiration { get; }
        float[] FRad { get; }
        float[] FWater { get; }
        float[] SoilWaterContent { get; }
        float[] PressHead { get; }
        int[] NumPrecipEvents { get; }
        float[] FOzone { get; }
        float[] Interception { get; }
        float[] AdjFolN { get; }
        float[] AdjFolBiomassFrac { get; }
        float[] CiModifier { get; }
        float[] DelAmax { get; }
        float BiomassLayerFrac { get; }
        float CanopyLayerFrac { get; }
        float CanopyGrowingSpace { get; }
    }
}

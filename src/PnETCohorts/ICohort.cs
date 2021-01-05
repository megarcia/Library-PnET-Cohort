namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// PnET's Cohort Interface
    /// </summary>
    public interface ICohort
        :Landis.Library.BiomassCohorts.ICohort, Landis.Library.AgeOnlyCohorts.ICohort
    {
        
        //---------------------------------------------------------------------

        //void IncrementAge();
    }
}

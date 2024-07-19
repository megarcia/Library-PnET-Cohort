//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Library.UniversalCohorts;
using Landis.SpatialModeling;
using Landis.Core;

namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// A wrapped age-cohort disturbance so it works with biomass cohorts.
    /// </summary>
    public class WrappedDisturbance
        : IDisturbance
    {
        private ICohortDisturbance ageCohortDisturbance;

        //---------------------------------------------------------------------

        public WrappedDisturbance(ICohortDisturbance ageCohortDisturbance)
        {
            this.ageCohortDisturbance = ageCohortDisturbance;
        }

        //---------------------------------------------------------------------

        public ExtensionType Type
        {
            get {
                return ageCohortDisturbance.Type;
            }
        }

        //---------------------------------------------------------------------

        public ActiveSite CurrentSite
        {
            get {
                return ageCohortDisturbance.CurrentSite;
            }
        }

        //---------------------------------------------------------------------

        public int ReduceOrKillMarkedCohort(ICohort cohort)
        {
            if (ageCohortDisturbance.MarkCohortForDeath(cohort)) {
                Cohort.KilledByAgeOnlyDisturbance(this, cohort,
                                                  ageCohortDisturbance.CurrentSite,
                                                  ageCohortDisturbance.Type);
                return (int)cohort.Data.Biomass;
            }
            else
                return 0;
        }
    }
}

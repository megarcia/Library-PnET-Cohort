using Landis.Core;
using Landis.SpatialModeling;
using Landis.Utilities;

namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// A calculator for computing the change in an individual cohort's biomass
    /// due to annual growth and mortality.
    /// </summary>
    public interface ICohorts
    {
        /// <summary>
        /// Computes the change in an individual cohort's biomass due to annual
        /// growth and mortality.
        /// </summary>
        /// <param name="cohort">
        /// The cohort whose biomass the change is to be computed for.
        /// </param>
        /// <param name="site">
        /// The site where the cohort is located.
        /// </param>
        int ComputeChange(ICohort cohort, Landis.SpatialModeling.ActiveSite site);

        /// <summary>
        /// Computes the percentage of a cohort's biomass that is non-woody.
        /// </summary>
        /// <param name="cohort">
        /// The cohort.
        /// </param>
        /// <param name="site">
        /// The site where the cohort is located.
        /// </param>
        Percentage ComputeNonWoodyPercentage(ICohort cohort, Landis.SpatialModeling.ActiveSite site);
    }
}

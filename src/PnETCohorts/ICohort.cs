//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;

namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// A species cohort with number of tree information.
    /// </summary>
    public interface ICohort
        :Landis.Library.BiomassCohorts.ICohort, Landis.Library.AgeOnlyCohorts.ICohort
    {
        ushort Age
        {
            get;
        }
        //---------------------------------------------------------------------

        void IncrementAge();
    }
}

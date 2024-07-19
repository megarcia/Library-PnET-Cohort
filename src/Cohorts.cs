//  Authors:  Robert M. Scheller, James B. Domingo

using System;

namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// Methods for PnET cohorts.
    /// </summary>
    public static class Cohorts
    {
        private static int successionTimeStep;
        private static ICalculator biomassCalculator;

        //---------------------------------------------------------------------

        /// <summary>
        /// The succession time step used by biomass cohorts.
        /// </summary>
        public static int SuccessionTimeStep
        {
            get {
                return successionTimeStep;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The calculator for computing how a cohort's biomass changes.
        /// </summary>
        public static ICalculator BiomassCalculator
        {
            get {
                return biomassCalculator;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes the cohorts module.
        /// </summary>
        /// <param name="successionTimeStep">
        /// The time step for the succession extension.  Unit: years
        /// </param>

        public static void Initialize(int         successionTimeStep)
        {
            Cohorts.successionTimeStep = successionTimeStep;

        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the total biomass for all the cohorts at a site.
        /// </summary>
        public static int ComputeBiomass(ISiteCohorts siteCohorts)
        {
            int youngBiomass;
            return ComputeBiomass(siteCohorts, out youngBiomass);
        }
        
        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the total aboveground live biomass for all the cohorts at a site, and the
        /// total aboveground live biomass for all the young cohorts.
        /// </summary>
        public static int ComputeBiomass(ISiteCohorts siteCohorts,
                                         out int      youngBiomass)
        {
            youngBiomass = 0;
            int totalBiomass = 0;
            foreach (ISpeciesCohorts speciesCohorts in (Landis.Library.UniversalCohorts.ISiteCohorts)siteCohorts) {
                foreach (ICohort cohort in speciesCohorts) {
                    totalBiomass += (int)cohort.Data.Biomass;
                    if (cohort.Data.Age < successionTimeStep)
                        youngBiomass += (int)cohort.Data.Biomass;
                }
            }
            return totalBiomass;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the total aboveground live biomass for all the cohorts, not including young cohorts.
        /// </summary>
        public static int ComputeNonYoungBiomass(ISiteCohorts siteCohorts)
        {
            if (siteCohorts == null)
                return 0;
            int totalBiomass = 0;
            foreach (ISpeciesCohorts speciesCohorts in (Landis.Library.UniversalCohorts.ISiteCohorts)siteCohorts) {
                foreach (ICohort cohort in speciesCohorts) {
                    if (cohort.Data.Age >= successionTimeStep)
                        totalBiomass += (int)cohort.Data.Biomass;
                }
            }
            return totalBiomass;
        }

    }
}

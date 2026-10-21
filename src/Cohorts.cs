using System;
using Landis.Library.UniversalCohorts;

namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// Methods for PnET cohorts.
    /// </summary>
    public static class Cohorts
    {
        private static int successionTimeStep;
        private static ICalculator biomassCalculator;

        /// <summary>
        /// The succession time step used by biomass cohorts.
        /// </summary>
        public static int SuccessionTimeStep
        {
            get 
            {
                return successionTimeStep;
            }
        }

        /// <summary>
        /// The calculator for computing how a cohort's biomass changes.
        /// </summary>
        public static ICalculator Calculator
        {
            get 
            {
                return biomassCalculator;
            }
        }

        /// <summary>
        /// Initializes the cohorts module.
        /// </summary>
        /// <param name="timestep">
        /// The time step for the Succession extension.  Unit: years
        /// </param>

        public static void Initialize(int timestep)
        {
            successionTimeStep = timestep;
        }

        /// <summary>
        /// Computes the total biomass for all the cohorts at a site.
        /// </summary>
        public static int ComputeBiomass(ISiteCohorts siteCohorts)
        {
            return ComputeBiomass(siteCohorts);
        }
        
        /// <summary>
        /// Computes the total aboveground live biomass for all the cohorts at a site, and the
        /// total aboveground live biomass for all the young cohorts.
        /// </summary>
        public static int ComputeBiomass(ISiteCohorts siteCohorts,
                                         out int youngBiomass)
        {
            youngBiomass = 0;
            int totalBiomass = 0;
            foreach (ISpeciesCohorts speciesCohorts in (UniversalCohorts.ISiteCohorts)siteCohorts) {
                foreach (ICohort cohort in speciesCohorts)
                {
                    // totalBiomass += (int)cohort.Data.UniversalData.Biomass;  //MG20261021
                    totalBiomass += (int)cohort.Data.UnivCohort.Data.Biomass;  //MG20261021
                    // if (cohort.Data.UniversalData.Age < successionTimeStep)  //MG20261021
                    if (cohort.Data.UnivCohort.Data.Age < successionTimeStep)  //MG20261021
                    {
                        // youngBiomass += (int)cohort.Data.UniversalData.Biomass;  //MG20261021
                        youngBiomass += (int)cohort.Data.UnivCohort.Data.Biomass;  //MG20261021
                    }
                }
            }
            return totalBiomass;
        }

        /// <summary>
        /// Computes the total aboveground live biomass for all the cohorts, not including young cohorts.
        /// </summary>
        public static int ComputeNonYoungBiomass(ISiteCohorts siteCohorts)
        {
            if (siteCohorts == null)
                return 0;
            int totalBiomass = 0;
            foreach (ISpeciesCohorts speciesCohorts in (UniversalCohorts.ISiteCohorts)siteCohorts)
                foreach (ICohort cohort in speciesCohorts)
                {
                    // if (cohort.Data.UniversalData.Age >= successionTimeStep)  //MG20261021
                    //     totalBiomass += (int)cohort.Data.UniversalData.Biomass;  //MG20261021
                    if (cohort.Data.UnivCohort.Data.Age >= successionTimeStep)  //MG20261021
                        totalBiomass += (int)cohort.Data.UnivCohort.Data.Biomass;  //MG20261021
                }
            return totalBiomass;
        }
    }
}

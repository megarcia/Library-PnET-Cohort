using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// Disturbance effects on canopy foliage, wood debris,
    /// and leaf litter pools. Input parameters are fractions
    /// of biomass that are allocated to the different pools.
    /// </summary>
    public class Disturbance
    {
        // These labels are used as input parameters in the input txt file
        private static readonly List<string> Disturbances = new List<string>() { "fire", "wind", "bda", "harvest" };
        private static readonly List<string> Reductions = new List<string>() { "WoodReduction", "FolReduction", "RootReduction", "WoodDebrisReduction", "LeafLitterReduction" };

        public static void Initialize(string fn, SortedDictionary<string, Parameter<string>> parameters)
        {
            Dictionary<string, Parameter<string>> BiomassReductionParameters = Names.LoadTable(Names.DisturbanceReductions, Reductions, Disturbances);
            foreach (KeyValuePair<string, Parameter<string>> parameter in BiomassReductionParameters)
            {
                if (parameters.ContainsKey(parameter.Key))
                    throw new Exception("Parameter " + parameter.Key + " was provided twice");
                foreach (string value in parameter.Value.Values)
                {
                    double v;
                    if (double.TryParse(value, out v) == false)
                        throw new Exception("Expecting digit value for " + parameter.Key);
                    if (v > 1 || v < 0)
                        throw new Exception("Expecting value for " + parameter.Key + " between 0.0 and 1.0. Found " + v);
                }
            }
            BiomassReductionParameters.ToList().ForEach(x => parameters.Add("disturbance:"+x.Key, x.Value));
        }

        public static float ReduceFoliage(float FolBiomass, double defoliationFrac)
        {
            FolBiomass *= (float)(1.0 - defoliationFrac);
            return FolBiomass;
        }

        public static void ReduceDeadPools(object sitecohorts, ExtensionType disturbanceType)
        {
            if (sitecohorts == null)
                throw new Exception("sitecohorts should not be null");
            float WoodDebrisReductionFrac = 0;
            float LeafLitterReductionFrac = 0;
            if (disturbanceType != null && Names.TryGetParameter(disturbanceType.Name, out Parameter<string> parameter))
            {
                // If parameters are available, then set the loss fractions here.
                if (parameter.ContainsKey("WoodDebrisReduction"))
                    WoodDebrisReductionFrac = float.Parse(parameter["WoodDebrisReduction"]);
                if (parameter.ContainsKey("LeafLitterReduction"))
                    LeafLitterReductionFrac = float.Parse(parameter["LeafLitterReduction"]);
            }
            ((SiteCohorts)sitecohorts).RemoveWoodDebris(WoodDebrisReductionFrac);
            ((SiteCohorts)sitecohorts).RemoveLeafLitter(LeafLitterReductionFrac);
        }

        public static void AllocateDeadPools(object sitecohorts, Cohort cohort, ExtensionType disturbanceType, double frac)
        {
            if (sitecohorts == null)
                throw new Exception("sitecohorts should not be null");
            // By default, all material is allocated to the wood debris or the leaf litter pool
            float WoodReductionFrac = 0;
            float RootReductionFrac = 0;
            float FolReductionFrac = 0;
            if (disturbanceType != null && Names.TryGetParameter(disturbanceType.Name, out Parameter<string> parameter))
            {
                // If parameters are available, then set the loss fractions here.
                if (parameter.ContainsKey("WoodReduction"))
                    WoodReductionFrac = float.Parse(parameter["WoodReduction"]);
                if (parameter.ContainsKey("RootReduction"))
                    RootReductionFrac = float.Parse(parameter["RootReduction"]);
                if (parameter.ContainsKey("FolReduction"))
                    FolReductionFrac = float.Parse(parameter["FolReduction"]);
            }            
            // Add new dead wood and leaf litter
            float woodAdded = (float)((1 - WoodReductionFrac) * cohort.Wood * frac);
            float rootAdded = (float)((1 - RootReductionFrac) * cohort.Root * frac);
            float folAdded = (float)((1 - FolReductionFrac) * cohort.Fol * frac);
            // Using Canopy fractioning
            ((SiteCohorts)sitecohorts).AddWoodDebris(woodAdded * cohort.CanopyLayerFrac, cohort.PnETSpecies.WoodDebrisDecompRate);
            ((SiteCohorts)sitecohorts).AddWoodDebris(rootAdded * cohort.CanopyLayerFrac, cohort.PnETSpecies.WoodDebrisDecompRate);
            ((SiteCohorts)sitecohorts).AddLeafLitter(folAdded * cohort.CanopyLayerFrac, cohort.PnETSpecies.FolLignin);
            cohort.AccumulateWoodSenescence((int)((woodAdded + rootAdded) * cohort.CanopyLayerFrac));
            cohort.AccumulateFolSenescence((int)(folAdded * cohort.CanopyLayerFrac));
        }
    }
}

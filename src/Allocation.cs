using Landis.Core;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// Allocates litters that result from disturbances. 
    /// Input parameters are fractions of litter that are allocated to different pools
    /// </summary>
    public class Allocation
    {
        // These labels are used as input parameters in the input txt file
        private static readonly List<string> Disturbances = new List<string>() { "fire", "wind", "bda", "harvest" };

        private static readonly List<string> Reductions = new List<string>() { "WoodReduction", "FolReduction", "RootReduction", "DeadWoodReduction", "LitterReduction" };

        public static void Initialize(string fn, SortedDictionary<string, Parameter<string>> parameters)
        {
            Dictionary<string, Parameter<string>> DisturbanceReductionParameters = Names.LoadTable(Names.DisturbanceReductions, Reductions, Disturbances);
            foreach (KeyValuePair<string, Parameter<string>> parameter in DisturbanceReductionParameters)
            {
                if (parameters.ContainsKey(parameter.Key))
                    throw new System.Exception("Parameter " + parameter.Key + " was provided twice");
                foreach (string value in parameter.Value.Values)
                {
                    double v;
                    if (double.TryParse(value, out v) == false)
                        throw new System.Exception("Expecting digit value for " + parameter.Key);
                    if (v > 1 || v < 0)
                        throw new System.Exception("Expecting value for " + parameter.Key + " between 0.0 and 1.0. Found " + v);
                }
            }
            DisturbanceReductionParameters.ToList().ForEach(x => parameters.Add("disturbance:"+x.Key, x.Value));
        }

        public static void ReduceDeadPools(object sitecohorts, ExtensionType disturbanceType)
        {
            if (sitecohorts == null)
                throw new System.Exception("sitecohorts should not be null");
            float pdeadwoodlost = 0;
            float plitterlost = 0;
            Parameter<string> parameter;
            if (disturbanceType != null && Names.TryGetParameter(disturbanceType.Name, out parameter))
            {
                // If parameters are available, then set the loss fractions here.
                if (parameter.ContainsKey("DeadWoodReduction"))
                    pdeadwoodlost = float.Parse(parameter["DeadWoodReduction"]);
                if (parameter.ContainsKey("LitterReduction"))
                    plitterlost = float.Parse(parameter["LitterReduction"]);
            }
            ((SiteCohorts)sitecohorts).RemoveWoodDebris(pdeadwoodlost);
            ((SiteCohorts)sitecohorts).RemoveLitter(plitterlost);
        }

        public static void Allocate(object sitecohorts, Cohort cohort, ExtensionType disturbanceType, double frac)
        {
            if (sitecohorts == null)
                throw new System.Exception("sitecohorts should not be null");
            // By default, all material is allocated to the wood debris or the litter pool
            float pwoodlost = 0;
            float prootlost = 0;
            float pfollost = 0;
            Parameter<string> parameter;
            if (disturbanceType != null && Names.TryGetParameter(disturbanceType.Name, out parameter))
            {
                // If parameters are available, then set the loss fractions here.
                if (parameter.ContainsKey("WoodReduction"))
                    pwoodlost = float.Parse(parameter["WoodReduction"]);
                if (parameter.ContainsKey("RootReduction"))
                    prootlost = float.Parse(parameter["RootReduction"]);
                if (parameter.ContainsKey("FolReduction"))
                    pfollost = float.Parse(parameter["FolReduction"]);
            }            
            // Add new dead wood and litter
            float woodAdded = (float)((1 - pwoodlost) * cohort.Wood * frac);
            float rootAdded = (float)((1 - prootlost) * cohort.Root * frac);
            float folAdded = (float)((1 - pfollost) * cohort.Fol * frac);
            // Using Canopy fractioning
            ((SiteCohorts)sitecohorts).AddWoodDebris(woodAdded * cohort.CanopyLayerFrac, cohort.PnETSpecies.KWdLit);
            ((SiteCohorts)sitecohorts).AddWoodDebris(rootAdded * cohort.CanopyLayerFrac, cohort.PnETSpecies.KWdLit);
            ((SiteCohorts)sitecohorts).AddLitter(folAdded * cohort.CanopyLayerFrac, cohort.PnETSpecies);
            cohort.AccumulateWoodSenescence((int)((woodAdded + rootAdded) * cohort.CanopyLayerFrac));
            cohort.AccumulateFoliageSenescence((int)((folAdded) * cohort.CanopyLayerFrac));
        }
    }
}

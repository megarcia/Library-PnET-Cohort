//  Author: Robert Scheller, Melissa Lucash

using Landis.Library.Climate;
using System.Linq;

namespace Landis.Library.PnETCohorts
{
    public class ClimateRegionData
    {
        public static Library.Parameters.Ecoregions.AuxParm<AnnualClimate> AnnualClimate;

        public static void Initialize()
        {
            AnnualClimate = new Library.Parameters.Ecoregions.AuxParm<AnnualClimate>(Globals.ModelCore.Ecoregions);
            Climate.Climate.GenerateEcoregionClimateData(((Parameter<float>)Names.GetParameter(Names.Latitude)).Value);
            // grab the first year's spinup climate
            foreach (var ecoregion in Globals.ModelCore.Ecoregions.Where(x => x.Active))
            {
                AnnualClimate[ecoregion] = Climate.Climate.SpinupEcoregionYearClimate[ecoregion.Index][1];      // Climate data year index is 1-based
            }
            Globals.SetMinMaxClimateYears();
        }

        public static void SetAllEcoregionsFutureAnnualClimate(int year)
        {
            if (Names.TryGetParameter(Names.ClimateConfigFile, out var climateLibraryFileName))
            {
                // grab the year's future climate
                foreach (var ecoregion in Globals.ModelCore.Ecoregions.Where(x => x.Active))
                {
                    AnnualClimate[ecoregion] = Climate.Climate.FutureEcoregionYearClimate[ecoregion.Index][year];      // Climate data year index is 1-based
                }
            }
        }
    }
}

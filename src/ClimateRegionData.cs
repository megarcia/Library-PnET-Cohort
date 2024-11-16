//  Author: Robert Scheller, Melissa Lucash

using Landis.Core;
using Landis.Library.Climate;
using System;
using System.Linq;

namespace Landis.Library.PnETCohorts
{
    public class ClimateRegionData
    {
        public static Library.Parameters.Ecoregions.AuxParm<AnnualClimate> AnnualClimate;

        //---------------------------------------------------------------------
        //public static void Initialize(IInputParameters parameters)
        public static void Initialize()
        {
            AnnualClimate = new Library.Parameters.Ecoregions.AuxParm<AnnualClimate>(Globals.ModelCore.Ecoregions);

            /*
            foreach (IEcoregion ecoregion in Globals.ModelCore.Ecoregions)
            {
                if (ecoregion.Active)
                {
                    // Latitude is contained in the PnET Ecoregion
                    Climate.Climate.GenerateEcoregionClimateData(ecoregion, 0, EcoregionData.GetPnETEcoregion(ecoregion).Latitude);
                    SetSingleAnnualClimate(ecoregion, 0, Climate.Climate.Phase.SpinUp_Climate);  // Some placeholder data to get things started.
                }
            }
            */

            Climate.Climate.GenerateEcoregionClimateData(((Parameter<float>)Names.GetParameter(Names.Latitude)).Value);

            // grab the first year's spinup climate
            foreach (var ecoregion in Globals.ModelCore.Ecoregions.Where(x => x.Active))
            {
                AnnualClimate[ecoregion] = Climate.Climate.SpinupEcoregionYearClimate[ecoregion.Index][1];      // Climate data year index is 1-based
            }
            Globals.SetMinMaxClimateYears();
        }

        /*
        public static void SetSingleAnnualClimate(IEcoregion ecoregion, int year, Climate.Phase spinupOrfuture)
        {
            int actualYear = Climate.Climate.Future_MonthlyData.Keys.Min() + year;

            if (spinupOrfuture == Climate.Climate.Phase.Future_Climate)
            {
                if (Climate.Climate.Future_MonthlyData.ContainsKey(actualYear))
                {
                    AnnualWeather[ecoregion] = Climate.Climate.Future_MonthlyData[actualYear][ecoregion.Index];
                }
            }
            else
            {
                if (Climate.Climate.Spinup_MonthlyData.ContainsKey(actualYear))
                {
                    AnnualWeather[ecoregion] = Climate.Climate.Spinup_MonthlyData[actualYear][ecoregion.Index];
                }
            }
        }
        */

        public static void SetAllEcoregionsFutureAnnualClimate(int year)
        {
            if(Names.TryGetParameter(Names.ClimateConfigFile, out var climateLibraryFileName))
            {
                // grab the year's future climate
                foreach (var ecoregion in Globals.ModelCore.Ecoregions.Where(x => x.Active))
                {
                    AnnualClimate[ecoregion] = Climate.Climate.FutureEcoregionYearClimate[ecoregion.Index][year];      // Climate data year index is 1-based
                }
            }

            //int actualYear = Climate.Future_MonthlyData.Keys.Min() + year - 1;
            //foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            //{
            //    if (ecoregion.Active)
            //    {
            //        //PlugIn.ModelCore.UI.WriteLine("Retrieving {0} for year {1}.", spinupOrfuture.ToString(), actualYear);
            //        if (Climate.Future_MonthlyData.ContainsKey(actualYear))
            //        {
            //            AnnualWeather[ecoregion] = Climate.Future_MonthlyData[actualYear][ecoregion.Index];
            //        }

            //        //PlugIn.ModelCore.UI.WriteLine("Utilizing Climate Data: Simulated Year = {0}, actualClimateYearUsed = {1}.", actualYear, AnnualWeather[ecoregion].Year);
            //    }

            //}
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

using Landis.Core;
using Landis.Library.AgeOnlyCohorts;
using Landis.Library.Biomass;
using Landis.SpatialModeling;

namespace Landis.Library.PnETCohorts
{

    public static class SiteVars
    {
        public static ISiteVar<Pool> WoodyDebris;
        public static ISiteVar<Pool> Litter;
        public static ISiteVar<Double> FineFuels;
        public static ISiteVar<float> PressureHead;
        public static ISiteVar<float> ExtremeMinTemp;
        public static ISiteVar<Landis.Library.PnETCohorts.SiteCohorts> SiteCohorts;

        public static void Initialize()
        {
            WoodyDebris = EcoregionData.ModelCore.Landscape.NewSiteVar<Pool>();
            Litter = EcoregionData.ModelCore.Landscape.NewSiteVar<Pool>();
            FineFuels = EcoregionData.ModelCore.Landscape.NewSiteVar<Double>();
            SiteCohorts = EcoregionData.ModelCore.Landscape.NewSiteVar<SiteCohorts>();
            PressureHead = EcoregionData.ModelCore.Landscape.NewSiteVar<float>();
            ExtremeMinTemp = EcoregionData.ModelCore.Landscape.NewSiteVar<float>();

            EcoregionData.ModelCore.RegisterSiteVar(WoodyDebris, "Succession.WoodyDebris");
            EcoregionData.ModelCore.RegisterSiteVar(Litter, "Succession.Litter");
            EcoregionData.ModelCore.RegisterSiteVar(FineFuels, "Succession.FineFuels");
            EcoregionData.ModelCore.RegisterSiteVar(PressureHead, "Succession.PressureHead");
            EcoregionData.ModelCore.RegisterSiteVar(ExtremeMinTemp, "Succession.ExtremeMinTemp");
        }

    }
}

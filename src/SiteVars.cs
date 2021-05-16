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
            WoodyDebris = Globals.ModelCore.Landscape.NewSiteVar<Pool>();
            Litter = Globals.ModelCore.Landscape.NewSiteVar<Pool>();
            FineFuels = Globals.ModelCore.Landscape.NewSiteVar<Double>();
            SiteCohorts = Globals.ModelCore.Landscape.NewSiteVar<SiteCohorts>();
            PressureHead = Globals.ModelCore.Landscape.NewSiteVar<float>();
            ExtremeMinTemp = Globals.ModelCore.Landscape.NewSiteVar<float>();

            Globals.ModelCore.RegisterSiteVar(WoodyDebris, "Succession.WoodyDebris");
            Globals.ModelCore.RegisterSiteVar(Litter, "Succession.Litter");
            Globals.ModelCore.RegisterSiteVar(FineFuels, "Succession.FineFuels");
            Globals.ModelCore.RegisterSiteVar(PressureHead, "Succession.PressureHead");
            Globals.ModelCore.RegisterSiteVar(ExtremeMinTemp, "Succession.ExtremeMinTemp");
        }

    }
}

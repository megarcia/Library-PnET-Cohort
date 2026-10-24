using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Threading;
using Landis.Core;

namespace Landis.Library.PnETCohorts
{
    public static class Globals
    {
        public static ICore ModelCore;
        public static ushort IMAX { get; private set; }
        public static DateTime StartDate { get; private set; }
        public static readonly object CWDThreadLock = new object();
        public static readonly object litterThreadLock = new object();
        public static readonly object distributionThreadLock = new object();
        public static readonly object ecoregionDataThreadLock = new object();
        public static readonly object initialSitesThreadLock = new object();

        public static void InitializeCore(ICore mCore, ushort IMAX, DateTime startDate)
        {
            StartDate = startDate;
            ModelCore = mCore;
            this.IMAX = IMAX;
        }
    }
}

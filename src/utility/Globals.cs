using Landis.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Landis.Library.PnETCohorts
{
    public static class Globals
    {
        public static ICore ModelCore;
        public static ushort IMAX { get; private set; }
        public static readonly object CWDThreadLock = new object();
        public static readonly object litterThreadLock = new object();
        public static readonly object distributionThreadLock = new object();

        public static void InitializeCore(ICore mCore, ushort IMAX)
        {
            ModelCore = mCore;
            Globals.IMAX = IMAX;
        }
    }
}

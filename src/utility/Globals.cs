using Landis.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Landis.Library.PnETCohorts
{
    public static class Globals
    {
        public static ICore ModelCore;
        public static ushort IMAX { get; private set; }
        public static DateTime StartDate {  get; private set; }
        public static int MinSpinUpClimateYear { get; private set; }
        public static int MaxSpinUpClimateYear { get; private set; }
        public static int MinFutureClimateYear { get; private set; }
        public static int MaxFutureClimateYear { get; private set; }
        public static int MaxSpinUpIndex {  get; private set; }
        public static int MaxFutureClimateIndex {  get; private set; }
        public static readonly object CWDThreadLock = new object();
        public static readonly object leaflitterThreadLock = new object();
        public static readonly object distributionThreadLock = new object();
        public static readonly object ecoregionDataThreadLock = new object();
        public static readonly object initialSitesThreadLock = new object();

        public static void InitializeCore(ICore mCore, ushort IMAX, DateTime startDate)
        {
            Globals.StartDate = startDate;
            ModelCore = mCore;
            Globals.IMAX = IMAX;
        }

        public static void SetMinMaxClimateYears()
        {
            MinSpinUpClimateYear = Climate.Climate.SpinupCalendarYear(1);
            MaxSpinUpClimateYear = Climate.Climate.SpinupEcoregionYearClimate.First(x => x != null).Last(x => x != null).CalendarYear;
            MinFutureClimateYear = Climate.Climate.FutureCalendarYear(1);
            MaxFutureClimateYear = Climate.Climate.FutureEcoregionYearClimate.First(x => x != null).Last(x => x != null).CalendarYear;
        }

        public static bool IsFutureClimate(DateTime date)
        {
            if (date.Year - MinFutureClimateYear + 1 <= 0)
                return false;
            return true;
        }

        public static int ConvertYearToFutureClimateYear(DateTime date)
        {
            int convert = date.Year - MinFutureClimateYear + 1;
            return convert >= 1 ? convert : -1;
        }

        public static int ConvertYearToSpinUpClimateYear(DateTime date)
        {
            int convert = date.Year - MinSpinUpClimateYear + 1;
            return convert >= 1 ? convert : -1;
        }
    }
}

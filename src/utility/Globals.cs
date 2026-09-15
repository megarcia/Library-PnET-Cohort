using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Landis.Core;
using Landis.Climate;

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
        public static readonly object litterThreadLock = new object();
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
            // since the climate may be using random years, we need to look through all the available Spinup and Future Calendar Years to get the min and max years of the input climate data
            MinSpinUpClimateYear = Enumerable.Range(1, Climate.Climate.SpinupAvailableYearCount).Min(x => Climate.Climate.SpinupCalendarYear(x));
            MaxSpinUpClimateYear = Enumerable.Range(1, Climate.Climate.SpinupAvailableYearCount).Max(x => Climate.Climate.SpinupCalendarYear(x));
            MinFutureClimateYear = Enumerable.Range(1, Climate.Climate.FutureAvailableYearCount).Min(x => Climate.Climate.FutureCalendarYear(x));
            MaxFutureClimateYear = Enumerable.Range(1, Climate.Climate.FutureAvailableYearCount).Max(x => Climate.Climate.FutureCalendarYear(x));
        }

        public static bool IsFutureClimate(DateTime date)
        {
            if (date.Year - MinFutureClimateYear + 1 <= 0)
            {
                return false;
            }
            return true;
        }

        public static int ConvertYearToFutureClimateYear(DateTime date)
        {
            int convert = date.Year - MinFutureClimateYear + 1;
            return convert >= 1 ? convert : -1;
        }

        public static int ConvertYearToSpinUpClimateYear(DateTime date)
        {
            // if the requested year is outside the range of [MinSpinUpClimateYear, MinSpinUpClimateYear + AvailableYearCount], then 
            //  adjust the year so that the final index falls in the correct range
            var year = date.Year;
            while (year < MinSpinUpClimateYear || year > MinSpinUpClimateYear + Climate.Climate.SpinupAvailableYearCount)
            {
                if (year < MinSpinUpClimateYear)
                    year += Climate.Climate.SpinupAvailableYearCount;
                if (year > MinSpinUpClimateYear + Climate.Climate.SpinupAvailableYearCount)
                    year -= Climate.Climate.SpinupAvailableYearCount;
            }
            var convert = year - MinSpinUpClimateYear + 1;
            return convert;
        }
    }
}

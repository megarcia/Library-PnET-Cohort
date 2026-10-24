using System;
//using System.Collections.Generic;
//using System.Text;

namespace Landis.Library.PnETCohorts
{
    public static class Statistics
    {
        /// <summary>
        /// Choose random integer between min and max (inclusive)
        /// </summary>
        /// <param name="min">range minimum, integer</param>
        /// <param name="max">range maximum, integer</param>
        /// <returns></returns>
        public static int DiscreteUniformRandom(int min, int max)
        {
            lock (Globals.distributionThreadLock)
            {
                Globals.ModelCore.ContinuousUniformDistribution.Alpha = min;
                Globals.ModelCore.ContinuousUniformDistribution.Beta = max + 1;
                Globals.ModelCore.ContinuousUniformDistribution.NextDouble();
                double valueD = Globals.ModelCore.ContinuousUniformDistribution.NextDouble();
                int value = Math.Min((int)valueD, max);
                return value;
            }
        }

        /// <summary>
        /// Choose random double between min and max (inclusive)
        /// </summary>
        /// <param name="min">range minimum, double, default 0</param>
        /// <param name="max">range maximum, double, default 1</param>
        /// <returns></returns>
        public static double ContinuousUniformRandom(double min = 0, double max = 1)
        {
            lock (Globals.distributionThreadLock)
            {
                Globals.ModelCore.ContinuousUniformDistribution.Alpha = min;
                Globals.ModelCore.ContinuousUniformDistribution.Beta = max;
                Globals.ModelCore.ContinuousUniformDistribution.NextDouble();
                double value = Globals.ModelCore.ContinuousUniformDistribution.NextDouble();
                return value;
            }
        }
    }
}

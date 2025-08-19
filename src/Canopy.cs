using System;

namespace Landis.Library.PnETCohorts
{
    public class Canopy
    {
        public static float CalcLAISum(int index, float[] LAI)
        {
            float LAISum = 0;
            if (LAI != null)
            {
                for (int i = 0; i < index; i++)
                {
                    LAISum += LAI[i];
                }
            }
            return LAISum;
        }

        public static float CalcLAI(IPnETSpecies species, float fol, int index, float LAISum)
        {
            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            // float LAISum = cumulativeLAI;
            // Cohort LAI is capped at 25; once LAI reaches 25, subsequent sublayers get LAI of 0.01
            float LAIlayerMax = (float)Math.Max(0.01, 25.0F - LAISum);
            float LAIlayer = 1 / (float)Globals.IMAX * fol / (species.SLWmax - species.SLWDel * index * (1 / (float)Globals.IMAX) * fol);
            if (fol > 0 && LAIlayer <= 0)
            {
                Globals.ModelCore.UI.WriteLine("\n Warning: LAI was calculated to be negative for " + species.Name + ". This could be caused by a low value for SLWmax.  LAI applied in this case is a max of 25 for each cohort.");
                LAIlayer = LAIlayerMax / (Globals.IMAX - index);
            }
            else
                LAIlayer = (float)Math.Min(LAIlayerMax, LAIlayer);
            return LAIlayer;
        }

        public static float CalcCohortLAI(IPnETSpecies species, float fol)
        {
            float CohortLAI = 0;
            for (int i = 0; i < Globals.IMAX; i++)
                CohortLAI += CalcLAI(species, fol, i, CohortLAI);
            return CohortLAI;
        }
    }
}

using System;

namespace Landis.Library.PnETCohorts
{
    public class Albedo
    {
        //MG20261116 moved from Cohort.cs
        public float AlbedoNoSnow(ISpeciesPnET SpeciesPnET, float SumLAI)
        {
            float albedo = 0.0f;
            if (!string.IsNullOrEmpty(SpeciesPnET.Lifeform))
            {
                if (SpeciesPnET.Lifeform.ToLower().Contains("ground")
                    || SpeciesPnET.Lifeform.ToLower().Contains("open")
                    || SumLAI == 0)
                    albedo = 0.20f;
                else if (SpeciesPnET.Lifeform.ToLower().Contains("dark"))
                    albedo = -0.067f * (float)Math.Log(SumLAI < 0.7 ? 0.7 : SumLAI) + 0.2095f;
                else if (SpeciesPnET.Lifeform.ToLower().Contains("light"))
                    albedo = -0.054f * (float)Math.Log(SumLAI < 0.7 ? 0.7 : SumLAI) + 0.2082f;
                else if (SpeciesPnET.Lifeform.ToLower().Contains("decid"))
                    albedo = -0.0073f * SumLAI + 0.231f;
            }
            if (albedo < 0)
                albedo = 0.0f;
            return albedo;            
        }
    }
}
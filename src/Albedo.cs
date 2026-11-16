using System;

namespace Landis.Library.PnETCohorts
{
    public class Albedo
    {
        //MG20261116 moved from Cohort.cs
        public float CalcAlbedoNoSnow(ISpeciesPnET SpeciesPnET, float SumLAI)
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

        private float CalcAlbedoWithSnow(Cohort cohort, float albedo, float snowDepth)
        {
            // Inactive sites become large negative values on the map and are not considered in the averages
            if (!EcoregionData.GetPnETEcoregion(Globals.ModelCore.Ecoregion[this.Site]).Active)
                return -1;
            float finalAlbedo = 0.0f;
            float snowMultiplier;
            if (snowDepth >= Constants.SnowReflectanceThreshold)
                snowMultiplier = 1.0f;
            else
                snowMultiplier = snowDepth / Constants.SnowReflectanceThreshold;
            if (!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
            {
                if (cohort.SpeciesPnET.Lifeform.ToLower().Contains("ground")
                    || cohort.SpeciesPnET.Lifeform.ToLower().Contains("open")
                    || cohort.SumLAI == 0)
                    finalAlbedo = albedo * (1.0f + 2.75f * snowMultiplier);
                else if (cohort.SpeciesPnET.Lifeform.ToLower().Contains("dark"))
                    finalAlbedo = albedo * (1.0f + 0.8f * snowMultiplier);
                else if (cohort.SpeciesPnET.Lifeform.ToLower().Contains("light"))
                    finalAlbedo = albedo * (1.0f + 0.75f * snowMultiplier);
                else if (cohort.SpeciesPnET.Lifeform.ToLower().Contains("decid"))
                    finalAlbedo = albedo * (1.0f + 0.35f * snowMultiplier);
            }
            return finalAlbedo;
        }
    }
}
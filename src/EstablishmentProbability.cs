using Landis.Core;
using System;
using System.Collections.Generic;

namespace Landis.Library.PnETCohorts
{
    public class ProbEstablishment : IProbEstablishment
    {
        private List<ISpeciesPnET> establishedSpecies;
        private Dictionary<ISpeciesPnET, float> speciesProbEstablishment;
        private Dictionary<ISpeciesPnET, float> speciesFWater;
        private Dictionary<ISpeciesPnET, float> speciesFRad;
        private LocalOutput probEstablishmentSiteOutput;

        public Library.Parameters.Species.AuxParm<float> SpeciesProbEstablishment
        {
            get
            {
                Library.Parameters.Species.AuxParm<float> SpeciesProbEstablishment = new Library.Parameters.Species.AuxParm<float>(Globals.ModelCore.Species);
                foreach (ISpecies species in Globals.ModelCore.Species)
                {
                    ISpeciesPnET speciespnet = SpeciesParameters.SpeciesPnET[species];
                    SpeciesProbEstablishment[species] = speciesProbEstablishment[speciespnet];
                }
                return SpeciesProbEstablishment; // 0.0-1.0 index
            }
        }

        public ProbEstablishment(string SiteOutputName, string FileName)
        {
            Reset();
            if (SiteOutputName != null && FileName != null)
                probEstablishmentSiteOutput = new LocalOutput(SiteOutputName, "Establishment.csv", OutputHeader);
        }

        public float GetSpeciesFWater(ISpeciesPnET species)
        {
            {
                return speciesFWater[species];
            }
        }

        public float GetSpeciesFRad(ISpeciesPnET species)
        {
            {
                return speciesFRad[species];
            }
        }

        public string OutputHeader
        {
            get
            {
                return "Year,Species,ProbEstablishment,FWater_Avg,FRad_Avg,ActiveMonths,IsEstablished";
            }
        }

        public Dictionary<ISpeciesPnET,float> CalcProbEstablishmentForMonth(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology,float minHalfSat, float maxHalfSat, bool invertProbEstablishment, float fracRootAboveFrost)
        {
            Dictionary<ISpeciesPnET, float> speciesProbEstablishment = new Dictionary<ISpeciesPnET, float>();
            float halfSatRange = maxHalfSat - minHalfSat;
            foreach (ISpeciesPnET species in SpeciesParameters.SpeciesPnET.AllSpecies)
            {
                if (pnetvars.Tmin > species.PsnTmin && pnetvars.Tmax < species.PsnTmax && fracRootAboveFrost > 0)
                {
                    // Adjust HalfSat for CO2 effect
                    float halfSatIntercept = species.HalfSat - Constants.CO2RefConc * species.CO2HalfSatEff;
                    float adjHalfSat = species.CO2HalfSatEff * pnetvars.CO2 + halfSatIntercept;
                    float fRad = (float)Math.Min(1.0, Math.Pow(Cohort.CalcFRad(PAR, adjHalfSat), 2) * (1 / Math.Pow(species.EstRad, 2)));
                    float fRad_adj = fRad;
                    // Optional adjustment to invert ProbEstablishment based on relative halfSat
                    if (invertProbEstablishment && halfSatRange > 0)
                    {
                        float fRad_adj_intercept = (species.HalfSat - minHalfSat) / halfSatRange;
                        float fRad_adj_slope = (fRad_adj_intercept * 2) - 1;
                        fRad_adj = 1 - fRad_adj_intercept + fRad * fRad_adj_slope;
                    }
                    float soilWaterPressureHead = hydrology.PressureHeadTable.CalcWaterContent(hydrology.SoilWaterContent, ecoregion.SoilType);
                    float fWater = (float)Math.Min(1.0, Math.Pow(Cohort.CalcFWater(species.H1,species.H2, species.H3, species.H4, soilWaterPressureHead), 2) * (1 / Math.Pow(species.EstMoist, 2)));
                    float probEstablishment = (float) Math.Min(1.0, fRad_adj * fWater);
                    speciesProbEstablishment[species] = probEstablishment;
                    speciesFWater[species] = fWater;
                    speciesFRad[species] = fRad_adj;
                }                
            }
            return speciesProbEstablishment;
        }

        public bool IsEstablishedSpecies(ISpeciesPnET species)
        {
            return establishedSpecies.Contains(species);
        }
       
        public void AddEstablishedSpecies(ISpeciesPnET species)
        {
            establishedSpecies.Add(species);
        }
        
        public void RecordProbEstablishment(int year, ISpeciesPnET species, float annualProbEstablishment, float annualFWater, float annualFRad, bool established, int monthCount)
        {
            if (established)
            {
                if (! IsEstablishedSpecies(species))
                    establishedSpecies.Add(species);
            }
            if (probEstablishmentSiteOutput != null)
            {
                if (monthCount == 0)
                    probEstablishmentSiteOutput.Add(year.ToString() + "," + species.Name + "," + annualProbEstablishment + "," + 0 + "," + 0 + "," + 0 + "," + IsEstablishedSpecies(species));
                else
                    probEstablishmentSiteOutput.Add(year.ToString() + "," + species.Name + "," + annualProbEstablishment + "," + annualFWater + "," + annualFRad + "," + monthCount + "," + IsEstablishedSpecies(species));
                probEstablishmentSiteOutput.Write();
            }
            // Record annualProbEstablishment to be accessed as speciesProbEstablishment
            speciesProbEstablishment[species] = annualProbEstablishment;
        }

        public void Reset()
        {
            speciesProbEstablishment = new Dictionary<ISpeciesPnET, float>();
            speciesFWater = new Dictionary<ISpeciesPnET, float>();
            speciesFRad = new Dictionary<ISpeciesPnET, float>();
            establishedSpecies = new List<ISpeciesPnET>();
            foreach (ISpeciesPnET species in SpeciesParameters.SpeciesPnET.AllSpecies)
            {
                speciesProbEstablishment.Add(species, 0F);
                speciesFWater.Add(species, 0F);
                speciesFRad.Add(species, 0F);
            }
        }
    }
}

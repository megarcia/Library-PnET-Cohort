//MG20261014 TODO move getters and setters to separate file --> "partial class"
//           see https://www.google.com/search?q=csharp+move+getters+to+another+file&sca_esv=a80c92513419f584&biw=1792&bih=904&sxsrf=APpeQnsgP3gkJavBOnl29PxZv6nEzTiezg%3A1791998198402&ei=9rjPatWEF_LXruEPi6uHiAg&uact=5&oq=csharp+move+getters+to+another+file&gs_lp=Egxnd3Mtd2l6LXNlcnAiI2NzaGFycCBtb3ZlIGdldHRlcnMgdG8gYW5vdGhlciBmaWxlMgUQIRigATIFECEYoAEyBRAhGKABMgUQIRigATIFECEYoAEyBRAhGKsCSLt0UJEQWP9qcAN4AZABAJgBgQGgAbgXqgEEMzMuM7gBA8gBAPgBAZgCJ6ACiBnCAgoQABhHGNYEGLADwgIOEAAY5AIY1gQYsAPYAQHCAhcQLhjcBhi4BhjaBhjYAhjIAxiwA9gBAcICBBAjGCfCAgsQABiABBiKBRiRAsICBhAAGBYYHsICBRAAGO8FwgIKEAAYgAQYigUYQ8ICBRAAGIAEwgIHEAAYgAQYCsICChAAGIAEGBQYhwLCAgsQABiABBiKBRiGA8ICCBAAGIkFGKIEwgIFECEYnwWYAwCIBgGQBg26BgYIARABGAmSBwQzMi43oAet2wGyBwQyOS43uAfwGMIHBzAuMTQuMjXIB4UBgAgB&sclient=gws-wiz-serp

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Landis.Core;
using Landis.Library.UniversalCohorts;
using Landis.SpatialModeling;
using Landis.Utilities;

namespace Landis.Library.PnETCohorts
{
    // public class Cohort : Library.UniversalCohorts.Cohort, ICohort  //MG20261021
    public class Cohort : ICohort  //MG20261021
    { 
        public delegate void SubtractTranspiration(float transpiration, ISpeciesPnET Species);
        public ushort index;
        private ISpecies species;
        private ActiveSite site;
        private ISpeciesPnET speciesPnET;
        private CohortData data;
        private bool firstYear;
        private LocalOutput cohortoutput;

        /// <summary>
        /// The site in which the cohort resides  //MG20261021
        /// </summary>
        public ActiveSite Site
        {
            get
            {
                return site;
            }
        }

        /// <summary>
        /// The cohort's data
        /// </summary>
        public CohortData Data
        {
            get
            {
                return data;
            }
        }

        // Age (years)
        // NOTE: draws from and writes to UniversalCohort representation of Cohort
        public ushort Age
        {
            get
            {
                // return data.UniversalData.Age;
                return data.UnivCohort.Data.Age; //MG20261021 trying something weird
            }
            set
            {
                // data.UniversalData.Age = value;
                data.UnivCohort.Data.Age = value;  //MG20261021 trying something weird
            }
        }

        // Aboveground Biomass (g/m2) scaled to the site
        // NOTE: draws from and writes to UniversalCohort representation of Cohort
        public int Biomass
        {
            get
            {
                // return data.UniversalData.Biomass;
                return data.UnivCohort.Data.Biomass; //MG20261021 trying something weird
            }
            set
            {
                // data.UniversalData.Biomass = value;
                data.UnivCohort.Data.Biomass = value;  //MG20261021 trying something weird
            }
        }

        //MG20261016 new variable to track biomass changes by timestep
        public int LastBiomassDelta
        {
            get
            {
                return data.LastBiomassDelta;
            }
            set
            {
                data.LastBiomassDelta = value;
            }
        }

        // Annual net primary productivity
        // NOTE: draws from and writes to UniversalCohort representation of Cohort
        public double ANPP
        {
            get
            {
                // return data.UniversalData.ANPP;
                return data.UnivCohort.Data.ANPP; //MG20261021 trying something weird
            }
            set
            {
                // data.UniversalData.ANPP = value;
                data.UnivCohort.Data.ANPP = value;  //MG20261021 trying something weird
            }
        }

        //MG20261016 new variable to track ANPP changes by timestep
        public double LastANPPDelta
        {
            get
            {
                return data.LastANPPDelta;
            }
            set
            {
                data.LastANPPDelta = value;
            }
        }

        // Non soluble carbons
        public float NSC
        {
            get
            {
                return data.NSC;
            }
            set
            {
                data.NSC = value;
            }
        }

        // Maximum Foliage Value For Current Year
        public float MaxFolYear
        {
            get
            {
                return data.MaxFolYear;
            }
            set
            {
                data.MaxFolYear = value;
            }
        }

        // Measure of cohort's diffuse reflection of solar radiation 
        // as a fraction of total solar radiation, without snow reflectance.
        public float Albedo
        {
            get
            {
                //MG20261014 TODO move this to an Albedo class, along with the
                //           counterpart calculation when snow is present (see
                //           SiteCohorts.CalculateAlbedoWithSnow)
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

        // Foliage biomass (g/m2)
        public float Fol
        {
            get
            {
                return data.Fol;
            }
            set
            {
                data.Fol = value;
            }
        }

        // Species Moss Depth (m)
        public float MossDepth
        {
            get
            {
                return data.UniversalData.Biomass * speciesPnET.MossScalar;
            }
        }

        // Aboveground Biomass (g/m2)
        public int AGBiomass
        {
            get
            {
                return (int)(Math.Round((1 - speciesPnET.FracBelowG) * data.TotalBiomass) + data.Fol);
            }
        }

        // Total Biomass (root + wood) (g/m2)
        public int TotalBiomass
        {
            get
            {
                return (int)Math.Round(data.TotalBiomass);
            }
        }

        // Wood biomass (g/m2)
        public uint Wood
        {
            get
            {
                return (uint)Math.Round((1 - speciesPnET.FracBelowG) * data.TotalBiomass);
            }
        }

        // Root biomass (g/m2)
        public uint Root
        {
            get
            {
                return (uint)Math.Round(speciesPnET.FracBelowG * data.TotalBiomass);
            }
        }

        // Max biomass achived in the cohort's life time. 
        // This value remains high after the cohort has reached its 
        // peak biomass. It is used to determine canopy layers and
        // prevents a cohort from descending in the canopy as it 
        // declines (g/m2)
        public float BiomassMax
        {
            get
            {
                return data.BiomassMax;
            }
        }

        /// <summary>
        /// Boolean whether cohort has been killed by cold temp relative to cold tolerance
        /// </summary>
        public int ColdKill
        {
            get
            {
                return data.ColdKill;
            }
        }

        // Add dead wood to last senescence
        public void AccumulateWoodySenescence(int senescence)
        {
            data.LastWoodySenescence += senescence;
        }

        // Add dead foliage to last senescence
        public void AccumulateFoliageSenescence(int senescence)
        {
            data.LastFoliageSenescence += senescence;
        }

        // Growth reduction factor for age
        float FAge
        {
            get
            {
                return Math.Max(0.0f, 1.0f - (float)Math.Pow(Age / (float)speciesPnET.Longevity, speciesPnET.PsnAgeRed));
            }
        }

        // NSC fraction: measure for resources
        public float NSCfrac
        {
            get
            {
                return NSC / (FActiveBiom * (data.TotalBiomass + Fol) * SpeciesPnET.CFracBiomass);
            }
        }

        // Species with PnET parameter additions
        public ISpeciesPnET SpeciesPnET
        {
            get
            {
                return speciesPnET;
            }
        }

        // LANDIS species (without PnET parameter additions)
        public ISpecies Species
        {
            get
            {
                int sppIndex = species.Index;
                return (ISpecies)Globals.ModelCore.Species[sppIndex];
            }
        }

        // Defoliation proportion - BRM
        public float DeFolProp
        {
            get
            {
                return data.DeFolProp;
            }
            private set
            {
                data.DeFolProp = value;
            }
        }

        // Annual Woody Senescence (g/m2)
        public float LastWoodySenescence
        {
            get
            {
                return data.LastWoodySenescence;
            }
            set
            {
                data.LastWoodySenescence = value;
            }
        }

        // Annual Foliage Senescence (g/m2)
        public float LastFoliageSenescence
        {
            get
            {
                return data.LastFoliageSenescence;
            }
            set
            {
                data.LastFoliageSenescence = value;
            }
        }

        // Last average FRad
        public float LastFRad
        {
            get
            {
                return data.LastFRad;
            }
        }

        public float adjFolN
        {
            get
            {
                return data.adjFolN;
            }
        }

        public float[] AdjFolN
        {
            get
            {
                return data.AdjFolN;
            }
        }

        public float adjFracFol
        {
            get
            {
                return data.adjFracFol;
            }
        }

        public float[] AdjFracFol
        {
            get
            {
                return data.AdjFracFol;
            }
        }

        public float AdjHalfSat
        {
            get
            {
                return data.AdjHalfSat;
            }
        }

        public float[] CiModifier
        {
            get
            {
                return data.CiModifier;
            }
        }

        public float[] DelAmax
        {
            get
            {
                return data.DelAmax;
            }
        }

        public float[] FolResp
        {
            get
            {
                return data.FolResp;
            }
        }

        public float[] FOzone
        {
            get
            {
                return data.FOzone;
            }
        }

        public float[] FRad
        {
            get
            {
                return data.FRad;
            }
        }

        public float[] FWater
        {
            get
            {
                return data.FWater;
            }
        }

        public float[] GrossPsn
        {
            get
            {
                return data.GrossPsn;
            }
        }

        public float[] Interception
        {
            get
            {
                return data.Interception;
            }
        }

        public float[] LAI
        {
            get
            {
                return data.LAI;
            }
        }        

        public float LastLAI
        {
            get
            {
                return data.LastLAI;
            }
            set
            {
                data.LastLAI = value;
            }
        }

        public float LastAGBio
        {
            get
            {
                return data.LastAGBio;
            }
            set
            {
                data.LastAGBio = value;
            }
        }

        public List<float> LastSeasonFRad
        {
            get
            {
                return data.LastSeasonFRad;
            }
        }

        public byte Layer
        {
            get
            {
                return data.Layer;
            }
            set
            {
                data.Layer = value;
            }
        }

        public bool Leaf_On
        {
            get
            {
                return data.Leaf_On;
            }
        }

        public float[] MaintenanceRespiration
        {
            get
            {
                return data.MaintenanceRespiration;
            }
        }

        public float[] NetPsn
        {
            get
            {
                return data.NetPsn;
            }
        }

        public float[] PressHead
        {
            get
            {
                return data.PressHead;
            }
        }

        public float[] Transpiration
        {
            get
            {
                return data.Transpiration;
            }
        }

        public float[] PotentialTranspiration
        {
            get
            {
                return data.PotentialTranspiration;
            }
        }

        public float[] Water
        {
            get
            {
                return data.Water;
            }
        }

        public int[] NumEvents
        {
            get
            {
                return data.NumEvents;
            }
        }

        public float FActiveBiom
        {
            get
            {
                return (float)Math.Exp(-speciesPnET.FrActWd * data.BiomassMax);
            }
        }

        // Determine if cohort is alive. It is assumed that a cohort is dead when 
        // NSC decline below 1% of biomass
        public bool IsAlive
        {
            get
            {
                return NSCfrac > 0.01f;
            }
        }

        public float SumLAI
        {
            get
            {
                if (data.LAI == null)
                    return 0;
                return data.LAI.Sum();
            }
        }

        public float BiomassLayerProp
        {
            get
            {
                return data.BiomassLayerProp;
            }
            set
            {
                data.BiomassLayerProp = value;
            }
        }

        public float CanopyLayerProp
        {
            get
            {
                return data.CanopyLayerProp;
            }
            set
            {
                data.CanopyLayerProp = value;
            }
        }

        public float CanopyGrowingSpace
        {
            get
            {
                return data.CanopyGrowingSpace;
            }
            set
            {
                data.CanopyGrowingSpace = value;
            }
        }

        // List of DisturbanceTypes that have had ReduceDeadPools applied
        public List<ExtensionType> ReducedTypes = null;

        // Index of growing season month
        public int growMonth = -1;

        public void InitializeSubLayers()
        {
            // Initialize subcanopy layers
            index = 0;
            data.LAI = new float[Globals.IMAX];
            data.GrossPsn = new float[Globals.IMAX];
            data.FolResp = new float[Globals.IMAX];
            data.NetPsn = new float[Globals.IMAX];
            data.Transpiration = new float[Globals.IMAX];
            data.PotentialTranspiration = new float[Globals.IMAX];
            data.FRad = new float[Globals.IMAX];
            data.FWater = new float[Globals.IMAX];
            data.Water = new float[Globals.IMAX];
            data.PressHead = new float[Globals.IMAX];
            data.NumEvents = new int[Globals.IMAX];
            data.FOzone = new float[Globals.IMAX];
            data.MaintenanceRespiration = new float[Globals.IMAX];
            data.Interception = new float[Globals.IMAX];
            data.AdjFolN = new float[Globals.IMAX];
            data.AdjFracFol = new float[Globals.IMAX];
            data.CiModifier = new float[Globals.IMAX];
            data.DelAmax = new float[Globals.IMAX];
        }

        public void StoreFRad()
        {
            // Filter for growing season months only
            if (data.Leaf_On)
            {
                data.LastFRad = data.FRad.Average();
                data.LastSeasonFRad.Add(LastFRad);
            }
        }

        public void SetAvgFRad(float lastAvgFrad)
        {
            data.LastSeasonFRad.Add(lastAvgFrad);
        }

        public void ClearFRad()
        {
            data.LastSeasonFRad = new List<float>();
        }

        public void CalcAdjFracFol()
        {
            if (data.LastSeasonFRad.Count() > 0)
            {
                float lastSeasonAvgFRad = data.LastSeasonFRad.ToArray().Average();
                float fracFol_slope = speciesPnET.FracFolShape;
                float fracFol_int = speciesPnET.MaxFracFol;
                //modified exponential version - controls lower and upper limit of function
                //slope is shape parm; fracFol is minFracFol; intercept is maxFracFol. EJG-7-24-18
                data.adjFracFol = speciesPnET.FracFol + ((fracFol_int - speciesPnET.FracFol) * (float)Math.Pow(lastSeasonAvgFRad, fracFol_slope));
                firstYear = false;
            }
            else
                data.adjFracFol = speciesPnET.FracFol;
        }

        public void NullSubLayers()
        {
            // Reset values for subcanopy layers
            data.LAI = null;
            data.GrossPsn = null;
            data.FolResp = null;
            data.NetPsn = null;
            data.Transpiration = null;
            data.PotentialTranspiration = null;
            data.FRad = null;
            data.FWater = null;
            data.PressHead = null;
            data.NumEvents = null;
            data.Water = null;
            data.FOzone = null;
            data.MaintenanceRespiration = null;
            data.Interception = null;
            data.AdjFolN = null;
            data.AdjFracFol = null;
            data.CiModifier = null;
            data.DelAmax = null;
        }

        // Get totals for combined cohorts
        public void Accumulate(Cohort cohort)
        {
            data.TotalBiomass += cohort.TotalBiomass;
            data.BiomassMax = Math.Max(BiomassMax, data.TotalBiomass);
            data.Fol += cohort.Fol;
            data.MaxFolYear = Math.Max(MaxFolYear, data.Fol);
            data.AGBiomass = (1 - cohort.SpeciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;
            // data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerProp);  //MG20261021
            data.UnivCohort.Data.Biomass = (int)(data.AGBiomass * data.CanopyLayerProp);  //MG20261021
            // data.UniversalData.ANPP += cohort.ANPP;  //MG20261021
            data.UnivCohort.Data.ANPP += cohort.ANPP;  //MG20261021
        }

        /// <summary>
        /// Increments the cohort's age by one year.
        /// </summary>
        public void IncrementAge()
        {
            data.UniversalData.Age += 1;
        }

        /// <summary>
        /// Changes the cohort's Biomass.
        /// </summary>
        public void ChangeBiomass(int delta)
        {
            float newTotalBiomass = data.TotalBiomass + delta;
            data.TotalBiomass = Math.Max(0.0f, newTotalBiomass);
            data.BiomassMax = Math.Max(data.BiomassMax, data.TotalBiomass);
            data.AGBiomass = (1 - SpeciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;
            // data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerProp);  //MG20261021
            data.UnivCohort.Data.Biomass = (int)(data.AGBiomass * data.CanopyLayerProp);  //MG20261021
        }

        /// <summary>
        /// Changes the cohort's ANPP.
        /// </summary>
        public void ChangeANPP(double delta)
        {            
            // data.UniversalData.ANPP += delta;  //MG20261021
            data.UnivCohort.Data.ANPP += delta;  //MG20261021
        }

        //MG20261021 trying something weird to get PnET to communicate properly with UCL;
        //           added ActiveSite site argument in order to spawn a corresponding 
        //           UniversalCohort whenever a new PnET Cohort is instantiated.
        public Cohort(ActiveSite site, ISpecies species, ISpeciesPnET speciesPnET, ushort cohort_start_year, string SiteName, double propBiomass, bool cohortStacking)
        {
            this.site = site;  //MG20261021
            this.species = species;
            this.speciesPnET = speciesPnET;
            data.ColdKill = int.MaxValue;
            data.NSC = (ushort)speciesPnET.InitialNSC;

            // Initialize biomass assuming fixed concentration of NSC, convert gC to gDW
            data.TotalBiomass = (uint)Math.Max(1.0f, NSC / (speciesPnET.DNSC * speciesPnET.CFracBiomass) * propBiomass) ;
            data.AGBiomass = (1 - speciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;
            data.BiomassMax = data.TotalBiomass;

            float cohortLAI = 0;
            float cohortIdealFol = speciesPnET.FracFol * FActiveBiom * data.TotalBiomass;
            for (int i = 0; i < Globals.IMAX; i++)
                cohortLAI += CalculateLAI(SpeciesPnET, cohortIdealFol, i, cohortLAI);
            data.LastLAI = cohortLAI;
            data.LastAGBio = data.AGBiomass;
            data.CanopyLayerProp = data.LastLAI / speciesPnET.MaxLAI;
            if (cohortStacking)
                data.CanopyLayerProp = 1.0f;
            data.CanopyGrowingSpace = 1.0f;
            data.LastSeasonFRad = new List<float>();
            firstYear = true;

            //MG20261021 begin changes to spawn new UniversalCohort within PnET Cohort
            //           note that this process needs the site variable now added to invocation
            int _age = 1;
            int _biomass = (int)(data.AGBiomass * data.CanopyLayerProp);
            double _anpp = _biomass;
            ExpandoObject AddlParams = new ExpandoObject();
            data.UnivCohort = SiteVars.UnivCohorts[site].AddNewCohort(species, _age, _biomass, _anpp, AddlParams);  
            // data.UniversalData.Age = 1;
            // data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerProp);
            // data.UniversalData.ANPP = data.UniversalData.Biomass;
            //MG20261021 end changes -- I told you it was weird...

            data.LastBiomassDelta = Biomass;  //MG20261016 new variable
            data.LastANPPDelta = Biomass;  //MG20261016 new variable

            if (SiteName != null)
                InitializeOutput(SiteName, cohort_start_year);
        }

        //MG20261021 trying something weird to get PnET to communicate properly with UCL;
        //           added ActiveSite site argument since that information is not in cohortData
        public Cohort(ActiveSite site, ISpecies species, CohortData cohortData)
        {
            this.site = site;  //MG20261021
            this.species = species;
            speciesPnET = SpeciesParameters.SpeciesPnET.AllSpecies[species.Index];
            data = cohortData;  //MG20261021 I think since the cohortData already exists,
                                //           already has a UniversalCohort in it, so no
                                //           need to spawn a new one... right?
        }

        public Cohort(Cohort cohort)
        {
            site = cohort.Site;  //MG20261021
            species = cohort.Species;
            speciesPnET = cohort.speciesPnET;
            data.NSC = cohort.NSC;
            data.TotalBiomass = cohort.TotalBiomass;
            data.AGBiomass = (1 - cohort.SpeciesPnET.FracBelowG) * cohort.TotalBiomass + cohort.Fol;
            data.BiomassMax = cohort.BiomassMax;
            data.Fol = cohort.Fol;
            data.MaxFolYear = cohort.MaxFolYear;
            data.LastSeasonFRad = cohort.data.LastSeasonFRad;
            data.ColdKill = int.MaxValue;

            //MG20261021 begin changes to spawn new UniversalCohort within PnET Cohort
            //           note that this process needs the site variable now added to invocation
            int _age = cohort.Age;
            int _biomass = (int)(data.AGBiomass * cohort.CanopyLayerProp);
            double _anpp = cohort.ANPP;
            ExpandoObject AddlParams = new ExpandoObject();
            data.UnivCohort = SiteVars.UnivCohorts[site].AddNewCohort(species, _age, _biomass, _anpp, AddlParams);  
            // data.UniversalData.Age = cohort.Age;
            // data.UniversalData.Biomass = (int)(data.AGBiomass * cohort.CanopyLayerProp);
            // data.UniversalData.ANPP = cohort.ANPP;
            //MG20261021 end changes -- I told you it was weird...

            data.LastBiomassDelta = cohort.LastBiomassDelta;  //MG20261016 new variable
            data.LastANPPDelta = cohort.LastANPPDelta;  //MG20261016 new variable
        }

        public Cohort(Cohort cohort, ushort FirstYear, string SiteName)
        {
            site = cohort.Site;  //MG20261021
            species = cohort.Species;
            speciesPnET = cohort.speciesPnET;
            data.NSC = cohort.NSC;
            data.TotalBiomass = cohort.TotalBiomass;
            data.AGBiomass = (1 - cohort.SpeciesPnET.FracBelowG) * cohort.TotalBiomass + cohort.Fol;
            data.BiomassMax = cohort.BiomassMax;
            data.Fol = cohort.Fol;
            data.MaxFolYear = cohort.MaxFolYear;
            data.LastSeasonFRad = cohort.data.LastSeasonFRad;
            data.ColdKill = int.MaxValue;
            firstYear = FirstYear;  //MG20261021 was ignored? otherwise unused...

            //MG20261021 begin changes to spawn new UniversalCohort within PnET Cohort
            //           note that this process needs the site variable now added to invocation
            int _age = cohort.Age;
            int _biomass = (int)(data.AGBiomass * cohort.CanopyLayerProp);
            double _anpp = cohort.ANPP;
            ExpandoObject AddlParams = new ExpandoObject();
            data.UnivCohort = SiteVars.UnivCohorts[site].AddNewCohort(species, _age, _biomass, _anpp, AddlParams);  
            // data.UniversalData.Age = cohort.Age;
            // data.UniversalData.Biomass = (int)(data.AGBiomass * cohort.CanopyLayerProp);
            // data.UniversalData.ANPP = cohort.ANPP;
            //MG20261021 end changes -- I told you it was weird...

            data.LastBiomassDelta = cohort.LastBiomassDelta;  //MG20261016 new variable
            data.LastANPPDelta = cohort.LastANPPDelta;  //MG20261016 new variable

            if (SiteName != null)
                InitializeOutput(SiteName, firstYear);
        }

        //MG20261021 trying something weird to get PnET to communicate properly with UCL;
        //           added ActiveSite site argument in order to spawn a corresponding 
        //           UniversalCohort whenever a new PnET Cohort is instantiated.
        public Cohort(ActiveSite site, ISpeciesPnET speciesPnET, ushort age, int woodBiomass, string SiteName, ushort FirstYear, bool cohortStacking)
        {
            InitializeSubLayers();
            this.site = site;  //MG20261021
            species = (ISpecies)speciesPnET;
            this.speciesPnET = speciesPnET;
            float biomass = woodBiomass / (1 - speciesPnET.FracBelowG);
            data.TotalBiomass = biomass;
            data.BiomassMax = biomass;
            data.LastSeasonFRad = new List<float>();
            data.adjFracFol = speciesPnET.MaxFracFol;
            data.ColdKill = int.MaxValue;
            float cohortLAI = 0;
            float cohortIdealFol = speciesPnET.MaxFracFol * FActiveBiom * data.TotalBiomass;
            for (int i = 0; i < Globals.IMAX; i++)
            {
                float subLayerLAI = CalculateLAI(SpeciesPnET, cohortIdealFol, i);
                cohortLAI += subLayerLAI;
                if (Leaf_On)
                    LAI[index] = subLayerLAI;
            }
            if (Leaf_On)
            {
                data.Fol = cohortIdealFol;
                data.MaxFolYear = cohortIdealFol;
            }
            data.LastLAI = cohortLAI;
            data.CanopyLayerProp = data.LastLAI / speciesPnET.MaxLAI;
            if (cohortStacking)
                data.CanopyLayerProp = 1.0f;
            data.CanopyGrowingSpace = 1.0f;
            data.AGBiomass = (1 - this.speciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;
            data.LastAGBio = data.AGBiomass;
            data.NSC = this.speciesPnET.DNSC * FActiveBiom * (data.TotalBiomass + data.Fol) * speciesPnET.CFracBiomass;
            firstYear = FirstYear;  //MG20261021 was ignored? otherwise unused...

            //MG20261021 begin changes to spawn new UniversalCohort within PnET Cohort
            //           note that this process needs the site variable now added to invocation
            int _age = age;
            int _biomass = (int)(data.AGBiomass * data.CanopyLayerProp);
            double _anpp = 0.0;
            ExpandoObject AddlParams = new ExpandoObject();
            data.UnivCohort = SiteVars.UnivCohorts[site].AddNewCohort(species, _age, _biomass, _anpp, AddlParams);  
            // data.UniversalData.Age = age;
            // data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerProp);
            // data.UniversalData.ANPP = 0.0;
            //MG20261021 end changes -- I told you it was weird...

            data.LastBiomassDelta = 0;  //MG20261016 new variable
            data.LastANPPDelta = 0.0;  //MG20261016 new variable

            if (SiteName != null)
                InitializeOutput(SiteName, firstYear);
        }

        //MG20261021 trying something weird to get PnET to communicate properly with UCL;
        //           added ActiveSite site argument in order to spawn a corresponding 
        //           UniversalCohort whenever a new PnET Cohort is instantiated.
        public Cohort(ActiveSite site, ISpeciesPnET speciesPnET, ushort age, int woodBiomass, int maxBiomass, float canopyGrowingSpace, string SiteName, ushort FirstYear, bool cohortStacking, float lastSeasonAvgFrad)
        {
            InitializeSubLayers();
            this.site = site;  //MG20261021
            species = (ISpecies)speciesPnET;
            this.speciesPnET = speciesPnET;
            float biomass = woodBiomass / (1 - speciesPnET.FracBelowG);
            data.TotalBiomass = biomass;
            data.BiomassMax = Math.Max(biomass,maxBiomass);
            data.LastSeasonFRad = new List<float>();
            data.LastSeasonFRad.Add(lastSeasonAvgFrad);
            CalcAdjFracFol();
            data.ColdKill = int.MaxValue;
            float cohortLAI = 0;
            float cohortIdealFol = adjFracFol * FActiveBiom * data.TotalBiomass;
            for (int i = 0; i < Globals.IMAX; i++)
            {
                float subLayerLAI = CalculateLAI(SpeciesPnET, cohortIdealFol, i);
                cohortLAI += subLayerLAI;
                if (Leaf_On)
                    LAI[index] = subLayerLAI;
            }
            if (Leaf_On)
            {
                data.Fol = cohortIdealFol;
                data.MaxFolYear = cohortIdealFol;
            }
            data.LastLAI = cohortLAI;
            data.CanopyLayerProp = data.LastLAI / speciesPnET.MaxLAI;
            if (cohortStacking)
                data.CanopyLayerProp = 1.0f;
            data.CanopyGrowingSpace = 1.0f;
            data.AGBiomass = (1 - this.speciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;
            data.LastAGBio = data.AGBiomass;
            data.NSC = this.speciesPnET.DNSC * FActiveBiom * (data.TotalBiomass + data.Fol) * speciesPnET.CFracBiomass;
            firstYear = FirstYear;  //MG20261021 was ignored? otherwise unused...

            //MG20261021 begin changes to spawn new UniversalCohort within PnET Cohort
            //           note that this process needs the site variable now added to invocation
            int _age = age;
            int _biomass = (int)(data.AGBiomass * data.CanopyLayerProp);
            double _anpp = 0.0;
            ExpandoObject AddlParams = new ExpandoObject();
            data.UnivCohort = SiteVars.UnivCohorts[site].AddNewCohort(species, _age, _biomass, _anpp, AddlParams);  
            // data.UniversalData.Age = age;
            // data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerProp);
            // data.UniversalData.ANPP = 0.0;
            //MG20261021 end changes -- I told you it was weird...

            data.LastBiomassDelta = 0;  //MG20261016 new variable
            data.LastANPPDelta = 0.0;  //MG20261016 new variable

            if (SiteName != null)
                InitializeOutput(SiteName, firstYear);
        }

        public void CalculateDefoliation(ActiveSite site, int SiteAboveGroundBiomass)
        {
            lock (Globals.distributionThreadLock)
            {
                data.DeFolProp = (float)UniversalCohorts.CohortDefoliation.Compute(site, this, 0, SiteAboveGroundBiomass);
            }
        }

        //MG20260910 TODO this calculation will be moving to a new Photosynthesis
        //           class and will be adjusted to rely on the Hydrology class 
        //           for water calculations
        // Photosynthesis by canopy layer
        public bool CalculatePhotosynthesis(float PrecInByCanopyLayer,int precipCount, float leakageFrac, ref Hydrology hydrology, float mainLayerPAR, ref float SubCanopyPar, float o3_cum, float o3_month, int subCanopyIndex, int layerCount, ref float O3Effect, float frostFreeProp, float MeltInByCanopyLayer, bool coldKillBoolean, IEcoregionPnETVariables variables, SiteCohorts siteCohort, float sumCanopyProp,float groundPETbyEvent, bool allowMortality = true)
        {      
            bool success = true;
            float lastO3Effect = O3Effect;
            O3Effect = 0;

            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            data.LAI[index] = CalculateLAI(speciesPnET, data.Fol, index);

            if (MeltInByCanopyLayer > 0)
            {
                // Add melted snow water to soil moisture
                // Instantaneous runoff (excess of porosity + RunoffCapture)
                float waterCapacity = siteCohort.Ecoregion.Porosity * siteCohort.Ecoregion.RootingDepth * frostFreeProp; //mm
                float meltrunoff = Math.Min(MeltInByCanopyLayer, Math.Max(hydrology.Water * siteCohort.Ecoregion.RootingDepth * frostFreeProp + MeltInByCanopyLayer - waterCapacity, 0));
                float capturedRunoff = 0;
                if ((siteCohort.Ecoregion.RunoffCapture > 0) & (meltrunoff >0))
                {
                    capturedRunoff = Math.Max(0, Math.Min(meltrunoff, siteCohort.Ecoregion.RunoffCapture - hydrology.SurfaceWater));
                    hydrology.SurfaceWater += capturedRunoff;
                }
                hydrology.Runoff += meltrunoff - capturedRunoff;

                success = hydrology.AddWater(MeltInByCanopyLayer - meltrunoff, siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                if (!success)
                    throw new Exception("Error adding water, MeltInByCanopyLayer = " + MeltInByCanopyLayer + "; water = " + hydrology.Water + "; meltrunoff = " + meltrunoff + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
            }
            float precipIn = 0;
            if (PrecInByCanopyLayer > 0)
            {
                // If more than one precip event assigned to layer,
                // repeat precip, runoff, leakage for all events prior 
                // to respiration
                for (int p = 1; p <= precipCount; p++)
                {
                    // Incoming precipitation
                    // Instantaneous runoff (excess of porosity)
                    float waterCapacity = siteCohort.Ecoregion.Porosity * siteCohort.Ecoregion.RootingDepth * frostFreeProp; //mm
                    float rainrunoff = Math.Min(PrecInByCanopyLayer, Math.Max(hydrology.Water * siteCohort.Ecoregion.RootingDepth * frostFreeProp + PrecInByCanopyLayer - waterCapacity, 0.0f));
                    float capturedRunoff = 0.0f;
                    if ((siteCohort.Ecoregion.RunoffCapture > 0) & (rainrunoff > 0))
                    {
                        capturedRunoff = Math.Max(0.0f, Math.Min(rainrunoff, siteCohort.Ecoregion.RunoffCapture - hydrology.SurfaceWater));
                        hydrology.SurfaceWater += capturedRunoff;
                    }
                    hydrology.Runoff += rainrunoff - capturedRunoff;

                    precipIn = PrecInByCanopyLayer - rainrunoff; //mm

                    // Add incoming precipitation to soil moisture
                    success = hydrology.AddWater(precipIn, siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                    if (!success)
                        throw new Exception("Error adding water, waterIn = " + precipIn + "; water = " + hydrology.Water + "; rainrunoff = " + rainrunoff + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);

                    float leakage = Math.Max(leakageFrac * (hydrology.Water - siteCohort.Ecoregion.FieldCap), 0.0f) * siteCohort.Ecoregion.RootingDepth * frostFreeProp; //mm
                    hydrology.Leakage += leakage;

                    // Remove fast leakage
                    success = hydrology.AddWater(-1 * leakage, siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                    if (!success)
                        throw new Exception("Error adding water, Hydrology.Leakage = " + hydrology.Leakage + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);

                    // Add surface water to soil
                    if (hydrology.SurfaceWater > 0)
                    {
                        float surfaceInput = Math.Min(hydrology.SurfaceWater, (siteCohort.Ecoregion.Porosity - hydrology.Water) * siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                        hydrology.SurfaceWater -= surfaceInput;
                        success = hydrology.AddWater(surfaceInput, siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                        if (!success)
                            throw new Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);

                    }
                }
            }
            else
            {
                // Leakage only occurs following precipitation events or incoming melt water
                if (MeltInByCanopyLayer > 0)
                {
                    float leakage = Math.Max(leakageFrac * (hydrology.Water - siteCohort.Ecoregion.FieldCap), 0.0f) * siteCohort.Ecoregion.RootingDepth * frostFreeProp; //mm
                    hydrology.Leakage += leakage;
                    // Remove fast leakage
                    success = hydrology.AddWater(-1 * leakage, siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                    if (!success)
                        throw new Exception("Error adding water, Hydrology.Leakage = " + hydrology.Leakage + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);

                    // Add surface water to soil
                    if (hydrology.SurfaceWater > 0)
                    {
                        float surfaceInput = Math.Min(hydrology.SurfaceWater, (siteCohort.Ecoregion.Porosity - hydrology.Water) * siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                        hydrology.SurfaceWater -= surfaceInput;
                        success = hydrology.AddWater(surfaceInput, siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                        if (!success)
                            throw new Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
                    }
                }
            }
                
            //MG20261014 TODO move this calculation to new Respiration class
            // Maintenance respiration depends on biomass,  non soluble carbon and temperature
            data.MaintenanceRespiration[index] = 1 / (float)Globals.IMAX * (float)Math.Min(NSC, variables[Species.Name].MaintRespFTempResp * data.TotalBiomass * speciesPnET.CFracBiomass); //gC //IMAXinverse
            // Subtract mainenance respiration (gC/mo)
            data.NSC -= MaintenanceRespiration[index];
            if (data.NSC < 0)
                data.NSC = 0f;

            // Woody decomposition: do once per year to reduce unnescessary computation time so with the last subcanopy layer 
            if (index == Globals.IMAX - 1)
            {
                // In the last month
                if (variables.Month == (int)Constants.Months.December)
                {
                    if (allowMortality)
                    {
                        // Check if nscfrac is below threshold to determine if cohort is alive
                        if (!IsAlive)
                            data.NSC = 0.0f;  // if cohort is dead, nsc goes to zero and becomes functionally dead even though not removed until end of timestep
                        else if (Globals.ModelCore.CurrentTime > 0 && TotalBiomass < (uint)speciesPnET.InitBiomass)  // Check if biomass < Initial Biomass -> cohort dies
                        {
                            data.NSC = 0.0f;  // if cohort is dead, nsc goes to zero and becomes functionally dead even though not removed until end of timestep
                            data.Leaf_On = false;
                            data.NSC = 0.0f;
                            float foliageSenescence = FoliageSenescence();
                            data.LastFoliageSenescence = foliageSenescence;
                            siteCohort.AddLitter(foliageSenescence * data.CanopyLayerProp, SpeciesPnET); // Using Canopy proportioning
                        }
                    }
                    float woodSenescence = Senescence();
                    data.LastWoodySenescence = woodSenescence;
                    siteCohort.AddWoodyDebris(woodSenescence * data.CanopyLayerProp, speciesPnET.KWdLit); // Using Canopy proportioning
                    
                    // Release of NSC, will be added to biomass components next year
                    // Assumed that NSC will have a minimum concentration, excess is allocated to biomass
                    float Allocation = Math.Max(NSC - (speciesPnET.DNSC * FActiveBiom * data.TotalBiomass * speciesPnET.CFracBiomass), 0.0f);
                    data.TotalBiomass += Allocation / speciesPnET.CFracBiomass;  // convert gC to gDW
                    data.AGBiomass = (1 - speciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;
                    data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerProp);
                    data.BiomassMax = Math.Max(BiomassMax, data.TotalBiomass);
                    data.NSC -= Allocation;
                    if (data.NSC < 0)
                        data.NSC = 0f;
                    data.UniversalData.Age++;
                }
            }
            // Phenology: do once per cohort per month, using the first sublayer 
            if (index == 0)
            {
                if (coldKillBoolean)
                {
                    data.ColdKill = (int)Math.Floor(variables.Tave - (3.0f * siteCohort.Ecoregion.WinterSTD));
                    data.Leaf_On = false;
                    data.NSC = 0.0f;
                    float foliageSenescence = FoliageSenescence();
                    data.LastFoliageSenescence = foliageSenescence;
                    siteCohort.AddLitter(foliageSenescence * data.CanopyLayerProp, SpeciesPnET); // Using Canopy proportioning
                }
                else
                {
                    // When LeafOn becomes false for the first time in the year
                    if (variables.Tmin <= SpeciesPnET.LeafOnMinT)
                    {
                        if (data.Leaf_On == true)
                        {
                            data.Leaf_On = false;
                            float foliageSenescence = FoliageSenescence();
                            data.LastFoliageSenescence = foliageSenescence;
                            siteCohort.AddLitter(foliageSenescence * data.CanopyLayerProp, SpeciesPnET); // Using Canopy proportioning
                        }
                        growMonth = -1;
                    }
                    else
                    {
                        if (frostFreeProp > 0)
                        {
                            if (data.Leaf_On == false) // LeafOn becomes true for the first time in a year
                                growMonth = 1;
                            else
                                growMonth += 1;
                            data.Leaf_On = true;
                        }
                    }
                }
                if (data.Leaf_On)
                {
                    if (growMonth == 2)  // Apply defoliation only in the second growing season month
                        ReduceFoliage(data.DeFolProp);
                    else
                    {
                        if (firstYear)
                            data.adjFracFol = speciesPnET.MaxFracFol;
                        // Foliage linearly increases with active biomass
                        float IdealFol = adjFracFol * FActiveBiom * data.TotalBiomass; // Using adjusted FracFol
                        float NSClimit = data.NSC;
                        if (mainLayerPAR < variables.PAR0) // indicates below the top layer
                        {
                            // lower canopy layers can retain a reserve of NSC (NSCReserve)
                            // which limits NSC available for refoliation - default is no 
                            // reserve (NSCReserve = 0)
                            NSClimit = data.NSC - (speciesPnET.NSCReserve * FActiveBiom * (data.TotalBiomass + data.Fol) * speciesPnET.CFracBiomass);
                        }
                        float FolCost = 0;
                        float FolTentative = 0;
                        if (growMonth < 2)  // Growing season months before defoliation outbreaks - can add foliage in first growing season month
                        {
                            if (IdealFol > data.Fol)
                            {
                                // Foliage allocation depends on availability of NSC
                                // (allows deficit at this time so no min nsc)
                                // carbon fraction of biomass to convert C to DW
                                FolCost = Math.Max(0.0f, Math.Min(NSClimit, speciesPnET.CFracBiomass * (IdealFol - Fol))); // gC/mo

                                // Add foliage allocation to foliage
                                FolTentative = FolCost / speciesPnET.CFracBiomass;// gDW
                            }
                            data.LastLAI = 0;
                        }
                        else if (growMonth == 3) // Refoliation can occur in the 3rd growing season month
                        {
                            if (data.DeFolProp > 0)  // Only defoliated cohorts can add refoliate
                            {
                                if (data.DeFolProp > speciesPnET.RefoliationMinimumTrigger)  // Refoliation threshold depends on species
                                {
                                    // Foliage allocation depends on availability of NSC
                                    // (allows deficit at this time so no min nsc)
                                    // carbon fraction of biomass to convert C to DW
                                    float FolAlloc = Math.Max(0.0f, Math.Min(NSClimit, speciesPnET.CFracBiomass * ((speciesPnET.RefoliationMaximum * IdealFol) - Fol)));  // variable refoliation
                                    FolCost = Math.Max(0.0f, Math.Min(NSClimit, speciesPnET.CFracBiomass * (speciesPnET.RefoliationCost * IdealFol - Fol)));  // cost of refol is the cost of getting to variable propotion of IdealFol
                                    FolTentative = FolAlloc / speciesPnET.CFracBiomass;// gDW
                                }
                                else // No attempted refoliation but carbon loss after defoliation
                                {
                                    // Foliage allocation depends on availability of NSC
                                    // (allows deficit at this time so no min nsc)
                                    // carbon fraction of biomass to convert C to DW
                                    FolCost = Math.Max(0.0f, Math.Min(NSClimit, speciesPnET.CFracBiomass * speciesPnET.NonRefoliationCost * IdealFol)); // gC/mo variable proportion of IdealFol to take out NSC 
                                }
                            }
                            // Non-defoliated trees do not add to their foliage
                        }
                        if (FolTentative > 0.01)
                        {
                            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
                            float tentativeLAI = 0;
                            for (int i = 0; i < Globals.IMAX; i++)
                                tentativeLAI += CalculateLAI(SpeciesPnET, Fol + FolTentative, i, tentativeLAI);
                            float tentativeCanopyProp = tentativeLAI / speciesPnET.MaxLAI;
                            if (sumCanopyProp > 1)
                                tentativeCanopyProp = tentativeCanopyProp / sumCanopyProp;
                            // Downgrade foliage added if canopy is expanding 
                            float actualFol = FolTentative;
                            // Add Foliage
                            data.Fol += actualFol;
                            data.MaxFolYear = Math.Max(data.MaxFolYear, data.Fol);
                        }
                        // Subtract refoliation cost from NSC
                        data.NSC -= FolCost;
                        if (data.NSC < 0)
                            data.NSC = 0.0f;
                    }
                }
            }

            // Leaf area index for the subcanopy layer by index. 
            // Function of specific leaf weight SLWMAX and the depth of the canopy
            data.LAI[index] = CalculateLAI(speciesPnET, Fol, index);

            // Adjust HalfSat for CO2 effect
            float halfSatIntercept = speciesPnET.HalfSat - 350 * speciesPnET.CO2HalfSatEff;
            data.AdjHalfSat = speciesPnET.CO2HalfSatEff * variables.CO2 + halfSatIntercept;
            // Reduction factor for radiation on photosynthesis
            float LayerPAR = (float) (mainLayerPAR * Math.Exp(-speciesPnET.K * (LAI.Sum() - LAI[index])));
            FRad[index] = ComputeFrad(LayerPAR, AdjHalfSat);
            // Get pressure head given ecoregion and soil water content (latter in hydrology)
            float PressureHead = hydrology.PressureHeadTable.CalculateWaterPressure(hydrology.Water, siteCohort.Ecoregion.SoilType);
            // Reduction water for sub or supra optimal soil water content
            float fWaterOzone = 1.0f;  //fWater for ozone functions; ignores H1 and H2 parameters because only impacts when drought-stressed
            if (Globals.ModelCore.CurrentTime > 0)
            {
                FWater[index] = ComputeFWater(speciesPnET.H1, speciesPnET.H2, speciesPnET.H3, speciesPnET.H4, PressureHead);
                Water[index] = hydrology.Water;
                PressHead[index] = PressureHead;
                NumEvents[index] = precipCount;
                fWaterOzone = ComputeFWater(-1, -1, speciesPnET.H3, speciesPnET.H4, PressureHead); // ignores H1 and H2 parameters because only impacts when drought-stressed
                if (frostFreeProp <= 0)
                {
                    FWater[index] = 0;
                    fWaterOzone = 0;
                }
            }
            else // Spinup
            {
                if (((Parameter<string>)Names.GetParameter(Names.SpinUpWaterStress)).Value == "true"
                    || ((Parameter<string>)Names.GetParameter(Names.SpinUpWaterStress)).Value == "yes")
                {
                    FWater[index] = ComputeFWater(speciesPnET.H1, speciesPnET.H2, speciesPnET.H3, speciesPnET.H4, PressureHead);
                    fWaterOzone = ComputeFWater(-1, -1, speciesPnET.H3, speciesPnET.H4, PressureHead); // ignores H1 and H2 parameters because only impacts when drought-stressed
                    if (frostFreeProp <= 0)
                    {
                        FWater[index] = 0;
                        fWaterOzone = 0;
                    }
                }
                else // Ignore H1 and H2 parameters during spinup
                {
                    FWater[index] = ComputeFWater(-1, -1, speciesPnET.H3, speciesPnET.H4, PressureHead);
                    fWaterOzone = FWater[index];
                    if (frostFreeProp <= 0)
                    {
                        FWater[index] = 0;
                        fWaterOzone = 0;
                    }
                }
                Water[index] = hydrology.Water;
                PressHead[index] = PressureHead;
                NumEvents[index] = precipCount;
            }
            
            // FoliarN adjusted based on canopy position (FRad)
            float folN_shape = speciesPnET.FolNShape; // Slope for linear FolN relationship
            float maxFolN = speciesPnET.MaxFolN; // Intercept for linear FolN relationship
            // Non-Linear reduction in FolN with canopy depth (FRad)
            data.adjFolN = speciesPnET.FolN + ((maxFolN - speciesPnET.FolN) * (float)Math.Pow(FRad[index], folN_shape)); //slope is shape parm; FolN is minFolN; intcpt is max FolN. EJG-7-24-18
            AdjFolN[index] = adjFolN;  // Stored for output
            AdjFracFol[index] = adjFracFol; //Stored for output

            float ciModifier = 1.0f; // if no ozone, ciModifier defaults to 1
            if (o3_cum > 0)
            {
                // Regression coefs estimated from New 3 algorithm for Ozone drought.xlsx
                // https://usfs.box.com/s/eksrr4d7fli8kr9r4knfr7byfy9r5z0i
                // Uses data provided by Yasutomo Hoshika and Elena Paoletti
                float ciMod_tol = fWaterOzone + (-0.021f * fWaterOzone + 0.0087f) * o3_cum;
                ciMod_tol = Math.Min(ciMod_tol, 1.0f);
                float ciMod_int = fWaterOzone + (-0.0148f * fWaterOzone + 0.0062f) * o3_cum;
                ciMod_int = Math.Min(ciMod_int, 1.0f);
                float ciMod_sens = fWaterOzone + (-0.0176f * fWaterOzone + 0.0118f) * o3_cum;
                ciMod_sens = Math.Min(ciMod_sens, 1.0f);
                if ((speciesPnET.O3StomataSens == "Sensitive") || (speciesPnET.O3StomataSens == "Sens"))
                    ciModifier = ciMod_sens;
                else if ((speciesPnET.O3StomataSens == "Tolerant") || (speciesPnET.O3StomataSens == "Tol"))
                    ciModifier = ciMod_tol;
                else if ((speciesPnET.O3StomataSens == "Intermediate") || (speciesPnET.O3StomataSens == "Int"))
                    ciModifier = ciMod_int;
                else
                    throw new Exception("Ozone data provided, but species O3StomataSens is not set to Sensitive, Tolerant or Intermediate");
            }
            // FIXME temporary fix
            if (ciModifier <= 0)
                ciModifier = 0.00001f;
            CiModifier[index] = ciModifier;  // Stored for output

            // If trees are physiologically active
            if (Leaf_On)
            {
                // CO2 ratio internal to the leaf versus external
                float cicaRatio = (-0.075f * adjFolN) + 0.875f;
                float modCiCaRatio = cicaRatio * ciModifier; 
                // Reference co2 ratio
                float ci350 = 350 * modCiCaRatio;
                // Elevated leaf internal co2 concentration
                float ciElev = variables.CO2 * modCiCaRatio;
                float Ca_Ci = variables.CO2 - ciElev;

                // Franks method
                // (Franks,2013, New Phytologist, 197:1077-1094)
                float Gamma = 40; // 40; Gamma is the CO2 compensation point (the point at which photorespiration balances exactly with photosynthesis.  Assumed to be 40 based on leaf temp is assumed to be 25 C

                // Modified Gamma based on air temp
                // Tested here but removed for release v3.0
                // Bernacchi et al. 2002. Plant Physiology 130, 1992-1998
                // Gamma* = e^(13.49-24.46/RTk) [R is universal gas constant = 0.008314 kJ/J/mole, Tk is absolute temperature]
                //float Gamma_T = (float) Math.Exp(13.49 - 24.46 / (0.008314 * (variables.Tday + 273)));

                float Ca0 = 350;  // 350
                float Ca0_adj = Ca0 * cicaRatio;  // Calculated internal concentration given external 350

                // Modified Franks method - by M. Kubiske
                // substitute ciElev for CO2
                float delamaxCi = (ciElev - Gamma) / (ciElev + 2 * Gamma) * (Ca0 + 2 * Gamma) / (Ca0 - Gamma);
                if (delamaxCi < 0)
                    delamaxCi = 0.0f;
                DelAmax[index] = delamaxCi;  // Modified Franks

                // M. Kubiske method for wue calculation:  Improved methods for calculating WUE and Transpiration in PnET.
                float V = (float)(8314.47 * (variables.Tmin + 273) / 101.3);
                float JCO2 = (float)(0.139 * ((variables.CO2 - ciElev) / V) * 0.000001);  // Corrected conversion units 11/29/22
                float JH2O = variables[species.Name].JH2O / ciModifier;  // Modified from * to / 11.18.2022 [mod1]
                float wue = JCO2 / JH2O * (44 / 18);  //44=mol wt CO2; 18=mol wt H2O; constant =2.44444444444444
                float Amax = (float)(delamaxCi * (speciesPnET.AmaxA + variables[species.Name].AmaxB_CO2 * adjFolN)); //nmole CO2/g Fol/s
                float BaseFolResp = variables[species.Name].BaseFolRespFrac * Amax; //nmole CO2/g Fol/s
                float AmaxAdj = Amax * speciesPnET.AmaxFrac;  //Amax adjustment as applied in PnET
                float GrossAmax = AmaxAdj + BaseFolResp; //nmole CO2/g Fol/s

                //Reference gross Psn (lab conditions) in gC/g Fol/month
                float RefGrossPsn = variables.DaySpan * GrossAmax * variables[species.Name].DVPD * variables.Daylength * Constants.MC / Constants.billion;

                // Remove FWater from psn reduction because it is accounted for in WUE through ciModifier [mod2, mod3]
                float GrossPsnPotential = 1 / (float)Globals.IMAX * variables[species.Name].FTempPSN * FRad[index] * FAge * RefGrossPsn * Fol;  // gC/m2 ground/mo
                
                // M. Kubiske equation for transpiration: Improved methods for calculating WUE and Transpiration in PnET.
                // JH2O has been modified by CiModifier to reduce water use efficiency
                // Scale transpiration to proportion of site occupied (CanopyLayerProp)
                // Corrected conversion factor                
                PotentialTranspiration[index] = (float)(0.0015f * (GrossPsnPotential / (JCO2 / JH2O))) * CanopyLayerProp; //mm

                // It is possible for transpiration to calculate to exceed available water
                // In this case, we cap transpiration at available water, and back-calculate GrossPsn and NetPsn to downgrade those as well
                // Volumetric water content (mm/m) at species wilting point (h4) 
                // Convert kPA to mH2o (/9.804139432)
                float wiltPtWater = (float) hydrology.PressureHeadTable.CalculateWaterContent(speciesPnET.H4 * 9.804139432f, siteCohort.Ecoregion.SoilType);
                float availableWater = (hydrology.Water - wiltPtWater) * siteCohort.Ecoregion.RootingDepth * frostFreeProp;

                if (PotentialTranspiration[index] > availableWater)
                {
                    Transpiration[index] = (float)Math.Max(availableWater, 0f); //mm
                    if (CanopyLayerProp > 0)
                        GrossPsn[index] = Transpiration[index] / 0.0015f * (JCO2 / JH2O) / CanopyLayerProp;
                    else
                        GrossPsn[index] = 0f;
                    if (PotentialTranspiration[index] > 0)
                        FWater[index] = Transpiration[index] / PotentialTranspiration[index];
                    else
                        FWater[index] = 0f;
                }
                else
                {
                    GrossPsn[index] = GrossPsnPotential * FWater[index];  // gC/m2 ground/mo
                    Transpiration[index] = PotentialTranspiration[index] * FWater[index]; //mm
                }

                // Subtract transpiration from hydrology
                success = hydrology.AddWater(-1 * Transpiration[index], siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                if (!success)
                    throw new Exception("Error adding water, Transpiration = " + Transpiration[index] + " water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
                if (hydrology.SurfaceWater > 0)
                {
                    float surfaceInput = Math.Min(hydrology.SurfaceWater, (siteCohort.Ecoregion.Porosity - hydrology.Water) * siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                    hydrology.SurfaceWater -= surfaceInput;
                    success = hydrology.AddWater(surfaceInput, siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                    if (!success)
                        throw new Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
                }

                // Net foliage respiration depends on reference psn (BaseFolResp)
                // Substitute 24 hours in place of DayLength because foliar respiration does occur at night.  BaseFolResp and Q10Factor use Tave temps reflecting both day and night temperatures.
                float RefFolResp = BaseFolResp * variables[species.Name].Q10Factor * variables.DaySpan * (Constants.SecondsPerHour * 24) * Constants.MC / Constants.billion; // gC/g Fol/month

                // Actual foliage respiration (growth respiration) 
                FolResp[index] = RefFolResp * Fol / (float)Globals.IMAX; // gC/m2 ground/mo

                // NetPsn psn depends on gross psn and foliage respiration
                float nonOzoneNetPsn = GrossPsn[index] - FolResp[index];

                // Convert Psn gC/m2 ground/mo to umolCO2/m2 fol/s
                // netPsn_ground = LayerNestPsn*1000000umol*(1mol/12gC) * (1/(60s*60min*14hr*30day))
                float netPsn_ground = nonOzoneNetPsn * 1000000F * (1F / 12F) * (1F / (variables.Daylength * variables.DaySpan));
                float netPsn_leaf_s = 0;
                if (netPsn_ground > 0 && LAI[index] > 0)
                {
                    netPsn_leaf_s = netPsn_ground * (1F / LAI[index]);
                    if (float.IsInfinity(netPsn_leaf_s))
                        netPsn_leaf_s = 0;
                }

                //Calculate water vapor conductance (gwv) from Psn and Ci; Kubiske Conductance_5.xlsx
                float gwv_mol = (float)(netPsn_leaf_s / (Ca_Ci) * 1.6 * 1000);
                float gwv = (float) (gwv_mol / (444.5 - 1.3667 * variables.Tave) * 10);

                // Reduction factor for ozone on photosynthesis
                if (o3_month > 0)
                {
                    float o3Coeff = speciesPnET.O3GrowthSens;
                    O3Effect = ComputeO3Effect_PnET(o3_month, subCanopyIndex, layerCount, Fol, lastO3Effect, gwv, o3Coeff);
                }
                else
                    O3Effect = 0;
                FOzone[index] = 1 - O3Effect;
                
                //Apply reduction factor for Ozone
                NetPsn[index] = nonOzoneNetPsn * FOzone[index];
                
                // Add net psn to non soluble carbons
                data.NSC += NetPsn[index]; //gC
                if (data.NSC < 0)
                    data.NSC = 0.0f;
            }
            else
            {
                // Reset subcanopy layer values
                NetPsn[index] = 0.0f;
                FolResp[index] = 0.0f;
                GrossPsn[index] = 0.0f;
                Transpiration[index] = 0.0f;
                PotentialTranspiration[index] = 0.0f;
                FOzone[index] = 1.0f;
            }
            index++;
            return success;
        }

        // LightEffect equation from PnET
        // Used in official releases >= 5.0
        public static float ComputeFrad(float Radiation, float HalfSat)
        {
            float fRad = 0.0f;
            if (HalfSat > 0)
                fRad = (float)(1.0 - Math.Exp(-1.0 * Radiation * Math.Log(2.0) / HalfSat));
            else
                throw new Exception("HalfSat <= 0. Cannot calculate fRad.");
            return fRad;
        }

        public static float ComputeFWater(float H1, float H2, float H3, float H4, float pressurehead)
        {
            float minThreshold = H1;
            if (H2 <= H1)
                minThreshold = H2;
            // Compute water stress
            if (pressurehead <= H1)
                return 0.0f;
            else if (pressurehead < minThreshold || pressurehead >= H4)
                return 0.0f;
            else if (pressurehead > H3)
                return 1 - ((pressurehead - H3) / (H4 - H3));
            else if (pressurehead < H2)
                return 1.0f / (H2 - H1) * pressurehead - (H1 / (H2 - H1));
            else
                return 1.0f;
        }

        public static float ComputeO3Effect_PnET(float o3, int Layer, int nLayers, float FolMass, float lastO3Effect, float gwv, float o3Coeff)
        {
            float droughtO3Frac = 1.0f; // Not using droughtO3Frac from PnET code per M. Kubiske and A. Chappelka
            float kO3Eff = 0.0026f * o3Coeff;  // Scaled by species using input parameters            
            float O3Prof = 0.6163f + (0.00105f * FolMass);
            float RelLayer = (float)Layer / (float)nLayers;
            float relO3 = Math.Min(1.0f, 1 - (float)Math.Pow(RelLayer * O3Prof, 3));
            // Kubiske method (using gwv in place of conductance
            float currentO3Effect = Math.Min(1.0f, (lastO3Effect * droughtO3Frac) + (kO3Eff * gwv * o3 * relO3));
            return currentO3Effect;
        }

        public int ComputeNonWoodyBiomass()
        {
            return (int)Fol;
        }

        public static Percentage ComputeNonWoodyPercentage(Cohort cohort)
        {
            return new Percentage(cohort.Fol / (cohort.Wood + cohort.Fol));
        }

        public void InitializeOutput(string SiteName, ushort YearOfBirth)
        {
            cohortoutput = new LocalOutput(SiteName, "Cohort_" + Species.Name + "_" + YearOfBirth + ".csv", OutputHeader);
        }

        public void InitializeOutput(string SiteName)
        {
            cohortoutput = new LocalOutput(SiteName, "Cohort_" + Species.Name + ".csv", OutputHeader);
        }

        public void InitializeOutput(LocalOutput localOutput)
        {
            cohortoutput = new LocalOutput(localOutput);
        }

        /// <summary>
        /// Raises a Cohort.DeathEvent.
        /// </summary>
        public static void Died(object sender,
                                ICohort cohort,
                                ActiveSite site,
                                ExtensionType disturbanceType)
        {
            if (DeathEvent != null)
                DeathEvent(sender, new DeathEventArgs(cohort, site, disturbanceType));
        }

        /// <summary>
        /// Raises a Cohort.DeathEvent if partial mortality.
        /// </summary>
        public static void PartialMortality(object sender,
                                            ICohort cohort,
                                            ActiveSite site,
                                            ExtensionType disturbanceType,
                                            float reduction)
        {
            if (PartialDeathEvent != null)
                PartialDeathEvent(sender, new PartialDeathEventArgs(cohort, site, disturbanceType, reduction));
        }

        /// <summary>
        /// Occurs when a cohort is killed by an age-only disturbance.
        /// </summary>
        public static event DeathEventHandler<DeathEventArgs> AgeOnlyDeathEvent;

        /// <summary>
        /// Occurs when a cohort dies either due to senescence or biomass
        /// disturbances.
        /// </summary>
        public static event DeathEventHandler<DeathEventArgs> DeathEvent;

        public static event PartialDeathEventHandler<PartialDeathEventArgs> PartialDeathEvent;

        /// <summary>
        /// Raises a Cohort.AgeOnlyDeathEvent.
        /// </summary>
        public static void KilledByAgeOnlyDisturbance(object sender,
                                                      ICohort cohort,
                                                      ActiveSite site,
                                                      ExtensionType disturbanceType)
        {
            if (AgeOnlyDeathEvent != null)
                AgeOnlyDeathEvent(sender, new DeathEventArgs(cohort, site, disturbanceType));
        }

        public void UpdateCohortData(IEcoregionPnETVariables monthdata)
        {
            float netPsnSum = NetPsn.Sum();
            float grossPsnSum = GrossPsn.Sum();
            float transpirationSum = Transpiration.Sum();
            //MG20261016 TODO this has an existing calculation elsewhere
            float JCO2_JH2O = 0;
            if (transpirationSum > 0)
                JCO2_JH2O = 0.0015f * grossPsnSum * CanopyLayerProp / transpirationSum;
            //MG20261016 TODO this will be in Constants.cs
            float WUE = JCO2_JH2O * (44.0f / 18.0f); 
            //44=mol wt CO2; 18=mol wt H2O; constant =2.44444444444444

            // determine the limiting factor 
            float fWaterAvg = FWater.Average();
            float pressHeadAvg = PressHead.Average();
            float fRadAvg = FRad.Average();
            float fOzoneAvg = FOzone.Average();
            float fTemp = monthdata[Species.Name].FTempPSN;
            string limitingFactor = "NA";
            if (ColdKill < int.MaxValue)
                limitingFactor = "ColdTol ("+ ColdKill.ToString()+ ")";
            else
            {
                List<float> factorList = new List<float>(new float[]{fWaterAvg,fRadAvg,fOzoneAvg,FAge,fTemp});
                float minFactor = factorList.Min();
                if (minFactor == fTemp)
                    limitingFactor = "fTemp";
                else if (minFactor == FAge)
                    limitingFactor = "fAge";
                else if (minFactor == fWaterAvg)
                {
                    if (pressHeadAvg > SpeciesPnET.H3)
                        limitingFactor = "Too_dry";
                    else if (pressHeadAvg < SpeciesPnET.H2)
                        limitingFactor = "Too_wet";
                    else
                        limitingFactor = "fWater";
                }
                else if (minFactor == fRadAvg)
                    limitingFactor = "fRad";
                else if (minFactor == fOzoneAvg)
                    limitingFactor = "fOzone";
            }

            // Cohort output file
            string s = Math.Round(monthdata.Time, 2) + "," +
                       monthdata.Year + "," +
                       monthdata.Month + "," +
                       Age + "," +
                       Layer + "," +
                       CanopyLayerProp + "," +
                       CanopyGrowingSpace + "," +
                       SumLAI + "," +
                       SumLAI * CanopyLayerProp + "," +
                       GrossPsn.Sum() + "," +
                       FolResp.Sum() + "," +
                       MaintenanceRespiration.Sum() + "," +
                       netPsnSum + "," +                  // Sum over canopy layers
                       transpirationSum + "," +
                       WUE.ToString() + "," +
                       Biomass + "," +  //MG20261016 more detail
                       LastBiomassDelta + "," +  //MG20261016 new variable
                       ANPP + "," +  //MG20261016 more detail
                       LastANPPDelta + "," +  //MG20261016 new variable
                       Fol + "," +
                       Root + "," +
                       Wood + "," +
                       Fol * CanopyLayerProp + "," +
                       Root * CanopyLayerProp + "," +
                       Wood * CanopyLayerProp + "," +
                       NSC + "," +
                       NSCfrac + "," +
                       fWaterAvg + "," +
                       Water.Average() + "," +
                       pressHeadAvg + "," +
                       fRadAvg + "," +
                       fOzoneAvg + "," +
                       DelAmax.Average() + "," +
                       monthdata[Species.Name].FTempPSN + "," +
                       monthdata[Species.Name].FTempRespWeightedDayAndNight + "," +
                       FAge + "," +
                       Leaf_On + "," +
                       FActiveBiom + "," +
                       AdjFolN.Average() + "," +
                       AdjFracFol.Average() + "," +
                       CiModifier.Average() + "," +
                       AdjHalfSat + "," +
                       limitingFactor + ",";
            cohortoutput.Add(s);
        }

        // Cohort output file header
        public string OutputHeader
        {
            get
            { 
                string hdr = OutputHeaders.Time + "," +
                             OutputHeaders.Year + "," +
                             OutputHeaders.Month + "," +
                             OutputHeaders.Age + "," +
                             OutputHeaders.Layer + "," +
                             OutputHeaders.CanopyLayerProp + "," +
                             OutputHeaders.CanopyGrowingSpace + "," +
                             OutputHeaders.LAI + "," +
                             OutputHeaders.LAISite + "," +
                             OutputHeaders.GrossPsn + "," +
                             OutputHeaders.FolResp + "," +
                             OutputHeaders.MaintResp + "," +
                             OutputHeaders.NetPsn + "," +
                             OutputHeaders.Transpiration + "," +
                             OutputHeaders.WUE + "," +
                             Biomass + "," +  //MG20261016 more detail
                             LastBiomassDelta + "," +  //MG20261016 new variable
                             ANPP + "," +  //MG20261016 more detail
                             LastANPPDelta + "," +  //MG20261016 new variable
                             OutputHeaders.Fol + "," +
                             OutputHeaders.Root + "," +
                             OutputHeaders.Wood + "," +
                             OutputHeaders.FolSite + "," +
                             OutputHeaders.RootSite + "," +
                             OutputHeaders.WoodSite + "," +
                             OutputHeaders.NSC + "," +
                             OutputHeaders.NSCfrac + "," +
                             OutputHeaders.fWater + "," +
                             OutputHeaders.Water + "," +
                             OutputHeaders.PressureHead + "," +
                             OutputHeaders.fRad + "," +
                             OutputHeaders.fOzone + "," +
                             OutputHeaders.DelAMax + "," +
                             OutputHeaders.fTemp_psn + "," +
                             OutputHeaders.fTemp_resp + "," +
                             OutputHeaders.fAge + "," +
                             OutputHeaders.LeafOn + "," +
                             OutputHeaders.FActiveBiom + "," +
                             OutputHeaders.AdjFolN + "," +
                             OutputHeaders.AdjFracFol + "," +
                             OutputHeaders.CiModifier + "," +
                             OutputHeaders.AdjHalfSat + "," +
                             OutputHeaders.LimitingFactor + ",";
                return hdr;
            }
        }

        public void WriteCohortData()
        {
            cohortoutput.Write();
        }

        public float FoliageSenescence()
        {
            // If it is fall 
            float Litter = speciesPnET.TOfol * Fol;
            // If cohort is dead, then all foliage is lost to litter
            if (NSCfrac <= 0.01f)
                Litter = Fol;
            Fol -= Litter;
            return Litter;
        }

        public float Senescence()
        {
            float senescence = (Root * speciesPnET.TOroot) + (Wood * speciesPnET.TOwood);
            data.TotalBiomass -= senescence;
            data.AGBiomass = (1 - speciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;
            data.BiomassMax = Math.Max(data.BiomassMax, data.TotalBiomass);
            data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerProp);
            return senescence;
        }

        public void ReduceFoliage(double fraction)
        {
            Fol *= 1.0f - (float)fraction;
            data.MaxFolYear = Math.Max(data.MaxFolYear, Fol);
        }

        public void ResetFoliageMax()
        {
            data.MaxFolYear = 0;
        }

        public void ReduceBiomass(object sitecohorts, double fraction, ExtensionType disturbanceType)
        {
            if (!((SiteCohorts)sitecohorts).DisturbanceTypesReduced.Contains(disturbanceType))
            {
                Allocation.ReduceDeadPools(sitecohorts, disturbanceType);  // Reduce dead pools before adding through Allocation
                ((SiteCohorts)sitecohorts).DisturbanceTypesReduced.Add(disturbanceType);
            }
            Allocation.Allocate(sitecohorts, this, disturbanceType, fraction);
            data.TotalBiomass *= (float)(1.0f - fraction);
            data.AGBiomass = (1 - speciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;
            data.BiomassMax = Math.Max(data.BiomassMax, data.TotalBiomass);
            Fol *= (float)(1.0f - fraction);
            data.MaxFolYear = Math.Max(data.MaxFolYear, Fol);
            data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerProp);
        }

        public float CalculateLAI(ISpeciesPnET species, float fol, int index)
        {
            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            float LAISum = 0;
            if (LAI != null)
                for (int i = 0; i < index; i++)
                    LAISum += LAI[i];
            float LAIlayerMax = (float)Math.Max(0.01f, 25.0f - LAISum); // Cohort LAI is capped at 25; once LAI reaches 25 subsequent sublayers get LAI of 0.01
            float LAIlayer = 1 / (float)Globals.IMAX * fol / (species.SLWmax - species.SLWDel * index * (1 / (float)Globals.IMAX) * fol);
            if (fol > 0 && LAIlayer <= 0)
            {
                Globals.ModelCore.UI.WriteLine("\n Warning: LAI was calculated to be negative for " + species.Name + ". This could be caused by a low value for SLWmax.  LAI applied in this case is a max of 25 for each cohort.");
                LAIlayer = LAIlayerMax/(Globals.IMAX - index);
            }
            else
                LAIlayer = (float)Math.Min(LAIlayerMax, LAIlayer);
            return LAIlayer;
        }

        public float CalculateLAI(ISpeciesPnET species, float fol, int index, float cumulativeLAI)
        {
            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            float LAISum = cumulativeLAI;
            // Cohort LAI is capped at 25; once LAI reaches 25 subsequent sublayers get LAI of 0.01
            float LAIlayerMax = Math.Max(0.01f, 25.0f - LAISum);
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

        /// <summary>
        /// Raises a Cohort.AgeOnlyDeathEvent.
        /// </summary>
        public static void RaiseDeathEvent(object sender,
                                           Cohort cohort, 
                                           ActiveSite site,
                                           ExtensionType disturbanceType)
        {
            if (DeathEvent != null)
                DeathEvent(sender, new DeathEventArgs(cohort, site, disturbanceType));
        }
    }
}

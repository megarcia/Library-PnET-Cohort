// uses dominance to allocate psn and subtract transpiration from soil water, average cohort vars over layer

using Landis.Core;
using Landis.Library.UniversalCohorts;
using Landis.SpatialModeling;
using Landis.Utilities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Landis.Library.PnETCohorts
{
    public class Cohort : Library.UniversalCohorts.Cohort, ICohort
    { 
        public delegate void SubtractTranspiration(float transpiration, ISpeciesPnET Species);
        public ushort index;
        private ISpecies species;
        private ISpeciesPnET speciesPnET;
        private CohortData data;
        private bool firstYear;
        private LocalOutput cohortoutput;

        /// <summary>
        /// Age (years)
        /// </summary>
        public ushort Age
        {
            get
            {
                return data.UniversalData.Age;
            }
        }

        /// <summary>
        /// Non soluble carbons
        /// </summary>
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

        /// <summary>
        /// Maximum Foliage Value For Current Year
        /// </summary>
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

        /// <summary>
        /// Measure of cohort's diffuse reflection of solar radiation out of total solar radiation without snow reflectance
        /// </summary>
        public float Albedo
        {
            get
            {
                float albedo = 0;
                if ((!string.IsNullOrEmpty(this.SpeciesPnET.Lifeform))
                        && (this.SpeciesPnET.Lifeform.ToLower().Contains("ground")
                            || this.SpeciesPnET.Lifeform.ToLower().Contains("open")
                            || this.SumLAI == 0))
                {
                    albedo = 0.20F;
                }
                else if ((!string.IsNullOrEmpty(this.SpeciesPnET.Lifeform))
                    && this.SpeciesPnET.Lifeform.ToLower().Contains("dark"))
                {
                    albedo = (float)((-0.067 * Math.Log(this.SumLAI < 0.7 ? 0.7 : this.SumLAI)) + 0.2095);
                }
                else if ((!string.IsNullOrEmpty(this.SpeciesPnET.Lifeform))
                        && this.SpeciesPnET.Lifeform.ToLower().Contains("light"))
                {
                    albedo = (float)((-0.054 * Math.Log(this.SumLAI < 0.7 ? 0.7 : this.SumLAI)) + 0.2082);
                }
                else if ((!string.IsNullOrEmpty(this.SpeciesPnET.Lifeform))
                        && this.SpeciesPnET.Lifeform.ToLower().Contains("decid"))
                {
                    albedo = (float)((-0.0073 * this.SumLAI) + 0.231);
                }

                // Do not allow albedo to be negative
                return albedo > 0 ? albedo : 0;
            }
        }

        /// <summary>
        /// Foliage (g/m2)
        /// </summary>
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

        /// <summary>
        /// Aboveground Biomass (g/m2) scaled to the site
        /// </summary>
        public int Biomass
        {
            get
            {
                return (int)(data.AGBiomass * data.CanopyLayerFrac);
            }
        }

        /// <summary>
        /// Species Moss Depth (m)
        /// </summary>
        public float MossDepth
        {
            get
            {
                return data.UniversalData.Biomass * speciesPnET.MossScalar;
            }
        }

        /// <summary>
        /// Aboveground Biomass (g/m2)
        /// </summary>
        public int AGBiomass
        {
            get
            {
                return (int)(Math.Round((1 - speciesPnET.FracBelowG) * data.TotalBiomass) + data.Fol);
            }
        }

        /// <summary>
        /// Total Biomass (root + wood) (g/m2)
        /// </summary>
        public int TotalBiomass
        {
            get
            {
                return (int)Math.Round(data.TotalBiomass);
            }
        }

        /// <summary>
        /// Wood (g/m2)
        /// </summary>
        public uint Wood
        {
            get
            {
                return (uint)Math.Round((1 - speciesPnET.FracBelowG) * data.TotalBiomass);
            }
        }

        /// <summary>
        /// Root (g/m2)
        /// </summary>
        public uint Root
        {
            get
            {
                return (uint)Math.Round(speciesPnET.FracBelowG * data.TotalBiomass);
            }
        }

        /// <summary>
        /// Max biomass achived in the cohorts' life time. 
        /// This value remains high after the cohort has reached its 
        /// peak biomass. It is used to determine canopy layers where
        /// it prevents that a cohort could descent in the canopy when 
        /// it declines (g/m2)
        /// </summary>
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

        /// <summary>
        /// Add dead wood to last senescence
        /// </summary>
        /// <param name="senescence"></param>
        public void AccumulateWoodySenescence(int senescence)
        {
            data.LastWoodySenescence += senescence;
        }

        /// <summary>
        /// Add dead foliage to last senescence
        /// </summary>
        /// <param name="senescence"></param>
        public void AccumulateFoliageSenescence(int senescence)
        {
            data.LastFoliageSenescence += senescence;
        }

        /// <summary>
        /// Growth reduction factor for age
        /// </summary>
        float FAge
        {
            get
            {
                return Math.Max(0, 1 - (float)Math.Pow(Age / (float)speciesPnET.Longevity, speciesPnET.PsnAgeRed));
            }
        }

        /// <summary>
        /// NSC fraction: measure for resources
        /// </summary>
        public float NSCfrac
        {
            get
            {
                return NSC / (FActiveBiom * (data.TotalBiomass + Fol) * SpeciesPnET.CFracBiomass);
            }
        }

        /// <summary>
        /// Species with PnET parameter additions
        /// </summary>
        public ISpeciesPnET SpeciesPnET
        {
            get
            {
                return speciesPnET;
            }
        }

        /// <summary>
        /// LANDIS species (without PnET parameter additions)
        /// </summary>
        public Landis.Core.ISpecies Species
        {
            get
            {
                return (Landis.Core.ISpecies)Globals.ModelCore.Species[species.Index];
            }
        }

        /// <summary>
        /// Defoliation fraction - BRM
        /// </summary>
        public float DefoliationFrac
        {
            get
            {
                return data.DefoliationFrac;
            }
            private set
            {
                data.DefoliationFrac = value;
            }
        }

        /// <summary>
        /// Annual Woody Senescence (g/m2)
        /// </summary>
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

        /// <summary>
        /// Annual Foliage Senescence (g/m2)
        /// </summary>
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

        /// <summary>
        /// Last average FRad
        /// </summary>
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

        public float[] FoliarRespiration
        {
            get
            {
                return data.FoliarRespiration;
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

        public bool IsLeafOn
        {
            get
            {
                return data.IsLeafOn;
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

        /// <summary>
        /// Determine if cohort is alive. It is assumed that a cohort is dead when 
        /// NSC decline below 1% of biomass
        /// </summary>
        public bool IsAlive
        {
            get
            {
                return NSCfrac > 0.01F;
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

        public float BiomassLayerFrac
        {
            get
            {
                return data.BiomassLayerFrac;
            }
            set
            {
                data.BiomassLayerFrac = value;
            }
        }

        public float CanopyLayerFrac
        {
            get
            {
                return data.CanopyLayerFrac;
            }
            set
            {
                data.CanopyLayerFrac = value;
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

        public double ANPP
        {
            get
            {
                return data.UniversalData.ANPP;
            }
            set
            {
                data.UniversalData.ANPP = value;
            }
        }

        /// <summary>
        /// List of DisturbanceTypes that have had ReduceDeadPools applied
        /// </summary>
        public List<ExtensionType> ReducedTypes = null;

        /// <summary>
        /// Index of growing season month
        /// </summary>
        public int growMonth = -1;

        /// <summary>
        /// Initialize subcanopy layers
        /// </summary>
        public void InitializeSubLayers()
        {
            index = 0;
            data.LAI = new float[Globals.IMAX];
            data.GrossPsn = new float[Globals.IMAX];
            data.FoliarRespiration = new float[Globals.IMAX];
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
            if (data.IsLeafOn)
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
                //slope is shape parm; fracFol is minFracFol; int is maxFracFol. EJG-7-24-18
                data.adjFracFol = speciesPnET.FracFol + ((fracFol_int - speciesPnET.FracFol) * (float)Math.Pow(lastSeasonAvgFRad, fracFol_slope)); 
                firstYear = false;
            }
            else
                data.adjFracFol = speciesPnET.FracFol;
        }

        /// <summary>
        /// Reset values for subcanopy layers
        /// </summary>
        public void NullSubLayers()
        {
            data.LAI = null;
            data.GrossPsn = null;
            data.FoliarRespiration = null;
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

        /// <summary>
        /// Get totals for combined cohorts
        /// </summary>
        /// <param name="c"></param>
        public void Accumulate(Cohort c)
        {
            data.TotalBiomass += c.TotalBiomass;
            data.BiomassMax = Math.Max(BiomassMax, data.TotalBiomass);
            data.Fol += c.Fol;
            data.MaxFolYear = Math.Max(MaxFolYear, data.Fol);
            data.AGBiomass = (1 - c.SpeciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;
            data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerFrac);
            data.UniversalData.ANPP += c.ANPP;
        }

        /// <summary>
        /// Increments the cohort's age by one year.
        /// </summary>
        public void IncrementAge()
        {
            data.UniversalData.Age += 1;
        }

        /// <summary>
        /// Changes the cohort's biomass.
        /// </summary>
        public void ChangeBiomass(int delta)
        {
            float newTotalBiomass = data.TotalBiomass + delta;
            data.TotalBiomass = System.Math.Max(0, newTotalBiomass);
            data.AGBiomass = (1 - this.SpeciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;
            data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerFrac);
            data.BiomassMax = Math.Max(data.BiomassMax, data.TotalBiomass);
        }

        /// <summary>
        /// Changes the cohort's ANPP.
        /// </summary>
        public void ChangeANPP(double delta)
        {
            data.UniversalData.ANPP = data.UniversalData.ANPP + delta;
        }

        // Constructor
        public Cohort(ISpecies species, ISpeciesPnET speciesPnET, ushort year_of_birth, string SiteName, double fracBiomass, bool cohortStacking) // : base(species, 0, (int)(1F / species.DNSC * (ushort)species.InitialNSC))
        {
            this.species = species;
            this.speciesPnET = speciesPnET;
            this.data.UniversalData.Age = 1;
            this.data.ColdKill = int.MaxValue;
            this.data.NSC = (ushort)speciesPnET.InitialNSC;
            // Initialize biomass assuming fixed concentration of NSC, convert gC to gDW
            this.data.TotalBiomass = (uint)Math.Max(1.0, this.NSC / (speciesPnET.DNSC * speciesPnET.CFracBiomass) * fracBiomass);
            this.data.AGBiomass = (1 - speciesPnET.FracBelowG) * this.data.TotalBiomass + this.data.Fol;
            this.data.BiomassMax = this.data.TotalBiomass;
            float cohortLAI = 0;
            float cohortIdealFol = speciesPnET.FracFol * this.FActiveBiom * this.data.TotalBiomass;

            for (int i = 0; i < Globals.IMAX; i++)
                cohortLAI += CalcLAI(this.SpeciesPnET, cohortIdealFol, i, cohortLAI);

            this.data.LastLAI = cohortLAI;
            this.data.LastAGBio = this.data.AGBiomass;
            this.data.CanopyLayerFrac = this.data.LastLAI / speciesPnET.MaxLAI;
            if (cohortStacking)
                this.data.CanopyLayerFrac = 1.0f;
            this.data.CanopyGrowingSpace = 1.0f;
            this.data.UniversalData.Biomass = (int)(this.data.AGBiomass * this.data.CanopyLayerFrac);
            this.data.UniversalData.ANPP = this.data.UniversalData.Biomass;
            // Then overwrite them if you need stuff for outputs
            if (SiteName != null)
                InitializeOutput(SiteName, year_of_birth);
            data.LastSeasonFRad = new List<float>();
            firstYear = true;
        }

        public Cohort(ISpecies species, CohortData cohortData)
        {
            this.species = species;
            this.speciesPnET = SpeciesParameters.SpeciesPnET.AllSpecies[species.Index];
            this.data = cohortData;
        }

        public Cohort(Cohort cohort)
        {
            this.species = cohort.Species;
            this.speciesPnET = cohort.speciesPnET;
            this.data.UniversalData.Age = cohort.Age;
            this.data.NSC = cohort.NSC;
            this.data.TotalBiomass = cohort.TotalBiomass;
            this.data.AGBiomass = (1 - cohort.SpeciesPnET.FracBelowG) * cohort.TotalBiomass + cohort.Fol;
            this.data.UniversalData.Biomass = (int)(this.data.AGBiomass * cohort.CanopyLayerFrac);
            this.data.BiomassMax = cohort.BiomassMax;
            this.data.Fol = cohort.Fol;
            this.data.MaxFolYear = cohort.MaxFolYear;
            this.data.LastSeasonFRad = cohort.data.LastSeasonFRad;
            this.data.ColdKill = int.MaxValue;
            this.data.UniversalData.ANPP = cohort.ANPP;
        }

        public Cohort(Cohort cohort, ushort firstYear, string SiteName)
        {
            this.species = cohort.Species;
            this.speciesPnET = cohort.speciesPnET;
            this.data.UniversalData.Age = cohort.Age;
            this.data.NSC = cohort.NSC;
            this.data.TotalBiomass = cohort.TotalBiomass;
            this.data.AGBiomass = (1 - cohort.SpeciesPnET.FracBelowG) * cohort.TotalBiomass + cohort.Fol;
            this.data.UniversalData.Biomass = (int)(this.data.AGBiomass * cohort.CanopyLayerFrac);
            this.data.BiomassMax = cohort.BiomassMax;
            this.data.Fol = cohort.Fol;
            this.data.MaxFolYear = cohort.MaxFolYear;
            this.data.LastSeasonFRad = cohort.data.LastSeasonFRad;
            this.data.ColdKill = int.MaxValue;
            this.data.UniversalData.ANPP = cohort.ANPP;
            if (SiteName != null)
                InitializeOutput(SiteName, firstYear);
        }

        public Cohort(ISpeciesPnET speciesPnET, ushort age, int woodBiomass, string SiteName, ushort firstYear, bool cohortStacking)
        {
            InitializeSubLayers();
            this.species = (ISpecies)speciesPnET;
            this.speciesPnET = speciesPnET;
            this.data.UniversalData.Age = age;
            // incoming biomass is aboveground wood, calculate total biomass
            float biomass = woodBiomass / (1 - speciesPnET.FracBelowG);
            this.data.TotalBiomass = biomass;
            this.data.BiomassMax = biomass;
            this.data.LastSeasonFRad = new List<float>();
            this.data.adjFracFol = speciesPnET.MaxFracFol;
            this.data.ColdKill = int.MaxValue;
            float cohortLAI = 0;
            float cohortIdealFol = speciesPnET.MaxFracFol * this.FActiveBiom * this.data.TotalBiomass;

            for (int i = 0; i < Globals.IMAX; i++)
            {
                float subLayerLAI = CalcLAI(this.SpeciesPnET, cohortIdealFol, i);
                cohortLAI += subLayerLAI;
                if (this.IsLeafOn)
                    LAI[index] = subLayerLAI;
            }

            if (this.IsLeafOn)
            {
                this.data.Fol = cohortIdealFol;
                this.data.MaxFolYear = cohortIdealFol;
            }
            this.data.LastLAI = cohortLAI;
            this.data.CanopyLayerFrac = this.data.LastLAI / speciesPnET.MaxLAI;
            if (cohortStacking)
                this.data.CanopyLayerFrac = 1.0f;
            this.data.CanopyGrowingSpace = 1.0f;
            this.data.AGBiomass = (1 - this.speciesPnET.FracBelowG) * this.data.TotalBiomass + this.data.Fol;
            this.data.LastAGBio = this.data.AGBiomass;
            this.data.UniversalData.Biomass = (int)(this.data.AGBiomass * this.data.CanopyLayerFrac);
            this.data.NSC = this.speciesPnET.DNSC * this.FActiveBiom * (this.data.TotalBiomass + this.data.Fol) * speciesPnET.CFracBiomass;
            if (SiteName != null)
                InitializeOutput(SiteName, firstYear);
        }

        public Cohort(ISpeciesPnET speciesPnET, ushort age, int woodBiomass, int maxBiomass, float canopyGrowingSpace, string SiteName, ushort firstYear, bool cohortStacking, float lastSeasonAvgFrad)
        {
            InitializeSubLayers();
            this.species = (ISpecies)speciesPnET;
            this.speciesPnET = speciesPnET;
            this.data.UniversalData.Age = age;
            // incoming biomass is aboveground wood, calculate total biomass
            float biomass = woodBiomass / (1 - speciesPnET.FracBelowG);
            this.data.TotalBiomass = biomass;
            this.data.BiomassMax = Math.Max(biomass, maxBiomass);
            this.data.LastSeasonFRad = new List<float>();
            this.data.LastSeasonFRad.Add(lastSeasonAvgFrad);
            this.CalcAdjFracFol();
            this.data.ColdKill = int.MaxValue;
            float cohortLAI = 0;
            float cohortIdealFol = this.adjFracFol * this.FActiveBiom * this.data.TotalBiomass;

            for (int i = 0; i < Globals.IMAX; i++)
            {
                float subLayerLAI = CalcLAI(this.SpeciesPnET, cohortIdealFol, i);
                cohortLAI += subLayerLAI;
                if (this.IsLeafOn)
                    LAI[index] = subLayerLAI;
            }

            if (this.IsLeafOn)
            {
                this.data.Fol = cohortIdealFol;
                this.data.MaxFolYear = cohortIdealFol;
            }
            this.data.LastLAI = cohortLAI;
            this.data.CanopyLayerFrac = this.data.LastLAI / speciesPnET.MaxLAI;
            if (cohortStacking)
                this.data.CanopyLayerFrac = 1.0f;
            this.data.CanopyGrowingSpace = 1.0f;
            this.data.AGBiomass = (1 - this.speciesPnET.FracBelowG) * this.data.TotalBiomass + this.data.Fol;
            this.data.LastAGBio = this.data.AGBiomass;
            this.data.UniversalData.Biomass = (int)(this.data.AGBiomass * this.data.CanopyLayerFrac);
            this.data.NSC = this.speciesPnET.DNSC * this.FActiveBiom * (this.data.TotalBiomass + data.Fol) * speciesPnET.CFracBiomass;
            if (SiteName != null)
                InitializeOutput(SiteName, firstYear);
        }

        public void CalcDefoliationFrac(ActiveSite site, int SiteAboveGroundBiomass)
        {
            lock (Globals.distributionThreadLock)
            {
                data.DefoliationFrac = (float)Library.UniversalCohorts.CohortDefoliation.Compute(site, this, 0, SiteAboveGroundBiomass);
            }
        }

        /// <summary>
        /// Photosynthesis by canopy layer
        /// </summary>
        /// <param name="PrecInByCanopyLayer"></param>
        /// <param name="precipCount"></param>
        /// <param name="leakageFrac"></param>
        /// <param name="hydrology"></param>
        /// <param name="mainLayerPAR"></param>
        /// <param name="SubCanopyPar"></param>
        /// <param name="o3_cum"></param>
        /// <param name="o3_month"></param>
        /// <param name="subCanopyIndex"></param>
        /// <param name="layerCount"></param>
        /// <param name="O3Effect"></param>
        /// <param name="frostFreeFrac"></param>
        /// <param name="MeltInByCanopyLayer"></param>
        /// <param name="coldKillBoolean"></param>
        /// <param name="variables"></param>
        /// <param name="siteCohort"></param>
        /// <param name="sumCanopyFrac"></param>
        /// <param name="groundPETbyEvent"></param>
        /// <param name="allowMortality"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public bool CalcPhotosynthesis(float PrecInByCanopyLayer, int precipCount, float leakageFrac, ref Hydrology hydrology, float mainLayerPAR, ref float SubCanopyPar, float o3_cum, float o3_month, int subCanopyIndex, int layerCount, ref float O3Effect, float frostFreeFrac, float MeltInByCanopyLayer, bool coldKillBoolean, IEcoregionPnETVariables variables, SiteCohorts siteCohort, float sumCanopyFrac, float groundPETbyEvent, bool allowMortality = true)
        {
            bool success = true;
            float lastO3Effect = O3Effect;
            O3Effect = 0;

            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            data.LAI[index] = CalcLAI(speciesPnET, data.Fol, index);

            if (MeltInByCanopyLayer > 0)
            {
                // Add melted snow water to soil moisture
                // Instantaneous runoff (excess of porosity + RunoffCapture)
                float waterCapacity = siteCohort.Ecoregion.Porosity * siteCohort.Ecoregion.RootingDepth * frostFreeFrac; //mm
                float meltrunoff = Math.Min(MeltInByCanopyLayer, Math.Max(hydrology.Water * siteCohort.Ecoregion.RootingDepth * frostFreeFrac + MeltInByCanopyLayer - waterCapacity, 0));
                float capturedRunoff = 0;
                if ((siteCohort.Ecoregion.RunoffCapture > 0) & (meltrunoff > 0))
                {
                    capturedRunoff = Math.Max(0, Math.Min(meltrunoff, siteCohort.Ecoregion.RunoffCapture - hydrology.SurfaceWater));
                    hydrology.SurfaceWater += capturedRunoff;
                }
                hydrology.RunOff += meltrunoff - capturedRunoff;
                success = hydrology.AddWater(MeltInByCanopyLayer - meltrunoff, siteCohort.Ecoregion.RootingDepth * frostFreeFrac);
                if (success == false)
                    throw new System.Exception("Error adding water, MeltInByCanopyLayer = " + MeltInByCanopyLayer + "; water = " + hydrology.Water + "; meltrunoff = " + meltrunoff + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
            }

            float precipIn = 0;
            if (PrecInByCanopyLayer > 0)
            {
                // If more than one precip event assigned to layer, repeat precip, runoff, leakage for all events prior to respiration
                for (int p = 1; p <= precipCount; p++)
                {
                    // Incoming precipitation
                    // Instantaneous runoff (excess of porosity)
                    float waterCapacity = siteCohort.Ecoregion.Porosity * siteCohort.Ecoregion.RootingDepth * frostFreeFrac; //mm
                    float rainrunoff = Math.Min(PrecInByCanopyLayer, Math.Max(hydrology.Water * siteCohort.Ecoregion.RootingDepth * frostFreeFrac + PrecInByCanopyLayer - waterCapacity, 0));
                    float capturedRunoff = 0;
                    if ((siteCohort.Ecoregion.RunoffCapture > 0) & (rainrunoff > 0))
                    {
                        capturedRunoff = Math.Max(0, Math.Min(rainrunoff, siteCohort.Ecoregion.RunoffCapture - hydrology.SurfaceWater));
                        hydrology.SurfaceWater += capturedRunoff;
                    }
                    hydrology.RunOff += rainrunoff - capturedRunoff;
                    precipIn = PrecInByCanopyLayer - rainrunoff; //mm
                    // Add incoming precipitation to soil moisture
                    success = hydrology.AddWater(precipIn, siteCohort.Ecoregion.RootingDepth * frostFreeFrac);
                    if (success == false)
                        throw new System.Exception("Error adding water, waterIn = " + precipIn + "; water = " + hydrology.Water + "; rainrunoff = " + rainrunoff + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
                    float leakage = Math.Max((float)leakageFrac * (hydrology.Water - siteCohort.Ecoregion.FieldCap), 0) * siteCohort.Ecoregion.RootingDepth * frostFreeFrac; //mm
                    hydrology.Leakage += leakage;
                    // Remove fast leakage
                    success = hydrology.AddWater(-1 * leakage, siteCohort.Ecoregion.RootingDepth * frostFreeFrac);
                    if (success == false)
                        throw new System.Exception("Error adding water, Hydrology.Leakage = " + hydrology.Leakage + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
                    // Add surface water to soil
                    if (hydrology.SurfaceWater > 0)
                    {
                        float surfaceInput = Math.Min(hydrology.SurfaceWater, (siteCohort.Ecoregion.Porosity - hydrology.Water) * siteCohort.Ecoregion.RootingDepth * frostFreeFrac);
                        hydrology.SurfaceWater -= surfaceInput;
                        success = hydrology.AddWater(surfaceInput, siteCohort.Ecoregion.RootingDepth * frostFreeFrac);
                        if (success == false)
                            throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
                    }
                }
            }
            else
            {
                // Leakage only occurs following precipitation events or incoming melt water
                if (MeltInByCanopyLayer > 0)
                {
                    float leakage = Math.Max((float)leakageFrac * (hydrology.Water - siteCohort.Ecoregion.FieldCap), 0) * siteCohort.Ecoregion.RootingDepth * frostFreeFrac; //mm
                    hydrology.Leakage += leakage;
                    // Remove fast leakage
                    success = hydrology.AddWater(-1 * leakage, siteCohort.Ecoregion.RootingDepth * frostFreeFrac);
                    if (success == false)
                        throw new System.Exception("Error adding water, Hydrology.Leakage = " + hydrology.Leakage + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
                    // Add surface water to soil
                    if (hydrology.SurfaceWater > 0)
                    {
                        float surfaceInput = Math.Min(hydrology.SurfaceWater, (siteCohort.Ecoregion.Porosity - hydrology.Water) * siteCohort.Ecoregion.RootingDepth * frostFreeFrac);
                        hydrology.SurfaceWater -= surfaceInput;
                        success = hydrology.AddWater(surfaceInput, siteCohort.Ecoregion.RootingDepth * frostFreeFrac);
                        if (success == false)
                            throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
                    }
                }
            }

            // Maintenance respiration depends on biomass,  non soluble carbon and temperature
            data.MaintenanceRespiration[index] = 1 / (float)Globals.IMAX * (float)Math.Min(NSC, variables[Species.Name].MaintRespFTempResp * (data.TotalBiomass * speciesPnET.CFracBiomass));//gC //IMAXinverse
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
                        //Check if nscfrac is below threshold to determine if cohort is alive
                        if (!this.IsAlive)
                            data.NSC = 0.0F;  // if cohort is dead, nsc goes to zero and becomes functionally dead even though not removed until end of timestep
                        else if (Globals.ModelCore.CurrentTime > 0 && this.TotalBiomass < (uint)speciesPnET.InitBiomass)  //Check if biomass < Initial Biomass -> cohort dies
                        {
                            data.NSC = 0.0F;  // if cohort is dead, nsc goes to zero and becomes functionally dead even though not removed until end of timestep
                            data.IsLeafOn = false;
                            data.NSC = 0.0F;
                            float foliageSenescence = FoliageSenescence();
                            data.LastFoliageSenescence = foliageSenescence;
                            siteCohort.AddLitter(foliageSenescence * data.CanopyLayerFrac, SpeciesPnET);// Using Canopy fractioning
                        }
                    }
                    float woodSenescence = Senescence();
                    data.LastWoodySenescence = woodSenescence;
                    siteCohort.AddWoodyDebris(woodSenescence * data.CanopyLayerFrac, speciesPnET.KWdLit); // Using Canopy fractioning

                    // Release of nsc, will be added to biomass components next year
                    // Assumed that NSC will have a minimum concentration, excess is allocated to biomass
                    float Allocation = Math.Max(NSC - (speciesPnET.DNSC * FActiveBiom * data.TotalBiomass * speciesPnET.CFracBiomass), 0);
                    data.TotalBiomass += Allocation / speciesPnET.CFracBiomass;  // convert gC to gDW
                    data.AGBiomass = (1 - speciesPnET.FracBelowG) * this.data.TotalBiomass + this.data.Fol;
                    data.UniversalData.Biomass = (int)(this.data.AGBiomass * this.data.CanopyLayerFrac);
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
                    data.ColdKill = (int)Math.Floor(variables.Tavg - (3.0 * siteCohort.Ecoregion.WinterSTD));
                    data.IsLeafOn = false;
                    data.NSC = 0.0F;
                    float foliageSenescence = FoliageSenescence();
                    data.LastFoliageSenescence = foliageSenescence;
                    siteCohort.AddLitter(foliageSenescence * data.CanopyLayerFrac, SpeciesPnET); // Using Canopy fractioning
                }
                else
                {
                    // When LeafOn becomes false for the first time in a year
                    if (variables.Tmin <= this.SpeciesPnET.LeafOnMinT)
                    {
                        if (data.IsLeafOn == true)
                        {
                            data.IsLeafOn = false;
                            float foliageSenescence = FoliageSenescence();
                            data.LastFoliageSenescence = foliageSenescence;
                            siteCohort.AddLitter(foliageSenescence * data.CanopyLayerFrac, SpeciesPnET); // Using Canopy fractioning
                        }
                        growMonth = -1;
                    }
                    else
                    {
                        if (frostFreeFrac > 0)
                        {
                            // LeafOn becomes true for the first time in a year
                            if (data.IsLeafOn == false)
                                growMonth = 1;
                            else
                                growMonth += 1;
                            data.IsLeafOn = true;
                        }
                    }
                }
                if (data.IsLeafOn)
                {
                    // Apply defoliation only in the second growing season month
                    if (growMonth == 2)
                        ReduceFoliage(data.DefoliationFrac);
                    else
                    {
                        if (firstYear)
                            data.adjFracFol = speciesPnET.MaxFracFol;
                        // Foliage linearly increases with active biomass
                        float IdealFol = adjFracFol * FActiveBiom * data.TotalBiomass; // Using adjusted FracFol
                        float NSClimit = data.NSC;
                        if (mainLayerPAR < variables.PAR0) // indicates below the top layer
                        {
                            // lower canopy layers can retain a reserve of NSC (NSCReserve) which limits NSC available for refoliation - default is no reserve (NSCReserve = 0)
                            NSClimit = data.NSC - (speciesPnET.NSCReserve * (FActiveBiom * (data.TotalBiomass + data.Fol) * speciesPnET.CFracBiomass));
                        }
                        float FolCost = 0;
                        float FolTentative = 0;
                        if (growMonth < 2)  // Growing season months before defoliation outbreaks - can add foliage in first growing season month
                        {
                            if (IdealFol > data.Fol)
                            {
                                // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                                // carbon fraction of biomass to convert C to DW
                                FolCost = Math.Max(0, Math.Min(NSClimit, speciesPnET.CFracBiomass * (IdealFol - Fol))); // gC/mo
                                // Add foliage allocation to foliage
                                FolTentative = FolCost / speciesPnET.CFracBiomass;// gDW
                            }
                            data.LastLAI = 0;
                        }
                        else if (growMonth == 3) // Refoliation can occur in the 3rd growing season month
                        {
                            if (data.DefoliationFrac > 0)  // Only defoliated cohorts can add refoliate
                            {
                                if (data.DefoliationFrac > speciesPnET.RefoliationMinimumTrigger)  // Refoliation threshold is variable
                                {
                                    // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                                    // carbon fraction of biomass to convert C to DW
                                    float Folalloc = Math.Max(0f, Math.Min(NSClimit, speciesPnET.CFracBiomass * ((speciesPnET.RefoliationMaximum * IdealFol) - Fol)));  // variable refoliation
                                    FolCost = Math.Max(0f, Math.Min(NSClimit, speciesPnET.CFracBiomass * (speciesPnET.RefoliationCost * IdealFol - Fol)));  // cost of refol is the cost of getting to variable propotion of IdealFol
                                    FolTentative = Folalloc / speciesPnET.CFracBiomass;// gDW
                                }
                                else // No attempted refoliation but carbon loss after defoliation
                                {
                                    // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                                    // carbon fraction of biomass to convert C to DW
                                    FolCost = Math.Max(0f, Math.Min(NSClimit, speciesPnET.CFracBiomass * (speciesPnET.NonRefoliationCost * IdealFol))); // gC/mo variable fraction of IdealFol to take out NSC 
                                }
                            }
                            // Non-defoliated trees do not add to their foliage
                        }
                        if (FolTentative > 0.01)
                        {
                            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
                            float tentativeLAI = 0;
                            for (int i = 0; i < Globals.IMAX; i++)
                                tentativeLAI += CalcLAI(this.SpeciesPnET, Fol + FolTentative, i, tentativeLAI);
                            float tentativeCanopyFrac = tentativeLAI / this.speciesPnET.MaxLAI;
                            if (sumCanopyFrac > 1)
                                tentativeCanopyFrac = tentativeCanopyFrac / sumCanopyFrac;
                            // Downgrade foliage added if canopy is expanding 
                            float actualFol = FolTentative;
                            // Add Foliage
                            data.Fol += actualFol;
                            data.MaxFolYear = Math.Max(data.MaxFolYear, data.Fol);
                        }
                        // Subtract from NSC
                        data.NSC -= FolCost;
                        if (data.NSC < 0)
                            data.NSC = 0f;
                    }
                }
            }
            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            data.LAI[index] = CalcLAI(speciesPnET, Fol, index);
            // Adjust HalfSat for CO2 effect
            float halfSatIntercept = speciesPnET.HalfSat - 350 * speciesPnET.CO2HalfSatEff;
            data.AdjHalfSat = speciesPnET.CO2HalfSatEff * variables.CO2 + halfSatIntercept;
            // Reduction factor for radiation on photosynthesis
            float LayerPAR = (float)(mainLayerPAR * Math.Exp(-speciesPnET.K * (LAI.Sum() - LAI[index])));
            FRad[index] = CalcFrad(LayerPAR, AdjHalfSat);
            // Get pressure head given ecoregion and soil water content (latter in hydrology)
            float PressureHead = hydrology.PressureHeadTable.CalcWaterPressure(hydrology.Water, siteCohort.Ecoregion.SoilType);
            // Reduction water for sub or supra optimal soil water content
            float fWaterOzone = 1.0f;  //fWater for ozone functions; ignores H1 and H2 parameters because only impacts when drought-stressed
            if (Globals.ModelCore.CurrentTime > 0)
            {
                FWater[index] = CalcFWater(speciesPnET.H1, speciesPnET.H2, speciesPnET.H3, speciesPnET.H4, PressureHead);
                Water[index] = hydrology.Water;
                PressHead[index] = PressureHead;
                NumEvents[index] = precipCount;
                fWaterOzone = CalcFWater(-1, -1, speciesPnET.H3, speciesPnET.H4, PressureHead); // ignores H1 and H2 parameters because only impacts when drought-stressed
                if (frostFreeFrac <= 0)
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
                    FWater[index] = CalcFWater(speciesPnET.H1, speciesPnET.H2, speciesPnET.H3, speciesPnET.H4, PressureHead);
                    fWaterOzone = CalcFWater(-1, -1, speciesPnET.H3, speciesPnET.H4, PressureHead); // ignores H1 and H2 parameters because only impacts when drought-stressed
                    if (frostFreeFrac <= 0)
                    {
                        FWater[index] = 0;
                        fWaterOzone = 0;
                    }
                }
                else // Ignore H1 and H2 parameters during spinup
                {
                    FWater[index] = CalcFWater(-1, -1, speciesPnET.H3, speciesPnET.H4, PressureHead);
                    fWaterOzone = FWater[index];
                    if (frostFreeFrac <= 0)
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
            // slope is shape parm; FolN is minFolN; intcpt is max FolN. EJG-7-24-18
            data.adjFolN = speciesPnET.FolN + ((maxFolN - speciesPnET.FolN) * (float)Math.Pow(FRad[index], folN_shape)); 
            AdjFolN[index] = adjFolN;  // Stored for output
            AdjFracFol[index] = adjFracFol; //Stored for output
            float ciModifier = 1.0f; // if no ozone, ciModifier defaults to 1
            if (o3_cum > 0)
            {
                // Regression coefs estimated from New 3 algorithm for Ozone drought.xlsx
                // https://usfs.box.com/s/eksrr4d7fli8kr9r4knfr7byfy9r5z0i
                // Uses data provided by Yasutomo Hoshika and Elena Paoletti
                float ciMod_tol = (float)(fWaterOzone + (-0.021 * fWaterOzone + 0.0087) * o3_cum);
                ciMod_tol = Math.Min(ciMod_tol, 1.0f);
                float ciMod_int = (float)(fWaterOzone + (-0.0148 * fWaterOzone + 0.0062) * o3_cum);
                ciMod_int = Math.Min(ciMod_int, 1.0f);
                float ciMod_sens = (float)(fWaterOzone + (-0.0176 * fWaterOzone + 0.0118) * o3_cum);
                ciMod_sens = Math.Min(ciMod_sens, 1.0f);
                if ((speciesPnET.O3StomataSens == "Sensitive") || (speciesPnET.O3StomataSens == "Sens"))
                    ciModifier = ciMod_sens;
                else if ((speciesPnET.O3StomataSens == "Tolerant") || (speciesPnET.O3StomataSens == "Tol"))
                    ciModifier = ciMod_tol;
                else if ((speciesPnET.O3StomataSens == "Intermediate") || (speciesPnET.O3StomataSens == "Int"))
                    ciModifier = ciMod_int;
                else
                    throw new System.Exception("Ozone data provided, but species O3StomataSens is not set to Sensitive, Tolerant or Intermediate");
            }
            // FIXME temporary fix
            if (ciModifier <= 0)
                ciModifier = 0.00001f;
            CiModifier[index] = ciModifier;  // Stored for output
            // If trees are physiologically active
            if (IsLeafOn)
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
                float Ca0 = 350;  // 350
                float Ca0_adj = Ca0 * cicaRatio;  // Calculated internal concentration given external 350
                // Modified Franks method - by M. Kubiske
                // substitute ciElev for CO2
                float delamaxCi = (ciElev - Gamma) / (ciElev + 2 * Gamma) * (Ca0 + 2 * Gamma) / (Ca0 - Gamma);
                if (delamaxCi < 0)
                    delamaxCi = 0;
                DelAmax[index] = delamaxCi;  // Modified Franks
                // M. Kubiske method for wue calculation:  Improved methods for calculating WUE and Transpiration in PnET.
                float V = (float)(8314.47 * (variables.Tmin + 273) / 101.3);
                float JCO2 = (float)(0.139 * ((variables.CO2 - ciElev) / V) * 0.000001);  // Corrected conversion units 11/29/22
                float JH2O = variables[species.Name].JH2O / ciModifier;  // Modified from * to / 11.18.2022 [mod1]
                float wue = JCO2 / JH2O * (44 / 18);  //44=mol wt CO2; 18=mol wt H2O; constant =2.44444444444444
                float Amax = (float)(delamaxCi * (speciesPnET.AmaxA + variables[species.Name].AmaxB_CO2 * adjFolN)); //nmole CO2/g Fol/s
                float BaseFoliarRespiration = variables[species.Name].BaseFoliarRespirationFrac * Amax; //nmole CO2/g Fol/s
                float AmaxAdj = Amax * speciesPnET.AmaxFrac;  //Amax adjustment as applied in PnET
                float GrossAmax = AmaxAdj + BaseFoliarRespiration; //nmole CO2/g Fol/s
                //Reference gross Psn (lab conditions) in gC/g Fol/month
                float RefGrossPsn = variables.DaySpan * (GrossAmax * variables[species.Name].DVPD * variables.Daylength * Constants.MC) / Constants.billion;
                // Calculate gross psn from stress factors and reference gross psn (gC/g Fol/month)
                // Reduction factors include temperature (PsnFTemp), water (FWater), light (FRad), age (FAge)
                // Remove FWater from psn reduction because it is accounted for in WUE through ciModifier [mod2, mod3]
                float GrossPsnPotential = 1 / (float)Globals.IMAX * variables[species.Name].PsnFTemp * FRad[index] * FAge * RefGrossPsn * Fol;  // gC/m2 ground/mo
                // M. Kubiske equation for transpiration: Improved methods for calculating WUE and Transpiration in PnET.
                // JH2O has been modified by CiModifier to reduce water use efficiency
                // Scale transpiration to fraction of site occupied (CanopyLayerFrac)
                // Corrected conversion factor                
                PotentialTranspiration[index] = (float)(0.0015f * (GrossPsnPotential / (JCO2 / JH2O))) * CanopyLayerFrac; //mm
                // It is possible for transpiration to calculate to exceed available water
                // In this case, we cap transpiration at available water, and back-calculate GrossPsn and NetPsn to downgrade those as well
                // Volumetric water content (mm/m) at species wilting point (h4) 
                // Convert kPA to mH2o (/9.804139432)
                float wiltPtWater = (float)hydrology.PressureHeadTable.CalcWaterContent(speciesPnET.H4 * 9.804139432f, siteCohort.Ecoregion.SoilType);
                float availableWater = (hydrology.Water - wiltPtWater) * siteCohort.Ecoregion.RootingDepth * frostFreeFrac;
                if (PotentialTranspiration[index] > availableWater)
                {
                    Transpiration[index] = (float)Math.Max(availableWater, 0f); //mm
                    if (CanopyLayerFrac > 0)
                        GrossPsn[index] = Transpiration[index] / 0.0015f * (JCO2 / JH2O) / CanopyLayerFrac;
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
                success = hydrology.AddWater(-1 * Transpiration[index], siteCohort.Ecoregion.RootingDepth * frostFreeFrac);
                if (success == false)
                    throw new System.Exception("Error adding water, Transpiration = " + Transpiration[index] + " water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
                if (hydrology.SurfaceWater > 0)
                {
                    float surfaceInput = Math.Min(hydrology.SurfaceWater, (siteCohort.Ecoregion.Porosity - hydrology.Water) * siteCohort.Ecoregion.RootingDepth * frostFreeFrac);
                    hydrology.SurfaceWater -= surfaceInput;
                    success = hydrology.AddWater(surfaceInput, siteCohort.Ecoregion.RootingDepth * frostFreeFrac);
                    if (success == false)
                        throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
                }
                // Net foliage respiration depends on reference psn (BaseFoliarRespiration)
                // Substitute 24 hours in place of DayLength because foliar respiration does occur at night.  BaseFoliarRespiration and FQ10 use Tavg temps reflecting both day and night temperatures.
                float RefFoliarRespiration = BaseFoliarRespiration * variables[species.Name].FQ10 * variables.DaySpan * (Constants.SecondsPerHour * 24) * Constants.MC / Constants.billion; // gC/g Fol/month
                // Actual foliage respiration (growth respiration) 
                FoliarRespiration[index] = RefFoliarRespiration * Fol / (float)Globals.IMAX; // gC/m2 ground/mo
                // NetPsn psn depends on gross psn and foliage respiration
                float nonOzoneNetPsn = GrossPsn[index] - FoliarRespiration[index];
                // Convert Psn gC/m2 ground/mo to umolCO2/m2 fol/s
                float netPsn_ground = nonOzoneNetPsn * 1000000F * (1F / 12F) * (1F / (variables.Daylength * variables.DaySpan));
                float netPsn_leaf_s = 0;
                if (netPsn_ground > 0 && LAI[index] > 0)
                {
                    netPsn_leaf_s = netPsn_ground * (1F / LAI[index]);
                    if (float.IsInfinity(netPsn_leaf_s))
                        netPsn_leaf_s = 0;
                }
                //Calculate water vapor conductance (gwv) from Psn and Ci; Kubiske Conductance_5.xlsx
                float gwv_mol = (float)(netPsn_leaf_s / Ca_Ci * 1.6 * 1000);
                float gwv = (float)(gwv_mol / (444.5 - 1.3667 * variables.Tavg) * 10);
                // Reduction factor for ozone on photosynthesis
                if (o3_month > 0)
                {
                    float o3Coeff = speciesPnET.O3GrowthSens;
                    O3Effect = CalcO3Effect_PnET(o3_month, delamaxCi, netPsn_leaf_s, subCanopyIndex, layerCount, Fol, lastO3Effect, gwv, LAI[index], o3Coeff);
                }
                else
                    O3Effect = 0;
                //Apply reduction factor for Ozone
                FOzone[index] = 1 - O3Effect;
                NetPsn[index] = nonOzoneNetPsn * FOzone[index];
                // Add net psn to non soluble carbons
                data.NSC += NetPsn[index]; //gC
                if (data.NSC < 0)
                    data.NSC = 0f;
            }
            else
            {
                // Reset subcanopy layer values
                NetPsn[index] = 0;
                FoliarRespiration[index] = 0;
                GrossPsn[index] = 0;
                Transpiration[index] = 0;
                PotentialTranspiration[index] = 0;
                FOzone[index] = 1;
            }
            index++;

            return success;
        }

        // LightEffect equation from PnET
        // Used in official releases >= 5.0
        public static float CalcFrad(float Radiation, float HalfSat)
        {
            float fRad = 0.0f;
            if (HalfSat > 0)
                fRad = (float)(1.0 - Math.Exp(-1.0 * Radiation * Math.Log(2.0) / HalfSat));
            else
                throw new System.Exception("HalfSat <= 0. Cannot calculate fRad.");

            return fRad;
        }

        public static float CalcFWater(float H1, float H2, float H3, float H4, float pressurehead)
        {
            float minThreshold = H1;
            if (H2 <= H1)
                minThreshold = H2;
            // Calculate water stress
            if (pressurehead <= H1)
                return 0;
            else if (pressurehead < minThreshold || pressurehead >= H4)
                return 0;
            else if (pressurehead > H3)
                return 1 - ((pressurehead - H3) / (H4 - H3));
            else if (pressurehead < H2)
                return 1.0F / (H2 - H1) * pressurehead - (H1 / (H2 - H1));
            else
                return 1;
        }

        public static float CalcO3Effect_PnET(float o3, float delAmax, float netPsn_leaf_s, int Layer, int nLayers, float FolMass, float lastO3Effect, float gwv, float layerLAI, float o3Coeff)
        {
            float currentO3Effect = 1.0F;
            float droughtO3Frac = 1.0F; // Not using droughtO3Frac from PnET code per M. Kubiske and A. Chappelka
            float kO3Eff = 0.0026F * o3Coeff;  // Scaled by species using input parameters
            float O3Prof = (float)(0.6163 + (0.00105 * FolMass));
            float RelLayer = (float)Layer / (float)nLayers;
            float relO3 = Math.Min(1, 1 - RelLayer * O3Prof * (RelLayer * O3Prof) * (RelLayer * O3Prof));
            // Kubiske method (using gwv in place of conductance
            currentO3Effect = (float)Math.Min(1, (lastO3Effect * droughtO3Frac) + (kO3Eff * gwv * o3 * relO3));

            return currentO3Effect;
        }

        public int CalcNonWoodyBiomass(ActiveSite site)
        {
            return (int)Fol;
        }

        public static Percentage CalcNonWoodyPercentage(Cohort cohort, ActiveSite site)
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
        /// Raises a Cohort.DeathEvent
        /// </summary>
        public static void Died(object sender, ICohort cohort, ActiveSite site, ExtensionType disturbanceType)
        {
            if (DeathEvent != null)
                DeathEvent(sender, new DeathEventArgs(cohort, site, disturbanceType));
        }

        /// <summary>
        /// Raises a Cohort.DeathEvent if partial mortality.
        /// </summary>
        public static void PartialMortality(object sender, ICohort cohort, ActiveSite site, ExtensionType disturbanceType, float reduction)
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
        /// Raises a Cohort.AgeOnlyDeathEvent
        /// </summary>
        public static void KilledByAgeOnlyDisturbance(object sender, ICohort cohort, ActiveSite site, ExtensionType disturbanceType)
        {
            if (AgeOnlyDeathEvent != null)
                AgeOnlyDeathEvent(sender, new DeathEventArgs(cohort, site, disturbanceType));
        }

        public void UpdateCohortData(IEcoregionPnETVariables monthdata)
        {
            float netPsnSum = NetPsn.Sum();
            float grossPsnSum = GrossPsn.Sum();
            float transpirationSum = Transpiration.Sum();
            float JCO2_JH2O = 0;
            if (transpirationSum > 0)
                JCO2_JH2O = (float)(0.0015f * grossPsnSum * CanopyLayerFrac / transpirationSum);
            float WUE = JCO2_JH2O * ((float)44 / (float)18); //44=mol wt CO2; 18=mol wt H2O; constant =2.44444444444444
            // determine the limiting factor 
            float fWaterAvg = FWater.Average();
            float PressHeadAvg = PressHead.Average();
            float fRadAvg = FRad.Average();
            float fOzoneAvg = FOzone.Average();
            float fTemp = monthdata[Species.Name].PsnFTemp;
            string limitingFactor = "NA";
            if (ColdKill < int.MaxValue)
                limitingFactor = "ColdTol (" + ColdKill.ToString() + ")";
            else
            {
                List<float> factorList = new List<float>(new float[] { fWaterAvg, fRadAvg, fOzoneAvg, Fage, fTemp });
                float minFactor = factorList.Min();
                if (minFactor == fTemp)
                    limitingFactor = "fTemp";
                else if (minFactor == FAge)
                    limitingFactor = "fAge";
                else if (minFactor == fWaterAvg)
                {
                    if (PressHeadAvg > this.SpeciesPnET.H3)
                        limitingFactor = "Too_dry";
                    else if (PressHeadAvg < this.SpeciesPnET.H2)
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
                       CanopyLayerFrac + "," +
                       CanopyGrowingSpace + "," +
                       SumLAI + "," +
                       SumLAI * CanopyLayerFrac + "," +
                       GrossPsn.Sum() + "," +
                       FoliarRespiration.Sum() + "," +
                       MaintenanceRespiration.Sum() + "," +
                       netPsnSum + "," +                  // Sum over canopy layers
                       transpirationSum + "," +
                       WUE.ToString() + "," +
                       Fol + "," +
                       Root + "," +
                       Wood + "," +
                       Fol * CanopyLayerFrac + "," +
                       Root * CanopyLayerFrac + "," +
                       Wood * CanopyLayerFrac + "," +
                       NSC + "," +
                       NSCfrac + "," +
                       fWaterAvg + "," +
                       Water.Average() + "," +
                       PressHead.Average() + "," +
                       fRadAvg + "," +
                       fOzoneAvg + "," +
                       DelAmax.Average() + "," +
                       monthdata[Species.Name].PsnFTemp + "," +
                       monthdata[Species.Name].FTempRespWeightedDayAndNight + "," +
                       FAge + "," +
                       IsLeafOn + "," +
                       FActiveBiom + "," +
                       AdjFolN.Average() + "," +
                       AdjFracFol.Average() + "," +
                       CiModifier.Average() + "," +
                       AdjHalfSat + "," +
                       limitingFactor + ",";
            cohortoutput.Add(s);
        }

        public string OutputHeader
        {
            get
            {
                // Cohort output file header
                string hdr = OutputHeaders.Time + "," +
                             OutputHeaders.Year + "," +
                             OutputHeaders.Month + "," +
                             OutputHeaders.Age + "," +
                             OutputHeaders.Layer + "," +
                             OutputHeaders.CanopyLayerFrac + "," +
                             OutputHeaders.CanopyGrowingSpace + "," +
                             OutputHeaders.LAI + "," +
                             OutputHeaders.LAISite + "," +
                             OutputHeaders.GrossPsn + "," +
                             OutputHeaders.FoliarRespiration + "," +
                             OutputHeaders.MaintResp + "," +
                             OutputHeaders.NetPsn + "," +
                             OutputHeaders.Transpiration + "," +
                             OutputHeaders.WUE + "," +
                             OutputHeaders.Fol + "," +
                             OutputHeaders.Root + "," +
                             OutputHeaders.Wood + "," +
                             OutputHeaders.FolSite + "," +
                             OutputHeaders.RootSite + "," +
                             OutputHeaders.WoodSite + "," +
                             OutputHeaders.NSC + "," +
                             OutputHeaders.NSCfrac + "," +
                             OutputHeaders.fWater + "," +
                             OutputHeaders.water + "," +
                             OutputHeaders.PressureHead + "," +
                             OutputHeaders.fRad + "," +
                             OutputHeaders.FOzone + "," +
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
            // If cohort is dead, then all foliage is lost
            if (NSCfrac <= 0.01F)
                Litter = Fol;
            Fol -= Litter;

            return Litter;
        }

        public float Senescence()
        {
            float senescence = (Root * speciesPnET.TOroot) + Wood * speciesPnET.TOwood;
            data.TotalBiomass -= senescence;
            data.AGBiomass = (1 - speciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;
            data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerFrac);
            data.BiomassMax = Math.Max(data.BiomassMax, data.TotalBiomass);

            return senescence;
        }

        public void ReduceFoliage(double frac)
        {
            Fol *= (float)(1.0 - frac);
            data.MaxFolYear = Math.Max(data.MaxFolYear, Fol);
        }

        public void ResetFoliageMax()
        {
            data.MaxFolYear = 0;
        }

        public void ReduceBiomass(object sitecohorts, double frac, ExtensionType disturbanceType)
        {
            if (!((SiteCohorts)sitecohorts).DisturbanceTypesReduced.Contains(disturbanceType))
            {
                Allocation.ReduceDeadPools(sitecohorts, disturbanceType);  // Reduce dead pools before adding through Allocation
                ((SiteCohorts)sitecohorts).DisturbanceTypesReduced.Add(disturbanceType);
            }
            Allocation.Allocate(sitecohorts, this, disturbanceType, frac);
            data.TotalBiomass *= (float)(1.0 - frac);
            data.AGBiomass = (1 - speciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;
            data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerFrac);
            data.BiomassMax = Math.Max(data.BiomassMax, data.TotalBiomass);
            Fol *= (float)(1.0 - frac);
            data.MaxFolYear = Math.Max(data.MaxFolYear, Fol);
        }

        public float CalcLAI(ISpeciesPnET species, float fol, int index)
        {
            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            float LAISum = 0;
            if (LAI != null)
            {
                for (int i = 0; i < index; i++)
                {
                    LAISum += LAI[i];
                }
            }
            float LAIlayerMax = (float)Math.Max(0.01, 25.0F - LAISum); // Cohort LAI is capped at 25; once LAI reaches 25 subsequent sublayers get LAI of 0.01
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

        public float CalcLAI(ISpeciesPnET species, float fol, int index, float cumulativeLAI)
        {
            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            float LAISum = cumulativeLAI;
            float LAIlayerMax = (float)Math.Max(0.01, 25.0F - LAISum); // Cohort LAI is capped at 25; once LAI reaches 25 subsequent sublayers get LAI of 0.01
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
        public static void RaiseDeathEvent(object sender, Cohort cohort, ActiveSite site, ExtensionType disturbanceType)
        {
            if (DeathEvent != null)
                DeathEvent(sender, new DeathEventArgs(cohort, site, disturbanceType));
        }

        public void ChangeParameters(ExpandoObject additionalParams)
        {
            return;
        }
    }
}

// authors: 

// NOTE: ActiveSite --> Landis.SpatialModeling

// uses dominance to allocate psn and subtract transpiration 
// from soil water, average cohort vars over layer

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Landis.Core;
using Landis.Library.UniversalCohorts;
using Landis.SpatialModeling;

namespace Landis.Library.PnETCohorts
{
    public class Cohort : Library.UniversalCohorts.Cohort, ICohort
    { 
        public delegate void SubtractTranspiration(float transpiration, IPnETSpecies Species);
        public ushort index;
        private Landis.Core.ISpecies species;
        private IPnETSpecies PnETspecies;
        private CohortData data;
        private bool firstYear;
        private LocalOutput cohortoutput;

        /// <summary>
        /// Age (y)
        /// </summary>
        public ushort Age
        {
            get
            {
                return data.UniversalData.Age;
            }
        }

        /// <summary>
        /// Succession timestep used by biomass cohorts (yrs)
        /// </summary>
        public ushort SuccessionTimestep
        {
            get
            {
                return data.SuccessionTimestep;
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
        /// Measure of cohort's diffuse reflection of solar radiation 
        /// out of total solar radiation, without snow reflectance
        /// </summary>
        public float Albedo
        {
            get
            {
                float albedo = 0;
                bool lifeform = !string.IsNullOrEmpty(PnETSpecies.Lifeform);
                bool ground = PnETSpecies.Lifeform.ToLower().Contains("ground");
                bool open = PnETSpecies.Lifeform.ToLower().Contains("open");
                bool dark = PnETSpecies.Lifeform.ToLower().Contains("dark");
                bool light = PnETSpecies.Lifeform.ToLower().Contains("light");
                bool deciduous = PnETSpecies.Lifeform.ToLower().Contains("decid");
                if (lifeform && (ground || open || SumLAI == 0))
                    albedo = 0.20F;
                else if (lifeform && dark)
                    albedo = (float)((-0.067 * Math.Log(SumLAI < 0.7 ? 0.7 : SumLAI)) + 0.2095);
                else if (lifeform && light)
                    albedo = (float)((-0.054 * Math.Log(SumLAI < 0.7 ? 0.7 : SumLAI)) + 0.2082);
                else if (lifeform && deciduous)
                    albedo = (float)((-0.0073 * SumLAI) + 0.231);
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
                return data.UniversalData.Biomass * PnETspecies.MossScalar;
            }
        }

        /// <summary>
        /// Aboveground Biomass (g/m2)
        /// </summary>
        public int AGBiomass
        {
            get
            {
                return (int)(Math.Round(PnETspecies.AGBiomassFrac * data.TotalBiomass) + data.Fol);
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
                return (uint)Math.Round(PnETspecies.AGBiomassFrac * data.TotalBiomass);
            }
        }

        /// <summary>
        /// Root (g/m2)
        /// </summary>
        public uint Root
        {
            get
            {
                return (uint)Math.Round(PnETspecies.BGBiomassFrac * data.TotalBiomass);
            }
        }

        /// <summary>
        /// Max biomass achived in the cohorts' life time. 
        /// This value remains high after the cohort has reached its 
        /// peak biomass. It is used to determine canopy layers where
        /// it prevents that a cohort could descent in the canopy when 
        /// it declines (g/m2)
        /// </summary>
        public float MaxBiomass
        {
            get
            {
                return data.MaxBiomass;
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
        public void AccumulateWoodSenescence(int senescence)
        {
            data.LastWoodSenescence += senescence;
        }

        /// <summary>
        /// Add dead foliage to last senescence
        /// </summary>
        /// <param name="senescence"></param>
        public void AccumulateFolSenescence(int senescence)
        {
            data.LastFolSenescence += senescence;
        }

        /// <summary>
        /// Growth reduction factor for age
        /// </summary>
        float FAge
        {
            get
            {
                return Math.Max(0, 1 - (float)Math.Pow(Age / (float)PnETspecies.Longevity, PnETspecies.PhotosynthesisFAge));
            }
        }

        /// <summary>
        /// NSC fraction: measure for resources
        /// </summary>
        public float NSCfrac
        {
            get
            {
                return NSC / (FActiveBiom * (data.TotalBiomass + Fol) * PnETSpecies.CFracBiomass);
            }
        }

        /// <summary>
        /// Species with PnET parameter additions
        /// </summary>
        public IPnETSpecies PnETSpecies
        {
            get
            {
                return PnETspecies;
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
        /// Annual Wood Senescence (g/m2)
        /// </summary>
        public float LastWoodSenescence
        {
            get
            {
                return data.LastWoodSenescence;
            }
            set
            {
                data.LastWoodSenescence = value;
            }
        }

        /// <summary>
        /// Annual Foliage Senescence (g/m2)
        /// </summary>
        public float LastFolSenescence
        {
            get
            {
                return data.LastFolSenescence;
            }
            set
            {
                data.LastFolSenescence = value;
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

        public float adjFolBiomassFrac
        {
            get
            {
                return data.adjFolBiomassFrac;
            }
        }

        public float[] AdjFolBiomassFrac
        {
            get
            {
                return data.AdjFolBiomassFrac;
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

        public float[] SoilWaterContent
        {
            get
            {
                return data.SoilWaterContent;
            }
        }

        public int[] NumPrecipEvents
        {
            get
            {
                return data.NumPrecipEvents;
            }
        }

        public float FActiveBiom
        {
            get
            {
                return (float)Math.Exp(-PnETspecies.LiveWoodBiomassFrac * data.MaxBiomass);
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
            data.SoilWaterContent = new float[Globals.IMAX];
            data.PressHead = new float[Globals.IMAX];
            data.NumPrecipEvents = new int[Globals.IMAX];
            data.FOzone = new float[Globals.IMAX];
            data.MaintenanceRespiration = new float[Globals.IMAX];
            data.Interception = new float[Globals.IMAX];
            data.AdjFolN = new float[Globals.IMAX];
            data.AdjFolBiomassFrac = new float[Globals.IMAX];
            data.CiModifier = new float[Globals.IMAX];
            data.DelAmax = new float[Globals.IMAX];
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
            data.NumPrecipEvents = null;
            data.SoilWaterContent = null;
            data.FOzone = null;
            data.MaintenanceRespiration = null;
            data.Interception = null;
            data.AdjFolN = null;
            data.AdjFolBiomassFrac = null;
            data.CiModifier = null;
            data.DelAmax = null;
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

        public void SetAvgFRad(float lastAvgFRad)
        {
            data.LastSeasonFRad.Add(lastAvgFRad);
        }

        public void ClearFRad()
        {
            data.LastSeasonFRad = new List<float>();
        }

        public void CalcAdjFolBiomassFrac()
        {
            if (data.LastSeasonFRad.Count() > 0)
            {
                float lastSeasonAvgFRad = data.LastSeasonFRad.ToArray().Average();
                float folBiomassFrac_slope = PnETspecies.FolBiomassFrac_slope;
                float folBiomassFrac_int = PnETspecies.FolBiomassFrac_intercept;
                //slope is shape parm; folBiomassFrac is minFolBiomassFrac; int is folBiomassFrac_intercept. EJG-7-24-18
                data.adjFolBiomassFrac = PnETspecies.FolBiomassFrac + ((folBiomassFrac_int - PnETspecies.FolBiomassFrac) * (float)Math.Pow(lastSeasonAvgFRad, folBiomassFrac_slope)); 
                firstYear = false;
            }
            else
                data.adjFolBiomassFrac = PnETspecies.FolBiomassFrac;
        }

        /// <summary>
        /// Get totals across several cohorts
        /// </summary>
        /// <param name="cohort"></param>
        public void Accumulate(Cohort cohort)
        {
            data.TotalBiomass += cohort.TotalBiomass;
            data.MaxBiomass = Math.Max(MaxBiomass, data.TotalBiomass);
            data.Fol += cohort.Fol;
            data.MaxFolYear = Math.Max(MaxFolYear, data.Fol);
            data.AGBiomass = cohort.PnETSpecies.AGBiomassFrac * data.TotalBiomass + data.Fol;
            data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerFrac);
            data.UniversalData.ANPP += cohort.ANPP;
        }

        /// <summary>
        /// Increment the cohort age by one year.
        /// </summary>
        public void IncrementAge()
        {
            data.UniversalData.Age += 1;
        }

        /// <summary>
        /// Calculate the cohort biomass change.
        /// </summary>
        public int CalcBiomassChange()
        {
            int dBiomass = (int)data.AGBiomass - (int)data.LastAGBio;
            return dBiomass;
        }

        /// <summary>
        /// Change the cohort biomass.
        /// </summary>
        public void ChangeBiomass(int dBiomass)
        {
            float newTotalBiomass = data.TotalBiomass + dBiomass;
            data.TotalBiomass = Math.Max(0, newTotalBiomass);
            data.AGBiomass = PnETSpecies.AGBiomassFrac * data.TotalBiomass + data.Fol;
            data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerFrac);
            data.MaxBiomass = Math.Max(data.MaxBiomass, data.TotalBiomass);
        }

        /// <summary>
        /// Change the cohort ANPP.
        /// </summary>
        public void ChangeANPP(double dANPP)
        {
            data.UniversalData.ANPP = data.UniversalData.ANPP + dANPP;
        }

        /// <summary>
        /// Constructor #1
        /// </summary>
        /// <param name="species"></param>
        /// <param name="PnETspecies"></param>
        /// <param name="establishmentYear"></param>
        /// <param name="SiteName"></param>
        /// <param name="fracBiomass"></param>
        /// <param name="cohortStacking"></param>
        public Cohort(Landis.Core.ISpecies species, IPnETSpecies PnETspecies, ushort establishmentYear, string SiteName, double fracBiomass, bool cohortStacking, ushort successionTimestep)
        {
            this.species = species;
            this.PnETspecies = PnETspecies;
            data.SuccessionTimestep = successionTimestep;
            data.UniversalData.Age = 1;
            data.ColdKill = int.MaxValue;
            data.NSC = (ushort)PnETspecies.InitialNSC;
            // Initialize biomass assuming fixed concentration of NSC, convert gC to gDW
            data.TotalBiomass = (uint)Math.Max(1.0, NSC / (PnETspecies.NSCFrac * PnETspecies.CFracBiomass) * fracBiomass);
            data.AGBiomass = PnETspecies.AGBiomassFrac * data.TotalBiomass + data.Fol;
            data.MaxBiomass = data.TotalBiomass;
            float cohortIdealFol = PnETspecies.FolBiomassFrac * FActiveBiom * data.TotalBiomass;
            float cohortLAI = Canopy.CalcCohortLAI(PnETSpecies, cohortIdealFol);
            data.LastLAI = cohortLAI;
            data.LastAGBio = data.AGBiomass;
            data.CanopyLayerFrac = data.LastLAI / PnETspecies.MaxLAI;
            if (cohortStacking)
                data.CanopyLayerFrac = 1.0f;
            data.CanopyGrowingSpace = 1.0f;
            data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerFrac);
            data.UniversalData.ANPP = data.UniversalData.Biomass;
            // Then overwrite them if needed for output
            if (SiteName != null)
                InitializeOutput(SiteName, establishmentYear);
            data.LastSeasonFRad = new List<float>();
            firstYear = true;
        }

        /// <summary>
        /// Cohort constructor #2
        /// </summary>
        /// <param name="species"></param>
        /// <param name="cohortData"></param>
        public Cohort(Landis.Core.ISpecies species, CohortData cohortData)
        {
            this.species = species;
            PnETspecies = SpeciesParameters.PnETSpecies.AllSpecies[species.Index];
            data = cohortData;
        }

        /// <summary>
        /// Cohort constructor #3 (cloning an existing cohort)
        /// </summary>
        /// <param name="cohort"></param>
        public Cohort(Cohort cohort)
        {
            species = cohort.Species;
            PnETspecies = cohort.PnETspecies;
            data.SuccessionTimestep = cohort.SuccessionTimestep;
            data.UniversalData.Age = cohort.Age;
            data.NSC = cohort.NSC;
            data.TotalBiomass = cohort.TotalBiomass;
            data.AGBiomass = cohort.PnETSpecies.AGBiomassFrac * cohort.TotalBiomass + cohort.Fol;
            data.UniversalData.Biomass = (int)(data.AGBiomass * cohort.CanopyLayerFrac);
            data.MaxBiomass = cohort.MaxBiomass;
            data.Fol = cohort.Fol;
            data.MaxFolYear = cohort.MaxFolYear;
            data.LastSeasonFRad = cohort.data.LastSeasonFRad;
            data.ColdKill = int.MaxValue;
            data.UniversalData.ANPP = cohort.ANPP;
        }

        /// <summary>
        /// Cohort constructor #4
        /// </summary>
        /// <param name="cohort"></param>
        /// <param name="establishmentYear"></param>
        /// <param name="SiteName"></param>
        public Cohort(Cohort cohort, ushort establishmentYear, string SiteName)
        {
            species = cohort.Species;
            PnETspecies = cohort.PnETspecies;
            data.SuccessionTimestep = cohort.SuccessionTimestep;
            data.UniversalData.Age = cohort.Age;
            data.NSC = cohort.NSC;
            data.TotalBiomass = cohort.TotalBiomass;
            data.AGBiomass = cohort.PnETSpecies.AGBiomassFrac * cohort.TotalBiomass + cohort.Fol;
            data.UniversalData.Biomass = (int)(data.AGBiomass * cohort.CanopyLayerFrac);
            data.MaxBiomass = cohort.MaxBiomass;
            data.Fol = cohort.Fol;
            data.MaxFolYear = cohort.MaxFolYear;
            data.LastSeasonFRad = cohort.data.LastSeasonFRad;
            data.ColdKill = int.MaxValue;
            data.UniversalData.ANPP = cohort.ANPP;
            if (SiteName != null)
                InitializeOutput(SiteName, establishmentYear);
        }

        /// <summary>
        /// Cohort constructor #5
        /// </summary>
        /// <param name="PnETspecies"></param>
        /// <param name="age"></param>
        /// <param name="woodBiomass"></param>
        /// <param name="SiteName"></param>
        /// <param name="establishmentYear"></param>
        /// <param name="cohortStacking"></param>
        public Cohort(IPnETSpecies PnETspecies, ushort age, int woodBiomass, string SiteName, ushort establishmentYear, bool cohortStacking, ushort successionTimestep)
        {
            InitializeSubLayers();
            species = (Landis.Core.ISpecies)PnETspecies;
            this.PnETspecies = PnETspecies;
            data.SuccessionTimestep = successionTimestep;
            data.UniversalData.Age = age;
            // incoming biomass is aboveground wood, calculate total biomass
            float biomass = woodBiomass / PnETspecies.AGBiomassFrac;
            data.TotalBiomass = biomass;
            data.MaxBiomass = biomass;
            data.LastSeasonFRad = new List<float>();
            data.adjFolBiomassFrac = PnETspecies.FolBiomassFrac_intercept;
            data.ColdKill = int.MaxValue;
            float cohortIdealFol = PnETspecies.FolBiomassFrac_intercept * FActiveBiom * data.TotalBiomass;
            float cohortLAI = 0;
            for (int i = 0; i < Globals.IMAX; i++)
            {
                float LAISum = Canopy.CalcLAISum(i, LAI);
                float subLayerLAI = Canopy.CalcLAI(PnETSpecies, cohortIdealFol, i, LAISum);
                cohortLAI += subLayerLAI;
                if (IsLeafOn)
                    LAI[index] = subLayerLAI;
            }
            if (IsLeafOn)
            {
                data.Fol = cohortIdealFol;
                data.MaxFolYear = cohortIdealFol;
            }
            data.LastLAI = cohortLAI;
            data.CanopyLayerFrac = data.LastLAI / PnETspecies.MaxLAI;
            if (cohortStacking)
                data.CanopyLayerFrac = 1.0f;
            data.CanopyGrowingSpace = 1.0f;
            data.AGBiomass = this.PnETspecies.AGBiomassFrac * data.TotalBiomass + data.Fol;
            data.LastAGBio = data.AGBiomass;
            data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerFrac);
            data.NSC = this.PnETspecies.NSCFrac * FActiveBiom * (data.TotalBiomass + data.Fol) * PnETspecies.CFracBiomass;
            if (SiteName != null)
                InitializeOutput(SiteName, establishmentYear);
        }

        /// <summary>
        /// Cohort constructor #6
        /// </summary>
        /// <param name="PnETspecies"></param>
        /// <param name="age"></param>
        /// <param name="woodBiomass"></param>
        /// <param name="maxBiomass"></param>
        /// <param name="canopyGrowingSpace"></param>
        /// <param name="SiteName"></param>
        /// <param name="establishmentYear"></param>
        /// <param name="cohortStacking"></param>
        /// <param name="lastSeasonAvgFRad"></param>
        public Cohort(IPnETSpecies PnETspecies, ushort age, int woodBiomass, int maxBiomass, float canopyGrowingSpace, string SiteName, ushort establishmentYear, bool cohortStacking, ushort successionTimestep, float lastSeasonAvgFRad)
        {
            InitializeSubLayers();
            species = (Landis.Core.ISpecies)PnETspecies;
            this.PnETspecies = PnETspecies;
            data.SuccessionTimestep = successionTimestep;
            data.UniversalData.Age = age;
            // incoming biomass is aboveground wood, calculate total biomass
            float biomass = woodBiomass / PnETspecies.AGBiomassFrac;
            data.TotalBiomass = biomass;
            data.MaxBiomass = Math.Max(biomass, maxBiomass);
            data.LastSeasonFRad = new List<float>();
            data.LastSeasonFRad.Add(lastSeasonAvgFRad);
            CalcAdjFolBiomassFrac();
            data.ColdKill = int.MaxValue;
            float cohortIdealFol = adjFolBiomassFrac * FActiveBiom * data.TotalBiomass;
            float cohortLAI = 0;
            for (int i = 0; i < Globals.IMAX; i++)
            {
                float LAISum = Canopy.CalcLAISum(i, LAI);
                float subLayerLAI = Canopy.CalcLAI(PnETSpecies, cohortIdealFol, i, LAISum);
                cohortLAI += subLayerLAI;
                if (IsLeafOn)
                    LAI[index] = subLayerLAI;
            }
            if (IsLeafOn)
            {
                data.Fol = cohortIdealFol;
                data.MaxFolYear = cohortIdealFol;
            }
            data.LastLAI = cohortLAI;
            data.CanopyLayerFrac = data.LastLAI / PnETspecies.MaxLAI;
            if (cohortStacking)
                data.CanopyLayerFrac = 1.0f;
            data.CanopyGrowingSpace = 1.0f;
            data.AGBiomass = this.PnETspecies.AGBiomassFrac * data.TotalBiomass + data.Fol;
            data.LastAGBio = data.AGBiomass;
            data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerFrac);
            data.NSC = this.PnETspecies.NSCFrac * FActiveBiom * (data.TotalBiomass + data.Fol) * PnETspecies.CFracBiomass;
            if (SiteName != null)
                InitializeOutput(SiteName, establishmentYear);
        }

        public void CalcDefoliationFrac(ActiveSite site, int SiteAGBiomass)
        {
            lock (Globals.DistributionThreadLock)
            {
                data.DefoliationFrac = (float)Library.UniversalCohorts.CohortDefoliation.Compute(site, this, 0, SiteAGBiomass);
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
        /// <param name="SubCanopyPAR"></param>
        /// <param name="o3_cum"></param>
        /// <param name="o3_month"></param>
        /// <param name="subCanopyIndex"></param>
        /// <param name="layerCount"></param>
        /// <param name="fOzone"></param>
        /// <param name="frostFreeFrac"></param>
        /// <param name="MeltInByCanopyLayer"></param>
        /// <param name="coldKillBoolean"></param>
        /// <param name="variables"></param>
        /// <param name="siteCohort"></param>
        /// <param name="sumCanopyFrac"></param>
        /// <param name="groundPotentialETbyEvent"></param>
        /// <param name="allowMortality"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool CalcPhotosynthesis(float PrecInByCanopyLayer, int precipCount, float leakageFrac, ref Hydrology hydrology, float mainLayerPAR, ref float SubCanopyPAR, float o3_cum, float o3_month, int subCanopyIndex, int layerCount, ref float fOzone, float frostFreeFrac, float snowpack, float MeltInByCanopyLayer, bool coldKillBoolean, IPnETEcoregionVars variables, SiteCohorts siteCohort, float sumCanopyFrac, float groundPotentialETbyEvent, bool allowMortality = true)
        {
            bool success = true;
            float lastFOzone = fOzone;
            fOzone = 0;
            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            float LAISum = Canopy.CalcLAISum(index, LAI);
            data.LAI[index] = Canopy.CalcLAI(PnETspecies, data.Fol, index, LAISum);
            if (MeltInByCanopyLayer > 0)
            {
                // Instantaneous runoff due to snowmelt (excess of soilPorosity)
                Hydrology.CalcRunoff(hydrology, siteCohort.Ecoregion, MeltInByCanopyLayer, frostFreeFrac, siteCohort.Site.Location);
                // Fast Leakage
                Hydrology.CalcLeakage(hydrology, siteCohort.Ecoregion, leakageFrac, frostFreeFrac, siteCohort.Site.Location);
            }
            if (PrecInByCanopyLayer > 0)
            {
                // If more than one precip event assigned to layer, repeat precip, runoff, leakage for all events prior to respiration
                for (int p = 1; p <= precipCount; p++)
                {
                    // Instantaneous runoff due to rain (excess of soilPorosity)
                    Hydrology.CalcRunoff(hydrology, siteCohort.Ecoregion, PrecInByCanopyLayer, frostFreeFrac, siteCohort.Site.Location);
                }
            }
            // Evaporation
            Hydrology.CalcSoilEvaporation(hydrology, siteCohort.Ecoregion, snowpack, frostFreeFrac, groundPotentialETbyEvent, siteCohort.Site.Location);
            // Infiltration (let captured surface water soak into soil)
            Hydrology.CalcInfiltration(hydrology, siteCohort.Ecoregion, frostFreeFrac, siteCohort.Site.Location);
            // Fast Leakage
            Hydrology.CalcLeakage(hydrology, siteCohort.Ecoregion, leakageFrac, frostFreeFrac, siteCohort.Site.Location);
            // Maintenance respiration depends on biomass,  non soluble carbon and temperature
            data.MaintenanceRespiration[index] = 1 / (float)Globals.IMAX * (float)Math.Min(NSC, variables[Species.Name].MaintenanceRespirationFTemp * (data.TotalBiomass * PnETspecies.CFracBiomass));//gC //IMAXinverse
            // Subtract mainenance respiration (gC/mo)
            data.NSC -= MaintenanceRespiration[index];
            if (data.NSC < 0)
                data.NSC = 0f;
            // Wood decomposition: do once per year to reduce unnescessary computation time so with the last subcanopy layer 
            if (index == Globals.IMAX - 1)
            {
                // In the last month
                if (variables.Month == (int)Calendar.Months.December)
                {
                    if (allowMortality)
                    {
                        // Check if nscfrac is below threshold to determine if cohort is alive
                        // if cohort is dead, nsc goes to zero and becomes functionally dead even though not removed until end of timestep
                        if (!IsAlive)
                            data.NSC = 0.0F;
                        else if (Globals.ModelCore.CurrentTime > 0 && TotalBiomass < (uint)PnETspecies.InitBiomass)  // Check if biomass < Initial Biomass -> cohort dies
                        {
                            data.NSC = 0.0F;
                            data.IsLeafOn = false;
                            data.NSC = 0.0F;
                            float folSenescence = FolSenescence();
                            data.LastFolSenescence = folSenescence;
                            siteCohort.AddLeafLitter(folSenescence * data.CanopyLayerFrac, PnETSpecies.FolLignin); // Using Canopy fractioning
                        }
                    }
                    float woodSenescence = WoodSenescence();
                    data.LastWoodSenescence = woodSenescence;
                    siteCohort.AddWoodDebris(woodSenescence * data.CanopyLayerFrac, PnETspecies.WoodDebrisDecompRate); // Using Canopy fractioning
                    // Release of NSC, will be added to biomass components next year
                    // Assumed that NSC will have a minimum concentration, excess is allocated to biomass
                    float NSCallocation = Math.Max(NSC - (PnETspecies.NSCFrac * FActiveBiom * data.TotalBiomass * PnETspecies.CFracBiomass), 0);
                    data.TotalBiomass += NSCallocation / PnETspecies.CFracBiomass;  // convert gC to gDW
                    data.AGBiomass = PnETspecies.AGBiomassFrac * data.TotalBiomass + data.Fol;
                    data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerFrac);
                    data.MaxBiomass = Math.Max(MaxBiomass, data.TotalBiomass);
                    data.NSC -= NSCallocation;
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
                    float folSenescence = FolSenescence();
                    data.LastFolSenescence = folSenescence;
                    siteCohort.AddLeafLitter(folSenescence * data.CanopyLayerFrac, PnETSpecies.FolLignin); // Using Canopy fractioning
                }
                else
                {
                    // When LeafOn becomes false for the first time in a year
                    if (variables.Tmin <= PnETSpecies.LeafOnMinT)
                    {
                        if (data.IsLeafOn == true)
                        {
                            data.IsLeafOn = false;
                            float folSenescence = FolSenescence();
                            data.LastFolSenescence = folSenescence;
                            siteCohort.AddLeafLitter(folSenescence * data.CanopyLayerFrac, PnETSpecies.FolLignin); // Using Canopy fractioning
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
                    {
                        Fol = Disturbance.ReduceFoliage(Fol, data.DefoliationFrac);
                        data.MaxFolYear = Math.Max(data.MaxFolYear, Fol);
                    }
                    else
                    {
                        if (firstYear)
                            data.adjFolBiomassFrac = PnETspecies.FolBiomassFrac_intercept;
                        // Foliage linearly increases with active biomass
                        float IdealFol = adjFolBiomassFrac * FActiveBiom * data.TotalBiomass; // Using adjusted FolBiomassFrac
                        float NSClimit = data.NSC;
                        if (mainLayerPAR < variables.PAR0) // indicates below the top layer
                        {
                            // lower canopy layers can retain a reserve of NSC (NSCReserve) which limits NSC available for refoliation - default is no reserve (NSCReserve = 0)
                            NSClimit = data.NSC - (PnETspecies.NSCReserve * (FActiveBiom * (data.TotalBiomass + data.Fol) * PnETspecies.CFracBiomass));
                        }
                        float FolCost = 0;
                        float FolTentative = 0;
                        if (growMonth < 2)  // Growing season months before defoliation outbreaks - can add foliage in first growing season month
                        {
                            if (IdealFol > data.Fol)
                            {
                                // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                                // carbon fraction of biomass to convert C to DW
                                FolCost = Math.Max(0, Math.Min(NSClimit, PnETspecies.CFracBiomass * (IdealFol - Fol))); // gC/mo
                                // Add foliage allocation to foliage
                                FolTentative = FolCost / PnETspecies.CFracBiomass;// gDW
                            }
                            data.LastLAI = 0;
                        }
                        else if (growMonth == 3) // Refoliation can occur in the 3rd growing season month
                        {
                            if (data.DefoliationFrac > 0)  // Only defoliated cohorts can add refoliate
                            {
                                if (data.DefoliationFrac > PnETspecies.RefoliationMinimumTrigger)  // Refoliation threshold is variable
                                {
                                    // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                                    // carbon fraction of biomass to convert C to DW
                                    float Folalloc = Math.Max(0f, Math.Min(NSClimit, PnETspecies.CFracBiomass * ((PnETspecies.MaxRefoliationFrac * IdealFol) - Fol)));  // variable refoliation
                                    FolCost = Math.Max(0f, Math.Min(NSClimit, PnETspecies.CFracBiomass * (PnETspecies.RefoliationCost * IdealFol - Fol)));  // cost of refol is the cost of getting to variable propotion of IdealFol
                                    FolTentative = Folalloc / PnETspecies.CFracBiomass;// gDW
                                }
                                else // No attempted refoliation but carbon loss after defoliation
                                {
                                    // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                                    // carbon fraction of biomass to convert C to DW
                                    FolCost = Math.Max(0f, Math.Min(NSClimit, PnETspecies.CFracBiomass * (PnETspecies.NonRefoliationCost * IdealFol))); // gC/mo variable fraction of IdealFol to take out NSC 
                                }
                            }
                            // Non-defoliated trees do not add to their foliage
                        }
                        if (FolTentative > 0.01)
                        {
                            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
                            float tentativeLAI = Canopy.CalcCohortLAI(PnETSpecies, Fol + FolTentative);
                            float tentativeCanopyFrac = tentativeLAI / PnETspecies.MaxLAI;
                            if (sumCanopyFrac > 1)
                                tentativeCanopyFrac /= sumCanopyFrac;
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
            LAISum = Canopy.CalcLAISum(index, LAI);
            data.LAI[index] = Canopy.CalcLAI(PnETspecies, Fol, index, LAISum);
            // Adjust HalfSat for CO2 effect
            data.AdjHalfSat = Photosynthesis.CalcAdjHalfSat(variables.CO2, PnETspecies.HalfSat, PnETspecies.HalfSatFCO2);
            // Reduction factor for radiation on photosynthesis
            float LayerPAR = (float)(mainLayerPAR * Math.Exp(-PnETspecies.K * (LAI.Sum() - LAI[index])));
            FRad[index] = Photosynthesis.CalcFRad(LayerPAR, AdjHalfSat);
            // Get pressure head given ecoregion and soil water content (latter in hydrology)
            float PressureHead = hydrology.PressureHeadTable.CalcSoilWaterPressureHead(hydrology.SoilWaterContent, siteCohort.Ecoregion.SoilType);
            // Reduction water for sub or supra optimal soil water content
            float FWaterOzone = 1.0f;  // fWater for ozone functions; ignores H1 and H2 parameters because only impacts when drought-stressed
            SoilWaterContent[index] = hydrology.SoilWaterContent;
            PressHead[index] = PressureHead;
            NumPrecipEvents[index] = precipCount;
            if (Globals.ModelCore.CurrentTime > 0)
            {
                FWater[index] = Photosynthesis.CalcFWater(PnETspecies.H1, PnETspecies.H2, PnETspecies.H3, PnETspecies.H4, PressureHead);
                FWaterOzone = Photosynthesis.CalcFWater(-1, -1, PnETspecies.H3, PnETspecies.H4, PressureHead); // ignores H1 and H2 parameters because only impacts when drought-stressed
            }
            else // Spinup
            {
                if (Names.GetParameter(Names.SpinUpWaterStress).Value == "true"
                    || Names.GetParameter(Names.SpinUpWaterStress).Value == "yes")
                {
                    FWater[index] = Photosynthesis.CalcFWater(PnETspecies.H1, PnETspecies.H2, PnETspecies.H3, PnETspecies.H4, PressureHead);
                    FWaterOzone = Photosynthesis.CalcFWater(-1, -1, PnETspecies.H3, PnETspecies.H4, PressureHead); // ignores H1 and H2 parameters because only impacts when drought-stressed
                }
                else // Ignore H1 and H2 parameters during spinup
                {
                    FWater[index] = Photosynthesis.CalcFWater(-1, -1, PnETspecies.H3, PnETspecies.H4, PressureHead);
                    FWaterOzone = FWater[index];
                }
            }
            if (frostFreeFrac <= 0)
            {
                FWater[index] = 0;
                FWaterOzone = 0;
            }
            data.adjFolN = Photosynthesis.CalcAdjFolN(PnETspecies.FolN_slope, PnETspecies.FolN_intercept, PnETspecies.FolN, FRad[index]);
            AdjFolN[index] = adjFolN;  // Stored for output
            AdjFolBiomassFrac[index] = adjFolBiomassFrac; // Stored for output
            float ciModifier = Photosynthesis.CalcCiModifier(o3_cum, PnETspecies.StomataO3Sensitivity, FWaterOzone);
            CiModifier[index] = ciModifier;  // Stored for output
            // If trees are physiologically active
            if (IsLeafOn)
            {
                // CO2 ratio internal to the leaf versus external
                float cicaRatio = (-0.075f * adjFolN) + 0.875f;
                // Elevated leaf internal CO2 concentration
                float ciElev = Photosynthesis.CalcCiElev(cicaRatio, ciModifier, variables.CO2);
                // modified Franks method (2013, New Phytologist, 197:1077-1094)
                float delamaxCi = Photosynthesis.CalcDelAmaxCi(ciElev);
                DelAmax[index] = delamaxCi;
                // M. Kubiske method for wue calculation: Improved methods for calculating WUE and Transpiration in PnET.
                float JCO2_JH2O = Photosynthesis.CalcJCO2_JH2O(variables[species.Name].JH2O, variables.Tmin, variables.CO2, ciElev, ciModifier);
                float wue = JCO2_JH2O * Constants.MCO2_MC;
                // Calculate potential gross photosynthesis
                float Amax = (float)(delamaxCi * (PnETspecies.AmaxA + variables[species.Name].AmaxB_CO2 * adjFolN)); // nmole CO2/g Fol/s
                float BaseFoliarRespiration = variables[species.Name].BaseFoliarRespirationFrac * Amax; // nmole CO2/g Fol/s
                float AmaxAdj = Amax * PnETspecies.AmaxAmod;  // Amax adjustment as applied in PnET
                float GrossPsnPotential = Photosynthesis.CalcPotentialGrossPsn(AmaxAdj, BaseFoliarRespiration, variables.DaySpan, variables[species.Name].DVPD, variables.DayLength, variables[species.Name].PsnFTemp, FRad[index], FAge, Fol);
                // M. Kubiske equation for transpiration: Improved methods for calculating WUE and Transpiration in PnET.
                // JH2O has been modified by CiModifier to reduce water use efficiency
                // Scale transpiration to fraction of site occupied (CanopyLayerFrac)
                // Corrected conversion factor                
                PotentialTranspiration[index] = (float)(0.0015f * (GrossPsnPotential / JCO2_JH2O)) * CanopyLayerFrac; //mm
                // It is possible for transpiration to calculate to exceed available water
                // In this case, we cap transpiration at available water, and back-calculate GrossPsn and NetPsn to downgrade those as well
                // Volumetric soil water content (mm/m) at species wilting point (h4) 
                // Convert kPA to mH2o (/9.804139432)
                float wiltPtWater = (float)hydrology.PressureHeadTable.CalcSoilWaterContent(PnETspecies.H4 * 9.804139432f, siteCohort.Ecoregion.SoilType);
                float availableWater = (hydrology.SoilWaterContent - wiltPtWater) * siteCohort.Ecoregion.RootingDepth * frostFreeFrac;
                if (PotentialTranspiration[index] > availableWater)
                {
                    Transpiration[index] = Math.Max(availableWater, 0f); // mm
                    if (CanopyLayerFrac > 0)
                        GrossPsn[index] = Transpiration[index] / 0.0015f * JCO2_JH2O / CanopyLayerFrac;
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
                // Subtract transpiration from soil water content
                Hydrology.SubtractTranspiration(hydrology, siteCohort.Ecoregion, Transpiration[index], frostFreeFrac, siteCohort.Site.Location);
                // Infiltration (let captured surface water soak into soil)
                Hydrology.CalcInfiltration(hydrology, siteCohort.Ecoregion, frostFreeFrac, siteCohort.Site.Location);
                // Net foliage respiration depends on reference psn (BaseFoliarRespiration)
                // Substitute 24 hours in place of DayLength because foliar respiration does occur at night.  BaseFoliarRespiration and RespirationFQ10 use Tavg temps reflecting both day and night temperatures.
                float RefFoliarRespiration = BaseFoliarRespiration * variables[species.Name].RespirationFQ10 * variables.DaySpan * Constants.SecondsPerDay * Constants.MC / Constants.billion; // gC/g Fol/month
                // Actual foliage respiration (growth respiration) 
                FoliarRespiration[index] = RefFoliarRespiration * Fol / Globals.IMAX; // gC/m2 ground/mo
                // NetPsn depends on gross psn and foliage respiration
                float nonOzoneNetPsn = GrossPsn[index] - FoliarRespiration[index];
                // Convert Psn gC/m2 ground/mo to umolCO2/m2 fol/s
                float netPsn_ground = nonOzoneNetPsn * 1000000F * (1F / 12F) * (1F / (variables.DayLength * variables.DaySpan));
                float netPsn_leaf_s = 0;
                if (netPsn_ground > 0 && LAI[index] > 0)
                {
                    netPsn_leaf_s = netPsn_ground * (1F / LAI[index]);
                    if (float.IsInfinity(netPsn_leaf_s))
                        netPsn_leaf_s = 0;
                }
                // Reduction factor for ozone on photosynthesis
                if (o3_month > 0)
                {
                    float wvConductance = Evapotranspiration.CalcWVConductance(variables.CO2, variables.Tavg, ciElev, netPsn_leaf_s);
                    float o3Coeff = PnETspecies.FOzone_slope;
                    fOzone = Photosynthesis.CalcFOzone(o3_month, subCanopyIndex, layerCount, Fol, lastFOzone, wvConductance, o3Coeff);
                }
                //Apply reduction factor for Ozone
                FOzone[index] = 1 - fOzone;
                NetPsn[index] = nonOzoneNetPsn * FOzone[index];
                // Add net psn to non soluble carbons
                data.NSC += NetPsn[index]; // gC
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

        public int CalcNonWoodBiomass(ActiveSite site)
        {
            return (int)Fol;
        }

        public static Percentage CalcNonWoodPercentage(Cohort cohort, ActiveSite site)
        {
            return new Percentage(cohort.Fol / (cohort.Wood + cohort.Fol));
        }

        public void InitializeOutput(LocalOutput localOutput)
        {
            cohortoutput = new LocalOutput(localOutput);
        }

        public void InitializeOutput(string SiteName)
        {
            cohortoutput = new LocalOutput(SiteName, "Cohort_" + Species.Name + ".csv", OutputHeader);
        }

        public void InitializeOutput(string SiteName, ushort EstablishmentYear)
        {
            cohortoutput = new LocalOutput(SiteName, "Cohort_" + Species.Name + "_" + EstablishmentYear + ".csv", OutputHeader);
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

        public void UpdateCohortData(IPnETEcoregionVars monthdata)
        {
            float netPsnSum = NetPsn.Sum();
            float grossPsnSum = GrossPsn.Sum();
            float transpirationSum = Transpiration.Sum();
            float WUE = Evapotranspiration.CalcWUE(grossPsnSum, CanopyLayerFrac, transpirationSum);
            // determine the limiting factor 
            float fWaterAvg = FWater.Average();
            float PressHeadAvg = PressHead.Average();
            float fRadAvg = FRad.Average();
            float fOzoneAvg = FOzone.Average();
            float fTemp = monthdata[Species.Name].PsnFTemp;
            string limitingFactor = "NA";
            if (ColdKill < int.MaxValue)
                limitingFactor = "ColdTolerance (" + ColdKill.ToString() + ")";
            else
            {
                List<float> factorList = new List<float>(new float[] { fWaterAvg, fRadAvg, fOzoneAvg, FAge, fTemp });
                float minFactor = factorList.Min();
                if (minFactor == fTemp)
                    limitingFactor = "fTemp";
                else if (minFactor == FAge)
                    limitingFactor = "fAge";
                else if (minFactor == fWaterAvg)
                {
                    if (PressHeadAvg > PnETSpecies.H3)
                        limitingFactor = "Too_dry";
                    else if (PressHeadAvg < PnETSpecies.H2)
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
                       netPsnSum + "," + // Sum over canopy layers
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
                       SoilWaterContent.Average() + "," +
                       PressHead.Average() + "," +
                       fRadAvg + "," +
                       fOzoneAvg + "," +
                       DelAmax.Average() + "," +
                       monthdata[Species.Name].PsnFTemp + "," +
                       monthdata[Species.Name].RespirationFTemp + "," +
                       FAge + "," +
                       IsLeafOn + "," +
                       FActiveBiom + "," +
                       AdjFolN.Average() + "," +
                       AdjFolBiomassFrac.Average() + "," +
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
                             OutputHeaders.FWater + "," +
                             OutputHeaders.SoilWaterContent + "," +
                             OutputHeaders.PressureHead + "," +
                             OutputHeaders.FRad + "," +
                             OutputHeaders.FOzone + "," +
                             OutputHeaders.DelAMax + "," +
                             OutputHeaders.PhotosynthesisFTemp + "," +
                             OutputHeaders.RespirationFTemp + "," +
                             OutputHeaders.FAge + "," +
                             OutputHeaders.LeafOn + "," +
                             OutputHeaders.FActiveBiom + "," +
                             OutputHeaders.AdjFolN + "," +
                             OutputHeaders.AdjFolBiomassFrac + "," +
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

        public float FolSenescence()
        {
            // If it is fall 
            float LeafLitter = PnETspecies.FolTurnoverRate * Fol;
            // If cohort is dead, then all foliage is lost
            if (NSCfrac <= 0.01F)
                LeafLitter = Fol;
            Fol -= LeafLitter;
            return LeafLitter;
        }

        public float WoodSenescence()
        {
            float senescence = (Root * PnETspecies.RootTurnoverRate) + Wood * PnETspecies.WoodTurnoverRate;
            data.TotalBiomass -= senescence;
            data.AGBiomass = PnETspecies.AGBiomassFrac * data.TotalBiomass + data.Fol;
            data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerFrac);
            data.MaxBiomass = Math.Max(data.MaxBiomass, data.TotalBiomass);
            return senescence;
        }

        public void ResetFoliageMax()
        {
            data.MaxFolYear = 0;
        }

        public void ReduceBiomass(object sitecohorts, double reductionFrac, ExtensionType disturbanceType)
        {
            if (!((SiteCohorts)sitecohorts).DisturbanceTypesReduced.Contains(disturbanceType))
            {
                Disturbance.ReduceDeadPools(sitecohorts, disturbanceType);  // Reduce dead pools before adding through Disturbance
                ((SiteCohorts)sitecohorts).DisturbanceTypesReduced.Add(disturbanceType);
            }
            Disturbance.AllocateDeadPools(sitecohorts, this, disturbanceType, reductionFrac);
            data.TotalBiomass *= (float)(1.0 - reductionFrac);
            data.AGBiomass = PnETspecies.AGBiomassFrac * data.TotalBiomass + data.Fol;
            data.UniversalData.Biomass = (int)(data.AGBiomass * data.CanopyLayerFrac);
            data.MaxBiomass = Math.Max(data.MaxBiomass, data.TotalBiomass);
            Fol *= (float)(1.0 - reductionFrac);
            data.MaxFolYear = Math.Max(data.MaxFolYear, Fol);
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

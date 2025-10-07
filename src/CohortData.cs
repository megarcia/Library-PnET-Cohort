// NOTE: ISpecies --> Landis.Core

using System.Collections.Generic;
using System;
using Landis.Core;
using Landis.Library.UniversalCohorts;

namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// Data for an individual cohort that is not shared with other cohorts.
    /// </summary>
    public struct CohortData
    {
        /// <summary>
        /// The cohort
        /// </summary>
        public Cohort Cohort;

        /// <summary>
        /// The universal cohort data
        /// </summary>
        public Library.UniversalCohorts.CohortData UniversalData;

        /// <summary>
        /// Succession timestep used by Biomass cohorts (yrs)
        /// </summary>
        public byte SuccessionTimestep;

        /// <summary>
        /// The cohort's live aboveground biomass (g/m2).
        /// </summary>
        public float AGBiomass;

        /// <summary>
        /// The cohort's live total biomass (wood + root) (g/m2).
        /// </summary>
        public float TotalBiomass;

        /// <summary>
        /// Are trees phsyiologically active
        /// </summary>
        public bool IsLeafOn;

        /// <summary>
        /// Max biomass achived in the cohorts' life time. 
        /// This value remains high after the cohort has reached its 
        /// peak biomass. It is used to determine canopy layers where
        /// it prevents that a cohort could descent in the canopy when 
        /// it declines (g/m2)
        /// </summary>
        public float MaxBiomass;

        /// <summary>
        /// Foliage (g/m2)
        /// </summary>
        public float Fol;

        /// <summary>
        /// Maximum Foliage For The Year (g/m2)
        /// </summary>
        public float MaxFolYear;

        /// <summary>
        /// Non-Soluble Carbons
        /// </summary>
        public float NSC;

        /// <summary>
        /// Defoliation Fraction
        /// </summary>
        public float DefoliationFrac;

        /// <summary>
        /// Annual Wood Senescence (g/m2)
        /// </summary>
        public float LastWoodSenescence;

        /// <summary>
        /// Annual Foliage Senescence (g/m2)
        /// </summary>
        public float LastFolSenescence;

        /// <summary>
        /// Last Average FRad
        /// </summary>
        public float LastFRad;

        /// <summary>
        /// Last Growing Season FRad
        /// </summary>
        public List<float> LastSeasonFRad;

        /// <summary>
        /// Adjusted Fraction of Foliage
        /// </summary>
        public float adjFolBiomassFrac;

        /// <summary>
        /// Adjusted Half Sat
        /// </summary>
        public float AdjHalfSat;

        /// <summary>
        /// Adjusted Foliage Carbons
        /// </summary>
        public float adjFolN;

        /// <summary>
        /// Boolean whether cohort has been killed by cold temp relative to cold tolerance
        /// </summary>
        public int ColdKill;

        /// <summary>
        /// The Layer of the Cohort
        /// </summary>
        public byte Layer;

        /// <summary>
        /// Leaf area index per subcanopy layer (m/m)
        /// </summary>
        public float[] LAI;

        /// <summary>
        /// Leaf area index (m/m) maximum last year
        /// </summary>
        public float LastLAI;

        /// <summary>
        /// Aboveground Biomass last year
        /// </summary>
        public float LastAGBio;

        /// <summary>
        /// Gross photosynthesis (gC/mo)
        /// </summary>
        public float[] GrossPsn;

        /// <summary>
        /// Foliar respiration (gC/mo)
        /// </summary>
        public float[] FoliarRespiration;

        /// <summary>
        /// Net photosynthesis (gC/mo)
        /// </summary>
        public float[] NetPsn;

        /// <summary>
        /// Mainenance respiration (gC/mo)
        /// </summary>
        public float[] MaintenanceRespiration;

        /// <summary>
        /// Transpiration (mm/mo)
        /// </summary>
        public float[] Transpiration;

        /// <summary>
        /// PotentialTranspiration (mm/mo)
        /// </summary>
        public float[] PotentialTranspiration;

        /// <summary>
        /// Reduction factor for suboptimal radiation on growth
        /// </summary>
        public float[] FRad;

        /// <summary>
        /// Reduction factor for suboptimal or supra optimal water 
        /// </summary>
        public float[] FWater;

        /// <summary>
        /// Actual water used to calculate FWater
        /// </summary>
        public float[] SoilWaterContent;

        /// <summary>
        /// Actual pressurehead used to calculate FWater
        /// </summary>
        public float[] PressHead;

        /// <summary>
        /// Number of precip events allocated to sublayer
        /// </summary>
        public int[] NumPrecipEvents;

        /// <summary>
        /// Reduction factor for ozone 
        /// </summary>
        public float[] FOzone;

        /// <summary>
        /// Interception (mm/mo)
        /// </summary>
        public float[] Interception;

        /// <summary>
        /// Adjustment folN based on fRad
        /// </summary>
        public float[] AdjFolN;

        /// <summary>
        /// Adjustment folBiomassFrac based on fRad
        /// </summary>
        public float[] AdjFolBiomassFrac;

        /// <summary>
        /// Modifier of CiCa ratio based on fWater and Ozone
        /// </summary>
        public float[] CiModifier;

        /// <summary>
        /// Adjustment to Amax based on CO2
        /// </summary>
        public float[] DelAmax;

        /// <summary>
        /// Fraction of layer biomass attributed to cohort
        /// </summary>
        public float BiomassLayerFrac;

        /// <summary>
        /// Fraction of layer canopy (foliage) attributed to cohort
        /// </summary>
        public float CanopyLayerFrac;

        /// <summary>
        /// Fraction of layer canopy growing space available to cohort
        /// </summary>
        public float CanopyGrowingSpace;

        /// <summary>
        /// CohortData constructor #1
        /// </summary>
        /// <param name="cohort"></param>
        public CohortData(Cohort cohort)
        {
            SuccessionTimestep = cohort.SuccessionTimestep;
            AdjFolN = cohort.AdjFolN;
            adjFolN = cohort.adjFolN;
            AdjFolBiomassFrac = cohort.AdjFolBiomassFrac;
            adjFolBiomassFrac = cohort.adjFolBiomassFrac;
            AdjHalfSat = cohort.AdjHalfSat;
            UniversalData.Age = cohort.Age;
            AGBiomass = cohort.PnETSpecies.AGBiomassFrac * cohort.TotalBiomass + cohort.Fol;
            UniversalData.Biomass = (int)(AGBiomass * cohort.CanopyLayerFrac);
            TotalBiomass = cohort.TotalBiomass;
            MaxBiomass = cohort.MaxBiomass;
            CiModifier = cohort.CiModifier;
            ColdKill = cohort.ColdKill;
            DefoliationFrac = cohort.DefoliationFrac;
            DelAmax = cohort.DelAmax;
            Fol = cohort.Fol;
            MaxFolYear = cohort.MaxFolYear;
            FoliarRespiration = cohort.FoliarRespiration;
            FOzone = cohort.FOzone;
            FRad = cohort.FRad;
            FWater = cohort.FWater;
            GrossPsn = cohort.GrossPsn;
            Interception = cohort.Interception;
            LAI = cohort.LAI;
            LastLAI = cohort.LastLAI;
            LastFolSenescence = cohort.LastFolSenescence;
            LastFRad = cohort.LastFRad;
            LastSeasonFRad = cohort.LastSeasonFRad;
            LastWoodSenescence = cohort.LastWoodSenescence;
            LastAGBio = cohort.LastAGBio;
            Layer = cohort.Layer;
            IsLeafOn = cohort.IsLeafOn;
            MaintenanceRespiration = cohort.MaintenanceRespiration;
            NetPsn = cohort.NetPsn;
            NSC = cohort.NSC;
            PressHead = cohort.PressHead;
            NumPrecipEvents = cohort.NumPrecipEvents;
            Transpiration = cohort.Transpiration;
            PotentialTranspiration = cohort.PotentialTranspiration;
            SoilWaterContent = cohort.SoilWaterContent;
            BiomassLayerFrac = cohort.BiomassLayerFrac;
            CanopyLayerFrac = cohort.CanopyLayerFrac;
            Cohort = cohort;
            CanopyGrowingSpace = cohort.CanopyGrowingSpace;
            UniversalData.ANPP = cohort.ANPP;
        }

        /// <summary>
        /// CohortData constructor #2
        /// </summary>
        /// <param name="age"></param>
        /// <param name="successionTimestep"></param>
        /// <param name="totalBiomass"></param>
        /// <param name="totalANPP"></param>
        /// <param name="species"></param>
        /// <param name="cohortStacking"></param>
        public CohortData(ushort age, byte successionTimestep, float totalBiomass, double totalANPP, ISpecies species, bool cohortStacking)
        {
            SuccessionTimestep = successionTimestep;
            AdjFolN = new float[Globals.IMAX];
            adjFolN = 0; ;
            AdjFolBiomassFrac = new float[Globals.IMAX];
            adjFolBiomassFrac = 0;
            AdjHalfSat = 0;
            UniversalData.Age = age;
            IPnETSpecies pnetspecies = SpeciesParameters.PnETSpecies.AllSpecies[species.Index];
            Cohort = new Cohort(species, pnetspecies, 0, "", 1, cohortStacking, successionTimestep);
            AGBiomass = pnetspecies.AGBiomassFrac * totalBiomass;
            UniversalData.Biomass = (int)AGBiomass;
            TotalBiomass = totalBiomass;
            MaxBiomass = totalBiomass;
            CiModifier = new float[Globals.IMAX];
            ColdKill = int.MaxValue;
            DefoliationFrac = 0;
            DelAmax = new float[Globals.IMAX];
            Fol = 0;
            MaxFolYear = 0;
            FoliarRespiration = new float[Globals.IMAX];
            FOzone = new float[Globals.IMAX];
            FRad = new float[Globals.IMAX];
            FWater = new float[Globals.IMAX];
            GrossPsn = new float[Globals.IMAX];
            Interception = new float[Globals.IMAX];
            LAI = new float[Globals.IMAX];
            LastFolSenescence = 0;
            LastFRad = 0;
            LastSeasonFRad = new List<float>();
            LastWoodSenescence = 0;
            LastAGBio = AGBiomass;
            Layer = 0;
            IsLeafOn = false;
            MaintenanceRespiration = new float[Globals.IMAX];
            NetPsn = new float[Globals.IMAX];
            NSC = 0;
            PressHead = new float[Globals.IMAX];
            NumPrecipEvents = new int[Globals.IMAX];
            Transpiration = new float[Globals.IMAX];
            PotentialTranspiration = new float[Globals.IMAX];
            SoilWaterContent = new float[Globals.IMAX];
            BiomassLayerFrac = 1.0f;
            float cohortIdealFol = pnetspecies.FolBiomassFrac * (float)Math.Exp(-pnetspecies.LiveWoodBiomassFrac * MaxBiomass) * TotalBiomass;
            float cohortLAI = Canopy.CalcCohortLAI(pnetspecies, cohortIdealFol);
            LastLAI = cohortLAI;
            CanopyLayerFrac = LastLAI / pnetspecies.MaxLAI;
            if (cohortStacking)
                CanopyLayerFrac = 1.0f;
            CanopyGrowingSpace = 1.0f;
            UniversalData.ANPP = totalANPP;
        }
    }
}

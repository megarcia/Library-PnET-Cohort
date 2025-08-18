//  Copyright ...
//  Authors:  Arjan de Bruijn

using Landis.Utilities;
using Landis.Core;
using Landis.Library.Climate;
using Landis.Library.InitialCommunities.Universal;
using Landis.Library.UniversalCohorts;
using Landis.SpatialModeling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Landis.Library.PnETCohorts
{
    public class SiteCohorts : Landis.Library.UniversalCohorts.SiteCohorts, ISiteCohorts
    {
        private float canopylaimax;
        private float avgSoilWaterContent;
        private float snowpack;
        private float[] CanopyLAI;
        private float subcanopypar;
        private float julysubcanopypar;
        private float subcanopyparmax;
        private float fracRootAboveFrost;
        private float soilDiffusivity;
        private float leakageFrac;
        private float[] netpsn = null;
        private float[] grosspsn = null;
        private float[] foliarRespiration = null;
        private float[] maintresp = null;
        private float[] averageAlbedo = null;
        private float[] activeLayerDepth = null;
        private float[] frostDepth = null;
        private float[] monthCount = null;
        private float[] monthlySnowpack = null;
        private float[] monthlyWater = null;
        private float[] monthlyLAI = null;
        private float[] monthlyLAICumulative = null;
        private float[] monthlyEvap = null;
        private float[] monthlyActualTrans = null;
        private float[] monthlyInterception = null;
        private float[] monthlyLeakage = null;
        private float[] monthlyRunoff = null;
        private float[] monthlyActualET = null;
        private float[] monthlyPotentialEvap = null;
        private float[] monthlyPotentialTrans = null;
        private float transpiration;
        private float potentialTranspiration;
        private double HeterotrophicRespiration;
        private Hydrology hydrology = null;
        IProbEstablishment probEstablishment = null;
        public ActiveSite Site;
        public Dictionary<ISpecies, List<Cohort>> cohorts = null;
        public List<ISpecies> SpeciesEstablishedByPlant = null;
        public List<ISpecies> SpeciesEstablishedBySerotiny = null;
        public List<ISpecies> SpeciesEstablishedByResprout = null;
        public List<ISpecies> SpeciesEstablishedBySeed = null;
        public List<int> CohortsKilledBySuccession = null;
        public List<int> CohortsKilledByCold = null;
        public List<int> CohortsKilledByHarvest = null;
        public List<int> CohortsKilledByFire = null;
        public List<int> CohortsKilledByWind = null;
        public List<int> CohortsKilledByOther = null;
        public List<ExtensionType> DisturbanceTypesReduced = null;
        public IEcoregionPnET Ecoregion;
        public LocalOutput siteoutput;
        private float[] ActualET = new float[12]; // mm/mo
        private static IDictionary<uint, SiteCohorts> initialSites;
        private static byte MaxCanopyLayers;
        private static float LayerThreshRatio;
        private float interception;
        private float precLoss;
        private static byte Timestep;
        private static int CohortBinSize;
        private static bool PrecipEventsWithReplacement;
        private int nlayers;
        private static int MaxLayer;
        private static bool soilIceDepth;
        private static bool permafrost;
        private static bool invertProbEstablishment;
        public SortedList<float, float> depthTempDict = new SortedList<float, float>();  //for permafrost
        float lastTempBelowSnow = float.MaxValue;
        private static float maxHalfSat;
        private static float minHalfSat;
        private static bool CohortStacking;
        Dictionary<double, bool> ratioAbove10 = new Dictionary<double, bool>();
        private static float CanopySumScale;

        public List<ISpecies> SpeciesByPlant
        {
            get
            {
                return SpeciesEstablishedByPlant;
            }
            set
            {
                SpeciesEstablishedByPlant = value;
            }
        }

        public List<ISpecies> SpeciesBySerotiny
        {
            get
            {
                return SpeciesEstablishedBySerotiny;
            }
            set
            {
                SpeciesEstablishedBySerotiny = value;
            }
        }

        public List<ISpecies> SpeciesByResprout
        {
            get
            {
                return SpeciesEstablishedByResprout;
            }
            set
            {
                SpeciesEstablishedByResprout = value;
            }
        }

        public List<ISpecies> SpeciesBySeed
        {
            get
            {
                return SpeciesEstablishedBySeed;
            }
            set
            {
                SpeciesEstablishedBySeed = value;
            }
        }

        public List<int> CohortsBySuccession
        {
            get
            {
                return CohortsKilledBySuccession;
            }
            set
            {
                CohortsKilledBySuccession = value;
            }
        }

        public List<int> CohortsByCold
        {
            get
            {
                return CohortsKilledByCold;
            }
            set
            {
                CohortsKilledByCold = value;
            }
        }

        public List<int> CohortsByHarvest
        {
            get
            {
                return CohortsKilledByHarvest;
            }
            set
            {
                CohortsKilledByHarvest = value;
            }
        }

        public List<int> CohortsByFire
        {
            get
            {
                return CohortsKilledByFire;
            }
            set
            {
                CohortsKilledByFire = value;
            }
        }

        public List<int> CohortsByWind
        {
            get
            {
                return CohortsKilledByWind;
            }
            set
            {
                CohortsKilledByWind = value;
            }
        }

        public List<int> CohortsByOther
        {
            get
            {
                return CohortsKilledByOther;
            }
            set
            {
                CohortsKilledByOther = value;
            }
        }

        public float Transpiration
        {
            get
            {
                return transpiration;
            }
        }

        public float PotentialTranspiration
        {
            get
            {
                return potentialTranspiration;
            }
        }

        public float JulySubCanopyPar
        {
            get
            {
                return julysubcanopypar;
            }
        }

        public float SubcanopyPAR
        {
            get
            {
                return subcanopypar;
            }
        }

        public IProbEstablishment ProbEstablishment
        {
            get
            {
                return probEstablishment;
            }
        }

        public float SubCanopyParMAX
        {
            get
            {
                return subcanopyparmax;
            }
        }

        public float AvgSoilWaterContent
        {
            get
            {
                return avgSoilWaterContent;
            }
        }

        public float[] NetPsn
        {
            get
            {
                if (netpsn == null)
                {
                    float[] netpsn_array = new float[12];
                    for (int i = 0; i < netpsn_array.Length; i++)
                    {
                        netpsn_array[i] = 0;
                    }

                    return netpsn_array;
                }
                else
                    return netpsn.ToArray();
            }
        }

        public static bool InitialSitesContainsKey(uint key)
        {
            if (initialSites != null && initialSites.ContainsKey(key))
                return true;
            return false;
        }

        public static void Initialize()
        {
            initialSites = new Dictionary<uint, SiteCohorts>();
            Timestep = ((Parameter<byte>)Names.GetParameter(Names.Timestep)).Value;
            LayerThreshRatio = ((Parameter<float>)Names.GetParameter(Names.LayerThreshRatio, 0, float.MaxValue)).Value;
            MaxCanopyLayers = ((Parameter<byte>)Names.GetParameter(Names.MaxCanopyLayers, 0, 20)).Value;
            soilIceDepth = ((Parameter<bool>)Names.GetParameter(Names.SoilIceDepth)).Value;
            invertProbEstablishment = ((Parameter<bool>)Names.GetParameter(Names.InvertPest)).Value;
            CohortStacking = ((Parameter<bool>)Names.GetParameter(Names.CohortStacking)).Value;
            CanopySumScale = ((Parameter<float>)Names.GetParameter(Names.CanopySumScale, 0f, 1f)).Value;
            permafrost = false;
            Parameter<string> CohortBinSizeParm = null;
            if (Names.TryGetParameter(Names.CohortBinSize, out CohortBinSizeParm))
            {
                if (!Int32.TryParse(CohortBinSizeParm.Value, out CohortBinSize))
                    throw new System.Exception("CohortBinSize is not an integer value.");
            }
            else
                CohortBinSize = Timestep;
            string precipEventsWithReplacement = ((Parameter<string>)Names.GetParameter(Names.PrecipEventsWithReplacement)).Value;
            PrecipEventsWithReplacement = true;
            if (precipEventsWithReplacement == "false" || precipEventsWithReplacement == "no")
                PrecipEventsWithReplacement = false;
            maxHalfSat = 0;
            minHalfSat = float.MaxValue;
            foreach (IPnETSpecies spc in SpeciesParameters.PnETSpecies.AllSpecies)
            {
                if (spc.HalfSat > maxHalfSat)
                    maxHalfSat = spc.HalfSat;
                if (spc.HalfSat < minHalfSat)
                    minHalfSat = spc.HalfSat;
            }
        }

        /// <summary>
        /// Constructor for initialization of SiteCohorts with no initialSite entry yet        
        /// </summary>
        /// <param name="StartDate"></param>
        /// <param name="site"></param>
        /// <param name="initialCommunity"></param>
        /// <param name="usingClimateLibrary"></param>
        /// <param name="initialCommunitiesSpinup"></param>
        /// <param name="minFolRatioFactor"></param>
        /// <param name="SiteOutputName"></param>
        public SiteCohorts(DateTime StartDate, ActiveSite site, ICommunity initialCommunity, bool usingClimateLibrary, string initialCommunitiesSpinup, float minFolRatioFactor, string SiteOutputName = null)
        {
            this.Ecoregion = EcoregionData.GetPnETEcoregion(Globals.ModelCore.Ecoregion[site]);
            this.Site = site;
            cohorts = new Dictionary<ISpecies, List<Cohort>>();
            SpeciesEstablishedByPlant = new List<ISpecies>();
            SpeciesEstablishedBySerotiny = new List<ISpecies>();
            SpeciesEstablishedByResprout = new List<ISpecies>();
            SpeciesEstablishedBySeed = new List<ISpecies>();
            CohortsKilledBySuccession = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByCold = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByHarvest = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByFire = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByWind = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByOther = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            DisturbanceTypesReduced = new List<ExtensionType>();
            uint key = CalcKey((ushort)initialCommunity.MapCode, Globals.ModelCore.Ecoregion[site].MapCode);
            SiteVars.MonthlyPressureHead[site] = new float[0];
            SiteVars.MonthlySoilTemp[site] = new SortedList<float, float>[0];
            int tempMaxCanopyLayers = MaxCanopyLayers;
            if (CohortStacking)
                tempMaxCanopyLayers = initialCommunity.Cohorts.Count();
            lock (Globals.initialSitesThreadLock)
            {
                if (initialSites.ContainsKey(key) == false)
                    initialSites.Add(key, this);
            }
            List<IEcoregionPnETVariables> ecoregionInitializer = usingClimateLibrary ? EcoregionData.GetClimateRegionData(Ecoregion, StartDate, StartDate.AddMonths(1)) : EcoregionData.GetData(Ecoregion, StartDate, StartDate.AddMonths(1));
            hydrology = new Hydrology(Ecoregion.FieldCapacity);
            avgSoilWaterContent = hydrology.SoilWaterContent;
            subcanopypar = ecoregionInitializer[0].PAR0;
            subcanopyparmax = subcanopypar;
            SiteVars.WoodDebris[Site] = new Pool();
            SiteVars.Litter[Site] = new Pool();
            SiteVars.FineFuels[Site] = SiteVars.Litter[Site].Mass;
            List<float> cohortBiomassLayerFrac = new List<float>();
            List<float> cohortCanopyLayerFrac = new List<float>();
            if (SiteOutputName != null)
            {
                this.siteoutput = new LocalOutput(SiteOutputName, "Site.csv", Header(site));
                probEstablishment = new ProbEstablishment(SiteOutputName, "Establishment.csv");
            }
            else
                probEstablishment = new ProbEstablishment(null, null);
            bool biomassProvided = false;
            foreach (Landis.Library.UniversalCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
            {
                foreach (Landis.Library.UniversalCohorts.ICohort cohort in speciesCohorts)
                {
                    if (cohort.Data.Biomass > 0)  // 0 biomass indicates biomass value was not read in
                    {
                        biomassProvided = true;
                        break;
                    }
                }
            }
            if (biomassProvided && !(initialCommunitiesSpinup.ToLower() == "spinup"))
            {
                List<double> CohortBiomassList = new List<double>();
                List<double> CohortMaxBiomassList = new List<double>();
                if (initialCommunitiesSpinup.ToLower() == "nospinup")
                {
                    foreach (Landis.Library.UniversalCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                    {
                        foreach (Landis.Library.UniversalCohorts.ICohort cohort in speciesCohorts)
                        {
                            // TODO: Add warning if biomass is 0
                            bool addCohort = AddNewCohort(new Cohort(SpeciesParameters.PnETSpecies[cohort.Species], cohort.Data.Age, cohort.Data.Biomass, SiteOutputName, (ushort)(StartDate.Year - cohort.Data.Age), CohortStacking));
                            CohortBiomassList.Add(AllCohorts.Last().AGBiomass);
                            CohortMaxBiomassList.Add(AllCohorts.Last().MaxBiomass);
                        }
                    }
                }
                else
                {
                    if ((initialCommunitiesSpinup.ToLower() != "spinuplayers") && (initialCommunitiesSpinup.ToLower() != "spinuplayersrescale"))
                        Globals.ModelCore.UI.WriteLine("Warning:  InitialCommunitiesSpinup parameter is not 'Spinup', 'SpinupLayers','SpinupLayersRescale' or 'NoSpinup'.  Biomass is provided so using 'SpinupLayers' by default.");
                    SpinUp(StartDate, site, initialCommunity, usingClimateLibrary, SiteOutputName, false);
                    // species-age key to store maxbiomass values
                    Dictionary<ISpecies, Dictionary<int, float[]>> cohortDictionary = new Dictionary<ISpecies, Dictionary<int, float[]>>();
                    foreach (Cohort cohort in AllCohorts)
                    {
                        ISpecies spp = cohort.Species;
                        int age = cohort.Age;
                        float lastSeasonAvgFRad = 0F;
                        if (cohort.LastSeasonFRad.Count() > 0)
                            lastSeasonAvgFRad = cohort.LastSeasonFRad.ToArray().Average();
                        if (cohortDictionary.ContainsKey(spp))
                        {
                            if (cohortDictionary[spp].ContainsKey(age))
                            {
                                // message duplicate species and age
                            }
                            else
                            {
                                float[] values = new float[] { (int)cohort.MaxBiomass, cohort.Biomass, lastSeasonAvgFRad };
                                cohortDictionary[spp].Add(age, values);
                            }
                        }
                        else
                        {
                            Dictionary<int, float[]> ageDictionary = new Dictionary<int, float[]>();
                            float[] values = new float[] { (int)cohort.MaxBiomass, cohort.Biomass, lastSeasonAvgFRad };
                            ageDictionary.Add(age, values);
                            cohortDictionary.Add(spp, ageDictionary);
                        }
                    }
                    ClearAllCohorts();
                    foreach (Landis.Library.UniversalCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                    {
                        foreach (Landis.Library.UniversalCohorts.ICohort cohort in speciesCohorts)
                        {
                            // TODO: Add warning if biomass is 0
                            int age = cohort.Data.Age;
                            ISpecies spp = cohort.Species;
                            float[] values = cohortDictionary[spp][age];
                            int cohortMaxBiomass = (int)values[0];
                            float cohortSpinupBiomass = values[1];
                            float lastSeasonAvgFRad = values[2];
                            float inputMaxBiomass = Math.Max(cohortMaxBiomass, cohort.Data.Biomass);
                            if (initialCommunitiesSpinup.ToLower() == "spinuplayersrescale")
                                inputMaxBiomass = cohortMaxBiomass * (cohort.Data.Biomass / cohortSpinupBiomass);
                            float cohortCanopyGrowingSpace = 1f;
                            bool addCohort = AddNewCohort(new Cohort(SpeciesParameters.PnETSpecies[cohort.Species], cohort.Data.Age, cohort.Data.Biomass, (int)inputMaxBiomass, cohortCanopyGrowingSpace, SiteOutputName, (ushort)(StartDate.Year - cohort.Data.Age), CohortStacking, lastSeasonAvgFRad));
                            CohortBiomassList.Add(AllCohorts.Last().AGBiomass);
                            CohortMaxBiomassList.Add(AllCohorts.Last().MaxBiomass);
                        }
                    }
                }
                bool runAgain = true;
                int attempts = 0;
                while (runAgain)
                {
                    attempts++;
                    bool badSpinup = false;
                    // Sort cohorts into layers                    
                    List<List<double>> cohortBins = GetBinsByCohort(CohortMaxBiomassList);
                    float[] CanopyLAISum = new float[tempMaxCanopyLayers];
                    float[] LayerBiomass = new float[tempMaxCanopyLayers];
                    List<float>[] LayerBiomassValues = new List<float>[tempMaxCanopyLayers];
                    float[] LayerFoliagePotential = new float[tempMaxCanopyLayers];
                    List<float>[] LayerFoliagePotentialValues = new List<float>[tempMaxCanopyLayers];
                    Dictionary<Cohort, float> canopyFracs = new Dictionary<Cohort, float>();
                    CanopyLAI = new float[tempMaxCanopyLayers];
                    List<double> NewCohortMaxBiomassList = new List<double>();
                    foreach (Cohort cohort in AllCohorts)
                    {
                        int layerIndex = 0;
                        foreach (List<double> layerBiomassList in cohortBins)
                        {
                            if (layerBiomassList.Contains(cohort.MaxBiomass))
                            {
                                cohort.Layer = (byte)layerIndex;
                                // if "ground" then ensure cohort.Layer = 0
                                if (cohort.PnETSpecies.Lifeform.ToLower().Contains("ground"))
                                    cohort.Layer = 0;
                                break;
                            }
                            layerIndex++;
                        }
                        int layer = cohort.Layer;
                        int layerCount = cohortBins[layer].Count();
                        // Estimate new wood biomass from BGBiomassFrac and FrActWs (see Estimate_WoodBio.xlsx)
                        float estSlope = -8.236285f + 27.768424f * cohort.PnETSpecies.BGBiomassFrac + 191053.281571f * cohort.PnETSpecies.LiveWoodBiomassFrac + 312.812679f * cohort.PnETSpecies.FolBiomassFrac + -594492.216284f * cohort.PnETSpecies.BGBiomassFrac * cohort.PnETSpecies.LiveWoodBiomassFrac + -941.447695f * cohort.PnETSpecies.BGBiomassFrac * cohort.PnETSpecies.FolBiomassFrac + -6490254.134756f * cohort.PnETSpecies.LiveWoodBiomassFrac * cohort.PnETSpecies.FolBiomassFrac + 19879995.810771f * cohort.PnETSpecies.BGBiomassFrac * cohort.PnETSpecies.LiveWoodBiomassFrac * cohort.PnETSpecies.FolBiomassFrac;
                        float estInt = 1735.179f + 2994.393f * cohort.PnETSpecies.BGBiomassFrac + 10167232.544f * cohort.PnETSpecies.LiveWoodBiomassFrac + 53598.871f * cohort.PnETSpecies.FolBiomassFrac + -92028081.987f * cohort.PnETSpecies.BGBiomassFrac * cohort.PnETSpecies.LiveWoodBiomassFrac + -168141.498f * cohort.PnETSpecies.BGBiomassFrac * cohort.PnETSpecies.FolBiomassFrac + -1104139533.563f * cohort.PnETSpecies.LiveWoodBiomassFrac * cohort.PnETSpecies.FolBiomassFrac + 3507005746.011f * cohort.PnETSpecies.BGBiomassFrac * cohort.PnETSpecies.LiveWoodBiomassFrac * cohort.PnETSpecies.FolBiomassFrac;
                        float newWoodBiomass = estInt + estSlope * cohort.AGBiomass * layerCount; // Inflate AGBiomass by # of cohorts in layer, assuming equal space among them
                        float newTotalBiomass = newWoodBiomass / (1 - cohort.PnETSpecies.BGBiomassFrac);
                        cohort.CanopyLayerFrac = 1f / layerCount;
                        if (CohortStacking)
                            cohort.CanopyLayerFrac = 1.0f;
                        float cohortFol = cohort.adjFolBiomassFrac * cohort.FActiveBiom * cohort.TotalBiomass;
                        float cohortLAI = 0;
                        for (int i = 0; i < Globals.IMAX; i++)
                            cohortLAI += cohort.CalcLAI(cohort.PnETSpecies, cohortFol, i, cohortLAI);
                        cohortLAI = Math.Min(cohortLAI, cohort.PnETSpecies.MaxLAI);
                        cohort.LastLAI = cohortLAI;
                        cohort.CanopyGrowingSpace = Math.Min(cohort.CanopyGrowingSpace, 1.0f);
                        float cohortLAIRatio = Math.Min(cohortLAI / cohort.PnETSpecies.MaxLAI, cohort.CanopyGrowingSpace);
                        if (CohortStacking)
                            cohortLAIRatio = 1.0f;
                        canopyFracs.Add(cohort, cohortLAIRatio);
                        cohort.NSC = cohort.PnETSpecies.NSCFrac * cohort.FActiveBiom * (cohort.TotalBiomass + cohort.Fol) * cohort.PnETSpecies.CFracBiomass;
                        cohort.Fol = cohortFol * (1 - cohort.PnETSpecies.FolTurnoverRate);
                        if (LayerFoliagePotentialValues[layer] == null)
                            LayerFoliagePotentialValues[layer] = new List<float>();
                        LayerFoliagePotentialValues[layer].Add(cohortLAIRatio);
                        LayerFoliagePotential[layer] += cohortLAIRatio;
                    }
                    // Adjust cohort biomass values so that site-values equal input biomass
                    float[] LayerFoliagePotentialAdj = new float[tempMaxCanopyLayers];
                    int index = 0;
                    foreach (Cohort cohort in AllCohorts)
                    {
                        int layer = cohort.Layer;
                        int layerCount = cohortBins[layer].Count();
                        float denomSum = 0f;
                        float canopyLayerFrac = Math.Min(canopyFracs[cohort], cohort.CanopyGrowingSpace);
                        canopyLayerFrac = Math.Min(canopyFracs[cohort], 1f / layerCount);
                        if (LayerFoliagePotential[layer] > 1)
                        {
                            float canopyLayerFracAdj = canopyFracs[cohort] / LayerFoliagePotential[layer];
                            canopyLayerFrac = (canopyLayerFracAdj - canopyFracs[cohort]) * CanopySumScale + canopyFracs[cohort];
                            cohort.CanopyGrowingSpace = canopyLayerFrac;
                        }
                        else
                            cohort.CanopyGrowingSpace = 1f;
                        cohort.CanopyLayerFrac = Math.Min(canopyFracs[cohort], canopyLayerFrac);
                        if (CohortStacking)
                        {
                            canopyLayerFrac = 1.0f;
                            cohort.CanopyLayerFrac = 1.0f;
                            cohort.CanopyGrowingSpace = 1.0f;
                        }
                        float targetBiomass = (float)CohortBiomassList[index];
                        float newWoodBiomass = targetBiomass / cohort.CanopyLayerFrac;
                        float newTotalBiomass = newWoodBiomass / (1 - cohort.PnETSpecies.BGBiomassFrac);
                        cohort.ChangeBiomass((int)Math.Round((newTotalBiomass - cohort.TotalBiomass) / 2f));
                        float cohortFoliage = cohort.adjFolBiomassFrac * cohort.FActiveBiom * cohort.TotalBiomass;
                        float cohortLAI = 0;
                        for (int i = 0; i < Globals.IMAX; i++)
                            cohortLAI += cohort.CalcLAI(cohort.PnETSpecies, cohortFoliage, i, cohortLAI);
                        cohortLAI = Math.Min(cohortLAI, cohort.PnETSpecies.MaxLAI);
                        cohort.LastLAI = cohortLAI;
                        cohort.CanopyGrowingSpace = Math.Min(cohort.CanopyGrowingSpace, 1.0f);
                        cohort.CanopyLayerFrac = Math.Min(cohort.LastLAI / cohort.PnETSpecies.MaxLAI, cohort.CanopyGrowingSpace);
                        if (CohortStacking)
                        {
                            canopyLayerFrac = 1.0f;
                            cohort.CanopyLayerFrac = 1.0f;
                            cohort.CanopyGrowingSpace = 1.0f;
                        }
                        float cohortFol = cohort.adjFolBiomassFrac * cohort.FActiveBiom * cohort.TotalBiomass;
                        cohort.Fol = cohortFol * (1 - cohort.PnETSpecies.FolTurnoverRate);
                        cohort.NSC = cohort.PnETSpecies.NSCFrac * cohort.FActiveBiom * (cohort.TotalBiomass + cohort.Fol) * cohort.PnETSpecies.CFracBiomass;
                        // Check cohort.Biomass
                        LayerFoliagePotentialAdj[layer] += cohort.CanopyLayerFrac;
                        CanopyLAISum[layer] += cohort.LAI.Sum() * cohort.CanopyLayerFrac;
                        LayerBiomass[layer] += cohort.CanopyLayerFrac * cohort.TotalBiomass;
                        index++;
                        NewCohortMaxBiomassList.Add(cohort.MaxBiomass);
                    }
                    //Re-sort layers
                    cohortBins = GetBinsByCohort(NewCohortMaxBiomassList);
                    float[] CanopyLayerSum = new float[tempMaxCanopyLayers];
                    List<double> FinalCohortMaxBiomassList = new List<double>();
                    // Assign new layers
                    foreach (Cohort cohort in AllCohorts)
                    {
                        int layerIndex = 0;
                        foreach (List<double> layerBiomassList in cohortBins)
                        {
                            if (layerBiomassList.Contains(cohort.MaxBiomass))
                            {
                                cohort.Layer = (byte)layerIndex;
                                // if "ground" then ensure cohort.Layer = 0
                                if (cohort.PnETSpecies.Lifeform.ToLower().Contains("ground"))
                                    cohort.Layer = 0;
                                break;
                            }
                            layerIndex++;
                        }
                    }
                    // Calculate new layer frac
                    float[] MainLayerCanopyFrac = new float[tempMaxCanopyLayers];
                    foreach (Cohort c in AllCohorts)
                    {
                        int layerIndex = c.Layer;
                        float LAISum = c.LAI.Sum();
                        if (c.IsLeafOn)
                        {
                            if (LAISum > c.LastLAI)
                                c.LastLAI = LAISum;
                        }
                        if (CohortStacking)
                            MainLayerCanopyFrac[layerIndex] += 1.0f;
                        else
                            MainLayerCanopyFrac[layerIndex] += Math.Min(c.LastLAI / c.PnETSpecies.MaxLAI, c.CanopyGrowingSpace);
                    }
                    int cohortIndex = 0;
                    float canopySumScale = CanopySumScale;
                    foreach (Cohort cohort in AllCohorts)
                    {
                        int layer = cohort.Layer;
                        int layerCount = cohortBins[layer].Count();
                        float targetBiomass = (float)CohortBiomassList[cohortIndex];
                        float canopyLayerFrac = Math.Min(cohort.LastLAI / cohort.PnETSpecies.MaxLAI, cohort.CanopyGrowingSpace);
                        if (MainLayerCanopyFrac[layer] > 1)
                        {
                            float canopyLayerFracAdj = cohort.CanopyLayerFrac / MainLayerCanopyFrac[layer];
                            canopyLayerFrac = (canopyLayerFracAdj - cohort.CanopyLayerFrac) * canopySumScale + cohort.CanopyLayerFrac;
                            cohort.CanopyGrowingSpace = Math.Min(cohort.CanopyGrowingSpace, canopyLayerFrac);
                        }
                        else
                        {
                            cohort.CanopyGrowingSpace = 1f;
                        }
                        if (CohortStacking)
                        {
                            canopyLayerFrac = 1.0f;
                            cohort.CanopyLayerFrac = 1.0f;
                            cohort.CanopyGrowingSpace = 1.0f;
                        }
                        float newWoodBiomass = targetBiomass / canopyLayerFrac;
                        float newTotalBiomass = newWoodBiomass / (1 - cohort.PnETSpecies.BGBiomassFrac);
                        float changeAmount = newTotalBiomass - cohort.TotalBiomass;
                        float tempFActiveBiom = (float)Math.Exp(-cohort.PnETSpecies.LiveWoodBiomassFrac * newTotalBiomass);
                        float cohortTempFoliage = cohort.adjFolBiomassFrac * tempFActiveBiom * newTotalBiomass;
                        float cohortTempLAI = 0;
                        for (int i = 0; i < Globals.IMAX; i++)
                            cohortTempLAI += cohort.CalcLAI(cohort.PnETSpecies, cohortTempFoliage, i, cohortTempLAI);
                        cohortTempLAI = Math.Min(cohortTempLAI, cohort.PnETSpecies.MaxLAI);
                        float tempBiomass = newTotalBiomass * (1 - cohort.PnETSpecies.BGBiomassFrac) * Math.Min(cohortTempLAI / cohort.PnETSpecies.MaxLAI, canopyLayerFrac);
                        if (CohortStacking)
                            tempBiomass = newTotalBiomass * (1.0f - cohort.PnETSpecies.BGBiomassFrac) * 1.0f;
                        float diff = tempBiomass - targetBiomass;
                        float lastDiff = diff;
                        bool match = Math.Abs(tempBiomass - targetBiomass) < 2;
                        float multiplierRoot = 1f;
                        while (!match)
                        {
                            float multiplier = multiplierRoot;
                            if (Math.Abs(tempBiomass - targetBiomass) > 1000)
                                multiplier = multiplierRoot * 200f;
                            else if (Math.Abs(tempBiomass - targetBiomass) > 500)
                                multiplier = multiplierRoot * 100f;
                            else if (Math.Abs(tempBiomass - targetBiomass) > 100)
                                multiplier = multiplierRoot * 20f;
                            else if (Math.Abs(tempBiomass - targetBiomass) > 50)
                                multiplier = multiplierRoot * 10f;
                            else if (Math.Abs(tempBiomass - targetBiomass) > 10)
                                multiplier = multiplierRoot * 2f;
                            lastDiff = diff;
                            if (tempBiomass > targetBiomass)
                                newTotalBiomass = Math.Max(newTotalBiomass - multiplier, 1);
                            else
                                newTotalBiomass = Math.Max(newTotalBiomass + multiplier, 1);
                            changeAmount = newTotalBiomass - cohort.TotalBiomass;
                            tempFActiveBiom = (float)Math.Exp(-cohort.PnETSpecies.LiveWoodBiomassFrac * newTotalBiomass);
                            cohortTempFoliage = cohort.adjFolBiomassFrac * tempFActiveBiom * newTotalBiomass;
                            cohortTempLAI = 0;
                            for (int i = 0; i < Globals.IMAX; i++)
                                cohortTempLAI += cohort.CalcLAI(cohort.PnETSpecies, cohortTempFoliage, i, cohortTempLAI);
                            cohortTempLAI = Math.Min(cohortTempLAI, cohort.PnETSpecies.MaxLAI);
                            if (CohortStacking)
                                tempBiomass = newTotalBiomass * (1.0f - cohort.PnETSpecies.BGBiomassFrac) * 1.0f;
                            else
                                tempBiomass = newTotalBiomass * (1 - cohort.PnETSpecies.BGBiomassFrac) * Math.Min(cohortTempLAI / cohort.PnETSpecies.MaxLAI, canopyLayerFrac);
                            diff = tempBiomass - targetBiomass;
                            if (Math.Abs(diff) > Math.Abs(lastDiff))
                                break;
                            if ((attempts < 3) && ((tempBiomass <= 0) || float.IsNaN(tempBiomass)))
                            {
                                badSpinup = true;
                                break;
                            }
                            match = Math.Abs(tempBiomass - targetBiomass) < 2;
                        }
                        cohort.ChangeBiomass((int)Math.Round((newTotalBiomass - cohort.TotalBiomass) * 1f / 1f));
                        float cohortFoliage = cohort.adjFolBiomassFrac * cohort.FActiveBiom * cohort.TotalBiomass;
                        float cohortLAI = 0;
                        for (int i = 0; i < Globals.IMAX; i++)
                            cohortLAI += cohort.CalcLAI(cohort.PnETSpecies, cohortFoliage, i, cohortLAI);
                        cohortLAI = Math.Min(cohortLAI, cohort.PnETSpecies.MaxLAI);
                        cohort.LastLAI = cohortLAI;
                        cohort.CanopyLayerFrac = Math.Min(cohort.LastLAI / cohort.PnETSpecies.MaxLAI, cohort.CanopyGrowingSpace);
                        if (CohortStacking)
                            cohort.CanopyLayerFrac = 1.0f;
                        CanopyLayerSum[layer] += cohort.CanopyLayerFrac;
                        cohort.Fol = cohortFoliage * (1 - cohort.PnETSpecies.FolTurnoverRate);
                        cohort.NSC = cohort.PnETSpecies.NSCFrac * cohort.FActiveBiom * (cohort.TotalBiomass + cohort.Fol) * cohort.PnETSpecies.CFracBiomass;
                        cohortIndex++;
                        FinalCohortMaxBiomassList.Add(cohort.MaxBiomass);
                    }
                    //Re-sort layers
                    cohortBins = GetBinsByCohort(FinalCohortMaxBiomassList);
                    // Assign new layers
                    foreach (Cohort cohort in AllCohorts)
                    {
                        int layerIndex = 0;
                        foreach (List<double> layerBiomassList in cohortBins)
                        {
                            if (layerBiomassList.Contains(cohort.MaxBiomass))
                            {
                                cohort.Layer = (byte)layerIndex;
                                // if "ground" then ensure cohort.Layer = 0
                                if (cohort.PnETSpecies.Lifeform.ToLower().Contains("ground"))
                                    cohort.Layer = 0;
                                break;
                            }
                            layerIndex++;
                        }
                    }
                    // Calculate new layer frac
                    MainLayerCanopyFrac = new float[tempMaxCanopyLayers];
                    foreach (Cohort c in AllCohorts)
                    {
                        int layerIndex = c.Layer;
                        float LAISum = c.LAI.Sum();
                        if (c.IsLeafOn)
                        {
                            if (LAISum > c.LastLAI)
                                c.LastLAI = LAISum;
                        }
                        if (CohortStacking)
                            MainLayerCanopyFrac[layerIndex] += 1.0f;
                        else
                            MainLayerCanopyFrac[layerIndex] += Math.Min(c.LastLAI / c.PnETSpecies.MaxLAI, c.CanopyGrowingSpace);
                    }
                    CanopyLayerSum = new float[tempMaxCanopyLayers];
                    cohortIndex = 0;
                    foreach (Cohort cohort in AllCohorts)
                    {
                        int layer = cohort.Layer;
                        int layerCount = cohortBins[layer].Count();
                        float targetBiomass = (float)CohortBiomassList[cohortIndex];
                        float canopyLayerFrac = Math.Min(cohort.LastLAI / cohort.PnETSpecies.MaxLAI, cohort.CanopyGrowingSpace);
                        if (MainLayerCanopyFrac[layer] > 1)
                        {
                            float canopyLayerFracAdj = cohort.CanopyLayerFrac / MainLayerCanopyFrac[layer];
                            canopyLayerFrac = (canopyLayerFracAdj - cohort.CanopyLayerFrac) * canopySumScale + cohort.CanopyLayerFrac;
                            cohort.CanopyGrowingSpace = Math.Min(cohort.CanopyGrowingSpace, canopyLayerFrac);
                        }
                        else
                            cohort.CanopyGrowingSpace = 1f;
                        if (CohortStacking)
                        {
                            canopyLayerFrac = 1.0f;
                            cohort.CanopyLayerFrac = 1.0f;
                            cohort.CanopyGrowingSpace = 1.0f;
                        }
                        float newWoodBiomass = targetBiomass / canopyLayerFrac;
                        float newTotalBiomass = newWoodBiomass / (1 - cohort.PnETSpecies.BGBiomassFrac);
                        float changeAmount = newTotalBiomass - cohort.TotalBiomass;
                        float tempMaxBio = Math.Max(cohort.MaxBiomass, newTotalBiomass);
                        float tempFActiveBiom = (float)Math.Exp(-cohort.PnETSpecies.LiveWoodBiomassFrac * tempMaxBio);
                        float cohortTempFoliage = cohort.adjFolBiomassFrac * tempFActiveBiom * newTotalBiomass;
                        float cohortTempLAI = 0;
                        for (int i = 0; i < Globals.IMAX; i++)
                            cohortTempLAI += cohort.CalcLAI(cohort.PnETSpecies, cohortTempFoliage, i, cohortTempLAI);
                        cohortTempLAI = Math.Min(cohortTempLAI, cohort.PnETSpecies.MaxLAI);
                        float tempBiomass = (newTotalBiomass * (1 - cohort.PnETSpecies.BGBiomassFrac) + cohortTempFoliage) * Math.Min(cohortTempLAI / cohort.PnETSpecies.MaxLAI, cohort.CanopyGrowingSpace);
                        if (CohortStacking)
                            tempBiomass = (newTotalBiomass * (1 - cohort.PnETSpecies.BGBiomassFrac) + cohortTempFoliage) * 1.0f;
                        if ((attempts < 3) && ((tempBiomass <= 0) || float.IsNaN(tempBiomass)))
                        {
                            badSpinup = true;
                            break;
                        }
                        float diff = tempBiomass - targetBiomass;
                        float lastDiff = diff;
                        bool match = Math.Abs(tempBiomass - targetBiomass) < 2;
                        int loopCount = 0;
                        while (!match)
                        {
                            float multiplier = 1f;
                            if (Math.Abs(tempBiomass - targetBiomass) > 1000)
                                multiplier = 200f;
                            else if (Math.Abs(tempBiomass - targetBiomass) > 500)
                                multiplier = 100f;
                            else if (Math.Abs(tempBiomass - targetBiomass) > 100)
                                multiplier = 20f;
                            else if (Math.Abs(tempBiomass - targetBiomass) > 50)
                                multiplier = 10f;
                            else if (Math.Abs(tempBiomass - targetBiomass) > 10)
                                multiplier = 2f;
                            if (tempBiomass > targetBiomass)
                                newTotalBiomass = Math.Max(newTotalBiomass - multiplier, 1);
                            else
                                newTotalBiomass = Math.Max(newTotalBiomass + multiplier, 1);
                            changeAmount = newTotalBiomass - cohort.TotalBiomass;
                            tempMaxBio = Math.Max(cohort.MaxBiomass, newTotalBiomass);
                            tempFActiveBiom = (float)Math.Exp(-cohort.PnETSpecies.LiveWoodBiomassFrac * tempMaxBio);
                            cohortTempFoliage = cohort.adjFolBiomassFrac * tempFActiveBiom * newTotalBiomass;
                            cohortTempLAI = 0;
                            for (int i = 0; i < Globals.IMAX; i++)
                                cohortTempLAI += cohort.CalcLAI(cohort.PnETSpecies, cohortTempFoliage, i, cohortTempLAI);
                            cohortTempLAI = Math.Min(cohortTempLAI, cohort.PnETSpecies.MaxLAI);
                            tempBiomass = (newTotalBiomass * (1 - cohort.PnETSpecies.BGBiomassFrac) + cohortTempFoliage) * Math.Min(cohortTempLAI / cohort.PnETSpecies.MaxLAI, cohort.CanopyGrowingSpace);
                            if (CohortStacking)
                                tempBiomass = (newTotalBiomass * (1 - cohort.PnETSpecies.BGBiomassFrac) + cohortTempFoliage) * 1.0f;
                            diff = tempBiomass - targetBiomass;
                            if (Math.Abs(diff) > Math.Abs(lastDiff))
                            {
                                if ((Math.Abs(diff) / targetBiomass > 0.10) && (attempts < 3))
                                    badSpinup = true;
                                break;
                            }
                            if ((attempts < 3) && ((tempBiomass <= 0) || float.IsNaN(tempBiomass)))
                            {
                                badSpinup = true;
                                break;
                            }
                            match = Math.Abs(tempBiomass - targetBiomass) < 2;
                            loopCount++;
                            if (loopCount > 1000)
                                break;
                        }
                        if (badSpinup)
                            break;
                        if (loopCount <= 1000)
                        {
                            float cohortFoliage = cohort.adjFolBiomassFrac * tempFActiveBiom * newTotalBiomass;
                            cohort.Fol = cohortFoliage;
                            cohort.ChangeBiomass((int)Math.Round((newTotalBiomass - cohort.TotalBiomass) * 1f / 1f));
                        }
                        else
                            cohort.Fol = cohort.adjFolBiomassFrac * cohort.FActiveBiom * cohort.TotalBiomass;
                        float cohortLAI = 0;
                        for (int i = 0; i < Globals.IMAX; i++)
                            cohortLAI += cohort.CalcLAI(cohort.PnETSpecies, cohort.Fol, i, cohortLAI);
                        cohortLAI = Math.Min(cohortLAI, cohort.PnETSpecies.MaxLAI);
                        cohort.LastLAI = cohortLAI;
                        cohort.CanopyLayerFrac = Math.Min(cohort.LastLAI / cohort.PnETSpecies.MaxLAI, cohort.CanopyGrowingSpace);
                        if (CohortStacking)
                            cohort.CanopyLayerFrac = 1.0f;
                        CanopyLayerSum[layer] += cohort.CanopyLayerFrac;
                        cohort.Fol = cohort.Fol * (1 - cohort.PnETSpecies.FolTurnoverRate);
                        cohort.NSC = cohort.PnETSpecies.NSCFrac * cohort.FActiveBiom * (cohort.TotalBiomass + cohort.Fol) * cohort.PnETSpecies.CFracBiomass;
                        float fol_total_ratio = cohort.Fol / (cohort.Fol + cohort.Wood);
                        // Calculate minimum foliage/total biomass ratios from Jenkins (reduced by MinFolRatioFactor to be not so strict)
                        float ratioLimit = 0;
                        if (SpeciesParameters.PnETSpecies[cohort.Species].SLWDel == 0) //Conifer
                            ratioLimit = 0.057f * minFolRatioFactor;
                        else
                            ratioLimit = 0.019f * minFolRatioFactor;
                        if ((attempts < 3) && (fol_total_ratio < ratioLimit))
                        {
                            badSpinup = true;
                            break;
                        }
                        cohortIndex++;
                    }
                    if (badSpinup)
                    {
                        if ((initialCommunitiesSpinup.ToLower() == "spinuplayers") && (attempts < 2))
                        {
                            Globals.ModelCore.UI.WriteLine("");
                            Globals.ModelCore.UI.WriteLine("Warning: initial community " + initialCommunity.MapCode + " could not initialize properly using SpinupLayers.  Processing with SpinupLayersRescale option instead.");
                            ClearAllCohorts();
                            SpinUp(StartDate, site, initialCommunity, usingClimateLibrary, null, false);
                            // species-age key to store maxbiomass values and canopy growing space
                            Dictionary<ISpecies, Dictionary<int, float[]>> cohortDictionary = new Dictionary<ISpecies, Dictionary<int, float[]>>();
                            foreach (Cohort cohort in AllCohorts)
                            {
                                ISpecies spp = cohort.Species;
                                int age = cohort.Age;
                                float lastSeasonAvgFRad = cohort.LastSeasonFRad.ToArray().Average();
                                if (cohortDictionary.ContainsKey(spp))
                                {
                                    if (cohortDictionary[spp].ContainsKey(age))
                                    {
                                        // FIXME - message duplicate species and age
                                    }
                                    else
                                    {
                                        float[] values = new float[] { (int)cohort.MaxBiomass, cohort.Biomass, lastSeasonAvgFRad };
                                        cohortDictionary[spp].Add(age, values);
                                    }
                                }
                                else
                                {
                                    Dictionary<int, float[]> ageDictionary = new Dictionary<int, float[]>();
                                    float[] values = new float[] { (int)cohort.MaxBiomass, cohort.Biomass, lastSeasonAvgFRad };
                                    ageDictionary.Add(age, values);
                                    cohortDictionary.Add(spp, ageDictionary);
                                }
                            }
                            ClearAllCohorts();
                            CohortBiomassList = new List<double>();
                            CohortMaxBiomassList = new List<double>();
                            foreach (Landis.Library.UniversalCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                            {
                                foreach (Landis.Library.UniversalCohorts.ICohort cohort in speciesCohorts)
                                {
                                    int age = cohort.Data.Age;
                                    ISpecies spp = cohort.Species;
                                    float[] values = cohortDictionary[spp][age];
                                    int cohortMaxBiomass = (int)values[0];
                                    float cohortSpinupBiomass = values[1];
                                    float lastSeasonAvgFRad = values[2];
                                    float inputMaxBiomass = Math.Max(cohortMaxBiomass, cohort.Data.Biomass);
                                    inputMaxBiomass = cohortMaxBiomass * (cohort.Data.Biomass / cohortSpinupBiomass);
                                    float cohortCanopyGrowingSpace = 1f;
                                    bool addCohort = AddNewCohort(new Cohort(SpeciesParameters.PnETSpecies[cohort.Species], cohort.Data.Age, cohort.Data.Biomass, (int)inputMaxBiomass, cohortCanopyGrowingSpace, SiteOutputName, (ushort)(StartDate.Year - cohort.Data.Age), CohortStacking, lastSeasonAvgFRad));
                                    CohortBiomassList.Add(AllCohorts.Last().AGBiomass);
                                    CohortMaxBiomassList.Add(AllCohorts.Last().MaxBiomass);
                                    AllCohorts.Last().SetAvgFRad(lastSeasonAvgFRad);
                                }
                            }
                            badSpinup = false;
                        }
                        else if ((initialCommunitiesSpinup.ToLower() == "spinuplayersrescale") && (attempts < 2))
                        {
                            Globals.ModelCore.UI.WriteLine("");
                            Globals.ModelCore.UI.WriteLine("Warning: initial community " + initialCommunity.MapCode + " could not initialize properly using SpinupLayersRescale.  Processing with SpinupLayers option instead.");
                            ClearAllCohorts();
                            SpinUp(StartDate, site, initialCommunity, usingClimateLibrary, null, false);
                            // species-age key to store maxbiomass values, biomass, LastSeasonFRad
                            Dictionary<ISpecies, Dictionary<int, float[]>> cohortDictionary = new Dictionary<ISpecies, Dictionary<int, float[]>>();
                            foreach (Cohort cohort in AllCohorts)
                            {
                                ISpecies spp = cohort.Species;
                                int age = cohort.Age;
                                float lastSeasonAvgFRad = cohort.LastSeasonFRad.ToArray().Average();
                                if (cohortDictionary.ContainsKey(spp))
                                {
                                    if (cohortDictionary[spp].ContainsKey(age))
                                    {
                                        // FIXME - message duplicate species and age
                                    }
                                    else
                                    {
                                        float[] values = new float[] { (int)cohort.MaxBiomass, cohort.Biomass, lastSeasonAvgFRad };
                                        cohortDictionary[spp].Add(age, values);
                                    }
                                }
                                else
                                {
                                    Dictionary<int, float[]> ageDictionary = new Dictionary<int, float[]>();
                                    float[] values = new float[] { (int)cohort.MaxBiomass, cohort.Biomass, lastSeasonAvgFRad };
                                    ageDictionary.Add(age, values);
                                    cohortDictionary.Add(spp, ageDictionary);
                                }

                            }
                            ClearAllCohorts();
                            CohortBiomassList = new List<double>();
                            CohortMaxBiomassList = new List<double>();
                            foreach (Landis.Library.UniversalCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                            {
                                foreach (Landis.Library.UniversalCohorts.ICohort cohort in speciesCohorts)
                                {
                                    int age = cohort.Data.Age;
                                    ISpecies spp = cohort.Species;
                                    float[] values = cohortDictionary[spp][age];
                                    int cohortMaxBiomass = (int)values[0];
                                    float cohortSpinupBiomass = values[1];
                                    float lastSeasonAvgFRad = values[2];
                                    float inputMaxBiomass = Math.Max(cohortMaxBiomass, cohort.Data.Biomass);
                                    float cohortCanopyGrowingSpace = 1f;
                                    bool addCohort = AddNewCohort(new Cohort(SpeciesParameters.PnETSpecies[cohort.Species], cohort.Data.Age, cohort.Data.Biomass, (int)inputMaxBiomass, cohortCanopyGrowingSpace, SiteOutputName, (ushort)(StartDate.Year - cohort.Data.Age), CohortStacking, lastSeasonAvgFRad));
                                    CohortBiomassList.Add(AllCohorts.Last().AGBiomass);
                                    CohortMaxBiomassList.Add(AllCohorts.Last().MaxBiomass);
                                    AllCohorts.Last().SetAvgFRad(lastSeasonAvgFRad);
                                }
                            }
                            badSpinup = false;
                        }
                        else // NoSpinup or secondAttempt
                        {
                            Globals.ModelCore.UI.WriteLine("");
                            if (initialCommunitiesSpinup.ToLower() == "nospinup")
                                Globals.ModelCore.UI.WriteLine("Warning: initial community " + initialCommunity.MapCode + " could not initialize properly on first attempt using NoSpinup. Reprocessing.");
                            else
                                Globals.ModelCore.UI.WriteLine("Warning: initial community " + initialCommunity.MapCode + " could not initialize properly using SpinupLayers or SpinupLayersRescale.  Processing with NoSpinup option instead.");
                            ClearAllCohorts();
                            CohortBiomassList = new List<double>();
                            CohortMaxBiomassList = new List<double>();
                            foreach (Landis.Library.UniversalCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                            {
                                foreach (Landis.Library.UniversalCohorts.ICohort cohort in speciesCohorts)
                                {
                                    // TODO: Add warning if biomass is 0
                                    bool addCohort = AddNewCohort(new Cohort(SpeciesParameters.PnETSpecies[cohort.Species], cohort.Data.Age, cohort.Data.Biomass, SiteOutputName, (ushort)(StartDate.Year - cohort.Data.Age), CohortStacking));
                                    CohortBiomassList.Add(AllCohorts.Last().AGBiomass);
                                    CohortMaxBiomassList.Add(AllCohorts.Last().MaxBiomass);
                                }
                            }
                        }
                    }
                    else
                    {
                        this.canopylaimax = CanopyLAISum.Sum();
                        runAgain = false;
                    }
                }
            }
            else
                SpinUp(StartDate, site, initialCommunity, usingClimateLibrary, SiteOutputName);
        }

        /// <summary>
        /// Constructor for SiteCohorts that have an initial site already set up
        /// </summary>
        /// <param name="StartDate"></param>
        /// <param name="site"></param>
        /// <param name="initialCommunity"></param>
        /// <param name="SiteOutputName"></param>
        public SiteCohorts(DateTime StartDate, ActiveSite site, ICommunity initialCommunity, string SiteOutputName = null)
        {
            this.Ecoregion = EcoregionData.GetPnETEcoregion(Globals.ModelCore.Ecoregion[site]);
            this.Site = site;
            cohorts = new Dictionary<ISpecies, List<Cohort>>();
            SpeciesEstablishedByPlant = new List<ISpecies>();
            SpeciesEstablishedBySerotiny = new List<ISpecies>();
            SpeciesEstablishedByResprout = new List<ISpecies>();
            SpeciesEstablishedBySeed = new List<ISpecies>();
            CohortsKilledBySuccession = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByCold = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByHarvest = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByFire = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByWind = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByOther = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            DisturbanceTypesReduced = new List<ExtensionType>();
            uint key = CalcKey((ushort)initialCommunity.MapCode, Globals.ModelCore.Ecoregion[site].MapCode);
            if (initialSites.ContainsKey(key))
            {
                if (SiteOutputName != null)
                {
                    this.siteoutput = new LocalOutput(SiteOutputName, "Site.csv", Header(site));
                    probEstablishment = new ProbEstablishment(SiteOutputName, "Establishment.csv");
                }
                else
                    probEstablishment = new ProbEstablishment(null, null);
                subcanopypar = initialSites[key].subcanopypar;
                subcanopyparmax = initialSites[key].SubCanopyParMAX;
                avgSoilWaterContent = initialSites[key].wateravg;
                hydrology = new Hydrology(initialSites[key].hydrology.SoilWaterContent);
                SiteVars.WoodDebris[Site] = SiteVars.WoodDebris[initialSites[key].Site].Clone();
                SiteVars.Litter[Site] = SiteVars.Litter[initialSites[key].Site].Clone();
                SiteVars.FineFuels[Site] = SiteVars.Litter[Site].Mass;
                SiteVars.MonthlyPressureHead[site] = (float[])SiteVars.MonthlyPressureHead[initialSites[key].Site].Clone();
                this.canopylaimax = initialSites[key].CanopyLAImax;
                List<float> cohortBiomassLayerFrac = new List<float>();
                List<float> cohortCanopyLayerFrac = new List<float>();
                List<float> cohortCanopyGrowingSpace = new List<float>();
                List<float> cohortLastLAI = new List<float>();
                List<float> cohortLastWoodSenescence = new List<float>();
                List<float> cohortLastFolSenescence = new List<float>();
                List<float> cohortLastYearAvgFRad = new List<float>();
                foreach (ISpecies spc in initialSites[key].cohorts.Keys)
                {
                    foreach (Cohort cohort in initialSites[key].cohorts[spc])
                    {
                        bool addCohort = false;
                        if (SiteOutputName != null)
                            addCohort = AddNewCohort(new Cohort(cohort, (ushort)(StartDate.Year - cohort.Age), SiteOutputName));
                        else
                            addCohort = AddNewCohort(new Cohort(cohort));
                        float biomassLayerFrac = cohort.BiomassLayerFrac;
                        cohortBiomassLayerFrac.Add(biomassLayerFrac);
                        float canopyLayerFrac = cohort.CanopyLayerFrac;
                        cohortCanopyLayerFrac.Add(canopyLayerFrac);
                        float canopyGrowingSpace = cohort.CanopyGrowingSpace;
                        cohortCanopyGrowingSpace.Add(canopyGrowingSpace);
                        float lastLAI = cohort.LastLAI;
                        cohortLastLAI.Add(lastLAI);
                        float lastWoodSenes = cohort.LastWoodSenescence;
                        cohortLastWoodSenescence.Add(lastWoodSenes);
                        float lastFolSenes = cohort.LastFoliageSenescence;
                        cohortLastFolSenescence.Add(lastFolSenes);
                    }
                }
                int index = 0;
                foreach (Cohort cohort in AllCohorts)
                {
                    cohort.BiomassLayerFrac = cohortBiomassLayerFrac[index];
                    cohort.CanopyLayerFrac = cohortCanopyLayerFrac[index];
                    cohort.CanopyGrowingSpace = cohortCanopyGrowingSpace[index];
                    cohort.LastLAI = cohortLastLAI[index];
                    cohort.LastWoodSenescence = cohortLastWoodSenescence[index];
                    cohort.LastFoliageSenescence = cohortLastFolSenescence[index];
                    index++;
                }
                SiteVars.MonthlySoilTemp[site] = new SortedList<float, float>[SiteVars.MonthlyPressureHead[site].Count()];
                for (int m = 0; m < SiteVars.MonthlyPressureHead[site].Count(); m++)
                {
                    SiteVars.MonthlySoilTemp[site][m] = SiteVars.MonthlySoilTemp[initialSites[key].Site][m];
                }
                this.netpsn = initialSites[key].NetPsn;
                this.foliarRespiration = initialSites[key].FoliarRespiration;
                this.grosspsn = initialSites[key].GrossPsn;
                this.maintresp = initialSites[key].MaintResp;
                this.averageAlbedo = initialSites[key].AverageAlbedo;
                this.CanopyLAI = initialSites[key].CanopyLAI;
                this.transpiration = initialSites[key].Transpiration;
                this.potentialTranspiration = initialSites[key].PotentialTranspiration;
                // Calculate AdjFolFrac
                AllCohorts.ForEach(x => x.CalcAdjFolBiomassFrac());
            }
        }

        /// <summary>
        /// Spin up sites if no biomass is provided
        /// </summary>
        /// <param name="StartDate"></param>
        /// <param name="site"></param>
        /// <param name="initialCommunity"></param>
        /// <param name="usingClimateLibrary"></param>
        /// <param name="SiteOutputName"></param>
        /// <param name="AllowMortality"></param>
        /// <exception cref="System.Exception"></exception>
        private void SpinUp(DateTime StartDate, ActiveSite site, ICommunity initialCommunity, bool usingClimateLibrary, string SiteOutputName = null, bool AllowMortality = true)
        {
            List<Landis.Library.UniversalCohorts.ICohort> sortedAgeCohorts = new List<Landis.Library.UniversalCohorts.ICohort>();
            foreach (var speciesCohorts in initialCommunity.Cohorts)
            {
                foreach (Landis.Library.UniversalCohorts.ICohort cohort in speciesCohorts)
                {
                    sortedAgeCohorts.Add(cohort);
                }
            }
            sortedAgeCohorts = new List<Library.UniversalCohorts.ICohort>(sortedAgeCohorts.OrderByDescending(o => o.Data.Age));
            if (sortedAgeCohorts.Count == 0)
                return;
            List<double> CohortMaxBiomassList = new List<double>();
            DateTime date = StartDate.AddYears(-(sortedAgeCohorts[0].Data.Age - 1));
            Landis.Library.Parameters.Ecoregions.AuxParm<List<EcoregionPnETVariables>> mydata = new Library.Parameters.Ecoregions.AuxParm<List<EcoregionPnETVariables>>(Globals.ModelCore.Ecoregions);
            while (date.CompareTo(StartDate) <= 0)
            {
                //  Add those cohorts that were born at the current year
                while (sortedAgeCohorts.Count() > 0 && StartDate.Year - date.Year == (sortedAgeCohorts[0].Data.Age - 1))
                {
                    Cohort cohort = new Cohort(sortedAgeCohorts[0].Species, SpeciesParameters.PnETSpecies[sortedAgeCohorts[0].Species], (ushort)date.Year, SiteOutputName,1, CohortStacking);
                    if (CohortStacking)
                    {
                        cohort.CanopyLayerFrac = 1.0f;
                        cohort.CanopyGrowingSpace = 1.0f;
                    }
                    bool addCohort = AddNewCohort(cohort);
                    sortedAgeCohorts.Remove(sortedAgeCohorts[0]);
                }
                // Simulation time runs until the next cohort is added
                DateTime EndDate = (sortedAgeCohorts.Count == 0) ? StartDate : new DateTime((int)(StartDate.Year - (sortedAgeCohorts[0].Data.Age - 1)), 1, 15);
                if (date.CompareTo(StartDate) == 0)
                    break;
                var climate_vars = usingClimateLibrary ? EcoregionData.GetClimateRegionData(Ecoregion, date, EndDate) : EcoregionData.GetData(Ecoregion, date, EndDate);
                Grow(climate_vars, AllowMortality,SiteOutputName != null);
                date = EndDate;
            }
            if (sortedAgeCohorts.Count > 0)
                throw new System.Exception("Not all cohorts in the initial communities file were initialized.");
        }

        List<List<int>> GetRandomRange(List<List<int>> bins)
        {
            List<List<int>> random_range = new List<List<int>>();
            if (bins != null) for (int b = 0; b < bins.Count(); b++)
                {
                    random_range.Add(new List<int>());
                    List<int> copy_range = new List<int>(bins[b]);
                    while (copy_range.Count() > 0)
                    {
                        int k = Statistics.DiscreteUniformRandom(0, copy_range.Count()-1);
                        random_range[b].Add(copy_range[k]);
                        copy_range.RemoveAt(k);
                    }
                }

            return random_range;
        }

        public void SetActualET(float value, int Month)
        {
            ActualET[Month-1] = value;
        }

        public void SetPotentialET(float value)
        {
            PotentialET = value;
        }

        class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
        }

        private static float CalcMaxSnowMelt(float Tavg, float DaySpan)
        {
            // Snowmelt rate can range between 1.6 to 6.0 mm/degree day, and default should be 2.74 according to NRCS Part 630 Hydrology National Engineering Handbook (Chapter 11: Snowmelt)
            return (float)2.74f * Math.Max(0, Tavg) * DaySpan;
        }

        private static float CalcSnowFrac(float Tavg)
        {
            return (float)Math.Max(0.0, Math.Min(1.0, (Tavg - 2) / -7));
        }

        public bool Grow(List<IEcoregionPnETVariables> data, bool AllowMortality = true, bool outputCohortData = true)
        {
            bool success = true;
            float sumPressureHead = 0;
            int countPressureHead = 0;
            probEstablishment.Reset();
            canopylaimax = float.MinValue;
            int tempMaxCanopyLayers = MaxCanopyLayers;
            if (CohortStacking)
                tempMaxCanopyLayers = AllCohorts.Count();
            SortedDictionary<double, Cohort> SubCanopyCohorts = new SortedDictionary<double, Cohort>();
            List<double> CohortBiomassList = new List<double>();
            List<double> CohortMaxBiomassList = new List<double>();
            int SiteAboveGroundBiomass = AllCohorts.Sum(a => a.AGBiomass);
            MaxLayer = 0;
            for (int cohort = 0; cohort < AllCohorts.Count(); cohort++)
            {
                if (Globals.ModelCore.CurrentTime > 0)
                    AllCohorts[cohort].CalcDefoliationFrac(Site, SiteAboveGroundBiomass);
                CohortBiomassList.Add(AllCohorts[cohort].TotalBiomass);
                CohortMaxBiomassList.Add(AllCohorts[cohort].MaxBiomass);
            }
            ratioAbove10.Clear();
            List<List<double>> cohortBins = GetBinsByCohort(CohortMaxBiomassList);
            List<int> cohortAges = new List<int>();
            List<List<int>> rawBins = new List<List<int>>();
            int subLayerIndex = 0;
            bool reducedLayer = false;
            for (int cohort = 0; cohort < AllCohorts.Count(); cohort++)
            {
                string lifeForm = AllCohorts[cohort].PnETSpecies.Lifeform.ToLower();
                int cohortLayer = 0;
                // Lifeform "ground" always restricted to layer 0
                if (!lifeForm.Contains("ground"))
                {
                    for (int j = 0; j < cohortBins.Count(); j++)
                    {
                        if (cohortBins[j].Contains(AllCohorts[cohort].MaxBiomass))
                            cohortLayer = j;
                    }
                    if (AllCohorts[cohort].Layer > MaxLayer)
                        MaxLayer = AllCohorts[cohort].Layer;
                }
                for (int i = 1; i <= Globals.IMAX; i++)
                {
                    SubCanopyCohorts.Add(subLayerIndex, AllCohorts[cohort]);
                    while (rawBins.Count() < (cohortLayer + 1))
                    {
                        List<int> subList = new List<int>();
                        rawBins.Add(subList);
                    }
                    rawBins[cohortLayer].Add(subLayerIndex);
                    subLayerIndex++;
                }
                if (!cohortAges.Contains(AllCohorts[cohort].Age))
                    cohortAges.Add(AllCohorts[cohort].Age);
            }
            List<List<int>> LayeredBins = new List<List<int>>();            
            LayeredBins = rawBins;
            nlayers = 0;
            foreach (List<int> layerList in LayeredBins)
            {
                if (layerList.Count > 0)
                    nlayers++;
            }
            MaxLayer = LayeredBins.Count - 1;
            List<List<int>> random_range = GetRandomRange(LayeredBins);
            foliarRespiration = new float[13];
            netpsn = new float[13];
            grosspsn = new float[13];
            maintresp = new float[13];
            averageAlbedo = new float[13];
            activeLayerDepth = new float[13];
            frostDepth = new float[13];
            monthCount = new float[13];
            monthlySnowpack = new float[13];
            monthlyWater = new float[13];
            monthlyLAI = new float[13];
            monthlyLAICumulative = new float[13];
            monthlyEvap = new float[13];
            monthlyActualTrans = new float[13];
            monthlyInterception = new float[13];
            monthlyLeakage = new float[13];
            monthlyRunoff = new float[13];
            monthlyActualET = new float[13];
            monthlyPotentialEvap = new float[13];
            monthlyPotentialTrans = new float[13];
            Dictionary<IPnETSpecies, float> cumulativeEstab = new Dictionary<IPnETSpecies, float>();
            Dictionary<IPnETSpecies, List<float>> annualFWater = new Dictionary<IPnETSpecies, List<float>>();
            Dictionary<IPnETSpecies, float> cumulativeFWater = new Dictionary<IPnETSpecies, float>();
            Dictionary<IPnETSpecies, List<float>> annualFRad = new Dictionary<IPnETSpecies, List<float>>();
            Dictionary<IPnETSpecies, float> cumulativeFRad = new Dictionary<IPnETSpecies, float>();
            Dictionary<IPnETSpecies, float> monthlyEstab = new Dictionary<IPnETSpecies, float>();
            Dictionary<IPnETSpecies, int> monthlyCount = new Dictionary<IPnETSpecies, int>();
            Dictionary<IPnETSpecies, int> coldKillMonth = new Dictionary<IPnETSpecies, int>(); // month in which cold kills each species
            foreach (IPnETSpecies spc in SpeciesParameters.PnETSpecies.AllSpecies)
            {
                cumulativeEstab[spc] = 1;
                annualFWater[spc] = new List<float>();
                cumulativeFWater[spc] = 0;
                annualFRad[spc] = new List<float>();
                cumulativeFRad[spc] = 0;
                monthlyCount[spc] = 0;
                coldKillMonth[spc] = int.MaxValue;
            }
            float[] lastOzoneEffect = new float[SubCanopyCohorts.Count()];
            for (int i = 0; i < lastOzoneEffect.Length; i++)
            {
                lastOzoneEffect[i] = 0;
            }
            float lastFracBelowFrost = hydrology.FrozenSoilDepth/Ecoregion.RootingDepth;
            int daysOfWinter = 0;
            if (Globals.ModelCore.CurrentTime > 0) // cold can only kill after spinup
            {
                // Loop through months & species to determine if cold temp would kill any species
                float extremeMinTemp = float.MaxValue;
                int extremeMonth = 0;
                for (int m = 0; m < data.Count(); m++)
                {
                    float minTemp = data[m].Tavg - (float)(3.0 * Ecoregion.WinterSTD);
                    if (minTemp < extremeMinTemp)
                    {
                        extremeMinTemp = minTemp;
                        extremeMonth = m;
                    }
                }
                SiteVars.ExtremeMinTemp[Site] = extremeMinTemp;
                foreach (IPnETSpecies spc in SpeciesParameters.PnETSpecies.AllSpecies)
                {
                    // Check if low temp kills species
                    if (extremeMinTemp < spc.ColdTolerance)
                        coldKillMonth[spc] = extremeMonth;
                }
            }
            //Clear pressurehead site values
            sumPressureHead = 0;
            countPressureHead = 0;
            SiteVars.MonthlyPressureHead[this.Site] = new float[data.Count()];
            SiteVars.MonthlySoilTemp[this.Site] = new SortedList<float, float>[data.Count()];
            for (int m = 0; m < data.Count(); m++)
            {
                Ecoregion.Variables = data[m];
                transpiration = 0;
                potentialTranspiration = 0;
                subcanopypar = data[m].PAR0;
                interception = 0;
                // Reset monthly variables that get reported as single year snapshots
                if (data[m].Month == 1)
                {
                    foliarRespiration = new float[13];
                    netpsn = new float[13];
                    grosspsn = new float[13];
                    maintresp = new float[13];
                    averageAlbedo = new float[13];
                    activeLayerDepth = new float[13];
                    frostDepth = new float[13];
                    // Reset annual SiteVars
                    SiteVars.AnnualPotentialEvaporation[Site] = 0;
                    SiteVars.ClimaticWaterDeficit[Site] = 0;
                    canopylaimax = float.MinValue;
                    monthlyLAI = new float[13];
                    // Reset max foliage and AdjFolBiomassFrac in each cohort
                    foreach (ISpecies spc in cohorts.Keys)
                    {
                        foreach (Cohort cohort in cohorts[spc])
                        {
                            cohort.ResetFoliageMax();
                            cohort.LastAGBio = cohort.AGBiomass;
                            cohort.CalcAdjFolBiomassFrac();
                            cohort.ClearFRad();
                        }
                    }
                }
                float ozoneD40 = 0;
                if (m > 0)
                    ozoneD40 = Math.Max(0, data[m].O3 - data[m - 1].O3);
                else
                    ozoneD40 = data[m].O3;
                float O3_D40_ppmh = ozoneD40 / 1000; // convert D40 units to ppm h
                // Melt snow
                float snowmelt = Math.Min(snowpack, CalcMaxSnowMelt(data[m].Tavg, data[m].DaySpan)); // mm
                if (snowmelt < 0)
                    throw new System.Exception("Error, snowmelt = " + snowmelt + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                float newSnow = CalcSnowFrac(data[m].Tavg) * data[m].Prec;
                float newSnowDepth = newSnow * (1 - Ecoregion.SnowSublimFrac); // (mm) Account for sublimation here
                if (newSnowDepth < 0 || newSnowDepth > data[m].Prec)
                    throw new System.Exception("Error, newSnowDepth = " + newSnowDepth + " availablePrecipitation = " + data[m].Prec);
                snowpack += newSnowDepth - snowmelt;
                if (snowpack < 0)
                    throw new System.Exception("Error, snowpack = " + snowpack + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                fracRootAboveFrost = 1;
                leakageFrac = Ecoregion.LeakageFrac;
                float fracThawed = 0;
                // Soil temp calculations - need for permafrost and Root Rot
                // snow calculations - from "Soil thawing worksheet with snow.xlsx"
                if (data[m].Tavg <= 0)
                    daysOfWinter += (int)data[m].DaySpan;
                else if (snowpack > 0)
                    daysOfWinter += (int)data[m].DaySpan;
                else
                    daysOfWinter = 0;
                float densitySnow_kg_m3 = Snow.CalcDensity(daysOfWinter);
                float snowDepth = Snow.CalcDepth(densitySnow_kg_m3, snowpack);
                if (lastTempBelowSnow == float.MaxValue)
                {
                    float thermalConductivity_Snow = Snow.CalcThermalConductivity(densitySnow_kg_m3);
                    float thermalDamping_Snow = Snow.CalcThermalDamping(thermalConductivity_Snow);
                    float DRz_snow = Snow.CalcDampingRatio(snowDepth, thermalDamping_Snow);
                    // Damping ratio for moss - adapted from Kang et al. (2000) and Liang et al. (2014)
                    float DRz_moss = (float)Math.Exp(-1.0F * this.SiteMossDepth * Constants.ThermalDampingMoss); 
                    // Soil diffusivity 
                    float soilWaterContent = hydrology.SoilWaterContent;// volumetric m/m
                    float soilPorosity = Ecoregion.Porosity;  // volumetric m/m 
                    float ga = 0.035F + 0.298F * (soilWaterContent / soilPorosity);
                    // ratio of air temp gradient
                    float Fa = (2.0F / 3.0F / (1.0F + ga * ((Constants.ThermalConductivityAir_kJperday / Constants.ThermalConductivityWater_kJperday) - 1.0F))) + (1.0F / 3.0F / (1.0F + (1.0F - 2.0F * ga) * ((Constants.ThermalConductivityAir_kJperday / Constants.ThermalConductivityWater_kJperday) - 1.0F)));
                    float Fs = Hydrology_SaxtonRawls.GetFs(Ecoregion.SoilType);
                    float ThermalConductivitySoil = Hydrology_SaxtonRawls.GetThermalConductivitySoil(Ecoregion.SoilType);
                    // soil thermal conductivity (kJ/m.d.K)
                    float ThermalConductivity_theta = (Fs * (1.0F - soilPorosity) * ThermalConductivitySoil + Fa * (soilPorosity - soilWaterContent) * Constants.ThermalConductivityAir_kJperday + soilWaterContent * Constants.ThermalConductivityWater_kJperday) / (Fs * (1.0F - soilPorosity) + Fa * (soilPorosity - soilWaterContent) + soilWaterContent);
                    float D = ThermalConductivity_theta / Hydrology_SaxtonRawls.GetCTheta(Ecoregion.SoilType);  // m2/day
                    float Dmms = D * 1000000F / Constants.SecondsPerDay; // mm2/s
                    soilDiffusivity = Dmms;
                    float Dmonth = D * data[m].DaySpan; // m2/month
                    float ks = Dmonth * 1000000F / (data[m].DaySpan * Constants.SecondsPerDay); // mm2/s
                    float d = (float)Math.Sqrt(2 * Dmms / Constants.omega);
                    float maxDepth = Ecoregion.RootingDepth + Ecoregion.LeakageFrostDepth;
                    float lastBelowZeroDepth = 0;
                    float bottomFreezeDepth = maxDepth / 1000;
                    activeLayerDepth[data[m].Month - 1] = bottomFreezeDepth;
                    // When there was permafrost at the end of summer, assume that the bottom of the ice lens is as deep as possible
                    frostDepth[data[m].Month - 1] = bottomFreezeDepth;
                    float testDepth = 0;
                    float zTemp = 0;
                    float tSum = 0;
                    float pSum = 0;
                    float tmax = float.MinValue;
                    float tmin = float.MaxValue;
                    int maxMonth = 0;
                    int minMonth = 0;
                    int mCount = 0;
                    if (m < 12)
                    {
                        mCount = Math.Min(12, data.Count());
                        foreach (int z in Enumerable.Range(0, mCount))
                        {
                            tSum += data[z].Tavg;
                            pSum += data[z].Prec;
                            if (data[z].Tavg > tmax)
                            {
                                tmax = data[z].Tavg;
                                maxMonth = data[z].Month;
                            }
                            if (data[z].Tavg < tmin)
                            {
                                tmin = data[z].Tavg;
                                minMonth = data[z].Month;
                            }
                        }
                    }
                    else
                    {
                        mCount = 12;
                        foreach (int z in Enumerable.Range(m - 11, 12))
                        {
                            tSum += data[z].Tavg;
                            pSum += data[z].Prec;
                            if (data[z].Tavg > tmax)
                            {
                                tmax = data[z].Tavg;
                                maxMonth = data[z].Month;
                            }
                            if (data[z].Tavg < tmin)
                            {
                                tmin = data[z].Tavg;
                                minMonth = data[z].Month;
                            }
                        }
                    }
                    float annualTavg = tSum / mCount;
                    float annualPcpAvg = pSum / mCount;
                    float tAmplitude = (tmax - tmin) / 2;
                    float tempBelowSnow = Ecoregion.Variables.Tavg;
                    if (snowDepth > 0)
                        tempBelowSnow = annualTavg + (Ecoregion.Variables.Tavg - annualTavg) * DRz_snow;
                    lastTempBelowSnow = tempBelowSnow;
                    // Regardless of permafrost, need to fill the tempDict with values
                    bool foundBottomIce = false;
                    // Calculate depth to bottom of ice lens with FrostDepth
                    while (testDepth <= bottomFreezeDepth)
                    {
                        // adapted from Kang et al. (2000) and Liang et al. (2014); added FrostFactor for calibration
                        float DRz = (float)Math.Exp(-1.0F * testDepth * d * Ecoregion.FrostFactor);
                        int lagMax = data[m].Month + (3 - maxMonth);
                        int lagMin = data[m].Month + (minMonth - 5);
                        if (minMonth >= 9)
                            lagMin = data[m].Month + (minMonth - 12 - 5);
                        float lagAvg = ((float)lagMax + (float)lagMin) / 2f;
                        zTemp = (float)(annualTavg + tAmplitude * DRz_snow * DRz_moss * DRz * Math.Sin(Constants.omega * lagAvg - testDepth / d));
                        depthTempDict[testDepth] = zTemp;
                        if (zTemp <= 0 && !permafrost)
                            lastBelowZeroDepth = testDepth;
                        if (zTemp > 0 && lastBelowZeroDepth > 0 && !foundBottomIce && !permafrost)
                        {
                            frostDepth[data[m].Month - 1] = lastBelowZeroDepth;
                            foundBottomIce = true;
                        }
                        if (zTemp <= 0)
                        {
                            if (testDepth < activeLayerDepth[data[m].Month - 1])
                                activeLayerDepth[data[m].Month - 1] = testDepth;
                        }
                        if (testDepth == 0f)
                            testDepth = 0.10f;
                        else if (testDepth == 0.10f)
                            testDepth = 0.25f;
                        else
                            testDepth += 0.25F;
                    }
                    // The ice lens is deeper than the max depth
                    if (zTemp <= 0 && !foundBottomIce && !permafrost)
                        frostDepth[data[m].Month - 1] = bottomFreezeDepth;
                }
                depthTempDict = FrozenSoils.CalcMonthlySoilTemps(depthTempDict, Ecoregion, daysOfWinter, snowpack, hydrology, lastTempBelowSnow);
                SortedList<float, float> monthlyDepthTempDict = new SortedList<float, float>();
                monthlyDepthTempDict.Add(0.1f, depthTempDict[0.1f]);
                lastTempBelowSnow = depthTempDict[0];
                if (soilIceDepth)
                {
                    // Calculate depth to bottom of ice lens with FrostDepth
                    float maxDepth = Ecoregion.RootingDepth + Ecoregion.LeakageFrostDepth;
                    float bottomFreezeDepth = maxDepth / 1000;
                    float lastBelowZeroDepth = 0;
                    float testDepth = 0;
                    bool foundBottomIce = false;
                    float zTemp = 0;
                    activeLayerDepth[data[m].Month - 1] = bottomFreezeDepth;
                    while (testDepth <= bottomFreezeDepth)
                    {
                        zTemp = depthTempDict[testDepth];
                        if (zTemp <= 0 && !permafrost)
                            lastBelowZeroDepth = testDepth;
                        if (zTemp > 0 && lastBelowZeroDepth > 0 && !foundBottomIce && !permafrost)
                        {
                            frostDepth[data[m].Month - 1] = lastBelowZeroDepth;
                            foundBottomIce = true;
                        }
                        if (zTemp <= 0)
                        {
                            if (testDepth < activeLayerDepth[data[m].Month - 1])
                                activeLayerDepth[data[m].Month - 1] = testDepth;
                        }
                        if (testDepth == 0f)
                            testDepth = 0.10f;
                        else if (testDepth == 0.10f)
                            testDepth = 0.25f;
                        else
                            testDepth += 0.25F;
                    }
                    // The ice lens is deeper than the max depth
                    if (zTemp <= 0 && !foundBottomIce && !permafrost)
                        frostDepth[data[m].Month - 1] = bottomFreezeDepth;
                    fracRootAboveFrost = Math.Min(1, activeLayerDepth[data[m].Month - 1] * 1000 / Ecoregion.RootingDepth);
                    float fracRootBelowFrost = 1 - fracRootAboveFrost;
                    fracThawed = Math.Max(0, fracRootAboveFrost - (1 - lastFracBelowFrost));
                    float fracNewFrozenSoil = Math.Max(0, fracRootBelowFrost - lastFracBelowFrost);
                    if (fracRootAboveFrost < 1) // If part of the rooting zone is frozen
                    {
                        if (fracNewFrozenSoil > 0)  // freezing
                        {
                            float newFrozenSoil = fracNewFrozenSoil * Ecoregion.RootingDepth;
                            bool successpct = hydrology.SetFrozenSoilWaterContent(((hydrology.FrozenSoilDepth * hydrology.FrozenSoilWaterContent) + (newFrozenSoil * hydrology.SoilWaterContent)) / (hydrology.FrozenSoilDepth + newFrozenSoil));
                            // Volume of rooting soil that is frozen
                            bool successdepth = hydrology.SetFrozenSoilDepth(Ecoregion.RootingDepth * fracRootBelowFrost);
                            // SoilWaterContent is a volumetric value (mm/m) so frozen water does not need to be removed, as the concentration stays the same
                        }
                    }
                    if (fracThawed > 0) // thawing
                    {
                        // Thawing soil water added to existing water - redistributes evenly in active soil
                        float existingWater = (1 - lastFracBelowFrost) * hydrology.SoilWaterContent;
                        float thawedWater = fracThawed * hydrology.FrozenSoilWaterContent;
                        float newWaterContent = (existingWater + thawedWater) / fracRootAboveFrost;
                        hydrology.AddWater(newWaterContent - hydrology.SoilWaterContent, Ecoregion.RootingDepth * fracRootBelowFrost);
                        // Volume of rooting soil that is frozen
                        bool successdepth = hydrology.SetFrozenSoilDepth(Ecoregion.RootingDepth * fracRootBelowFrost);  
                    }
                    float leakageFrostReduction = 1.0F;
                    if ((activeLayerDepth[data[m].Month - 1] * 1000) < (Ecoregion.RootingDepth + Ecoregion.LeakageFrostDepth))
                    {
                        if ((activeLayerDepth[data[m].Month - 1] * 1000) < Ecoregion.RootingDepth)
                            leakageFrostReduction = 0.0F;
                        else
                            leakageFrostReduction = (Math.Min(activeLayerDepth[data[m].Month - 1] * 1000, Ecoregion.LeakageFrostDepth) - Ecoregion.RootingDepth) / (Ecoregion.LeakageFrostDepth - Ecoregion.RootingDepth);
                    }
                    leakageFrac = Ecoregion.LeakageFrac * leakageFrostReduction;
                    lastFracBelowFrost = fracRootBelowFrost;
                }
                AllCohorts.ForEach(x => x.InitializeSubLayers());
                if (data[m].Prec < 0)
                    throw new System.Exception("Error, this.data[m].Prec = " + data[m].Prec + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                // Calculate above-canopy reference daily ET
                float ReferenceET = Evapotranspiration.CalcReferenceET_Hamon(data[m].Tavg, data[m].DayLength); //mm/day
                float newrain = data[m].Prec - newSnow;
                // Reduced by interception
                if (CanopyLAI == null)
                    CanopyLAI = new float[tempMaxCanopyLayers];
                interception = newrain * (float)(1 - Math.Exp(-1 * Ecoregion.PrecIntConst * CanopyLAI.Sum()));
                float surfaceRain = newrain - interception;
                // Reduced by PrecLossFrac
                precLoss = surfaceRain * Ecoregion.PrecLossFrac;
                float precin = surfaceRain - precLoss;
                if (precin < 0)
                    throw new System.Exception("Error, precin = " + precin + " newSnow = " + newSnow + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                // maximum number of precipitation events per month
                int numEvents = Ecoregion.PrecipEvents;
                // Divide precip into discrete events within the month
                float PrecInByEvent = precin / numEvents;
                if (PrecInByEvent < 0)
                    throw new System.Exception("Error, PrecInByEvent = " + PrecInByEvent + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                if (fracRootAboveFrost >= 1)
                {
                    bool successpct = hydrology.SetFrozenSoilWaterContent(0F);
                    bool successdepth = hydrology.SetFrozenSoilDepth(0F);
                }
                float MeltInWater = snowmelt;
                // Calculate ground PotentialET
                float groundPotentialET = Evapotranspiration.CalcPotentialGroundET_LAI(CanopyLAI.Sum(), data[m].Tavg, data[m].DayLength, data[m].DaySpan, ((Parameter<float>)Names.GetParameter("ETExtCoeff")).Value);
                float  groundPotentialETbyEvent = groundPotentialET / numEvents;  // divide evaporation into discreet events to match precip
                // Randomly choose which layers will receive the precip events
                // If # of layers < precipEvents, some layers will show up multiple times in number list.  This ensures the same number of precip events regardless of the number of cohorts
                List<int> randomNumbers = new List<int>();
                if (PrecipEventsWithReplacement)
                {
                    // Sublayer selection with replacement    
                    while (randomNumbers.Count < numEvents)
                    {
                        int rand = Statistics.DiscreteUniformRandom(1, SubCanopyCohorts.Count());
                        randomNumbers.Add(rand);
                    }
                }
                else
                {
                    // Sublayer selection without replacement
                    if (SubCanopyCohorts.Count() > 0)
                    {
                        while (randomNumbers.Count < numEvents)
                        {
                            List<int> subCanopyList = Enumerable.Range(1, SubCanopyCohorts.Count()).ToList();
                            while ((randomNumbers.Count < numEvents) && (subCanopyList.Count() > 0))
                            {
                                int rand = Statistics.DiscreteUniformRandom(0, subCanopyList.Count() - 1);
                                randomNumbers.Add(subCanopyList[rand]);
                                subCanopyList.RemoveAt(rand);
                            }
                        }
                    }
                }
                var groupList = randomNumbers.GroupBy(i => i);
                // Reset Hydrology values
                hydrology.Runoff = 0;
                hydrology.Leakage = 0;
                hydrology.Evaporation = 0;
                hydrology.PotentialEvaporation = groundPotentialET;
                hydrology.PotentialET = 0;
                float PotentialETcumulative = 0;
                float TransCumulative = 0;
                float InterceptCumulative = 0;
                float O3_ppmh = data[m].O3 / 1000; // convert AOT40 units to ppm h
                float lastO3 = 0;
                if (m > 0)
                    lastO3 = data[m - 1].O3 / 1000f;
                float O3_ppmh_month = Math.Max(0, O3_ppmh - lastO3);
                List<IPnETSpecies> species = SpeciesParameters.PnETSpecies.AllSpecies.ToList();
                Dictionary<string, float> DelAmax_spp = new Dictionary<string, float>();
                Dictionary<string, float> JCO2_spp = new Dictionary<string, float>();
                Dictionary<string, float> Amax_spp = new Dictionary<string, float>();
                Dictionary<string, float> PsnFTempRefNetPsn_spp = new Dictionary<string, float>();
                Dictionary<string, float> Ca_Ci_spp = new Dictionary<string, float>();
                float subCanopyPrecip = 0;
                float subCanopyPotentialET = 0;;
                float subCanopyMelt = 0;
                int subCanopyIndex = 0;
                int layerCount = 0;
                if (LayeredBins != null)
                    layerCount = LayeredBins.Count();
                float[] layerWtLAI = new float[layerCount];
                float[] layerSumBio = new float[layerCount];
                float[] layerSumCanopyFrac = new float[layerCount];
                if (LayeredBins != null && LayeredBins.Count() > 0)
                {
                    // main canopy layers
                    for (int b = LayeredBins.Count() - 1; b >= 0; b--)
                    {
                        // sublayers within main canopy b
                        foreach (int r in random_range[b])
                        {
                            Cohort c = SubCanopyCohorts.Values.ToArray()[r];
                            // A cohort cannot be reduced to a lower layer once it reaches a higher layer
                            c.Layer = (byte)b;
                        }
                    }
                    for (int b = LayeredBins.Count() - 1; b >= 0; b--)
                    {
                        // main canopy layers
                        float mainLayerPARweightedSum = 0;
                        float mainLayerLAIweightedSum = 0;
                        float mainLayerPAR = subcanopypar;
                        float mainLayerBioSum = 0;
                        float mainLayerCanopyFrac = 0;
                        // Estimate layer SumCanopyFrac
                        float sumCanopyFrac = 0;
                        foreach (int r in random_range[b])
                        {
                            // sublayers within main canopy b
                            Cohort c = SubCanopyCohorts.Values.ToArray()[r];
                            sumCanopyFrac += c.LastLAI / c.PnETSpecies.MaxLAI;
                        }
                        sumCanopyFrac /= Globals.IMAX;
                        foreach (int r in random_range[b])
                        {
                            // sublayers within main canopy b
                            subCanopyIndex++;
                            int precipCount = 0;
                            subCanopyPrecip = 0;
                            subCanopyPotentialET = 0;
                            subCanopyMelt = MeltInWater / SubCanopyCohorts.Count();
                            PotentialETcumulative = PotentialETcumulative + ReferenceET * data[m].DaySpan / SubCanopyCohorts.Count();
                            bool coldKillBoolean = false;
                            foreach (var g in groupList)
                            {
                                if (g.Key == subCanopyIndex)
                                {
                                    precipCount = g.Count();
                                    subCanopyPrecip = PrecInByEvent; 
                                    InterceptCumulative += interception / groupList.Count();
                                    if (snowpack == 0)
                                        subCanopyPotentialET = groundPotentialETbyEvent;
                                }
                            }
                            Cohort c = SubCanopyCohorts.Values.ToArray()[r];
                            IPnETSpecies spc = c.PnETSpecies;
                            if (coldKillMonth[spc] == m)
                                coldKillBoolean = true;
                            float FOzone = lastOzoneEffect[subCanopyIndex - 1];
                            float PotentialETnonfor = PotentialETcumulative - TransCumulative - InterceptCumulative - hydrology.Evaporation; // hydrology.Evaporation is cumulative
                            success = c.CalcPhotosynthesis(subCanopyPrecip, precipCount, leakageFrac, ref hydrology, mainLayerPAR,
                                ref subcanopypar, O3_ppmh, O3_ppmh_month, subCanopyIndex, SubCanopyCohorts.Count(), ref FOzone,
                                fracRootAboveFrost, subCanopyMelt, coldKillBoolean, data[m], this, sumCanopyFrac, subCanopyPotentialET, AllowMortality);
                            if (!success)
                                throw new System.Exception("Error CalcPhotosynthesis");
                            TransCumulative = TransCumulative + c.Transpiration[c.index-1];
                            lastOzoneEffect[subCanopyIndex - 1] = FOzone;
                            if (groundPotentialET > 0)
                            {
                                float evaporationEvent = 0;
                                // If more than one precip event assigned to layer, repeat evaporation for all events prior to respiration
                                for (int p = 1; p <= precipCount; p++)
                                {
                                    PotentialETnonfor = groundPotentialETbyEvent;
                                    if (fracRootAboveFrost > 0 && snowpack == 0)
                                        evaporationEvent = hydrology.CalcEvaporation(this, PotentialETnonfor); //mm
                                    success = hydrology.AddWater(-1 * evaporationEvent, Ecoregion.RootingDepth * fracRootAboveFrost);
                                    if (!success)
                                        throw new System.Exception("Error adding water, evaporation = " + evaporationEvent + "; soilWaterContent = " + hydrology.SoilWaterContent + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                                    hydrology.Evaporation += evaporationEvent;
                                }
                            }
                        } // end sublayer loop in canopy b
                        int cCount = AllCohorts.Count();
                        foreach (Cohort c in AllCohorts)
                        {
                            if (c.Layer == b)
                            {
                                float LAISum = c.LAI.Sum();
                                if (c.IsLeafOn)
                                {
                                    if (LAISum > c.LastLAI)
                                        c.LastLAI = LAISum;
                                }
                                float PARFracUnderCohort = (float)Math.Exp(-c.PnETSpecies.K * LAISum);
                                if (CohortStacking)
                                    mainLayerPARweightedSum += PARFracUnderCohort * 1.0f;
                                else
                                    mainLayerPARweightedSum += PARFracUnderCohort * Math.Min(c.LastLAI / c.PnETSpecies.MaxLAI, c.CanopyGrowingSpace);
                                mainLayerLAIweightedSum += LAISum * Math.Min(c.LastLAI / c.PnETSpecies.MaxLAI, c.CanopyGrowingSpace);
                                mainLayerBioSum += c.AGBiomass;
                                c.ANPP = (int)(c.AGBiomass - c.LastAGBio);
                                if(CohortStacking)
                                    mainLayerCanopyFrac += 1.0f;
                                else
                                    mainLayerCanopyFrac += Math.Min(c.LastLAI / c.PnETSpecies.MaxLAI, c.CanopyGrowingSpace);
                            }
                        }
                        layerSumBio[b] = mainLayerBioSum;
                        layerSumCanopyFrac[b] = mainLayerCanopyFrac;
                        if (layerSumCanopyFrac[b] > 1)
                            mainLayerPARweightedSum = 0;
                        List<float> Frac_list = new List<float>();
                        List<float> prop_List = new List<float>();
                        List<int> index_List = new List<int>();
                        int index = 0;
                        foreach (Cohort c in AllCohorts)
                        {
                            if (c.Layer == b)
                            {
                                index++;
                                index_List.Add(index);
                                c.BiomassLayerFrac = c.AGBiomass / layerSumBio[b];
                                c.CanopyLayerFrac = Math.Min(c.LastLAI / c.PnETSpecies.MaxLAI, c.CanopyGrowingSpace);
                                if (layerSumCanopyFrac[b] > 1)
                                {
                                    if (c.growMonth == 1)
                                    {
                                        float canopyLayerFracAdj = c.CanopyLayerFrac / layerSumCanopyFrac[b];
                                        c.CanopyLayerFrac = (canopyLayerFracAdj - c.CanopyLayerFrac) * CanopySumScale + c.CanopyLayerFrac;
                                        c.CanopyGrowingSpace = Math.Min(c.CanopyGrowingSpace, c.CanopyLayerFrac);
                                    }
                                    float LAISum = c.LAI.Sum();
                                    float PARFracUnderCohort = (float)Math.Exp(-c.PnETSpecies.K * LAISum);
                                    Frac_list.Add(PARFracUnderCohort);
                                    if (CohortStacking)
                                        mainLayerPARweightedSum += PARFracUnderCohort * 1.0f;
                                    else
                                        mainLayerPARweightedSum += PARFracUnderCohort * c.CanopyLayerFrac;
                                }
                                if (CohortStacking)
                                {
                                    c.CanopyLayerFrac = 1.0f;
                                    c.CanopyGrowingSpace = 1.0f;                                    
                                }
                                prop_List.Add(c.CanopyLayerFrac);
                                c.ANPP = (int)(c.ANPP * c.CanopyLayerFrac);
                            }
                        }
                        if (mainLayerBioSum > 0)
                        {
                            if (Frac_list.Count() > 0)
                            {                                
                                float cumulativeFracProp = 1;
                                for (int i = 0; i < Frac_list.Count(); i++)
                                {
                                    float prop = prop_List[i];
                                    float frac = Frac_list[i];
                                    cumulativeFracProp = cumulativeFracProp * (float)Math.Pow(frac, prop);
                                }
                                subcanopypar = mainLayerPAR * cumulativeFracProp;
                            }
                            else
                                subcanopypar = mainLayerPAR * (mainLayerPARweightedSum + (1 - mainLayerCanopyFrac));
                            layerWtLAI[b] = mainLayerLAIweightedSum;
                        }
                        else
                            subcanopypar = mainLayerPAR;
                    }
                    // end main canopy layer loop
                    hydrology.PotentialET += PotentialETcumulative;
                }
                else
                {
                    // When no cohorts are present
                    if (MeltInWater > 0)
                    {
                        // Add melted snow to soil moisture
                        // Instantaneous runoff (excess of soilPorosity)
                        float soilWaterCapacity = Ecoregion.Porosity * Ecoregion.RootingDepth * fracRootAboveFrost; //mm
                        float snowmeltRunoff = Math.Min(MeltInWater, Math.Max(hydrology.SoilWaterContent * Ecoregion.RootingDepth * fracRootAboveFrost + MeltInWater - soilWaterCapacity, 0));
                        hydrology.Runoff += snowmeltRunoff;
                        success = hydrology.AddWater(MeltInWater - snowmeltRunoff, Ecoregion.RootingDepth * fracRootAboveFrost);
                        if (!success)
                            throw new System.Exception("Error adding water, MeltInWaterr = " + MeltInWater + "; soilWaterContent = " + hydrology.SoilWaterContent + "; snowmeltRunoff = " + snowmeltRunoff + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                        float capturedRunoff = 0;
                        if ((Ecoregion.RunoffCapture > 0) & (snowmeltRunoff > 0))
                        {
                            capturedRunoff = Math.Max(0, Math.Min(snowmeltRunoff, Ecoregion.RunoffCapture - hydrology.SurfaceWater));
                            hydrology.SurfaceWater += capturedRunoff;
                        }
                        hydrology.Runoff += snowmeltRunoff - capturedRunoff;
                    }
                    if (precin > 0)
                    {
                        for (int p = 0; p < numEvents; p++)
                        {
                            // Instantaneous runoff (excess of soilPorosity)
                            float soilWaterCapacity = Ecoregion.Porosity * Ecoregion.RootingDepth * fracRootAboveFrost; //mm
                            float rainRunoff = Math.Min(precin, Math.Max(hydrology.SoilWaterContent * Ecoregion.RootingDepth * fracRootAboveFrost + PrecInByEvent - soilWaterCapacity, 0));
                            float capturedRunoff = 0;
                            if ((Ecoregion.RunoffCapture > 0) & (rainRunoff > 0))
                            {
                                capturedRunoff = Math.Max(0, Math.Min(rainRunoff, Ecoregion.RunoffCapture - hydrology.SurfaceWater));
                                hydrology.SurfaceWater += capturedRunoff;
                            }
                            hydrology.Runoff += rainRunoff - capturedRunoff;
                            float precipIn = PrecInByEvent - rainRunoff; //mm
                            // Add incoming precipitation to soil moisture
                            success = hydrology.AddWater(precipIn, Ecoregion.RootingDepth * fracRootAboveFrost);
                            if (!success)
                                throw new System.Exception("Error adding water, waterIn = " + precipIn + "; soilWaterContent = " + hydrology.SoilWaterContent + "; rainRunoff = " + rainRunoff + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                            // Fast Leakage
                            float leakage = Math.Max((float)leakageFrac * (hydrology.SoilWaterContent - Ecoregion.FieldCapacity), 0) * Ecoregion.RootingDepth * fracRootAboveFrost; //mm
                            hydrology.Leakage += leakage;
                            // Remove fast leakage
                            success = hydrology.AddWater(-1 * leakage, Ecoregion.RootingDepth * fracRootAboveFrost);
                            if (!success)
                                throw new System.Exception("Error adding water, Hydrology.Leakage = " + hydrology.Leakage + "; soilWaterContent = " + hydrology.SoilWaterContent + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                            // Evaporation
                            float PotentialETnonfor = groundPotentialET / numEvents;
                            PotentialETcumulative += ReferenceET * data[m].DaySpan / numEvents;
                            float evaporationEvent = 0;
                            if (fracRootAboveFrost > 0 && snowpack == 0)
                                evaporationEvent = hydrology.CalcEvaporation(this, PotentialETnonfor); //mm
                            success = hydrology.AddWater(-1 * evaporationEvent, Ecoregion.RootingDepth * fracRootAboveFrost);
                            if (!success)
                                throw new System.Exception("Error adding water, evaporation = " + evaporationEvent + "; soilWaterContent = " + hydrology.SoilWaterContent + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                            hydrology.Evaporation += evaporationEvent;
                            // Add surface water to soil
                            if (hydrology.SurfaceWater > 0)
                            {
                                float surfaceInput = Math.Min(hydrology.SurfaceWater, (Ecoregion.Porosity - hydrology.SoilWaterContent) * Ecoregion.RootingDepth * fracRootAboveFrost);
                                hydrology.SurfaceWater -= surfaceInput;
                                success = hydrology.AddWater(surfaceInput, Ecoregion.RootingDepth * fracRootAboveFrost);
                                if (!success)
                                    throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; soilWaterContent = " + hydrology.SoilWaterContent + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                            }
                        }
                    }
                    else  // precin > 0
                    {
                        if (MeltInWater > 0)
                        {
                            // Add surface water to soil
                            if (hydrology.SurfaceWater > 0)
                            {
                                float surfaceInput = Math.Min(hydrology.SurfaceWater, (Ecoregion.Porosity - hydrology.SoilWaterContent) * Ecoregion.RootingDepth * fracRootAboveFrost);
                                hydrology.SurfaceWater -= surfaceInput;
                                success = hydrology.AddWater(surfaceInput, Ecoregion.RootingDepth * fracRootAboveFrost);
                                if (!success)
                                    throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; soilWaterContent = " + hydrology.SoilWaterContent + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                            }
                        }
                    }
                    hydrology.PotentialET += PotentialETcumulative;
                }
                SiteVars.AnnualPotentialEvaporation[Site] = hydrology.PotentialEvaporation;
                int cohortCount = AllCohorts.Count();
                CanopyLAI = new float[tempMaxCanopyLayers];
                float[] CanopyLAISum = new float[tempMaxCanopyLayers];
                float[] CanopyLAICount = new float[tempMaxCanopyLayers];
                float[] CanopyAlbedo = new float[tempMaxCanopyLayers];
                float[] LayerLAI = new float[tempMaxCanopyLayers];
                float[] CanopyFracSum = new float[tempMaxCanopyLayers];
                CumulativeLeafAreas leafAreas = new CumulativeLeafAreas();
                monthCount[data[m].Month - 1]++;
                monthlySnowpack[data[m].Month - 1] += snowpack;
                monthlyWater[data[m].Month - 1] += hydrology.SoilWaterContent;
                monthlyEvap[data[m].Month - 1] += hydrology.Evaporation;
                monthlyInterception[data[m].Month - 1] += InterceptCumulative;
                monthlyLeakage[data[m].Month - 1] += hydrology.Leakage;
                monthlyRunoff[data[m].Month - 1] += hydrology.Runoff;
                monthlyPotentialEvap[data[m].Month - 1] += hydrology.PotentialEvaporation;
                foreach (Cohort cohort in AllCohorts)
                {
                    foliarRespiration[data[m].Month - 1] += cohort.FoliarRespiration.Sum() * cohort.CanopyLayerFrac;
                    netpsn[data[m].Month - 1] += cohort.NetPsn.Sum() * cohort.CanopyLayerFrac;
                    grosspsn[data[m].Month - 1] += cohort.GrossPsn.Sum() * cohort.CanopyLayerFrac;
                    maintresp[data[m].Month - 1] += cohort.MaintenanceRespiration.Sum() * cohort.CanopyLayerFrac;
                    transpiration += cohort.Transpiration.Sum(); // Transpiration already scaled to CanopyLayerFrac
                    potentialTranspiration += cohort.PotentialTranspiration.Sum(); // Transpiration already scaled to CanopyLayerFrac
                    CalcCumulativeLeafArea(ref leafAreas, cohort);                    
                    int layer = cohort.Layer;
                    if (layer < CanopyLAISum.Length)
                    {
                        CanopyLAISum[layer] += cohort.LAI.Sum() * ((1 - cohort.PnETSpecies.BGBiomassFrac) * cohort.TotalBiomass + cohort.Fol);
                        CanopyLAICount[layer] += (1 - cohort.PnETSpecies.BGBiomassFrac) * cohort.TotalBiomass+cohort.Fol;
                    }
                    else
                        Globals.ModelCore.UI.WriteLine("DEBUG: Cohort count = " + AllCohorts.Count() + "; CanopyLAISum count = " + CanopyLAISum.Count());
                    CanopyAlbedo[layer] += CalcAlbedoWithSnow(cohort, cohort.Albedo, snowDepth) * cohort.CanopyLayerFrac;
                    LayerLAI[layer] += cohort.SumLAI * cohort.CanopyLayerFrac;
                    monthlyLAI[data[m].Month - 1] += cohort.LAI.Sum() * cohort.CanopyLayerFrac;
                    monthlyLAICumulative[data[m].Month - 1] += cohort.LAI.Sum() * cohort.CanopyLayerFrac;
                    CanopyFracSum[layer] += cohort.CanopyLayerFrac;
                }
                monthlyActualTrans[data[m].Month - 1] += transpiration;
                monthlyPotentialTrans[data[m].Month - 1] += potentialTranspiration;
                monthlyActualET[data[m].Month - 1] = monthlyActualTrans[data[m].Month - 1] + monthlyEvap[data[m].Month - 1] + monthlyInterception[data[m].Month - 1];
                float groundAlbedo = 0.20F;
                if (snowDepth > 0)
                {
                    float snowMultiplier = snowDepth >= Constants.snowReflectanceThreshold ? 1 : snowDepth / Constants.snowReflectanceThreshold;
                    groundAlbedo = (float)(groundAlbedo + (groundAlbedo * (2.125 * snowMultiplier)));
                }
                for (int layer = 0; layer < tempMaxCanopyLayers; layer++)
                {
                    if (CanopyFracSum[layer] < 1.0)
                    {
                        float fracGround = 1.0f - CanopyFracSum[layer];
                        CanopyAlbedo[layer] += fracGround * groundAlbedo;
                    }
                    else if (CanopyFracSum[layer] > 1.0)
                        CanopyAlbedo[layer] = CanopyAlbedo[layer] / CanopyFracSum[layer];
                }
                if (AllCohorts.Count == 0)
                    averageAlbedo[data[m].Month - 1] = groundAlbedo;
                else
                {
                    if(LayerLAI.Max() == 0)
                    {
                        var index = Array.FindLastIndex(CanopyAlbedo, value => value != groundAlbedo);
                        // If a value not equal to zero was found
                        if (index != -1)
                            averageAlbedo[data[m].Month - 1] = CanopyAlbedo[index];
                        else
                            averageAlbedo[data[m].Month - 1] = groundAlbedo;
                    }
                    else if (LayerLAI.Max() < 1)
                    {
                        var index = Array.FindLastIndex(LayerLAI, value => value != 0);
                        // If a value not equal to zero was found
                        if (index != -1)
                            averageAlbedo[data[m].Month - 1] = CanopyAlbedo[index];
                        else
                            averageAlbedo[data[m].Month - 1] = groundAlbedo;
                    }
                    else
                    {
                        for (int layer = tempMaxCanopyLayers - 1; layer >= 0; layer--)
                        {
                            if (LayerLAI[layer] > 1)
                            {
                                averageAlbedo[data[m].Month - 1] = CanopyAlbedo[layer];
                                break;
                            }
                        }
                    }
                }
                for (int layer = 0; layer < tempMaxCanopyLayers; layer++)
                {
                    if (layer < layerWtLAI.Length)
                        CanopyLAI[layer] = layerWtLAI[layer];
                    else
                        CanopyLAI[layer] = 0;
                }
                canopylaimax = (float)Math.Max(canopylaimax, LayerLAI.Sum());
                if (data[m].Tavg > 0)
                {
                    float monthlyPressureHead = hydrology.GetPressureHead(Ecoregion);
                    sumPressureHead += monthlyPressureHead;
                    countPressureHead += 1;
                    SiteVars.MonthlyPressureHead[this.Site][m] = monthlyPressureHead;
                    SiteVars.MonthlySoilTemp[this.Site][m] = monthlyDepthTempDict;
                }
                else
                {
                    SiteVars.MonthlyPressureHead[this.Site][m] = -9999;
                    SiteVars.MonthlySoilTemp[this.Site][m] = null;
                }
                // Calculate establishment probability
                if (Globals.ModelCore.CurrentTime > 0)
                {
                    probEstablishment.CalcProbEstablishmentForMonth(data[m], Ecoregion, subcanopypar, hydrology, minHalfSat, maxHalfSat, invertProbEstablishment, fracRootAboveFrost);
                    foreach (IPnETSpecies spc in SpeciesParameters.PnETSpecies.AllSpecies)
                    {
                        if (annualFWater.ContainsKey(spc))
                        {
                            if (data[m].Tmin > spc.PsnTmin && data[m].Tmax < spc.PsnTmax && fracRootAboveFrost > 0) // Active growing season
                            {
                                // Store monthly values for later averaging
                                annualFWater[spc].Add(probEstablishment.GetSpeciesFWater(spc));
                                annualFRad[spc].Add(probEstablishment.GetSpeciesFRad(spc));
                            }
                        }
                    }
                }
                float ActualET = hydrology.Evaporation + TransCumulative + InterceptCumulative;
                this.SetActualET(ActualET, data[m].Month);
                this.SetPotentialET(PotentialETcumulative);
                SiteVars.ClimaticWaterDeficit[this.Site] += PotentialETcumulative - ActualET;
                // Add surface water to soil
                if ((hydrology.SurfaceWater > 0) & (hydrology.SoilWaterContent < Ecoregion.Porosity))
                {
                    float surfaceInput = Math.Min(hydrology.SurfaceWater, (Ecoregion.Porosity - hydrology.SoilWaterContent) * Ecoregion.RootingDepth * fracRootAboveFrost);
                    hydrology.SurfaceWater -= surfaceInput;
                    success = hydrology.AddWater(surfaceInput, Ecoregion.RootingDepth * fracRootAboveFrost);
                    if (!success)
                        throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; soilWaterContent = " + hydrology.SoilWaterContent + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                }
                if (siteoutput != null && outputCohortData)
                {
                    AddSiteOutput(data[m]);
                    AllCohorts.ForEach(a => a.UpdateCohortData(data[m]));
                }
                if (data[m].Tavg > 0)
                {
                    sumPressureHead += hydrology.PressureHeadTable.CalcSoilWaterPressureHead(hydrology.SoilWaterContent,Ecoregion.SoilType);
                    countPressureHead += 1;
                }
                if (data[m].Month == 7)
                    julysubcanopypar = subcanopypar;
                // Store growing season FRad values                
                AllCohorts.ForEach(x => x.StoreFRad());
                // Reset all cohort values
                AllCohorts.ForEach(x => x.NullSubLayers());
                //  Processes that happen only once per year
                if (data[m].Month == (int)Calendar.Months.December)
                {
                    //  Decompose litter
                    HeterotrophicRespiration = (ushort)(SiteVars.Litter[Site].Decompose() + SiteVars.WoodDebris[Site].Decompose());
                    // Calculate AdjFolFrac
                    AllCohorts.ForEach(x => x.CalcAdjFolBiomassFrac());
                    // Filter monthly pest values
                    // This assumes up to 3 months of growing season are relevant for establishment
                    // When > 3 months of growing season, exlcude 1st month, assuming trees focus on foliage growth in first month
                    // When > 4 months, ignore the 4th month and beyond as not primarily relevant for establishment
                    // When < 3 months, include all months
                    foreach (IPnETSpecies spc in SpeciesParameters.PnETSpecies.AllSpecies)
                    {
                        if (annualFWater[spc].Count > 3)
                        {
                            cumulativeFWater[spc] = cumulativeFWater[spc] + annualFWater[spc][1] + annualFWater[spc][2] + annualFWater[spc][3];
                            cumulativeFRad[spc] = cumulativeFRad[spc] + annualFRad[spc][1] + annualFRad[spc][2] + annualFRad[spc][3];
                            monthlyCount[spc] = monthlyCount[spc] + 3;
                        }
                        else if (annualFWater[spc].Count > 2)
                        {
                            cumulativeFWater[spc] = cumulativeFWater[spc] + annualFWater[spc][0] + annualFWater[spc][1] + annualFWater[spc][2];
                            cumulativeFRad[spc] = cumulativeFRad[spc] + annualFRad[spc][0] + annualFRad[spc][1] + annualFRad[spc][2];
                            monthlyCount[spc] = monthlyCount[spc] + 3;
                        }
                        else if (annualFWater[spc].Count > 1)
                        {
                            cumulativeFWater[spc] = cumulativeFWater[spc] + annualFWater[spc][0] + annualFWater[spc][1];
                            cumulativeFRad[spc] = cumulativeFRad[spc] + annualFRad[spc][0] + annualFRad[spc][1];
                            monthlyCount[spc] = monthlyCount[spc] + 2;
                        }
                        else if (annualFWater[spc].Count == 1)
                        {
                            cumulativeFWater[spc] = cumulativeFWater[spc] + annualFWater[spc][0];
                            cumulativeFRad[spc] = cumulativeFRad[spc] + annualFRad[spc][0];
                            monthlyCount[spc] = monthlyCount[spc] + 1;
                        }
                        //Reset annual lists for next year
                        annualFWater[spc].Clear();
                        annualFRad[spc].Clear();
                    }
                }
                avgSoilWaterContent += hydrology.SoilWaterContent;
            }
            // Above is monthly loop                           
            // Below runs once per timestep
            avgSoilWaterContent /= data.Count(); // convert to average value
            if (Globals.ModelCore.CurrentTime > 0)
            {
                foreach (IPnETSpecies spc in SpeciesParameters.PnETSpecies.AllSpecies)
                {
                    bool estab = false;
                    float pest = 0;
                    if (monthlyCount[spc] > 0)
                    {
                        // Transform cumulative probability of no successful establishments to probability of at least one successful establishment
                        cumulativeFWater[spc] = cumulativeFWater[spc] / monthlyCount[spc];
                        cumulativeFRad[spc] = cumulativeFRad[spc] / monthlyCount[spc];
                        // Calculate Pest from average FWater, FRad and modified by MaxPest
                        pest = cumulativeFWater[spc] * cumulativeFRad[spc] * spc.MaxPest;
                    }                    
                    if (!spc.PreventEstablishment)
                    {
                        if (pest > (float)Statistics.ContinuousUniformRandom())
                        {
                            probEstablishment.AddEstablishedSpecies(spc);
                            estab = true;
                        }
                    }
                    ProbEstablishment.RecordProbEstablishment(Globals.ModelCore.CurrentTime, spc, pest, cumulativeFWater[spc], cumulativeFRad[spc], estab, monthlyCount[spc]);
                }
            }
            if (siteoutput != null && outputCohortData)
            {
                siteoutput.Write();
                AllCohorts.ForEach(cohort => { cohort.WriteCohortData(); });
            }
            float avgPH = sumPressureHead / countPressureHead;
            SiteVars.PressureHead[Site] = avgPH;
            if((Globals.ModelCore.CurrentTime > 0) || AllowMortality)
                RemoveMarkedCohorts();

            return success;
        }

        // Finds the maximum value from an array of floats
        private float Max(float[] values)
        {
            float maximum = float.MinValue;
            for(int i = 0; i < values.Length; i++)
            {
                if (values[i] > maximum)
                    maximum = values[i];
            }

            return maximum;
        }

        private bool isSummer(byte month)
        {
            switch (month)
            {
                case 5:
                    return true;
                case 6:
                    return true;
                case 7:
                    return true;
                case 8:
                    return true;
                default:
                    return false;
            }
        }

        private bool isWinter(byte month)
        {
            switch (month)
            {
                case 1:
                    return true;
                case 2:
                    return true;
                case 3:
                    return true;
                case 12:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Albedo calculation by adding snow consideration
        /// </summary>
        /// <param name="cohort"></param>
        /// <param name="albedo"></param>
        /// <param name="snowDepth"></param>
        /// <returns></returns>
        private float CalcAlbedoWithSnow(Cohort cohort, float albedo, float snowDepth)
        {
            // Inactive sites become large negative values on the map and are not considered in the averages
            if (!EcoregionData.GetPnETEcoregion(Globals.ModelCore.Ecoregion[this.Site]).Active)
                return -1;
            float finalAlbedo = 0;
            float snowMultiplier = snowDepth >= Constants.snowReflectanceThreshold ? 1 : snowDepth / Constants.snowReflectanceThreshold;
            if ((!string.IsNullOrEmpty(cohort.PnETSpecies.Lifeform))
                    && (cohort.PnETSpecies.Lifeform.ToLower().Contains("ground")
                        || cohort.PnETSpecies.Lifeform.ToLower().Contains("open")
                        || cohort.SumLAI == 0))
                finalAlbedo = (float)(albedo + (albedo * (2.75 * snowMultiplier)));
            else if ((!string.IsNullOrEmpty(cohort.PnETSpecies.Lifeform))
                    && cohort.PnETSpecies.Lifeform.ToLower().Contains("dark"))
                finalAlbedo = (float)(albedo + (albedo * (0.8 * snowMultiplier)));
            else if ((!string.IsNullOrEmpty(cohort.PnETSpecies.Lifeform))
                    && cohort.PnETSpecies.Lifeform.ToLower().Contains("light"))
                finalAlbedo = (float)(albedo + (albedo * (0.75 * snowMultiplier)));
            else if ((!string.IsNullOrEmpty(cohort.PnETSpecies.Lifeform))
                    && cohort.PnETSpecies.Lifeform.ToLower().Contains("decid"))
                finalAlbedo = (float)(albedo + (albedo * (0.35 * snowMultiplier)));

            return finalAlbedo;
        }

        private void CalcCumulativeLeafArea(ref CumulativeLeafAreas leafAreas, Cohort cohort)
        {
            if ((!string.IsNullOrEmpty(cohort.PnETSpecies.Lifeform))
                    && cohort.PnETSpecies.Lifeform.ToLower().Contains("dark"))
                leafAreas.DarkConifer += cohort.SumLAI;
            else if ((!string.IsNullOrEmpty(cohort.PnETSpecies.Lifeform))
                    && cohort.PnETSpecies.Lifeform.ToLower().Contains("light"))
                leafAreas.LightConifer += cohort.SumLAI;
            else if ((!string.IsNullOrEmpty(cohort.PnETSpecies.Lifeform))
                    && cohort.PnETSpecies.Lifeform.ToLower().Contains("decid"))
                leafAreas.Deciduous += cohort.SumLAI;
            else if ((!string.IsNullOrEmpty(cohort.PnETSpecies.Lifeform))
                    && (cohort.PnETSpecies.Lifeform.ToLower().Contains("ground")
                        || cohort.PnETSpecies.Lifeform.ToLower().Contains("open")))
                leafAreas.GrassMossOpen += cohort.SumLAI;
            else if ((!string.IsNullOrEmpty(cohort.PnETSpecies.Lifeform))
                    && (cohort.PnETSpecies.Lifeform.ToLower().Contains("tree")
                        || cohort.PnETSpecies.Lifeform.ToLower().Contains("shrub")))
                leafAreas.Deciduous += cohort.SumLAI;
        }
        
        public float[] MaintResp
        {
            get
            {
                if (maintresp == null)
                {
                    float[] maintresp_array = new float[12];
                    for (int i = 0; i < maintresp_array.Length; i++)
                    {
                        maintresp_array[i] = 0;
                    }
                    return maintresp_array;
                }
                else
                    return maintresp.Select(r => (float)r).ToArray();
            }
        }

        public float[] FoliarRespiration
        {
            get
            {
                if (foliarRespiration == null)
                {
                    float[] foliarRespiration_array = new float[12];
                    for (int i = 0; i < foliarRespiration_array.Length; i++)
                    {
                        foliarRespiration_array[i] = 0;
                    }
                    return foliarRespiration_array;
                }
                else
                    return foliarRespiration.Select(psn => (float)psn).ToArray();
            }
        }

        public float[] GrossPsn
        {
            get
            {
                if (grosspsn == null)
                {
                    float[] grosspsn_array = new float[12];
                    for (int i = 0; i < grosspsn_array.Length; i++)
                    {
                        grosspsn_array[i] = 0;
                    }
                    return grosspsn_array;
                }
                else
                    return grosspsn.Select(psn => (float)psn).ToArray();
            }
        }

        public float[] MonthlyAvgSnowpack
        {
            get
            {
                if (monthlySnowpack == null)
                {
                    float[] snowpack_array = new float[12];
                    for (int i = 0; i < snowpack_array.Length; i++)
                    {
                        snowpack_array[i] = 0;
                    }
                    return snowpack_array;
                }
                else
                {
                    float[] snowSum = monthlySnowpack.Select(snowpack => (float)snowpack).ToArray();
                    float[] monthSum = monthCount.Select(months => (float)months).ToArray();
                    float[] snowpack_array = new float[12];
                    for (int i = 0; i < snowpack_array.Length; i++)
                    {
                        snowpack_array[i] = snowSum[i] / monthSum[i];
                    }
                    return snowpack_array;
                }
            }
        }

        public float[] MonthlyAvgWater
        {
            get
            {
                if (monthlyWater == null)
                {
                    float[] soilWaterContent_array = new float[12];
                    for (int i = 0; i < soilWaterContent_array.Length; i++)
                    {
                        soilWaterContent_array[i] = 0;
                    }
                    return soilWaterContent_array;
                }
                else
                {
                    float[] soilWaterContentSum = monthlyWater.Select(water => (float)water).ToArray();
                    float[] monthSum = monthCount.Select(months => (float)months).ToArray();
                    float[] soilWaterContent_array = new float[12];
                    for (int i = 0; i < soilWaterContent_array.Length; i++)
                    {
                        soilWaterContent_array[i] = soilWaterContentSum[i] / monthSum[i];
                    }
                    return soilWaterContent_array;
                }
            }
        }
    
        public float[] MonthlyAvgLAI
        {
            get
            {
                if (monthlyLAICumulative == null)
                {
                    float[] lai_array = new float[12];
                    for (int i = 0; i < lai_array.Length; i++)
                    {
                        lai_array[i] = 0;
                    }
                    return lai_array;
                }
                else
                {
                    float[] laiSum = monthlyLAICumulative.Select(lai => (float)lai).ToArray();
                    float[] monthSum = monthCount.Select(months => (float)months).ToArray();
                    float[] lai_array = new float[12];
                    for (int i = 0; i < lai_array.Length; i++)
                    {
                        lai_array[i] = laiSum[i] / monthSum[i];
                    }
                    return lai_array;
                }
            }
        }
    
        public float[] MonthlyEvap
        {
            get
            {
                if (monthlyEvap == null)
                {
                    float[] evap_array = new float[12];
                    for (int i = 0; i < evap_array.Length; i++)
                    {
                        evap_array[i] = 0;
                    }
                    return evap_array;
                }
                else
                {
                    float[] evapSum = monthlyEvap.Select(evap => (float)evap).ToArray();
                    float[] monthSum = monthCount.Select(months => (float)months).ToArray();
                    float[] evap_array = new float[12];
                    for (int i = 0; i < evap_array.Length; i++)
                    {
                        evap_array[i] = evapSum[i] / monthSum[i];
                    }
                    return evap_array;
                }
            }
        }
    
        public float[] MonthlyInterception
        {
            get
            {
                if (monthlyInterception == null)
                {
                    float[] interception_array = new float[12];
                    for (int i = 0; i < interception_array.Length; i++)
                    {
                        interception_array[i] = 0;
                    }
                    return interception_array;
                }
                else
                {
                    float[] interceptionSum = monthlyInterception.Select(interception => (float)interception).ToArray();
                    float[] monthSum = monthCount.Select(months => (float)months).ToArray();
                    float[] interception_array = new float[12];
                    for (int i = 0; i < interception_array.Length; i++)
                    {
                        interception_array[i] = interceptionSum[i] / monthSum[i];
                    }
                    return interception_array;
                }
            }
        }
        public float[] MonthlyActualTrans
        {
            get
            {
                if (monthlyActualTrans == null)
                {
                    float[] actualTrans_array = new float[12];
                    for (int i = 0; i < actualTrans_array.Length; i++)
                    {
                        actualTrans_array[i] = 0;
                    }
                    return actualTrans_array;
                }
                else
                {
                    float[] actualTransSum = monthlyActualTrans.Select(actualTrans => (float)actualTrans).ToArray();
                    float[] monthSum = monthCount.Select(months => (float)months).ToArray();
                    float[] actualTrans_array = new float[12];
                    for (int i = 0; i < actualTrans_array.Length; i++)
                    {
                        actualTrans_array[i] = actualTransSum[i] / monthSum[i];
                    }
                    return actualTrans_array;
                }
            }
        }
    
        public float[] MonthlyLeakage
        {
            get
            {
                if (monthlyLeakage == null)
                {
                    float[] leakage_array = new float[12];
                    for (int i = 0; i < leakage_array.Length; i++)
                    {
                        leakage_array[i] = 0;
                    }
                    return leakage_array;
                }
                else
                {
                    float[] leakageSum = monthlyLeakage.Select(leakage => (float)leakage).ToArray();
                    float[] monthSum = monthCount.Select(months => (float)months).ToArray();
                    float[] leakage_array = new float[12];
                    for (int i = 0; i < leakage_array.Length; i++)
                    {
                        leakage_array[i] = leakageSum[i] / monthSum[i];
                    }
                    return leakage_array;
                }
            }
        }
    
        public float[] MonthlyRunoff
        {
            get
            {
                if (monthlyRunoff == null)
                {
                    float[] runoff_array = new float[12];
                    for (int i = 0; i < runoff_array.Length; i++)
                    {
                        runoff_array[i] = 0;
                    }
                    return runoff_array;
                }
                else
                {
                    float[] runoffSum = monthlyRunoff.Select(runoff => (float)runoff).ToArray();
                    float[] monthSum = monthCount.Select(months => (float)months).ToArray();
                    float[] runoff_array = new float[12];
                    for (int i = 0; i < runoff_array.Length; i++)
                    {
                        runoff_array[i] = runoffSum[i] / monthSum[i];
                    }
                    return runoff_array;
                }
            }
        }
    
        public float[] MonthlyActualET
        {
            get
            {
                if (monthlyActualET == null)
                {
                    float[] actualET_array = new float[12];
                    for (int i = 0; i < actualET_array.Length; i++)
                    {
                        actualET_array[i] = 0;
                    }
                    return actualET_array;
                }
                else
                {
                    float[] actualETSum = monthlyActualET.Select(actualET => (float)actualET).ToArray();
                    float[] monthSum = monthCount.Select(months => (float)months).ToArray();
                    float[] actualET_array = new float[12];
                    for (int i = 0; i < actualET_array.Length; i++)
                    {
                        actualET_array[i] = actualETSum[i] / monthSum[i];
                    }
                    return actualET_array;
                }
            }
        }
    
        public float[] MonthlyPotentialEvap
        {
            get
            {
                if (monthlyPotentialEvap == null)
                {
                    float[] potentialEvap_array = new float[12];
                    for (int i = 0; i < potentialEvap_array.Length; i++)
                    {
                        potentialEvap_array[i] = 0;
                    }
                    return potentialEvap_array;
                }
                else
                {
                    float[] potentialEvap_Sum = monthlyPotentialEvap.Select(potentialEvap => (float)potentialEvap).ToArray();
                    float[] monthSum = monthCount.Select(months => (float)months).ToArray();
                    float[] potentialEvap_array = new float[12];
                    for (int i = 0; i < potentialEvap_array.Length; i++)
                    {
                        potentialEvap_array[i] = potentialEvap_Sum[i] / monthSum[i];
                    }
                    return potentialEvap_array;
                }
            }
        }

        public float[] MonthlyPotentialTrans
        {
            get
            {
                if (monthlyPotentialTrans == null)
                {
                    float[] potentialTrans_array = new float[12];
                    for (int i = 0; i < potentialTrans_array.Length; i++)
                    {
                        potentialTrans_array[i] = 0;
                    }
                    return potentialTrans_array;
                }
                else
                {
                    float[] potentialTransSum = monthlyPotentialTrans.Select(potentialTrans => (float)potentialTrans).ToArray();
                    float[] monthSum = monthCount.Select(months => (float)months).ToArray();
                    float[] potentialTrans_array = new float[12];
                    for (int i = 0; i < potentialTrans_array.Length; i++)
                    {
                        potentialTrans_array[i] = potentialTransSum[i] / monthSum[i];
                    }
                    return potentialTrans_array;
                }
            }
        }

        public float[] AverageAlbedo
        {
            get
            {
                if (averageAlbedo == null)
                {
                    float[] averageAlbedo_array = new float[12];
                    for (int i = 0; i < averageAlbedo_array.Length; i++)
                    {
                        averageAlbedo_array[i] = 0.20f;
                    }
                    return averageAlbedo_array;
                }
                else
                    return averageAlbedo.Select(r => (float)r).ToArray();
            }
        }

        public float[] ActiveLayerDepth
        {
            get
            {
                if (activeLayerDepth == null)
                {
                    float[] activeLayerDepth_array = new float[12];
                    for (int i = 0; i < activeLayerDepth_array.Length; i++)
                    {
                        activeLayerDepth_array[i] = 0;
                    }
                    return activeLayerDepth_array;
                }
                else
                    return activeLayerDepth.Select(r => (float)r).ToArray();
            }
        }

        public float[] FrostDepth
        {
            get
            {
                if (frostDepth == null)
                {
                    float[] frostDepth_array = new float[12];
                    for (int i = 0; i < frostDepth_array.Length; i++)
                    {
                        frostDepth_array[i] = 0;
                    }
                    return frostDepth_array;
                }
                else
                    return frostDepth.Select(r => (float)r).ToArray();
            }
        }

        public float NetPsnSum
        {
            get
            {
                if (netpsn == null)
                {
                    float[] netpsn_array = new float[12];
                    for (int i = 0; i < netpsn_array.Length; i++)
                    {
                        netpsn_array[i] = 0;
                    }
                    return netpsn_array.Sum();
                }
                else
                    return netpsn.Select(psn => (float)psn).ToArray().Sum();
            }
        }

        public float CanopyLAImax
        {
            get
            {
                return canopylaimax;
            }
        }

        public float[] MonthlyLAI
        {
            get
            {
                return monthlyLAI;
            }
        }

        public float SiteMossDepth
        {
            get
            {
                float mossDepth = Ecoregion.MossDepth; //m
                foreach (ISpecies spc in cohorts.Keys)
                {
                    // Add each species' mossDepth to the total
                    foreach (Cohort cohort in cohorts[spc])
                    {
                        mossDepth += cohort.MossDepth * cohort.CanopyLayerFrac;
                    }
                }
                return mossDepth;
            }
        }

        public double WoodDebris 
        {
            get
            {
                return SiteVars.WoodDebris[Site].Mass;
            }
        }

        public double Litter 
        {
            get
            {
                return SiteVars.Litter[Site].Mass;
            }
        }
       
        public  Landis.Library.Parameters.Species.AuxParm<bool> SpeciesPresent
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<bool> SpeciesPresent = new Library.Parameters.Species.AuxParm<bool>(Globals.ModelCore.Species);
                foreach (ISpecies spc in cohorts.Keys)
                {
                    SpeciesPresent[spc] = true;
                }
                return SpeciesPresent;
            }
        }

        public Landis.Library.Parameters.Species.AuxParm<int> BiomassPerSpecies 
        { 
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> BiomassPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);
                foreach (ISpecies spc in cohorts.Keys)
                {
                    BiomassPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.TotalBiomass * o.CanopyLayerFrac));
                }
                return BiomassPerSpecies;
            }
        }

        public Landis.Library.Parameters.Species.AuxParm<int> AbovegroundBiomassPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> AbovegroundBiomassPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);
                foreach (ISpecies spc in cohorts.Keys)
                {
                    AbovegroundBiomassPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.AGBiomass * o.CanopyLayerFrac));
                }
                return AbovegroundBiomassPerSpecies;
            }
        }

        public Landis.Library.Parameters.Species.AuxParm<int> WoodBiomassPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> WoodBiomassPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);
                foreach (ISpecies spc in cohorts.Keys)
                {
                    WoodBiomassPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.Wood * o.CanopyLayerFrac));
                }
                return WoodBiomassPerSpecies;
            }
        }

        public Landis.Library.Parameters.Species.AuxParm<int> BelowGroundBiomassPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> BelowGroundBiomassPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);
                foreach (ISpecies spc in cohorts.Keys)
                {
                    BelowGroundBiomassPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.Root * o.CanopyLayerFrac));
                }
                return BelowGroundBiomassPerSpecies;
            }
        }

        public Landis.Library.Parameters.Species.AuxParm<int> FoliageBiomassPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> FoliageBiomassPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);
                foreach (ISpecies spc in cohorts.Keys)
                {
                    FoliageBiomassPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.Fol * o.CanopyLayerFrac));
                }
                return FoliageBiomassPerSpecies;
            }
        }

        public Landis.Library.Parameters.Species.AuxParm<int> MaxFoliageYearPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> MaxFoliageYearPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);
                foreach (ISpecies spc in cohorts.Keys)
                {
                    // Edited according to Brian Miranda's advice (https://github.com/LANDIS-II-Foundation/Extension-Output-Biomass-PnET/issues/11#issuecomment-2400646970_
                    // to correct how the variable is computed, to make it similar to FoliageSum.
                    MaxFoliageYearPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.MaxFolYear * o.CanopyLayerFrac));
                }
                return MaxFoliageYearPerSpecies;
            }
        }

        public Landis.Library.Parameters.Species.AuxParm<int> NSCPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> NSCPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);
                foreach (ISpecies spc in cohorts.Keys)
                {
                    NSCPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.NSC * o.CanopyLayerFrac));
                }
                return NSCPerSpecies;
            }
        }

        public Landis.Library.Parameters.Species.AuxParm<float> LAIPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<float> LAIPerSpecies = new Library.Parameters.Species.AuxParm<float>(Globals.ModelCore.Species);
                foreach (ISpecies spc in cohorts.Keys)
                {
                    //LAIPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.LAI != null ? o.LAI.Sum() * o.CanopyLayerFrac : 0));
                    LAIPerSpecies[spc] = cohorts[spc].Sum(o => o.LastLAI * o.CanopyLayerFrac);
                }
                return LAIPerSpecies;
            }
        }

        public Landis.Library.Parameters.Species.AuxParm<int> WoodSenescencePerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> WoodSenescencePerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);
                foreach (ISpecies spc in cohorts.Keys)
                {
                    WoodSenescencePerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.LastWoodSenescence * o.CanopyLayerFrac));
                }
                return WoodSenescencePerSpecies;
            }
        }

        public Landis.Library.Parameters.Species.AuxParm<int> FoliageSenescencePerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> FoliageSenescencePerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);
                foreach (ISpecies spc in cohorts.Keys)
                {
                    FoliageSenescencePerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.LastFoliageSenescence * o.CanopyLayerFrac));
                }
                return FoliageSenescencePerSpecies;
            }
        }

        public Landis.Library.Parameters.Species.AuxParm<int> CohortCountPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> CohortCountPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);
                foreach (ISpecies spc in cohorts.Keys)
                {
                    CohortCountPerSpecies[spc] = cohorts[spc].Count();
                }
                return CohortCountPerSpecies;
            }
        }


        public Landis.Library.Parameters.Species.AuxParm<List<ushort>> CohortAges
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<List<ushort>> CohortAges = new Library.Parameters.Species.AuxParm<List<ushort>>(Globals.ModelCore.Species);
                foreach (ISpecies spc in cohorts.Keys)
                {
                    CohortAges[spc] = new List<ushort>(cohorts[spc].Select(o => o.Age));
                }
                return CohortAges;
            }
        }

        public float BiomassSum
        {
            get
            {
                return AllCohorts.Sum(o => o.TotalBiomass * o.CanopyLayerFrac);
            }
        }

        public float AbovegroundBiomassSum
        {
            get
            {
                return AllCohorts.Sum(o => o.AGBiomass * o.CanopyLayerFrac);
            }
        }

        public float WoodBiomassSum
        {
            get
            {
                return AllCohorts.Sum(o => o.Wood * o.CanopyLayerFrac);
            }
        }

        public float WoodSenescenceSum
        {
            get
            {
                return AllCohorts.Sum(o => o.LastWoodSenescence * o.CanopyLayerFrac);
            }
        }

        public float FoliageSenescenceSum
        {
            get
            {
                return AllCohorts.Sum(o => o.LastFoliageSenescence * o.CanopyLayerFrac);
            }
        }

        public float BelowGroundBiomassSum
        {
            get
            {
                return AllCohorts.Sum(o => o.Root * o.CanopyLayerFrac);
            }
        }


        public float FoliageSum
        {
            get
            {
                return AllCohorts.Sum(o => o.Fol * o.CanopyLayerFrac);
            }
        }

        public float NSCSum
        {
            get
            {
                return AllCohorts.Sum(o => o.NSC * o.CanopyLayerFrac);
            }
        }

        public float PotentialET
        {
            get;
            set;
        }

        public int CohortCount
        {
            get
            {
                return cohorts.Values.Sum(o => o.Count());
            }
        }
        
        public int AverageAge 
        {
            get
            {
                return (int) cohorts.Values.Average(o => o.Average(x=>x.Age));
            }
        }

        public float ActualETSum
        {
            get
            {
                return ActualET.Sum();
            }
        }

        class SubCanopyComparer : IComparer<int[]>
        {
            // Compare second int (cumulative cohort biomass)
            public int Compare(int[] x, int[] y)
            {
                return (x[0] > y[0]) ? 1 : -1;
            }
        }

        private SortedDictionary<int[], Cohort> GetSubcanopyLayers()
        {
            SortedDictionary<int[], Cohort> subcanopylayers = new SortedDictionary<int[], Cohort>(new SubCanopyComparer());
            foreach (Cohort cohort in AllCohorts)
            {
                for (int i = 0; i < Globals.IMAX; i++)
                {
                    int[] subcanopylayer = new int[] { (ushort)((i + 1) / (float)Globals.IMAX * cohort.MaxBiomass) };
                    subcanopylayers.Add(subcanopylayer, cohort);
                }
            }
            return subcanopylayers;
        }

        private static int[] GetNextBinPositions(int[] index_in, int numcohorts)
        {            
            for (int index = index_in.Length - 1; index >= 0; index--)
            {
                int maxvalue = numcohorts - index_in.Length + index - 1;
                if (index_in[index] < maxvalue)
                {
                    index_in[index]++;
                    for (int i = index+1; i < index_in.Length; i++)
                    {
                        index_in[i] = index_in[i - 1] + 1;
                    }
                    return index_in;
                }
            }
            return null;
        }
      
        private int[] GetFirstBinPositions(int nlayers, int ncohorts)
        {
            int[] Bin = new int[nlayers - 1];
            for (int ly = 0; ly < Bin.Length; ly++)
            {
                Bin[ly] = ly+1;
            }
            return Bin;
        }

        public static List<T> Shuffle<T>(List<T> array)
        {
            int n = array.Count;
            while (n > 1)
            {
                int k = Statistics.DiscreteUniformRandom(0, n);
                n--;
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
            return array;
        }

        uint CalcLayerMaxDev(List<double> f)
        {
            return (uint)Math.Max(Math.Abs(f.Max() - f.Average()), Math.Abs(f.Min() - f.Average()));
        }

        int[] MinMaxCohortNr(int[] Bin, int i, int Count)
        {
            int min = (i > 0) ? Bin[i - 1] : 0;
            int max = (i < Bin.Count()) ? Bin[i] : Count - 1;
            return new int[] { min, max };
        }

        private List<double> layerThreshRatio = new List<double>();

        private List<List<double>> GetBinsByCohort(List<double> CohortBiomassList)
        {
            if (CohortBiomassList.Count() == 0)
                return null;
            nlayers = 1;
            layerThreshRatio.Clear();
            float diffFrac = LayerThreshRatio;
            // sort by ascending biomass
            CohortBiomassList.Sort();
            // reverse to sort by descending biomass
            CohortBiomassList.Reverse();
            int tempMaxCanopyLayers = MaxCanopyLayers;
            if (CohortStacking)
            {
                tempMaxCanopyLayers = CohortBiomassList.Count();
                diffFrac = 1;
            }
            List<List<double>> CohortBins = new List<List<double>>();
            int topLayerIndex = 0;
            CohortBins.Add(new List<double>());
            CohortBins[0].Add(CohortBiomassList[0]);
            foreach (double cohortBio in CohortBiomassList)
            {
                double smallestThisLayer = CohortBins[0][0];
                double ratio = cohortBio / smallestThisLayer;
                if (ratio < diffFrac)
                {
                    if (topLayerIndex < (tempMaxCanopyLayers - 1))
                    {
                        topLayerIndex++;
                        nlayers++;
                        CohortBins.Add(new List<double>());
                        foreach (int i in Enumerable.Range(1, topLayerIndex).Reverse())
                        {
                            CohortBins[i] = new List<double>(CohortBins[i - 1]);
                        }
                        CohortBins[0].Clear();
                    }
                }
                // Add a negligable value [-1e-10; + 1e-10] to ratio in order to prevent duplicate keys
                double k = 1e-10 * 2.0 * (Statistics.ContinuousUniformRandom() - 0.5);
                layerThreshRatio.Add(ratio + k);
                if (!CohortBins[0].Contains(cohortBio))
                    CohortBins[0].Add(cohortBio);
            }
            bool tooManyLayers = false;
            if (CohortBins.Count() > tempMaxCanopyLayers)
                tooManyLayers = true;
            if (tooManyLayers)
            {
                List<double> sortedRatios = layerThreshRatio.ToList();
                sortedRatios.Sort();
                List<double> smallestRatios = new List<double>();
                for (int r = 0; r < (tempMaxCanopyLayers - 1); r++)
                {
                    smallestRatios.Add(sortedRatios[r]);
                }
                CohortBins.Clear();
                topLayerIndex = tempMaxCanopyLayers - 1;
                nlayers = 1;
                for (int r = 0; r < tempMaxCanopyLayers; r++)
                {
                    CohortBins.Add(new List<double>());
                }
                CohortBins[topLayerIndex].Add(CohortBiomassList[0]);
                int cohortInd = 0;
                foreach (double cohortRatio in layerThreshRatio)
                {
                    if (smallestRatios.Contains(cohortRatio))
                    {
                        topLayerIndex--;
                        nlayers++;
                    }
                    if (!CohortBins[topLayerIndex].Contains(CohortBiomassList[cohortInd]))
                        CohortBins[topLayerIndex].Add(CohortBiomassList[cohortInd]);
                    cohortInd++;
                }
            }
            return CohortBins;
        }

        public static uint CalcKey(uint a, ushort b)
        {
            uint value = (uint)((a << 16) | b);
            return value;
        }

        public List<Cohort> AllCohorts
        {
            get
            {
                List<Cohort> all = new List<Cohort>();
                foreach (ISpecies spc in cohorts.Keys)
                {
                    all.AddRange(cohorts[spc]);
                }
                return all;
            }
        }

        public void ClearAllCohorts()
        {
            cohorts.Clear();
        }

        public override int ReduceOrKillCohorts(Landis.Library.UniversalCohorts.IDisturbance disturbance)
        {
            List<int> reduction = new List<int>();
            List<Cohort> ToRemove = new List<Cohort>();
            foreach (List<Cohort> species_cohort in cohorts.Values)
            {
                Landis.Library.PnETCohorts.SpeciesCohorts species_cohorts = GetSpeciesCohort(cohorts[species_cohort[0].Species]);
                for (int c = 0; c < species_cohort.Count(); c++)
                {
                    Landis.Library.PnETCohorts.ICohort cohort = species_cohort[c];
                    // Disturbances return reduction in aboveground biomass
                    int _reduction = disturbance.ReduceOrKillMarkedCohort(cohort);
                    reduction.Add(_reduction);
                    if (reduction[reduction.Count() - 1] >= species_cohort[c].Biomass)  //Compare to aboveground biomass at site scale
                        ToRemove.Add(species_cohort[c]); // Edited by BRM - 090115
                    else
                    {
                        double reductionFrac = (double)reduction[reduction.Count() - 1] / (double)species_cohort[c].Biomass;  //Fraction of aboveground biomass at site scale
                        species_cohort[c].ReduceBiomass(this, reductionFrac, disturbance.Type);  // Reduction applies to all biomass
                    }
                }
            }
            foreach (Cohort cohort in ToRemove)
            {
                RemoveCohort(cohort, disturbance.Type);
            }
            return reduction.Sum();
        }

        public int AgeMax 
        {
            get
            {
                return (cohorts.Values.Count() > 0) ? cohorts.Values.Max(o => o.Max(x => x.Age)) : -1;
            }
        }

        Landis.Library.UniversalCohorts.ISpeciesCohorts IISiteCohorts<Landis.Library.UniversalCohorts.ISpeciesCohorts>.this[ISpecies species]
        {
            get
            {
                if (cohorts.ContainsKey(species))
                    return (Landis.Library.UniversalCohorts.ISpeciesCohorts)GetSpeciesCohort(cohorts[species]);
                return null;
            }
        }

        public Landis.Library.UniversalCohorts.ISpeciesCohorts this[ISpecies species]
        {
            get
            {
                if (cohorts.ContainsKey(species))
                    return GetSpeciesCohort(cohorts[species]);
                return null;
            }
        }

        public override void RemoveMarkedCohorts(Landis.Library.UniversalCohorts.ICohortDisturbance disturbance)
        {
            base.RemoveMarkedCohorts(disturbance);
            ReduceOrKillCohorts(disturbance);
        }

        public override void RemoveMarkedCohorts(ISpeciesCohortsDisturbance disturbance)
        {
            //  Go through list of species cohorts from back to front so that
            //  a removal does not mess up the loop.
            base.RemoveMarkedCohorts(disturbance);
            int totalReduction = 0;
            List<Cohort> ToRemove = new List<Cohort>();
            Landis.Library.UniversalCohorts.SpeciesCohortBoolArray isSpeciesCohortDamaged = new Landis.Library.UniversalCohorts.SpeciesCohortBoolArray();
            foreach (ISpecies spc in cohorts.Keys)
            {
                Landis.Library.PnETCohorts.SpeciesCohorts speciescohort = GetSpeciesCohort(cohorts[spc]);               
                isSpeciesCohortDamaged.SetAllFalse(speciescohort.Count);
                disturbance.MarkCohortsForDeath((Landis.Library.UniversalCohorts.ISpeciesCohorts)speciescohort, isSpeciesCohortDamaged);
                for (int c = 0; c < isSpeciesCohortDamaged.Count; c++)
                {
                    if (isSpeciesCohortDamaged[c])
                    {
                        totalReduction += (int) speciescohort[c].Data.UniversalData.Biomass;
                        ToRemove.Add(cohorts[spc][c]);
                    }
                }
            }
            foreach (Cohort cohort in ToRemove)
            {
                Landis.Library.UniversalCohorts.Cohort.KilledByAgeOnlyDisturbance(disturbance, cohort, disturbance.CurrentSite, disturbance.Type);
                RemoveCohort(cohort, disturbance.Type);
            }
        }

        private void RemoveMarkedCohorts()
        {
            for (int c = cohorts.Values.Count - 1; c >= 0; c--)
            {
                List<Cohort> species_cohort = cohorts.Values.ElementAt(c);
                for (int cc = species_cohort.Count - 1; cc >= 0; cc--)
                {
                    if (species_cohort[cc].IsAlive == false)
                    {
                        bool coldKill = species_cohort[cc].ColdKill < int.MaxValue;
                        if (coldKill)
                            RemoveCohort(species_cohort[cc], new ExtensionType(Names.ExtensionName + ":Cold"));
                        else
                            RemoveCohort(species_cohort[cc], new ExtensionType(Names.ExtensionName));
                    }
                }
            }
        }

        public void RemoveCohort(Cohort cohort, ExtensionType disturbanceType)
        {
            if(disturbanceType.Name == Names.ExtensionName)
                CohortsKilledBySuccession[cohort.Species.Index] += 1;
            else if(disturbanceType.Name == (Names.ExtensionName+":Cold"))
                CohortsKilledByCold[cohort.Species.Index] += 1;
            else if(disturbanceType.Name == "disturbance:harvest")
                CohortsKilledByHarvest[cohort.Species.Index] += 1;
            else if(disturbanceType.Name == "disturbance:fire")
                CohortsKilledByFire[cohort.Species.Index] += 1;
            else if (disturbanceType.Name == "disturbance:wind")
                CohortsKilledByWind[cohort.Species.Index] += 1;
            else
                CohortsKilledByOther[cohort.Species.Index] += 1;
            if (disturbanceType.Name != Names.ExtensionName)
                Cohort.RaiseDeathEvent(this, cohort, Site, disturbanceType);
            cohorts[cohort.Species].Remove(cohort);
            if (cohorts[cohort.Species].Count == 0)
                cohorts.Remove(cohort.Species);
            if (!DisturbanceTypesReduced.Contains(disturbanceType))
            {
                Disturbance.ReduceDeadBiomass(this, disturbanceType); // Reduce dead pools before adding through Disturbance
                DisturbanceTypesReduced.Add(disturbanceType);
            }
            Disturbance.Allocate(this, cohort, disturbanceType, 1.0);  // Disturbance fraction is 1.0 for complete removals
        }

        public bool IsMaturePresent(ISpecies species)
        {
            bool speciesPresent = cohorts.ContainsKey(species);
            bool IsMaturePresent = (speciesPresent && (cohorts[species].Max(o => o.Age) >= species.Maturity)) ? true : false;
            return IsMaturePresent;
        }

        public bool AddNewCohort(Cohort newCohort)
        {
            bool addCohort = false;
            if (cohorts.ContainsKey(newCohort.Species))
            {
                // This should deliver only one KeyValuePair
                KeyValuePair<ISpecies, List<Cohort>> i = new List<KeyValuePair<ISpecies, List<Cohort>>>(cohorts.Where(o => o.Key == newCohort.Species))[0];
                List<Cohort> Cohorts = new List<Cohort>(i.Value.Where(o => o.Age < CohortBinSize));
                if (Cohorts.Count() > 1)
                {
                    foreach(Cohort Cohort in Cohorts.Skip(1))
                    {
                        newCohort.Accumulate(Cohort);
                    }
                }                
                if (Cohorts.Count() > 0)
                {
                    Cohorts[0].Accumulate(newCohort);
                    return addCohort;
                }
                else
                {
                    cohorts[newCohort.Species].Add(newCohort);
                    addCohort = true;
                    return addCohort;
                }
            }
            cohorts.Add(newCohort.Species, new List<Cohort>(new Cohort[] { newCohort }));
            addCohort = true;
            return addCohort;
        }

        Landis.Library.PnETCohorts.SpeciesCohorts GetSpeciesCohort(List<Cohort> cohorts)
        {
            Landis.Library.PnETCohorts.SpeciesCohorts spc = new Library.PnETCohorts.SpeciesCohorts(cohorts[0]);
            for (int c = 1; c < cohorts.Count; c++)
            {
                spc.AddNewCohort(cohorts[c]);
            }
            return spc;
        }

        public void AddWoodDebris(float Litter, float WoodLitterDecompRate)
        {
            lock (Globals.CWDThreadLock)
            {
                SiteVars.WoodDebris[Site].AddMass(Litter, WoodLitterDecompRate);
            }
        }

        public void RemoveWoodDebris(double percentReduction)
        {
            lock (Globals.CWDThreadLock)
            {
                SiteVars.WoodDebris[Site].ReduceMass(percentReduction);
            }
        }

        public void AddLitter(float AddLitter, IPnETSpecies spc)
        {
            lock (Globals.litterThreadLock)
            {
                double KNwdLitter = Math.Max(0.3, -0.5365 + (0.00241 * ActualET.Sum()) - (-0.01586 + (0.000056 * ActualET.Sum())) * spc.FolLignin * 100);
                SiteVars.Litter[Site].AddMass(AddLitter, KNwdLitter);
            }
        }

        public void RemoveLitter(double percentReduction)
        {
            lock (Globals.litterThreadLock)
            {
                SiteVars.Litter[Site].ReduceMass(percentReduction);
            }
        }
        
        string Header(Landis.SpatialModeling.ActiveSite site)
        {            
            string s = OutputHeaders.Time +  "," +
                       OutputHeaders.Year + "," +
                       OutputHeaders.Month + "," +
                       OutputHeaders.Ecoregion + "," + 
                       OutputHeaders.SoilType +"," +
                       OutputHeaders.NrOfCohorts + "," +
                       OutputHeaders.MaxLayerRatio + "," +
                       OutputHeaders.Layers + "," +
                       OutputHeaders.SumCanopyFrac + "," +
                       OutputHeaders.PAR0 + "," +
                       OutputHeaders.Tmin + "," +
                       OutputHeaders.Tavg + "," +
                       OutputHeaders.Tday + "," +
                       OutputHeaders.Tmax + "," +
                       OutputHeaders.Precip + "," +
                       OutputHeaders.CO2 + "," +
                       OutputHeaders.O3 + "," +
                       OutputHeaders.Runoff + "," + 
                       OutputHeaders.Leakage + "," + 
                       OutputHeaders.PotentialET + "," +
                       OutputHeaders.PotentialEvaporation + "," +
                       OutputHeaders.Evaporation + "," +
                       OutputHeaders.PotentialTranspiration + "," +
                       OutputHeaders.Transpiration + "," + 
                       OutputHeaders.Interception + "," +
                       OutputHeaders.SurfaceRunoff + "," +
                       OutputHeaders.SoilWaterContent + "," +
                       OutputHeaders.PressureHead + "," + 
                       OutputHeaders.SurfaceWater + "," +
                       OutputHeaders.availableWater + "," +
                       OutputHeaders.Snowpack + "," +
                       OutputHeaders.LAI + "," + 
                       OutputHeaders.VPD + "," + 
                       OutputHeaders.GrossPsn + "," + 
                       OutputHeaders.NetPsn + "," +
                       OutputHeaders.MaintResp + "," +
                       OutputHeaders.Wood + "," + 
                       OutputHeaders.Root + "," + 
                       OutputHeaders.Fol + "," + 
                       OutputHeaders.NSC + "," + 
                       OutputHeaders.HeteroResp + "," +
                       OutputHeaders.Litter + "," + 
                       OutputHeaders.CWD + "," +
                       OutputHeaders.WoodSenescence + "," + 
                       OutputHeaders.FoliageSenescence + "," +
                       OutputHeaders.SubCanopyPAR + ","+
                       OutputHeaders.SoilDiffusivity + "," +
                       OutputHeaders.ActiveLayerDepth+","+
                       OutputHeaders.LeakageFrac + "," +
                       OutputHeaders.AverageAlbedo + "," +
                       OutputHeaders.FrostDepth + "," +
                       OutputHeaders.SPEI;
            return s;
        }

        private void AddSiteOutput(IEcoregionPnETVariables monthdata)
        {
            double maxLayerRatio = 0;
            if (layerThreshRatio.Count() > 0)
                maxLayerRatio = layerThreshRatio.Max();
            string s = monthdata.Time + "," +
                       monthdata.Year + "," +
                       monthdata.Month + "," +
                       Ecoregion.Name + "," +
                       Ecoregion.SoilType + "," +
                       cohorts.Values.Sum(o => o.Count) + "," +
                       maxLayerRatio + "," +
                       nlayers + "," +
                       cohorts.Values.Sum(o => o.Sum(x => x.CanopyLayerFrac)) + "," +
                       monthdata.PAR0 + "," +
                       monthdata.Tmin + "," +
                       monthdata.Tavg + "," +
                       monthdata.Tday + "," +
                       monthdata.Tmax + "," +
                       monthdata.Prec + "," +
                       monthdata.CO2 + "," +
                       monthdata.O3 + "," +
                       hydrology.Runoff + "," +
                       hydrology.Leakage + "," +
                       hydrology.PotentialET + "," +
                       hydrology.PotentialEvaporation + "," +
                       hydrology.Evaporation + "," +
                       cohorts.Values.Sum(o => o.Sum(x => x.PotentialTranspiration.Sum())) + "," +
                       cohorts.Values.Sum(o => o.Sum(x => x.Transpiration.Sum())) + "," +
                       interception + "," +
                       precLoss + "," +
                       hydrology.SoilWaterContent + "," +
                       hydrology.PressureHeadTable.CalcSoilWaterPressureHead(hydrology.SoilWaterContent,Ecoregion.SoilType)+ "," +
                       hydrology.SurfaceWater + "," +
                       ((hydrology.SoilWaterContent - Ecoregion.WiltingPoint) * Ecoregion.RootingDepth * fracRootAboveFrost + hydrology.SurfaceWater) + "," +  // mm of avialable water
                       snowpack + "," +
                       cohorts.Values.Sum(o => o.Sum(x => x.LAI.Sum() * x.CanopyLayerFrac)) + "," +
                       monthdata.VPD + "," +
                       cohorts.Values.Sum(o => o.Sum(x => x.GrossPsn.Sum() * x.CanopyLayerFrac)) + "," +
                       cohorts.Values.Sum(o => o.Sum(x => x.NetPsn.Sum() * x.CanopyLayerFrac)) + "," +
                       cohorts.Values.Sum(o => o.Sum(x => x.MaintenanceRespiration.Sum() * x.CanopyLayerFrac)) + "," +
                       cohorts.Values.Sum(o => o.Sum(x => x.Wood * x.CanopyLayerFrac)) + "," +
                       cohorts.Values.Sum(o => o.Sum(x => x.Root * x.CanopyLayerFrac)) + "," +
                       cohorts.Values.Sum(o => o.Sum(x => x.Fol * x.CanopyLayerFrac)) + "," +
                       cohorts.Values.Sum(o => o.Sum(x => x.NSC * x.CanopyLayerFrac)) + "," +
                       HeterotrophicRespiration + "," +
                       SiteVars.Litter[Site].Mass + "," +
                       SiteVars.WoodDebris[Site].Mass + "," +
                       cohorts.Values.Sum(o => o.Sum(x => x.LastWoodSenescence * x.CanopyLayerFrac)) + "," +
                       cohorts.Values.Sum(o => o.Sum(x => x.LastFoliageSenescence * x.CanopyLayerFrac)) + "," +
                       subcanopypar + "," +
                       soilDiffusivity + "," +
                       activeLayerDepth[monthdata.Month - 1] * 1000 + "," +
                       leakageFrac + "," +
                       averageAlbedo[monthdata.Month - 1] + "," +
                       frostDepth[monthdata.Month - 1] * 1000 + ","+
                       monthdata.SPEI;
            this.siteoutput.Add(s);
        }
 
        public override IEnumerator<Landis.Library.UniversalCohorts.ISpeciesCohorts> GetEnumerator()
        {
            foreach (ISpecies species in cohorts.Keys)
            {
                yield return this[species];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<Landis.Library.UniversalCohorts.ISpeciesCohorts> IEnumerable<Landis.Library.UniversalCohorts.ISpeciesCohorts>.GetEnumerator()
        {
            foreach (ISpecies species in cohorts.Keys)
            {
                Landis.Library.UniversalCohorts.ISpeciesCohorts isp = this[species];
                yield return isp;
            }
             
        }

        public struct CumulativeLeafAreas
        {
            public float DarkConifer;
            public float LightConifer;
            public float Deciduous;
            public float GrassMossOpen;

            public float Total
            {
                get
                {
                    return DarkConifer + LightConifer + Deciduous + GrassMossOpen;
                }
            }

            public float DarkConiferFrac
            {
                get
                {
                    return Total == 0 ? 0 : DarkConifer / Total;
                }
            }

            public float LightConiferFrac
            {
                get
                {
                    return Total == 0 ? 0 : LightConifer / Total;
                }
            }

            public float DeciduousFrac
            {
                get
                {
                    return Total == 0 ? 0 : Deciduous / Total;
                }
            }

            public float GrassMossOpenFrac
            {
                get
                {
                    return Total == 0 ? 0 : GrassMossOpen / Total;
                }
            }

            public void Reset()
            {
                DarkConifer = 0;
                LightConifer = 0;
                Deciduous = 0;
                GrassMossOpen = 0;
            }
        }

        public IEnumerable<IEnumerable<T>> GetPowerSet<T>(List<T> list)
        {
            return from m in Enumerable.Range(0, 1 << list.Count)
                   select
                       from i in Enumerable.Range(0, list.Count)
                       where (m & (1 << i)) != 0
                       select list[i];
        }
    }
}
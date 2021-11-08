//  Copyright ...
//  Authors:  Arjan de Bruijn

using Landis.Utilities;
using Landis.Core;
using Landis.Library.Climate;
using Landis.Library.InitialCommunities;
using Landis.SpatialModeling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Landis.Library.PnETCohorts
{
    public class SiteCohorts : ISiteCohorts, Landis.Library.BiomassCohorts.ISiteCohorts, Landis.Library.AgeOnlyCohorts.ISiteCohorts
    {
        private float canopylaimax;
        private float wateravg;
        private float snowPack;
        private float[] CanopyLAI;
        private float subcanopypar;
        private float subcanopyparmax;
        private float propRootAboveFrost;
        private float topFreezeDepth;
        private float soilDiffusivity;
        private float leakageFrac;
        //private float runoffCapture;
        private float[] netpsn = null;
        //private float netpsnsum;
        private float[] grosspsn = null;
        private float[] folresp = null;
        private float[] maintresp = null;
        private float[] averageAlbedo = null;
        private float transpiration;
        private double HeterotrophicRespiration;
        private Hydrology hydrology = null;
        IEstablishmentProbability establishmentProbability = null;
        
        public ActiveSite Site;
        public Dictionary<ISpecies, List<Cohort>> cohorts = null;
        public List<ISpecies> SpeciesEstablishedByPlant = null;
        public List<ISpecies> SpeciesEstablishedBySerotiny = null;
        public List<ISpecies> SpeciesEstablishedByResprout = null;
        public List<ISpecies> SpeciesEstablishedBySeed = null;
        public List<int> CohortsKilledBySuccession = null;
        public List<int> CohortsKilledByHarvest = null;
        public List<int> CohortsKilledByFire = null;
        public List<int> CohortsKilledByWind = null;
        public List<int> CohortsKilledByOther = null;
        public List<ExtensionType> DisturbanceTypesReduced = null;
        public IEcoregionPnET Ecoregion;
        public LocalOutput siteoutput;

        private float[] AET = new float[12]; // mm/mo
        private static IDictionary<uint, SiteCohorts> initialSites;
        private static byte MaxCanopyLayers;
        //private static ushort MaxDevLyrAv;
        private static float LayerThreshRatio;
        private float interception;
        private float precLoss;
        private static byte Timestep;
        private static int CohortBinSize;
        private static bool PrecipEventsWithReplacement;
        private int nlayers;
        private static int MaxLayer;
        private static bool permafrost;
        private static bool invertPest;
        //private static string parUnits;
        Dictionary<float, float> depthTempDict = new Dictionary<float, float>();  //for permafrost
        //float lastTempBelowSnow = float.MaxValue;
        private static float maxHalfSat;
        private static float minHalfSat;
        Dictionary<double, bool> ratioAbove10 = new Dictionary<double, bool>();


        /// <summary>
        /// Occurs when a site is disturbed by an age-only disturbance.
        /// </summary>
        //public static event Landis.Library.BiomassCohorts.DisturbanceEventHandler AgeOnlyDisturbanceEvent;

        //---------------------------------------------------------------------
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
        //---------------------------------------------------------------------
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
        //---------------------------------------------------------------------
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
        //---------------------------------------------------------------------
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
        //---------------------------------------------------------------------
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
        //---------------------------------------------------------------------
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
        //---------------------------------------------------------------------
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
        //---------------------------------------------------------------------
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
        //---------------------------------------------------------------------
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
        //---------------------------------------------------------------------

        public float Transpiration
        {
            get
            {
                return transpiration;
            }
        }
        //---------------------------------------------------------------------
        public float SubcanopyPAR
        {
            get
            {
                return subcanopypar;
            }
        }
        //---------------------------------------------------------------------
        public IEstablishmentProbability EstablishmentProbability 
        {
            get
            {
                return establishmentProbability;
            }
        }
        //---------------------------------------------------------------------
        public float SubCanopyParMAX
        {
            get
            {
                return subcanopyparmax;
            }
        }
        //---------------------------------------------------------------------
        public float WaterAvg
        {
            get
            {
                return wateravg;
            }
        }
        //---------------------------------------------------------------------
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
                {
                    //return netpsn.Select(psn => (int)psn).ToArray();
                    return netpsn.ToArray();
                }
            }
        }

        public static void Initialize()
        {
            initialSites = new Dictionary<uint, SiteCohorts>();
            Timestep = ((Parameter<byte>)Names.GetParameter(Names.Timestep)).Value;
            //MaxDevLyrAv = ((Parameter<ushort>)PlugIn.GetParameter(Names.MaxDevLyrAv, 0, ushort.MaxValue)).Value;
            LayerThreshRatio = ((Parameter<float>)Names.GetParameter(Names.LayerThreshRatio, 0, float.MaxValue)).Value;
            MaxCanopyLayers = ((Parameter<byte>)Names.GetParameter(Names.MaxCanopyLayers, 0, 20)).Value;
            permafrost = ((Parameter<bool>)Names.GetParameter(Names.Permafrost)).Value;
            invertPest = ((Parameter<bool>)Names.GetParameter(Names.InvertPest)).Value;
            //parUnits = ((Parameter<string>)Names.GetParameter(Names.PARunits)).Value;
            
            Parameter<string> CohortBinSizeParm = null;
            if (Names.TryGetParameter(Names.CohortBinSize, out CohortBinSizeParm))
            {
                if (!Int32.TryParse(CohortBinSizeParm.Value, out CohortBinSize))
                {
                    throw new System.Exception("CohortBinSize is not an integer value.");
                }
            }
            else
                CohortBinSize = Timestep;

            string precipEventsWithReplacement = ((Parameter<string>)Names.GetParameter(Names.PrecipEventsWithReplacement)).Value;
            PrecipEventsWithReplacement = true;
            if (precipEventsWithReplacement == "false" || precipEventsWithReplacement == "no")
                PrecipEventsWithReplacement = false;

            maxHalfSat = 0;
            minHalfSat = float.MaxValue;
            foreach(ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
            {
                if (spc.HalfSat > maxHalfSat)
                    maxHalfSat = spc.HalfSat;
                if (spc.HalfSat < minHalfSat)
                    minHalfSat = spc.HalfSat;
        }
        }

        public SiteCohorts(DateTime StartDate, ActiveSite site, ICommunity initialCommunity, bool usingClimateLibrary, string initialCommunitiesSpinup, string SiteOutputName = null)
        {
            Cohort.SetSiteAccessFunctions(this);
            this.Ecoregion = EcoregionData.GetPnETEcoregion(Globals.ModelCore.Ecoregion[site]);
            this.Site = site;
            cohorts = new Dictionary<ISpecies, List<Cohort>>();
            SpeciesEstablishedByPlant = new List<ISpecies>();
            SpeciesEstablishedBySerotiny = new List<ISpecies>();
            SpeciesEstablishedByResprout = new List<ISpecies>();
            SpeciesEstablishedBySeed = new List<ISpecies>();
            CohortsKilledBySuccession = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByHarvest = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByFire = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByWind = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByOther = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            DisturbanceTypesReduced = new List<ExtensionType>();

            uint key = ComputeKey((ushort)initialCommunity.MapCode, Globals.ModelCore.Ecoregion[site].MapCode);

            if (initialSites.ContainsKey(key) && SiteOutputName == null)
            {
                establishmentProbability = new EstablishmentProbability(null, null);
                subcanopypar = initialSites[key].subcanopypar;
                subcanopyparmax = initialSites[key].SubCanopyParMAX;
                wateravg = initialSites[key].wateravg;

                hydrology = new Hydrology(initialSites[key].hydrology.Water);

                SiteVars.WoodyDebris[Site] = SiteVars.WoodyDebris[initialSites[key].Site].Clone();
                SiteVars.Litter[Site] = SiteVars.Litter[initialSites[key].Site].Clone();
                SiteVars.FineFuels[Site] = SiteVars.Litter[Site].Mass;
                //PlugIn.PressureHead[Site] = hydrology.GetPressureHead(this.Ecoregion);
                this.canopylaimax = initialSites[key].CanopyLAImax;

                List<float> cohortLayerProp = new List<float>();
                foreach (ISpecies spc in initialSites[key].cohorts.Keys)
                {
                    foreach (Cohort cohort in initialSites[key].cohorts[spc])
                    {
                        bool addCohort = AddNewCohort(new Cohort(cohort));
                        float layerProp = cohort.BiomassLayerProp;
                        cohortLayerProp.Add(layerProp);
                    }          
                }
                int index = 0;
                foreach (Cohort cohort in AllCohorts)
                {
                    cohort.BiomassLayerProp = cohortLayerProp[index];
                    index++;
                }

                this.netpsn = initialSites[key].NetPsn;
                this.folresp = initialSites[key].FolResp;
                this.grosspsn = initialSites[key].GrossPsn;
                this.maintresp = initialSites[key].MaintResp;
                this.averageAlbedo = initialSites[key].AverageAlbedo;
                this.CanopyLAI = initialSites[key].CanopyLAI;
                this.transpiration = initialSites[key].Transpiration;

                // Calculate AdjFolFrac
                AllCohorts.ForEach(x => x.CalcAdjFracFol());

            }
            else
            {
                if (initialSites.ContainsKey(key) == false)
                {
                    initialSites.Add(key, this);
                }
                List<IEcoregionPnETVariables> ecoregionInitializer = EcoregionData.GetData(Ecoregion, StartDate, StartDate.AddMonths(1));
                hydrology = new Hydrology(Ecoregion.FieldCap);
                wateravg = hydrology.Water;
                subcanopypar = ecoregionInitializer[0].PAR0;
                subcanopyparmax = subcanopypar;

                SiteVars.WoodyDebris[Site] = new Library.Biomass.Pool();
                SiteVars.Litter[Site] = new Library.Biomass.Pool();
                SiteVars.FineFuels[Site] = SiteVars.Litter[Site].Mass;
                //PlugIn.PressureHead[Site] = hydrology.GetPressureHead(Ecoregion);

                if (SiteOutputName != null)
                {
                    this.siteoutput = new LocalOutput(SiteOutputName, "Site.csv", Header(site));

                    establishmentProbability = new EstablishmentProbability(SiteOutputName, "Establishment.csv");
                }
                else
                {
                    establishmentProbability = new EstablishmentProbability(null, null);
                }

                bool biomassProvided = false;
                foreach (Landis.Library.BiomassCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                {
                    foreach (Landis.Library.BiomassCohorts.ICohort cohort in speciesCohorts)
                    {
                        if (cohort.Biomass > 0)  // 0 biomass indicates biomass value was not read in
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
                        foreach (Landis.Library.BiomassCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                        {
                            foreach (Landis.Library.BiomassCohorts.ICohort cohort in speciesCohorts)
                            {
                                // TODO: Add warning if biomass is 0
                                bool addCohort = AddNewCohort(new Cohort(SpeciesParameters.SpeciesPnET[cohort.Species], cohort.Age, cohort.Biomass, SiteOutputName, (ushort)(StartDate.Year - cohort.Age)));
                                CohortBiomassList.Add(AllCohorts.Last().Biomass);
                                CohortMaxBiomassList.Add(AllCohorts.Last().BiomassMax);
                            }
                        }
                    }
                    else
                    {
                        if (initialCommunitiesSpinup.ToLower() != "spinuplayers")
                            Globals.ModelCore.UI.WriteLine("Warning:  InitialCommunitiesSpinup parameter is not 'Spinup', 'SpinupLayers' or 'NoSpinup'.  Biomass is provided so using 'SpinUpLayers' by default.");
                        SpinUp(StartDate, site, initialCommunity, usingClimateLibrary, SiteOutputName, false);
                        // species-age key to store maxbiomass values
                        Dictionary<ISpecies, Dictionary<int, int>> cohortDictionary = new Dictionary<ISpecies, Dictionary<int, int>>();
                        foreach (Cohort cohort in AllCohorts)
                        {
                            ISpecies spp = cohort.Species;
                            int age = cohort.Age;
                            if(cohortDictionary.ContainsKey(spp))
                            {
                                if(cohortDictionary[spp].ContainsKey(age))
                                {
                                    // message duplicate species and age
                                }
                                else
                                {
                                    cohortDictionary[spp].Add(age, (int)cohort.BiomassMax);
                                }
                            }
                            else
                            {
                                Dictionary<int, int> ageDictionary = new Dictionary<int, int>();
                                ageDictionary.Add(age, (int)cohort.BiomassMax);
                                cohortDictionary.Add(spp, ageDictionary);
                            }
                            
                        }
                        ClearAllCohorts();
                        foreach (Landis.Library.BiomassCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                        {
                            foreach (Landis.Library.BiomassCohorts.ICohort cohort in speciesCohorts)
                            {
                                // TODO: Add warning if biomass is 0
                                int age = cohort.Age;
                                ISpecies spp = cohort.Species;
                                int cohortMaxBiomass = cohortDictionary[spp][age];
                                

                                bool addCohort = AddNewCohort(new Cohort(SpeciesParameters.SpeciesPnET[cohort.Species], cohort.Age, cohort.Biomass, cohortMaxBiomass, SiteOutputName, (ushort)(StartDate.Year - cohort.Age)));
                                CohortBiomassList.Add(AllCohorts.Last().Biomass);
                                CohortMaxBiomassList.Add(AllCohorts.Last().BiomassMax);
                            }
                        }
                    }
                    // Sort cohorts into layers                    
                    List<List<double>> cohortBins = GetBinsByCohort(CohortMaxBiomassList);


                    float[] CanopyLAISum = new float[MaxCanopyLayers];
                    float[] LayerBiomass = new float[MaxCanopyLayers];
                    List<float>[] LayerBiomassValues = new List<float>[MaxCanopyLayers];
                    CanopyLAI = new float[MaxCanopyLayers];
                    foreach (Cohort cohort in AllCohorts)
                    {
                        int layerIndex = 0;
                        foreach(List<double> layerBiomassList in cohortBins)
                        {
                            if(layerBiomassList.Contains(cohort.BiomassMax))
                            {
                                cohort.Layer = (byte)layerIndex;
                                // if "ground" then ensure cohort.Layer = 0
                                if(cohort.SpeciesPnET.Lifeform.ToLower().Contains("ground"))
                                {
                                    cohort.Layer = 0;
                                }
                                break;
                            }
                            layerIndex++;
                        }
                        int layer = cohort.Layer;
                        if (LayerBiomassValues[layer] == null)
                        {
                            LayerBiomassValues[layer] = new List<float>();
                        }
                        LayerBiomassValues[layer].Add(cohort.Biomass);
                        CanopyLAISum[layer] += (cohort.LAI.Sum() * cohort.Biomass);
                        LayerBiomass[layer] += cohort.Biomass;
                        //MaxLAI[layer] = Math.Max(MaxLAI[layer], cohort.SpeciesPNET.MaxLAI);
                    
                    }
                    foreach (Cohort cohort in AllCohorts)
                    {
                        int layer = cohort.Layer;
                        float denomSum = 0f;
                        foreach(float cohortBio in LayerBiomassValues[layer])
                        {
                            denomSum += (float)Math.Sqrt(cohortBio / cohort.Biomass);
                        }
                        cohort.BiomassLayerProp = 1.0f / denomSum;
                        float newAGBiomass = cohort.Biomass / cohort.BiomassLayerProp;
                        float newTotalBiomass = (newAGBiomass - cohort.Fol) / (1 - cohort.SpeciesPnET.FracBelowG);
                        //cohort.BiomassLayerProp = cohort.Biomass / LayerBiomass[layer];
                        cohort.ChangeBiomass( (int)Math.Round(newTotalBiomass  - cohort.TotalBiomass));
                    }
                    for (int layer = 0; layer < MaxCanopyLayers; layer++)
                    {
                        if (LayerBiomass[layer] > 0)
                            CanopyLAI[layer] = CanopyLAISum[layer] / LayerBiomass[layer];
                        else
                            CanopyLAI[layer] = 0;
                    }
                    this.canopylaimax = CanopyLAI.Sum();

                    //CalculateInitialWater(StartDate);
                }
                else
                {
                    SpinUp(StartDate, site, initialCommunity, usingClimateLibrary, SiteOutputName);
                }
            }
        }

        // Spins up sites if no biomass is provided
        private void SpinUp(DateTime StartDate, ActiveSite site, ICommunity initialCommunity, bool usingClimateLibrary, string SiteOutputName = null, bool AllowMortality = true)
        {
            List<Landis.Library.AgeOnlyCohorts.ICohort> sortedAgeCohorts = new List<Landis.Library.AgeOnlyCohorts.ICohort>();
            foreach (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
            {
                foreach (Landis.Library.AgeOnlyCohorts.ICohort cohort in speciesCohorts)
                {
                    sortedAgeCohorts.Add(cohort);
                }
            }
            sortedAgeCohorts = new List<Library.AgeOnlyCohorts.ICohort>(sortedAgeCohorts.OrderByDescending(o => o.Age));

            if (sortedAgeCohorts.Count == 0) return;

            List<double> CohortMaxBiomassList = new List<double>();


            DateTime date = StartDate.AddYears(-(sortedAgeCohorts[0].Age - 1));

            Landis.Library.Parameters.Ecoregions.AuxParm<List<EcoregionPnETVariables>> mydata = new Library.Parameters.Ecoregions.AuxParm<List<EcoregionPnETVariables>>(Globals.ModelCore.Ecoregions);

            while (date.CompareTo(StartDate) <= 0)
            {
                //  Add those cohorts that were born at the current year
                while (sortedAgeCohorts.Count() > 0 && StartDate.Year - date.Year == (sortedAgeCohorts[0].Age - 1))
                {
                    Cohort cohort = new Cohort(sortedAgeCohorts[0].Species, SpeciesParameters.SpeciesPnET[sortedAgeCohorts[0].Species], (ushort)date.Year, SiteOutputName);

                    bool addCohort = AddNewCohort(cohort);

                    sortedAgeCohorts.Remove(sortedAgeCohorts[0]);
                }

                // Simulation time runs untill the next cohort is added
                DateTime EndDate = (sortedAgeCohorts.Count == 0) ? StartDate : new DateTime((int)(StartDate.Year - (sortedAgeCohorts[0].Age - 1)), 1, 15);
                if (date.CompareTo(StartDate) == 0)
                    break;

                var climate_vars = usingClimateLibrary ? EcoregionData.GetClimateRegionData(Ecoregion, date, EndDate, Climate.Climate.Phase.SpinUp_Climate) : EcoregionData.GetData(Ecoregion, date, EndDate);

                Grow(climate_vars, AllowMortality);

                date = EndDate;

            }
            if (sortedAgeCohorts.Count > 0) throw new System.Exception("Not all cohorts in the initial communities file were initialized.");
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

        public void SetAet(float value, int Month)
        {
            AET[Month-1] = value;
        }

        class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
        }

        private static float ComputeMaxSnowMelt(float Tave, float DaySpan)
        {
            // Snowmelt rate can range between 1.6 to 6.0 mm/degree day, and default should be 2.74 according to NRCS Part 630 Hydrology National Engineering Handbook (Chapter 11: Snowmelt)
            return 2.74f * Math.Max(0, Tave) * DaySpan;
        }
        private static float CumputeSnowFraction(float Tave)
        {
            return (float)Math.Max(0.0, Math.Min(1.0, (Tave - 2) / -7));
        }

        public bool Grow(List<IEcoregionPnETVariables> data, bool AllowMortality = true)
        {
            bool success = true;
            float sumPressureHead = 0;
            int countPressureHead = 0;

            establishmentProbability.ResetPerTimeStep();
            Cohort.SetSiteAccessFunctions(this);

            canopylaimax = float.MinValue;

            SortedDictionary<double, Cohort> SubCanopyCohorts = new SortedDictionary<double, Cohort>();
            List<double> CohortBiomassList = new List<double>();
            List<double> CohortMaxBiomassList = new List<double>();
            int SiteAboveGroundBiomass = AllCohorts.Sum(a => a.Biomass);
            MaxLayer = 0;
            for (int cohort = 0; cohort < AllCohorts.Count(); cohort++)
            {
                if (Globals.ModelCore.CurrentTime > 0)
                {
                    AllCohorts[cohort].CalculateDefoliation(Site, SiteAboveGroundBiomass);
                }
                CohortBiomassList.Add(AllCohorts[cohort].TotalBiomass);
                CohortMaxBiomassList.Add(AllCohorts[cohort].BiomassMax);
            }

            //List<List<int>> rawBins = GetBins(new List<double>(SubCanopyCohorts.Keys));
            /*//Debug
            if (Globals.ModelCore.CurrentTime == 10 && this.Site.Location.Row == 188 && this.Site.Location.Column == 22 && CohortBiomassList.Count() == 9)
            {
                Globals.ModelCore.UI.WriteLine("AllCohorts = ");
                foreach (Cohort c in AllCohorts)
                {
                    Globals.ModelCore.UI.WriteLine("Species = "+ c.Species.Name+ "; Age = "+ c.Age.ToString()+"; Biomass = "+c.Biomass.ToString()+"; Layer = "+c.Layer.ToString());
                }

                Globals.ModelCore.UI.WriteLine("CohortBiomassList = ");
                foreach (double cohortBio in CohortBiomassList)
                {
                    Globals.ModelCore.UI.WriteLine(cohortBio.ToString());
                }
            }*/
            ratioAbove10.Clear();
            List<List<double>> cohortBins = GetBinsByCohort(CohortMaxBiomassList);

            List<int> cohortAges = new List<int>();
            List<List<int>> rawBins = new List<List<int>>();
            int subLayerIndex = 0;
            bool reducedLayer = false;
            for (int cohort = 0; cohort < AllCohorts.Count(); cohort++)
            {
                string lifeForm = AllCohorts[cohort].SpeciesPnET.Lifeform.ToLower();
                int cohortLayer = 0;
                // Lifeform "ground" always restricted to layer 0
                if (!lifeForm.Contains("ground"))
                {
                    for (int j = 0; j < cohortBins.Count(); j++)
                    {
                        if (cohortBins[j].Contains(AllCohorts[cohort].BiomassMax))
                            cohortLayer = j;
                    }
                    //if ((AllCohorts[cohort].Layer > cohortLayer) && (!string.IsNullOrEmpty(lifeForm))
                    //    && (lifeForm.Contains("tree") || lifeForm.Contains("shrub"))) 
                   // {
                   //     reducedLayer = true;
                   // }

                    if (AllCohorts[cohort].Layer > MaxLayer)
                        MaxLayer = AllCohorts[cohort].Layer;
                }
                for (int i = 1; i <= Globals.IMAX; i++)
                {
                    SubCanopyCohorts.Add(subLayerIndex, AllCohorts[cohort]);
                    while (rawBins.Count() < (cohortLayer + 1))
                    {
                        List<int> subList = new List<int>();
                        //subList.Add(subLayerIndex);
                        rawBins.Add(subList);
                    }
                    //else
                    rawBins[cohortLayer].Add(subLayerIndex);
                    subLayerIndex++;
                }
                if (!cohortAges.Contains(AllCohorts[cohort].Age))
                {
                    cohortAges.Add(AllCohorts[cohort].Age);
                }
            }

            List<List<int>> LayeredBins = new List<List<int>>();

           /*if ((rawBins.Count > 0) && (reducedLayer)) // cohort(s) were previously in a higher layer
            {
                double maxCohortBiomass = CohortBiomassList.Max();
                for (int i = 0; i < rawBins.Count(); i++)
                {
                    List<int> binLayers = rawBins[i];
                    for (int b = 0; b < binLayers.Count(); b++)
                    {
                        int layerKey = binLayers[b];
                        int canopyIndex = i;
                        Cohort layerCohort = SubCanopyCohorts.Values.ToArray()[layerKey];
                        double cohortBio = layerCohort.TotalBiomass;
                        //bool highRatio = ((maxCohortBiomass / cohortBio) > 10.0);
                        //if(layerCohort.Layer > i && !highRatio)
                        if (layerCohort.Layer > i)

                        {
                            canopyIndex = layerCohort.Layer;
                        }
                        if (LayeredBins.ElementAtOrDefault(canopyIndex) == null)
                        {
                            while (LayeredBins.ElementAtOrDefault(canopyIndex) == null)
                            {
                                LayeredBins.Add(new List<int>());
                            }
                        }
                        LayeredBins[canopyIndex].Add(layerKey);
                    }
                }
            }
            else
            {*/
                LayeredBins = rawBins;
            //}
            nlayers = 0;
            foreach (List<int> layerList in LayeredBins)
            {
                if (layerList.Count > 0)
                    nlayers++;
            }
            MaxLayer = LayeredBins.Count - 1;

            //List<List<int>> bins = new List<List<int>>();
            //bins = LayeredBins;

            List<List<int>> random_range = GetRandomRange(LayeredBins);
             
            folresp = new float[13];
            netpsn = new float[13];
            grosspsn = new float[13];
            maintresp = new float[13];
            averageAlbedo = new float[13];

            //Dictionary<ISpeciesPnET, List<float>> annualEstab = new Dictionary<ISpeciesPnET, List<float>>();
            Dictionary<ISpeciesPnET, float> cumulativeEstab = new Dictionary<ISpeciesPnET, float>();
            Dictionary<ISpeciesPnET, List<float>> annualFwater = new Dictionary<ISpeciesPnET, List<float>>();
            Dictionary<ISpeciesPnET, float> cumulativeFwater = new Dictionary<ISpeciesPnET, float>();
            Dictionary<ISpeciesPnET, List<float>> annualFrad = new Dictionary<ISpeciesPnET, List<float>>();
            Dictionary<ISpeciesPnET, float> cumulativeFrad = new Dictionary<ISpeciesPnET, float>();
            Dictionary<ISpeciesPnET, float> monthlyEstab = new Dictionary<ISpeciesPnET, float>();
            Dictionary<ISpeciesPnET, int> monthlyCount = new Dictionary<ISpeciesPnET, int>();
            Dictionary<ISpeciesPnET, int> coldKillMonth = new Dictionary<ISpeciesPnET, int>(); // month in which cold kills each species

            foreach (ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
            {
                //annualEstab[spc] = new List<float>();
                cumulativeEstab[spc] = 1;
                annualFwater[spc] = new List<float>();
                cumulativeFwater[spc] = 0;
                annualFrad[spc] = new List<float>();
                cumulativeFrad[spc] = 0;
                monthlyCount[spc] = 0;
                coldKillMonth[spc] = int.MaxValue;
            }

            float[] lastOzoneEffect = new float[SubCanopyCohorts.Count()];
            for (int i = 0; i < lastOzoneEffect.Length; i++)
            {
                lastOzoneEffect[i] = 0;
            }


            float lastPropBelowFrost = (hydrology.FrozenDepth/Ecoregion.RootingDepth);
            int daysOfWinter = 0;

            if (Globals.ModelCore.CurrentTime > 0) // cold can only kill after spinup
            {
                // Loop through months & species to determine if cold temp would kill any species
                float extremeMinTemp = float.MaxValue;
                int extremeMonth = 0;
                    for (int m = 0; m < data.Count(); m++)
                    {
                        float minTemp = data[m].Tave - (float)(3.0 * Ecoregion.WinterSTD);
                    if(minTemp < extremeMinTemp)
                        {
                            extremeMinTemp = minTemp;
                            extremeMonth = m;
                        }
                    }
                SiteVars.ExtremeMinTemp[Site] = extremeMinTemp;
                foreach (ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
                {
                        // Check if low temp kills species
                    if (extremeMinTemp < spc.ColdTol)
                        {
                        coldKillMonth[spc] = extremeMonth;
                        }

                    }
                }
            //Clear pressurehead site values
            sumPressureHead = 0;
            countPressureHead = 0;
            for (int m = 0; m < data.Count(); m++)
            {
                Ecoregion.Variables = data[m];
                transpiration = 0;
                subcanopypar = data[m].PAR0;
                interception = 0;

                // Reset monthly variables that get reported as single year snapshots
                if (data[m].Month == 1)
                {
                    folresp = new float[13];
                    netpsn = new float[13];
                    grosspsn = new float[13];
                    maintresp = new float[13];
                    averageAlbedo = new float[13];
                    // Reset annual SiteVars
                    SiteVars.AnnualPET[Site] = 0;
                    SiteVars.ClimaticWaterDeficit[Site] = 0;
                }

                float ozoneD40 = 0;
                if (m > 0)
                    ozoneD40 = Math.Max(0, data[m].O3 - data[m - 1].O3);
                else
                    ozoneD40 = data[m].O3;
                float O3_D40_ppmh = ozoneD40 / 1000; // convert D40 units to ppm h

                propRootAboveFrost = 1;
                leakageFrac = Ecoregion.LeakageFrac;
                float propThawed = 0;

                // snow calculations - from "Soil thawing worksheet with snow.xlsx"
                if (data[m].Tave <= 0)
                {
                    daysOfWinter += (int)data[m].DaySpan;
                }
                else if (snowPack > 0)
                {
                    daysOfWinter += (int)data[m].DaySpan;
                }
                else
                {
                    daysOfWinter = 0;
                }

                float Psno_kg_m3 = Globals.bulkIntercept + (Globals.bulkSlope * daysOfWinter); //kg/m3
                float Psno_g_cm3 = Psno_kg_m3 / 1000; //g/cm3

                float sno_dep = Globals.Pwater * (snowPack / 1000) / Psno_kg_m3; //m
                //if (data[m].Tave >= 0)  -- snowmelt has already been accounted for
                //{
                //    float fracAbove0 = data[m].Tmax / (data[m].Tmax - data[m].Tmin);
                //    sno_dep = sno_dep * fracAbove0;
                //}
                // from CLM model - https://escomp.github.io/ctsm-docs/doc/build/html/tech_note/Soil_Snow_Temperatures/CLM50_Tech_Note_Soil_Snow_Temperatures.html#soil-and-snow-thermal-properties
                // Eq. 85 - Jordan (1991)

                if (permafrost)
                {
                    float lambda_Snow = (float) (Globals.lambAir+((0.0000775*Psno_kg_m3)+(0.000001105*Math.Pow(Psno_kg_m3,2)))*(Globals.lambIce-Globals.lambAir))*3.6F*24F; //(kJ/m/d/K) includes unit conversion from W to kJ
                    float vol_heat_capacity_snow = Globals.snowHeatCapacity * Psno_kg_m3 / 1000f; // kJ/m3/K
                                                                                          //float Ks_snow = data[m].DaySpan * lambda_Snow / vol_heat_capacity_snow; //thermal diffusivity (m2/month)
                    float Ks_snow = 1000000F / 86400F * (lambda_Snow / vol_heat_capacity_snow); //thermal diffusivity (mm2/s)
                    float damping = (float)Math.Sqrt((2.0F * Ks_snow) / Constants.omega);
                    float DRz_snow = 1F;
                    if (sno_dep > 0)
                        DRz_snow = (float)Math.Exp(-1.0F * sno_dep * damping); // Damping ratio for snow - adapted from Kang et al. (2000) and Liang et al. (2014)

                    float mossDepth = Ecoregion.MossDepth; //m
                    float cv = 2500; // heat capacity moss - kJ/m3/K (Sazonova and Romanovsky 2003)
                    float lambda_moss = 432; // kJ/m/d/K - converted from 0.2 W/mK (Sazonova and Romanovsky 2003)
                    float moss_diffusivity = lambda_moss / cv;
                    float damping_moss = (float)Math.Sqrt((2.0F * moss_diffusivity) / Constants.omega);
                    float DRz_moss = (float)Math.Exp(-1.0F * mossDepth * damping_moss); // Damping ratio for moss - adapted from Kang et al. (2000) and Liang et al. (2014)


                    //float waterContent = (float)Math.Min(1.0, hydrology.Water / Ecoregion.RootingDepth);  //m3/m3
                    //float waterContent = hydrology.Water/1000;
                    float waterContent = hydrology.Water;// volumetric m/m
                                                         // Permafrost calculations - from "Soil thawing worksheet.xlsx"
                                                         // 
                                                         //if (data[m].Tave < minMonthlyAvgTemp)
                                                         //    minMonthlyAvgTemp = data[m].Tave;
                                                         //Calculations of diffusivity from soil properties 
                                                         //float porosity = Ecoregion.Porosity / Ecoregion.RootingDepth;  //m3/m3                    
                                                         //float porosity = Ecoregion.Porosity/1000;  // m/m   
                    float porosity = Ecoregion.Porosity;  // volumetric m/m 
                    float ga = 0.035F + 0.298F * (waterContent / porosity);
                    float Fa = ((2.0F / 3.0F) / (1.0F + ga * ((Constants.lambda_a / Constants.lambda_w) - 1.0F))) + ((1.0F / 3.0F) / (1.0F + (1.0F - 2.0F * ga) * ((Constants.lambda_a / Constants.lambda_w) - 1.0F))); // ratio of air temp gradient
                    float Fs = PressureHeadSaxton_Rawls.GetFs(Ecoregion.SoilType);
                    float lambda_s = PressureHeadSaxton_Rawls.GetLambda_s(Ecoregion.SoilType);
                    float lambda_theta = (Fs * (1.0F - porosity) * lambda_s + Fa * (porosity - waterContent) * Constants.lambda_a + waterContent * Constants.lambda_w) / (Fs * (1.0F - porosity) + Fa * (porosity - waterContent) + waterContent); //soil thermal conductivity (kJ/m/d/K)
                    float D = lambda_theta / PressureHeadSaxton_Rawls.GetCTheta(Ecoregion.SoilType);  //m2/day
                    float Dmms = D * 1000000 / 86400; //mm2/s
                    soilDiffusivity = Dmms;
                    float Dmonth = D * data[m].DaySpan; // m2/month
                    float ks = Dmonth * 1000000F / (data[m].DaySpan * (Constants.SecondsPerHour * 24)); // mm2/s
                                                                                                        //float d = (float)Math.Pow((Constants.omega / (2.0F * Dmonth)), (0.5));
                    float d = (float)Math.Sqrt(2 * Dmms / Constants.omega);

                    float maxDepth = Ecoregion.RootingDepth + Ecoregion.LeakageFrostDepth;
                    topFreezeDepth = maxDepth / 1000;
                    float bottomFreezeDepth = maxDepth / 1000;
                    float testDepth = 0;

                    float tSum = 0;
                    float pSum = 0;
                    float tMax = float.MinValue;
                    float tMin = float.MaxValue;
                    int maxMonth = 0;
                    int mCount = 0;
                    if (m < 12)
                    {
                        mCount = Math.Min(12, data.Count());
                        foreach (int z in Enumerable.Range(0, mCount))
                        {
                            tSum += data[z].Tave;
                            pSum += data[z].Prec;
                            if (data[z].Tave > tMax)
                            {
                                tMax = data[z].Tave;
                                maxMonth = data[z].Month;
                            }
                            if (data[z].Tave < tMin)
                                tMin = data[z].Tave;
                        }
                    }
                    else
                    {
                        mCount = 12;
                        foreach (int z in Enumerable.Range(m - 11, 12))
                        {
                            tSum += data[z].Tave;
                            pSum += data[z].Prec;
                            if (data[z].Tave > tMax)
                            {
                                tMax = data[z].Tave;
                                maxMonth = data[z].Month;
                            }
                            if (data[z].Tave < tMin)
                                tMin = data[z].Tave;
                        }
                    }
                    float annualTavg = tSum / mCount;
                    float annualPcpAvg = pSum / mCount;
                    float tAmplitude = (tMax - tMin) / 2;

                    // Calculation of diffusivity from climate data (Soil thawing_Campbell_121719.xlsx, Fit_Diffusivity_Climate.R)
                    //if(this.AbovegroundBiomassSum >= permafrostMinVegBiomass) // Vegetated
                    //    soilDiffusivity = (float)Math.Max(0.006, (-0.09674 - 0.005967 * annualTavg + 0.00001552 * annualTavg * annualTavg + 0.002395 * annualPcpAvg)); // Diffusivity (m2/day)
                    //else // Bare
                    //    soilDiffusivity = (float)Math.Max(0.006,(-0.050839 - 0.0055182 * annualTavg - 0.0002212 * annualTavg * annualTavg + 0.0011535 * annualPcpAvg)); // Diffusivity (m2/day)

                    while (testDepth <= (maxDepth / 1000.0))
                    {
                        float DRz = (float)Math.Exp(-1.0F * testDepth * d); // adapted from Kang et al. (2000) and Liang et al. (2014)
                                                                            //float zTemp = annualTavg + (tempBelowSnow - annualTavg) * DRz;
                        float zTemp = (float)(annualTavg + tAmplitude * DRz_snow * DRz_moss * DRz * Math.Sin(Constants.omega * (data[m].Month + (maxMonth + 1)) - testDepth / d));
                        depthTempDict[testDepth] = zTemp;
                        if ((zTemp <= 0) && (testDepth < topFreezeDepth))
                            topFreezeDepth = testDepth;
                        testDepth += 0.25F;
                    }
                    propRootAboveFrost = Math.Min(1, (topFreezeDepth * 1000) / Ecoregion.RootingDepth);
                    float propRootBelowFrost = 1 - propRootAboveFrost;
                    propThawed = Math.Max(0, propRootAboveFrost - (1 - lastPropBelowFrost));
                    float propNewFrozen = Math.Max(0, propRootBelowFrost - lastPropBelowFrost);
                    if (propRootAboveFrost < 1) // If part of the rooting zone is frozen
                    {
                        if (propNewFrozen > 0)  // freezing
                        {
                            float newFrozenSoil = propNewFrozen * Ecoregion.RootingDepth;
                            bool successpct = hydrology.SetFrozenWaterContent(((hydrology.FrozenDepth * hydrology.FrozenWaterContent) + (newFrozenSoil * hydrology.Water)) / (hydrology.FrozenDepth + newFrozenSoil));
                            bool successdepth = hydrology.SetFrozenDepth(Ecoregion.RootingDepth * propRootBelowFrost); // Volume of rooting soil that is frozen
                                                                                                                       // Water is a volumetric value (mm/m) so frozen water does not need to be removed, as the concentration stays the same
                        }
                    }
                    if (propThawed > 0) // thawing
                    {
                        // Thawing soil water added to existing water - redistributes evenly in active soil
                        float existingWater = (1 - lastPropBelowFrost) * hydrology.Water;
                        float thawedWater = propThawed * hydrology.FrozenWaterContent;
                        float newWaterContent = (existingWater + thawedWater) / propRootAboveFrost;
                        hydrology.AddWater(newWaterContent - hydrology.Water, Ecoregion.RootingDepth * propRootBelowFrost);
                        bool successdepth = hydrology.SetFrozenDepth(Ecoregion.RootingDepth * propRootBelowFrost);  // Volume of rooting soil that is frozen
                    }
                    float leakageFrostReduction = 1.0F;
                    if ((topFreezeDepth * 1000) < (Ecoregion.RootingDepth + Ecoregion.LeakageFrostDepth))
                    {
                        if ((topFreezeDepth * 1000) < Ecoregion.RootingDepth)
                        {
                            leakageFrostReduction = 0.0F;
                        }
                        else
                        {
                            leakageFrostReduction = (Math.Min((topFreezeDepth * 1000), Ecoregion.LeakageFrostDepth) - Ecoregion.RootingDepth) / (Ecoregion.LeakageFrostDepth - Ecoregion.RootingDepth);
                        }
                    }
                    leakageFrac = Ecoregion.LeakageFrac * leakageFrostReduction;
                    lastPropBelowFrost = propRootBelowFrost;

                }

                // permafrost
                //float frostFreeProp = Math.Min(1.0F, frostFreeSoilDepth / Ecoregion.RootingDepth);

                AllCohorts.ForEach(x => x.InitializeSubLayers());

                if (data[m].Prec < 0) throw new System.Exception("Error, this.data[m].Prec = " + data[m].Prec + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                float snowmelt = Math.Min(snowPack, ComputeMaxSnowMelt(data[m].Tave, data[m].DaySpan)); // mm
                if (snowmelt < 0) throw new System.Exception("Error, snowmelt = " + snowmelt + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                float newsnow = CumputeSnowFraction(data[m].Tave) * data[m].Prec;
                float newsnowpack = newsnow * (1 - Ecoregion.SnowSublimFrac); // (mm) Account for sublimation here
                if (newsnowpack < 0 || newsnowpack > data[m].Prec)
                {
                    throw new System.Exception("Error, newsnowpack = " + newsnowpack + " availablePrecipitation = " + data[m].Prec);
                }

                snowPack += newsnowpack - snowmelt;
                if (snowPack < 0) throw new System.Exception("Error, snowPack = " + snowPack + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                float newrain = data[m].Prec - newsnow;

                // Reduced by interception
                if (CanopyLAI == null)
                {
                    CanopyLAI = new float[MaxCanopyLayers];
                    //MaxLAI = new float[MaxCanopyLayers];
                }
                interception = newrain * (float)(1 - Math.Exp(-1 * Ecoregion.PrecIntConst * CanopyLAI.Sum()));
                float surfaceRain = newrain - interception;

                // Reduced by PrecLossFrac
                precLoss = surfaceRain * Ecoregion.PrecLossFrac;
                float precin = surfaceRain - precLoss;

                if (precin < 0) throw new System.Exception("Error, precin = " + precin + " newsnow = " + newsnow + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                int numEvents = Ecoregion.PrecipEvents;  // maximum number of precipitation events per month
                float PrecInByEvent = precin / numEvents;  // Divide precip into discreet events within the month
                if (PrecInByEvent < 0) throw new System.Exception("Error, PrecInByEvent = " + PrecInByEvent + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                if (propRootAboveFrost >= 1)
                {
                    bool successpct = hydrology.SetFrozenWaterContent(0F);
                    bool successdepth = hydrology.SetFrozenDepth(0F);
                }
                float MeltInWater = snowmelt;

                // Randomly choose which layers will receive the precip events
                // If # of layers < precipEvents, some layers will show up multiple times in number list.  This ensures the same number of precip events regardless of the number of cohorts
                List<int> randomNumbers = new List<int>();
                if (PrecipEventsWithReplacement)// Sublayer selection with replacement
                {
                    while (randomNumbers.Count < numEvents)
                    {
                        int rand = Statistics.DiscreteUniformRandom(1, SubCanopyCohorts.Count());
                        randomNumbers.Add(rand);
                    }
                }
                else // Sublayer selection without replacement
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
                var groupList = randomNumbers.GroupBy(i => i);

                // Reset Hydrology values
                hydrology.RunOff = 0;
                hydrology.Leakage = 0;
                hydrology.Evaporation = 0;



                float O3_ppmh = data[m].O3 / 1000; // convert AOT40 units to ppm h
                float lastO3 = 0;
                if (m > 0)
                    lastO3 = (data[m - 1].O3 / 1000f);
                float O3_ppmh_month = Math.Max(0, O3_ppmh - lastO3);

                List<ISpeciesPnET> species = SpeciesParameters.SpeciesPnET.AllSpecies.ToList();
                Dictionary<string, float> DelAmax_spp = new Dictionary<string, float>();
                Dictionary<string, float> JCO2_spp = new Dictionary<string, float>();
                Dictionary<string, float> Amax_spp = new Dictionary<string, float>();
                Dictionary<string, float> FTempPSNRefNetPSN_spp = new Dictionary<string, float>();
                Dictionary<string, float> Ca_Ci_spp = new Dictionary<string, float>();

                float subCanopyPrecip = 0;
                float subCanopyMelt = 0;
                int subCanopyIndex = 0;
                // set empty layer summaries to 0
                int layerCount = 0;
                if (LayeredBins != null)
                    layerCount = LayeredBins.Count();
                float[] layerWtTotalBio = new float[layerCount];
                float[] layerWtBio = new float[layerCount];
                float[] layerWtWoodBio = new float[layerCount];
                float[] layerWtRootBio = new float[layerCount];
                float[] layerWtFolBio = new float[layerCount];
                float[] layerWtNSC = new float[layerCount];
                float[] layerWtLAI = new float[layerCount];
                float[] layerWtNetPsn = new float[layerCount];
                float[] layerWtGrossPsn = new float[layerCount];
                float[] layerWtWoodSenescence = new float[layerCount];
                float[] layerWtFolSenescence = new float[layerCount];
                float[] layerWtMaintResp = new float[layerCount];
                float[] layerWtFolResp = new float[layerCount];
                float[] layerWtTranspiration = new float[layerCount];
                float[] layerSumBio = new float[layerCount];

                if (LayeredBins != null && LayeredBins.Count() > 0)
                {
                    for (int b = LayeredBins.Count() - 1; b >= 0; b--) // main canopy layers
                    {
                        float mainLayerPARweightedSum = 0;
                        float mainLayerLAIweightedSum = 0;
                        /*float mainLayerTotalBioWeightedSum = 0;
                        float mainLayerBioWeightedSum = 0;
                        float mainLayerWoodBioWeightedSum = 0;
                        float mainLayerRootBioWeightedSum = 0;
                        float mainLayerFolBioWeightedSum = 0;
                        float mainLayerNSCweightedSum = 0;
                        float mainLayerNetPsnWeightedSum = 0;
                        float mainLayerGrossPsnWeightedSum = 0;
                        float mainLayerWoodSenescenceWeightedSum = 0;
                        float mainLayerFolSenescenceWeightedSum = 0;
                        float mainLayerMaintRespWeightedSum = 0;
                        float mainLayerFolRespWeightedSum = 0;
                        float mainLayerTranspirationWeightedSum = 0;*/
                        float mainLayerPAR = subcanopypar;
                        float mainLayerBioSum = 0;
                        foreach (int r in random_range[b]) // sublayers within main canopy b
                        {
                            subCanopyIndex++;
                            int precipCount = 0;
                            subCanopyPrecip = 0;
                            subCanopyMelt = MeltInWater / SubCanopyCohorts.Count();
                            bool coldKillBoolean = false;
                            foreach (var g in groupList)
                            {
                                if (g.Key == subCanopyIndex)
                                {
                                    precipCount = g.Count();
                                    subCanopyPrecip = PrecInByEvent;
                                }
                            }
                            Cohort c = SubCanopyCohorts.Values.ToArray()[r];
                            ISpeciesPnET spc = c.SpeciesPnET;
                            // A cohort cannot be reduced to a lower layer once it reaches a higher layer
                            //c.Layer = (byte)Math.Max(b, c.Layer);
                            c.Layer = (byte)b;
                            if (coldKillMonth[spc] == m)
                                coldKillBoolean = true;
                            float O3Effect = lastOzoneEffect[subCanopyIndex - 1];

                            success = c.CalculatePhotosynthesis(subCanopyPrecip, precipCount, leakageFrac, ref hydrology, mainLayerPAR,
                                ref subcanopypar, O3_ppmh, O3_ppmh_month, subCanopyIndex, SubCanopyCohorts.Count(), ref O3Effect,
                                propRootAboveFrost, subCanopyMelt, coldKillBoolean, data[m], this.Ecoregion, this.Site.Location);

                            lastOzoneEffect[subCanopyIndex - 1] = O3Effect;

                            if (success == false)
                            {
                                throw new System.Exception("Error CalculatePhotosynthesis");
                            }
                        } // end sublayer loop in canopy b
                        int cCount = AllCohorts.Count();
                        foreach (Cohort c in AllCohorts)
                        {
                            if (c.Layer == b)
                            {
                                float PARFracUnderCohort = (float)Math.Exp(-c.SpeciesPnET.K * c.LAI.Sum());
                                mainLayerPARweightedSum += PARFracUnderCohort * c.Biomass;
                                mainLayerLAIweightedSum += c.LAI.Sum() * c.Biomass;
                                /*mainLayerWoodBioWeightedSum += c.Wood * c.Biomass;
                                mainLayerRootBioWeightedSum += c.Root * c.Biomass;
                                mainLayerFolBioWeightedSum += c.Fol * c.Biomass;
                                mainLayerNSCweightedSum += c.NSC * c.Biomass;
                                mainLayerNetPsnWeightedSum += c.NetPsn.Sum() * c.Biomass;
                                mainLayerGrossPsnWeightedSum += c.GrossPsn.Sum() * c.Biomass;
                                mainLayerWoodSenescenceWeightedSum += c.LastWoodySenescence * c.Biomass;
                                mainLayerFolSenescenceWeightedSum += c.LastFoliageSenescence * c.Biomass;
                                mainLayerFolRespWeightedSum += c.FolResp.Sum() * c.Biomass;
                                mainLayerMaintRespWeightedSum += c.MaintenanceRespiration.Sum() * c.Biomass;
                                mainLayerTranspirationWeightedSum += c.Transpiration.Sum() * c.Biomass;
                                mainLayerTotalBioWeightedSum += c.TotalBiomass * c.Biomass;
                                mainLayerBioWeightedSum += c.Biomass * c.Biomass;*/
                                mainLayerBioSum += c.Biomass;
                            }
                        }
                        layerSumBio[b] = mainLayerBioSum;
                        if (mainLayerBioSum > 0)
                        {
                            subcanopypar = mainLayerPAR * mainLayerPARweightedSum / mainLayerBioSum;
                            layerWtLAI[b] = mainLayerLAIweightedSum / mainLayerBioSum;
                            /*layerWtTotalBio[b] = mainLayerTotalBioWeightedSum / mainLayerBioSum;
                            layerWtBio[b] = mainLayerBioWeightedSum / mainLayerBioSum;
                            layerWtWoodBio[b] = mainLayerWoodBioWeightedSum / mainLayerBioSum;
                            layerWtRootBio[b] = mainLayerRootBioWeightedSum / mainLayerBioSum;
                            layerWtFolBio[b] = mainLayerFolBioWeightedSum / mainLayerBioSum;
                            layerWtNSC[b] = mainLayerNSCweightedSum / mainLayerBioSum;                            
                            layerWtNetPsn[b] = mainLayerNetPsnWeightedSum / mainLayerBioSum;
                            layerWtGrossPsn[b] = mainLayerGrossPsnWeightedSum / mainLayerBioSum;
                            layerWtWoodSenescence[b] = mainLayerWoodSenescenceWeightedSum / mainLayerBioSum;
                            layerWtFolSenescence[b] = mainLayerFolSenescenceWeightedSum / mainLayerBioSum;
                            layerWtMaintResp[b] = mainLayerMaintRespWeightedSum / mainLayerBioSum;
                            layerWtFolResp[b] = mainLayerFolRespWeightedSum / mainLayerBioSum;
                            layerWtTranspiration[b] = mainLayerTranspirationWeightedSum / mainLayerBioSum;
                            */
                        }
                        else
                            subcanopypar = mainLayerPAR;
                    } // end main canopy layer loop
                }
                else // When no cohorts are present
                {
                    if (MeltInWater > 0)
                    {
                        // Add melted snow to soil moisture
                        // Instantaneous runoff (excess of porosity)
                        float waterCapacity = Ecoregion.Porosity * Ecoregion.RootingDepth * propRootAboveFrost; //mm
                        float meltrunoff = Math.Min(MeltInWater, Math.Max(hydrology.Water * Ecoregion.RootingDepth * propRootAboveFrost + MeltInWater - waterCapacity, 0));

                        //if ((hydrology.Water + meltrunoff) > (Ecoregion.Porosity + Ecoregion.RunoffCapture))
                        //    meltrunoff = (hydrology.Water + meltrunoff) - (Ecoregion.Porosity + Ecoregion.RunoffCapture);
                        hydrology.RunOff += meltrunoff;

                        success = hydrology.AddWater(MeltInWater - meltrunoff, Ecoregion.RootingDepth * propRootAboveFrost);
                        if (success == false) throw new System.Exception("Error adding water, MeltInWaterr = " + MeltInWater + "; water = " + hydrology.Water + "; meltrunoff = " + meltrunoff + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                        float capturedRunoff = 0;
                        if ((Ecoregion.RunoffCapture > 0) & (meltrunoff > 0))
                        {
                            capturedRunoff = Math.Max(0, Math.Min(meltrunoff, (Ecoregion.RunoffCapture - hydrology.SurfaceWater)));
                            hydrology.SurfaceWater += capturedRunoff;
                        }
                        hydrology.RunOff += (meltrunoff - capturedRunoff);
                    }
                    if (precin > 0)
                    {
                        for (int p = 0; p < numEvents; p++)
                        {
                            // Instantaneous runoff (excess of porosity)
                            float waterCapacity = Ecoregion.Porosity * Ecoregion.RootingDepth * propRootAboveFrost; //mm
                            float rainrunoff = Math.Min(precin, Math.Max(hydrology.Water * Ecoregion.RootingDepth * propRootAboveFrost + PrecInByEvent - waterCapacity, 0));
                            //if ((hydrology.Water + rainrunoff) > (ecoregion.Porosity + ecoregion.RunoffCapture))
                            //    rainrunoff = (hydrology.Water + rainrunoff) - (ecoregion.Porosity + ecoregion.RunoffCapture);
                            float capturedRunoff = 0;
                            if ((Ecoregion.RunoffCapture > 0) & (rainrunoff > 0))
                            {
                                capturedRunoff = Math.Max(0, Math.Min(rainrunoff, (Ecoregion.RunoffCapture - hydrology.SurfaceWater)));
                                hydrology.SurfaceWater += capturedRunoff;
                            }
                            hydrology.RunOff += (rainrunoff - capturedRunoff);

                            float precipIn = PrecInByEvent - rainrunoff; //mm

                            // Add incoming precipitation to soil moisture
                            success = hydrology.AddWater(precipIn, Ecoregion.RootingDepth * propRootAboveFrost);
                            if (success == false) throw new System.Exception("Error adding water, waterIn = " + precipIn + "; water = " + hydrology.Water + "; rainrunoff = " + rainrunoff + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                            // Fast Leakage
                            float leakage = Math.Max((float)leakageFrac * (hydrology.Water - Ecoregion.FieldCap), 0) * Ecoregion.RootingDepth * propRootAboveFrost; //mm
                            hydrology.Leakage += leakage;

                            // Remove fast leakage
                            success = hydrology.AddWater(-1 * leakage, Ecoregion.RootingDepth * propRootAboveFrost);
                            if (success == false) throw new System.Exception("Error adding water, Hydrology.Leakage = " + hydrology.Leakage + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                            // Add surface water to soil
                            if (hydrology.SurfaceWater > 0)
                            {
                                float surfaceInput = Math.Min(hydrology.SurfaceWater, (Ecoregion.Porosity - hydrology.Water));
                                hydrology.SurfaceWater -= surfaceInput;
                                success = hydrology.AddWater(surfaceInput, Ecoregion.RootingDepth * propRootAboveFrost);
                                if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                            }
                        }
                    }
                    else
                    {
                        if (MeltInWater > 0)
                        {
                            // Add surface water to soil
                            if (hydrology.SurfaceWater > 0)
                            {
                                float surfaceInput = Math.Min(hydrology.SurfaceWater, (Ecoregion.Porosity - hydrology.Water));
                                hydrology.SurfaceWater -= surfaceInput;
                                success = hydrology.AddWater(surfaceInput, Ecoregion.RootingDepth * propRootAboveFrost);
                                if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                            }
                        }
                    }
                }
                int cohortCount = AllCohorts.Count();
                CanopyLAI = new float[MaxCanopyLayers];
                float[] CanopyLAISum = new float[MaxCanopyLayers];
                float[] CanopyLAICount = new float[MaxCanopyLayers];
                float[] CanopyAlbedo = new float[MaxCanopyLayers];
                float[] LayerLAI = new float[MaxCanopyLayers];
                CumulativeLeafAreas leafAreas = new CumulativeLeafAreas();

                foreach (Cohort cohort in AllCohorts)
                {
                    folresp[data[m].Month - 1] += cohort.FolResp.Sum();
                    netpsn[data[m].Month - 1] += cohort.NetPsn.Sum();
                    grosspsn[data[m].Month - 1] += cohort.GrossPsn.Sum();
                    maintresp[data[m].Month - 1] += cohort.MaintenanceRespiration.Sum();
                    transpiration += cohort.Transpiration.Sum();
                    CalculateCumulativeLeafArea(ref leafAreas, cohort);
                    
                    int layer = cohort.Layer;
                    if (layer < CanopyLAISum.Length)
                    {
                        CanopyLAISum[layer] += (cohort.LAI.Sum() * cohort.Biomass);
                        CanopyLAICount[layer] += cohort.Biomass;
                        //MaxLAI[layer] = Math.Max(MaxLAI[layer], cohort.SpeciesPNET.MaxLAI);
                    }
                    //else
                    //{
                    //    Globals.ModelCore.UI.WriteLine("DEBUG: Cohort count = " + AllCohorts.Count() + "; CanopyLAISum count = " + CanopyLAISum.Count());
                    //}
                    else
                    {
                        Globals.ModelCore.UI.WriteLine("DEBUG: Cohort count = " + AllCohorts.Count() + "; CanopyLAISum count = " + CanopyLAISum.Count());
                    }

                    CanopyAlbedo[layer] += CalculateAlbedoWithSnow(cohort, cohort.Albedo, sno_dep) * cohort.BiomassLayerProp;
                    LayerLAI[layer] += cohort.SumLAI * cohort.BiomassLayerProp;
                }

                for (int layer = 0; layer < MaxCanopyLayers; layer++)
                {
                    if (CanopyLAICount[layer] > 0)
                        CanopyLAI[layer] = CanopyLAISum[layer] / CanopyLAICount[layer];
                    else
                        CanopyLAI[layer] = 0;

                    averageAlbedo[data[m].Month - 1] += CanopyAlbedo[layer] > 0 ? CanopyAlbedo[layer] * LayerLAI[layer] : 0;
                }

                averageAlbedo[data[m].Month - 1] /= LayerLAI.Sum() > 0 ? LayerLAI.Sum() : 1;

                foreach (Cohort cohort in AllCohorts)
                {
                    int b = cohort.Layer;
                    cohort.BiomassLayerProp = cohort.Biomass / layerSumBio[b];
                    int c = cohort.Layer;
                }

                folresp[data[m].Month - 1] = layerWtFolResp.Sum();
                netpsn[data[m].Month - 1] = layerWtNetPsn.Sum();
                grosspsn[data[m].Month - 1] = layerWtGrossPsn.Sum();
                maintresp[data[m].Month - 1] = layerWtMaintResp.Sum();
                transpiration = layerWtTranspiration.Sum();
                for (int layer = 0; layer < MaxCanopyLayers; layer++)
                {
                    if (layer < layerWtLAI.Length)
                        CanopyLAI[layer] = layerWtLAI[layer];
                    else
                        CanopyLAI[layer] = 0;
                }


                // Calculate establishment probability
                if (Globals.ModelCore.CurrentTime > 0)
                {
                    establishmentProbability.Calculate_Establishment_Month(data[m], Ecoregion, subcanopypar, hydrology, minHalfSat, maxHalfSat, invertPest);

                    foreach (ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
                    {
                        if (annualFwater.ContainsKey(spc))
                        {
                            if (data[m].Tmin > spc.PsnTMin && data[m].Tmax < spc.PsnTMax) // Active growing season
                            {
                                // Store monthly values for later averaging
                                //annualEstab[spc].Add(monthlyEstab[spc]);
                                annualFwater[spc].Add(establishmentProbability.Get_FWater(spc));
                                annualFrad[spc].Add(establishmentProbability.Get_FRad(spc));
                            }
                        }
                    }
                }

                // Soil water Evaporation
                // Surface PAR is effectively 0 when snowpack is present
                if (snowPack > 0)
                    subcanopypar = 0;

                canopylaimax = (float)Math.Max(canopylaimax, CanopyLAI.Sum());
                subcanopyparmax = Math.Max(subcanopyparmax, subcanopypar);
                if (propRootAboveFrost > 0 && snowPack == 0)
                {
                    hydrology.Evaporation = hydrology.CalculateEvaporation(this, data[m]); //mm
                }
                else
                {
                    hydrology.Evaporation = 0;
                    hydrology.PET = 0;
                }
                success = hydrology.AddWater(-1 * hydrology.Evaporation, Ecoregion.RootingDepth * propRootAboveFrost);
                if (success == false)
                {
                    throw new System.Exception("Error adding water, evaporation = " + hydrology.Evaporation + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                }
                // Add surface water to soil
                if ((hydrology.SurfaceWater > 0) & (hydrology.Water < Ecoregion.Porosity))
                {
                    float surfaceInput = Math.Min(hydrology.SurfaceWater, ((Ecoregion.Porosity - hydrology.Water) * Ecoregion.RootingDepth * propRootAboveFrost));
                    hydrology.SurfaceWater -= surfaceInput;
                    success = hydrology.AddWater(surfaceInput, Ecoregion.RootingDepth * propRootAboveFrost);
                    if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                }

                if (siteoutput != null)
                {
                    AddSiteOutput(data[m]);

                    AllCohorts.ForEach(a => a.UpdateCohortData(data[m]));
                }
                if (data[m].Tave > 0)
                {
                    sumPressureHead += hydrology.GetPressureHead(Ecoregion);
                    countPressureHead += 1;
                }

                // Store growing season FRad values                
                AllCohorts.ForEach(x => x.StoreFRad());
                // Reset all cohort values
                AllCohorts.ForEach(x => x.NullSubLayers());

                //  Processes that happen only once per year
                if (data[m].Month == (int)Constants.Months.December)
                {
                    //  Decompose litter
                    HeterotrophicRespiration = (ushort)(SiteVars.Litter[Site].Decompose() + SiteVars.WoodyDebris[Site].Decompose());

                    // Calculate AdjFolFrac
                    AllCohorts.ForEach(x => x.CalcAdjFracFol());

                    // Filter monthly pest values
                    // This assumes up to 3 months of growing season are relevant for establishment
                    // When > 3 months of growing season, exlcude 1st month, assuming trees focus on foliage growth in first month
                    // When > 4 months, ignore the 4th month and beyond as not primarily relevant for establishment
                    // When < 3 months, include all months
                    foreach (ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
                    {
                        if (annualFwater[spc].Count > 3)
                        {
                            //cumulativeEstab[spc] = cumulativeEstab[spc] * (1 - annualEstab[spc][1]) * (1 - annualEstab[spc][2]) * (1 - annualEstab[spc][3]);
                            cumulativeFwater[spc] = cumulativeFwater[spc] + annualFwater[spc][1] + annualFwater[spc][2] + annualFwater[spc][3];
                            cumulativeFrad[spc] = cumulativeFrad[spc] + annualFrad[spc][1] + annualFrad[spc][2] + annualFrad[spc][3];
                            monthlyCount[spc] = monthlyCount[spc] + 3;
                        }
                        else if (annualFwater[spc].Count > 2)
                        {
                            //cumulativeEstab[spc] = cumulativeEstab[spc] * (1 - annualEstab[spc][0]) * (1 - annualEstab[spc][1]) * (1 - annualEstab[spc][2]) ;
                            cumulativeFwater[spc] = cumulativeFwater[spc] + annualFwater[spc][0] + annualFwater[spc][1] + annualFwater[spc][2];
                            cumulativeFrad[spc] = cumulativeFrad[spc] + annualFrad[spc][0] + annualFrad[spc][1] + annualFrad[spc][2];
                            monthlyCount[spc] = monthlyCount[spc] + 3;
                        }
                        else if (annualFwater[spc].Count > 1)
                        {
                            //cumulativeEstab[spc] = cumulativeEstab[spc] * (1 - annualEstab[spc][0]) * (1 - annualEstab[spc][1]);
                            cumulativeFwater[spc] = cumulativeFwater[spc] + annualFwater[spc][0] + annualFwater[spc][1];
                            cumulativeFrad[spc] = cumulativeFrad[spc] + annualFrad[spc][0] + annualFrad[spc][1];
                            monthlyCount[spc] = monthlyCount[spc] + 2;
                        }
                        else if (annualFwater[spc].Count == 1)
                        {
                            //cumulativeEstab[spc] = cumulativeEstab[spc] * (1 - annualEstab[spc][0]);
                            cumulativeFwater[spc] = cumulativeFwater[spc] + annualFwater[spc][0];
                            cumulativeFrad[spc] = cumulativeFrad[spc] + annualFrad[spc][0];
                            monthlyCount[spc] = monthlyCount[spc] + 1;
                        }

                        //Reset annual lists for next year
                        //annualEstab[spc].Clear();
                        annualFwater[spc].Clear();
                        annualFrad[spc].Clear();
                    } //foreach (ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
                } //if (data[m].Month == (int)Constants.Months.December)

                wateravg += hydrology.Water;
            } //for (int m = 0; m < data.Count(); m++ )
            // Above is monthly loop                           
            // Below runs once per timestep
            wateravg = wateravg / data.Count(); // convert to average value
            if (Globals.ModelCore.CurrentTime > 0)
            {
                foreach (ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
                {
                    bool estab = false;
                    float pest = 0;
                    if (monthlyCount[spc] > 0)
                    {
                        //annualEstab[spc] = annualEstab[spc] / monthlyCount[spc];
                        // Transform cumulative probability of no successful establishments to probability of at least one successful establishment
                        //cumulativeEstab[spc] = 1 - cumulativeEstab[spc] ;
                        cumulativeFwater[spc] = cumulativeFwater[spc] / monthlyCount[spc];
                        cumulativeFrad[spc] = cumulativeFrad[spc] / monthlyCount[spc];

                        // Modify Pest by maximum value
                        //pest = cumulativeEstab[spc] * spc.MaxPest;

                        // Calculate Pest from average Fwater, Frad and modified by MaxPest
                        pest = cumulativeFwater[spc] * cumulativeFrad[spc] * spc.MaxPest;
                    }
                    
                    if (!spc.PreventEstablishment)
                    {

                        if (pest > (float)Statistics.ContinuousUniformRandom())
                        {
                            establishmentProbability.EstablishmentTrue(spc);
                            estab = true;

                        }
                    }
                    EstablishmentProbability.RecordPest(Globals.ModelCore.CurrentTime, spc, pest, cumulativeFwater[spc], cumulativeFrad[spc], estab, monthlyCount[spc]);

                }
            }

            if (siteoutput != null)
            {
                siteoutput.Write();

                AllCohorts.ForEach(cohort => { cohort.WriteCohortData(); });
            }
            float avgPH = sumPressureHead / countPressureHead;
            SiteVars.PressureHead[Site] = avgPH;
            
            if((Globals.ModelCore.CurrentTime > 0) || AllowMortality)
                RemoveMarkedCohorts();

            //HeterotrophicRespiration = (ushort)(PlugIn.Litter[Site].Decompose() + PlugIn.WoodyDebris[Site].Decompose());//Moved within m loop to trigger once per year

            return success;
        }

        private float CalculateAverageAlbedo(CumulativeLeafAreas leafAreas, float snowDepth)
        {

            if (!Globals.ModelCore.Ecoregion[this.Site].Active)
            {
                return -1;
            }

            float snowMultiplier = snowDepth >= Globals.snowReflectanceThreshold ? 1 : snowDepth / Globals.snowReflectanceThreshold;

            float darkConiferAlbedo = (float)((-0.067 * Math.Log(leafAreas.DarkConifer < 0.7 ? 0.7 : leafAreas.DarkConifer)) + 0.2095);
            darkConiferAlbedo = (float)(darkConiferAlbedo + (darkConiferAlbedo * (0.8 * snowMultiplier)));

            float lightConiferAlbedo = (float)((-0.054 * Math.Log(leafAreas.LightConifer < 0.7 ? 0.7 : leafAreas.LightConifer)) + 0.2082);
            lightConiferAlbedo = (float)(lightConiferAlbedo + (lightConiferAlbedo * (0.75 * snowMultiplier)));

            float deciduousAlbedo = (float)((-0.0073 * leafAreas.Deciduous) + 0.231);
            deciduousAlbedo = (float)(deciduousAlbedo + (deciduousAlbedo * (0.35 * snowMultiplier)));

            float grassMossOpenAlbedo = 0.2F;
            grassMossOpenAlbedo = (float)(grassMossOpenAlbedo + (grassMossOpenAlbedo * (3.75 * snowMultiplier)));

            // Set Albedo values to 0 if they are negative
            darkConiferAlbedo = darkConiferAlbedo >= 0 ? darkConiferAlbedo : 0;
            lightConiferAlbedo = lightConiferAlbedo >= 0 ? lightConiferAlbedo : 0;
            deciduousAlbedo = deciduousAlbedo >= 0 ? deciduousAlbedo : 0;
            grassMossOpenAlbedo = grassMossOpenAlbedo >= 0 ? grassMossOpenAlbedo : 0;

            if (leafAreas.DarkConiferProportion + leafAreas.LightConiferProportion + leafAreas.DeciduousProportion + leafAreas.GrassMossOpenProportion == 0)
            {
                return 0;
            }

            return ((darkConiferAlbedo * leafAreas.DarkConiferProportion) + (lightConiferAlbedo * leafAreas.LightConiferProportion)
                + (deciduousAlbedo * leafAreas.DeciduousProportion) + (grassMossOpenAlbedo * leafAreas.GrassMossOpenProportion))
                / (leafAreas.DarkConiferProportion + leafAreas.LightConiferProportion + leafAreas.DeciduousProportion + leafAreas.GrassMossOpenProportion);
        }

        // Does the final bits of Albedo calculation by adding snow consideration in
        private float CalculateAlbedoWithSnow(Cohort cohort, float albedo, float snowDepth)
        {
            // Inactive sites become large negative values on the map and are not considered in the averages
            if (!EcoregionData.GetPnETEcoregion(Globals.ModelCore.Ecoregion[this.Site]).Active)
            {
                return -1;
            }

            float snowMultiplier = snowDepth >= Globals.snowReflectanceThreshold ? 1 : snowDepth / Globals.snowReflectanceThreshold;

            if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && (cohort.SpeciesPnET.Lifeform.ToLower().Contains("ground")
                        || cohort.SpeciesPnET.Lifeform.ToLower().Contains("open")
                        || cohort.SumLAI == 0))
            {
                return (float)(albedo + (albedo * (3.125 * snowMultiplier)));
            }
            else if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && cohort.SpeciesPnET.Lifeform.ToLower().Contains("dark"))
            {
                return (float)(albedo + (albedo * (0.8 * snowMultiplier)));
            }
            else if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && cohort.SpeciesPnET.Lifeform.ToLower().Contains("light"))
            {
                return (float)(albedo + (albedo * (0.75 * snowMultiplier)));
            }
            else if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && cohort.SpeciesPnET.Lifeform.ToLower().Contains("decid"))
            {
                return (float)(albedo + (albedo * (0.35 * snowMultiplier)));
            }

            return 0;
        }

        private void CalculateCumulativeLeafArea(ref CumulativeLeafAreas leafAreas, Cohort cohort)
        {
            if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && cohort.SpeciesPnET.Lifeform.ToLower().Contains("dark"))
            {
                leafAreas.DarkConifer += cohort.SumLAI;
            }
            else if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && cohort.SpeciesPnET.Lifeform.ToLower().Contains("light"))
            {
                leafAreas.LightConifer += cohort.SumLAI;
            }
            else if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && cohort.SpeciesPnET.Lifeform.ToLower().Contains("decid"))
            {
                leafAreas.Deciduous += cohort.SumLAI;
            }
            else if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && (cohort.SpeciesPnET.Lifeform.ToLower().Contains("ground")
                        || cohort.SpeciesPnET.Lifeform.ToLower().Contains("open")))
            {
                leafAreas.GrassMossOpen += cohort.SumLAI;
            }
            else if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && (cohort.SpeciesPnET.Lifeform.ToLower().Contains("tree")
                        || cohort.SpeciesPnET.Lifeform.ToLower().Contains("shrub")))
            {
                leafAreas.Deciduous += cohort.SumLAI;
            }
        }

        private void CalculateInitialWater(DateTime StartDate)
        {
            IEcoregionPnETVariables variables = null;
            canopylaimax = float.MinValue;

            SortedDictionary<double, Cohort> SubCanopyCohorts = new SortedDictionary<double, Cohort>();
            List<double> CohortBiomassList = new List<double>();
            List<double> CohortMaxBiomassList = new List<double>();
            int SiteAboveGroundBiomass = AllCohorts.Sum(a => a.Biomass);
            int MaxLayer = 0;
            for (int cohort = 0; cohort < AllCohorts.Count(); cohort++)
            {
                if (Globals.ModelCore.CurrentTime > 0)
                {
                    AllCohorts[cohort].CalculateDefoliation(Site, SiteAboveGroundBiomass);
                }

                CohortBiomassList.Add(AllCohorts[cohort].TotalBiomass);
                CohortMaxBiomassList.Add(AllCohorts[cohort].BiomassMax);
            }

            //List<List<int>> rawBins = GetBins(new List<double>(SubCanopyCohorts.Keys));
            /*//Debug
            if (Globals.ModelCore.CurrentTime == 10 && this.Site.Location.Row == 188 && this.Site.Location.Column == 22 && CohortBiomassList.Count() == 9)
            {
                Globals.ModelCore.UI.WriteLine("AllCohorts = ");
                foreach (Cohort c in AllCohorts)
                {
                    Globals.ModelCore.UI.WriteLine("Species = ", c.Species.Name, "; Age = ", c.Age.ToString(), "; Biomass = ", c.Biomass.ToString(), "; Layer = ", c.Layer.ToString());
                }

                Globals.ModelCore.UI.WriteLine("CohortBiomassList = ");
                foreach (double cohortBio in CohortBiomassList)
                {
                    Globals.ModelCore.UI.WriteLine(cohortBio.ToString());
                }
            }*/
            ratioAbove10.Clear();
            List<List<double>> cohortBins = GetBinsByCohort(CohortMaxBiomassList);

            List<int> cohortAges = new List<int>();
            List<List<int>> rawBins = new List<List<int>>();
            int subLayerIndex = 0;
            bool reducedLayer = false;
            for (int cohort = 0; cohort < AllCohorts.Count(); cohort++)
            {
                string lifeForm = AllCohorts[cohort].SpeciesPnET.Lifeform.ToLower();
                int cohortLayer = 0;
                // Lifeform "ground" always restricted to layer 0
                if (!lifeForm.Contains("ground"))
                {
                    for (int j = 0; j < cohortBins.Count(); j++)
                    {
                        if (cohortBins[j].Contains(AllCohorts[cohort].BiomassMax))
                            cohortLayer = j;
                    }
                    //if ((AllCohorts[cohort].Layer > cohortLayer) && (!string.IsNullOrEmpty(lifeForm))
                    //    && (lifeForm.Contains("tree") || lifeForm.Contains("shrub"))) 
                    // {
                    //     reducedLayer = true;
                    // }

                    if (AllCohorts[cohort].Layer > MaxLayer)
                        MaxLayer = AllCohorts[cohort].Layer;
                }
                for (int i = 1; i <= Globals.IMAX; i++)
                {
                    //double CumCohortBiomass = ((float)i / (float)PlugIn.IMAX) * AllCohorts[cohort].TotalBiomass;
                    /*double CumCohortBiomass = (1f / (float)PlugIn.IMAX) * AllCohorts[cohort].TotalBiomass;
                    while (SubCanopyCohorts.ContainsKey(CumCohortBiomass))
                    {
                        // Add a negligable value [-1e-10; + 1e-10] to CumCohortBiomass in order to prevent duplicate keys
                        double k = 1e-10 * 2.0 * (PlugIn.ContinuousUniformRandom() - 0.5);
                        CumCohortBiomass += k;
                    }
                    SubCanopyCohorts.Add(CumCohortBiomass, AllCohorts[cohort]);*/
                    SubCanopyCohorts.Add(subLayerIndex, AllCohorts[cohort]);
                    while (rawBins.Count() < (cohortLayer + 1))
                    {
                        List<int> subList = new List<int>();
                        //subList.Add(subLayerIndex);
                        rawBins.Add(subList);
                    }
                    //else
                    rawBins[cohortLayer].Add(subLayerIndex);
                    subLayerIndex++;
                }
                if (!cohortAges.Contains(AllCohorts[cohort].Age))
                {
                    cohortAges.Add(AllCohorts[cohort].Age);
                }
            }

            List<List<int>> LayeredBins = new List<List<int>>();
            /*if ((rawBins.Count > 0) && (reducedLayer)) // cohort(s) were previously in a higher layer
            {
                double maxCohortBiomass = CohortBiomassList.Max();
                for (int i = 0; i < rawBins.Count(); i++)
                {
                    List<int> binLayers = rawBins[i];
                    for (int b = 0; b < binLayers.Count(); b++)
                    {
                        int layerKey = binLayers[b];
                        int canopyIndex = i;
                        Cohort layerCohort = SubCanopyCohorts.Values.ToArray()[layerKey];
                        double cohortBio = layerCohort.TotalBiomass;
                        //bool highRatio = ((maxCohortBiomass / cohortBio) > 10.0);
                        //if(layerCohort.Layer > i && !highRatio)
                        if (layerCohort.Layer > i)
                        {
                            canopyIndex = layerCohort.Layer;
                        }
                        if (LayeredBins.ElementAtOrDefault(canopyIndex) == null)
                        {
                            while (LayeredBins.ElementAtOrDefault(canopyIndex) == null)
                            {
                                LayeredBins.Add(new List<int>());
                            }
                        }
                        LayeredBins[canopyIndex].Add(layerKey);
                    }
                }
            }
            else
            {*/
                LayeredBins = rawBins;
            //}
            nlayers = 0;
            foreach (List<int> layerList in LayeredBins)
            {
                if (layerList.Count > 0)
                    nlayers++;
            }
            MaxLayer = LayeredBins.Count - 1;

            List<List<int>> random_range = GetRandomRange(LayeredBins);

            List<IEcoregionPnETVariables> climate_vars = EcoregionData.GetData(Ecoregion, StartDate, StartDate.AddMonths(1));

            if (climate_vars != null && climate_vars.Count > 0)
            {
                Ecoregion.Variables = climate_vars.First();
                variables = climate_vars.First();
            }
            else
            {
                return;
            }
            transpiration = 0;
            subcanopypar = variables.PAR0;
            interception = 0;

            AllCohorts.ForEach(x => x.InitializeSubLayers());

            if (variables.Prec < 0) throw new System.Exception("Error, Ecoregion.Variables.Prec = " + variables.Prec);

            float snowmelt = Math.Min(snowPack, ComputeMaxSnowMelt(variables.Tave, variables.DaySpan)); // mm
            if (snowmelt < 0) throw new System.Exception("Error, snowmelt = " + snowmelt);

            float newsnow = CumputeSnowFraction(variables.Tave) * variables.Prec;
            float newsnowpack = newsnow * (1 - Ecoregion.SnowSublimFrac); // (mm) Account for sublimation here
            if (newsnowpack < 0 || newsnowpack > variables.Prec)
            {
                throw new System.Exception("Error, newsnowpack = " + newsnowpack + " availablePrecipitation = " + variables.Prec);
            }

            snowPack += newsnowpack - snowmelt;
            if (snowPack < 0) throw new System.Exception("Error, snowPack = " + snowPack);

            float newrain = variables.Prec - newsnow;

            // Reduced by interception
            interception = newrain * (float)(1 - Math.Exp(-1 * Ecoregion.PrecIntConst * CanopyLAI.Sum()));
            float surfaceRain = newrain - interception;

            // Reduced by PrecLossFrac
            precLoss = surfaceRain * Ecoregion.PrecLossFrac;
            float availableRain = surfaceRain - precLoss;

            float precin = availableRain + snowmelt;
            if (precin < 0) throw new System.Exception("Error, precin = " + precin + " newsnow = " + newsnow + " snowmelt = " + snowmelt);

            int numEvents = Ecoregion.PrecipEvents;  // maximum number of precipitation events per month
            float PrecInByEvent = precin / numEvents;  // Divide precip into discreet events within the month
            if (PrecInByEvent < 0) throw new System.Exception("Error, PrecInByEvent = " + PrecInByEvent);

            // Randomly choose which layers will receive the precip events
            // If # of layers < precipEvents, some layers will show up multiple times in number list.  This ensures the same number of precip events regardless of the number of cohorts
            List<int> randomNumbers = new List<int>();
            if (PrecipEventsWithReplacement)// Sublayer selection with replacement
            {
                while (randomNumbers.Count < numEvents)
                {
                    int rand = Statistics.DiscreteUniformRandom(1, SubCanopyCohorts.Count());
                    randomNumbers.Add(rand);
                }
            }
            else // Sublayer selection without replacement
            {
                while (randomNumbers.Count < numEvents)
                {
                    List<int> subCanopyList = Enumerable.Range(1, SubCanopyCohorts.Count()).ToList();
                    while ((randomNumbers.Count < numEvents) && (subCanopyList.Count() > 0))
                    {
                        int rand = Statistics.DiscreteUniformRandom(1, subCanopyList.Count());
                        randomNumbers.Add(subCanopyList[rand]);
                        subCanopyList.RemoveAt(rand);
                    }
                }
            }
            var groupList = randomNumbers.GroupBy(i => i);

            // Reset Hydrology values
            hydrology.RunOff = 0;
            hydrology.Leakage = 0;
            hydrology.Evaporation = 0;

            float subCanopyPrecip = 0;
            int subCanopyIndex = 0;
            if (LayeredBins != null)
            {
                for (int b = LayeredBins.Count() - 1; b >= 0; b--)
                {
                    foreach (int r in random_range[b])
                    {
                        subCanopyIndex++;
                        int precipCount = 0;
                        subCanopyPrecip = 0;
                        foreach (var g in groupList)
                        {
                            if (g.Key == subCanopyIndex)
                            {
                                precipCount = g.Count();
                                subCanopyPrecip = PrecInByEvent;
                            }
                        }
                        Cohort c = SubCanopyCohorts.Values.ToArray()[r];
                        ISpeciesPnET spc = c.SpeciesPnET;

                        // A cohort cannot be reduced to a lower layer once it reaches a higher layer
                        //if (c.Layer > bins.Count())
                        //    c.Layer = (byte)bins.Count();
                        //c.Layer = (byte)Math.Max(b, c.Layer);
                        c.Layer = (byte)b;
                    }
                }
            }
            else // When no cohorts are present
            {
                return;
            }

            // Surface PAR is effectively 0 when snowpack is present
            if (snowPack > 0)
                subcanopypar = 0;

            canopylaimax = (float)Math.Max(canopylaimax, CanopyLAI.Sum());
            wateravg = hydrology.Water;
            subcanopyparmax = Math.Max(subcanopyparmax, subcanopypar);

            if (propRootAboveFrost > 0 && snowPack == 0)
            {
                hydrology.Evaporation = hydrology.CalculateEvaporation(this, variables); //mm
            }
            else
            {
                hydrology.Evaporation = 0;
                hydrology.PET = 0;
            }
            bool success = hydrology.AddWater(-1 * hydrology.Evaporation, Ecoregion.RootingDepth * propRootAboveFrost);
            if (success == false)
            {
                throw new System.Exception("Error adding water, evaporation = " + hydrology.Evaporation + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
            }
            if (hydrology.SurfaceWater > 0)
            {
                float surfaceInput = Math.Min(hydrology.SurfaceWater, (Ecoregion.Porosity - hydrology.Water));
                hydrology.SurfaceWater -= surfaceInput;
                success = hydrology.AddWater(surfaceInput, Ecoregion.RootingDepth * propRootAboveFrost);
                if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
            }
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
                {
                    return maintresp.Select(r => (float)r).ToArray();
                }
            }
        }

        public float[] FolResp
        {
            get
            {
                if (folresp == null)
                {
                    float[] folresp_array = new float[12];
                    for (int i = 0; i < folresp_array.Length; i++)
                    {
                        folresp_array[i] = 0;
                    }
                    return folresp_array;
                }
                else
                {
                    return folresp.Select(psn => (float)psn).ToArray();
                }
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
                {
                    return grosspsn.Select(psn => (float)psn).ToArray();
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
                        averageAlbedo_array[i] = 0;
                    }
                    return averageAlbedo_array;
                }
                else
                {
                    return averageAlbedo.Select(r => (float)r).ToArray();
                }
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
                {
                    return netpsn.Select(psn => (float)psn).ToArray().Sum();
                }
            }
        }
        public float CanopyLAImax
        {
            get
            {
                return canopylaimax;
            }
        }

        public double WoodyDebris 
        {
            get
            {
                return SiteVars.WoodyDebris[Site].Mass;
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
                    BiomassPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.TotalBiomass * o.BiomassLayerProp));
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
                    AbovegroundBiomassPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.Biomass * o.BiomassLayerProp));
                }
                return AbovegroundBiomassPerSpecies;
            }
        }
        public Landis.Library.Parameters.Species.AuxParm<int> WoodySenescencePerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> WoodySenescencePerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    WoodySenescencePerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.LastWoodySenescence * o.BiomassLayerProp));
                }
                return WoodySenescencePerSpecies;
            }
        }
        public Landis.Library.Parameters.Species.AuxParm<int> FoliageSenescencePerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> FoliageSenescencePerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    FoliageSenescencePerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.LastFoliageSenescence * o.BiomassLayerProp));
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
                return AllCohorts.Sum(o => (o.TotalBiomass * o.BiomassLayerProp));
            }

        }
        public float AbovegroundBiomassSum
        {
            get
            {
                return AllCohorts.Sum(o => (o.Biomass * o.BiomassLayerProp));
            }

        }
        public float WoodySenescenceSum
        {
            get
            {
                return AllCohorts.Sum(o => (o.LastWoodySenescence * o.BiomassLayerProp));
            }

        }
        public float FoliageSenescenceSum
        {
            get
            {
                return AllCohorts.Sum(o => (o.LastFoliageSenescence * o.BiomassLayerProp));
            }

        }
        public float BelowGroundBiomassSum 
        {
            get
            {
                return AllCohorts.Sum(o =>(o.Root * o.BiomassLayerProp));
                //return (uint)cohorts.Values.Sum(o => o.Sum(x => x.Root));
            }
        }

        public float FoliageSum
        {
            get
            {
                return AllCohorts.Sum(o => (o.Fol * o.BiomassLayerProp));
            }

        }

        public float NSCSum
        {
            get
            {
                return AllCohorts.Sum(o => (o.NSC * o.BiomassLayerProp));
            }

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

        public float AETSum
        {
            get
            {
                return AET.Sum();
            }
        }
        class SubCanopyComparer : IComparer<int[]>
        {
            // Compare second int (cumulative cohort biomass)
            public int Compare(int[] x, int[] y)
            {
                return (x[0] > y[0])? 1:-1;
            }
        }

        private SortedDictionary<int[], Cohort> GetSubcanopyLayers()
        {
            SortedDictionary<int[], Cohort> subcanopylayers = new SortedDictionary<int[], Cohort>(new SubCanopyComparer());

            foreach (Cohort cohort in AllCohorts)
            {
                for (int i = 0; i < Globals.IMAX; i++)
                {
                    int[] subcanopylayer = new int[] { (ushort)((i + 1) / (float)Globals.IMAX * cohort.BiomassMax) };
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
                    {
                        index_in[index]++;

                        for (int i = index+1; i < index_in.Length; i++)
                        {
                            index_in[i] = index_in[i - 1] + 1;
                        }
                        return index_in;
                    }

                }
                /*
                else 
                {
                    if (index == 0) return null;

                    index_in[index - 1]++;
 
                    for (int i = index; i < index_in.Length; i++)
                    {
                        index_in[i] = index_in[index - 1] + i;
                    }
                     
                }
                 */
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

        uint CalculateLayerMaxDev(List<double> f)
        {
            return (uint)Math.Max(Math.Abs(f.Max() - f.Average()), Math.Abs(f.Min() - f.Average()));
        }

        int[] MinMaxCohortNr(int[] Bin, int i, int Count)
        {
            int min = (i > 0) ? Bin[i - 1] : 0;
            int max = (i < Bin.Count()) ? Bin[i] : Count - 1;

            return new int[] { min, max };
        }

        //private List<uint> layermaxdev = new List<uint>();

        /*private List<List<int>> GetBins(List<double> CumSublayerBiomass)
        {
            nlayers = 0;
            layermaxdev.Clear();
            if (CumSublayerBiomass.Count() == 0)
            {                
                return null;
            }

            // Bin and BestBin are lists of indexes that determine what cohort belongs to what canopy layer, 
            // i.e. when Bin[1] contains 45 then SubCanopyCohorts[45] is in layer 1
            int[] BestBin = null;
            int[] Bin = null;
                       

            float LayerMaxDev = float.MaxValue;

            //=====================OPTIMIZATION LOOP====================================
            do
            {
                nlayers++;
                

                Bin = GetFirstBinPositions(nlayers, CumSublayerBiomass.Count());

                while (Bin != null)
                {
                    layermaxdev.Clear();

                    if (Bin.Count() == 0)
                    {
                        layermaxdev.Add(CalculateLayerMaxDev(CumSublayerBiomass));
                    }
                    else for (int i = 0; i <= Bin.Count(); i++)
                    {
                        int[] MinMax = MinMaxCohortNr(Bin, i, CumSublayerBiomass.Count());

                        // Get the within-layer variance in biomass
                        layermaxdev.Add(CalculateLayerMaxDev(CumSublayerBiomass.GetRange(MinMax[0], MinMax[1] - MinMax[0])));
                    }

                    // Keep the optimal (min within-layer variance) layer setting
                    if (layermaxdev.Max() < LayerMaxDev)
                    {
                        BestBin = new List<int>(Bin).ToArray();
                        LayerMaxDev = layermaxdev.Max();
                    }
                    Bin = GetNextBinPositions(Bin, CumSublayerBiomass.Count());

                }
            }
            while (layermaxdev.Max() >= MaxDevLyrAv && nlayers < MaxCanopyLayers && nlayers < (CumSublayerBiomass.Count()/PlugIn.IMAX));
            //=====================END OPTIMIZATION LOOP====================================


            // Actual layer configuration
            List<List<int>> Bins = new List<List<int>>();
            if (BestBin.Count() == 0)
            {
                // One canopy layer
                Bins.Add(new List<int>());
                for (int i = 0; i < CumSublayerBiomass.Count(); i++)
                {
                    Bins[0].Add(i);
                }
            }
            else for (int i = 0; i <= BestBin.Count(); i++)
            {
                // Multiple canopy layers
                Bins.Add(new List<int>());

                int[] minmax = MinMaxCohortNr(BestBin, i, CumSublayerBiomass.Count());

                // Add index numbers to the Bins array
                for (int a = minmax[0]; a < ((i == BestBin.Count()) ? minmax[1]+1 : minmax[1]); a++)
                {
                    Bins[i].Add(a);
                }
            }
            return Bins;
        }*/
        private List<double> layerThreshRatio = new List<double>();
        private List<List<double>> GetBinsByCohort(List<double> CohortBiomassList)
        {
            if (CohortBiomassList.Count() == 0)
            {
                return null;
            }

            nlayers = 1;
            layerThreshRatio.Clear();
            float diffProp = LayerThreshRatio;
            // sort by ascending biomass
            CohortBiomassList.Sort();
            // reverse to sort by descending biomass
            CohortBiomassList.Reverse();

            List<List<double>> CohortBins = new List<List<double>>();
            int topLayerIndex = 0;
            CohortBins.Add(new List<double>());
            CohortBins[0].Add(CohortBiomassList[0]);
            foreach (double cohortBio in CohortBiomassList)
            {
                double smallestThisLayer = CohortBins[0][0];
                //if (layerIndex > 0)
                //{
                //    smallestThisLayer = CohortBins[layerIndex][0];
                //}
                double ratio = (cohortBio / smallestThisLayer);

                if (ratio < (diffProp))
                {
                    if (topLayerIndex < (MaxCanopyLayers - 1))
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
                if (!(CohortBins[0].Contains(cohortBio)))
                    CohortBins[0].Add(cohortBio);
                //bool largeRatio = (ratio > 10.0);
                //if (!(ratioAbove10.ContainsKey(cohortBio)))
                //{
                //    ratioAbove10.Add(cohortBio, largeRatio);
                //}
            }
            bool tooManyLayers = false;
            if (CohortBins.Count() > MaxCanopyLayers)
            {
                tooManyLayers = true;
            }
            if (tooManyLayers)
            {
                List<double> sortedRatios = layerThreshRatio.ToList();
                sortedRatios.Sort();
                //sortedRatios.Reverse();
                List<double> smallestRatios = new List<double>();
                for (int r = 0; r < (MaxCanopyLayers - 1); r++)
                {
                    smallestRatios.Add(sortedRatios[r]);
                }

                CohortBins.Clear();
                topLayerIndex = MaxCanopyLayers - 1;
                nlayers = 1;
                for (int r = 0; r < MaxCanopyLayers; r++)
                {
                    CohortBins.Add(new List<double>());
                }
                CohortBins[topLayerIndex].Add(CohortBiomassList[0]);
                int cohortInd = 0;
                foreach (double cohortRatio in layerThreshRatio)
                {
                    if (smallestRatios.Contains(cohortRatio))
                    {
                        //if (nlayers < (MaxCanopyLayers))
                        //{
                        topLayerIndex--;
                        nlayers++;

                    }
                    if (!(CohortBins[topLayerIndex].Contains(CohortBiomassList[cohortInd])))
                        CohortBins[topLayerIndex].Add(CohortBiomassList[cohortInd]);
                    cohortInd++;
                }

            }

            return CohortBins;
        }
        public static uint ComputeKey(uint a, ushort b)
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
        public int ReduceOrKillBiomassCohorts(Landis.Library.BiomassCohorts.IDisturbance disturbance)
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
                    if (reduction[reduction.Count() - 1] >= species_cohort[c].Biomass)  //Compare to aboveground biomass
                    {
                        ToRemove.Add(species_cohort[c]);
                        // Edited by BRM - 090115
                    }
                    else
                    {
                        double reductionProp = (double)reduction[reduction.Count() - 1] / (double)species_cohort[c].Biomass;  //Proportion of aboveground biomass
                        species_cohort[c].ReduceBiomass(this, reductionProp, disturbance.Type);  // Reduction applies to all biomass
                    }
                    //
                }

            }

            foreach (Cohort cohort in ToRemove)
            {
                RemoveCohort(cohort, disturbance.Type);
            }

            return reduction.Sum();
        }

        public int ReduceOrKillBiomassCohorts(IDisturbance disturbance)
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
                    if (reduction[reduction.Count() - 1] >= species_cohort[c].Biomass)  //Compare to aboveground biomass
                    {
                        ToRemove.Add(species_cohort[c]);
                        // Edited by BRM - 090115
                    }
                    else
                    {
                        double reductionProp = (double)reduction[reduction.Count() - 1] / (double)species_cohort[c].Biomass;  //Proportion of aboveground biomass
                        species_cohort[c].ReduceBiomass(this, reductionProp, disturbance.Type);  // Reduction applies to all biomass
                    }
                    //
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
       
        Landis.Library.AgeOnlyCohorts.ISpeciesCohorts Landis.Library.Cohorts.ISiteCohorts<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts>.this[ISpecies species]
        {
            get
            {
                if (cohorts.ContainsKey(species))
                {
                    return (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts)GetSpeciesCohort(cohorts[species]);
                }
                return null;
            }
        }

        public Landis.Library.BiomassCohorts.ISpeciesCohorts this[ISpecies species]
        {
            get
            {
                if (cohorts.ContainsKey(species))
                {
                    return GetSpeciesCohort(cohorts[species]);
                }
                return null;
                
            }
        }

        void Landis.Library.AgeOnlyCohorts.ISiteCohorts.RemoveMarkedCohorts(Landis.Library.AgeOnlyCohorts.ICohortDisturbance disturbance)
        {
            /*
            if (AgeOnlyDisturbanceEvent != null)
            {
                AgeOnlyDisturbanceEvent(this, new Landis.Library.BiomassCohorts.DisturbanceEventArgs(disturbance.CurrentSite, disturbance.Type));
            }
             */
            ReduceOrKillBiomassCohorts(new Landis.Library.BiomassCohorts.WrappedDisturbance(disturbance));
        }

        void Landis.Library.AgeOnlyCohorts.ISiteCohorts.RemoveMarkedCohorts(Landis.Library.AgeOnlyCohorts.ISpeciesCohortsDisturbance disturbance)
        {
            /*
            if (AgeOnlyDisturbanceEvent != null)
            {
                AgeOnlyDisturbanceEvent(this, new Landis.Library.BiomassCohorts.DisturbanceEventArgs(disturbance.CurrentSite, disturbance.Type));
            }
            */

            // Does this only occur when a site is disturbed?
            //Allocation.ReduceDeadPools(this, disturbance.Type); 

            //  Go through list of species cohorts from back to front so that
            //  a removal does not mess up the loop.
            int totalReduction = 0;

            List<Cohort> ToRemove = new List<Cohort>();

            Landis.Library.AgeOnlyCohorts.SpeciesCohortBoolArray isSpeciesCohortDamaged = new Landis.Library.AgeOnlyCohorts.SpeciesCohortBoolArray();

            foreach (ISpecies spc in cohorts.Keys)
            {
                Landis.Library.PnETCohorts.SpeciesCohorts speciescohort = GetSpeciesCohort(cohorts[spc]);
               
                isSpeciesCohortDamaged.SetAllFalse(speciescohort.Count);

                disturbance.MarkCohortsForDeath((Landis.Library.AgeOnlyCohorts.ISpeciesCohorts)speciescohort, isSpeciesCohortDamaged);

                for (int c = 0; c < isSpeciesCohortDamaged.Count; c++)
                {
                    if (isSpeciesCohortDamaged[c])
                    {
                        totalReduction += speciescohort[c].Biomass;

                        ToRemove.Add(cohorts[spc][c]);
//                        ToRemove.AddRange(cohorts[spc].Where(o => o.Age == speciescohort[c].Age));
                    }
                }

            }
            foreach (Cohort cohort in ToRemove)
            {
                Landis.Library.BiomassCohorts.Cohort.KilledByAgeOnlyDisturbance(disturbance, cohort, disturbance.CurrentSite, disturbance.Type);
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
                        RemoveCohort(species_cohort[cc], new ExtensionType(Names.ExtensionName));
                    }
                }
            }
        }

        // with cold temp killing cohorts - now moved to within CalculatePhotosynthesis function
        /*private void RemoveMarkedCohorts(float minMonthlyAvgTemp, float winterSTD)
        {

            for (int c = cohorts.Values.Count - 1; c >= 0; c--)
            {
                List<Cohort> species_cohort = cohorts.Values.ElementAt(c);

                for (int cc = species_cohort.Count - 1; cc >= 0; cc--)
                {
                    if (species_cohort[cc].IsAlive == false)
                    {
                      
                        RemoveCohort(species_cohort[cc], new ExtensionType(Names.ExtensionName));

                    }
                    else
                    {
                        if(permafrost)
                        {
                            // Check if low temp kills cohorts
                            if((minMonthlyAvgTemp - (3.0 * winterSTD)) < species_cohort[cc].SpeciesPnET.ColdTol)
                            {
                                RemoveCohort(species_cohort[cc], new ExtensionType(Names.ExtensionName));
                            }
                        }
                    }
                
                }
            }

        }*/

        public void RemoveCohort(Cohort cohort, ExtensionType disturbanceType)
        {

            if(disturbanceType.Name == Names.ExtensionName)
            {
                CohortsKilledBySuccession[cohort.Species.Index] += 1;
            }
            else if(disturbanceType.Name == "disturbance:harvest")
            {
                CohortsKilledByHarvest[cohort.Species.Index] += 1;
            }
            else if(disturbanceType.Name == "disturbance:fire")
            {
                CohortsKilledByFire[cohort.Species.Index] += 1;
            }
            else if (disturbanceType.Name == "disturbance:wind")
            {
                CohortsKilledByWind[cohort.Species.Index] += 1;
            }
            else
            {
                CohortsKilledByOther[cohort.Species.Index] += 1;
            }

            if (disturbanceType.Name != Names.ExtensionName)
            {
                Cohort.RaiseDeathEvent(this, cohort, Site, disturbanceType);
            }

            cohorts[cohort.Species].Remove(cohort);

            if (cohorts[cohort.Species].Count == 0)
            {
                cohorts.Remove(cohort.Species);
            }

            if (!DisturbanceTypesReduced.Contains(disturbanceType))
            {
                Allocation.ReduceDeadPools(this, disturbanceType); // Reduce dead pools before adding through Allocation
                DisturbanceTypesReduced.Add(disturbanceType);
            }
            Allocation.Allocate(this, cohort, disturbanceType, 1.0);  // Allocation fraction is 1.0 for complete removals


        }

        public bool IsMaturePresent(ISpecies species)
        {
            //ISpeciesPnET pnetSpecies = SpeciesParameters.SpeciesPnET[species];

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
                //List<Cohort> Cohorts = new List<Cohort>(i.Value.Where(o => o.Age < Timestep));

                if(Cohorts.Count > 1)
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
        
        public void AddWoodyDebris(float Litter, float KWdLit)
        {
            lock (Globals.CWDThreadLock)
            {
                SiteVars.WoodyDebris[Site].AddMass(Litter, KWdLit);
            }
        }
        public void RemoveWoodyDebris(double percentReduction)
        {
            lock (Globals.CWDThreadLock)
            {
                SiteVars.WoodyDebris[Site].ReduceMass(percentReduction);
            }
        }
        public void AddLitter(float AddLitter, ISpeciesPnET spc)
        {
            lock (Globals.litterThreadLock)
            {
                double KNwdLitter = Math.Max(0.3, (-0.5365 + (0.00241 * AET.Sum())) - (((-0.01586 + (0.000056 * AET.Sum())) * spc.FolLignin * 100)));

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
                       OutputHeaders.TopLayer + "," +
                       OutputHeaders.PAR0 + "," +
                       OutputHeaders.Tmin + "," +
                       OutputHeaders.Tave + "," +
                       OutputHeaders.Tday + "," +
                       OutputHeaders.Tmax + "," +
                       OutputHeaders.Precip + "," +
                       OutputHeaders.CO2 + "," +
                       OutputHeaders.O3 + "," +
                       OutputHeaders.RunOff + "," + 
                       OutputHeaders.Leakage + "," + 
                       OutputHeaders.PET + "," +
                       OutputHeaders.Evaporation + "," +
                       OutputHeaders.Transpiration + "," + 
                       OutputHeaders.Interception + "," +
                       OutputHeaders.SurfaceRunOff + "," +
                       OutputHeaders.water + "," +
                       OutputHeaders.PressureHead + "," + 
                       OutputHeaders.availableWater + "," +
                       OutputHeaders.SnowPack + "," +
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
                        OutputHeaders.WoodySenescence + "," + 
                        OutputHeaders.FoliageSenescence + "," +
                        OutputHeaders.SubCanopyPAR + ","+
                        OutputHeaders.SoilDiffusivity + "," +
                        OutputHeaders.FrostDepth+","+
                        OutputHeaders.LeakageFrac + "," +
                        OutputHeaders.AverageAlbedo;

            return s;
        }

        private void AddSiteOutput(IEcoregionPnETVariables monthdata)
        {
            //uint maxLayerDev = 0;
            //if (layermaxdev.Count() > 0)
            //    maxLayerDev = layermaxdev.Max();
            double maxLayerRatio = 0;
            if (layerThreshRatio.Count() > 0)
                maxLayerRatio = layerThreshRatio.Max();

            string s = monthdata.Time + "," +
                monthdata.Year + "," +
                monthdata.Month + "," +
                Ecoregion.Name + "," +
                Ecoregion.SoilType + "," +
                cohorts.Values.Sum(o => o.Count) + "," +
                //maxLayerDev + "," +
                maxLayerRatio + "," +
                nlayers + "," +
                MaxLayer + "," +
                monthdata.PAR0 + "," +
                monthdata.Tmin + "," +
                monthdata.Tave + "," +
                monthdata.Tday + "," +
                monthdata.Tmax + "," +
                monthdata.Prec + "," +
                monthdata.CO2 + "," +
                monthdata.O3 + "," +
                hydrology.RunOff + "," +
                hydrology.Leakage + "," +
                hydrology.PET + "," +
                hydrology.Evaporation + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.Transpiration.Sum() * x.BiomassLayerProp))) + "," +
                interception + "," +
                precLoss + "," +
                hydrology.Water + "," +
                hydrology.GetPressureHead(Ecoregion) + "," +
                (hydrology.Water * Ecoregion.RootingDepth * propRootAboveFrost + hydrology.SurfaceWater) + "," +  // mm of avialable water
                snowPack + "," +
                this.CanopyLAI.Sum() + "," +
                monthdata.VPD + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.GrossPsn.Sum() * x.BiomassLayerProp))) + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.NetPsn.Sum() * x.BiomassLayerProp))) + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.MaintenanceRespiration.Sum() * x.BiomassLayerProp))) + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.Wood * x.BiomassLayerProp))) + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.Root * x.BiomassLayerProp))) + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.Fol * x.BiomassLayerProp))) + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.NSC * x.BiomassLayerProp))) + "," +
                HeterotrophicRespiration + "," +
                SiteVars.Litter[Site].Mass + "," +
                SiteVars.WoodyDebris[Site].Mass + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.LastWoodySenescence * x.BiomassLayerProp))) + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.LastFoliageSenescence * x.BiomassLayerProp))) + "," +
                subcanopypar + "," +
                soilDiffusivity + "," +
                (topFreezeDepth * 1000) + "," +
                leakageFrac + "," +
                averageAlbedo[monthdata.Month - 1];

            this.siteoutput.Add(s);
        }
 
        public IEnumerator<Landis.Library.BiomassCohorts.ISpeciesCohorts> GetEnumerator()
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

        IEnumerator<Landis.Library.BiomassCohorts.ISpeciesCohorts> IEnumerable<Landis.Library.BiomassCohorts.ISpeciesCohorts>.GetEnumerator()
        {
            foreach (ISpecies species in cohorts.Keys)
            {
                Landis.Library.BiomassCohorts.ISpeciesCohorts isp = this[species];
                yield return isp;
            }
             
        }

        IEnumerator<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts> IEnumerable<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts>.GetEnumerator()
        {
            foreach (ISpecies species in cohorts.Keys)
            {
                yield return (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts)this[species];
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

            public float DarkConiferProportion
            {
                get
                {
                    return Total == 0 ? 0 : DarkConifer / Total;
                }
            }

            public float LightConiferProportion
            {
                get
                {
                    return Total == 0 ? 0 : LightConifer / Total;
                }
            }

            public float DeciduousProportion
            {
                get
                {
                    return Total == 0 ? 0 : Deciduous / Total;
                }
            }

            public float GrassMossOpenProportion
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

    }


}
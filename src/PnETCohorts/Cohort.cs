// uses dominance to allocate psn and subtract transpiration from soil water, average cohort vars over layer

using Landis.Core;
using Landis.Extension.Succession.BiomassPnET;
using Landis.SpatialModeling;
using Landis.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Library.PnETCohorts
{
    public class Cohort : Landis.Library.AgeOnlyCohorts.ICohort, Landis.Library.BiomassCohorts.ICohort, Landis.Library.PnETCohorts.ICohort
    {
        public delegate void SubtractTranspiration(float transpiration, ISpeciesPnET Species);
        public delegate void AddWoodyDebris(float Litter, float KWdLit);
        public delegate void AddLitter(float AddLitter, ISpeciesPnET Species);

        public static IEcoregionPnET ecoregion;
        public static ActiveSite site;

        public static AddWoodyDebris addwoodydebris;
        
        public static AddLitter addlitter;

        public ushort index;

        private ISpecies species;
        private ISpeciesPnET speciesPnET;
        private CohortData data;
        private LocalOutput cohortoutput;
        private bool firstYear;
        private bool firstDefol = true; // First defoliation applied to cohort
        private bool firstAlloc = true;  //First foliage allocation applied to cohort (to avoid multiple applications for each sublayer)

        // Age (years)
        public ushort Age
        {
            get
            {
                return data.Age;
            }
        }

        // Non soluble carbons
        public float NSC
        {
            get
            {
                return data.NSC;
            }
        }

        // Foliage (g/m2)
        public float Fol
        {
            get
            {
                return data.Fol;
            }
            private set
            {
                data.Fol = value;
            }
        }

        // Aboveground Biomass (g/m2)
        public int Biomass
        {
            get
            {
                return (int)Math.Round((1 - speciesPnET.FracBelowG) * data.Biomass) + (int)data.Fol;
            }
        }

        // Total Biomass (root + wood) (g/m2)
        public int TotalBiomass
        {
            get
            {
                return (int)Math.Round(data.Biomass);
            }
        }

        // Wood (g/m2)
        public uint Wood
        {
            get
            {
                return (uint)Math.Round((1 - speciesPnET.FracBelowG) * data.Biomass);
            }
        }

        // Root (g/m2)
        public uint Root
        {
            get
            {
                return (uint)Math.Round(speciesPnET.FracBelowG * data.Biomass);
            }
        }

        // Max biomass achived in the cohorts' life time. 
        // This value remains high after the cohort has reached its 
        // peak biomass. It is used to determine canopy layers where
        // it prevents that a cohort could descent in the canopy when 
        // it declines (g/m2)
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
        float Fage
        {
            get
            {
                return Math.Max(0, 1 - (float)Math.Pow((Age / (float)speciesPnET.Longevity), speciesPnET.PsnAgeRed));
            }
        }

        // NSC fraction: measure for resources
        public float NSCfrac
        {
            get
            {
                return NSC / (FActiveBiom * (Biomass + Fol));
            }
        }

        // Species with PnET parameter additions
        public ISpeciesPnET SpeciesPNET
        {
            get
            {
                return speciesPnET;
            }
        }

        // LANDIS species (without PnET parameter additions)
        public Landis.Core.ISpecies Species
        {
            get
            {
                return species;
            }
        }

        // Defoliation proportion - BRM
        public float DefolProp
        {
            get
            {
                return data.DeFolProp;
            }
        }

        // Annual Woody Senescence (g/m2)
        public int LastWoodySenescence
        {
            get
            {
                return (int)data.LastWoodySenescence;
            }
        }
        // Annual Foliage Senescence (g/m2)
        public int LastFoliageSenescence
        {
            get
            {
                return (int)data.LastFoliageSenescence;
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

        public float[] Water
        {
            get
            {
                return data.Water;
            }
        }

        public float FActiveBiom
        {
            get
            {
                return (float)Math.Exp(-speciesPnET.FrActWd * Biomass);
            }
        }

        public bool IsAlive
        {
            // Determine if cohort is alive. It is assumed that a cohort is dead when 
            // NSC decline below 1% of biomass
            get
            {
                return NSCfrac > 0.01F;
            }
        }

        public float SumLAI
        {
            get
            {
                return data.LAI.Sum();
            }
        }

        public void InitializeSubLayers()
        {
            // Initialize subcanopy layers
            index = 0;
            data.LAI = new float[PlugIn.IMAX];
            data.GrossPsn = new float[PlugIn.IMAX];
            data.FolResp = new float[PlugIn.IMAX];
            data.NetPsn = new float[PlugIn.IMAX];
            data.Transpiration = new float[PlugIn.IMAX];
            data.FRad = new float[PlugIn.IMAX];
            data.FWater = new float[PlugIn.IMAX];
            data.Water = new float[PlugIn.IMAX];
            data.PressHead = new float[PlugIn.IMAX];
            data.FOzone = new float[PlugIn.IMAX];
            data.MaintenanceRespiration = new float[PlugIn.IMAX];
            data.Interception = new float[PlugIn.IMAX];
            data.AdjFolN = new float[PlugIn.IMAX];
            data.AdjFracFol = new float[PlugIn.IMAX];
            data.CiModifier = new float[PlugIn.IMAX];
            data.DelAmax = new float[PlugIn.IMAX];
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

        public void CalcAdjFracFol()
        {
            if (data.LastSeasonFRad.Count() > 0)
            {
                float lastSeasonAvgFRad = data.LastSeasonFRad.ToArray().Average();
                float fracFol_slope = speciesPnET.FracFolShape;
                float fracFol_int = speciesPnET.MaxFracFol;
                // linear version
                //data.adjFracFol = (lastSeasonAvgFRad * fracFol_slope + fracFol_int) * speciesPnET.FracFol;
                //exponential version
                //data.adjFracFol = (float)Math.Pow((lastSeasonAvgFRad + 0.2), fracFol_slope) * speciesPnET.FracFol + speciesPnET.FracFol * fracFol_int;
                //modified exponential version - controls lower and upper limit of function
                data.adjFracFol = speciesPnET.FracFol + ((fracFol_int - speciesPnET.FracFol) * (float)Math.Pow(lastSeasonAvgFRad, fracFol_slope)); //slope is shape parm; fracFol is minFracFol; int is maxFracFol. EJG-7-24-18

                firstYear = false;
            }
            else
                data.adjFracFol = speciesPnET.FracFol;
            data.LastSeasonFRad = new List<float>();

        }

        public void NullSubLayers()
        {
            // Reset values for subcanopy layers
            data.LAI = null;
            data.GrossPsn = null;
            data.FolResp = null;
            data.NetPsn = null;
            data.Transpiration = null;
            data.FRad = null;
            data.FWater = null;
            data.PressHead = null;
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
        public void Accumulate(Cohort c)
        {
            data.Biomass += c.Biomass;
            data.BiomassMax = Math.Max(BiomassMax, Biomass);
            data.Fol += c.Fol;
        }

        // Constructor
        public Cohort(ISpeciesPnET speciesPnET, ushort year_of_birth, string SiteName) // : base(speciesPnET, 0, (int)(1F / speciesPnET.DNSC * (ushort)speciesPnET.InitialNSC))
        {
            this.speciesPnET =  speciesPnET;
            data.Age = 1;
            data.ColdKill = int.MaxValue;
           
            this.data.NSC = (ushort)speciesPnET.InitialNSC;
           
            // Initialize biomass assuming fixed concentration of NSC
            this.data.Biomass = (uint)(1F / speciesPnET.DNSC * (ushort)speciesPnET.InitialNSC);

            data.BiomassMax = Biomass;

            // Then overwrite them if you need stuff for outputs
            if (SiteName != null)
            {
                InitializeOutput(SiteName, year_of_birth);
            }

            data.LastSeasonFRad = new List<float>();
            firstYear = true;
        }

        public Cohort(ISpecies species,
                      CohortData cohortData)
        {
            this.species = (ISpecies)species;
            this.speciesPnET = SpeciesParameters.SpeciesPnET.AllSpecies[speciesPnET.Index];
            this.data = cohortData;
        }

        public Cohort(Cohort cohort) // : base(cohort.speciesPnET, new Landis.Library.BiomassCohorts.CohortData(cohort.age, cohort.Biomass))
        {
            this.species = (ISpecies)cohort.SpeciesPNET;
            this.speciesPnET = cohort.speciesPnET;
            this.data.Age = cohort.Age;
            this.data.NSC = cohort.NSC;
            this.data.Biomass = cohort.Biomass;
            this.data.BiomassMax = cohort.BiomassMax;
            this.data.Fol = cohort.Fol;
            this.data.LastSeasonFRad = cohort.data.LastSeasonFRad;
        }

        public Cohort(ISpeciesPnET speciesPnET, ushort age, int woodBiomass, string SiteName, ushort firstYear)
        {
            InitializeSubLayers();
            this.speciesPnET = speciesPnET;
            this.data.Age = age;
            //incoming biomass is aboveground wood, calculate total biomass
            int biomass = (int) (woodBiomass / (1 - speciesPnET.FracBelowG));
            this.data.Biomass = biomass;
            this.data.NSC = this.speciesPnET.DNSC * this.FActiveBiom * this.Biomass;
            this.data.BiomassMax = biomass;
            this.data.LastSeasonFRad = new List<float>();
            this.data.adjFracFol = speciesPnET.FracFol;
            this.data.ColdKill = int.MaxValue;

            if (this.data.Leaf_On)
            {
                this.data.Fol = (data.adjFracFol * FActiveBiom * biomass);
                data.LAI[index] = CalculateLAI(this.speciesPnET, Fol, index);
            }

            if (SiteName != null)
            {
                InitializeOutput(SiteName, firstYear);
            }
        }

        // Makes sure that litters are allocated to the appropriate site
        public static void SetSiteAccessFunctions(SiteCohorts sitecohorts)
        {
             Cohort.addlitter = sitecohorts.AddLitter;
             Cohort.addwoodydebris = sitecohorts.AddWoodyDebris;
             Cohort.ecoregion = sitecohorts.Ecoregion;
             Cohort.site = sitecohorts.Site;
        }
        

        public void CalculateDefoliation(ActiveSite site, int SiteAboveGroundBiomass)
        {
            int abovegroundBiomass = (int)((1 - speciesPnET.FracBelowG) * Biomass) + (int)Fol;
            //defolProp = (float)Landis.Library.Biomass.CohortDefoliation.Compute(site, speciesPnET, abovegroundBiomass, SiteAboveGroundBiomass);
            data.DeFolProp = (float)Landis.Library.BiomassCohorts.CohortDefoliation.Compute(this, site, SiteAboveGroundBiomass);
        }

        // Photosynthesis by canopy layer
        public bool CalculatePhotosynthesis(float PrecInByCanopyLayer,int precipCount, float leakageFrac, IHydrology hydrology, ref float SubCanopyPar, float o3_cum, float o3_month, int subCanopyIndex, int layerCount, ref float O3Effect, float frostFreeProp, float MeltInByCanopyLayer, bool coldKillBoolean)
         {      
            bool success = true;
            float lastO3Effect = O3Effect;
            O3Effect = 0;

            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            data.LAI[index] = CalculateLAI(speciesPnET, data.Fol, index);

            // Precipitation interception has a max in the upper canopy and decreases exponentially through the canopy
            //Interception[index] = PrecInByCanopyLayer * (float)(1 - Math.Exp(-1 * ecoregion.PrecIntConst * LAI[index]));
            //if (Interception[index] > PrecInByCanopyLayer) throw new System.Exception("Error adding water, PrecInByCanopyLayer = " + PrecInByCanopyLayer + " Interception[index] = " + Interception[index]);

            if (MeltInByCanopyLayer > 0)
            {
                // Add melted snow water to soil moisture
                // Instantaneous runoff (excess of porosity + RunoffCapture)
                float waterCapacity = ecoregion.Porosity * ecoregion.RootingDepth * frostFreeProp; //mm
                float meltrunoff = Math.Min(MeltInByCanopyLayer, Math.Max(hydrology.Water * ecoregion.RootingDepth * frostFreeProp + MeltInByCanopyLayer - waterCapacity, 0));
                //if ((hydrology.Water + meltrunoff) > (ecoregion.Porosity + ecoregion.RunoffCapture))
                //    meltrunoff = (hydrology.Water + meltrunoff) - (ecoregion.Porosity + ecoregion.RunoffCapture);
                float capturedRunoff = 0;
                if ((ecoregion.RunoffCapture > 0) & (meltrunoff >0))
                {
                    capturedRunoff = Math.Max(0, Math.Min(meltrunoff, (ecoregion.RunoffCapture - Hydrology.SurfaceWater)));
                    Hydrology.SurfaceWater += capturedRunoff;
                }
                Hydrology.RunOff += (meltrunoff - capturedRunoff);

                success = hydrology.AddWater(MeltInByCanopyLayer - meltrunoff, ecoregion.RootingDepth * frostFreeProp);
                if (success == false) throw new System.Exception("Error adding water, MeltInByCanopyLayer = " + MeltInByCanopyLayer + "; water = " + hydrology.Water + "; meltrunoff = " + meltrunoff + "; ecoregion = " + ecoregion.Name + "; site = " + site.Location);
            }
            float precipIn = 0;
            // If more than one precip event assigned to layer, repeat precip, runoff, leakage for all events prior to respiration
            for (int p = 1; p <= precipCount; p++)
            {
                // Incoming precipitation
                // Instantaneous runoff (excess of porosity)
                float waterCapacity = ecoregion.Porosity * ecoregion.RootingDepth * frostFreeProp; //mm
                float rainrunoff = Math.Min(PrecInByCanopyLayer, Math.Max(hydrology.Water * ecoregion.RootingDepth * frostFreeProp + PrecInByCanopyLayer - waterCapacity, 0));
                //if ((hydrology.Water + rainrunoff) > (ecoregion.Porosity + ecoregion.RunoffCapture))
                //    rainrunoff = (hydrology.Water + rainrunoff) - (ecoregion.Porosity + ecoregion.RunoffCapture);
                float capturedRunoff = 0;
                if ((ecoregion.RunoffCapture > 0) & (rainrunoff > 0))
                {
                    capturedRunoff = Math.Max(0,Math.Min(rainrunoff,(ecoregion.RunoffCapture - Hydrology.SurfaceWater)));
                    Hydrology.SurfaceWater += capturedRunoff;
                }
                Hydrology.RunOff += (rainrunoff - capturedRunoff);
                
                precipIn = PrecInByCanopyLayer - rainrunoff; //mm

                // Add incoming precipitation to soil moisture
                success = hydrology.AddWater(precipIn, ecoregion.RootingDepth * frostFreeProp);
                if (success == false) throw new System.Exception("Error adding water, waterIn = " + precipIn + "; water = " + hydrology.Water + "; rainrunoff = " + rainrunoff + "; ecoregion = " + ecoregion.Name + "; site = " + site.Location);
            }

            // Leakage only occurs following precipitation events or incoming melt water
            if (precipIn > 0 || MeltInByCanopyLayer > 0)
            {
                float leakage = Math.Max((float)leakageFrac * (hydrology.Water - ecoregion.FieldCap), 0) * ecoregion.RootingDepth * frostFreeProp; //mm
                Hydrology.Leakage += leakage;

                // Remove fast leakage
                success = hydrology.AddWater(-1 * leakage, ecoregion.RootingDepth * frostFreeProp);
                if (success == false) throw new System.Exception("Error adding water, Hydrology.Leakage = " + Hydrology.Leakage + "; water = " + hydrology.Water + "; ecoregion = " + ecoregion.Name + "; site = " + site.Location);
                if (Hydrology.SurfaceWater > 0)
                {
                    float surfaceInput = Math.Min(Hydrology.SurfaceWater,(ecoregion.Porosity - hydrology.Water));
                    Hydrology.SurfaceWater -= surfaceInput;
                    success = hydrology.AddWater(surfaceInput, ecoregion.RootingDepth * frostFreeProp);
                    if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + Hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + ecoregion.Name + "; site = " + site.Location);
                }
            }

            //// Adjust soil water for freezing - Now done when calculating frozen depth
            //if (frostFreeProp < 1.0)
            //{
            //    // water in frozen soil is not accessible - treat it as if it leaked out
            //    float frozenLimit = ecoregion.FieldCap * frostFreeProp;
            //    float frozenWater = hydrology.Water - frozenLimit;
            //    // Remove frozen water
            //    success = hydrology.AddWater(-1 * frozenWater);
            //    if (success == false) throw new System.Exception("Error adding water, frozenWater = " + frozenWater + "; water = " + hydrology.Water + "; ecoregion = " + ecoregion.Name + "; site = " + site.Location);
            //}
            // Maintenance respiration depends on biomass,  non soluble carbon and temperature
            data.MaintenanceRespiration[index] = (1 / (float)PlugIn.IMAX) * (float)Math.Min(NSC, ecoregion.Variables[Species.Name].MaintRespFTempResp * Biomass);//gC //IMAXinverse

            // Subtract mainenance respiration (gC/mo)
            data.NSC -= data.MaintenanceRespiration[index];


            // Woody decomposition: do once per year to reduce unnescessary computation time so with the last subcanopy layer 
            if (index == PlugIn.IMAX - 1)
            {

                // In the last month
                if (ecoregion.Variables.Month == (int)Constants.Months.December)
                {
                    //Check if nscfrac is below threshold to determine if cohort is alive
                    if (!this.IsAlive)
                    {
                        data.NSC = 0.0F;  // if cohort is dead, nsc goes to zero and becomes functionally dead even though not removed until end of timestep
                    }
                    else if(PlugIn.ModelCore.CurrentTime > 0 && this.TotalBiomass < (uint)speciesPnET.InitBiomass)  //Check if biomass < Initial Biomass -> cohort dies
                    {
                        data.NSC = 0.0F;  // if cohort is dead, nsc goes to zero and becomes functionally dead even though not removed until end of timestep
                        data.Leaf_On = false;
                        data.NSC = 0.0F;
                        float foliageSenescence = FoliageSenescence();
                        addlitter(foliageSenescence, SpeciesPNET);
                        data.LastFoliageSenescence = foliageSenescence;
                    }

                    float woodSenescence = Senescence();
                    addwoodydebris(woodSenescence, speciesPnET.KWdLit);
                    data.LastWoodySenescence = woodSenescence;

                    // Release of nsc, will be added to biomass components next year
                    // Assumed that NSC will have a minimum concentration, excess is allocated to biomass
                    float Allocation = Math.Max(NSC - (speciesPnET.DNSC * FActiveBiom * Biomass), 0);
                    data.Biomass += Allocation;
                    data.BiomassMax = Math.Max(BiomassMax, Biomass);
                    data.NSC -= Allocation;
                    data.Age++;

                    firstDefol = true;
                    firstAlloc = true;
                }

            }
            if (coldKillBoolean)
            {
                data.ColdKill = (int)Math.Floor(ecoregion.Variables.Tave - (3.0 * ecoregion.WinterSTD));
                data.Leaf_On = false;
                data.NSC = 0.0F;
                float foliageSenescence = FoliageSenescence();
                addlitter(foliageSenescence, SpeciesPNET);
                data.LastFoliageSenescence = foliageSenescence;
            }
            else
            {
                // When LeafOn becomes false for the first time in a year
                if (ecoregion.Variables.Tmin <= this.SpeciesPNET.LeafOnMinT)
                {
                    if (data.Leaf_On == true)
                    {
                        data.Leaf_On = false;
                        float foliageSenescence = FoliageSenescence();
                        addlitter(foliageSenescence, SpeciesPNET);
                        data.LastFoliageSenescence = foliageSenescence;
                    }
                }
                else
                {
                    data.Leaf_On = true;
                }
            }
            /****************************** MGM's restructuring 10/25/2018 ***************************************/
            if (data.Leaf_On)
            {
                // Foliage linearly increases with active biomass
                //float IdealFol = (speciesPnET.FracFol * FActiveBiom * biomass);
                if (firstYear)
                    data.adjFracFol = speciesPnET.FracFol;
                float IdealFol = (data.adjFracFol * FActiveBiom * Biomass); // Using adjusted FracFol

                if (ecoregion.Variables.Month < (int)Constants.Months.June) //Growing season before defoliation outbreaks
                {
                    if (IdealFol > Fol)
                    {
                        // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                        // carbon fraction of biomass to convert C to DW
                        float Folalloc = Math.Max(0, Math.Min(NSC, speciesPnET.CFracBiomass * (IdealFol - Fol))); // gC/mo

                        // Add foliage allocation to foliage
                        data.Fol += Folalloc / speciesPnET.CFracBiomass;// gDW

                        // Subtract from NSC
                        data.NSC -= Folalloc;
                    }
                }
                else if (ecoregion.Variables.Month == (int)Constants.Months.June) //Apply defoliation only in June
                {                    
                    if(firstDefol) // prevents multiple rounds of defoliation within a cohort (which shares canopy variables, like foliage)
                    {
                        ReduceFoliage(DefolProp);
                        firstDefol = false;
                    }                    
                }
                else if (ecoregion.Variables.Month > (int)Constants.Months.June) //During and after defoliation events
                {
                    if (DefolProp > 0 && firstAlloc)
                    {
                        if (DefolProp > 0.60 && speciesPnET.TOfol == 1)  // Refoliation at >60% reduction in foliage for deciduous trees - MGM
                        {
                            // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                            // carbon fraction of biomass to convert C to DW
                            float Folalloc = Math.Max(0f, Math.Min(NSC, speciesPnET.CFracBiomass * ((0.70f * IdealFol) - Fol)));  // 70% refoliation
 
                            float Folalloc2 = Math.Max(0f, Math.Min(NSC, speciesPnET.CFracBiomass * (0.95f * IdealFol - Fol)));  // cost of refol is the cost of getting to IdealFol

                            Fol += Folalloc / speciesPnET.CFracBiomass;// gDW

                            // Subtract from NSC
                            data.NSC -= Folalloc2; // resource intensive to reflush in middle of growing season
                        }
                        else //No attempted refoliation but carbon loss after defoliation
                        {
                            // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                            // carbon fraction of biomass to convert C to DW
                            float Folalloc = Math.Max(0f, Math.Min(NSC, speciesPnET.CFracBiomass * (0.10f * IdealFol))); // gC/mo 10% of IdealFol to take out NSC 

                            // Subtract from NSC do not add Fol
                            data.NSC -= Folalloc;
                        }
                        firstAlloc = false;  // Denotes that allocation has been applied to one sublayer
                    }
                    else if (IdealFol > Fol && firstAlloc)    //Non-defoliated trees refoliate 'normally', but only once per cohort
                    {
                        // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                        // carbon fraction of biomass to convert C to DW
                        float Folalloc = Math.Max(0, Math.Min(NSC, speciesPnET.CFracBiomass * (IdealFol - Fol))); // gC/mo

                        // Add foliage allocation to foliage
                        data.Fol += Folalloc / speciesPnET.CFracBiomass;// gDW

                        // Subtract from NSC
                        data.NSC -= Folalloc;
                        firstAlloc = false; // Denotes that allocation has been applied to one sublayer
                    }
                }
            }
            /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^ MGM's restructuring 10/25/2018 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/

            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            data.LAI[index] = CalculateLAI(speciesPnET, Fol, index);

            // Adjust HalfSat for CO2 effect
            float halfSatIntercept = speciesPnET.HalfSat - 350 * speciesPnET.CO2HalfSatEff;
            data.AdjHalfSat = speciesPnET.CO2HalfSatEff * ecoregion.Variables.CO2 + halfSatIntercept;
            // Reduction factor for radiation on photosynthesis
            data.FRad[index] = ComputeFrad(SubCanopyPar, data.AdjHalfSat);

            // Below-canopy PAR if updated after each subcanopy layer
            SubCanopyPar *= (float)Math.Exp(-speciesPnET.K * data.LAI[index]);

            // Get pressure head given ecoregion and soil water content (latter in hydrology)
            float PressureHead = hydrology.GetPressureHead(ecoregion);

            // Reduction water for sub or supra optimal soil water content

            float fWaterOzone = 1.0f;  //fWater for ozone functions; ignores H1 and H2 parameters because only impacts when drought-stressed
            if (PlugIn.ModelCore.CurrentTime > 0)
            {
                data.FWater[index] = ComputeFWater(speciesPnET.H1, speciesPnET.H2, speciesPnET.H3, speciesPnET.H4, PressureHead);
                data.Water[index] = hydrology.Water;
                data.PressHead[index] = PressureHead;
                fWaterOzone = ComputeFWater(-1, -1, speciesPnET.H3, speciesPnET.H4, PressureHead); // ignores H1 and H2 parameters because only impacts when drought-stressed
            }
            else // Ignore H1 and H2 parameters during spinup
            {
                data.FWater[index] = ComputeFWater(-1, -1, speciesPnET.H3, speciesPnET.H4, PressureHead);
                data.Water[index] = hydrology.Water;
                data.PressHead[index] = PressureHead;
                fWaterOzone = data.FWater[index];
            }



            // FoliarN adjusted based on canopy position (FRad)
            float folN_shape = speciesPnET.FolNShape; //Slope for linear FolN relationship
            float maxFolN = speciesPnET.MaxFolN; //Intercept for linear FolN relationship
            //adjFolN = (FRad[index] * folN_slope + folN_int) * speciesPnET.FolN; // Linear reduction (with intercept) in FolN with canopy depth (FRad)
            //adjFolN = (float)Math.Pow((FRad[index]), folN_slope) * speciesPnET.FolN + speciesPnET.FolN * folN_int; // Expontential reduction
            // Non-Linear reduction in FolN with canopy depth (FRad)
            data.adjFolN = speciesPnET.FolN + ((maxFolN - speciesPnET.FolN) * (float)Math.Pow(data.FRad[index], folN_shape)); //slope is shape parm; FolN is minFolN; intcpt is max FolN. EJG-7-24-18

            data.AdjFolN[index] = data.adjFolN;  // Stored for output
            data.AdjFracFol[index] = data.adjFracFol; //Stored for output


            float ciModifier = fWaterOzone; // if no ozone, ciModifier defaults to fWater
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
                {
                    throw new System.Exception("Ozone data provided, but speciesPnET O3StomataSens is not set to Sensitive, Tolerant or Intermediate");
                }
            }

            data.CiModifier[index] = ciModifier;  // Stored for output

            // If trees are physiologically active
            if (data.Leaf_On)
            {
                // CO2 ratio internal to the leaf versus external
                float cicaRatio = (-0.075f * data.adjFolN) + 0.875f;
                float modCiCaRatio = cicaRatio * ciModifier;
                // Reference co2 ratio
                float ci350 = 350 * modCiCaRatio;
                // Elevated leaf internal co2 concentration
                float ciElev = ecoregion.Variables.CO2 * modCiCaRatio;
                float Ca_Ci = ecoregion.Variables.CO2 - ciElev;

                // Franks method
                // (Franks,2013, New Phytologist, 197:1077-1094)
                float Gamma = 40; // 40; Gamma is the CO2 compensation point (the point at which photorespiration balances exactly with photosynthesis.  Assumed to be 40 based on leaf temp is assumed to be 25 C

                // Modified Gamma based on air temp
                // Tested here but removed for release v3.0
                // Bernacchi et al. 2002. Plant Physiology 130, 1992-1998
                // Gamma* = e^(13.49-24.46/RTk) [R is universal gas constant = 0.008314 kJ/J/mole, Tk is absolute temperature]
                //float Gamma_T = (float) Math.Exp(13.49 - 24.46 / (0.008314 * (ecoregion.Variables.Tday + 273)));

                float Ca0 = 350;  // 350
                float Ca0_adj = Ca0 * cicaRatio;  // Calculated internal concentration given external 350


                // Franks method (Franks,2013, New Phytologist, 197:1077-1094)
                float delamax = (ecoregion.Variables.CO2 - Gamma) / (ecoregion.Variables.CO2 + 2 * Gamma) * (Ca0 + 2 * Gamma) / (Ca0 - Gamma);
                if (delamax < 0)
                {
                    delamax = 0;
                }

                // Franks method (Franks,2013, New Phytologist, 197:1077-1094)
                // Adj Ca0
                float delamax_adj = (ecoregion.Variables.CO2 - Gamma) / (ecoregion.Variables.CO2 + 2 * Gamma) * (Ca0_adj + 2 * Gamma) / (Ca0_adj - Gamma);
                if (delamax_adj < 0)
                {
                    delamax_adj = 0;
                }

                // Modified Franks method - by M. Kubiske
                // substitute ciElev for CO2
                float delamaxCi = (ciElev - Gamma) / (ciElev + 2 * Gamma) * (Ca0 + 2 * Gamma) / (Ca0 - Gamma);
                if (delamaxCi < 0)
                {
                    delamaxCi = 0;
                }

                // Modified Franks method - by M. Kubiske
                // substitute ciElev for CO2
                // adjusted Ca0
                float delamaxCi_adj = (ciElev - Gamma) / (ciElev + 2 * Gamma) * (Ca0_adj + 2 * Gamma) / (Ca0_adj - Gamma);
                if (delamaxCi_adj < 0)
                {
                    delamaxCi_adj = 0;
                }

                // Choose between delamax methods here:
                data.DelAmax[index] = delamax;  // Franks
                //DelAmax[index] = delamax_adj;  // Franks with adjusted Ca0
                //DelAmax[index] = delamaxCi;  // Modified Franks
                //DelAmax[index] = delamaxCi_adj;  // Modified Franks with adjusted Ca0

                // M. Kubiske method for wue calculation:  Improved methods for calculating WUE and Transpiration in PnET.
                float V = (float)(8314.47 * (ecoregion.Variables.Tmin + 273) / 101.3);
                float JCO2 = (float)(0.139 * ((ecoregion.Variables.CO2 - ciElev) / V) * 0.00001);
                float JH2O = ecoregion.Variables[speciesPnET.Name].JH2O * ciModifier;
                float wue = (JCO2 / JH2O) * (44 / 18);  //44=mol wt CO2; 18=mol wt H2O; constant =2.44444444444444

                float Amax = (float)(delamaxCi * (speciesPnET.AmaxA + ecoregion.Variables[speciesPnET.Name].AmaxB_CO2 * data.adjFolN));

                //Reference net Psn (lab conditions) in gC/g Fol/month
                float RefNetPsn = ecoregion.Variables.DaySpan * (Amax * ecoregion.Variables[speciesPnET.Name].DVPD * ecoregion.Variables.Daylength * Constants.MC) / Constants.billion;

                // PSN (gC/g Fol/month) reference net psn in a given temperature
                float FTempPSNRefNetPsn = ecoregion.Variables[speciesPnET.Name].FTempPSN * RefNetPsn;

                // Compute net psn from stress factors and reference net psn (gC/g Fol/month)
                // FTempPSNRefNetPsn units are gC/g Fol/mo
                float nonOzoneNetPsn = (1 / (float)PlugIn.IMAX) * data.FWater[index] * data.FRad[index] * Fage * FTempPSNRefNetPsn * Fol;  // gC/m2 ground/mo

                // Convert Psn gC/m2 ground/mo to umolCO2/m2 fol/s
                // netPsn_ground = LayerNestPsn*1000000umol*(1mol/12gC) * (1/(60s*60min*14hr*30day))
                float netPsn_ground = nonOzoneNetPsn * 1000000F * (1F / 12F) * (1F / (ecoregion.Variables.Daylength * ecoregion.Variables.DaySpan));
                float netPsn_leaf_s = 0;
                if (netPsn_ground > 0 && data.LAI[index] > 0)
                {
                    // nesPsn_leaf_s = NetPsn_ground*(1/LAI){m2 fol/m2 ground}
                    netPsn_leaf_s = netPsn_ground * (1F / data.LAI[index]);
                    if (float.IsInfinity(netPsn_leaf_s))
                    {
                        netPsn_leaf_s = 0;
                    }
                }

                //Calculate water vapor conductance (gwv) from Psn and Ci; Kubiske Conductance_5.xlsx
                //gwv_mol = NetPsn_leaf_s /(Ca-Ci) {umol/mol} * 1.6(molH20/molCO2)*1000 {mmol/mol}
                float gwv_mol = (float)(netPsn_leaf_s / (Ca_Ci) * 1.6 * 1000);
                //gwv = gwv_mol / (444.5 - 1.3667*Tc)*10    {denominator is from Koerner et al. 1979 (Sheet 3),  Tc = temp in degrees C, * 10 converts from cm to mm.  
                float gwv = (float)(gwv_mol / (444.5 - 1.3667 * ecoregion.Variables.Tave) * 10);

                // Calculate gwv from Psn using Ollinger equation
                // g = -0.3133+0.8126*NetPsn_leaf_s
                //float g = (float) (-0.3133 + 0.8126 * netPsn_leaf_s);

                // Reduction factor for ozone on photosynthesis
                if (o3_month > 0)
                {
                    float o3Coeff = speciesPnET.O3GrowthSens;
                    O3Effect = ComputeO3Effect_PnET(o3_month, delamaxCi, netPsn_leaf_s, subCanopyIndex, layerCount, Fol, lastO3Effect, gwv, data.LAI[index], o3Coeff);
                }
                else
                { O3Effect = 0; }
                data.FOzone[index] = 1 - O3Effect;


                //Apply reduction factor for Ozone
                data.NetPsn[index] = nonOzoneNetPsn * data.FOzone[index];

                // Net foliage respiration depends on reference psn (AMAX)
                //float FTempRespDayRefResp = ecoregion.Variables[speciesPnET.Name].FTempRespDay * ecoregion.Variables.DaySpan * ecoregion.Variables.Daylength * Constants.MC / Constants.billion * ecoregion.Variables[speciesPnET.Name].Amax;
                //Subistitute 24 hours in place of DayLength because foliar respiration does occur at night.  FTempRespDay uses Tave temps reflecting both day and night temperatures.
                float FTempRespDayRefResp = ecoregion.Variables[speciesPnET.Name].FTempRespDay * ecoregion.Variables.DaySpan * (Constants.SecondsPerHour * 24) * Constants.MC / Constants.billion * Amax;

                // Actal foliage respiration (growth respiration) 
                data.FolResp[index] = data.FWater[index] * FTempRespDayRefResp * Fol / (float)PlugIn.IMAX;

                // Gross psn depends on net psn and foliage respiration
                data.GrossPsn[index] = data.NetPsn[index] + data.FolResp[index];

                // M. Kubiske equation for transpiration: Improved methods for calculating WUE and Transpiration in PnET.
                // JH2O has been modified by CiModifier to reduce water use efficiency
                data.Transpiration[index] = (float)(0.01227 * (data.GrossPsn[index] / (JCO2 / JH2O))); //mm

                // It is possible for transpiration to calculate to exceed available water
                // In this case, we cap transpiration at available water, and back-calculate GrossPsn and NetPsn to downgrade those as well
                if(data.Transpiration[index] > (hydrology.Water * ecoregion.RootingDepth * frostFreeProp))
                {
                    data.Transpiration[index] = hydrology.Water * ecoregion.RootingDepth * frostFreeProp; //mm
                    data.GrossPsn[index] = (data.Transpiration[index] / 0.01227F) * (JCO2 / JH2O);
                    data.NetPsn[index] = data.GrossPsn[index] - data.FolResp[index];
                }

                // Subtract transpiration from hydrology
                success = hydrology.AddWater(-1 * data.Transpiration[index], ecoregion.RootingDepth * frostFreeProp);
                if (success == false) throw new System.Exception("Error adding water, Transpiration = " + data.Transpiration[index] + " water = " + hydrology.Water + "; ecoregion = " + ecoregion.Name + "; site = " + site.Location);
                if (Hydrology.SurfaceWater > 0)
                {
                    float surfaceInput = Math.Min(Hydrology.SurfaceWater, (ecoregion.Porosity - hydrology.Water));
                    Hydrology.SurfaceWater -= surfaceInput;
                    success = hydrology.AddWater(surfaceInput, ecoregion.RootingDepth * frostFreeProp);
                    if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + Hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + ecoregion.Name + "; site = " + site.Location);
                }
                // Add net psn to non soluble carbons
                data.NSC += data.NetPsn[index];

            }
            else
            {
                // Reset subcanopy layer values
                data.NetPsn[index] = 0;
                data.FolResp[index] = 0;
                data.GrossPsn[index] = 0;
                data.Transpiration[index] = 0;
                data.FOzone[index] = 1;
            }

            if (index < PlugIn.IMAX - 1) index++;
            return success;
        }

        // Based on Michaelis-Menten saturation curve
        // https://en.wikibooks.org/wiki/Structural_Biochemistry/Enzyme/Michaelis_and_Menten_Equation
        public static float ComputeFrad(float Radiation, float HalfSat)
        {
            // Derived from Michaelis-Menton equation
            // https://en.wikibooks.org/wiki/Structural_Biochemistry/Enzyme/Michaelis_and_Menten_Equation

            return Radiation / (Radiation + HalfSat);
        }
        public static float ComputeFWater(float H1, float H2, float H3, float H4, float pressurehead)
        {
            float minThreshold = H1;
            if (H2 <= H1)
                minThreshold = H2;
            // Compute water stress
            if (pressurehead <= H1) return 0;
            else if (pressurehead < minThreshold || pressurehead >= H4) return 0;
            else if (pressurehead > H3) return 1 - ((pressurehead - H3) / (H4 - H3));
            else if (pressurehead < H2) return (1.0F/(H2-H1))*pressurehead - (H1/(H2-H1));
            else return 1;
        }

        public static float ComputeO3Effect_PnET(float o3, float delAmax, float netPsn_leaf_s, int Layer, int nLayers, float FolMass, float lastO3Effect, float gwv, float layerLAI, float o3Coeff)
        {
            float currentO3Effect = 1.0F;
            float droughtO3Frac = 1.0F; // Not using droughtO3Frac from PnET code per M. Kubiske and A. Chappelka
            //float kO3Eff = 0.0026F;  // Generic coefficient from Ollinger
            float kO3Eff = 0.0026F * o3Coeff;  // Scaled by speciesPnET using input parameters
            

            float O3Prof = (float)(0.6163 + (0.00105 * FolMass));
            float RelLayer = (float)Layer / (float)nLayers;
            float relO3 = Math.Min(1,1 - (RelLayer * O3Prof) * (RelLayer * O3Prof) * (RelLayer * O3Prof));
            // Kubiske method (using gwv in place of conductance
            currentO3Effect = (float)Math.Min(1, (lastO3Effect * droughtO3Frac) + (kO3Eff * gwv * o3 * relO3));

            // Ollinger method
            // Calculations for gsSlope and gsInt could be moved back to EcoregionPnETVariables since they only depend on delamax
            //float gsSlope=(float)((-1.1309*delAmax)+1.9762);
            //float gsInt = (float)((0.4656 * delAmax) - 0.9701);
            //float conductance = Math.Max(0, (gsInt + (gsSlope * netPsn_leaf_s)) * (1 - lastO3Effect));
            //float currentO3Effect_conductance = (float)Math.Min(1, (lastO3Effect * droughtO3Frac) + (kO3Eff * conductance * o3 * relO3));

            // Tested here but removed for release v3.0
            //string OzoneConductance = ((Parameter<string>)PlugIn.GetParameter(Names.OzoneConductance)).Value;
            //if (OzoneConductance == "Kubiske")
            //    return currentO3Effect;
            //else if (OzoneConductance == "Ollinger")
            //    return currentO3Effect_conductance;
            //else
            //{
            //    System.Console.WriteLine("OzoneConductance is not Kubiske or Ollinger.  Using Kubiske by default");
            //    return currentO3Effect;
            //}

            return currentO3Effect;
            
        }

        public int ComputeNonWoodyBiomass(ActiveSite site)
        {
            return (int)(Fol);
        }

        public static Percentage ComputeNonWoodyPercentage(Cohort cohort, ActiveSite site)
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

        /// <summary>
        /// Occurs when a cohort is killed by an age-only disturbance.
        /// </summary>
        public static event DeathEventHandler<DeathEventArgs> AgeOnlyDeathEvent;

        /// <summary>
        /// Occurs when a cohort dies either due to senescence or biomass
        /// disturbances.
        /// </summary>
        public static event DeathEventHandler<DeathEventArgs> DeathEvent;

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

        public void UpdateCohortData(IEcoregionPnETVariables monthdata )
        {
            float netPsnSum = data.NetPsn.Sum();
            float transpirationSum = data.Transpiration.Sum();
            float JCO2_JH2O = 0;
            if(transpirationSum > 0)
                JCO2_JH2O = (float) (0.01227 * (netPsnSum / transpirationSum));
            float WUE = JCO2_JH2O * ((float)44 / (float)18); //44=mol wt CO2; 18=mol wt H2O; constant =2.44444444444444

            // determine the limiting factor 
            float fWaterAvg = data.FWater.Average();
            float PressHeadAvg = data.PressHead.Average();
            float fRadAvg = data.FRad.Average();
            float fOzoneAvg = data.FOzone.Average();
            float fTemp = monthdata[Species.Name].FTempPSN;
            string limitingFactor = "NA";
            if(data.ColdKill < int.MaxValue)
            {
                limitingFactor = "ColdTol ("+ data.ColdKill.ToString()+")";
            }
            else
            {
               List<float> factorList = new List<float>(new float[]{fWaterAvg,fRadAvg,fOzoneAvg,Fage,fTemp});
               float minFactor = factorList.Min();
                if (minFactor == fTemp)
                    limitingFactor = "fTemp";
                else if (minFactor == Fage)
                    limitingFactor = "fAge";
                else if (minFactor == fWaterAvg)
                {
                    if(PressHeadAvg > this.SpeciesPNET.H3)
                    {
                        limitingFactor = "Too dry";
                    }
                    else if (PressHeadAvg < this.SpeciesPNET.H2)
                    {
                        limitingFactor = "Too wet";
                    }
                    else
                        limitingFactor = "fWater";
                }
                else if (minFactor == fRadAvg)
                    limitingFactor = "fRad";
                else if (minFactor == fOzoneAvg)
                    limitingFactor = "fOzone";
            }

            // Cohort output file
            string s = Math.Round(monthdata.Year, 2) + "," +
                       Age + "," +
                       data.Layer + "," +
                       SumLAI + "," +
                       data.GrossPsn.Sum() + "," +
                       data.FolResp.Sum() + "," +
                       data.MaintenanceRespiration.Sum() + "," +
                       netPsnSum + "," +                  // Sum over canopy layers
                       transpirationSum + "," +
                       WUE.ToString() + "," +
                       Fol + "," +
                       Root + "," +
                       Wood + "," +
                       NSC + "," +
                       NSCfrac + "," +
                       fWaterAvg + "," +
                       data.Water.Average() + ","+
                       data.PressHead.Average() + ","+
                       fRadAvg + "," +
                       fOzoneAvg + "," +
                       data.DelAmax.Average() + "," +
                       monthdata[Species.Name].FTempPSN + "," +
                       monthdata[Species.Name].FTempRespWeightedDayAndNight + "," +
                       Fage + "," +
                       data.Leaf_On + "," +
                       FActiveBiom + "," +
                       data.AdjFolN.Average() + "," +
                       data.AdjFracFol.Average() + "," +
                       data.CiModifier.Average() + ","+
                       data.AdjHalfSat + ","+
                       limitingFactor+",";
             
            cohortoutput.Add(s);

       
        }

        public string OutputHeader
        {
            get
            { 
                // Cohort output file header
                string hdr = OutputHeaders.Time + "," +
                            OutputHeaders.Age + "," +
                            OutputHeaders.Layer + "," +
                            OutputHeaders.LAI + "," +
                            OutputHeaders.GrossPsn + "," +
                            OutputHeaders.FolResp + "," +
                            OutputHeaders.MaintResp + "," +
                            OutputHeaders.NetPsn + "," +
                            OutputHeaders.Transpiration + "," +
                            OutputHeaders.WUE + "," +
                            OutputHeaders.Fol + "," +
                            OutputHeaders.Root + "," +
                            OutputHeaders.Wood + "," +
                            OutputHeaders.NSC + "," +
                            OutputHeaders.NSCfrac + "," +
                            OutputHeaders.fWater + "," +
                            OutputHeaders.water + "," +
                            OutputHeaders.PressureHead + ","+
                            OutputHeaders.fRad + "," +
                            OutputHeaders.FOzone + "," +
                            OutputHeaders.DelAMax + "," +
                            OutputHeaders.fTemp_psn + "," +
                            OutputHeaders.fTemp_resp + "," +
                            OutputHeaders.fage + "," +
                            OutputHeaders.LeafOn + "," +
                            OutputHeaders.FActiveBiom + "," +
                            OutputHeaders.AdjFolN + "," +
                            OutputHeaders.AdjFracFol + "," +
                            OutputHeaders.CiModifier + ","+
                            OutputHeaders.AdjHalfSat + ","+
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
            data.Fol -= Litter;

            return Litter;
        }
        
        public float Senescence()
        {
            float senescence = ((Root * speciesPnET.TOroot) + Wood * speciesPnET.TOwood);
            data.Biomass -= senescence;

            return senescence;
        }

        public void ReduceFoliage(double fraction)
        {
            data.Fol *= (float)(1.0 - fraction);
        }

        public void ReduceBiomass(object sitecohorts, double fraction, ExtensionType disturbanceType)
        {
            Allocation.Allocate(sitecohorts, this, disturbanceType, fraction);

            data.Biomass *= (float)(1.0 - fraction);
            data.Fol *= (float)(1.0 - fraction);

        }

        public float CalculateLAI(ISpeciesPnET speciesPnET, float fol, int index)
        {
            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            float LAISum = 0;
            for (int i = 0; i < index; i++)
            {
                LAISum += data.LAI[i];
            }
            float LAIlayerMax = (float)Math.Max(0.01, 25.0F - LAISum); // Cohort LAI is capped at 25; once LAI reaches 25 subsequent sublayers get LAI of 0.01
            float LAIlayer = (1 / (float)PlugIn.IMAX) * fol / (speciesPnET.SLWmax - speciesPnET.SLWDel * index * (1 / (float)PlugIn.IMAX) * fol);
            if (fol > 0 && LAIlayer <= 0)
            {
                PlugIn.ModelCore.UI.WriteLine("\n Warning: LAI was calculated to be negative for " + speciesPnET.Name + ". This could be caused by a low value for SLWmax.  LAI applied in this case is a max of 25 for each cohort.");
                LAIlayer = LAIlayerMax/(PlugIn.IMAX - index);
            }
            else
                LAIlayer = (float)Math.Min(LAIlayerMax, LAIlayer);

            return LAIlayer;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Raises a Cohort.AgeOnlyDeathEvent.
        /// </summary>
        public static void RaiseDeathEvent(object sender,
                                Cohort cohort, 
                                ActiveSite site,
                                ExtensionType disturbanceType)
        {
            //if (AgeOnlyDeathEvent != null)
            //{
            //    AgeOnlyDeathEvent(sender, new Landis.Library.BiomassCohorts.DeathEventArgs(cohort, site, disturbanceType));
            //}
            if (DeathEvent != null)
            {
                DeathEvent(sender, new Landis.Library.PnETCohorts.DeathEventArgs(cohort, site, disturbanceType));
            }

        }

        
    } 
}

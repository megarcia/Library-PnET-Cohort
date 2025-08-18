using Landis.Core;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// The information for a tree species (its index and parameters).
    /// </summary>
    public class PnETSpecies : IPnETSpecies
    {
        static List<Tuple<ISpecies, IPnETSpecies>> SpeciesCombinations;

        #region private variables
        private float _halfSatFCO2;
        private float _cfracbiomass;
        private float _woodlitterdecomprate;
        private float _dnsc;
        private float _bgbiomassfrac;
        private float _folbiomassfrac;
        private float _liveWoodBiomassFrac;
        private float _photosynthesisfage;
        private float _h1;
        private float _h2;
        private float _h3;
        private float _h4;
        private float _slwdel;
        private float _slwmax;
        private float _folturnoverrate;
        private float _rootturnoverrate;
        private float _halfsat;
        private float _initialnsc;
        private float _k;
        private float _woodturnoverrate;
        private float _estrad;
        private float _estmoist;
        private float _maxPest;
        private float _follignin;
        private bool _preventestablishment;
        private float _psntopt;
        private float _q10;
        private float _psntmin;
        private float _psntmax;
        private float _dvpd1;
        private float _foln;
        private float _dvpd2;
        private float _amaxa;
        private float _amaxb;
        private float _amaxamod;
        private float _aMaxBFCO2;
        private float _maintresp;
        private float _baseFoliarRespiration;
        private string _ozoneSens;
        private float _coldTol;
        private int _initBiomass;
        private string name;
        private int index;        
        private int  maxSproutAge;
        private int minSproutAge;
        private Landis.Core.PostFireRegeneration postfireregeneration;
        private int maxSeedDist;
        private int effectiveSeedDist;
        private float  vegReprodProb;
        private byte fireTolerance;
        private byte shadeTolerance;
        int maturity;
        int longevity;
        private float _folN_slope;
        private float _folN_intercept;
        private float _folBiomassFrac_slope;
        private float _folBiomassFrac_intercept;
        private float _o3Coeff;
        private float _leafOnMinT;
        private float _NSCreserve;
        private string _lifeform;
        private float _refoliationMinimumTrigger;
        private float _refoliationMaximum;
        private float _refoliationCost;
        private float _nonRefoliationCost;
        private float _maxLAI;
        private float _mossScalar;
        #endregion

        #region private static species variables
        private static Landis.Library.Parameters.Species.AuxParm<float> halfSatFCO2;
        private static Landis.Library.Parameters.Species.AuxParm<float> dnsc;
        private static Landis.Library.Parameters.Species.AuxParm<float> cfracbiomass;
        private static Landis.Library.Parameters.Species.AuxParm<float> woodlitterdecomprate;
        private static Landis.Library.Parameters.Species.AuxParm<float> bgbiomassfrac;
        private static Landis.Library.Parameters.Species.AuxParm<float> folbiomassfrac;
        private static Landis.Library.Parameters.Species.AuxParm<float> liveWoodBiomassFrac;
        private static Landis.Library.Parameters.Species.AuxParm<float> photosynthesisfage;
        private static Landis.Library.Parameters.Species.AuxParm<float> h1;
        private static Landis.Library.Parameters.Species.AuxParm<float> h2;
        private static Landis.Library.Parameters.Species.AuxParm<float> h3;
        private static Landis.Library.Parameters.Species.AuxParm<float> h4;
        private static Landis.Library.Parameters.Species.AuxParm<float> slwdel;
        private static Landis.Library.Parameters.Species.AuxParm<float> slwmax;    
        private static Landis.Library.Parameters.Species.AuxParm<float> folturnoverrate;
        private static Landis.Library.Parameters.Species.AuxParm<float> halfsat;
        private static Landis.Library.Parameters.Species.AuxParm<float> rootturnoverrate;
        private static Landis.Library.Parameters.Species.AuxParm<float> initialnsc;
        private static Landis.Library.Parameters.Species.AuxParm<float> k;
        private static Landis.Library.Parameters.Species.AuxParm<float> woodturnoverrate;
        private static Landis.Library.Parameters.Species.AuxParm<float> estrad;
        private static Landis.Library.Parameters.Species.AuxParm<float> estmoist;
        private static Landis.Library.Parameters.Species.AuxParm<float> maxPest;
        private static Landis.Library.Parameters.Species.AuxParm<float> follignin;
        private static Landis.Library.Parameters.Species.AuxParm<bool> preventestablishment;
        private static Landis.Library.Parameters.Species.AuxParm<float> psntopt;
        private static Landis.Library.Parameters.Species.AuxParm<float> q10;
        private static Landis.Library.Parameters.Species.AuxParm<float> psntmin;
        private static Landis.Library.Parameters.Species.AuxParm<float> psntmax;
        private static Landis.Library.Parameters.Species.AuxParm<float> dvpd1;
        private static Landis.Library.Parameters.Species.AuxParm<float> dvpd2;
        private static Landis.Library.Parameters.Species.AuxParm<float> foln;
        private static Landis.Library.Parameters.Species.AuxParm<float> amaxa;
        private static Landis.Library.Parameters.Species.AuxParm<float> amaxb;
        private static Landis.Library.Parameters.Species.AuxParm<float> amaxamod;
        private static Landis.Library.Parameters.Species.AuxParm<float> aMaxBFCO2;
        private static Landis.Library.Parameters.Species.AuxParm<float> maintresp;
        private static Landis.Library.Parameters.Species.AuxParm<float> baseFoliarRespiration;
        private static Landis.Library.Parameters.Species.AuxParm<float> coldTol;
        private static Landis.Library.Parameters.Species.AuxParm<string> ozoneSens;
        private static Landis.Library.Parameters.Species.AuxParm<float> folN_slope;
        private static Landis.Library.Parameters.Species.AuxParm<float> folN_intercept;
        private static Landis.Library.Parameters.Species.AuxParm<float> folBiomassFrac_slope;
        private static Landis.Library.Parameters.Species.AuxParm<float> folBiomassFrac_intercept;
        private static Landis.Library.Parameters.Species.AuxParm<float> o3Coeff;
        private static Landis.Library.Parameters.Species.AuxParm<float> leafOnMinT;
        private static Landis.Library.Parameters.Species.AuxParm<float> NSCreserve;
        private static Landis.Library.Parameters.Species.AuxParm<string> lifeform;
        private static Landis.Library.Parameters.Species.AuxParm<float> refoliationMinimumTrigger;
        private static Landis.Library.Parameters.Species.AuxParm<float> refoliationMaximum;
        private static Landis.Library.Parameters.Species.AuxParm<float> refoliationCost;
        private static Landis.Library.Parameters.Species.AuxParm<float> nonRefoliationCost;
        private static Landis.Library.Parameters.Species.AuxParm<float> maxlai;
        private static Landis.Library.Parameters.Species.AuxParm<float> mossScalar;
        private static Dictionary<ISpecies,float> maxLAI;
        private static Dictionary<ISpecies, string> lifeForm;
        #endregion

        public PnETSpecies()
        {
            #region initialize private static species variables
            halfSatFCO2 = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("HalfSatFCO2");
            dnsc = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("DNSC");
            cfracbiomass = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("CFracBiomass");
            woodlitterdecomprate = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("woodlitterdecomprate");
            bgbiomassfrac = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("bgbiomassfrac");
            folbiomassfrac = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("folbiomassfrac");
            liveWoodBiomassFrac = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("liveWoodBiomassFrac");
            photosynthesisfage = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("photosynthesisfage");
            h1 = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("h1");
            h2 = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("h2");
            h3 = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("h3");
            h4 = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("h4");
            slwdel = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("slwdel");
            slwmax = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("slwmax");
            folturnoverrate = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("folturnoverrate");
            halfsat = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("halfsat");
            rootturnoverrate = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("rootturnoverrate");
            initialnsc = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("initialnsc"); ;
            k = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("k"); ;
            woodturnoverrate = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("woodturnoverrate"); ;
            estrad = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("estrad"); ;
            estmoist = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("estmoist");
            maxPest = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("MaxPest");
            follignin = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("follignin");
            preventestablishment = (Landis.Library.Parameters.Species.AuxParm<bool>)(Parameter<bool>)Names.GetParameter("preventestablishment");
            psntopt = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("psntopt");
            q10 = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("q10");
            psntmin = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("psntmin");
            psntmax = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("psntmax");
            dvpd1 = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("dvpd1");
            dvpd2 = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("dvpd2");
            foln = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("foln");
            amaxa = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("amaxa");
            amaxb = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("amaxb");
            amaxamod = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("AmaxAmod");
            aMaxBFCO2 = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("AMaxBFCO2");
            maintresp = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("maintresp");
            baseFoliarRespiration = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("baseFoliarRespiration");
            ozoneSens = (Landis.Library.Parameters.Species.AuxParm<string>)(Parameter<string>)Names.GetParameter("StomataO3Sensitivity");
            folN_slope = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("FolN_slope");
            folN_intercept = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("FolN_intercept"); //Optional
            // If FolN_intercept is not provided, then set to foln
            if (folN_intercept[this] == -9999F)
                folN_intercept = foln;
            folBiomassFrac_slope = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("FolBiomassFrac_slope");
            folBiomassFrac_intercept = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("FolBiomassFrac_intercept"); //Optional
            // If FolBiomassFrac_intercept is not provided, then set to folbiomassfrac
            if (folBiomassFrac_intercept[this] == -9999F)
                folBiomassFrac_intercept = folbiomassfrac;
            o3Coeff = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("FOzone_slope");
            coldTol = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("ColdTol");
            leafOnMinT = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("LeafOnMinT"); //Optional
            // If LeafOnMinT is not provided, then set to PsnMinT
            if (leafOnMinT[this] == -9999F)
                leafOnMinT = psntmin;
            NSCreserve = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("NSCReserve");
            refoliationMinimumTrigger = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("RefolMinimumTrigger");
            refoliationMaximum = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("RefolMaximum");
            refoliationCost = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("RefolCost");
            nonRefoliationCost = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("NonRefolCost");
            maxlai = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("MaxLAI"); //Optional
            mossScalar = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("MossScalar"); //Optional
            maxLAI = new Dictionary<ISpecies, float>();
            foreach (ISpecies species in Globals.ModelCore.Species)
            {
                if (maxlai[species] == -9999F)
                {
                    // Calculate MaxLAI
                    float peakBiomass = 1f / liveWoodBiomassFrac[species];
                    float peakFoliage = peakBiomass * folbiomassfrac[species] * (float)Math.Exp(-1f * liveWoodBiomassFrac[species] * peakBiomass);
                    float tempLAI = 0;
                    for (int i = 0; i < Globals.IMAX; i++)
                    {
                        tempLAI += (float)Math.Max(0.01, peakFoliage / Globals.IMAX / (slwmax[species] - (slwdel[species] * i * (peakFoliage / Globals.IMAX))));
                    }
                    maxLAI.Add(species, tempLAI);
                }
                else
                    maxLAI.Add(species, maxlai[species]);
            }
            lifeform = (Landis.Library.Parameters.Species.AuxParm<string>)(Parameter<string>)Names.GetParameter("Lifeform");
            lifeForm = new Dictionary<ISpecies, string>();
            foreach (ISpecies species in Globals.ModelCore.Species)
            {
                if (lifeform != null && lifeform[species] != null && !string.IsNullOrEmpty(lifeform[species]))
                {
                    string[] matches = new string[2];
                    if (Names.HasMultipleMatches(lifeform[species], ref matches))
                        throw new System.Exception("LifeForm parameter " + lifeForm + " contains mutually exclusive terms: " + matches[0] + " and " + matches[1] + ".");
                    lifeForm.Add(species, lifeform[species]);
                }
                else
                    lifeForm.Add(species, "tree");
            }
            #endregion

            SpeciesCombinations = new List<Tuple<ISpecies, IPnETSpecies>>();
            foreach (ISpecies spc in Globals.ModelCore.Species)
            {
                PnETSpecies species = new PnETSpecies(spc);
                SpeciesCombinations.Add(new Tuple<ISpecies, IPnETSpecies>(spc, species));
            }
        }

        PnETSpecies(PostFireRegeneration postFireGeneration,
                    float dnsc, float cfracbiomass, float woodlitterdecomprate,
                    float bgbiomassfrac, float folbiomassfrac, float liveWoodBiomassFrac,
                    float photosynthesisfage, float h1, float h2, float h3,
                    float h4, float slwdel, float slwmax, float folturnoverrate,
                    float rootturnoverrate, float halfsat, float initialnsc,
                    float k, float woodturnoverrate, float estrad, float estmoist,
                    float maxPest, float follignin, bool preventestablishment,
                    float psntopt, float q10, float psntmin, float psntmax,
                    float dvpd1, float dvpd2, float foln, float amaxa,
                    float amaxb, float amaxamod, float aMaxBFCO2,
                    float maintresp, float baseFoliarRespiration, float coldTol,
                    string ozoneSens, int Index, string name,
                    int maxSproutAge, int minSproutAge, int maxSeedDist,
                    int effectiveSeedDist, float vegReprodProb,
                    byte fireTolerance, byte shadeTolerance, int maturity,
                    int longevity, float folN_slope, float folN_intercept,
                    float folBiomassFrac_slope, float folBiomassFrac_intercept, float o3Coeff,
                    float leafOnMinT, float NSCreserve, string lifeForm,
                    float refoliationMinimumTrigger, float refoliationMaximum,
                    float refoliationCost, float nonRefoliationCost,
                    float maxLAI)
        {
            this.postfireregeneration = postFireGeneration;
            this._dnsc = dnsc;
            this._cfracbiomass = cfracbiomass;
            this._woodlitterdecomprate = woodlitterdecomprate;
            this._bgbiomassfrac = bgbiomassfrac;
            this._folbiomassfrac = folbiomassfrac;
            this._liveWoodBiomassFrac = liveWoodBiomassFrac;
            this._photosynthesisfage = photosynthesisfage;
            this._h1 = h1;
            this._h2 = h2;
            this._h3 = h3;
            this._h4 = h4;
            this._slwdel = slwdel;
            this._slwmax = slwmax;
            this._folturnoverrate = folturnoverrate;
            this._rootturnoverrate = rootturnoverrate;
            this._halfsat = halfsat;
            this._initialnsc = initialnsc;
            this._k = k;
            this._woodturnoverrate = woodturnoverrate;
            this._estrad = estrad;
            this._estmoist = estmoist;
            this._maxPest = maxPest;
            this._follignin = follignin;
            this._preventestablishment = preventestablishment;
            this._psntopt = psntopt;
            this._q10 = q10;
            this._psntmin = psntmin;
            this._psntmax = psntmax;
            this._dvpd1 = dvpd1;
            this._foln = foln;
            this._dvpd2 = dvpd2;
            this._amaxa = amaxa;
            this._amaxb = amaxb;
            this._amaxamod = amaxamod;
            this._aMaxBFCO2 = aMaxBFCO2;
            this._maintresp = maintresp;
            this._baseFoliarRespiration = baseFoliarRespiration;
            this._coldTol = coldTol;
            this._ozoneSens = ozoneSens;
            this.index = Index;
            this.name = name;
            this.maxSproutAge = maxSproutAge;
            this.minSproutAge = minSproutAge;
            this.postfireregeneration = postFireGeneration;
            this.maxSeedDist = maxSeedDist;
            this.effectiveSeedDist = effectiveSeedDist;
            this.vegReprodProb = vegReprodProb;
            this.fireTolerance = fireTolerance;
            this.shadeTolerance = shadeTolerance;
            this.maturity = maturity;
            this.longevity = longevity;
            this._folN_slope = folN_slope;
            this._folN_intercept = folN_intercept;
            this._folBiomassFrac_slope = folBiomassFrac_slope;
            this._folBiomassFrac_intercept = folBiomassFrac_intercept;
            this._o3Coeff = o3Coeff;
            this._leafOnMinT = leafOnMinT;
            uint initBiomass = (uint)(initialnsc / (dnsc * cfracbiomass));
            this._initBiomass = (int)(initBiomass - ((uint)(bgbiomassfrac * initBiomass)) * rootturnoverrate - ((uint)((1 - bgbiomassfrac) * initBiomass) * woodturnoverrate));
            this._NSCreserve = NSCreserve;
            this._lifeform = lifeForm;
            this._refoliationMinimumTrigger = refoliationMinimumTrigger;
            this._refoliationMaximum = refoliationMaximum;
            this._refoliationCost = refoliationCost;
            this._nonRefoliationCost = nonRefoliationCost;
            this._maxLAI = maxLAI;
        }

        private PnETSpecies(ISpecies species)
        {
            uint initBiomass = (uint)(initialnsc[species] / (dnsc[species] * cfracbiomass[species]));
            _initBiomass = (int)(initBiomass - ((uint)(bgbiomassfrac[species] * initBiomass)) * rootturnoverrate[species] - ((uint)((1 - bgbiomassfrac[species]) * initBiomass) * woodturnoverrate[species]));
            _dnsc = dnsc[species];
            _cfracbiomass = cfracbiomass[species];
            _woodlitterdecomprate = woodlitterdecomprate[species];
            _bgbiomassfrac = bgbiomassfrac[species];
            _folbiomassfrac = folbiomassfrac[species];
            _liveWoodBiomassFrac = liveWoodBiomassFrac[species];
            _photosynthesisfage = photosynthesisfage[species];
            _h1 = h1[species];
            _h2 = h2[species];
            _h3 = h3[species];
            _h4 = h4[species];
            _slwdel = slwdel[species];
            _slwmax = slwmax[species];
            _folturnoverrate = folturnoverrate[species];
            _rootturnoverrate = rootturnoverrate[species];
            _halfsat = halfsat[species];
            _initialnsc = initialnsc[species];
            _k = k[species];
            _woodturnoverrate = woodturnoverrate[species];
            _estrad = estrad[species];
            _estmoist = estmoist[species];
            _maxPest = maxPest[species];
            _follignin = follignin[species];
            _preventestablishment = preventestablishment[species];
            _psntopt = psntopt[species];
            _q10 = q10[species];
            _psntmin = psntmin[species];
            _psntmax = psntmax[species];
            _dvpd1 = dvpd1[species];
            _foln = foln[species];
            _dvpd2 = dvpd2[species];
            _amaxa = amaxa[species];
            _amaxb = amaxb[species];
            _amaxamod = amaxamod[species];
            _aMaxBFCO2 = aMaxBFCO2[species];
            _maintresp = maintresp[species];
            _baseFoliarRespiration = baseFoliarRespiration[species];
            _coldTol = coldTol[species];
            _halfSatFCO2 = halfSatFCO2[species];
            _ozoneSens = ozoneSens[species];
            _NSCreserve = NSCreserve[species];
            _lifeform = lifeForm[species];
            _refoliationMinimumTrigger = refoliationMinimumTrigger[species];
            _refoliationMaximum = refoliationMaximum[species];
            _refoliationCost = refoliationCost[species];
            _nonRefoliationCost = nonRefoliationCost[species];
            _maxLAI = maxLAI[species];
            index = species.Index;
            name = species.Name;
            _mossScalar = mossScalar[species];
            maxSproutAge = species.MaxSproutAge;
            minSproutAge = species.MinSproutAge;
            postfireregeneration = species.PostFireRegeneration;
            maxSeedDist = species.MaxSeedDist;
            effectiveSeedDist = species.EffectiveSeedDist;
            vegReprodProb = species.VegReprodProb;
            maturity = species.Maturity;
            longevity = species.Longevity;
            _folN_slope = folN_slope[species];
            _folN_intercept = folN_intercept[species];
            _folBiomassFrac_slope = folBiomassFrac_slope[species];
            _folBiomassFrac_intercept = folBiomassFrac_intercept[species];
            _o3Coeff = o3Coeff[species];
            _leafOnMinT = leafOnMinT[species];
        }

        #region Accessors
        public List<IPnETSpecies> AllSpecies
        {
            get
            {
                return SpeciesCombinations.Select(combination => combination.Item2).ToList();
            }
        }

        public IPnETSpecies this[ISpecies species]
        {
            get
            {
                return SpeciesCombinations.Where(spc => spc.Item1 == species).First().Item2;
            }
        }

        public ISpecies this[IPnETSpecies species]
        {
            get
            {
                return SpeciesCombinations.Where(spc => spc.Item2 == species).First().Item1;
            }
        }

        public int Index
        {
            get
            {
                return index;
            }
        }

        public float BaseFoliarRespiration
        {
            get
            {
                return _baseFoliarRespiration;
            }
        }

        public float ColdTol
        {
            get
            {
                return _coldTol;
            }
        }

        public float AmaxA
        {
            get
            {
                return _amaxa;
            }
        }

        public float AmaxB
        {
            get
            {
                return _amaxb;
            }
        }

        public float AmaxAmod
        {
            get
            {
                return _amaxamod;
            }
        }

        public float AMaxBFCO2
        {
            get
            {
                return _aMaxBFCO2;
            }
        }

        public float MaintResp
        {
            get
            {
                return _maintresp;
            }
        }

        public float PsnTmin
        {
            get
            {
                return _psntmin;
            }
        }

        public float PsnTmax
        {
            get
            {
                if (_psntmax == -9999F)
                    return _psntopt + (_psntopt - _psntmin);
                else
                    return _psntmax;
            }
        }

        public float DVPD1
        {
            get
            {
                return _dvpd1;
            }
        }

        public float FolN
        {
            get
            {
                return _foln;
            }
        }

        public float DVPD2
        {
            get
            {
                return _dvpd2;
            }

        }

        public float PsnTopt
        {
            get
            {
                return _psntopt;
            }
        }

        public float Q10
        {
            get
            {
                return _q10;
            }
        }

        public float EstRad
        {
            get
            {
                return _estrad;
            }
        }

        public bool PreventEstablishment
        {
            get
            {
                return _preventestablishment;
            }
        }

        public float FolLignin
        {
            get
            {
                return _follignin;
            }
        }

        public float EstMoist
        {
            get
            {
                return _estmoist;
            }
        }

        public float MaxPest
        {
            get
            {
                return _maxPest;
            }
        }

        public float WoodTurnoverRate
        {
            get
            {
                return _woodturnoverrate;
            }
        }

        public float K
        {
            get
            {
                return _k;
            }
        }

        public float InitialNSC
        {
            get
            {
                return _initialnsc;
            }
        }

        public float HalfSat
        {
            get
            {
                return _halfsat;
            }
        }

        public float RootTurnoverRate
        {
            get
            {
                return _rootturnoverrate;
            }
        }

        public float FolTurnoverRate
        {
            get
            {
                return _folturnoverrate;
            }
        }

        public float SLWDel
        {
            get
            {
                return _slwdel;
            }
        }

        public float SLWmax
        {
            get
            {
                return _slwmax;
            }
        }

        public float H4
        {
            get
            {
                return _h4;
            }
        }

        public float H3
        {
            get
            {
                return _h3;
            }
        }

        public float H2
        {
            get
            {
                return _h2;
            }
        }

        public float H1
        {
            get
            {
                return _h1;
            }
        }

        public float PhotosynthesisFAge
        {
            get
            {
                return _photosynthesisfage;
            }
        }

        public float WoodLitterDecompRate
        {
            get
            {
                return _woodlitterdecomprate;
            }
        }

        public float LiveWoodBiomassFrac
        {
            get
            {
                return _liveWoodBiomassFrac;
            }
        }

        public float FolBiomassFrac
        {
            get
            {
                return _folbiomassfrac;
            }
        }

        public float BGBiomassFrac
        {
            get
            {
                return _bgbiomassfrac;
            }
        }

        public float DNSC
        {
            get
            {
                return _dnsc;
            }
        }

        public int InitBiomass
        {
            get
            {
                return _initBiomass;
            }
        }

        public float CFracBiomass
        {
            get
            {
                return _cfracbiomass;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }
        public int MaxSproutAge
        {
            get
            {
                return maxSproutAge;
            }
        }

        public int MinSproutAge
        {
            get
            {
                return minSproutAge;
            }
        }

        public float HalfSatFCO2
        {
            get
            {
                return _halfSatFCO2;
            }
        }

        public Landis.Core.PostFireRegeneration PostFireRegeneration
        {
            get
            {
                return postfireregeneration;
            }
        }

        public int MaxSeedDist
        {
            get
            {
                return maxSeedDist;
            }
        }

        public int EffectiveSeedDist
        {
            get
            {
                return effectiveSeedDist;
            }
        }

        public float VegReprodProb
        {
            get
            {
                return vegReprodProb;
            }
        }

        public byte FireTolerance
        {
            get
            {
                return fireTolerance;
            }
        }

        public byte ShadeTolerance
        {
            get
            {
                return shadeTolerance;
            }
        }

        public int Maturity
        {
            get
            {
                return maturity;
            }
        }

        public int Longevity
        {
            get
            {
                return longevity;
            }
        }

        public string StomataO3Sensitivity
        {
            get
            {
                return _ozoneSens;
            }
        }

        public float FolN_slope
        {
            get
            {
                return _folN_slope;
            }
        }

        public float FolN_intercept
        {
            get
            {
                return _folN_intercept;
            }
        }

        public float FolBiomassFrac_slope
        {
            get
            {
                return _folBiomassFrac_slope;
            }
        }

        public float FolBiomassFrac_intercept
        {
            get
            {
                return _folBiomassFrac_intercept;
            }
        }

        public float FOzone_slope
        {
            get
            {
                return _o3Coeff;
            }
        }

        public float LeafOnMinT
        {
            get
            {
                return _leafOnMinT;
            }
        }

        public float NSCReserve
        {
            get
            {
                return _NSCreserve;
            }
        }

        public string Lifeform
        {
            get
            {
                return _lifeform;
            }
        }

        public float RefoliationMinimumTrigger
        {
            get
            {
                return _refoliationMinimumTrigger;
            }
        }

        public float RefoliationMaximum
        {
            get
            {
                return _refoliationMaximum;
            }
        }

        public float RefoliationCost
        {
            get
            {
                return _refoliationCost;
            }
        }

        public float NonRefoliationCost
        {
            get
            {
                return _nonRefoliationCost;
            }
        }

        public float MaxLAI
        {
            get
            {
                return _maxLAI;
            }
        }

        public float MossScalar
        {
            get
            {
                // If mossScalar not provided, set to zero
                if (mossScalar[this] == -9999)
                    return 0;
                return _mossScalar;
            }
        }

        public static List<string> ParameterNames
        {
            get
            {
                System.Type type = typeof(PnETSpecies); // Get type pointer
                List<string> names = type.GetProperties().Select(x => x.Name).ToList(); // Obtain all fields
                return names;
            }
        }

        public string FullName
        {
            get;
            set;
        }
        #endregion
    }
}

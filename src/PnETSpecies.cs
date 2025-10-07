// NOTE: ISpecies --> Landis.Core
// NOTE: PostFireRegeneration --> Landis.Core

using System;
using System.Collections.Generic;
using System.Linq;
using Landis.Core;
using Landis.Library.Parameters;

namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// The information for a tree species (its index and parameters).
    /// </summary>
    public class PnETSpecies : IPnETSpecies
    {
        static List<Tuple<ISpecies, IPnETSpecies>> SpeciesCombinations;
        private float _halfSatFCO2;
        private float _cfracbiomass;
        private float _wooddebrisdecomprate;
        private float _nscfrac;
        private float _bgbiomassfrac;
        private float _agbiomassfrac;
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
        private float _establishmentfrad;
        private float _establishmentfwater;
        private float _maxProbEstablishment;
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
        private float _coldTolerance;
        private int _initBiomass;
        private string name;
        private int index;        
        private int maxSproutAge;
        private int minSproutAge;
        private PostFireRegeneration postfireregeneration;
        private int maxSeedDist;
        private int effectiveSeedDist;
        private float vegReprodProb;
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
        private float _maxRefoliationFrac;
        private float _refoliationCost;
        private float _nonRefoliationCost;
        private float _maxLAI;
        private float _mossScalar;
        private static Library.Parameters.Species.AuxParm<float> halfSatFCO2;
        private static Library.Parameters.Species.AuxParm<float> nscfrac;
        private static Library.Parameters.Species.AuxParm<float> cfracbiomass;
        private static Library.Parameters.Species.AuxParm<float> wooddebrisdecomprate;
        private static Library.Parameters.Species.AuxParm<float> bgbiomassfrac;
        private static Library.Parameters.Species.AuxParm<float> agbiomassfrac;
        private static Library.Parameters.Species.AuxParm<float> folbiomassfrac;
        private static Library.Parameters.Species.AuxParm<float> liveWoodBiomassFrac;
        private static Library.Parameters.Species.AuxParm<float> photosynthesisfage;
        private static Library.Parameters.Species.AuxParm<float> h1;
        private static Library.Parameters.Species.AuxParm<float> h2;
        private static Library.Parameters.Species.AuxParm<float> h3;
        private static Library.Parameters.Species.AuxParm<float> h4;
        private static Library.Parameters.Species.AuxParm<float> slwdel;
        private static Library.Parameters.Species.AuxParm<float> slwmax;    
        private static Library.Parameters.Species.AuxParm<float> folturnoverrate;
        private static Library.Parameters.Species.AuxParm<float> halfsat;
        private static Library.Parameters.Species.AuxParm<float> rootturnoverrate;
        private static Library.Parameters.Species.AuxParm<float> initialnsc;
        private static Library.Parameters.Species.AuxParm<float> k;
        private static Library.Parameters.Species.AuxParm<float> woodturnoverrate;
        private static Library.Parameters.Species.AuxParm<float> establishmentfrad;
        private static Library.Parameters.Species.AuxParm<float> establishmentfwater;
        private static Library.Parameters.Species.AuxParm<float> maxProbEstablishment;
        private static Library.Parameters.Species.AuxParm<float> follignin;
        private static Library.Parameters.Species.AuxParm<bool> preventestablishment;
        private static Library.Parameters.Species.AuxParm<float> psntopt;
        private static Library.Parameters.Species.AuxParm<float> q10;
        private static Library.Parameters.Species.AuxParm<float> psntmin;
        private static Library.Parameters.Species.AuxParm<float> psntmax;
        private static Library.Parameters.Species.AuxParm<float> dvpd1;
        private static Library.Parameters.Species.AuxParm<float> dvpd2;
        private static Library.Parameters.Species.AuxParm<float> foln;
        private static Library.Parameters.Species.AuxParm<float> amaxa;
        private static Library.Parameters.Species.AuxParm<float> amaxb;
        private static Library.Parameters.Species.AuxParm<float> amaxamod;
        private static Library.Parameters.Species.AuxParm<float> aMaxBFCO2;
        private static Library.Parameters.Species.AuxParm<float> maintresp;
        private static Library.Parameters.Species.AuxParm<float> baseFoliarRespiration;
        private static Library.Parameters.Species.AuxParm<float> coldTolerance;
        private static Library.Parameters.Species.AuxParm<string> ozoneSens;
        private static Library.Parameters.Species.AuxParm<float> folN_slope;
        private static Library.Parameters.Species.AuxParm<float> folN_intercept;
        private static Library.Parameters.Species.AuxParm<float> folBiomassFrac_slope;
        private static Library.Parameters.Species.AuxParm<float> folBiomassFrac_intercept;
        private static Library.Parameters.Species.AuxParm<float> o3Coeff;
        private static Library.Parameters.Species.AuxParm<float> leafOnMinT;
        private static Library.Parameters.Species.AuxParm<float> NSCreserve;
        private static Library.Parameters.Species.AuxParm<string> lifeform;
        private static Library.Parameters.Species.AuxParm<float> refoliationMinimumTrigger;
        private static Library.Parameters.Species.AuxParm<float> maxRefoliationFrac;
        private static Library.Parameters.Species.AuxParm<float> refoliationCost;
        private static Library.Parameters.Species.AuxParm<float> nonRefoliationCost;
        private static Library.Parameters.Species.AuxParm<float> maxlai;
        private static Library.Parameters.Species.AuxParm<float> mossScalar;
        private static Dictionary<ISpecies,float> maxLAI;
        private static Dictionary<ISpecies, string> lifeForm;

        public PnETSpecies()
        {
            halfSatFCO2 = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("HalfSatFCO2");
            nscfrac = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("NSCFrac");
            cfracbiomass = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("CFracBiomass");
            wooddebrisdecomprate = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("wooddebrisdecomprate");
            bgbiomassfrac = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("bgbiomassfrac");
            agbiomassfrac = (Library.Parameters.Species.AuxParm<float>)(1.0F - (float)bgbiomassfrac);
            folbiomassfrac = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("folbiomassfrac");
            liveWoodBiomassFrac = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("liveWoodBiomassFrac");
            photosynthesisfage = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("photosynthesisfage");
            h1 = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("h1");
            h2 = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("h2");
            h3 = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("h3");
            h4 = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("h4");
            slwdel = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("slwdel");
            slwmax = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("slwmax");
            folturnoverrate = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("folturnoverrate");
            halfsat = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("halfsat");
            rootturnoverrate = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("rootturnoverrate");
            initialnsc = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("initialnsc"); ;
            k = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("k"); ;
            woodturnoverrate = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("woodturnoverrate"); ;
            establishmentfrad = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("establishmentfrad"); ;
            establishmentfwater = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("establishmentfwater");
            maxProbEstablishment = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("MaxProbEstablishment");
            follignin = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("follignin");
            preventestablishment = (Library.Parameters.Species.AuxParm<bool>)(Parameter<bool>)Names.GetParameter("preventestablishment");
            psntopt = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("psntopt");
            q10 = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("q10");
            psntmin = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("psntmin");
            psntmax = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("psntmax");
            dvpd1 = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("dvpd1");
            dvpd2 = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("dvpd2");
            foln = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("foln");
            amaxa = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("amaxa");
            amaxb = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("amaxb");
            amaxamod = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("AmaxAmod");
            aMaxBFCO2 = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("AMaxBFCO2");
            maintresp = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("maintresp");
            baseFoliarRespiration = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("baseFoliarRespiration");
            ozoneSens = (Library.Parameters.Species.AuxParm<string>)(Parameter<string>)Names.GetParameter("StomataO3Sensitivity");
            folN_slope = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("FolN_slope");
            folN_intercept = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("FolN_intercept"); //Optional
            // If FolN_intercept is not provided, then set to foln
            if (folN_intercept[this] == -9999F)
                folN_intercept = foln;
            folBiomassFrac_slope = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("FolBiomassFrac_slope");
            folBiomassFrac_intercept = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("FolBiomassFrac_intercept"); //Optional
            // If FolBiomassFrac_intercept is not provided, then set to folbiomassfrac
            if (folBiomassFrac_intercept[this] == -9999F)
                folBiomassFrac_intercept = folbiomassfrac;
            o3Coeff = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("FOzone_slope");
            coldTolerance = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("ColdTolerance");
            leafOnMinT = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("LeafOnMinT"); //Optional
            // If LeafOnMinT is not provided, then set to PsnMinT
            if (leafOnMinT[this] == -9999F)
                leafOnMinT = psntmin;
            NSCreserve = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("NSCReserve");
            refoliationMinimumTrigger = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("RefolMinimumTrigger");
            maxRefoliationFrac = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("RefolMaximum");
            refoliationCost = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("RefolCost");
            nonRefoliationCost = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("NonRefolCost");
            maxlai = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("MaxLAI"); //Optional
            mossScalar = (Library.Parameters.Species.AuxParm<float>)(Parameter<float>)Names.GetParameter("MossScalar"); //Optional
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
                        tempLAI += (float)Math.Max(0.01, peakFoliage / Globals.IMAX / (slwmax[species] - (slwdel[species] * i * (peakFoliage / Globals.IMAX))));
                    maxLAI.Add(species, tempLAI);
                }
                else
                    maxLAI.Add(species, maxlai[species]);
            }
            lifeform = (Library.Parameters.Species.AuxParm<string>)(Parameter<string>)Names.GetParameter("Lifeform");
            lifeForm = new Dictionary<ISpecies, string>();
            foreach (ISpecies species in Globals.ModelCore.Species)
            {
                if (lifeform != null && lifeform[species] != null && !string.IsNullOrEmpty(lifeform[species]))
                {
                    string[] matches = new string[2];
                    if (Names.HasMultipleMatches(lifeform[species], ref matches))
                        throw new Exception("LifeForm parameter " + lifeForm + " contains mutually exclusive terms: " + matches[0] + " and " + matches[1] + ".");
                    lifeForm.Add(species, lifeform[species]);
                }
                else
                    lifeForm.Add(species, "tree");
            }
            SpeciesCombinations = new List<Tuple<ISpecies, IPnETSpecies>>();
            foreach (ISpecies species in Globals.ModelCore.Species)
            {
                PnETSpecies pnetspecies = new PnETSpecies(species);
                SpeciesCombinations.Add(new Tuple<ISpecies, IPnETSpecies>(species, pnetspecies));
            }
        }

        PnETSpecies(PostFireRegeneration postFireRegeneration,
                    float nscfrac, float cfracbiomass, float wooddebrisdecomprate,
                    float bgbiomassfrac, float folbiomassfrac, float liveWoodBiomassFrac,
                    float photosynthesisfage, float h1, float h2, float h3,
                    float h4, float slwdel, float slwmax, float folturnoverrate,
                    float rootturnoverrate, float halfsat, float initialnsc,
                    float k, float woodturnoverrate, float establishmentfrad, float establishmentfwater,
                    float maxprobestablishment, float follignin, bool preventestablishment,
                    float psntopt, float q10, float psntmin, float psntmax,
                    float dvpd1, float dvpd2, float foln, float amaxa,
                    float amaxb, float amaxamod, float aMaxBFCO2,
                    float maintresp, float baseFoliarRespiration, float coldTolerance,
                    string ozoneSens, int Index, string name,
                    int maxSproutAge, int minSproutAge, int maxSeedDist,
                    int effectiveSeedDist, float vegReprodProb,
                    byte fireTolerance, byte shadeTolerance, int maturity,
                    int longevity, float folN_slope, float folN_intercept,
                    float folBiomassFrac_slope, float folBiomassFrac_intercept, float o3Coeff,
                    float leafOnMinT, float NSCreserve, string lifeForm,
                    float refoliationMinimumTrigger, float maxRefoliationFrac,
                    float refoliationCost, float nonRefoliationCost,
                    float maxLAI)
        {
            float initBiomass = initialnsc / (nscfrac * cfracbiomass);
            _bgbiomassfrac = bgbiomassfrac;
            _agbiomassfrac = 1F - bgbiomassfrac;
            _initBiomass = (int)(initBiomass * (1F - (bgbiomassfrac * rootturnoverrate) - (_agbiomassfrac * woodturnoverrate)));
            _nscfrac = nscfrac;
            _cfracbiomass = cfracbiomass;
            _wooddebrisdecomprate = wooddebrisdecomprate;
            _folbiomassfrac = folbiomassfrac;
            _liveWoodBiomassFrac = liveWoodBiomassFrac;
            _photosynthesisfage = photosynthesisfage;
            _h1 = h1;
            _h2 = h2;
            _h3 = h3;
            _h4 = h4;
            _slwdel = slwdel;
            _slwmax = slwmax;
            _folturnoverrate = folturnoverrate;
            _rootturnoverrate = rootturnoverrate;
            _halfsat = halfsat;
            _initialnsc = initialnsc;
            _k = k;
            _woodturnoverrate = woodturnoverrate;
            _establishmentfrad = establishmentfrad;
            _establishmentfwater = establishmentfwater;
            _maxProbEstablishment = maxprobestablishment;
            _follignin = follignin;
            _preventestablishment = preventestablishment;
            _psntopt = psntopt;
            _q10 = q10;
            _psntmin = psntmin;
            _psntmax = psntmax;
            _dvpd1 = dvpd1;
            _foln = foln;
            _dvpd2 = dvpd2;
            _amaxa = amaxa;
            _amaxb = amaxb;
            _amaxamod = amaxamod;
            _aMaxBFCO2 = aMaxBFCO2;
            _maintresp = maintresp;
            _baseFoliarRespiration = baseFoliarRespiration;
            _coldTolerance = coldTolerance;
            _ozoneSens = ozoneSens;
            _folN_slope = folN_slope;
            _folN_intercept = folN_intercept;
            _folBiomassFrac_slope = folBiomassFrac_slope;
            _folBiomassFrac_intercept = folBiomassFrac_intercept;
            _o3Coeff = o3Coeff;
            _leafOnMinT = leafOnMinT;
            _NSCreserve = NSCreserve;
            _lifeform = lifeForm;
            _refoliationMinimumTrigger = refoliationMinimumTrigger;
            _maxRefoliationFrac = maxRefoliationFrac;
            _refoliationCost = refoliationCost;
            _nonRefoliationCost = nonRefoliationCost;
            _maxLAI = maxLAI;
            index = Index;
            postfireregeneration = postFireRegeneration;
            this.name = name;
            this.maxSproutAge = maxSproutAge;
            this.minSproutAge = minSproutAge;
            this.maxSeedDist = maxSeedDist;
            this.effectiveSeedDist = effectiveSeedDist;
            this.vegReprodProb = vegReprodProb;
            this.fireTolerance = fireTolerance;
            this.shadeTolerance = shadeTolerance;
            this.maturity = maturity;
            this.longevity = longevity;
        }

        private PnETSpecies(ISpecies species)
        {
            float initBiomass = initialnsc[species] / (nscfrac[species] * cfracbiomass[species]);
            _bgbiomassfrac = bgbiomassfrac[species];
            _agbiomassfrac = 1F - bgbiomassfrac[species];
            _initBiomass = (int)(initBiomass * (1F - (bgbiomassfrac[species] * rootturnoverrate[species]) - (_agbiomassfrac * woodturnoverrate[species])));
            _nscfrac = nscfrac[species];
            _cfracbiomass = cfracbiomass[species];
            _wooddebrisdecomprate = wooddebrisdecomprate[species];
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
            _establishmentfrad = establishmentfrad[species];
            _establishmentfwater = establishmentfwater[species];
            _maxProbEstablishment = maxProbEstablishment[species];
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
            _coldTolerance = coldTolerance[species];
            _halfSatFCO2 = halfSatFCO2[species];
            _ozoneSens = ozoneSens[species];
            _NSCreserve = NSCreserve[species];
            _lifeform = lifeForm[species];
            _refoliationMinimumTrigger = refoliationMinimumTrigger[species];
            _maxRefoliationFrac = maxRefoliationFrac[species];
            _refoliationCost = refoliationCost[species];
            _nonRefoliationCost = nonRefoliationCost[species];
            _maxLAI = maxLAI[species];
            _mossScalar = mossScalar[species];
            _folN_slope = folN_slope[species];
            _folN_intercept = folN_intercept[species];
            _folBiomassFrac_slope = folBiomassFrac_slope[species];
            _folBiomassFrac_intercept = folBiomassFrac_intercept[species];
            _o3Coeff = o3Coeff[species];
            _leafOnMinT = leafOnMinT[species];
            index = species.Index;
            name = species.Name;
            maxSproutAge = species.MaxSproutAge;
            minSproutAge = species.MinSproutAge;
            postfireregeneration = species.PostFireRegeneration;
            maxSeedDist = species.MaxSeedDist;
            effectiveSeedDist = species.EffectiveSeedDist;
            vegReprodProb = species.VegReprodProb;
            maturity = species.Maturity;
            longevity = species.Longevity;
        }

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

        public float ColdTolerance
        {
            get
            {
                return _coldTolerance;
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

        public float EstablishmentFRad
        {
            get
            {
                return _establishmentfrad;
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

        public float EstablishmentFWater
        {
            get
            {
                return _establishmentfwater;
            }
        }

        public float MaxProbEstablishment
        {
            get
            {
                return _maxProbEstablishment;
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

        public float WoodDebrisDecompRate
        {
            get
            {
                return _wooddebrisdecomprate;
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

        public float AGBiomassFrac
        {
            get
            {
                return _agbiomassfrac;
            }
        }

        public float NSCFrac
        {
            get
            {
                return _nscfrac;
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

        public PostFireRegeneration PostFireRegeneration
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

        public float MaxRefoliationFrac
        {
            get
            {
                return _maxRefoliationFrac;
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
                Type type = typeof(PnETSpecies); // Get type pointer
                List<string> names = type.GetProperties().Select(x => x.Name).ToList(); // Obtain all fields
                return names;
            }
        }

        public string FullName
        {
            get;
            set;
        }
    }
}

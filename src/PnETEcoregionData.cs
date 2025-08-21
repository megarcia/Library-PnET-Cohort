using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Library.PnETCohorts
{
    /// <summary>
    /// The information for a tree species (its index and parameters).
    /// </summary>
    public class PnETEcoregionData : IPnETEcoregionData
    {
        #region private variables
        private Landis.Core.IEcoregion ecoregion;
        private float _precintconst;
        private float _preclossfrac;
        private float _rootingdepth;
        private string _soiltype;
        private float _leakagefrac;
        private float _runoffcapture;
        private float _fieldcap;
        private float _wiltpnt;
        private float _soilPorosity;
        private float _snowsublimfrac;
        private float _latitude;
        private int _precipEvents;
        private float _leakageFrostDepth;
        private float _winterSTD;
        private float _mossDepth;
        IPnETEcoregionVars _variables;
        private float _evapDepth;
        private float _frostFactor;
        #endregion

        #region private static variables
        private static bool wythers;
        private static bool dtemp;
        private static float etExtCoeff;
        private static float retCropCoeff;
        private static Dictionary<IPnETEcoregionData, Dictionary<DateTime, IPnETEcoregionVars>> all_values = new Dictionary<IPnETEcoregionData, Dictionary<DateTime, IPnETEcoregionVars>>();
        private static Dictionary<IEcoregion, IPnETEcoregionData> AllEcoregions;
        private static Library.Parameters.Ecoregions.AuxParm<string> soiltype;
        private static Library.Parameters.Ecoregions.AuxParm<float> rootingdepth;
        private static Library.Parameters.Ecoregions.AuxParm<float> precintconst;
        private static Library.Parameters.Ecoregions.AuxParm<float> preclossfrac;
        private static Library.Parameters.Ecoregions.AuxParm<float> leakagefrac;
        private static Library.Parameters.Ecoregions.AuxParm<float> runoffcapture;
        private static Library.Parameters.Ecoregions.AuxParm<float> snowsublimfrac;
        private static Library.Parameters.Ecoregions.AuxParm<string> climateFileName;
        private static Library.Parameters.Ecoregions.AuxParm<float> latitude;
        private static Library.Parameters.Ecoregions.AuxParm<int> precipEvents;
        private static Library.Parameters.Ecoregions.AuxParm<float> leakageFrostDepth;
        private static Library.Parameters.Ecoregions.AuxParm<float> winterSTD;
        private static Library.Parameters.Ecoregions.AuxParm<float> mossDepth;
        private static Library.Parameters.Ecoregions.AuxParm<float> evapDepth;
        private static Library.Parameters.Ecoregions.AuxParm<float> frostFactor;
        #endregion

        #region accessors for private static variables
        public static List<IPnETEcoregionData> Ecoregions
        {
            get 
            {
                return AllEcoregions.Values.ToList();
            }
        }

        /// <summary>
        /// Returns the PnET Ecoregion for a given Landis Core Ecoregion
        /// </summary>
        /// <param name="landisCoreEcoregion"></param>
        public static IPnETEcoregionData GetPnETEcoregion(IEcoregion landisCoreEcoregion)
        {
            return AllEcoregions[landisCoreEcoregion];
        }
        #endregion

        #region accessors for private variables
        public IPnETEcoregionVars Variables
        {
            get
            {
                return _variables;
            }
            set
            {
                _variables = value;
            }

        }

        public float FieldCapacity
        {
            get
            {
                return _fieldcap;
            }
            set
            {
                _fieldcap = value;
            }
        }
        public float WiltingPoint
        {
            get
            {
                return _wiltpnt;
            }
            set
            {
                _wiltpnt = value;
            }
        }
        public float Porosity
        {
            get
            {
                return _soilPorosity;
            }
            set
            {
                _soilPorosity = value;
            }
        }

        public float LeakageFrac
        {
            get
            {
                return _leakagefrac;
            }
        }

        public float RunoffCapture
        {
            get
            {
                return _runoffcapture;
            }
        }

        public float PrecIntConst
        {
            get
            {
                return _precintconst;
            }
        }

        public float RootingDepth
        {
            get
            {
                return _rootingdepth;
            }
        }

        public string SoilType
        {
            get
            {
                return _soiltype;
            }
        }

        public float PrecLossFrac
        {
            get
            {
                return _preclossfrac;
            }
        }

        public string Description
        {
            get
            {
                return ecoregion.Description;
            }
        }

        public bool Active
        {
            get
            {
                return ecoregion.Active;
            }
        }

        public ushort MapCode
        {
            get
            {
                return ecoregion.MapCode;
            }
        }

        public int Index
        {
            get
            {
                return ecoregion.Index;
            }
        }

        public string Name
        {
            get
            {
                return ecoregion.Name;
            }
        }

        public float SnowSublimFrac
        {
            get
            {
                return _snowsublimfrac;
            }
        }

        public float Latitude
        {
            get
            {
                return _latitude;
            }
        }

        public int PrecipEvents
        {
            get
            {
                return _precipEvents;
            }
        }

        public float LeakageFrostDepth
        {
            get
            {
                return _leakageFrostDepth;
            }
        }

        public float WinterSTD
        {
            get
            {
                return _winterSTD;
            }
        }

        public float MossDepth
        {
            get
            {
                return _mossDepth;
            }
        }

        /// <summary>
        /// Maximum soil depth susceptible to surface evaporation
        /// </summary>
        public float EvapDepth
        {
            get
            {
                return _evapDepth;
            }
        }

        public float FrostFactor
        {
            get
            {
                return _frostFactor; ;
            }
        }
        #endregion

        public static List<string> ParameterNames
        {
            get
            {
                System.Type type = typeof(PnETEcoregionData); // Get type pointer
                List<string> names = type.GetProperties().Select(x => x.Name).ToList(); // Obtain all fields
                names.Add("ClimateFileName");

                return names;
            }
        }

        public static List<IPnETEcoregionVars> GetClimateRegionData(IPnETEcoregionData ecoregion, DateTime start, DateTime end)
        {
            // Monthly simulation data untill but not including end
            List<IPnETEcoregionVars> data = new List<IPnETEcoregionVars>();
            // Date: the last date in the collection of running data
            DateTime date = new DateTime(start.Ticks);
            var oldYear = -1;
            // Ensure only one thread at a time accesses this shared object
            lock (Globals.EcoregionDataThreadLock)
            {
                while (end.Ticks > date.Ticks)
                {
                    if (!all_values[ecoregion].ContainsKey(date))
                    {
                        if (date.Year != oldYear)
                        {
                            if (Globals.IsFutureClimate(date))
                            {
                                ClimateRegionData.AnnualClimate[ecoregion] = 
                                Climate.Climate.FutureEcoregionYearClimate[ecoregion.Index][Globals.ConvertYearToFutureClimateYear(date)];
                            }
                            else
                            {
                                ClimateRegionData.AnnualClimate[ecoregion] = 
                                Climate.Climate.SpinupEcoregionYearClimate[ecoregion.Index][Globals.ConvertYearToSpinUpClimateYear(date)];
                            }
                            oldYear = date.Year;
                        }
                        var monthlyData = new MonthlyClimateRecord(ecoregion, date);
                        List<IPnETSpecies> species = SpeciesParameters.PnETSpecies.AllSpecies.ToList();
                        IPnETEcoregionVars ecoregion_variables = new PnETClimateVars(monthlyData, date, wythers, dtemp, species, ecoregion.Latitude);
                        all_values[ecoregion].Add(date, ecoregion_variables);
                    }
                    data.Add(all_values[ecoregion][date]);
                    date = date.AddMonths(1);
                }
            }

            return data;
        }

        public static List<IPnETEcoregionVars> GetData(IPnETEcoregionData ecoregion, DateTime start, DateTime end)
        {
            // Monthly simulation data untill but not including end
            List<IPnETEcoregionVars> data = new List<IPnETEcoregionVars>();
            // Date: the last date in the collection of running data
            DateTime date = new DateTime(start.Ticks);
            // Ensure only one thread at a time accesses this shared object
            lock (Globals.EcoregionDataThreadLock)
            {
                while (end.Ticks > date.Ticks)
                {
                    if (all_values[ecoregion].ContainsKey(date) == false)
                    {
                        IObservedClimate observedClimate = ObservedClimate.GetData(ecoregion, date);
                        List<IPnETSpecies> species = SpeciesParameters.PnETSpecies.AllSpecies.ToList();
                        IPnETEcoregionVars ecoregion_variables = new PnETEcoregionVars(observedClimate, date, wythers, dtemp, species, ecoregion.Latitude);
                        try
                        {
                            all_values[ecoregion].Add(date, ecoregion_variables);
                        }
                        catch (System.ArgumentException e)
                        {
                            continue;
                        }
                    }
                    data.Add(all_values[ecoregion][date]);
                    date = date.AddMonths(1);
                }
            }

            return data;
        }

        public static void Initialize()
        {
            soiltype = (Library.Parameters.Ecoregions.AuxParm<string>)(Parameter<string>)Names.GetParameter("SoilType");
            rootingdepth = (Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)Names.GetParameter("RootingDepth", 0, 1000);
            precintconst = (Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)Names.GetParameter("PrecIntConst", 0, 1);
            preclossfrac = (Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)Names.GetParameter("PrecLossFrac", 0, 1);
            snowsublimfrac = (Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)Names.GetParameter("SnowSublimFrac", 0, 1);
            latitude = (Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)Names.GetParameter("Latitude", 0, 90);
            leakageFrostDepth = (Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)Names.GetParameter("LeakageFrostDepth", 0, 999999);
            precipEvents = (Library.Parameters.Ecoregions.AuxParm<int>)(Parameter<int>)Names.GetParameter("PrecipEvents", 1, 100);
            winterSTD = (Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)Names.GetParameter("WinterSTD", 0, 100);
            mossDepth = (Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)Names.GetParameter("MossDepth", 0, 1000);
            evapDepth = (Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)Names.GetParameter("EvapDepth", 0, 9999999);
            wythers = ((Parameter<bool>)Names.GetParameter("Wythers")).Value;
            dtemp = ((Parameter<bool>)Names.GetParameter("DTemp")).Value;
            etExtCoeff = ((Parameter<float>)Names.GetParameter("ETExtCoeff")).Value;
            retCropCoeff = ((Parameter<float>)Names.GetParameter("ReferenceETCropCoeff")).Value;
            leakagefrac = (Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)Names.GetParameter("LeakageFrac", 0, 1);
            runoffcapture = (Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)Names.GetParameter(Names.RunoffCapture, 0, 999999);
            frostFactor = (Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)Names.GetParameter("FrostFactor", 0, 999999);
            AllEcoregions = new Dictionary<IEcoregion, IPnETEcoregionData>();
            foreach (IEcoregion ecoregion in Globals.ModelCore.Ecoregions)
            {
                AllEcoregions.Add(ecoregion, new PnETEcoregionData(ecoregion));
            }
            all_values = new Dictionary<IPnETEcoregionData, Dictionary<DateTime, IPnETEcoregionVars>>();
            foreach (IPnETEcoregionData ecoregion in PnETEcoregionData.AllEcoregions.Values)
            {
                all_values[ecoregion] = new Dictionary<DateTime, IPnETEcoregionVars>();
            }
        }

        public PnETEcoregionData(Landis.Core.IEcoregion ecoregion)
        {
            this.ecoregion = ecoregion;
            this._rootingdepth = rootingdepth[ecoregion];
            this._soiltype = soiltype[ecoregion];
            this._precintconst = precintconst[ecoregion];
            this._preclossfrac = preclossfrac[ecoregion];
            this._leakagefrac = leakagefrac[ecoregion];
            this._runoffcapture = runoffcapture[ecoregion];
            this._snowsublimfrac = snowsublimfrac[ecoregion];
            this._latitude = latitude[ecoregion];
            this._precipEvents = precipEvents[ecoregion];
            this._leakageFrostDepth = leakageFrostDepth[ecoregion];
            this._winterSTD = winterSTD[ecoregion];
            this._mossDepth = mossDepth[ecoregion];
            this._evapDepth = evapDepth[ecoregion];
            this._frostFactor = frostFactor[ecoregion];
        }

        public static bool TryGetParameter(string label, out Parameter<string> parameter)
        {
            parameter = null;
            if (label == null)
                return false;
            if (Names.parameters.ContainsKey(label) == false)
                return false;
            else
            {
                parameter = Names.parameters[label];
                return true;
            }
        }

        public static Parameter<string> GetParameter(string label)
        {
            if (Names.parameters.ContainsKey(label) == false)
                throw new System.Exception("No value provided for parameter " + label);

            return Names.parameters[label];
        }

        public static Parameter<string> GetParameter(string label, float min, float max)
        {
            if (Names.parameters.ContainsKey(label) == false)
                throw new System.Exception("No value provided for parameter " + label);
            Parameter<string> p = Names.parameters[label];
            foreach (KeyValuePair<string, string> value in p)
            {
                float f;
                if (float.TryParse(value.Value, out f) == false)
                    throw new System.Exception("Unable to parse value " + value.Value + " for parameter " + label + " unexpected format.");
                if (f > max || f < min)
                    throw new System.Exception("Parameter value " + value.Value + " for parameter " + label + " is out of range. [" + min + "," + max + "]");
            }

            return p;
        }
    }
}
 
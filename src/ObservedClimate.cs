using Landis.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Library.PnETCohorts
{
    public class ObservedClimate : IObservedClimate
    {
        #region private variables
        private static Dictionary<string, IObservedClimate> ClimateData = new Dictionary<string, IObservedClimate>(); 
        private static Library.Parameters.Ecoregions.AuxParm<string> ClimateFileName; 
        private string year;
        private string month;
        private float par0;
        private float prec;
        private float tmin;
        private float tmax;
        private float co2;
        private float o3;
        private float spei;
        private List<ObservedClimate> data_lines = new List<ObservedClimate>();
        #endregion

        #region public accessors
        public float CO2
        {
            get
            {
                return co2;
            }
        }

        public string Year
        {
            get
            {
                return year;
            }
        }

        public string Month
        {
            get
            {
                return month;
            }
        }

        public float PAR0
        {
            get
            {
                return par0;
            }
        }

        public float Prec
        {
            get
            {
                return prec;
            }
        }

        public float Tmin
        {
            get
            {
                return tmin;
            }
        }

        public float O3
        {
            get
            {
                return o3;
            }
        }

        public float Tmax
        {
            get
            {
                return tmax;
            }
        }

        public float SPEI
        {
            get
            {
                return spei;
            }
        }
        #endregion

        public static void Initialize()
        {
            ClimateFileName = (Library.Parameters.Ecoregions.AuxParm<string>)Names.GetParameter("climateFileName");
            Dictionary<IEcoregion, IObservedClimate> dict = new Dictionary<IEcoregion, IObservedClimate>();
            foreach(IEcoregion ecoregion in Globals.ModelCore.Ecoregions)
            {
                if (ecoregion.Active == false)
                    continue;
                else
                {
                    if (dict.ContainsKey(ecoregion))
                        ClimateData[ClimateFileName[ecoregion]] = dict[ecoregion];
                    else
                        ClimateData[ClimateFileName[ecoregion]] = new ObservedClimate(ClimateFileName[ecoregion]);
                }
            }
        }

        /// <summary>
        /// Get the observed climate data from the climate txt file
        /// </summary>
        /// <param name="ecoregion"></param>
        /// <returns></returns>
        public static IObservedClimate GetClimateData(IEcoregion ecoregion)
        {
            return ClimateData[ClimateFileName[ecoregion]];
        }

        /// <summary>
        /// Get specific climate data from climate txt file
        /// </summary>
        /// <param name="ecoregion"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static ObservedClimate GetData(IEcoregion ecoregion, DateTime date)
        {
            // get the appropriate values as read in from a climate txt file
            IObservedClimate observed_climate = GetClimateData(ecoregion);
            try
            {
                return GetData(observed_climate, date);
            }
            catch
            {
                throw new System.Exception("Can't get climate data for ecoregion " + ecoregion.Name + " and date " + date.ToString());
            }
        }

        /// <summary>
        /// Get actual climate data values for specific date (Year, Month)
        /// </summary>
        /// <param name="observed_climate"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static ObservedClimate GetData(IObservedClimate observed_climate, DateTime date)
        {
            foreach (ObservedClimate d in observed_climate)
            {
                if (d.Year.Length == 4) //single year record (e.g., 1974)
                {
                    if (int.Parse(d.Month) == date.Month && date.Year == int.Parse(d.Year))
                        return d;
                }
                else
                {
                    string[] yearExtremes = d.Year.Split('-'); // range of years (e.g., 1800-1900)
                    if (int.Parse(d.Month) == date.Month && int.Parse(yearExtremes[0]) <= date.Year && date.Year <= int.Parse(yearExtremes[1]))
                        return d;
                }
            }
            throw new System.Exception("No climate entry for ecoregion date " + date);
        }

        public struct ColumnNumbers
        {
            public int Year;
            public int Month;
            public int Tmax;
            public int Tmin;
            public int CO2;
            public int PAR0;
            public int Prec;
            public int O3;

            private static int GetColNr(string[] Headers, string Label)
            {
                for (int h = 0; h < Headers.Count(); h++)
                {
                    if (System.Globalization.CultureInfo.InvariantCulture.CompareInfo.IndexOf(Headers[h], Label, System.Globalization.CompareOptions.IgnoreCase) >= 0)
                        return h;
                }

                return -1;
            }

            public ColumnNumbers(string HeaderLine)
            {
                string[] Headers = HeaderLine.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
                Year = GetColNr(Headers, "Year");
                Month = GetColNr(Headers, "Month");
                Tmax = GetColNr(Headers, "Tmax");
                Tmin = GetColNr(Headers, "TMin");
                CO2 = GetColNr(Headers, "CO2");
                PAR0 = GetColNr(Headers, "PAR");
                Prec = GetColNr(Headers, "Prec");
                O3 = GetColNr(Headers, "O3");
            }
        }

        static string[] ReadClimateFile(string climatefilename)
        {
            string[] ClimateFileContent = System.IO.File.ReadAllLines(climatefilename).Where(l => !String.IsNullOrEmpty(l.Trim())).ToArray();
            for (int line = 0; line < ClimateFileContent.Count(); line++)
            {
                int startcomment = ClimateFileContent[line].IndexOf(">>");
                if (startcomment > 0)
                    ClimateFileContent[line] = ClimateFileContent[line].Remove(startcomment, ClimateFileContent[line].Count() - startcomment);
            }

            return ClimateFileContent;
        }

        private static T CheckInRange<T>(T value, T min, T max, string label)
           where T : System.IComparable<T>
        {
            if (Library.Parameters.InputValue_ExtensionMethods.GreaterThan<T>(value, max))
                throw new System.Exception(label + " is out of range " + min + " " + max);
            if (Library.Parameters.InputValue_ExtensionMethods.LessThan<T>(value, min))
                throw new System.Exception(label + " is out of range " + min + " " + max);

            return value;
        }

        ObservedClimate()
        {
        }

        public ObservedClimate(string filename)
        {
            List<string> ClimateFileContent = new List<string>(ReadClimateFile(filename));
            ColumnNumbers columns = new ColumnNumbers(ClimateFileContent[0]);
            ClimateFileContent.Remove(ClimateFileContent[0]);
            foreach (string line in ClimateFileContent)
            {
                ObservedClimate climate = new ObservedClimate();
                string[] terms = line.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
                // Get one state of static information for the line in the climate file
                climate.tmax = CheckInRange<float>(float.Parse(terms[columns.Tmax]), -80, 80, "Tmax");
                climate.tmin = CheckInRange<float>(float.Parse(terms[columns.Tmin]), -80, climate.tmax, "TMin");
                climate.co2 = CheckInRange<float>(float.Parse(terms[columns.CO2]), 0, float.MaxValue, "CO2");
                climate.par0 = (ushort)CheckInRange<float>(float.Parse(terms[columns.PAR0]), 0, float.MaxValue, "PAR0");
                climate.prec = CheckInRange<float>(float.Parse(terms[columns.Prec]), 0, float.MaxValue, "PREC");
                climate.o3 = columns.O3 > 0 ? CheckInRange<float>(float.Parse(terms[columns.O3]), 0, float.MaxValue, "O3") : 0;
                climate.year = terms[columns.Year];
                climate.month = terms[columns.Month];
                data_lines.Add(climate);
            }
        }

        public IEnumerator<ObservedClimate> GetEnumerator()
        {
            return data_lines.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

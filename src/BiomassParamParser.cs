// NOTE: AtEndOfInput --> Landis.Utilities
// NOTE: CurrentLine --> Landis.Utilities
// NOTE: GetNextLine --> Landis.Utilities
// NOTE: InputVar --> Landis.Utilities
// NOTE: NewParseException --> Landis.Utilities
// NOTE: ReadLandisDataVar --> Landis.Core
// NOTE: ReadValue --> Landis.Utilities
// NOTE: ReadVar --> Landis.Utilities
// NOTE: StringReader --> Landis.Utilities

using Landis.Core;
using Landis.Utilities;

namespace Landis.Library.PnETCohorts
{
    class BiomassParamParser : Landis.TextParser<BiomassParam>
    {
        public override string LandisDataValue
        {
            get
            {
                return "BiomassCoefficients";
            }
        }

        // read biomass coefficients from a file into matrix, (float) BioMassData(int ID, 2). No Return Value
        // Read ID first from the file, and ID is the size of BioMassData;
        // Read the two variable in to BioMassData(v1, v2)
        protected override BiomassParam Parse()
        {
            ReadLandisDataVar();
            InputVar<int> speciesnum = new InputVar<int>("Number_of_species_class");
            ReadVar(speciesnum);
            SpeciesParameters.biomass_util.SetBiomassNum(speciesnum.Value.Actual);
            InputVar<float> biomassThreshold = new InputVar<float>("minimum_DBH_for_calculating_biomass");
            ReadVar(biomassThreshold);
            SpeciesParameters.biomass_util.BiomassThreshold = biomassThreshold.Value.Actual;
            InputVar<float> float_val = new InputVar<float>("V0 or V1 value for each species");
            for (int i = 1; i <= speciesnum.Value.Actual; i++)
            {
                if (AtEndOfInput)
                    throw NewParseException("Expected a line here");
                StringReader currentLine = new StringReader(CurrentLine);
                ReadValue(float_val, currentLine);
                SpeciesParameters.biomass_util.SetBiomassData(i, 1, float_val.Value.Actual);
                ReadValue(float_val, currentLine);
                SpeciesParameters.biomass_util.SetBiomassData(i, 2, float_val.Value.Actual);
                GetNextLine();
            }
            return null;
        }
    }
}

using System.Collections.Generic;
using Landis.Library.Parameters;

namespace Landis.Library.PnETCohorts
{
    public interface IEstablishmentProbability
    {
        Library.Parameters.Species.AuxParm<float> Probability { get; }
        Dictionary<ISpeciesPnET,float> Calculate_Establishment_Month(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology, float minHalfSat, float maxHalfSat, bool invertPest, float propRootAboveFrost);
        void ResetPerTimeStep();
        bool HasEstablished(ISpeciesPnET species);
        void EstablishmentTrue(ISpeciesPnET species);
        void RecordPest(int year, ISpeciesPnET species, float pest, float fwater, float frad, bool estab, int monthCount);
        float Get_FWater(ISpeciesPnET species);
        float Get_FRad(ISpeciesPnET species);
    }
}

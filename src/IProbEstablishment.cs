using System.Collections.Generic;

namespace Landis.Library.PnETCohorts
{
    public interface IProbEstablishment
    {
        Library.Parameters.Species.AuxParm<float> SpeciesProbEstablishment { get; }

        float GetSpeciesFWater(ISpeciesPnET species);

        float GetSpeciesFRad(ISpeciesPnET species);

        Dictionary<ISpeciesPnET, float> CalcProbEstablishmentForMonth(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology, float minHalfSat, float maxHalfSat, bool invertProbEstablishment, float fracRootAboveFrost);
         
        bool IsEstablishedSpecies(ISpeciesPnET species);

        void AddEstablishedSpecies(ISpeciesPnET species);

        void RecordProbEstablishment(int year, ISpeciesPnET species, float annualProbEstablishment, float annualFWater, float annualFRad, bool established, int monthCount);

        void Reset();
    }
}

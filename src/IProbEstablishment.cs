using System.Collections.Generic;

namespace Landis.Library.PnETCohorts
{
    public interface IProbEstablishment
    {
        Library.Parameters.Species.AuxParm<float> SpeciesProbEstablishment { get; }

        float GetSpeciesFWater(IPnETSpecies species);

        float GetSpeciesFRad(IPnETSpecies species);

        Dictionary<IPnETSpecies, float> CalcProbEstablishmentForMonth(IPnETEcoregionVars pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology, float minHalfSat, float maxHalfSat, bool invertProbEstablishment, float fracRootAboveFrost);
         
        bool IsEstablishedSpecies(IPnETSpecies species);

        void AddEstablishedSpecies(IPnETSpecies species);

        void RecordProbEstablishment(int year, IPnETSpecies species, float annualProbEstablishment, float annualFWater, float annualFRad, bool established, int monthCount);

        void Reset();
    }
}

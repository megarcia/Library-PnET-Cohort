 
namespace Landis.Library.PnETCohorts 
{
    /// <summary>
    /// The information for a tree species (its index and parameters).
    /// </summary>
    public interface ISpeciesPnET : Landis.Core.ISpecies
    { 
        float CFracBiomass { get; } // Carbon fraction in biomass
        float DNSC { get; } // Fraction of non-soluble carbon to active biomass
        float FracBelowG { get; } // Fraction biomass below-ground
        float FracFol { get; } // Fraction foliage to active biomass
        float FrActWd { get; } // Fraction active biomass to total biomass
        float H1 { get; } // Water stress parameter for excess water: pressurehead below which growth halts
        float H2 { get; } // Water stress parameter for excess water: pressurehead below which growth declines
        float H3 { get; } // Water stress parameter for water shortage: pressurehead above which growth declines
        float H4 { get; } // Water stress parameter: pressurehead above growth halts (= wilting point)
        float InitialNSC { get; } // Initial NSC for new cohort
        float HalfSat { get; } // Half saturation value for radiation (W/m2)
        float K { get; } // Radiation extinction rate through the canopy (LAI-1)
        float KWdLit { get; } // Decomposition constant of woody litter (yr-1)
        float PsnAgeRed { get; } //Growth reduction parameter with age
        float SLWDel { get; } // Reduction of specific leaf weight throught the canopy (g/m2/g)
        float SLWmax { get; } // Max specific leaf weight (g/m2)
        float TOfol { get; } // Foliage turnover (g/g/y)  
        float TOroot { get; } // Root turnover (g/g/y)        
        float TOwood { get; } // Wood turnover (g/g/y)
        float EstRad { get; } // Establishment factor related to light
        float EstMoist { get; } // Establishment factor related to moisture
        float MaxPest { get; } // Mamximum total probability of establishment under optimal conditions
        float FolLignin { get; } // Lignin concentration in foliage
        bool PreventEstablishment { get; } // Prevent establishment 
        float PsnTOpt { get; } // Optimal temperature for photosynthesis
        float Q10 { get; } // Temperature response factor for respiration
        float BFolResp { get; } // Base foliar respiration (g respired / g photosynthesis)
        float PsnTMin { get; } // Minimum temperature for photosynthesis
        float PsnTMax { get; } //Maximum temperature for photosynthesis
        float FolN { get; } // Foliar nitrogen (gN/gC)
        float DVPD1 { get; } // Vapor pressure deficit response parameter
        float DVPD2 { get; } // Vapor pressure deficit response parameter
        float AmaxA { get; } // Reference photosynthesis (g)
        float AmaxB { get; } // Response parameter for photosynthesis to N
        float AmaxFrac { get; } // Modifier of AmaxA due to averaging non-linear Amax data
        float MaintResp { get; } // Referece maintenance respiration
        float CO2AMaxBEff { get; } // Effect of CO2 on AMaxB (change in AMaxB with increase of 200 ppm CO2)
        float CO2HalfSatEff { get; } // Effect of CO2 on HalfSat (change in HalfSat with increase of 1 ppm CO2 [slope])
        string O3StomataSens { get; } // Ozone stomatal sensitivity class (Sensitive, Intermediate, Tolerant)
        float FolNShape { get; } // Slope for linear FolN relationship
        float MaxFolN { get; } // Intercept for linear FolN relationship
        float FracFolShape { get; } // Slope for linear FracFol relationship
        float MaxFracFol { get; } // Intercept for linear FracFol relationship
        float O3GrowthSens { get; } // Slope coefficient for O3Effect
        float ColdTol { get; } // Cold tolerance
        float LeafOnMinT { get; } // Mininum Temp for leaf-on (optional) -- If not provided, LeafOnMinT = PsnTMin
        int InitBiomass { get; } // Initial Biomass         
        float NSCReserve { get; } // Lower canopy NSC reserve
        string Lifeform { get; } // Lifeform
        float RefoliationMinimumTrigger { get; } // Minimum defoliation amount that triggers refoliation
        float RefoliationMaximum { get; } // Maximum amount of refoliation
        float RefoliationCost { get; } // Cost to NSC of refoliation
        float NonRefoliationCost { get; } // Cost to NSC without refoliation
        float MaxLAI { get; } // Maximum LAI
        float MossScalar { get; } // Scalar value for calculating species moss depth
    }
}

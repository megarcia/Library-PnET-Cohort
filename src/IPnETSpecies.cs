 
namespace Landis.Library.PnETCohorts 
{
    /// <summary>
    /// The information for a tree species (its index and parameters).
    /// </summary>
    public interface IPnETSpecies : Landis.Core.ISpecies
    {
        /// <summary>
        /// Species name 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Carbon fraction in biomass 
        /// </summary>
        float CFracBiomass { get; }

        /// <summary>
        /// Fraction of non-soluble carbon to active biomass
        /// </summary>
        float DNSC { get; }

        /// <summary>
        /// Fraction biomass below-ground
        /// </summary>
        float FracBelowG { get; }

        /// <summary>
        /// Fraction foliage to active biomass
        /// </summary>
        float FracFol { get; }

        /// <summary>
        /// Fraction active biomass to total biomass 
        /// </summary>
        float FrActWd { get; }

        /// <summary>
        /// Water stress parameter for excess water: pressure head below which growth halts
        /// </summary>
        float H1 { get; }

        /// <summary>
        /// Water stress parameter for excess water: pressure head below which growth declines
        /// </summary>
        float H2 { get; }

        /// <summary>
        /// Water stress parameter for water shortage: pressure head above which growth declines
        /// </summary>
        float H3 { get; }

        /// <summary>
        /// Water stress parameter for water shortage: pressure head above growth halts (= wilting point)
        /// </summary>
        float H4 { get; }

        /// <summary>
        /// Initial NSC for new cohort
        /// </summary>
        float InitialNSC { get; }

        /// <summary>
        /// Half saturation value for radiation (W/m2)
        /// </summary>
        float HalfSat { get; }

        /// <summary>
        /// Radiation extinction rate through the canopy (LAI-1)
        /// </summary>
        float K { get; }

        /// <summary>
        /// Decomposition constant of wood litter (yr-1)
        /// </summary>
        float WoodLitterDecompRate { get; }

        /// <summary>
        /// Species longevity (yr)
        /// </summary>
        int Longevity { get; }

        /// <summary>
        /// Growth reduction parameter with age
        /// </summary>
        float PsnAgeRed { get; }

        /// <summary>
        /// Reduction of specific leaf weight throught the canopy (g/m2/g)
        /// </summary>
        float SLWDel { get; }

        /// <summary>
        /// Max specific leaf weight (g/m2)
        /// </summary>
        float SLWmax { get; }

        /// <summary>
        /// Foliage turnover (g/g/y)
        /// </summary>
        float TOfol { get; }

        /// <summary>
        /// Root turnover (g/g/y)
        /// </summary>
        float TOroot { get; }

        /// <summary>
        /// Wood turnover (g/g/y)
        /// </summary>
        float TOwood { get; }

        /// <summary>
        /// Establishment factor related to light - fRad value that equates to optimal light for establishment
        /// </summary>
        float EstRad { get; }

        /// <summary>
        /// Establishment factor related to moisture - fWater value that equates to optimal water for establishment
        /// </summary>
        float EstMoist { get; }

        /// <summary>
        /// Mamximum total probability of establishment under optimal conditions
        /// </summary>
        float MaxPest { get; }

        /// <summary>
        /// Lignin concentration in foliage
        /// </summary>
        float FolLignin { get; }

        /// <summary>
        /// Prevent establishment 
        /// </summary>
        bool PreventEstablishment { get; }

        /// <summary>
        /// Optimal temperature for photosynthesis
        /// </summary>
        float PsnTopt { get; }

        /// <summary>
        /// Temperature response factor for respiration
        /// </summary>
        float Q10 { get; }

        /// <summary>
        /// Base foliar respiration (g respired / g photosynthesis)
        /// </summary>
        float BaseFoliarRespiration { get; }

        /// <summary>
        /// Minimum temperature for photosynthesis
        /// </summary>
        float PsnTmin { get; }

        /// <summary>
        /// Maximum temperature for photosynthesis
        /// </summary>
        float PsnTmax { get; }

        /// <summary>
        /// Foliar nitrogen (gN/gC)
        /// </summary>
        float FolN { get; }

        /// <summary>
        /// Vapor pressure deficit response parameter 
        /// </summary>
        float DVPD1 { get; }

        /// <summary>
        /// Vapor pressure deficit response parameter 
        /// </summary>
        float DVPD2 { get; }

        /// <summary>
        /// Reference photosynthesis (g)
        /// </summary>
        float AmaxA { get; }

        /// <summary>
        /// Response parameter for photosynthesis to N
        /// </summary>
        float AmaxB { get; }

        /// <summary>
        /// Modifier of AmaxA due to averaging non-linear Amax data
        /// </summary>
        float AmaxFrac { get; }

        /// <summary>
        /// Referece maintenance respiration 
        /// </summary>
        float MaintResp { get; }

        /// <summary>
        /// Effect of CO2 on AMaxB (change in AMaxB with increase of 200 ppm CO2)
        /// </summary>
        float CO2AMaxBEff { get; }

        /// <summary>
        /// Effect of CO2 on HalfSat (change in HalfSat with increase of 1 ppm CO2 [slope])
        /// </summary>
        float CO2HalfSatEff { get; }

        /// <summary>
        /// Ozone stomatal sensitivity class (Sensitive, Intermediate, Tolerant)
        /// </summary>
        string O3StomataSens { get; }

        /// <summary>
        /// Slope for linear FolN relationship
        /// </summary>
        float FolNShape { get; }

        /// <summary>
        /// Intercept for linear FolN relationship
        /// </summary>
        float MaxFolN { get; }

        /// <summary>
        /// Slope for linear FracFol relationship
        /// </summary>
        float FracFolShape { get; }

        /// <summary>
        /// Intercept for linear FracFol relationship
        /// </summary>
        float MaxFracFol { get; }

        /// <summary>
        /// Slope coefficient for FOzone
        /// </summary>
        float O3GrowthSens { get; }

        /// <summary>
        /// Cold tolerance
        /// </summary>
        float ColdTol { get; }

        /// <summary>
        /// Mininum Temp for leaf-on (optional)
        /// If not provided, LeafOnMinT = PsnTmin
        /// </summary>
        float LeafOnMinT { get; }

        /// <summary>
        /// Initial Biomass
        /// </summary>
        int InitBiomass { get; }

        /// <summary>
        /// Lower canopy NSC reserve 
        /// </summary>
        float NSCReserve { get; }

        /// <summary>
        /// Lifeform
        /// </summary>
        string Lifeform { get; }

        /// <summary>
        /// Minimum defoliation amount that triggers refoliation
        /// </summary>
        float RefoliationMinimumTrigger { get; }
        
        /// <summary>
        /// Maximum amount of refoliation
        /// </summary>
        float RefoliationMaximum { get; }

        /// <summary>
        /// Cost of refoliation
        /// </summary>
        float RefoliationCost { get; }

        /// <summary>
        /// Cost to NSC without refoliation
        /// </summary>
        float NonRefoliationCost { get; }

        /// <summary>
        /// Maximum LAI
        /// </summary>
        float MaxLAI { get; }

        /// <summary>
        /// Scalar value for calculating species moss depth
        /// </summary>
        float MossScalar { get; }
    }
}

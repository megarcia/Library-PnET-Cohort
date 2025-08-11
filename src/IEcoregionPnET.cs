namespace Landis.Library.PnETCohorts
{
    public interface IEcoregionPnET : Landis.Core.IEcoregion
    {
        /// <summary>
        /// Ecoregion name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Is active ecoregion
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// Fraction of water above field capacity that drains out of the soil rooting zone immediately after entering the soil (fast leakage)
        /// </summary>
        float LeakageFrac{ get; }

        /// <summary>
        /// Depth of surface water (mm) that can be held on site instead of running off
        /// </summary>
        float RunoffCapture { get; }

        /// <summary>
        /// Fraction of incoming precipitation that does not enter the soil - surface runoff due to impermeability, slope, etc.
        /// </summary>
        float PrecLossFrac { get; } 

        /// <summary>
        /// Ecoregion soil type descriptor 
        /// </summary>
        string SoilType { get; }

        /// <summary>
        /// Rate at which incoming precipitation is intercepted by foliage for each unit of LAI
        /// </summary>
        float PrecIntConst { get; }

        /// <summary>
        /// Depth of rooting zone in the soil (mm)
        /// </summary>
        float RootingDepth { get; }

        /// <summary>
        /// Volumetric soil water content (mm/m) at field capacity
        /// </summary>
        float FieldCapacity { get; set; }

        /// <summary>
        /// Volumetric soil water content (mm/m) at wilting point
        /// </summary>
        float WiltingPoint { get; set; }

        /// <summary>
        /// Volumetric soil water content (mm/m) at porosity
        /// </summary>
        float Porosity { get; set; }

        /// <summary>
        /// Fraction of snow pack that sublimates before melting
        /// </summary>
        float SnowSublimFrac { get; }

        float LeakageFrostDepth { get; }

        int PrecipEvents { get; }

        /// <summary>
        /// Ecoregion latitude
        /// </summary>
        float Latitude { get; } 

        float WinterSTD { get; }

        float MossDepth { get; }

        /// <summary>
        /// Maximum soil depth susceptible to surface evaporation
        /// </summary>
        float EvapDepth { get; }

        /// <summary>
        /// Tuning parameter to adjust frost depth
        /// </summary>
        float FrostFactor { get; }

        IEcoregionPnETVariables Variables { get; set; }        
    }
}

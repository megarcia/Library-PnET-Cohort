using Landis.Core;

//JUST BE ECOREGIONDATA? GET VALUES FROM ECOREGION IN PNET?

namespace Landis.Library.PnETCohorts
{
    public interface IEcoregionPnET : IEcoregion
    {
        float LeakageFrac{ get;}
        float RunoffCapture { get; }
        float PrecLossFrac { get; }
        string SoilType { get; }        
        float PrecIntConst { get;  }
        float RootingDepth { get;   }
        float FieldCap { get; set; }
        float WiltPnt { get; set; }
        float Porosity { get; set; }
        float SnowSublimFrac { get; }
        float LeakageFrostDepth { get; }
        int PrecipEvents { get; }
        float Latitude { get; } 
        float WinterSTD { get; }
        float MossDepth { get; }
        
       
        IEcoregionPnETVariables Variables { get; set; }
        
    }
}

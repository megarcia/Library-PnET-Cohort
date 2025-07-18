
namespace Landis.Library.PnETCohorts 
{
    public class Constants
    {
        /// <summary>
        /// Molecular weight of C
        /// </summary>
        public const float MC = 12;

        /// <summary>
        /// Molecular weight of CO2
        /// </summary>
        public const float MCO2 = 44;

        /// <summary>
        /// Molecular weight of CO2 relative to C 
        /// </summary>
        public const float MCO2_MC = MCO2 / MC;

        /// <summary>
        /// Seconds per hour
        /// </summary>
        public const int SecondsPerHour = 60 * 60;

        /// <summary>
        /// Seconds per day
        /// </summary>
        public const int SecondsPerDay = SecondsPerHour * 24;

        /// <summary>
        /// Billion
        /// </summary>
        public const int billion = 1000000000;

        public const enum Months
        {
            January = 1,
            February,
            March,
            April,
            May,
            June,
            July,
            August,
            September,
            October,
            November,
            December
        }

        /// <summary>
        /// heat capacity of solid portion of soil in kJ/m3.K 
        /// (Farouki 1986 in vanLier and Durigon 2013)
        /// </summary>
        public const float cs = 1942;

        /// <summary>
        /// heat capacity of water in kJ/m3.K
        /// (vanLier and Durigon 2013)
        /// </summary>
        public const float cw = 4186;

        /// <summary>
        /// thermal conductivity of air in kJ/m.d.K 
        /// (vanLier and Durigon 2013)
        /// </summary>
        public const float lambda_a = 2.25F;

        /// <summary>
        /// thermal conductivity of water in kJ/m.d.K
        /// (vanLier and Durigon 2013)
        /// </summary>
        public const float lambda_w = 51.51F;

        /// <summary>
        /// thermal conductivity of clay soil in kJ/m.d.K
        /// (Michot et al. 2008 in vanLier and Durigon 2013)
        /// </summary>
        public const float lambda_clay = 80F;

        /// <summary>
        /// thermal conductivity of sandstone in kJ/m.d.K 
        /// (Gemant 1950 in vanLier and Durigon 2013)
        /// </summary>
        public const float lambda_0 = 360F;

        /// <summary>
        /// unexplained coefficient in vanLier and Durigon (2013)
        /// (via Farouki 1986)
        /// </summary>
        public const float gs = 0.125F;

        /// <summary>
        /// angular velocity of Earth in radians/month
        /// </summary>
        public const float omega = (float)System.Math.PI * 2 / 12;

        /// <summary>
        /// length of temp record in months
        /// </summary>
        public const float tau = 12F;

        /// <summary>
        /// intercept of function for bulk density of snow in kg/m3
        /// </summary>
        public const float PSnow_intercept = 165.0f; 

        /// <summary>
        /// slope of function for bulk density of snow in kg/m3
        /// </summary>
        public const float PSnow_slope = 1.3f; 

        /// <summary>
        /// Density of water in kg/m3
        /// </summary>
        public const float PWater = 1000.0f;  

        /// <summary>
        /// thermal conductivity of air in W/m.K 
        /// (CLM5 documentation, Table 2.7)
        /// </summary>
        public const float lambda_Air = 0.023f; 

        /// <summary>
        /// thermal conductivity of ice in W/m.K 
        /// (CLM5 documentation, Table 2.7)
        /// </summary>
        public const float lambda_Ice = 2.29f; 

        /// <summary>
        /// heat capacity of snow, in J/kg.K 
        /// (https://www.engineeringtoolbox.com/specific-heat-capacity-d_391.html)
        /// </summary>
        public const float snowHeatCapacity = 2090f;

        /// <summary>
        /// minimum depth of snow (m) that counts as 
        /// full snow cover for albedo calculations
        /// </summary>
        public const float snowReflectanceThreshold = 0.100f;  
    }
}

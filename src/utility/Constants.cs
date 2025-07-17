
namespace Landis.Library.PnETCohorts 
{
    public class Constants 
    {
        /// <summary>
        /// Molecular weight of C
        /// </summary>
        public static float MC = 12;

        /// <summary>
        /// Molecular weight of CO2
        /// </summary>
        public static float MCO2 = 44;

        /// <summary>
        /// Molecular weight of CO2 relative to C 
        /// </summary>
        public static float MCO2_MC = MCO2 / MC;

        public static int SecondsPerHour = 60 * 60;
        public static int billion = 1000000000; 
            
        public enum Months
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

        // For Permafrost

        /// <summary>
        /// heat capacity solid in kJ/m3/K (Farouki 1986 in vanLier and Durigon 2013)
        /// </summary>
        public static float cs = 1942;

        /// <summary>
        /// heat capacity of water in kJ/m3/K (vanLier and Durigon 2013)
        /// </summary>
        public static float cw = 4186;

        /// <summary>
        /// thermal conductivity of air	kJ/m/d/K (vanLier and Durigon 2013)
        /// </summary>
        public static float lambda_a = 2.25F;

        /// <summary>
        /// thermal conductivity of water in kJ/m/d/K (vanLier and Durigon 2013)
        /// </summary>
        public static float lambda_w = 51.51F;

        /// <summary>
        /// thermal conductivity of clay in kJ/m/d/K (Michot et al. 2008 in vanLier and Durigon 2013)
        /// </summary>
        public static float lambda_clay = 80F;

        /// <summary>
        /// thermal conductivity of sand-silt in kJ/m/d/K (Gemant 1950 in vanLier and Durigon 2013)
        /// </summary>
        public static float lambda_0 = 360F;

        /// <summary>
        /// (Farouki 1986 in vanLier and Durigon 2013)
        /// </summary>
        public static float gs = 0.125F;

        /// <summary>
        /// angular velocity of Earth in radians/month
        /// </summary>
        public static float omega = (float)System.Math.PI * 2 / 12;

        /// <summary>
        /// length of temp record in months
        /// </summary>
        public static float tau = 12F;
    }
}

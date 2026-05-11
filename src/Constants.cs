using System;

namespace Landis.Library.PnETCohorts 
{
    public class Constants 
    {
        /// <summary>
        /// Million
        /// </summary>
        public const int Million = 1E6;

        /// <summary>
        /// Billion
        /// </summary>
        public const int Billion = 1E9;

        /// <summary>
        /// Seconds per hour
        /// </summary>
        public const int SecondsPerHour = 60 * 60;

        /// <summary>
        /// Seconds per day
        /// </summary>
        public const int SecondsPerDay = SecondsPerHour * 24;

        /// <summary>
        /// Molecular weight of C in g/mol
        /// </summary>
        public const float MC = 12.0;

        /// <summary>
        /// Molecular weight of CO2 in g/mol
        /// </summary>
        public const float MCO2 = 44.0;

        /// <summary>
        /// Ratio of molecular weight of CO2 to C 
        /// </summary>
        public const float MCO2_MC = MCO2 / MC; // Molecular weight of CO2 relative to C 

        /// <summary>
        /// Joules <--> calorie conversion
        /// </summary>
        public const float Jpercal = 4.184;
        public const float CalperJ = 1.0 / Jpercal;

        /// <summary>
        /// Universal gas constant in J/kmol.K
        /// </summary>
        public const float GasConst_JperkmolK = 8314.47;

        /// <summary>
        /// Reference temperature in K
        /// </summary>
        public const float Tref_K = 273.15;

        /// <summary>
        /// Reference pressure in kPa
        /// </summary>
        public const float Pref_kPa = 101.3;

        /// <summary>
        /// Psychrometric coefficient in kPa/K
        /// (Cabrera et al., 2016, Table 1)
        /// </summary>
        public const float PsychrometricCoeff = 0.062;

        /// <summary>
        /// Reference atmospheric concentration of CO2 in ppm
        /// </summary>
        public const float CO2RefConc = 350.0;

        /// <summary>
        /// convert kJ/m.d.K to W/m.K
        /// </summary>
        public const float Convert_kJperday_to_Watts = 0.2777777778 / 24.0;

        /// <summary>
        /// density of water in kg/m3
        /// </summary>
        public const float DensityWater = 1000.0;

        /// <summary>
        /// latent heat of vaporization for water in MJ/m3
        /// </summary>
        public const float LatentHeatVaporWater = 2453.0;

        /// <summary>
        /// heat capacity of solid portion of soil in kJ/m3.K 
        /// (Farouki, 1986, in vanLier and Durigon, 2013)
        /// </summary>
        public const float HeatCapacitySoil = 1942.0;

        /// <summary>
        /// heat capacity of water in kJ/m3.K = J/kg.K
        /// (vanLier and Durigon, 2013)
        /// </summary>
        public const float HeatCapacityWater_Jperkg = 4186.0;

        /// <summary>
        /// heat capacity of snow in J/kg.K 
        /// (https://www.engineeringtoolbox.com/specific-heat-capacity-d_391.html)
        /// </summary>
        public const float HeatCapacitySnow_Jperkg = 2090.0;

        /// <summary>
        /// heat capacity of moss in kJ/m3.K 
        /// (Sazonova and Romanovsky, 2003)
        /// </summary>
        public const float HeatCapacityMoss = 2500.0;

        /// <summary>
        /// thermal conductivity of air in kJ/m.d.K 
        /// (vanLier and Durigon, 2013)
        /// </summary>
        public const float ThermalConductivityAir_kJperday = 2.25;

        /// <summary>
        /// thermal conductivity of air in W/m.K 
        /// (CLM5 documentation, Table 2.7)
        /// </summary>
        public const float ThermalConductivityAir_Watts = 0.023;

        /// <summary>
        /// thermal conductivity of water in kJ/m.d.K
        /// (vanLier and Durigon, 2013)
        /// </summary>
        public const float ThermalConductivityWater_kJperday = 51.51;

        /// <summary>
        /// thermal conductivity of water in W/m.K 
        /// (CLM5 documentation, Table 2.7)
        /// </summary>
        public const float ThermalConductivityWater_Watts = 0.57;

        /// <summary>
        /// thermal conductivity of ice in W/m.K 
        /// (CLM5 documentation, Table 2.7)
        /// </summary>
        public const float ThermalConductivityIce_Watts = 2.29;

        /// <summary>
        /// thermal conductivity of clay soil in kJ/m.d.K
        /// (Michot et al., 2008 in vanLier and Durigon, 2013)
        /// </summary>
        public const float ThermalConductivityClay = 80.0;

        /// <summary>
        /// thermal conductivity of sandstone in kJ/m.d.K 
        /// (Gemant, 1950, in vanLier and Durigon, 2013)
        /// </summary>
        public const float ThermalConductivitySandstone = 360.0;

        /// <summary>
        /// thermal conductivity of moss in kJ/m.d.K
        /// converted from 0.2 W/m.K (Sazonova and Romanovsky, 2003)
        /// </summary>
        public const float ThermalConductivityMoss = 432.0;

        /// <summary>
        /// unexplained coefficient in vanLier and Durigon (2013)
        /// (via Farouki, 1986)
        /// </summary>
        public const float gs = 0.125;

        /// <summary>
        /// angular velocity of Earth in radians/day
        /// </summary>
        public const float omega = 2.0 * (float)Math.PI;

        /// <summary>
        /// length of temperature record in months
        /// </summary>
        public const float tau = 12.0;

        /// <summary>
        /// intercept of function for bulk density of snow in kg/m3
        /// </summary>
        public const float DensitySnow_intercept = 165.0;

        /// <summary>
        /// slope of function for bulk density of snow in kg/m3
        /// </summary>
        public const float DensitySnow_slope = 1.3;

        /// <summary>
        /// minimum depth of snow (m) that counts as 
        /// full snow cover for albedo calculations
        /// </summary>
        public const float SnowReflectanceThreshold = 0.1;
    }
}

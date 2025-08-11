using System;

namespace Landis.Library.PnETCohorts 
{
    public class Constants
    {
        /// <summary>
        /// Molecular weight of C in g/mol
        /// </summary>
        public const float MC = 12F;

        /// <summary>
        /// Molecular weight of CO2 in g/mol
        /// </summary>
        public const float MCO2 = 44F;

        /// <summary>
        /// Molecular weight of CO2 relative to C 
        /// </summary>
        public const float MCO2_MC = MCO2 / MC;

        /// <summary>
        /// Reference atmospheric concentration of CO2 in ppm
        /// </summary>
        public const float CO2RefConc = 350F;

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
        /// latent heat of vaporization for water in MJ/m3
        /// </summary>
        public const float LatentHeatVaporWater = 2453F;

        /// <summary>
        /// heat capacity of solid portion of soil in kJ/m3.K 
        /// (Farouki, 1986, in vanLier and Durigon, 2013)
        /// </summary>
        public const float HeatCapacitySoil = 1942F;

        /// <summary>
        /// heat capacity of water in kJ/m3.K
        /// (vanLier and Durigon, 2013)
        /// </summary>
        public const float HeatCapacityWater = 4186F;

        /// <summary>
        /// heat capacity of snow in J/kg.K 
        /// (https://www.engineeringtoolbox.com/specific-heat-capacity-d_391.html)
        /// </summary>
        public const float HeatCapacitySnow_Jperkg = 2090F;

        /// <summary>
        /// heat capacity of moss in kJ/m3.K 
        /// (Sazonova and Romanovsky, 2003)
        /// </summary>
        public const float HeatCapacityMoss = 2500F;

        /// <summary>
        /// thermal conductivity of air in kJ/m.d.K 
        /// (vanLier and Durigon, 2013)
        /// </summary>
        public const float ThermalConductivityAir_kJperday = 2.25F;

        /// <summary>
        /// thermal conductivity of water in kJ/m.d.K
        /// (vanLier and Durigon, 2013)
        /// </summary>
        public const float ThermalConductivityWater_kJperday = 51.51F;

        /// <summary>
        /// thermal conductivity of clay soil in kJ/m.d.K
        /// (Michot et al., 2008 in vanLier and Durigon, 2013)
        /// </summary>
        public const float ThermalConductivityClay = 80F;

        /// <summary>
        /// thermal conductivity of sandstone in kJ/m.d.K 
        /// (Gemant, 1950, in vanLier and Durigon, 2013)
        /// </summary>
        public const float ThermalConductivitySandstone = 360F;

        /// <summary>
        /// thermal conductivity of moss in kJ/m.d.K
        /// converted from 0.2 W/m.K (Sazonova and Romanovsky, 2003)
        /// </summary>
        public const float ThermalConductivityMoss = 432F;

        /// <summary>
        /// unexplained coefficient in vanLier and Durigon (2013)
        /// (via Farouki, 1986)
        /// </summary>
        public const float gs = 0.125F;

        /// <summary>
        /// angular velocity of Earth in radians/month
        /// </summary>
        public const float omega = (float)Math.PI * 2 / 12F;

        /// <summary>
        /// length of temp record in months
        /// </summary>
        public const float tau = 12F;

        /// <summary>
        /// intercept of function for bulk density of snow in kg/m3
        /// </summary>
        public const float DensitySnow_intercept = 165.0F;

        /// <summary>
        /// slope of function for bulk density of snow in kg/m3
        /// </summary>
        public const float DensitySnow_slope = 1.3F;

        /// <summary>
        /// Density of water in kg/m3
        /// </summary>
        public const float DensityWater = 1000.0F;

        /// <summary>
        /// thermal conductivity of air in W/m.K 
        /// (CLM5 documentation, Table 2.7)
        /// </summary>
        public const float ThermalConductivityAir_Watts = 0.023F;

        /// <summary>
        /// thermal conductivity of ice in W/m.K 
        /// (CLM5 documentation, Table 2.7)
        /// </summary>
        public const float ThermalConductivityIce_Watts = 2.29F;

        /// <summary>
        /// minimum depth of snow (m) that counts as 
        /// full snow cover for albedo calculations
        /// </summary>
        public const float snowReflectanceThreshold = 0.1F;

        /// <summary>
        /// thermal diffusivity of moss
        /// </summary>
        public const float ThermalDiffusivityMoss = ThermalConductivityMoss / HeatCapacityMoss;

        /// <summary>
        /// thermal damping coefficient for moss
        /// </summary>
        public const float ThermalDampingMoss = (float)Math.Sqrt(2.0F * ThermalDiffusivityMoss / omega);

        /// <summary>
        /// Psychrometric coefficient in kPa/K
        /// (Cabrera et al., 2016, Table 1)
        /// </summary>
        public const float PsychrometricCoeff = 0.062F;

        /// <summary>
        /// field capacity pore water (negative) pressure in kPa
        /// </summary>
        public const float FieldCapacity_kPa = -33F;

        /// <summary>
        /// field capacity pore water (negative) pressure in mmH2O
        /// </summary>
        public const float FieldCapacity_mmH2O = FieldCapacity_kPa / 9.804139432F;

        /// <summary>
        /// wilting point pore water (negative) pressure in kPa
        /// </summary>
        public const float WiltingPoint_kPa = -1500F;

        /// <summary>
        /// wilting point pore water (negative) pressure in mmH2O
        /// </summary>
        public const float WiltingPoint_mmH2O = WiltingPoint_kPa / 9.804139432F;

    }
}

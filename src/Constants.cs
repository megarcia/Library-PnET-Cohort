using System;

namespace Landis.Library.PnETCohorts 
{
    public class Constants 
    {
        public const int Million = 1E6; // Million
        public const int Billion = 1E9; // Billion 
        public const int SecondsPerHour = 60 * 60; // Seconds per hour
        public const int SecondsPerDay = SecondsPerHour * 24; // Seconds per day 
        public const float MC = 12.0; // Molecular weight of C
        public const float MCO2 = 44.0; // Molecular weight of CO2
        public const float MCO2_MC = MCO2 / MC; // Molecular weight of CO2 relative to C 
        public const float cs = 1942.0; // heat capacity of solid rock in kJ/m3/K from Farouki (1986) via vanLier and Durigon (2013)
        public const float cw = 4186.0; // heat capacity of water in kJ/m3/K from vanLier and Durigon (2013)
        public const float lambda_a = 2.25; // thermal conductivity of air in kJ/m/d/K from vanLier and Durigon (2013)
        public const float lambda_w = 51.51; // thermal conductivity of water in kJ/m/d/K from vanLier and Durigon (2013)
        public const float lambda_clay = 80.0; // thermal conductivity of clay in kJ/m/d/K from Michot et al. (2008) via vanLier and Durigon (2013)
        public const float lambda_0 = 360.0; // thermal conductivity of sand-silt in kJ/m/d/K from Gemant (1950) via vanLier and Durigon (2013)
        public const float gs = 0.125; // from Farouki (1986) via vanLier and Durigon (2013)
        public const float omega = 2.0 * (float)Math.PI; // angular velocity of Earth (daily rotation) in radians/day; see Ochsner, 2019: Rain or Shine (textbook), Eq. 13-5 https://open.library.okstate.edu/rainorshine/chapter/13-4-sub-surface-soil-temperatures/ for explanation
        public const float tau = 12.0; // length of temp record in months
        public const float DensitySnow_intercept = 165.0; // kg/m3
        public const float DensitySnow_slope = 1.3; // kg/m3
        public const float DensityWater = 1000.0;  // Density of water (kg/m3)
        public const float ThermalConductivityAir_Watts = 0.023; // W/m.K (CLM5 documentation, Table 2.7)
        public const float ThermalConductivityWater_Watts = 0.57;  // W/m.K (CLM5 documentation, Table 2.7)
        public const float ThermalConductivityIce_Watts = 2.29; // W/m.K (CLM5 documentation, Table 2.7)
        public const float snowHeatCapacity = 2090.0; // J/kg K (https://www.engineeringtoolbox.com/specific-heat-capacity-d_391.html)
        public const float snowReflectanceThreshold = 0.100;  // minimum depth of snow (m) that counts as full snow for albedo calculations
        public const float cv_moss = 2500.0; // heat capacity of moss - kJ/m3/K (Sazonova and Romanovsky 2003)
        public const float lambda_moss = 432.0; // thermal conductivity of moss in kJ/m/d/K - converted from 0.2 W/mK (Sazonova and Romanovsky 2003)

        /// <summary>
        /// Universal gas constant in J/kmol.K
        /// </summary>
        public const float GasConst_JperkmolK = 8314.47F;

        /// <summary>
        /// Reference temperature in K
        /// </summary>
        public const float Tref_K = 273.15F;

        /// <summary>
        /// Reference pressure in kPa
        /// </summary>
        public const float Pref_kPa = 101.3F;

        /// <summary>
        /// Psychrometric coefficient in kPa/K
        /// (Cabrera et al., 2016, Table 1)
        /// </summary>
        public const float PsychrometricCoeff = 0.062;

        /// <summary>
        /// Reference atmospheric concentration of CO2 in ppm
        /// </summary>
        public const float CO2RefConc = 350F;

        /// <summary>
        /// convert kJ/m.d.K to W/m.K
        /// </summary>
        public const float Convert_kJperday_to_Watts = 0.2777777778 / 24.0;
    }
}

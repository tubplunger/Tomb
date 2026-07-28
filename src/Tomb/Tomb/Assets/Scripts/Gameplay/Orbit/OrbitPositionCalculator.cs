using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Orbit
{
    public static class OrbitPositionCalculator
    {
        private const float MinutesPerEarthRotation = 1440f;

        public static OrbitSnapshot CreateSnapshot(
            float orbitProgress,
            int completedOrbits,
            OrbitSettings settings)
        {
            float normalizedProgress =
                Mathf.Repeat(orbitProgress, 1f);

            float orbitAngleDegrees =
                normalizedProgress * 360f;

            float orbitAngleRadians =
                orbitAngleDegrees * Mathf.Deg2Rad;

            float inclinationRadians =
                settings.OrbitalInclinationDegrees *
                Mathf.Deg2Rad;

            float approximateLatitude =
                Mathf.Asin(
                    Mathf.Sin(inclinationRadians) *
                    Mathf.Sin(orbitAngleRadians)
                ) * Mathf.Rad2Deg;

            float earthRotationDegrees =
                CalculateEarthRotationDegrees(
                    normalizedProgress,
                    completedOrbits,
                    settings.OrbitDurationGameMinutes
                );

            float approximateLongitude =
                NormalizeLongitude(
                    settings.StartingLongitudeDegrees +
                    orbitAngleDegrees -
                    earthRotationDegrees
                );

            return new OrbitSnapshot(
                normalizedProgress,
                orbitAngleDegrees,
                approximateLatitude,
                approximateLongitude,
                completedOrbits
            );
        }

        private static float CalculateEarthRotationDegrees(
            float orbitProgress,
            int completedOrbits,
            int orbitDurationGameMinutes)
        {
            float totalGameMinutes =
                completedOrbits *
                orbitDurationGameMinutes +
                orbitProgress *
                orbitDurationGameMinutes;

            return
                totalGameMinutes /
                MinutesPerEarthRotation *
                360f;
        }

        private static float NormalizeLongitude(
            float longitude)
        {
            return Mathf.Repeat(
                longitude + 180f,
                360f
            ) - 180f;
        }
    }
}
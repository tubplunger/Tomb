using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Gameplay.Orbit
{
    [Serializable]
    public readonly struct OrbitSnapshot
    {
        public readonly float OrbitProgress;
        public readonly float OrbitAngleDegrees;
        public readonly float ApproximateLatitude;
        public readonly float ApproximateLongitude;
        public readonly int CompletedOrbits;

        public OrbitSnapshot (float orbitProgress, float orbitAngleDegrees, float approximateLatitude, float approximateLongitude, int completedOrbits)
        {
            OrbitProgress = orbitProgress;
            OrbitAngleDegrees = orbitAngleDegrees;
            ApproximateLatitude = approximateLatitude;
            ApproximateLongitude = approximateLongitude;
            CompletedOrbits = completedOrbits;
        }

        public override string ToString()
        {
            return
                $"Orbit {CompletedOrbits + 1} | " +
                $"{OrbitProgress * 100f:0.0}% | " +
                $"Lat {ApproximateLatitude:0.0}° | " +
                $"Lon {ApproximateLongitude:0.0}°";
        }
    }
}
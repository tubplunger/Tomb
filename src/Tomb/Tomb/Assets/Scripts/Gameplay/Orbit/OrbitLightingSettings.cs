using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Orbit
{
    [CreateAssetMenu(
        fileName = "OrbitLightingSettings",
        menuName = "Tomb/Orbit/Orbit Lighting Settings"
    )]
    public sealed class OrbitLightingSettings : ScriptableObject
    {
        [Header("Eclipse Region")]
        [Tooltip(
            "Normalized orbit position where eclipse begins."
        )]
        [Range(0f, 1f)]
        [SerializeField]
        private float eclipseStartProgress = 0.35f;

        [Tooltip(
            "Normalized orbit position where eclipse ends."
        )]
        [Range(0f, 1f)]
        [SerializeField]
        private float eclipseEndProgress = 0.65f;

        [Header("Solar Output")]
        [Range(0f, 1f)]
        [SerializeField]
        private float sunlightGenerationMultiplier = 1f;

        [Range(0f, 1f)]
        [SerializeField]
        private float eclipseGenerationMultiplier = 0f;

        public float EclipseStartProgress =>
            Mathf.Repeat(eclipseStartProgress, 1f);

        public float EclipseEndProgress =>
            Mathf.Repeat(eclipseEndProgress, 1f);

        public float SunlightGenerationMultiplier =>
            Mathf.Clamp01(sunlightGenerationMultiplier);

        public float EclipseGenerationMultiplier =>
            Mathf.Clamp01(eclipseGenerationMultiplier);

        public bool IsInEclipse(float orbitProgress)
        {
            float progress =
                Mathf.Repeat(orbitProgress, 1f);

            float start =
                EclipseStartProgress;

            float end =
                EclipseEndProgress;

            if (Mathf.Approximately(start, end))
                return false;

            // Normal non-wrapping region, such as 0.35 to 0.65.
            if (start < end)
            {
                return progress >= start &&
                       progress < end;
            }

            // Wrapping region, such as 0.85 to 0.15.
            return progress >= start ||
                   progress < end;
        }

        private void OnValidate()
        {
            eclipseStartProgress =
                Mathf.Repeat(eclipseStartProgress, 1f);

            eclipseEndProgress =
                Mathf.Repeat(eclipseEndProgress, 1f);

            sunlightGenerationMultiplier =
                Mathf.Clamp01(
                    sunlightGenerationMultiplier
                );

            eclipseGenerationMultiplier =
                Mathf.Clamp01(
                    eclipseGenerationMultiplier
                );
        }
    }
}
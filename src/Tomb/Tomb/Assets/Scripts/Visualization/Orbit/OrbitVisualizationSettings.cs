using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Visualization.Orbit
{
    [CreateAssetMenu(
        fileName = "OrbitVisualizationSettings",
        menuName = "Tomb/Visualization/Orbit Visualization Settings"
    )]
    public sealed class OrbitVisualizationSettings :
        ScriptableObject
    {
        [Header("Scale")]
        [Min(0.1f)]
        [SerializeField]
        private float earthRadius = 2.5f;

        [Min(0.1f)]
        [SerializeField]
        private float orbitRadius = 4.5f;

        [Min(0.01f)]
        [SerializeField]
        private float stationScale = 0.2f;

        [Header("Earth Rotation")]
        [Tooltip(
            "One full Earth rotation in game minutes."
        )]
        [Min(1f)]
        [SerializeField]
        private float earthRotationGameMinutes = 1440f;

        [SerializeField]
        private float earthRotationOffsetDegrees;

        [Header("Orbit Ring")]
        [Range(16, 256)]
        [SerializeField]
        private int orbitRingSegments = 96;

        [Min(0.001f)]
        [SerializeField]
        private float orbitRingWidth = 0.025f;

        [Header("Station Appearance")]
        [SerializeField]
        private Material sunlightStationMaterial;

        [SerializeField]
        private Material eclipseStationMaterial;

        public float EarthRadius =>
            earthRadius;

        public float OrbitRadius =>
            orbitRadius;

        public float StationScale =>
            stationScale;

        public float EarthRotationGameMinutes =>
            earthRotationGameMinutes;

        public float EarthRotationOffsetDegrees =>
            earthRotationOffsetDegrees;

        public int OrbitRingSegments =>
            orbitRingSegments;

        public float OrbitRingWidth =>
            orbitRingWidth;

        public Material SunlightStationMaterial =>
            sunlightStationMaterial;

        public Material EclipseStationMaterial =>
            eclipseStationMaterial;

        private void OnValidate()
        {
            earthRadius = Mathf.Max(
                0.1f,
                earthRadius
            );

            orbitRadius = Mathf.Max(
                earthRadius + 0.1f,
                orbitRadius
            );

            stationScale = Mathf.Max(
                0.01f,
                stationScale
            );

            earthRotationGameMinutes = Mathf.Max(
                1f,
                earthRotationGameMinutes
            );

            orbitRingSegments = Mathf.Clamp(
                orbitRingSegments,
                16,
                256
            );

            orbitRingWidth = Mathf.Max(
                0.001f,
                orbitRingWidth
            );
        }
    }
}
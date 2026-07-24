using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Orbit
{
    [CreateAssetMenu(
        fileName = "OrbitSettings",
        menuName = "Tomb/Orbit/Orbit Settings"
    )]
    public sealed class OrbitSettings : ScriptableObject
    {
        [Header("Orbit Timing")]
        [Min(1)]
        [SerializeField]
        private int orbitDurationGameMinutes = 90;

        [Range(0f, 1f)]
        [SerializeField]
        private float startingOrbitProgress;

        [Header("Orbit Shape")]
        [Range(0f, 90f)]
        [SerializeField]
        private float orbitalInclinationDegrees = 51.6f;

        [Range(-180f, 180f)]
        [SerializeField]
        private float startingLongitudeDegrees;

        [Header("Display")]
        [Min(0.01f)]
        [SerializeField]
        private float visualOrbitRadius = 15f;

        public int OrbitDurationGameMinutes =>
            Mathf.Max(1, orbitDurationGameMinutes);

        public float StartingOrbitProgress =>
            Mathf.Repeat(startingOrbitProgress, 1f);

        public float OrbitalInclinationDegrees =>
            orbitalInclinationDegrees;

        public float StartingLongitudeDegrees =>
            startingLongitudeDegrees;

        public float VisualOrbitRadius =>
            visualOrbitRadius;

        private void OnValidate()
        {
            orbitDurationGameMinutes =
                Mathf.Max(1, orbitDurationGameMinutes);

            startingOrbitProgress =
                Mathf.Repeat(startingOrbitProgress, 1f);

            visualOrbitRadius =
                Mathf.Max(0.01f, visualOrbitRadius);
        }
    }
}
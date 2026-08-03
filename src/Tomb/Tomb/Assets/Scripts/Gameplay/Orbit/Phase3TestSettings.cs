using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Orbit
{
    [CreateAssetMenu(
        fileName = "Phase3TestSettings",
        menuName = "Tomb/Development/Phase 3 Test Settings"
    )]
    public sealed class Phase3TestSettings :
        ScriptableObject
    {
        [Header("Orbit Test Positions")]
        [Range(0f, 1f)]
        [SerializeField]
        private float sunlightProgress = 0.10f;

        [Range(0f, 1f)]
        [SerializeField]
        private float eclipseProgress = 0.50f;

        [Range(0f, 1f)]
        [SerializeField]
        private float postEclipseProgress = 0.75f;

        [Header("Power Test")]
        [SerializeField]
        private string solarArrayMachineId =
            "solar_array";

        [SerializeField]
        private string batteryMachineId =
            "battery_bank";

        [SerializeField]
        private string communicationsMachineId =
            "communications_array";

        public float SunlightProgress =>
            sunlightProgress;

        public float EclipseProgress =>
            eclipseProgress;

        public float PostEclipseProgress =>
            postEclipseProgress;

        public string SolarArrayMachineId =>
            solarArrayMachineId;

        public string BatteryMachineId =>
            batteryMachineId;

        public string CommunicationsMachineId =>
            communicationsMachineId;
    }
}
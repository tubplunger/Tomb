using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Station
{
    [CreateAssetMenu(
        fileName = "StationTestSettings",
        menuName = "Tomb/Development/Station Test Settings"
    )]
    public sealed class StationTestSettings : ScriptableObject
    {
        [Header("Test IDs")]
        [SerializeField]
        private string oxygenResourceId = "oxygen";

        [SerializeField]
        private string waterResourceId = "water";

        [SerializeField]
        private string repairMaterialsResourceId =
            "repair_materials";

        [SerializeField]
        private string oxygenGeneratorMachineId =
            "oxygen_generator";

        [SerializeField]
        private string communicationsMachineId =
            "communications_array";

        [SerializeField]
        private string solarArrayMachineId =
            "solar_array";

        [SerializeField]
        private string batteryMachineId =
            "battery_bank";

        [Header("Test Values")]
        [Min(0f)]
        [SerializeField]
        private float lowOxygenAmount = 10f;

        [Min(0f)]
        [SerializeField]
        private float machineDamageAmount = 40f;

        public string OxygenResourceId =>
            oxygenResourceId;

        public string WaterResourceId =>
            waterResourceId;

        public string RepairMaterialsResourceId =>
            repairMaterialsResourceId;

        public string OxygenGeneratorMachineId =>
            oxygenGeneratorMachineId;

        public string CommunicationsMachineId =>
            communicationsMachineId;

        public string SolarArrayMachineId =>
            solarArrayMachineId;

        public string BatteryMachineId =>
            batteryMachineId;

        public float LowOxygenAmount =>
            lowOxygenAmount;

        public float MachineDamageAmount =>
            machineDamageAmount;
    }
}
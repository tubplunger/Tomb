using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Power
{
    [CreateAssetMenu(
        fileName = "PowerSettings",
        menuName = "Tomb/Power/Power Settings"
    )]
    public sealed class PowerSettings : ScriptableObject
    {
        [Header("Battery")]
        [SerializeField]
        private string batteryMachineId = "battery_bank";

        [Min(0.01f)]
        [SerializeField]
        private float batteryCapacity = 100f;

        [Min(0f)]
        [SerializeField]
        private float startingBatteryCharge = 80f;

        [Min(0f)]
        [SerializeField]
        private float maximumChargePerGameMinute = 5f;

        [Min(0f)]
        [SerializeField]
        private float maximumDischargePerGameMinute = 5f;

        public string BatteryMachineId =>
            batteryMachineId;

        public float BatteryCapacity =>
            batteryCapacity;

        public float StartingBatteryCharge =>
            Mathf.Clamp(
                startingBatteryCharge,
                0f,
                batteryCapacity
            );

        public float MaximumChargePerGameMinute =>
            maximumChargePerGameMinute;

        public float MaximumDischargePerGameMinute =>
            maximumDischargePerGameMinute;

        private void OnValidate()
        {
            batteryMachineId =
                batteryMachineId?.Trim();

            batteryCapacity =
                Mathf.Max(0.01f, batteryCapacity);

            startingBatteryCharge =
                Mathf.Clamp(
                    startingBatteryCharge,
                    0f,
                    batteryCapacity
                );
        }
    }
}
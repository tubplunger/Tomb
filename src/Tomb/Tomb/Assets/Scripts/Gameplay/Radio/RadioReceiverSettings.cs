using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Radio
{
    [CreateAssetMenu(
        fileName = "RadioReceiverSettings",
        menuName = "Tomb/Radio/Radio Receiver Settings"
    )]
    public sealed class RadioReceiverSettings :
        ScriptableObject
    {
        [Header("Hardware")]
        [SerializeField]
        private string communicationsMachineId =
            "communications_array";

        [Header("Detection")]
        [Range(0f, 1f)]
        [SerializeField]
        private float automaticDetectionThreshold = 0.35f;

        [Range(0f, 1f)]
        [SerializeField]
        private float minimumReceivableStrength = 0.15f;

        [Header("Signal Quality")]
        [Range(0f, 1f)]
        [SerializeField]
        private float baseStaticAmount = 0.05f;

        [Range(0f, 1f)]
        [SerializeField]
        private float damagedReceiverStaticMultiplier = 0.75f;

        [Header("Frequency Tuning")]
        [Min(0f)]
        [SerializeField]
        private float minimumFrequencyMHz = 100f;

        [Min(0f)]
        [SerializeField]
        private float maximumFrequencyMHz = 160f;

        [Min(0.01f)]
        [SerializeField]
        private float frequencyStepMHz = 0.1f;

        [Min(0.01f)]
        [SerializeField]
        private float tuningToleranceMHz = 0.15f;

        [Min(0.01f)]
        [SerializeField]
        private float coarseFrequencyStepMHz = 1f;

        public float CoarseFrequencyStepMHz =>
            coarseFrequencyStepMHz;

        public string CommunicationsMachineId =>
            communicationsMachineId;

        public float AutomaticDetectionThreshold =>
            automaticDetectionThreshold;

        public float MinimumReceivableStrength =>
            minimumReceivableStrength;

        public float BaseStaticAmount =>
            baseStaticAmount;

        public float DamagedReceiverStaticMultiplier =>
            damagedReceiverStaticMultiplier;

        public float MinimumFrequencyMHz =>
            minimumFrequencyMHz;

        public float MaximumFrequencyMHz =>
            maximumFrequencyMHz;

        public float FrequencyStepMHz =>
            frequencyStepMHz;

        public float TuningToleranceMHz =>
            tuningToleranceMHz;

        private void OnValidate()
        {
            communicationsMachineId =
                communicationsMachineId?.Trim();

            automaticDetectionThreshold =
                Mathf.Clamp01(
                    automaticDetectionThreshold
                );

            minimumReceivableStrength =
                Mathf.Clamp01(
                    minimumReceivableStrength
                );

            baseStaticAmount =
                Mathf.Clamp01(baseStaticAmount);

            damagedReceiverStaticMultiplier =
                Mathf.Clamp01(
                    damagedReceiverStaticMultiplier
                );

            minimumFrequencyMHz =
                Mathf.Max(0f, minimumFrequencyMHz);

            maximumFrequencyMHz =
                Mathf.Max(
                    minimumFrequencyMHz,
                    maximumFrequencyMHz
                );

            frequencyStepMHz =
                Mathf.Max(
                    0.01f,
                    frequencyStepMHz
                );

            tuningToleranceMHz =
                Mathf.Max(
                    0.01f,
                    tuningToleranceMHz
                );
        }
    }
}
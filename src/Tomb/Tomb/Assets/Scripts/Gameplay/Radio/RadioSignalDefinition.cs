using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Radio
{
    [CreateAssetMenu(
        fileName = "RadioSignalDefinition",
        menuName = "Tomb/Radio/Radio Signal"
    )]
    public sealed class RadioSignalDefinition :
        ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string signalId;

        [SerializeField]
        private string displayName;

        [Header("Source")]
        [SerializeField]
        private string sourceRegionId;

        [Header("Frequency")]
        [Min(0f)]
        [SerializeField]
        private float frequencyMHz = 100f;

        [Header("Signal Strength")]
        [Range(0f, 1f)]
        [SerializeField]
        private float baseSignalStrength = 1f;

        [Header("Availability")]
        [SerializeField]
        private bool requiresSunlight;

        [SerializeField]
        private bool enabledByDefault = true;

        public string SignalId =>
            signalId;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName)
                ? signalId
                : displayName;

        public string SourceRegionId =>
            sourceRegionId;

        public float FrequencyMHz => 
            frequencyMHz;

        public float BaseSignalStrength => 
            baseSignalStrength;

        public bool RequiresSunlight =>
            requiresSunlight;

        public bool EnabledByDefault =>
            enabledByDefault;

        private void OnValidate()
        {
            signalId = signalId?.Trim();
            displayName = displayName?.Trim();
            sourceRegionId = sourceRegionId?.Trim();

            frequencyMHz =
                Mathf.Max(0f, frequencyMHz);

            baseSignalStrength =
                Mathf.Clamp01(baseSignalStrength);
        }
    }
}
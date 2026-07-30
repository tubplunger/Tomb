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

        public bool RequiresSunlight =>
            requiresSunlight;

        public bool EnabledByDefault =>
            enabledByDefault;
    }
}
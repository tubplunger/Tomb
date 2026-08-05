using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    [CreateAssetMenu(
        fileName = "SpeakerDefinition",
        menuName = "Tomb/Radio/Speaker Definition"
    )]
    public sealed class SpeakerDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string speakerId;

        [SerializeField]
        private string displayName;

        [TextArea]
        [SerializeField]
        private string description;

        [Header("Presentation")]
        [SerializeField]
        private string callsign;

        public string SpeakerId => speakerId;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName)
                ? speakerId
                : displayName;

        public string Description => description;

        public string Callsign => callsign;

        private void OnValidate()
        {
            speakerId = speakerId?.Trim();
            displayName = displayName?.Trim();
            callsign = callsign?.Trim();
        }
    }
}
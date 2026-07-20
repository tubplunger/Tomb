using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Power
{
    [CreateAssetMenu(
        fileName = "MachinePowerProfile",
        menuName = "Tomb/Power/Machine Power Profile"
    )]
    public sealed class MachinePowerProfile : ScriptableObject
    {
        [Header("Consumption")]
        [Min(0f)]
        [SerializeField]
        private float demandPerGameMinute;

        [Header("Generation")]
        [Min(0f)]
        [SerializeField]
        private float generationPerGameMinute;

        public float DemandPerGameMinute =>
            demandPerGameMinute;

        public float GenerationPerGameMinute =>
            generationPerGameMinute;

        public bool IsConsumer =>
            demandPerGameMinute > 0f;

        public bool IsProducer =>
            generationPerGameMinute > 0f;
    }
}
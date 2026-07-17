using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Resources
{
    [CreateAssetMenu(
        fileName = "SurvivalConsumptionProfile",
        menuName = "Tomb/Resources/Survival Consumption Profile"
    )]
    public sealed class SurvivalConsumptionProfile
        : ScriptableObject
    {
        [Header("Consumed Each Game Hour")]
        [SerializeField]
        private List<ResourceAmount> hourlyConsumption = new();

        [Header("Produced Each Game Hour")]
        [SerializeField]
        private List<ResourceAmount> hourlyOutputs = new();

        public IReadOnlyList<ResourceAmount> HourlyConsumption =>
            hourlyConsumption;

        public IReadOnlyList<ResourceAmount> HourlyOutputs =>
            hourlyOutputs;
    }
}
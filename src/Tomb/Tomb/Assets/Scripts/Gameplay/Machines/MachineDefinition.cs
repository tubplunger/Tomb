using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Machines
{
    [CreateAssetMenu(
        fileName = "MachineDefinition",
        menuName = "Tomb/Machines/Machine Definition"
    )]
    public sealed class MachineDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string machineId;

        [SerializeField]
        private string displayName;

        [TextArea]
        [SerializeField]
        private string description;

        [Header("Classification")]
        [SerializeField]
        private MachineCategory category;

        [SerializeField]
        private MachinePriority priority;

        [Header("Starting State")]
        [SerializeField]
        private bool startingEnabled = true;

        [SerializeField]
        private bool canBeDisabled = true;

        [Min(0.01f)]
        [SerializeField]
        private float maximumCondition = 100f;

        [Min(0f)]
        [SerializeField]
        private float startingCondition = 100f;

        public string MachineId => machineId;
        public string DisplayName => displayName;
        public string Description => description;

        public MachineCategory Category => category;
        public MachinePriority Priority => priority;

        public bool StartingEnabled => startingEnabled;
        public bool CanBeDisabled => canBeDisabled;

        public float MaximumCondition => maximumCondition;

        public float StartingCondition =>
            Mathf.Clamp(
                startingCondition,
                0f,
                maximumCondition
            );

        private void OnValidate()
        {
            machineId = machineId?.Trim();
            displayName = displayName?.Trim();

            maximumCondition =
                Mathf.Max(0.01f, maximumCondition);

            startingCondition = Mathf.Clamp(
                startingCondition,
                0f,
                maximumCondition
            );
        }
    }
}
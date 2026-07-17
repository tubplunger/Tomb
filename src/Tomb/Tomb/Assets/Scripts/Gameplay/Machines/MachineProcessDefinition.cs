using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Tomb.Gameplay.Resources;

namespace Tomb.Gameplay.Machines
{
    [CreateAssetMenu(
        fileName = "MachineProcessDefinition",
        menuName = "Tomb/Machines/Machine Process Definition"
    )]
    public sealed class MachineProcessDefinition : ScriptableObject
    {
        [Header("Timing")]
        [Min(1)]
        [SerializeField]
        private int intervalGameMinutes = 10;

        [Header("Resources")]
        [SerializeField]
        private List<ResourceAmount> inputs = new();

        [SerializeField]
        private List<ResourceAmount> outputs = new();

        [Header("Efficiency")]
        [SerializeField]
        private bool scaleOutputsByEfficiency = true;

        public int IntervalGameMinutes =>
            Mathf.Max(1, intervalGameMinutes);

        public IReadOnlyList<ResourceAmount> Inputs =>
            inputs;

        public IReadOnlyList<ResourceAmount> Outputs =>
            outputs;

        public bool ScaleOutputsByEfficiency =>
            scaleOutputsByEfficiency;

        public string GetSummary()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append($"Every {IntervalGameMinutes}m");

            if (inputs.Count > 0)
            {
                builder.Append(" | In: ");
                AppendResourceList(builder, inputs);
            }

            if (outputs.Count > 0)
            {
                builder.Append(" | Out: ");
                AppendResourceList(builder, outputs);
            }

            return builder.ToString();
        }

        private static void AppendResourceList(
            StringBuilder builder,
            List<ResourceAmount> values)
        {
            bool addedAny = false;

            foreach (ResourceAmount value in values)
            {
                if (value == null || value.Resource == null)
                    continue;

                if (addedAny)
                    builder.Append(", ");

                builder.Append(value.Amount.ToString("0.##"));
                builder.Append(' ');
                builder.Append(value.Resource.DisplayName);

                addedAny = true;
            }

            if (!addedAny)
                builder.Append("None");
        }
    }
}
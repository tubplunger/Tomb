using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Tomb.Gameplay.Earth;

namespace Tomb.Gameplay.Radio
{
    public sealed class DebugSignalRowView :
        MonoBehaviour
    {
        [SerializeField]
        private TMP_Text nameText;

        [SerializeField]
        private TMP_Text frequencyText;

        [SerializeField]
        private TMP_Text availabilityText;

        [SerializeField]
        private TMP_Text strengthText;

        [SerializeField]
        private TMP_Text staticText;

        [SerializeField]
        private TMP_Text statusText;

        private RadioSignalRuntimeState state;
        private RadioVisibilitySystem visibilitySystem;

        public void Initialize(
            RadioSignalRuntimeState runtimeState,
            RadioVisibilitySystem visibility)
        {
            state = runtimeState;
            visibilitySystem = visibility;

            Refresh();
        }

        public void Refresh()
        {
            if (state == null ||
                visibilitySystem == null)
            {
                return;
            }

            RadioSignalDefinition definition =
                state.Definition;

            bool physicallyVisible =
                visibilitySystem.IsSignalVisible(
                    definition.SignalId
                );

            nameText.text =
                definition.DisplayName;

            frequencyText.text =
                $"{definition.FrequencyMHz:0.0} MHz";

            availabilityText.text =
                physicallyVisible
                    ? "IN RANGE"
                    : "OUT OF RANGE";

            strengthText.text =
                $"Strength: " +
                $"{state.CurrentStrength * 100f:0}%";

            staticText.text =
                $"Static: " +
                $"{state.StaticAmount * 100f:0}%";

            statusText.text =
                state.Status
                    .ToString()
                    .ToUpperInvariant();
        }
    }
}
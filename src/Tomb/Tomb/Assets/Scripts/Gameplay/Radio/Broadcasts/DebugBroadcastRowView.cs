using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    public sealed class DebugBroadcastRowView :
        MonoBehaviour
    {
        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text signalText;

        [SerializeField]
        private TMP_Text statusText;

        [SerializeField]
        private TMP_Text speakerText;

        [SerializeField]
        private TMP_Text priorityText;

        [SerializeField]
        private TMP_Text playsText;

        [SerializeField]
        private TMP_Text durationText;

        [SerializeField]
        private TMP_Text reasonText;

        private BroadcastRuntimeState state;

        public void Initialize(
            BroadcastRuntimeState runtimeState)
        {
            state = runtimeState;
            Refresh();
        }

        public void Refresh()
        {
            if (state == null)
                return;

            BroadcastDefinition definition =
                state.Definition;

            titleText.text = definition.Title;

            signalText.text =
                definition.Signal != null
                    ? definition.Signal.DisplayName
                    : "No Signal";

            statusText.text =
                state.Status.ToString()
                    .ToUpperInvariant();

            speakerText.text =
                definition.Speaker != null
                    ? definition.Speaker.DisplayName
                    : "Unknown Speaker";

            priorityText.text =
                $"Priority: {definition.Priority}";

            string playLimit =
                definition.RepeatMode ==
                BroadcastRepeatMode.Unlimited
                    ? "?"
                    : definition.MaximumPlayCount
                        .ToString();

            playsText.text =
                $"Plays: {state.PlayCount} / " +
                $"{playLimit}";

            durationText.text =
                $"Duration: " +
                $"{definition.EstimatedDurationSeconds:0.#}s";

            reasonText.text =
                $"Reason: {state.StatusReason}";
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Tomb.Gameplay.Machines
{
    public sealed class DebugMachineRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text priorityText;
        [SerializeField] private TMP_Text categoryText;
        [SerializeField] private TMP_Text conditionText;
        [SerializeField] private TMP_Text efficiencyText;

        [SerializeField]
        private RectTransform conditionFillRect;

        [SerializeField]
        private RectTransform conditionBackgroundRect;

        private MachineState machine;
        private Coroutine delayedBarRefresh;

        public void Initialize(MachineState machineState)
        {
            machine = machineState;
            Refresh();
        }

        public void Refresh()
        {
            if (machine == null)
                return;

            MachineDefinition definition = machine.Definition;

            nameText.text = definition.DisplayName;

            statusText.text =
                machine.Status.ToString().ToUpperInvariant();

            priorityText.text =
                $"Priority: {definition.Priority}";

            categoryText.text =
                $"Category: {definition.Category}";

            conditionText.text =
                $"Condition: {machine.CurrentCondition:0.##} / " +
                $"{machine.MaximumCondition:0.##}";

            efficiencyText.text =
                $"Efficiency: {machine.Efficiency * 100f:0.#}%";

            QueueConditionBarRefresh();
        }

        private void QueueConditionBarRefresh()
        {
            if (!isActiveAndEnabled)
                return;

            if (delayedBarRefresh != null)
                StopCoroutine(delayedBarRefresh);

            delayedBarRefresh =
                StartCoroutine(RefreshBarAfterLayout());
        }

        private IEnumerator RefreshBarAfterLayout()
        {
            yield return null;

            Canvas.ForceUpdateCanvases();

            if (conditionBackgroundRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    conditionBackgroundRect
                );
            }

            UpdateConditionBar();
            delayedBarRefresh = null;
        }

        private void UpdateConditionBar()
        {
            if (machine == null ||
                conditionFillRect == null ||
                conditionBackgroundRect == null)
            {
                return;
            }

            float backgroundWidth =
                conditionBackgroundRect.rect.width;

            if (backgroundWidth <= 0f)
                return;

            float fillWidth =
                backgroundWidth *
                Mathf.Clamp01(machine.NormalizedCondition);

            conditionFillRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                fillWidth
            );
        }
    }
}
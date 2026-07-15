using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Tomb.Gameplay.Resources
{
    public sealed class DebugResourceRowView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text nameText;

        [SerializeField]
        private TMP_Text amountText;

        [SerializeField]
        private TMP_Text statusText;

        [SerializeField]
        private RectTransform fillRect;

        [SerializeField]
        private RectTransform fillBackgroundRect;

        private ResourceState resource;

        public void Initialize(ResourceState resourceState)
        {
            resource = resourceState;
            Refresh();
        }

        public void Refresh()
        {
            if (resource == null)
                return;

            ResourceDefinition definition =
                resource.Definition;

            nameText.text = definition.DisplayName;

            amountText.text =
                $"{resource.CurrentAmount:0.##} / " +
                $"{resource.MaximumAmount:0.##} " +
                $"{definition.UnitLabel}";

            if (resource.IsEmpty)
            {
                statusText.text = "EMPTY";
            }
            else if (resource.IsLow)
            {
                statusText.text = "LOW";
            }
            else
            {
                statusText.text = "NORMAL";
            }

            UpdateFillBar();
        }

        private void UpdateFillBar()
        {
            if (fillRect == null ||
                fillBackgroundRect == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();

            float backgroundWidth =
                fillBackgroundRect.rect.width;

            float fillWidth =
                backgroundWidth *
                Mathf.Clamp01(resource.NormalizedAmount);

            fillRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                fillWidth
            );
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tomb.Gameplay.Radio.Responses;

namespace Tomb.Presentation.Radio
{
    public sealed class RadioResponseButtonView :
        MonoBehaviour
    {
        [SerializeField]
        private TMP_Text labelText;

        [SerializeField]
        private Button button;

        private RadioResponseDefinition response;
        private System.Action<
            RadioResponseDefinition> callback;

        public void Initialize(
            RadioResponseDefinition definition,
            bool interactable,
            System.Action<
                RadioResponseDefinition> onSelected)
        {
            response = definition;
            callback = onSelected;

            labelText.text =
                response.DisplayText;

            button.interactable =
                interactable;

            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(
                HandleClick
            );
        }

        private void HandleClick()
        {
            callback?.Invoke(response);
        }

        private void OnDestroy()
        {
            button?.onClick.RemoveListener(
                HandleClick
            );
        }
    }
}
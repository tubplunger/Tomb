using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tomb.Core.Services;

namespace Tomb.Core.Debugging.Overlay
{
    [RequireComponent(typeof(Button))]
    public sealed class DebugOverlayTab : MonoBehaviour
    {
        [SerializeField] private string tabId;
        [SerializeField] private GameObject contentPanel;

        private Button button;
        private DebugOverlaySystem overlaySystem;

        public string TabId => tabId;
        public GameObject ContentPanel => contentPanel;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(SelectTab);
        }

        private void Start()
        {
            overlaySystem = CoreServices.Get<DebugOverlaySystem>();
        }

        private void SelectTab()
        {
            overlaySystem.SetActiveTab(tabId);
        }

        private void OnDestroy()
        {
            button?.onClick.RemoveListener(SelectTab);
        }
    }
}
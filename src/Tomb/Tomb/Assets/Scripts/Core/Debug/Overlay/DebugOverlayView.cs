using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;
using Tomb.Core.Services;

namespace Tomb.Core.Debugging.Overlay
{
    public sealed class DebugOverlayView : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject overlayRoot;

        [Header("Tabs")]
        [SerializeField] private List<DebugOverlayTab> tabs = new();

        private EventBus eventBus;
        private DebugOverlaySystem overlaySystem;

        private void Start()
        {
            eventBus = CoreServices.Get<EventBus>();
            overlaySystem = CoreServices.Get<DebugOverlaySystem>();

            eventBus.Subscribe<DebugOverlayVisibilityChangedEvent>(
                OnVisibilityChanged
            );

            eventBus.Subscribe<DebugOverlayTabChangedEvent>(
                OnTabChanged
            );

            overlayRoot.SetActive(overlaySystem.IsVisible);
            ShowTab(overlaySystem.ActiveTabId);
        }

        private void OnDestroy()
        {
            eventBus?.Unsubscribe<DebugOverlayVisibilityChangedEvent>(
                OnVisibilityChanged
            );

            eventBus?.Unsubscribe<DebugOverlayTabChangedEvent>(
                OnTabChanged
            );
        }

        private void OnVisibilityChanged(
            DebugOverlayVisibilityChangedEvent visibilityEvent)
        {
            overlayRoot.SetActive(visibilityEvent.IsVisible);
        }

        private void OnTabChanged(
            DebugOverlayTabChangedEvent tabEvent)
        {
            ShowTab(tabEvent.TabId);
        }

        private void ShowTab(string tabId)
        {
            foreach (DebugOverlayTab tab in tabs)
            {
                if (tab.ContentPanel == null)
                    continue;

                tab.ContentPanel.SetActive(tab.TabId == tabId);
            }
        }
    }
}
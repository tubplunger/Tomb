using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Core.Debugging.Overlay
{
    public sealed class DebugOverlaySystem
    {
        private readonly EventBus eventBus;

        public bool IsVisible { get; private set; }
        public string ActiveTabId { get; private set; } = "overview";

        public DebugOverlaySystem(EventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public void Toggle()
        {
            SetVisible(!IsVisible);
        }

        public void SetVisible(bool visible)
        {
            if (IsVisible == visible)
                return;

            IsVisible = visible;

            eventBus.Publish(new DebugOverlayVisibilityChangedEvent(IsVisible));
        }

        public void SetActiveTab(string tabId)
        {
            if (string.IsNullOrWhiteSpace(tabId))
                return;

            if (ActiveTabId == tabId)
                return;

            ActiveTabId = tabId;

            eventBus.Publish(new DebugOverlayTabChangedEvent(ActiveTabId));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Core.Debugging.Overlay
{
    public readonly struct DebugOverlayVisibilityChangedEvent : IGameEvent
    {
        public readonly bool IsVisible;

        public DebugOverlayVisibilityChangedEvent(bool isVisible)
        {
            IsVisible = isVisible;
        }
    }

    public readonly struct DebugOverlayTabChangedEvent : IGameEvent
    {
        public readonly string TabId;

        public DebugOverlayTabChangedEvent(string tabID)
        {
            TabId = tabID;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Radio
{
    public readonly struct RadioSignalBecameVisibleEvent :
        IGameEvent
    {
        public readonly RadioSignalDefinition Signal;

        public RadioSignalBecameVisibleEvent(
            RadioSignalDefinition signal)
        {
            Signal = signal;
        }
    }

    public readonly struct RadioSignalBecameHiddenEvent :
        IGameEvent
    {
        public readonly RadioSignalDefinition Signal;

        public RadioSignalBecameHiddenEvent(
            RadioSignalDefinition signal)
        {
            Signal = signal;
        }
    }

    public readonly struct RadioVisibilityUpdatedEvent :
        IGameEvent
    {
        public readonly int VisibleSignalCount;

        public RadioVisibilityUpdatedEvent(
            int visibleSignalCount)
        {
            VisibleSignalCount =
                visibleSignalCount;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Orbit
{
    public readonly struct OrbitUpdatedEvent : IGameEvent
    {
        public readonly OrbitSnapshot Snapshot;

        public OrbitUpdatedEvent(OrbitSnapshot snapshot)
        {
            Snapshot = snapshot;
        }
    }

    public readonly struct OrbitCompletedEvent : IGameEvent
    {
        public readonly int CompletedOrbitCount;

        public OrbitCompletedEvent(int completedOrbitCount)
        {
            CompletedOrbitCount = completedOrbitCount;
        }
    }

    public readonly struct OrbitPositionSetEvent : IGameEvent
    {
        public readonly OrbitSnapshot Snapshot;
        public readonly string Reason;

        public OrbitPositionSetEvent(
            OrbitSnapshot snapshot,
            string reason)
        {
            Snapshot = snapshot;
            Reason = reason;
        }
    }

    public readonly struct OrbitRestoredFromSaveEvent : IGameEvent
    {
        public readonly OrbitSnapshot Snapshot;

        public OrbitRestoredFromSaveEvent(
            OrbitSnapshot snapshot)
        {
            Snapshot = snapshot;
        }
    }
}
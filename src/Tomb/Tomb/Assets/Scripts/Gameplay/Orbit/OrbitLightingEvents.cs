using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Orbit
{
    public readonly struct OrbitLightingUpdatedEvent :
        IGameEvent
    {
        public readonly OrbitLightingSnapshot Snapshot;

        public OrbitLightingUpdatedEvent(
            OrbitLightingSnapshot snapshot)
        {
            Snapshot = snapshot;
        }
    }

    public readonly struct EnteredSunlightEvent :
        IGameEvent
    {
        public readonly OrbitLightingSnapshot Snapshot;

        public EnteredSunlightEvent(
            OrbitLightingSnapshot snapshot)
        {
            Snapshot = snapshot;
        }
    }

    public readonly struct EnteredEclipseEvent :
        IGameEvent
    {
        public readonly OrbitLightingSnapshot Snapshot;

        public EnteredEclipseEvent(
            OrbitLightingSnapshot snapshot)
        {
            Snapshot = snapshot;
        }
    }
}
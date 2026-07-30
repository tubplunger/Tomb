using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Earth
{
    public readonly struct EarthRegionUpdatedEvent :
        IGameEvent
    {
        public readonly EarthRegionSnapshot Snapshot;

        public EarthRegionUpdatedEvent(
            EarthRegionSnapshot snapshot)
        {
            Snapshot = snapshot;
        }
    }

    public readonly struct EnteredEarthRegionEvent :
        IGameEvent
    {
        public readonly EarthRegionDefinition Region;

        public EnteredEarthRegionEvent(
            EarthRegionDefinition region)
        {
            Region = region;
        }
    }

    public readonly struct ExitedEarthRegionEvent :
        IGameEvent
    {
        public readonly EarthRegionDefinition Region;

        public ExitedEarthRegionEvent(
            EarthRegionDefinition region)
        {
            Region = region;
        }
    }

    public readonly struct EarthRegionBecameVisibleEvent :
        IGameEvent
    {
        public readonly EarthRegionDefinition Region;

        public EarthRegionBecameVisibleEvent(
            EarthRegionDefinition region)
        {
            Region = region;
        }
    }

    public readonly struct EarthRegionBecameHiddenEvent :
        IGameEvent
    {
        public readonly EarthRegionDefinition Region;

        public EarthRegionBecameHiddenEvent(
            EarthRegionDefinition region)
        {
            Region = region;
        }
    }
}
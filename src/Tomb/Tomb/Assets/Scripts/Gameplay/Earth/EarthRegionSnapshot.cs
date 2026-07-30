using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Gameplay.Earth
{
    [Serializable]
    public sealed class EarthRegionSnapshot
    {
        public EarthRegionDefinition CurrentRegion
        {
            get;
        }

        public IReadOnlyList<EarthRegionDefinition>
            VisibleRegions
        {
            get;
        }

        public float Latitude
        {
            get;
        }

        public float Longitude
        {
            get;
        }

        public EarthRegionSnapshot(
            EarthRegionDefinition currentRegion,
            IReadOnlyList<EarthRegionDefinition>
                visibleRegions,
            float latitude,
            float longitude)
        {
            CurrentRegion = currentRegion;
            VisibleRegions = visibleRegions;
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
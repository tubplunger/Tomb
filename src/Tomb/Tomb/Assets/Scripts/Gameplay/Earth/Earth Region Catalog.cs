using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Earth
{
    [CreateAssetMenu(
        fileName = "EarthRegionCatalog",
        menuName = "Tomb/Earth/Earth Region Catalog"
    )]
    public sealed class EarthRegionCatalog :
        ScriptableObject
    {
        [SerializeField]
        private List<EarthRegionDefinition> regions =
            new();

        public IReadOnlyList<EarthRegionDefinition>
            Regions => regions;

        public EarthRegionDefinition FindById(
            string regionId)
        {
            if (string.IsNullOrWhiteSpace(regionId))
                return null;

            foreach (EarthRegionDefinition region in regions)
            {
                if (region == null)
                    continue;

                if (region.RegionId == regionId)
                    return region;
            }

            return null;
        }

        private void OnValidate()
        {
            HashSet<string> usedIds =
                new();

            foreach (EarthRegionDefinition region in regions)
            {
                if (region == null)
                    continue;

                if (string.IsNullOrWhiteSpace(
                        region.RegionId))
                {
                    Debug.LogWarning(
                        $"Earth region '{region.name}' " +
                        "has no Region ID.",
                        region
                    );

                    continue;
                }

                if (!usedIds.Add(region.RegionId))
                {
                    Debug.LogWarning(
                        $"Duplicate Earth Region ID: " +
                        $"{region.RegionId}",
                        region
                    );
                }
            }
        }
    }
}
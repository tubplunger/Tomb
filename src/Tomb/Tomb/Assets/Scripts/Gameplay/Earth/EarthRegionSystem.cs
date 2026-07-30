using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;
using Tomb.Gameplay.Orbit;

namespace Tomb.Gameplay.Earth
{
    public sealed class EarthRegionSystem
    {
        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;
        private readonly OrbitSystem orbitSystem;
        private readonly EarthRegionCatalog catalog;
        private readonly EarthRegionVisibilitySettings
            visibilitySettings;

        private EarthRegionSnapshot currentSnapshot;

        private readonly HashSet<string>
            visibleRegionIds = new();

        public EarthRegionSnapshot CurrentSnapshot =>
            currentSnapshot;

        public EarthRegionDefinition CurrentRegion =>
            currentSnapshot?.CurrentRegion;

        public EarthRegionSystem(
            EventBus eventBus,
            DebugLogger debugLogger,
            OrbitSystem orbitSystem,
            EarthRegionCatalog catalog,
            EarthRegionVisibilitySettings
                visibilitySettings)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;
            this.orbitSystem = orbitSystem;
            this.catalog = catalog;
            this.visibilitySettings =
                visibilitySettings;

            eventBus.Subscribe<OrbitUpdatedEvent>(
                OnOrbitUpdated
            );

            eventBus.Subscribe<AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );

            Recalculate(
                orbitSystem.CurrentSnapshot,
                false
            );

            debugLogger.Log(
                "Earth region system initialized.",
                "Earth"
            );
        }

        public bool IsRegionVisible(
            string regionId)
        {
            return !string.IsNullOrWhiteSpace(regionId) &&
                   visibleRegionIds.Contains(regionId);
        }

        private void OnOrbitUpdated(
            OrbitUpdatedEvent orbitEvent)
        {
            Recalculate(
                orbitEvent.Snapshot,
                true
            );
        }

        private void OnAllSaveDataRestored(
            AllSaveDataRestoredEvent restoredEvent)
        {
            Recalculate(
                orbitSystem.CurrentSnapshot,
                true
            );
        }

        private void Recalculate(
            OrbitSnapshot orbitSnapshot,
            bool publishEvents)
        {
            EarthRegionDefinition previousRegion =
                currentSnapshot?.CurrentRegion;

            HashSet<string> previousVisibleIds =
                new(visibleRegionIds);

            EarthRegionDefinition currentRegion =
                FindCurrentRegion(
                    orbitSnapshot.ApproximateLatitude,
                    orbitSnapshot.ApproximateLongitude
                );

            List<EarthRegionDefinition>
                visibleRegions =
                    FindVisibleRegions(
                        orbitSnapshot
                            .ApproximateLatitude,
                        orbitSnapshot
                            .ApproximateLongitude
                    );

            visibleRegionIds.Clear();

            foreach (EarthRegionDefinition region
                     in visibleRegions)
            {
                visibleRegionIds.Add(
                    region.RegionId
                );
            }

            currentSnapshot =
                new EarthRegionSnapshot(
                    currentRegion,
                    visibleRegions,
                    orbitSnapshot
                        .ApproximateLatitude,
                    orbitSnapshot
                        .ApproximateLongitude
                );

            if (!publishEvents)
                return;

            eventBus.Publish(
                new EarthRegionUpdatedEvent(
                    currentSnapshot
                )
            );

            PublishCurrentRegionChanges(
                previousRegion,
                currentRegion
            );

            PublishVisibilityChanges(
                previousVisibleIds,
                visibleRegions
            );
        }

        private EarthRegionDefinition
            FindCurrentRegion(
                float latitude,
                float longitude)
        {
            foreach (EarthRegionDefinition region
                     in catalog.Regions)
            {
                if (region == null)
                    continue;

                if (region.Contains(
                        latitude,
                        longitude))
                {
                    return region;
                }
            }

            return null;
        }

        private List<EarthRegionDefinition>
            FindVisibleRegions(
                float stationLatitude,
                float stationLongitude)
        {
            List<EarthRegionDefinition> results =
                new();

            foreach (EarthRegionDefinition region
                     in catalog.Regions)
            {
                if (region == null)
                    continue;

                float angularDistance =
                    GeographicMath
                        .CalculateAngularDistanceDegrees(
                            stationLatitude,
                            stationLongitude,
                            region.RadioLatitude,
                            region.RadioLongitude
                        );

                if (angularDistance <=
                    visibilitySettings
                        .VisibilityAngleDegrees)
                {
                    results.Add(region);
                }
            }

            return results;
        }

        private void PublishCurrentRegionChanges(
            EarthRegionDefinition previousRegion,
            EarthRegionDefinition newRegion)
        {
            if (previousRegion == newRegion)
                return;

            if (previousRegion != null)
            {
                eventBus.Publish(
                    new ExitedEarthRegionEvent(
                        previousRegion
                    )
                );

                debugLogger.Log(
                    $"Station left region: " +
                    $"{previousRegion.DisplayName}",
                    "Earth"
                );
            }

            if (newRegion != null)
            {
                eventBus.Publish(
                    new EnteredEarthRegionEvent(
                        newRegion
                    )
                );

                debugLogger.Log(
                    $"Station entered region: " +
                    $"{newRegion.DisplayName}",
                    "Earth"
                );
            }
        }

        private void PublishVisibilityChanges(
            HashSet<string> previousVisibleIds,
            List<EarthRegionDefinition>
                newVisibleRegions)
        {
            HashSet<string> newVisibleIds =
                new();

            foreach (EarthRegionDefinition region
                     in newVisibleRegions)
            {
                newVisibleIds.Add(region.RegionId);

                if (!previousVisibleIds.Contains(
                        region.RegionId))
                {
                    eventBus.Publish(
                        new EarthRegionBecameVisibleEvent(
                            region
                        )
                    );

                    debugLogger.Log(
                        $"Radio region visible: " +
                        $"{region.DisplayName}",
                        "Radio"
                    );
                }
            }

            foreach (string previousId
                     in previousVisibleIds)
            {
                if (newVisibleIds.Contains(previousId))
                    continue;

                EarthRegionDefinition region =
                    catalog.FindById(previousId);

                if (region == null)
                    continue;

                eventBus.Publish(
                    new EarthRegionBecameHiddenEvent(
                        region
                    )
                );

                debugLogger.Log(
                    $"Radio region no longer visible: " +
                    $"{region.DisplayName}",
                    "Radio"
                );
            }
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<OrbitUpdatedEvent>(
                OnOrbitUpdated
            );

            eventBus.Unsubscribe<
                AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );
        }
    }
}
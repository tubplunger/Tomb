using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;
using Tomb.Gameplay.Earth;
using Tomb.Gameplay.Orbit;

namespace Tomb.Gameplay.Radio
{
    public sealed class RadioVisibilitySystem
    {
        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;
        private readonly EarthRegionSystem earthRegionSystem;
        private readonly OrbitLightingSystem orbitLightingSystem;
        private readonly RadioSignalCatalog catalog;

        private readonly List<RadioSignalDefinition>
            visibleSignals = new();

        private readonly HashSet<string>
            visibleSignalIds = new();

        public IReadOnlyList<RadioSignalDefinition>
            VisibleSignals => visibleSignals;

        public RadioVisibilitySystem(
            EventBus eventBus,
            DebugLogger debugLogger,
            EarthRegionSystem earthRegionSystem,
            OrbitLightingSystem orbitLightingSystem,
            RadioSignalCatalog catalog)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;
            this.earthRegionSystem = earthRegionSystem;
            this.orbitLightingSystem = orbitLightingSystem;
            this.catalog = catalog;

            eventBus.Subscribe<EarthRegionUpdatedEvent>(
                OnEarthRegionUpdated
            );

            eventBus.Subscribe<OrbitLightingUpdatedEvent>(
                OnOrbitLightingUpdated
            );

            eventBus.Subscribe<AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );

            Recalculate(false);

            debugLogger.Log(
                "Radio visibility system initialized.",
                "Radio"
            );
        }

        public bool IsSignalVisible(
            string signalId)
        {
            return !string.IsNullOrWhiteSpace(signalId) &&
                   visibleSignalIds.Contains(signalId);
        }

        private void OnEarthRegionUpdated(
            EarthRegionUpdatedEvent regionEvent)
        {
            Recalculate(true);
        }

        private void OnOrbitLightingUpdated(
            OrbitLightingUpdatedEvent lightingEvent)
        {
            Recalculate(true);
        }

        private void OnAllSaveDataRestored(
            AllSaveDataRestoredEvent restoredEvent)
        {
            Recalculate(true);
        }

        private void Recalculate(
            bool publishEvents)
        {
            HashSet<string> previousVisibleIds =
                new(visibleSignalIds);

            visibleSignals.Clear();
            visibleSignalIds.Clear();

            foreach (RadioSignalDefinition signal
                    in catalog.Signals)
            {
                if (signal == null ||
                    !signal.EnabledByDefault)
                {
                    continue;
                }

                bool regionVisible =
                    earthRegionSystem.IsRegionVisible(
                        signal.SourceRegionId
                    );

                Debug.Log(
                    $"[RADIO VISIBILITY] " +
                    $"Signal={signal.SignalId} | " +
                    $"SourceRegion={signal.SourceRegionId} | " +
                    $"RegionVisible={regionVisible}"
                );

                if (!regionVisible)
                {
                    continue;
                }

                if (signal.RequiresSunlight &&
                    orbitLightingSystem.IsInEclipse)
                {
                    continue;
                }

                visibleSignals.Add(signal);
                visibleSignalIds.Add(signal.SignalId);

                Debug.Log(
                    $"[RADIO VISIBILITY ADD] " +
                    $"{signal.SignalId} added to visible signals."
                );
            }

            if (!publishEvents)
                return;

            foreach (RadioSignalDefinition signal
                     in visibleSignals)
            {
                if (previousVisibleIds.Contains(
                        signal.SignalId))
                {
                    continue;
                }

                Debug.Log(
                    $"[RADIO VISIBILITY FINAL] " +
                    $"Visible count = {visibleSignals.Count} | " +
                    $"NA = {IsSignalVisible("na_emergency_broadcast")} | " +
                    $"Europe = {IsSignalVisible("europe_automated_beacon")}"
                );

                eventBus.Publish(
                    new RadioSignalBecameVisibleEvent(
                        signal
                    )
                );

                debugLogger.Log(
                    $"Signal acquired: " +
                    $"{signal.DisplayName}",
                    "Radio"
                );
            }

            foreach (string previousSignalId
                     in previousVisibleIds)
            {
                if (visibleSignalIds.Contains(
                        previousSignalId))
                {
                    continue;
                }

                RadioSignalDefinition hiddenSignal =
                    FindSignal(previousSignalId);

                if (hiddenSignal == null)
                    continue;

                eventBus.Publish(
                    new RadioSignalBecameHiddenEvent(
                        hiddenSignal
                    )
                );

                debugLogger.Log(
                    $"Signal lost: " +
                    $"{hiddenSignal.DisplayName}",
                    "Radio"
                );
            }

            eventBus.Publish(
                new RadioVisibilityUpdatedEvent(
                    visibleSignals.Count
                )
            );
        }

        private RadioSignalDefinition FindSignal(
            string signalId)
        {
            foreach (RadioSignalDefinition signal
                     in catalog.Signals)
            {
                if (signal != null &&
                    signal.SignalId == signalId)
                {
                    return signal;
                }
            }

            return null;
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<EarthRegionUpdatedEvent>(
                OnEarthRegionUpdated
            );

            eventBus.Unsubscribe<OrbitLightingUpdatedEvent>(
                OnOrbitLightingUpdated
            );

            eventBus.Unsubscribe<AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );
        }
    }
}
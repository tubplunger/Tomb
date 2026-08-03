using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;
using Tomb.Gameplay.Earth;
using Tomb.Gameplay.Orbit;
using Tomb.Gameplay.Machines;

namespace Tomb.Gameplay.Radio
{
    public sealed class RadioVisibilitySystem
    {
        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;
        private readonly EarthRegionSystem
            earthRegionSystem;
        private readonly OrbitLightingSystem
            orbitLightingSystem;
        private readonly RadioSignalCatalog catalog;
        private readonly MachineSystem machineSystem;
        private readonly string communicationsMachineId;

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
            MachineSystem machineSystem,
            RadioSignalCatalog catalog,
            string communicationsMachineId =
                "communications_array")
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;
            this.earthRegionSystem = earthRegionSystem;
            this.orbitLightingSystem = orbitLightingSystem;
            this.machineSystem = machineSystem;
            this.catalog = catalog;
            this.communicationsMachineId =
                communicationsMachineId;

            eventBus.Subscribe<
                EarthRegionUpdatedEvent>(
                OnEarthRegionUpdated
            );

            eventBus.Subscribe<
                OrbitLightingUpdatedEvent>(
                OnOrbitLightingUpdated
            );

            eventBus.Subscribe<
                AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );

            eventBus.Subscribe<MachineStateChangedEvent>(
                OnMachineStateChanged
            );

            eventBus.Subscribe<MachineConditionChangedEvent>(
                OnMachineConditionChanged
            );

            Recalculate(false);

            debugLogger.Log(
                "Radio visibility system initialized.",
                "Radio"
            );
        }

        public bool IsCommunicationsAvailable()
        {
            if (!machineSystem.TryGetMachine(
                    communicationsMachineId,
                    out MachineState communications))
            {
                return false;
            }

            return communications.IsEnabled &&
                   !communications.IsBroken &&
                   communications.HasPower &&
                   !communications.IsInMaintenance;
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

        private void OnMachineStateChanged(
            MachineStateChangedEvent stateEvent)
        {
            if (stateEvent.MachineId !=
                communicationsMachineId)
            {
                return;
            }

            Recalculate(true);
        }

        private void OnMachineConditionChanged(
            MachineConditionChangedEvent conditionEvent)
        {
            if (conditionEvent.MachineId !=
                communicationsMachineId)
            {
                return;
            }

            Recalculate(true);
        }

        private void Recalculate(
            bool publishEvents)
        {
            HashSet<string> previousVisibleIds =
                new(visibleSignalIds);

            visibleSignals.Clear();
            visibleSignalIds.Clear();

            bool communicationsAvailable =
                IsCommunicationsAvailable();

            if (communicationsAvailable)
            {
                foreach (RadioSignalDefinition signal
                         in catalog.Signals)
                {
                    if (signal == null ||
                        !signal.EnabledByDefault)
                    {
                        continue;
                    }

                    if (!earthRegionSystem.IsRegionVisible(
                            signal.SourceRegionId))
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
                }
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
            eventBus.Unsubscribe<
                EarthRegionUpdatedEvent>(
                OnEarthRegionUpdated
            );

            eventBus.Unsubscribe<
                OrbitLightingUpdatedEvent>(
                OnOrbitLightingUpdated
            );

            eventBus.Unsubscribe<MachineStateChangedEvent>(
                OnMachineStateChanged
            );

            eventBus.Unsubscribe<MachineConditionChangedEvent>(
                OnMachineConditionChanged
            );

            eventBus.Unsubscribe<
                AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );
        }
    }
}
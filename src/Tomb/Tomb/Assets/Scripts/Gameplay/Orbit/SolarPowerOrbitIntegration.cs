using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;
using Tomb.Gameplay.Power;

namespace Tomb.Gameplay.Orbit
{
    public sealed class SolarPowerOrbitIntegration
    {
        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;
        private readonly OrbitLightingSystem lightingSystem;
        private readonly PowerSystem powerSystem;
        private readonly SolarOrbitIntegrationSettings settings;

        public SolarPowerOrbitIntegration(
            EventBus eventBus,
            DebugLogger debugLogger,
            OrbitLightingSystem lightingSystem,
            PowerSystem powerSystem,
            SolarOrbitIntegrationSettings settings)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;
            this.lightingSystem = lightingSystem;
            this.powerSystem = powerSystem;
            this.settings = settings;

            this.eventBus.Subscribe<
                OrbitLightingUpdatedEvent>(
                OnLightingUpdated
            );

            this.eventBus.Subscribe<
                AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );

            ApplyCurrentLighting();

            debugLogger.Log(
                "Solar-orbit power integration initialized.",
                "Power"
            );
        }

        private void OnLightingUpdated(
            OrbitLightingUpdatedEvent lightingEvent)
        {
            ApplyMultiplier(
                lightingEvent.Snapshot
            );
        }

        private void OnAllSaveDataRestored(
            AllSaveDataRestoredEvent restoredEvent)
        {
            ApplyCurrentLighting();
        }

        private void ApplyCurrentLighting()
        {
            ApplyMultiplier(
                lightingSystem.CurrentSnapshot
            );
        }

        private void ApplyMultiplier(
            OrbitLightingSnapshot snapshot)
        {
            powerSystem.SetGenerationMultiplier(
                settings.SolarArrayMachineId,
                snapshot.GenerationMultiplier,
                $"Orbital lighting: {snapshot.State}"
            );
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<
                OrbitLightingUpdatedEvent>(
                OnLightingUpdated
            );

            eventBus.Unsubscribe<
                AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );
        }
    }
}
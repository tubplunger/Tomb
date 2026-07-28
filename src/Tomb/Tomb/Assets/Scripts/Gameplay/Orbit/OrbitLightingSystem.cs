using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;

namespace Tomb.Gameplay.Orbit
{
    public sealed class OrbitLightingSystem
    {
        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;
        private readonly OrbitSystem orbitSystem;
        private readonly OrbitLightingSettings settings;

        private OrbitLightingSnapshot currentSnapshot;
        private bool initialized;

        public OrbitLightingSnapshot CurrentSnapshot =>
            currentSnapshot;

        public OrbitLightingState CurrentState =>
            currentSnapshot.State;

        public bool IsInSunlight =>
            currentSnapshot.IsInSunlight;

        public bool IsInEclipse =>
            currentSnapshot.IsInEclipse;

        public float SolarGenerationMultiplier =>
            currentSnapshot.GenerationMultiplier;

        public OrbitLightingSystem(
            EventBus eventBus,
            DebugLogger debugLogger,
            OrbitSystem orbitSystem,
            OrbitLightingSettings settings)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;
            this.orbitSystem = orbitSystem;
            this.settings = settings;

            this.eventBus.Subscribe<OrbitUpdatedEvent>(
                OnOrbitUpdated
            );

            this.eventBus.Subscribe<AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );

            Recalculate(
                orbitSystem.CurrentSnapshot,
                false
            );

            initialized = true;

            debugLogger.Log(
                $"Orbit lighting initialized: " +
                $"{currentSnapshot}",
                "Orbit"
            );
        }

        public void RecalculateCurrentState()
        {
            Recalculate(
                orbitSystem.CurrentSnapshot,
                true
            );
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
            OrbitLightingSnapshot previousSnapshot =
                currentSnapshot;

            OrbitLightingSnapshot newSnapshot =
                CreateLightingSnapshot(
                    orbitSnapshot.OrbitProgress
                );

            currentSnapshot = newSnapshot;

            if (!publishEvents)
                return;

            eventBus.Publish(
                new OrbitLightingUpdatedEvent(
                    newSnapshot
                )
            );

            bool stateChanged =
                !initialized ||
                previousSnapshot.State != newSnapshot.State;

            if (!stateChanged)
                return;

            if (newSnapshot.State ==
                OrbitLightingState.Eclipse)
            {
                eventBus.Publish(
                    new EnteredEclipseEvent(
                        newSnapshot
                    )
                );

                debugLogger.Log(
                    $"Station entered eclipse. " +
                    $"{newSnapshot.MinutesUntilTransition:0.0} " +
                    $"game minutes until sunlight.",
                    "Orbit"
                );
            }
            else
            {
                eventBus.Publish(
                    new EnteredSunlightEvent(
                        newSnapshot
                    )
                );

                debugLogger.Log(
                    $"Station entered sunlight. " +
                    $"{newSnapshot.MinutesUntilTransition:0.0} " +
                    $"game minutes until eclipse.",
                    "Orbit"
                );
            }
        }

        private OrbitLightingSnapshot
            CreateLightingSnapshot(float orbitProgress)
        {
            float normalizedProgress =
                Mathf.Repeat(orbitProgress, 1f);

            bool isInEclipse =
                settings.IsInEclipse(normalizedProgress);

            OrbitLightingState state =
                isInEclipse
                    ? OrbitLightingState.Eclipse
                    : OrbitLightingState.Sunlight;

            OrbitLightingState nextState =
                isInEclipse
                    ? OrbitLightingState.Sunlight
                    : OrbitLightingState.Eclipse;

            float nextTransitionProgress =
                isInEclipse
                    ? settings.EclipseEndProgress
                    : settings.EclipseStartProgress;

            float progressUntilTransition =
                ForwardDistance(
                    normalizedProgress,
                    nextTransitionProgress
                );

            float minutesUntilTransition =
                progressUntilTransition *
                orbitSystem.OrbitDurationGameMinutes;

            float generationMultiplier =
                isInEclipse
                    ? settings.EclipseGenerationMultiplier
                    : settings.SunlightGenerationMultiplier;

            return new OrbitLightingSnapshot(
                state,
                normalizedProgress,
                generationMultiplier,
                minutesUntilTransition,
                nextState
            );
        }

        private static float ForwardDistance(
            float currentProgress,
            float targetProgress)
        {
            return Mathf.Repeat(
                targetProgress - currentProgress,
                1f
            );
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<OrbitUpdatedEvent>(
                OnOrbitUpdated
            );

            eventBus.Unsubscribe<AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );
        }
    }
}
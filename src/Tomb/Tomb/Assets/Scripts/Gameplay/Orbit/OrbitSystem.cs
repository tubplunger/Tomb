using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;
using Tomb.Core.Time;

namespace Tomb.Gameplay.Orbit
{
    public sealed class OrbitSystem : ISaveable
    {
        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;
        private readonly OrbitSettings settings;

        private float orbitProgress;
        private int completedOrbits;

        public string SaveKey =>
            "station_orbit";

        public Type SaveStateType =>
            typeof(OrbitSaveState);

        public float OrbitProgress =>
            orbitProgress;

        public int CompletedOrbits =>
            completedOrbits;

        public int OrbitDurationGameMinutes =>
            settings.OrbitDurationGameMinutes;

        public OrbitSnapshot CurrentSnapshot =>
            CreateSnapshot();

        public OrbitSystem(
            EventBus eventBus,
            DebugLogger debugLogger,
            OrbitSettings settings)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;
            this.settings = settings;

            orbitProgress =
                settings.StartingOrbitProgress;

            this.eventBus.Subscribe<GameMinutePassedEvent>(
                OnGameMinutePassed
            );

            debugLogger.Log(
                $"Orbit system initialized at " +
                $"{orbitProgress * 100f:0.0}%.",
                "Orbit"
            );
        }

        public void SetOrbitProgress(
            float normalizedProgress,
            string reason = "Unspecified")
        {
            orbitProgress =
                Mathf.Repeat(normalizedProgress, 1f);

            OrbitSnapshot snapshot =
                CreateSnapshot();

            eventBus.Publish(
                new OrbitPositionSetEvent(
                    snapshot,
                    reason
                )
            );

            eventBus.Publish(
                new OrbitUpdatedEvent(snapshot)
            );
        }

        public void AdvanceByNormalizedAmount(
            float normalizedAmount,
            string reason = "Unspecified")
        {
            if (Mathf.Approximately(normalizedAmount, 0f))
                return;

            float previousProgress =
                orbitProgress;

            float rawProgress =
                orbitProgress + normalizedAmount;

            if (rawProgress >= 1f)
            {
                int completed =
                    Mathf.FloorToInt(rawProgress);

                completedOrbits += completed;

                eventBus.Publish(
                    new OrbitCompletedEvent(
                        completedOrbits
                    )
                );
            }
            else if (rawProgress < 0f)
            {
                int backwardsOrbits =
                    Mathf.CeilToInt(-rawProgress);

                completedOrbits =
                    Mathf.Max(
                        0,
                        completedOrbits - backwardsOrbits
                    );
            }

            orbitProgress =
                Mathf.Repeat(rawProgress, 1f);

            OrbitSnapshot snapshot =
                CreateSnapshot();

            eventBus.Publish(
                new OrbitPositionSetEvent(
                    snapshot,
                    reason
                )
            );

            eventBus.Publish(
                new OrbitUpdatedEvent(snapshot)
            );
        }

        private void OnGameMinutePassed(
            GameMinutePassedEvent minuteEvent)
        {
            float progressPerGameMinute =
                1f / settings.OrbitDurationGameMinutes;

            float previousProgress =
                orbitProgress;

            orbitProgress +=
                progressPerGameMinute;

            if (orbitProgress >= 1f)
            {
                orbitProgress =
                    Mathf.Repeat(orbitProgress, 1f);

                completedOrbits++;

                eventBus.Publish(
                    new OrbitCompletedEvent(
                        completedOrbits
                    )
                );

                debugLogger.Log(
                    $"Completed orbit {completedOrbits}.",
                    "Orbit"
                );
            }

            eventBus.Publish(
                new OrbitUpdatedEvent(
                    CreateSnapshot()
                )
            );
        }

        private OrbitSnapshot CreateSnapshot()
        {
            float orbitAngleDegrees =
                orbitProgress * 360f;

            float orbitAngleRadians =
                orbitAngleDegrees * Mathf.Deg2Rad;

            float inclinationRadians =
                settings.OrbitalInclinationDegrees *
                Mathf.Deg2Rad;

            float approximateLatitude =
                Mathf.Asin(
                    Mathf.Sin(inclinationRadians) *
                    Mathf.Sin(orbitAngleRadians)
                ) * Mathf.Rad2Deg;

            float earthRotationDegrees =
                CalculateEarthRotationDegrees();

            float approximateLongitude =
                Mathf.Repeat(
                    settings.StartingLongitudeDegrees +
                    orbitAngleDegrees -
                    earthRotationDegrees +
                    180f,
                    360f
                ) - 180f;

            return new OrbitSnapshot(
                orbitProgress,
                orbitAngleDegrees,
                approximateLatitude,
                approximateLongitude,
                completedOrbits
            );
        }

        private float CalculateEarthRotationDegrees()
        {
            float totalGameMinutes =
                completedOrbits *
                settings.OrbitDurationGameMinutes +
                orbitProgress *
                settings.OrbitDurationGameMinutes;

            const float minutesPerEarthRotation =
                1440f;

            return
                totalGameMinutes /
                minutesPerEarthRotation *
                360f;
        }

        public object CaptureState()
        {
            debugLogger.Log(
                $"Capturing orbit: " +
                $"{orbitProgress * 100f:0.0}% | " +
                $"Completed: {completedOrbits}",
                "Orbit"
            );

            return new OrbitSaveState
            {
                orbitProgress = orbitProgress,
                completedOrbits = completedOrbits
            };
        }

        public void RestoreState(object state)
        {
            if (state is not OrbitSaveState saveState)
            {
                debugLogger.Log(
                    "Orbit restore received an invalid state object.",
                    "Orbit"
                );

                return;
            }

            debugLogger.Log(
                $"Restoring raw orbit data: " +
                $"{saveState.orbitProgress * 100f:0.0}% | " +
                $"Completed: {saveState.completedOrbits}",
                "Orbit"
            );

            orbitProgress = Mathf.Repeat(
                saveState.orbitProgress,
                1f
            );

            completedOrbits = Mathf.Max(
                0,
                saveState.completedOrbits
            );

            OrbitSnapshot snapshot = CreateSnapshot();

            eventBus.Publish(
                new OrbitRestoredFromSaveEvent(snapshot)
            );

            eventBus.Publish(
                new OrbitUpdatedEvent(snapshot)
            );

            debugLogger.Log(
                $"Orbit restored: {snapshot}",
                "Orbit"
            );
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<GameMinutePassedEvent>(
                OnGameMinutePassed
            );
        }
    }
}
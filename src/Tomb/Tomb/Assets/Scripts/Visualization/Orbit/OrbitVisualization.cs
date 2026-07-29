using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;
using Tomb.Gameplay.Orbit;
using Tomb.Core.Services;

namespace Tomb.Visualization.Orbit
{
    public sealed class OrbitVisualization :
        MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private OrbitVisualizationSettings settings;

        [Header("Scene References")]
        [SerializeField]
        private Transform earthTransform;

        [SerializeField]
        private Transform stationTransform;

        [SerializeField]
        private Renderer stationRenderer;

        [SerializeField]
        private OrbitRingRenderer orbitRingRenderer;

        private EventBus eventBus;
        private OrbitSystem orbitSystem;
        private OrbitLightingSystem lightingSystem;

        private Material currentStationMaterial;

        private bool initialized;

        private void Start()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            eventBus =
                CoreServices.Get<EventBus>();

            orbitSystem =
                CoreServices.Get<OrbitSystem>();

            lightingSystem =
                CoreServices.Get<OrbitLightingSystem>();

            if (eventBus == null ||
                orbitSystem == null ||
                lightingSystem == null)
            {
                Debug.LogError(
                    "[OrbitVisualization] Required " +
                    "services could not be found."
                );

                enabled = false;
                return;
            }

            eventBus.Subscribe<OrbitUpdatedEvent>(
                OnOrbitUpdated
            );

            eventBus.Subscribe<
                OrbitLightingUpdatedEvent>(
                OnLightingUpdated
            );

            eventBus.Subscribe<
                OrbitRestoredFromSaveEvent>(
                OnOrbitRestored
            );

            BuildVisualization();

            RefreshOrbit(
                orbitSystem.CurrentSnapshot
            );

            RefreshLighting(
                lightingSystem.CurrentSnapshot
            );

            initialized = true;
        }

        private void BuildVisualization()
        {
            earthTransform.localScale =
                Vector3.one *
                settings.EarthRadius *
                2f;

            stationTransform.localScale =
                Vector3.one *
                settings.StationScale;

            orbitRingRenderer.BuildRing(
                settings.OrbitRadius,
                orbitSystem
                    .OrbitalInclinationDegrees,
                settings.OrbitRingSegments,
                settings.OrbitRingWidth
            );
        }

        private void OnOrbitUpdated(
            OrbitUpdatedEvent orbitEvent)
        {
            RefreshOrbit(
                orbitEvent.Snapshot
            );
        }

        private void OnLightingUpdated(
            OrbitLightingUpdatedEvent lightingEvent)
        {
            RefreshLighting(
                lightingEvent.Snapshot
            );
        }

        private void OnOrbitRestored(
            OrbitRestoredFromSaveEvent restoredEvent)
        {
            RefreshOrbit(
                restoredEvent.Snapshot
            );

            RefreshLighting(
                lightingSystem.CurrentSnapshot
            );
        }

        private void RefreshOrbit(
            OrbitSnapshot snapshot)
        {
            float orbitAngleRadians =
                snapshot.OrbitAngleDegrees *
                Mathf.Deg2Rad;

            float inclinationRadians =
                orbitSystem
                    .OrbitalInclinationDegrees *
                Mathf.Deg2Rad;

            Vector3 stationLocalPosition =
                OrbitRingRenderer
                    .CalculateOrbitPosition(
                        orbitAngleRadians,
                        inclinationRadians,
                        settings.OrbitRadius
                    );

            stationTransform.localPosition =
                stationLocalPosition;

            UpdateStationOrientation(
                stationLocalPosition
            );

            UpdateEarthRotation(snapshot);
        }

        private void UpdateEarthRotation(
            OrbitSnapshot snapshot)
        {
            float totalGameMinutes =
                snapshot.CompletedOrbits *
                orbitSystem
                    .OrbitDurationGameMinutes +
                snapshot.OrbitProgress *
                orbitSystem
                    .OrbitDurationGameMinutes;

            float earthRotationDegrees =
                totalGameMinutes /
                settings.EarthRotationGameMinutes *
                360f;

            earthRotationDegrees +=
                settings
                    .EarthRotationOffsetDegrees;

            earthTransform.localRotation =
                Quaternion.Euler(
                    0f,
                    -earthRotationDegrees,
                    0f
                );
        }

        private void UpdateStationOrientation(
            Vector3 stationPosition)
        {
            if (stationPosition.sqrMagnitude <=
                Mathf.Epsilon)
            {
                return;
            }

            Vector3 directionTowardEarth =
                -stationPosition.normalized;

            stationTransform.localRotation =
                Quaternion.LookRotation(
                    directionTowardEarth,
                    Vector3.up
                );
        }

        private void RefreshLighting(
            OrbitLightingSnapshot snapshot)
        {
            Material desiredMaterial =
                snapshot.IsInSunlight
                    ? settings
                        .SunlightStationMaterial
                    : settings
                        .EclipseStationMaterial;

            if (desiredMaterial == null)
                return;

            if (currentStationMaterial ==
                desiredMaterial)
            {
                return;
            }

            stationRenderer.sharedMaterial =
                desiredMaterial;

            currentStationMaterial =
                desiredMaterial;
        }

        private bool ValidateReferences()
        {
            if (settings == null)
            {
                Debug.LogError(
                    "[OrbitVisualization] Missing settings."
                );

                return false;
            }

            if (earthTransform == null)
            {
                Debug.LogError(
                    "[OrbitVisualization] Missing Earth transform."
                );

                return false;
            }

            if (stationTransform == null)
            {
                Debug.LogError(
                    "[OrbitVisualization] Missing Station transform."
                );

                return false;
            }

            if (stationRenderer == null)
            {
                Debug.LogError(
                    "[OrbitVisualization] Missing Station renderer."
                );

                return false;
            }

            if (orbitRingRenderer == null)
            {
                Debug.LogError(
                    "[OrbitVisualization] Missing Orbit Ring renderer."
                );

                return false;
            }

            return true;
        }

        private void OnDestroy()
        {
            if (!initialized ||
                eventBus == null)
            {
                return;
            }

            eventBus.Unsubscribe<OrbitUpdatedEvent>(
                OnOrbitUpdated
            );

            eventBus.Unsubscribe<
                OrbitLightingUpdatedEvent>(
                OnLightingUpdated
            );

            eventBus.Unsubscribe<
                OrbitRestoredFromSaveEvent>(
                OnOrbitRestored
            );
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tomb.Core.Events;
using Tomb.Core.Services;

namespace Tomb.Gameplay.Orbit
{
    public sealed class DebugOrbitPanel : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField]
        private TMP_Text orbitStatusText;

        [Header("Orbit")]
        [SerializeField]
        private TMP_Text progressText;

        [SerializeField]
        private TMP_Text angleText;

        [SerializeField]
        private TMP_Text revolutionText;

        [SerializeField]
        private TMP_Text durationText;

        [Header("Position")]
        [SerializeField]
        private TMP_Text latitudeText;

        [SerializeField]
        private TMP_Text longitudeText;

        [Header("Progress Bar")]
        [SerializeField]
        private RectTransform progressFillRect;

        [SerializeField]
        private RectTransform progressBackgroundRect;

        private EventBus eventBus;
        private OrbitSystem orbitSystem;

        private bool initialized;
        private bool refreshQueued;

        public int OrbitDurationGameMinutes => orbitSystem.OrbitDurationGameMinutes;

        private void Start()
        {
            eventBus =
                CoreServices.Get<EventBus>();

            orbitSystem =
                CoreServices.Get<OrbitSystem>();

            eventBus.Subscribe<OrbitUpdatedEvent>(
                OnOrbitUpdated
            );

            eventBus.Subscribe<OrbitRestoredFromSaveEvent>(
                OnOrbitRestored
            );

            initialized = true;
            refreshQueued = true;
        }

        private void OnOrbitRestored(
            OrbitRestoredFromSaveEvent restoredEvent)
        {
            refreshQueued = false;
            Refresh();
        }

        private void OnEnable()
        {
            if (initialized)
                refreshQueued = true;
        }

        private void LateUpdate()
        {
            if (!initialized || !refreshQueued)
                return;

            refreshQueued = false;
            Refresh();
        }

        private void OnDestroy()
        {
            eventBus?.Unsubscribe<OrbitUpdatedEvent>(
                OnOrbitUpdated
            );

            eventBus?.Unsubscribe<OrbitRestoredFromSaveEvent>(
                OnOrbitRestored
            );
        }

        private void OnRectTransformDimensionsChange()
        {
            if (initialized)
                refreshQueued = true;
        }

        private void OnOrbitUpdated(
            OrbitUpdatedEvent orbitEvent)
        {
            refreshQueued = true;
        }

        private void Refresh()
        {
            OrbitSnapshot snapshot =
                orbitSystem.CurrentSnapshot;

            orbitStatusText.text =
                "STATUS: TRACKING";

            progressText.text =
                $"Progress: " +
                $"{snapshot.OrbitProgress * 100f:0.0}%";

            angleText.text =
                $"Angle: " +
                $"{snapshot.OrbitAngleDegrees:0.0}°";

            revolutionText.text =
                $"Completed Orbits: " +
                $"{snapshot.CompletedOrbits}";

            durationText.text =
                $"Orbital Period: " +
                $"{orbitSystem.OrbitDurationGameMinutes} game minutes";

            latitudeText.text =
                $"Latitude: " +
                $"{FormatLatitude(snapshot.ApproximateLatitude)}";

            longitudeText.text =
                $"Longitude: " +
                $"{FormatLongitude(snapshot.ApproximateLongitude)}";

            UpdateProgressBar(snapshot.OrbitProgress);
        }

        private void UpdateProgressBar(float normalizedProgress)
        {
            if (progressFillRect == null ||
                progressBackgroundRect == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();

            LayoutRebuilder.ForceRebuildLayoutImmediate(
                progressBackgroundRect
            );

            float backgroundWidth =
                progressBackgroundRect.rect.width;

            if (backgroundWidth <= 0f)
                return;

            progressFillRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                backgroundWidth *
                Mathf.Clamp01(normalizedProgress)
            );
        }

        private static string FormatLatitude(float latitude)
        {
            string direction =
                latitude >= 0f ? "N" : "S";

            return
                $"{Mathf.Abs(latitude):0.0}° {direction}";
        }

        private static string FormatLongitude(float longitude)
        {
            string direction =
                longitude >= 0f ? "E" : "W";

            return
                $"{Mathf.Abs(longitude):0.0}° {direction}";
        }
    }
}
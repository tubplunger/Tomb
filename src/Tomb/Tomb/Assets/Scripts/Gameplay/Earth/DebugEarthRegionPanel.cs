using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;
using Tomb.Core.Events;
using Tomb.Gameplay.Earth;
using Tomb.Gameplay.Radio;
using Tomb.Core.Services;

namespace Tomb.Core.Debugging.UI
{
    public sealed class DebugEarthRegionPanel :
        MonoBehaviour
    {
        [SerializeField]
        private TMP_Text currentPositionText;

        [SerializeField]
        private TMP_Text currentRegionText;

        [SerializeField]
        private TMP_Text visibleRegionsText;

        [SerializeField]
        private TMP_Text visibleSignalsText;

        private EventBus eventBus;
        private EarthRegionSystem earthRegionSystem;
        private RadioVisibilitySystem
            radioVisibilitySystem;

        private bool refreshQueued;

        private void Start()
        {
            eventBus =
                CoreServices.Get<EventBus>();

            earthRegionSystem =
                CoreServices.Get<EarthRegionSystem>();

            radioVisibilitySystem =
                CoreServices.Get<
                    RadioVisibilitySystem>();

            eventBus.Subscribe<
                EarthRegionUpdatedEvent>(
                OnEarthRegionUpdated
            );

            eventBus.Subscribe<
                RadioVisibilityUpdatedEvent>(
                OnRadioVisibilityUpdated
            );

            Refresh();
        }

        private void LateUpdate()
        {
            if (!refreshQueued)
                return;

            refreshQueued = false;
            Refresh();
        }

        private void OnEarthRegionUpdated(
            EarthRegionUpdatedEvent regionEvent)
        {
            refreshQueued = true;
        }

        private void OnRadioVisibilityUpdated(
            RadioVisibilityUpdatedEvent radioEvent)
        {
            refreshQueued = true;
        }

        private void Refresh()
        {
            EarthRegionSnapshot snapshot =
                earthRegionSystem.CurrentSnapshot;

            if (snapshot == null)
                return;

            currentPositionText.text =
                $"Position: " +
                $"{FormatLatitude(snapshot.Latitude)}, " +
                $"{FormatLongitude(snapshot.Longitude)}";

            currentRegionText.text =
                snapshot.CurrentRegion != null
                    ? $"Current Region: " +
                      $"{snapshot.CurrentRegion.DisplayName}"
                    : "Current Region: Unclassified";

            StringBuilder regionBuilder =
                new();

            if (snapshot.VisibleRegions.Count == 0)
            {
                regionBuilder.Append("None");
            }
            else
            {
                foreach (EarthRegionDefinition region
                         in snapshot.VisibleRegions)
                {
                    regionBuilder.AppendLine(
                        $"• {region.DisplayName}"
                    );
                }
            }

            visibleRegionsText.text =
                regionBuilder.ToString();

            StringBuilder signalBuilder =
                new();

            if (radioVisibilitySystem
                    .VisibleSignals.Count == 0)
            {
                signalBuilder.Append("None");
            }
            else
            {
                foreach (RadioSignalDefinition signal
                         in radioVisibilitySystem
                             .VisibleSignals)
                {
                    signalBuilder.AppendLine(
                        $"• {signal.DisplayName}"
                    );
                }
            }

            visibleSignalsText.text =
                signalBuilder.ToString();
        }

        private static string FormatLatitude(
            float latitude)
        {
            string direction =
                latitude >= 0f ? "N" : "S";

            return
                $"{Mathf.Abs(latitude):0.0}° {direction}";
        }

        private static string FormatLongitude(
            float longitude)
        {
            string direction =
                longitude >= 0f ? "E" : "W";

            return
                $"{Mathf.Abs(longitude):0.0}° {direction}";
        }

        private void OnDestroy()
        {
            if (eventBus == null)
                return;

            eventBus.Unsubscribe<
                EarthRegionUpdatedEvent>(
                OnEarthRegionUpdated
            );

            eventBus.Unsubscribe<
                RadioVisibilityUpdatedEvent>(
                OnRadioVisibilityUpdated
            );
        }
    }
}
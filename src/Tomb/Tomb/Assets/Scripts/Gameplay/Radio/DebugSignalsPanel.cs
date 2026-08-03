using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Tomb.Core.Debugging.Lists;
using Tomb.Core.Events;
using Tomb.Core.Services;
using Tomb.Gameplay.Earth;
using Tomb.Gameplay.Machines;

namespace Tomb.Gameplay.Radio
{
    public sealed class DebugSignalsPanel :
        DebugListPanelBase
    {
        [Header("Rows")]
        [SerializeField]
        private DebugSignalRowView rowPrefab;

        [Header("UI")]
        [SerializeField]
        private TMP_Text signalCountText;

        [SerializeField]
        private TMP_Text radioStateText;

        [Header("Data")]
        [SerializeField]
        private RadioSignalCatalog signalCatalog;

        [SerializeField]
        private EarthRegionCatalog regionCatalog;

        private readonly List<DebugSignalRowView>
            rows = new();

        private EventBus eventBus;
        private RadioVisibilitySystem visibilitySystem;
        private MachineSystem machineSystem;

        protected override void InitializePanel()
        {
            eventBus =
                CoreServices.Get<EventBus>();

            visibilitySystem =
                CoreServices.Get<RadioVisibilitySystem>();

            machineSystem =
                CoreServices.Get<MachineSystem>();

            eventBus.Subscribe<
                RadioVisibilityUpdatedEvent>(
                OnRadioVisibilityUpdated
            );

            eventBus.Subscribe<
                MachineStateChangedEvent>(
                OnMachineStateChanged
            );

            BuildRows();
        }

        protected override void RefreshList()
        {
            foreach (DebugSignalRowView row in rows)
            {
                row.Refresh();
            }

            signalCountText.text =
                $"Visible Signals: " +
                $"{visibilitySystem.VisibleSignals.Count} / " +
                $"{rows.Count}";

            radioStateText.text =
                visibilitySystem
                    .IsCommunicationsAvailable()
                    ? "Communications Array: AVAILABLE"
                    : "Communications Array: UNAVAILABLE";

            RebuildListLayout();
        }

        private void BuildRows()
        {
            if (signalCatalog == null ||
                regionCatalog == null)
            {
                Debug.LogError(
                    "[DebugSignalsPanel] Missing catalog asset."
                );

                return;
            }

            foreach (RadioSignalDefinition signal
                     in signalCatalog.Signals)
            {
                if (signal == null)
                    continue;

                DebugSignalRowView row =
                    SpawnRow(rowPrefab);

                if (row == null)
                    continue;

                row.Initialize(
                    signal,
                    regionCatalog,
                    visibilitySystem
                );

                rows.Add(row);
            }

            RebuildListLayout();
        }

        private void OnRadioVisibilityUpdated(
            RadioVisibilityUpdatedEvent visibilityEvent)
        {
            QueueRefresh();
        }

        private void OnMachineStateChanged(
            MachineStateChangedEvent stateEvent)
        {
            QueueRefresh();
        }

        private void OnDestroy()
        {
            eventBus?.Unsubscribe<
                RadioVisibilityUpdatedEvent>(
                OnRadioVisibilityUpdated
            );

            eventBus?.Unsubscribe<
                MachineStateChangedEvent>(
                OnMachineStateChanged
            );
        }
    }
}
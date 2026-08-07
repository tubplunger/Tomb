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
        private RadioReceiverSystem receiverSystem;

        protected override void InitializePanel()
        {
            eventBus =
                CoreServices.Get<EventBus>();

            visibilitySystem =
                CoreServices.Get<RadioVisibilitySystem>();

            machineSystem =
                CoreServices.Get<MachineSystem>();

            receiverSystem =
                CoreServices.Get<RadioReceiverSystem>();

            eventBus.Subscribe<
                RadioVisibilityUpdatedEvent>(
                OnRadioVisibilityUpdated
            );

            eventBus.Subscribe<RadioReceiverUpdatedEvent>(
                OnReceiverUpdated
            );

            eventBus.Subscribe<
                MachineStateChangedEvent>(
                OnMachineStateChanged
            );

            BuildRows();
        }

        protected override void RefreshList()
        {
            int detectedCount = 0;

            foreach (RadioSignalRuntimeState state
                     in receiverSystem.Signals)
            {
                if (state.HasBeenDetected)
                    detectedCount++;
            }

            signalCountText.text =
                $"Detected: {detectedCount} / " +
                $"{receiverSystem.Signals.Count}";

            radioStateText.text =
                receiverSystem.IsReceiverOperational
                    ? "Receiver: OPERATIONAL"
                    : "Receiver: OFFLINE";
        }

        private void BuildRows()
        {
            foreach (RadioSignalRuntimeState state
                     in receiverSystem.Signals)
            {
                DebugSignalRowView row =
                    SpawnRow(rowPrefab);

                if (row == null)
                    continue;

                row.Initialize(
                    state,
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

        private void OnReceiverUpdated(
            RadioReceiverUpdatedEvent receiverEvent)
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
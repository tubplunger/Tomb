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

        [Header("Summary")]
        [SerializeField]
        private TMP_Text signalCountText;

        [SerializeField]
        private TMP_Text radioStateText;

        private readonly List<DebugSignalRowView>
            rows = new();

        private EventBus eventBus;
        private RadioReceiverSystem receiverSystem;
        private RadioVisibilitySystem visibilitySystem;

        protected override void InitializePanel()
        {
            eventBus =
                CoreServices.Get<EventBus>();

            receiverSystem =
                CoreServices.Get<RadioReceiverSystem>();

            visibilitySystem =
                CoreServices.Get<RadioVisibilitySystem>();

            eventBus.Subscribe<RadioReceiverUpdatedEvent>(
                OnReceiverUpdated
            );

            eventBus.Subscribe<RadioVisibilityUpdatedEvent>(
                OnVisibilityUpdated
            );

            eventBus.Subscribe<MachineStateChangedEvent>(
                OnMachineStateChanged
            );

            eventBus.Subscribe<MachineConditionChangedEvent>(
                OnMachineConditionChanged
            );

            BuildRows();

            QueueRefresh();
        }

        protected override void RefreshList()
        {
            int knownCount = 0;
            int activeCount = 0;

            foreach (RadioSignalRuntimeState state
                     in receiverSystem.Signals)
            {
                if (state.HasBeenDetected)
                {
                    knownCount++;
                }

                if (state.Status ==
                        RadioSignalReceiverStatus.Detected ||
                    state.Status ==
                        RadioSignalReceiverStatus.Tuned)
                {
                    activeCount++;
                }
            }

            signalCountText.text =
                $"Active: {activeCount} | " +
                $"Known: {knownCount} / " +
                $"{receiverSystem.Signals.Count}";

            radioStateText.text =
                receiverSystem.IsReceiverOperational
                    ? "Receiver: OPERATIONAL"
                    : "Receiver: OFFLINE";

            foreach (DebugSignalRowView row in rows)
            {
                row.Refresh();
            }

            RebuildListLayout();
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

        private void OnReceiverUpdated(
            RadioReceiverUpdatedEvent receiverEvent)
        {
            QueueRefresh();
        }

        private void OnVisibilityUpdated(
            RadioVisibilityUpdatedEvent visibilityEvent)
        {
            QueueRefresh();
        }

        private void OnMachineStateChanged(
            MachineStateChangedEvent stateEvent)
        {
            QueueRefresh();
        }

        private void OnMachineConditionChanged(
            MachineConditionChangedEvent conditionEvent)
        {
            QueueRefresh();
        }

        private void OnDestroy()
        {
            eventBus?.Unsubscribe<RadioReceiverUpdatedEvent>(
                OnReceiverUpdated
            );

            eventBus?.Unsubscribe<RadioVisibilityUpdatedEvent>(
                OnVisibilityUpdated
            );

            eventBus?.Unsubscribe<MachineStateChangedEvent>(
                OnMachineStateChanged
            );

            eventBus?.Unsubscribe<MachineConditionChangedEvent>(
                OnMachineConditionChanged
            );
        }
    }
}
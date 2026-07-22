using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Tomb.Core.Debugging.Lists;
using Tomb.Core.Events;
using Tomb.Core.Services;
using Tomb.Core.Time;

namespace Tomb.Gameplay.Machines
{
    public sealed class DebugMachinesPanel
        : DebugListPanelBase
    {
        [Header("Machines")]
        [SerializeField]
        private DebugMachineRowView rowPrefab;

        [SerializeField]
        private TMP_Text machineCountText;

        private readonly List<DebugMachineRowView>
            rows = new();

        private EventBus eventBus;
        private MachineSystem machineSystem;

        protected override void InitializePanel()
        {
            eventBus = CoreServices.Get<EventBus>();
            machineSystem = CoreServices.Get<MachineSystem>();

            eventBus.Subscribe<MachineStateChangedEvent>(
                OnMachineStateChanged
            );

            eventBus.Subscribe<MachineConditionChangedEvent>(
                OnMachineConditionChanged
            );

            eventBus.Subscribe<GameMinutePassedEvent>(
                OnGameMinutePassed
                );

            BuildRows();
        }

        protected override void RefreshList()
        {
            foreach (DebugMachineRowView row in rows)
            {
                row.Refresh();
            }

            RebuildListLayout();
        }

        private void OnDestroy()
        {
            eventBus?.Unsubscribe<MachineStateChangedEvent>(
                OnMachineStateChanged
            );

            eventBus?.Unsubscribe<MachineConditionChangedEvent>(
                OnMachineConditionChanged
            );

            eventBus?.Unsubscribe<GameMinutePassedEvent>(
                OnGameMinutePassed
            );
        }

        private void BuildRows()
        {
            foreach (MachineState machine
                     in machineSystem.Machines)
            {
                DebugMachineRowView row =
                    SpawnRow(rowPrefab);

                if (row == null)
                    continue;

                row.Initialize(machine);
                rows.Add(row);
            }

            machineCountText.text =
                $"Machines: {rows.Count}";

            RebuildListLayout();
        }

        private void OnGameMinutePassed(
            GameMinutePassedEvent minuteEvent)
        {
            QueueRefresh();
        }

        private void OnMachineStateChanged(
            MachineStateChangedEvent stateChangedEvent)
        {
            QueueRefresh();
        }

        private void OnMachineConditionChanged(
            MachineConditionChangedEvent conditionChangedEvent)
        {
            QueueRefresh();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Tomb.Core.Debugging.Lists;
using Tomb.Core.Events;
using Tomb.Core.Services;
using Tomb.Gameplay.Machines;

namespace Tomb.Gameplay.Power
{
    public sealed class DebugPowerPanel
        : DebugListPanelBase
    {
        [Header("Rows")]
        [SerializeField]
        private DebugPowerRowView rowPrefab;

        [Header("Summary")]
        [SerializeField]
        private TMP_Text powerStatusText;

        [SerializeField]
        private TMP_Text generationText;

        [SerializeField]
        private TMP_Text demandText;

        [SerializeField]
        private TMP_Text servedText;

        [SerializeField]
        private TMP_Text batteryText;

        [SerializeField]
        private RectTransform batteryFillRect;

        [SerializeField]
        private RectTransform batteryBackgroundRect;

        private readonly List<DebugPowerRowView>
            rows = new();

        private EventBus eventBus;
        private PowerSystem powerSystem;
        private MachineSystem machineSystem;

        protected override void InitializePanel()
        {
            eventBus = CoreServices.Get<EventBus>();
            powerSystem = CoreServices.Get<PowerSystem>();
            machineSystem = CoreServices.Get<MachineSystem>();

            eventBus.Subscribe<PowerBalanceUpdatedEvent>(
                OnPowerChanged
            );

            eventBus.Subscribe<MachineStateChangedEvent>(
                OnMachineChanged
            );

            eventBus.Subscribe<MachineConditionChangedEvent>(
                OnMachineConditionChanged
            );

            BuildRows();
        }

        protected override void RefreshList()
        {
            generationText.text =
                $"Generation: " +
                $"{powerSystem.CurrentGeneration:0.##}";

            demandText.text =
                $"Demand: " +
                $"{powerSystem.CurrentDemand:0.##}";

            servedText.text =
                $"Served: " +
                $"{powerSystem.CurrentServedDemand:0.##}";

            batteryText.text =
                $"Battery: " +
                $"{powerSystem.BatteryCharge:0.##} / " +
                $"{powerSystem.BatteryCapacity:0.##}";

            bool shortage =
                powerSystem.CurrentServedDemand + 0.0001f <
                powerSystem.CurrentDemand;

            powerStatusText.text =
                shortage
                    ? "STATUS: POWER SHORTAGE"
                    : "STATUS: NOMINAL";

            foreach (DebugPowerRowView row in rows)
            {
                row.Refresh();
            }

            UpdateBatteryBar();
            RebuildListLayout();
        }

        private void BuildRows()
        {
            foreach (MachineState machine
                     in machineSystem.Machines)
            {
                MachinePowerProfile profile =
                    machine.Definition.PowerProfile;

                bool isBattery =
                    machine.Definition.MachineId ==
                    "battery_bank";

                if (profile == null && !isBattery)
                    continue;

                DebugPowerRowView row =
                    SpawnRow(rowPrefab);

                if (row == null)
                    continue;

                row.Initialize(machine);
                rows.Add(row);
            }

            RebuildListLayout();
        }

        private void UpdateBatteryBar()
        {
            if (batteryFillRect == null ||
                batteryBackgroundRect == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();

            float width =
                batteryBackgroundRect.rect.width;

            batteryFillRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                width * powerSystem.BatteryNormalized
            );
        }

        private void OnPowerChanged(
            PowerBalanceUpdatedEvent powerEvent)
        {
            QueueRefresh();
        }

        private void OnMachineChanged(
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
            eventBus?.Unsubscribe<PowerBalanceUpdatedEvent>(
                OnPowerChanged
            );

            eventBus?.Unsubscribe<MachineStateChangedEvent>(
                OnMachineChanged
            );

            eventBus?.Unsubscribe<MachineConditionChangedEvent>(
                OnMachineConditionChanged
            );
        }
    }
}
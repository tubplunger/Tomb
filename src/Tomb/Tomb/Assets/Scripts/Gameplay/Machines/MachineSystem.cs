using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;

namespace Tomb.Gameplay.Machines
{
    public sealed class MachineSystem : ISaveable
    {
        private const float ComparisonTolerance = 0.0001f;

        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;

        private readonly Dictionary<string, MachineState>
            machinesById = new();

        private readonly List<MachineState>
            orderedMachines = new();

        public string SaveKey => "station_machines";

        public Type SaveStateType =>
            typeof(MachineSaveState);

        public IReadOnlyList<MachineState> Machines =>
            orderedMachines;

        public MachineSystem(
            EventBus eventBus,
            DebugLogger debugLogger,
            MachineCatalog catalog)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;

            InitializeFromCatalog(catalog);
        }

        public bool TryGetMachine(
            string machineId,
            out MachineState machine)
        {
            if (string.IsNullOrWhiteSpace(machineId))
            {
                machine = null;
                return false;
            }

            return machinesById.TryGetValue(
                machineId,
                out machine
            );
        }

        public MachineState GetMachine(string machineId)
        {
            if (TryGetMachine(machineId, out MachineState machine))
                return machine;

            throw new KeyNotFoundException(
                $"Machine not found: {machineId}"
            );
        }

        public bool ToggleEnabled(
            string machineId,
            string reason = "Unspecified")
        {
            if (!TryGetMachine(machineId, out MachineState machine))
            {
                LogMissingMachine(machineId);
                return false;
            }

            return SetEnabled(
                machineId,
                !machine.IsEnabled,
                reason
            );
        }

        public bool SetEnabled(
            string machineId,
            bool enabled,
            string reason = "Unspecified")
        {
            if (!TryGetMachine(machineId, out MachineState machine))
            {
                LogMissingMachine(machineId);
                return false;
            }

            MachineStatus previousStatus = machine.Status;

            if (!machine.SetEnabled(enabled))
                return false;

            PublishStateChange(
                machine,
                previousStatus,
                reason
            );

            return true;
        }

        public float Damage(
            string machineId,
            float amount,
            string reason = "Unspecified")
        {
            if (!TryGetMachine(machineId, out MachineState machine))
            {
                LogMissingMachine(machineId);
                return 0f;
            }

            if (amount <= 0f)
                return 0f;

            float previousCondition =
                machine.CurrentCondition;

            MachineStatus previousStatus =
                machine.Status;

            bool wasBroken = machine.IsBroken;

            float amountDamaged =
                machine.Damage(amount);

            if (amountDamaged <= ComparisonTolerance)
                return 0f;

            eventBus.Publish(
                new MachineConditionChangedEvent(
                    machine.Definition.MachineId,
                    previousCondition,
                    machine.CurrentCondition,
                    -amountDamaged,
                    reason
                )
            );

            if (!wasBroken && machine.IsBroken)
            {
                eventBus.Publish(
                    new MachineBrokenEvent(
                        machine.Definition.MachineId
                    )
                );
            }

            if (machine.Status != previousStatus)
            {
                PublishStateChange(
                    machine,
                    previousStatus,
                    reason
                );
            }

            return amountDamaged;
        }

        public float Repair(
            string machineId,
            float amount,
            string reason = "Unspecified")
        {
            if (!TryGetMachine(machineId, out MachineState machine))
            {
                LogMissingMachine(machineId);
                return 0f;
            }

            if (amount <= 0f)
                return 0f;

            float previousCondition =
                machine.CurrentCondition;

            MachineStatus previousStatus =
                machine.Status;

            bool wasBroken = machine.IsBroken;

            float amountRepaired =
                machine.Repair(amount);

            if (amountRepaired <= ComparisonTolerance)
                return 0f;

            eventBus.Publish(
                new MachineConditionChangedEvent(
                    machine.Definition.MachineId,
                    previousCondition,
                    machine.CurrentCondition,
                    amountRepaired,
                    reason
                )
            );

            if (wasBroken && !machine.IsBroken)
            {
                eventBus.Publish(
                    new MachineRepairedEvent(
                        machine.Definition.MachineId,
                        machine.CurrentCondition
                    )
                );
            }

            if (machine.Status != previousStatus)
            {
                PublishStateChange(
                    machine,
                    previousStatus,
                    reason
                );
            }

            return amountRepaired;
        }

        public float SetCondition(
            string machineId,
            float condition,
            string reason = "Unspecified")
        {
            if (!TryGetMachine(machineId, out MachineState machine))
            {
                LogMissingMachine(machineId);
                return 0f;
            }

            float previousCondition =
                machine.CurrentCondition;

            MachineStatus previousStatus =
                machine.Status;

            bool wasBroken = machine.IsBroken;

            float changeAmount =
                machine.SetCondition(condition);

            if (Math.Abs(changeAmount) <= ComparisonTolerance)
                return 0f;

            eventBus.Publish(
                new MachineConditionChangedEvent(
                    machine.Definition.MachineId,
                    previousCondition,
                    machine.CurrentCondition,
                    changeAmount,
                    reason
                )
            );

            if (!wasBroken && machine.IsBroken)
            {
                eventBus.Publish(
                    new MachineBrokenEvent(
                        machine.Definition.MachineId
                    )
                );
            }
            else if (wasBroken && !machine.IsBroken)
            {
                eventBus.Publish(
                    new MachineRepairedEvent(
                        machine.Definition.MachineId,
                        machine.CurrentCondition
                    )
                );
            }

            if (machine.Status != previousStatus)
            {
                PublishStateChange(
                    machine,
                    previousStatus,
                    reason
                );
            }

            return changeAmount;
        }

        public object CaptureState()
        {
            MachineSaveState saveState =
                new MachineSaveState();

            foreach (MachineState machine in orderedMachines)
            {
                saveState.machines.Add(
                    new MachineSaveEntry
                    {
                        machineId =
                            machine.Definition.MachineId,

                        isEnabled =
                            machine.IsEnabled,

                        currentCondition =
                            machine.CurrentCondition
                    }
                );
            }

            return saveState;
        }

        public void RestoreState(object state)
        {
            if (state is not MachineSaveState saveState)
                return;

            int restoredCount = 0;

            foreach (MachineSaveEntry entry
                     in saveState.machines)
            {
                if (entry == null ||
                    string.IsNullOrWhiteSpace(entry.machineId))
                {
                    continue;
                }

                if (!TryGetMachine(
                        entry.machineId,
                        out MachineState machine))
                {
                    debugLogger.Log(
                        $"Save contains unknown machine: " +
                        $"{entry.machineId}",
                        "Machines"
                    );

                    continue;
                }

                SetCondition(
                    entry.machineId,
                    entry.currentCondition,
                    "Save restoration"
                );

                SetEnabled(
                    entry.machineId,
                    entry.isEnabled,
                    "Save restoration"
                );

                restoredCount++;
            }

            eventBus.Publish(
                new MachinesRestoredFromSaveEvent(
                    restoredCount
                )
            );

            debugLogger.Log(
                $"Restored {restoredCount} machine states.",
                "Machines"
            );
        }

        private void InitializeFromCatalog(
            MachineCatalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(
                    nameof(catalog),
                    "Machine catalog cannot be null."
                );
            }

            foreach (MachineDefinition definition
                     in catalog.Machines)
            {
                RegisterDefinition(definition);
            }

            debugLogger.Log(
                $"Machine system initialized with " +
                $"{orderedMachines.Count} machines.",
                "Machines"
            );
        }

        private void RegisterDefinition(
            MachineDefinition definition)
        {
            if (definition == null)
            {
                debugLogger.Log(
                    "Machine catalog contains a null entry.",
                    "Machines"
                );

                return;
            }

            string machineId =
                definition.MachineId;

            if (string.IsNullOrWhiteSpace(machineId))
            {
                debugLogger.Log(
                    $"Machine definition '{definition.name}' " +
                    "has an empty Machine ID.",
                    "Machines"
                );

                return;
            }

            if (machinesById.ContainsKey(machineId))
            {
                debugLogger.Log(
                    $"Duplicate machine ID rejected: {machineId}",
                    "Machines"
                );

                return;
            }

            MachineState machine =
                new MachineState(definition);

            machinesById.Add(machineId, machine);
            orderedMachines.Add(machine);
        }

        private void PublishStateChange(
            MachineState machine,
            MachineStatus previousStatus,
            string reason)
        {
            eventBus.Publish(
                new MachineStateChangedEvent(
                    machine.Definition.MachineId,
                    previousStatus,
                    machine.Status,
                    machine.IsEnabled,
                    machine.CurrentCondition,
                    reason
                )
            );
        }

        private void LogMissingMachine(string machineId)
        {
            debugLogger.Log(
                $"Unknown machine requested: {machineId}",
                "Machines"
            );
        }
    }
}
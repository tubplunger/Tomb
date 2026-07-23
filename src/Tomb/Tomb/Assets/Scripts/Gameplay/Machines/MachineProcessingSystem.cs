using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;
using Tomb.Core.Time;
using Tomb.Gameplay.Resources;

namespace Tomb.Gameplay.Machines
{
    public sealed class MachineProcessingSystem : ISaveable
    {
        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;
        private readonly MachineSystem machineSystem;
        private readonly ResourceSystem resourceSystem;

        private readonly Dictionary<string, int>
            elapsedMinutesByMachine = new();

        public string SaveKey =>
            "machine_processing";

        public Type SaveStateType =>
            typeof(MachineProcessingSaveState);

        public MachineProcessingSystem(
            EventBus eventBus,
            DebugLogger debugLogger,
            MachineSystem machineSystem,
            ResourceSystem resourceSystem)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;
            this.machineSystem = machineSystem;
            this.resourceSystem = resourceSystem;

            InitializeTimers();

            this.eventBus.Subscribe<GameMinutePassedEvent>(
                OnGameMinutePassed
            );

            this.eventBus.Subscribe<AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );

            debugLogger.Log(
                "Machine processing system initialized.",
                "Machines"
            );
        }

        private void OnAllSaveDataRestored(
            AllSaveDataRestoredEvent loadEvent)
        {
            RefreshInputAvailability();
        }

        private void InitializeTimers()
        {
            foreach (MachineState machine
                     in machineSystem.Machines)
            {
                if (machine.Definition.ProcessDefinition == null)
                    continue;

                elapsedMinutesByMachine[
                    machine.Definition.MachineId
                ] = 0;
            }
        }

        private void OnGameMinutePassed(
            GameMinutePassedEvent minuteEvent)
        {
            foreach (MachineState machine
                     in machineSystem.Machines)
            {
                MachineProcessDefinition process =
                    machine.Definition.ProcessDefinition;

                if (process == null)
                    continue;

                if (!machine.IsEnabled ||
                    machine.IsBroken ||
                    !machine.HasPower ||
                    machine.IsInMaintenance)
                {
                    continue;
                }

                string machineId =
                    machine.Definition.MachineId;

                int elapsed =
                    elapsedMinutesByMachine[machineId];

                elapsed++;

                if (elapsed < process.IntervalGameMinutes)
                {
                    elapsedMinutesByMachine[machineId] = elapsed;
                    continue;
                }

                bool processed =
                    TryProcess(machine, process);

                if (processed)
                {
                    elapsedMinutesByMachine[machineId] = 0;
                }
                else
                {
                    // Keep the timer ready so it retries next minute.
                    elapsedMinutesByMachine[machineId] =
                        process.IntervalGameMinutes;
                }
            }
        }

        private bool TryProcess(
            MachineState machine,
            MachineProcessDefinition process)
        {
            string machineId =
                machine.Definition.MachineId;

            if (!resourceSystem.HasAll(process.Inputs))
            {
                bool statusChanged =
                    machineSystem.SetInputsAvailable(
                        machineId,
                        false,
                        "Required resources unavailable"
                    );

                if (statusChanged)
                {
                    eventBus.Publish(
                        new MachineProcessBlockedEvent(
                            machineId,
                            MachineStatus.MissingInputs,
                            "Required resources unavailable"
                        )
                    );
                }

                return false;
            }

            bool consumed =
                resourceSystem.TryConsumeAll(
                    process.Inputs,
                    $"{machine.Definition.DisplayName} input"
                );

            if (!consumed)
                return false;

            float outputMultiplier =
                process.ScaleOutputsByEfficiency
                    ? machine.Efficiency
                    : 1f;

            resourceSystem.AddAll(
                process.Outputs,
                outputMultiplier,
                $"{machine.Definition.DisplayName} output"
            );

            machineSystem.SetInputsAvailable(
                machineId,
                true,
                "Processing resumed"
            );

            eventBus.Publish(
                new MachineProcessCompletedEvent(
                    machineId,
                    machine.Efficiency
                )
            );

            return true;
        }

        public object CaptureState()
        {
            MachineProcessingSaveState saveState =
                new MachineProcessingSaveState();

            foreach (KeyValuePair<string, int> pair
                     in elapsedMinutesByMachine)
            {
                saveState.machines.Add(
                    new MachineProcessingSaveEntry
                    {
                        machineId = pair.Key,
                        elapsedGameMinutes = pair.Value
                    }
                );
            }

            return saveState;
        }

        public void RestoreState(object state)
        {
            if (state is not MachineProcessingSaveState saveState)
                return;

            foreach (MachineProcessingSaveEntry entry
                     in saveState.machines)
            {
                if (entry == null ||
                    string.IsNullOrWhiteSpace(entry.machineId))
                {
                    continue;
                }

                if (!elapsedMinutesByMachine.ContainsKey(
                        entry.machineId))
                {
                    continue;
                }

                elapsedMinutesByMachine[entry.machineId] =
                    Math.Max(0, entry.elapsedGameMinutes);
            }

            debugLogger.Log(
                "Machine processing timers restored.",
                "Machines"
            );
        }

        private void RefreshInputAvailability()
        {
            foreach (MachineState machine in machineSystem.Machines)
            {
                MachineProcessDefinition process =
                    machine.Definition.ProcessDefinition;

                if (process == null)
                    continue;

                bool inputsAvailable =
                    resourceSystem.HasAll(process.Inputs);

                machineSystem.SetInputsAvailable(
                    machine.Definition.MachineId,
                    inputsAvailable,
                    inputsAvailable
                        ? "Inputs available after load"
                        : "Inputs unavailable after load"
                );
            }
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<GameMinutePassedEvent>(
                OnGameMinutePassed
            );

            eventBus.Unsubscribe<AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );
        }
    }
}
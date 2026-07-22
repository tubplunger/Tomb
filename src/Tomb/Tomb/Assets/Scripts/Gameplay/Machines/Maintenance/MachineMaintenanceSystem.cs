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
    public sealed class MachineMaintenanceSystem : ISaveable
    {
        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;
        private readonly MachineSystem machineSystem;
        private readonly ResourceSystem resourceSystem;

        private readonly Dictionary<string, int>
            elapsedMinutesByMachine = new();

        private readonly Dictionary<string, bool>
            criticalWarningByMachine = new();

        public string SaveKey =>
            "machine_maintenance";

        public Type SaveStateType =>
            typeof(MachineMaintenanceSaveState);

        public MachineMaintenanceSystem(
            EventBus eventBus,
            DebugLogger debugLogger,
            MachineSystem machineSystem,
            ResourceSystem resourceSystem)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;
            this.machineSystem = machineSystem;
            this.resourceSystem = resourceSystem;

            InitializeStates();

            this.eventBus.Subscribe<GameMinutePassedEvent>(
                OnGameMinutePassed
            );

            this.eventBus.Subscribe<MachineConditionChangedEvent>(
                OnMachineConditionChanged
            );

            debugLogger.Log(
                "Machine maintenance system initialized.",
                "Machines"
            );
        }

        public int GetElapsedMinutes(string machineId)
        {
            return elapsedMinutesByMachine.TryGetValue(
                machineId,
                out int elapsed)
                ? elapsed
                : 0;
        }

        public int GetMinutesUntilNextDegradation(
            string machineId)
        {
            if (!machineSystem.TryGetMachine(
                    machineId,
                    out MachineState machine))
            {
                return 0;
            }

            MachineMaintenanceProfile profile =
                machine.Definition.MaintenanceProfile;

            if (profile == null)
                return 0;

            int elapsed = GetElapsedMinutes(machineId);

            return Math.Max(
                0,
                profile.DegradationIntervalGameMinutes - elapsed
            );
        }

        public bool IsCritical(MachineState machine)
        {
            if (machine == null)
                return false;

            MachineMaintenanceProfile profile =
                machine.Definition.MaintenanceProfile;

            if (profile == null || machine.IsBroken)
                return false;

            return machine.NormalizedCondition <=
                   profile.CriticalConditionNormalized;
        }

        public bool TryRepair(
            string machineId,
            string reason = "Unspecified")
        {
            if (!machineSystem.TryGetMachine(
                    machineId,
                    out MachineState machine))
            {
                PublishRepairFailure(
                    machineId,
                    "Unknown machine"
                );

                return false;
            }

            MachineMaintenanceProfile profile =
                machine.Definition.MaintenanceProfile;

            if (profile == null)
            {
                PublishRepairFailure(
                    machineId,
                    "Machine has no maintenance profile"
                );

                return false;
            }

            if (machine.CurrentCondition >=
                machine.MaximumCondition)
            {
                PublishRepairFailure(
                    machineId,
                    "Machine is already at maximum condition"
                );

                return false;
            }

            if (!resourceSystem.HasAll(profile.RepairCosts))
            {
                PublishRepairFailure(
                    machineId,
                    "Required repair resources are unavailable"
                );

                return false;
            }

            bool consumed =
                resourceSystem.TryConsumeAll(
                    profile.RepairCosts,
                    $"{machine.Definition.DisplayName} repair"
                );

            if (!consumed)
            {
                PublishRepairFailure(
                    machineId,
                    "Repair resources could not be consumed"
                );

                return false;
            }

            float restored =
                machineSystem.Repair(
                    machineId,
                    profile.ConditionRestoredPerRepair,
                    reason
                );

            if (restored <= 0f)
            {
                PublishRepairFailure(
                    machineId,
                    "No condition was restored"
                );

                return false;
            }

            eventBus.Publish(
                new MachineRepairCompletedEvent(
                    machineId,
                    restored,
                    machine.CurrentCondition
                )
            );

            EvaluateCriticalState(machine);

            return true;
        }

        private void InitializeStates()
        {
            foreach (MachineState machine
                     in machineSystem.Machines)
            {
                if (machine.Definition.MaintenanceProfile == null)
                    continue;

                string machineId =
                    machine.Definition.MachineId;

                elapsedMinutesByMachine[machineId] = 0;
                criticalWarningByMachine[machineId] =
                    IsCritical(machine);
            }
        }

        private void OnGameMinutePassed(
            GameMinutePassedEvent minuteEvent)
        {
            foreach (MachineState machine
                     in machineSystem.Machines)
            {
                MachineMaintenanceProfile profile =
                    machine.Definition.MaintenanceProfile;

                if (profile == null || machine.IsBroken)
                    continue;

                if (!profile.DegradeWhileDisabled &&
                    !machine.IsEnabled)
                {
                    continue;
                }

                string machineId =
                    machine.Definition.MachineId;

                int elapsed =
                    elapsedMinutesByMachine[machineId] + 1;

                if (elapsed <
                    profile.DegradationIntervalGameMinutes)
                {
                    elapsedMinutesByMachine[machineId] =
                        elapsed;

                    continue;
                }

                elapsedMinutesByMachine[machineId] = 0;

                ApplyDegradation(
                    machine,
                    profile
                );
            }
        }

        private void ApplyDegradation(
            MachineState machine,
            MachineMaintenanceProfile profile)
        {
            if (profile.DegradationPerInterval <= 0f)
                return;

            string machineId =
                machine.Definition.MachineId;

            float conditionLost =
                machineSystem.Damage(
                    machineId,
                    profile.DegradationPerInterval,
                    "Normal machine degradation"
                );

            if (conditionLost <= 0f)
                return;

            eventBus.Publish(
                new MachineDegradedEvent(
                    machineId,
                    conditionLost,
                    machine.CurrentCondition
                )
            );

            EvaluateCriticalState(machine);
        }

        private void OnMachineConditionChanged(
            MachineConditionChangedEvent conditionEvent)
        {
            if (!machineSystem.TryGetMachine(
                    conditionEvent.MachineId,
                    out MachineState machine))
            {
                return;
            }

            EvaluateCriticalState(machine);
        }

        private void EvaluateCriticalState(
            MachineState machine)
        {
            string machineId =
                machine.Definition.MachineId;

            if (!criticalWarningByMachine.ContainsKey(machineId))
                return;

            bool wasCritical =
                criticalWarningByMachine[machineId];

            bool isCritical =
                IsCritical(machine);

            if (wasCritical == isCritical)
                return;

            criticalWarningByMachine[machineId] =
                isCritical;

            if (isCritical)
            {
                eventBus.Publish(
                    new MachineCriticalConditionEvent(
                        machineId,
                        machine.CurrentCondition,
                        machine.MaximumCondition
                    )
                );
            }
            else
            {
                eventBus.Publish(
                    new MachineConditionRecoveredEvent(
                        machineId,
                        machine.CurrentCondition
                    )
                );
            }
        }

        private void PublishRepairFailure(
            string machineId,
            string reason)
        {
            eventBus.Publish(
                new MachineRepairFailedEvent(
                    machineId,
                    reason
                )
            );

            debugLogger.Log(
                $"Repair failed for '{machineId}': {reason}",
                "Machines"
            );
        }

        public object CaptureState()
        {
            MachineMaintenanceSaveState saveState =
                new MachineMaintenanceSaveState();

            foreach (KeyValuePair<string, int> pair
                     in elapsedMinutesByMachine)
            {
                saveState.machines.Add(
                    new MachineMaintenanceSaveEntry
                    {
                        machineId = pair.Key,

                        elapsedGameMinutes =
                            pair.Value,

                        criticalWarningActive =
                            criticalWarningByMachine.TryGetValue(
                                pair.Key,
                                out bool critical) &&
                            critical
                    }
                );
            }

            return saveState;
        }

        public void RestoreState(object state)
        {
            if (state is not MachineMaintenanceSaveState saveState)
                return;

            foreach (MachineMaintenanceSaveEntry entry
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
                    Math.Max(
                        0,
                        entry.elapsedGameMinutes
                    );

                criticalWarningByMachine[entry.machineId] =
                    entry.criticalWarningActive;
            }

            debugLogger.Log(
                "Machine maintenance state restored.",
                "Machines"
            );
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<GameMinutePassedEvent>(
                OnGameMinutePassed
            );

            eventBus.Unsubscribe<MachineConditionChangedEvent>(
                OnMachineConditionChanged
            );
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Machines
{
    public readonly struct MachineDegradedEvent : IGameEvent
    {
        public readonly string MachineId;
        public readonly float ConditionLost;
        public readonly float CurrentCondition;

        public MachineDegradedEvent(
            string machineId,
            float conditionLost,
            float currentCondition)
        {
            MachineId = machineId;
            ConditionLost = conditionLost;
            CurrentCondition = currentCondition;
        }
    }

    public readonly struct MachineCriticalConditionEvent : IGameEvent
    {
        public readonly string MachineId;
        public readonly float CurrentCondition;
        public readonly float MaximumCondition;

        public MachineCriticalConditionEvent(
            string machineId,
            float currentCondition,
            float maximumCondition)
        {
            MachineId = machineId;
            CurrentCondition = currentCondition;
            MaximumCondition = maximumCondition;
        }
    }

    public readonly struct MachineConditionRecoveredEvent : IGameEvent
    {
        public readonly string MachineId;
        public readonly float CurrentCondition;

        public MachineConditionRecoveredEvent(
            string machineId,
            float currentCondition)
        {
            MachineId = machineId;
            CurrentCondition = currentCondition;
        }
    }

    public readonly struct MachineRepairCompletedEvent : IGameEvent
    {
        public readonly string MachineId;
        public readonly float ConditionRestored;
        public readonly float CurrentCondition;

        public MachineRepairCompletedEvent(
            string machineId,
            float conditionRestored,
            float currentCondition)
        {
            MachineId = machineId;
            ConditionRestored = conditionRestored;
            CurrentCondition = currentCondition;
        }
    }

    public readonly struct MachineRepairFailedEvent : IGameEvent
    {
        public readonly string MachineId;
        public readonly string Reason;

        public MachineRepairFailedEvent(
            string machineId,
            string reason)
        {
            MachineId = machineId;
            Reason = reason;
        }
    }
}
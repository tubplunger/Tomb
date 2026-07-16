using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Machines
{
    public readonly struct MachineStateChangedEvent : IGameEvent
    {
        public readonly string MachineId;
        public readonly MachineStatus PreviousStatus;
        public readonly MachineStatus CurrentStatus;
        public readonly bool IsEnabled;
        public readonly float CurrentCondition;
        public readonly string Reason;

        public MachineStateChangedEvent(
            string machineId,
            MachineStatus previousStatus,
            MachineStatus currentStatus,
            bool isEnabled,
            float currentCondition,
            string reason)
        {
            MachineId = machineId;
            PreviousStatus = previousStatus;
            CurrentStatus = currentStatus;
            IsEnabled = isEnabled;
            CurrentCondition = currentCondition;
            Reason = reason;
        }
    }

    public readonly struct MachineConditionChangedEvent : IGameEvent
    {
        public readonly string MachineId;
        public readonly float PreviousCondition;
        public readonly float CurrentCondition;
        public readonly float ChangeAmount;
        public readonly string Reason;

        public MachineConditionChangedEvent(
            string machineId,
            float previousCondition,
            float currentCondition,
            float changeAmount,
            string reason)
        {
            MachineId = machineId;
            PreviousCondition = previousCondition;
            CurrentCondition = currentCondition;
            ChangeAmount = changeAmount;
            Reason = reason;
        }
    }

    public readonly struct MachineBrokenEvent : IGameEvent
    {
        public readonly string MachineId;

        public MachineBrokenEvent(string machineId)
        {
            MachineId = machineId;
        }
    }

    public readonly struct MachineRepairedEvent : IGameEvent
    {
        public readonly string MachineId;
        public readonly float CurrentCondition;

        public MachineRepairedEvent(
            string machineId,
            float currentCondition)
        {
            MachineId = machineId;
            CurrentCondition = currentCondition;
        }
    }

    public readonly struct MachinesRestoredFromSaveEvent : IGameEvent
    {
        public readonly int RestoredMachineCount;

        public MachinesRestoredFromSaveEvent(
            int restoredMachineCount)
        {
            RestoredMachineCount = restoredMachineCount;
        }
    }
}
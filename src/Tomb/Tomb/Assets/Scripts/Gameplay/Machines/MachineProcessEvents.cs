using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Machines
{
    public readonly struct MachineProcessCompletedEvent : IGameEvent
    {
        public readonly string MachineId;
        public readonly float Efficiency;

        public MachineProcessCompletedEvent(
            string machineId,
            float efficiency)
        {
            MachineId = machineId;
            Efficiency = efficiency;
        }
    }

    public readonly struct MachineProcessBlockedEvent : IGameEvent
    {
        public readonly string MachineId;
        public readonly MachineStatus Status;
        public readonly string Reason;

        public MachineProcessBlockedEvent(
            string machineId,
            MachineStatus status,
            string reason)
        {
            MachineId = machineId;
            Status = status;
            Reason = reason;
        }
    }
}
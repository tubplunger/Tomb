using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Power
{
    public readonly struct PowerBalanceUpdatedEvent : IGameEvent
    {
        public readonly float Generation;
        public readonly float TotalDemand;
        public readonly float ServedDemand;
        public readonly float BatteryCharge;

        public PowerBalanceUpdatedEvent(
            float generation,
            float totalDemand,
            float servedDemand,
            float batteryCharge)
        {
            Generation = generation;
            TotalDemand = totalDemand;
            ServedDemand = servedDemand;
            BatteryCharge = batteryCharge;
        }
    }

    public readonly struct BatteryChargeChangedEvent : IGameEvent
    {
        public readonly float PreviousCharge;
        public readonly float CurrentCharge;
        public readonly float MaximumCharge;
        public readonly float ChangeAmount;

        public BatteryChargeChangedEvent(
            float previousCharge,
            float currentCharge,
            float maximumCharge,
            float changeAmount)
        {
            PreviousCharge = previousCharge;
            CurrentCharge = currentCharge;
            MaximumCharge = maximumCharge;
            ChangeAmount = changeAmount;
        }
    }

    public readonly struct MachinePowerAllocationChangedEvent : IGameEvent
    {
        public readonly string MachineId;
        public readonly bool HasPower;
        public readonly float Demand;

        public MachinePowerAllocationChangedEvent(
            string machineId,
            bool hasPower,
            float demand)
        {
            MachineId = machineId;
            HasPower = hasPower;
            Demand = demand;
        }
    }

    public readonly struct PowerShortageStartedEvent : IGameEvent
    {
        public readonly float Demand;
        public readonly float AvailablePower;

        public PowerShortageStartedEvent(
            float demand,
            float availablePower)
        {
            Demand = demand;
            AvailablePower = availablePower;
        }
    }

    public readonly struct PowerShortageEndedEvent : IGameEvent
    {
    }
}
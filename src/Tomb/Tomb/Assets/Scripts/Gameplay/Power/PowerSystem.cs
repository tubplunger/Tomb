using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;
using Tomb.Core.Time;
using Tomb.Gameplay.Machines;

namespace Tomb.Gameplay.Power
{
    public sealed class PowerSystem : ISaveable
    {
        private const float Tolerance = 0.0001f;

        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;
        private readonly MachineSystem machineSystem;
        private readonly PowerSettings settings;

        private float batteryCharge;
        private bool shortageActive;

        private bool isEvaluating;

        public string SaveKey =>
            "station_power";

        public Type SaveStateType =>
            typeof(PowerSaveState);

        public float BatteryCharge =>
            batteryCharge;

        public float BatteryCapacity =>
            settings.BatteryCapacity;

        public float BatteryNormalized =>
            BatteryCapacity <= 0f
                ? 0f
                : batteryCharge / BatteryCapacity;

        public float CurrentGeneration { get; private set; }
        public float CurrentDemand { get; private set; }
        public float CurrentServedDemand { get; private set; }

        public PowerSystem(
            EventBus eventBus,
            DebugLogger debugLogger,
            MachineSystem machineSystem,
            PowerSettings settings)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;
            this.machineSystem = machineSystem;
            this.settings = settings;

            batteryCharge =
                settings.StartingBatteryCharge;

            this.eventBus.Subscribe<GameMinutePassedEvent>(
                OnGameMinutePassed
            );

            this.eventBus.Subscribe<MachineStateChangedEvent>(
                OnMachineStateChanged
            );

            this.eventBus.Subscribe<MachineConditionChangedEvent>(
                OnMachineConditionChanged
            );

            EvaluatePower();

            debugLogger.Log(
                "Power system initialized.",
                "Power"
            );
        }

        private void OnGameMinutePassed(
            GameMinutePassedEvent minuteEvent)
        {
            EvaluatePower();
        }

        private void OnMachineStateChanged(
            MachineStateChangedEvent stateEvent)
        {
            EvaluatePower();
        }

        private void OnMachineConditionChanged(
            MachineConditionChangedEvent conditionEvent)
        {
            EvaluatePower();
        }

        public void EvaluatePower()
        {
            if (isEvaluating)
                return;

            isEvaluating = true;

            try
            {
                List<MachineState> consumers =
                GetActiveConsumers();

                CurrentGeneration =
                    CalculateGeneration();

                CurrentDemand =
                    consumers.Sum(
                        machine =>
                            machine.Definition
                                .PowerProfile
                                .DemandPerGameMinute
                    );

                float batteryAvailable =
                    IsBatteryAvailable()
                        ? Math.Min(
                            batteryCharge,
                            settings.MaximumDischargePerGameMinute
                        )
                        : 0f;

                float totalAvailable =
                    CurrentGeneration + batteryAvailable;

                float servedDemand = 0f;

                foreach (MachineState consumer in consumers)
                {
                    float demand =
                        consumer.Definition
                            .PowerProfile
                            .DemandPerGameMinute;

                    bool hasPower =
                        servedDemand + demand <=
                        totalAvailable + Tolerance;

                    SetMachinePower(
                        consumer,
                        hasPower,
                        demand
                    );

                    if (hasPower)
                    {
                        servedDemand += demand;
                    }
                }

                CurrentServedDemand = servedDemand;

                UpdateBattery(
                    CurrentGeneration,
                    servedDemand
                );

                UpdateShortageState(
                    CurrentDemand,
                    totalAvailable
                );

                eventBus.Publish(
                    new PowerBalanceUpdatedEvent(
                        CurrentGeneration,
                        CurrentDemand,
                        CurrentServedDemand,
                        batteryCharge
                    )
                );
            }
            finally
            {
                isEvaluating = false;
            }
        }

        private List<MachineState> GetActiveConsumers()
        {
            return machineSystem.Machines
                .Where(machine =>
                    machine.Definition.PowerProfile != null &&
                    machine.Definition.PowerProfile.IsConsumer &&
                    machine.IsEnabled &&
                    !machine.IsBroken &&
                    !machine.IsInMaintenance)
                .OrderBy(machine =>
                    machine.Definition.Priority)
                .ThenBy(machine =>
                    machine.Definition.MachineId)
                .ToList();
        }

        private float CalculateGeneration()
        {
            float generation = 0f;

            foreach (MachineState machine
                     in machineSystem.Machines)
            {
                MachinePowerProfile profile =
                    machine.Definition.PowerProfile;

                if (profile == null ||
                    !profile.IsProducer ||
                    !machine.IsEnabled ||
                    machine.IsBroken ||
                    machine.IsInMaintenance)
                {
                    continue;
                }

                generation +=
                    profile.GenerationPerGameMinute *
                    machine.Efficiency;
            }

            return generation;
        }

        private void SetMachinePower(
            MachineState machine,
            bool hasPower,
            float demand)
        {
            bool changed =
                machineSystem.SetPowerAvailable(
                    machine.Definition.MachineId,
                    hasPower,
                    hasPower
                        ? "Power allocated"
                        : "Power unavailable"
                );

            if (!changed)
                return;

            eventBus.Publish(
                new MachinePowerAllocationChangedEvent(
                    machine.Definition.MachineId,
                    hasPower,
                    demand
                )
            );
        }

        private void UpdateBattery(
            float generation,
            float servedDemand)
        {
            if (!IsBatteryAvailable())
                return;

            float previousCharge =
                batteryCharge;

            if (generation > servedDemand)
            {
                float surplus =
                    generation - servedDemand;

                float chargeAmount =
                    Math.Min(
                        surplus,
                        settings.MaximumChargePerGameMinute
                    );

                batteryCharge =
                    Math.Min(
                        settings.BatteryCapacity,
                        batteryCharge + chargeAmount
                    );
            }
            else if (servedDemand > generation)
            {
                float deficit =
                    servedDemand - generation;

                float dischargeAmount =
                    Math.Min(
                        deficit,
                        settings.MaximumDischargePerGameMinute
                    );

                batteryCharge =
                    Math.Max(
                        0f,
                        batteryCharge - dischargeAmount
                    );
            }

            float change =
                batteryCharge - previousCharge;

            if (Math.Abs(change) <= Tolerance)
                return;

            eventBus.Publish(
                new BatteryChargeChangedEvent(
                    previousCharge,
                    batteryCharge,
                    settings.BatteryCapacity,
                    change
                )
            );
        }

        private bool IsBatteryAvailable()
        {
            if (!machineSystem.TryGetMachine(
                    settings.BatteryMachineId,
                    out MachineState batteryMachine))
            {
                return false;
            }

            return batteryMachine.IsEnabled &&
                   !batteryMachine.IsBroken &&
                   !batteryMachine.IsInMaintenance;
        }

        private void UpdateShortageState(
            float demand,
            float available)
        {
            bool hasShortage =
                demand > available + Tolerance;

            if (hasShortage == shortageActive)
                return;

            shortageActive = hasShortage;

            if (shortageActive)
            {
                eventBus.Publish(
                    new PowerShortageStartedEvent(
                        demand,
                        available
                    )
                );
            }
            else
            {
                eventBus.Publish(
                    new PowerShortageEndedEvent()
                );
            }
        }

        public object CaptureState()
        {
            return new PowerSaveState
            {
                batteryCharge = batteryCharge
            };
        }

        public void RestoreState(object state)
        {
            if (state is not PowerSaveState saveState)
                return;

            batteryCharge =
                Math.Clamp(
                    saveState.batteryCharge,
                    0f,
                    settings.BatteryCapacity
                );

            EvaluatePower();

            debugLogger.Log(
                $"Battery restored to {batteryCharge:0.##}.",
                "Power"
            );
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<GameMinutePassedEvent>(
                OnGameMinutePassed
            );

            eventBus.Unsubscribe<MachineStateChangedEvent>(
                OnMachineStateChanged
            );

            eventBus.Unsubscribe<MachineConditionChangedEvent>(
                OnMachineConditionChanged
            );
        }
    }
}
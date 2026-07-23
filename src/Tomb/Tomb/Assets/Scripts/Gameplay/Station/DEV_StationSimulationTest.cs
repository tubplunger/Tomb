using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;
using Tomb.Core.Services;
using Tomb.Gameplay.Machines;
using Tomb.Gameplay.Power;
using Tomb.Gameplay.Resources;

namespace Tomb.Gameplay.Station
{
    public sealed class DEV_StationSimulationTest : MonoBehaviour
    {
        [SerializeField]
        private StationTestSettings settings;

        private EventBus eventBus;
        private ResourceSystem resourceSystem;
        private MachineSystem machineSystem;
        private MachineMaintenanceSystem maintenanceSystem;
        private PowerSystem powerSystem;

        private void Start()
        {
            if (settings == null)
            {
                Debug.LogError(
                    "[DEV Station Test] Missing StationTestSettings."
                );

                enabled = false;
                return;
            }

            eventBus =
                CoreServices.Get<EventBus>();

            resourceSystem =
                CoreServices.Get<ResourceSystem>();

            machineSystem =
                CoreServices.Get<MachineSystem>();

            maintenanceSystem =
                CoreServices.Get<MachineMaintenanceSystem>();

            powerSystem =
                CoreServices.Get<PowerSystem>();

            Debug.Log(
                "[DEV Station Test] Integration controls initialized."
            );
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                TriggerLowOxygenTest();
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                TriggerPowerFailureTest();
            }

            if (Input.GetKeyDown(KeyCode.F12))
            {
                TriggerMachineFailureTest();
            }

            if (Input.GetKeyDown(KeyCode.Home))
            {
                AttemptOxygenGeneratorRepair();
            }

            if (Input.GetKeyDown(KeyCode.End))
            {
                RestoreBasicTestState();
            }
        }

        private void TriggerLowOxygenTest()
        {
            resourceSystem.SetAmount(
                settings.OxygenResourceId,
                settings.LowOxygenAmount,
                "Station integration test"
            );

            PublishStep(
                "LOW OXYGEN",
                $"Set oxygen to {settings.LowOxygenAmount:0.##}."
            );
        }

        private void TriggerPowerFailureTest()
        {
            machineSystem.SetEnabled(
                settings.SolarArrayMachineId,
                false,
                "Station integration test"
            );

            machineSystem.SetEnabled(
                settings.BatteryMachineId,
                false,
                "Station integration test"
            );

            powerSystem.EvaluatePower();

            PublishStep(
                "POWER FAILURE",
                "Disabled solar generation and battery storage."
            );
        }

        private void TriggerMachineFailureTest()
        {
            machineSystem.Damage(
                settings.OxygenGeneratorMachineId,
                settings.MachineDamageAmount,
                "Station integration test"
            );

            PublishStep(
                "MACHINE DAMAGE",
                $"Damaged Oxygen Generator by " +
                $"{settings.MachineDamageAmount:0.##}."
            );
        }

        private void AttemptOxygenGeneratorRepair()
        {
            bool repaired =
                maintenanceSystem.TryRepair(
                    settings.OxygenGeneratorMachineId,
                    "Station integration test"
                );

            PublishStep(
                "REPAIR ATTEMPT",
                repaired
                    ? "Oxygen Generator repaired."
                    : "Oxygen Generator repair failed."
            );
        }

        private void RestoreBasicTestState()
        {
            machineSystem.SetEnabled(
                settings.SolarArrayMachineId,
                true,
                "Station integration reset"
            );

            machineSystem.SetEnabled(
                settings.BatteryMachineId,
                true,
                "Station integration reset"
            );

            resourceSystem.SetAmount(
                settings.OxygenResourceId,
                resourceSystem
                    .GetResource(settings.OxygenResourceId)
                    .MaximumAmount,
                "Station integration reset"
            );

            resourceSystem.SetAmount(
                settings.WaterResourceId,
                resourceSystem
                    .GetResource(settings.WaterResourceId)
                    .MaximumAmount,
                "Station integration reset"
            );

            resourceSystem.SetAmount(
                settings.RepairMaterialsResourceId,
                resourceSystem
                    .GetResource(
                        settings.RepairMaterialsResourceId
                    )
                    .MaximumAmount,
                "Station integration reset"
            );

            machineSystem.SetCondition(
                settings.OxygenGeneratorMachineId,
                machineSystem
                    .GetMachine(
                        settings.OxygenGeneratorMachineId
                    )
                    .MaximumCondition,
                "Station integration reset"
            );

            powerSystem.EvaluatePower();

            PublishStep(
                "TEST RESET",
                "Restored core test systems and supplies."
            );
        }

        private void PublishStep(
            string stepName,
            string description)
        {
            eventBus.Publish(
                new StationTestStepEvent(
                    stepName,
                    description
                )
            );

            Debug.Log(
                $"[DEV Station Test] {stepName}: {description}"
            );
        }
    }
}
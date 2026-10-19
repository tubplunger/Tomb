using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Tomb.Core.Events;
using Tomb.Core.Save;
using Tomb.Core.Services;
using Tomb.Gameplay.Resources;
using Tomb.Gameplay.Power;
using Tomb.Gameplay.Machines;
using Tomb.Gameplay.Orbit;

namespace Tomb.Presentation.Station
{
    public sealed class StationOverviewPresenter : MonoBehaviour
    {
        [Header("View")]
        [SerializeField]
        private StationOverviewUI overviewUI;

        [Header("Resource IDs")]
        [SerializeField]
        private string oxygenResourceId = "oxygen";

        [SerializeField]
        private string waterResourceId = "water";

        [Header("Machine IDs")]
        [SerializeField]
        private string oxygenGeneratorId =
            "oxygen_generator";

        [SerializeField]
        private string waterRecyclerId =
            "water_recycler";

        [SerializeField]
        private string co2ScrubberId =
            "co2_scrubber";

        [SerializeField]
        private string communicationsMachineId =
            "communications_array";

        private EventBus eventBus;

        private ResourceSystem resourceSystem;
        private PowerSystem powerSystem;
        private MachineSystem machineSystem;
        private OrbitSystem orbitSystem;

        private bool initialized;

        private void OnEnable()
        {
            if (!initialized)
                Initialize();

            if (initialized)
                Refresh();
        }

        private void Initialize()
        {
            if (overviewUI == null)
            {
                Debug.LogError(
                    "Station Overview Presenter: " +
                    "Overview UI reference is missing.",
                    this
                );
                return;
            }

            try
            {
                eventBus = CoreServices.Get<EventBus>();

                resourceSystem =
                    CoreServices.Get<ResourceSystem>();

                powerSystem =
                    CoreServices.Get<PowerSystem>();

                machineSystem =
                    CoreServices.Get<MachineSystem>();

                orbitSystem =
                    CoreServices.Get<OrbitSystem>();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "Station Overview Presenter: " +
                    "Unable to access simulation services. " +
                    exception.Message,
                    this
                );
                return;
            }

            eventBus.Subscribe<ResourceChangedEvent>(
                OnResourceChanged);

            eventBus.Subscribe<PowerBalanceUpdatedEvent>(
                OnPowerUpdated);

            eventBus.Subscribe<BatteryChargeChangedEvent>(
                OnBatteryChanged);

            eventBus.Subscribe<MachineStateChangedEvent>(
                OnMachineChanged);

            eventBus.Subscribe<MachineConditionChangedEvent>(
                OnMachineConditionChanged);

            eventBus.Subscribe<OrbitUpdatedEvent>(
                OnOrbitUpdated);

            eventBus.Subscribe<AllSaveDataRestoredEvent>(
                OnSaveRestored);

            initialized = true;
        }

        private void Refresh()
        {
            if (!initialized)
                return;

            float oxygen =
                GetResourcePercentage(oxygenResourceId);

            float water =
                GetResourcePercentage(waterResourceId);

            float battery =
                powerSystem.BatteryNormalized * 100f;

            overviewUI.DisplayResources(
                oxygen,
                water,
                battery
            );

            overviewUI.DisplayOrbit(
                orbitSystem.CompletedOrbits
            );

            string lifeSupport =
                GetLifeSupportStatus();

            string powerGrid =
                GetPowerGridStatus();

            string communications =
                GetMachineStatus(
                    communicationsMachineId
                );

            overviewUI.DisplaySystemStatuses(
                lifeSupport,
                powerGrid,
                communications
            );
        }

        private float GetResourcePercentage(
            string resourceId)
        {
            if (!resourceSystem.TryGetResource(
                    resourceId,
                    out ResourceState resource))
            {
                return 0f;
            }

            if (resource.MaximumAmount <= 0f)
                return 0f;

            return Mathf.Clamp01(
                resource.CurrentAmount /
                resource.MaximumAmount
            ) * 100f;
        }

        private string GetLifeSupportStatus()
        {
            bool oxygenWorking =
                IsMachineOperational(
                    oxygenGeneratorId);

            bool waterWorking =
                IsMachineOperational(
                    waterRecyclerId);

            bool scrubberWorking =
                IsMachineOperational(
                    co2ScrubberId);

            if (oxygenWorking &&
                waterWorking &&
                scrubberWorking)
            {
                return "OPERATIONAL";
            }

            if (oxygenWorking ||
                waterWorking ||
                scrubberWorking)
            {
                return "DEGRADED";
            }

            return "OFFLINE";
        }

        private bool IsMachineOperational(
            string machineId)
        {
            if (!machineSystem.TryGetMachine(
                    machineId,
                    out MachineState machine))
            {
                return false;
            }

            return machine.IsEnabled &&
                   !machine.IsBroken &&
                   !machine.IsInMaintenance &&
                   machine.HasPower;
        }

        private string GetMachineStatus(
            string machineId)
        {
            if (!machineSystem.TryGetMachine(
                    machineId,
                    out MachineState machine))
            {
                return "UNKNOWN";
            }

            return machine.Status.ToString()
                .ToUpperInvariant();
        }

        private string GetPowerGridStatus()
        {
            if (powerSystem.CurrentServedDemand + 0.0001f <
                powerSystem.CurrentDemand)
            {
                return "SHORTAGE";
            }

            if (powerSystem.BatteryNormalized <= 0.15f)
            {
                return "LOW BATTERY";
            }

            return "STABLE";
        }

        private void OnResourceChanged(
            ResourceChangedEvent changeEvent)
        {
            Refresh();
        }

        private void OnPowerUpdated(
            PowerBalanceUpdatedEvent powerEvent)
        {
            Refresh();
        }

        private void OnBatteryChanged(
            BatteryChargeChangedEvent batteryEvent)
        {
            Refresh();
        }

        private void OnMachineChanged(
            MachineStateChangedEvent machineEvent)
        {
            Refresh();
        }

        private void OnMachineConditionChanged(
            MachineConditionChangedEvent conditionEvent)
        {
            Refresh();
        }

        private void OnOrbitUpdated(
            OrbitUpdatedEvent orbitEvent)
        {
            Refresh();
        }

        private void OnSaveRestored(
            AllSaveDataRestoredEvent restoredEvent)
        {
            Refresh();
        }

        private void OnDestroy()
        {
            if (!initialized || eventBus == null)
                return;

            eventBus.Unsubscribe<ResourceChangedEvent>(
                OnResourceChanged);

            eventBus.Unsubscribe<PowerBalanceUpdatedEvent>(
                OnPowerUpdated);

            eventBus.Unsubscribe<BatteryChargeChangedEvent>(
                OnBatteryChanged);

            eventBus.Unsubscribe<MachineStateChangedEvent>(
                OnMachineChanged);

            eventBus.Unsubscribe<MachineConditionChangedEvent>(
                OnMachineConditionChanged);

            eventBus.Unsubscribe<OrbitUpdatedEvent>(
                OnOrbitUpdated);

            eventBus.Unsubscribe<AllSaveDataRestoredEvent>(
                OnSaveRestored);
        }
    }
}
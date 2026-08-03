using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;
using Tomb.Core.Services;
using Tomb.Gameplay.Earth;
using Tomb.Gameplay.Machines;
using Tomb.Gameplay.Power;
using Tomb.Gameplay.Radio;

namespace Tomb.Gameplay.Orbit
{
    public sealed class DEV_Phase3IntegrationTest :
        MonoBehaviour
    {
        [SerializeField]
        private Phase3TestSettings settings;

        private EventBus eventBus;
        private OrbitSystem orbitSystem;
        private OrbitLightingSystem lightingSystem;
        private EarthRegionSystem earthRegionSystem;
        private RadioVisibilitySystem radioVisibilitySystem;
        private MachineSystem machineSystem;
        private PowerSystem powerSystem;

        private void Start()
        {
            if (settings == null)
            {
                Debug.LogError(
                    "[DEV Phase 3] Missing Phase3TestSettings."
                );

                enabled = false;
                return;
            }

            eventBus =
                CoreServices.Get<EventBus>();

            orbitSystem =
                CoreServices.Get<OrbitSystem>();

            lightingSystem =
                CoreServices.Get<OrbitLightingSystem>();

            earthRegionSystem =
                CoreServices.Get<EarthRegionSystem>();

            radioVisibilitySystem =
                CoreServices.Get<RadioVisibilitySystem>();

            machineSystem =
                CoreServices.Get<MachineSystem>();

            powerSystem =
                CoreServices.Get<PowerSystem>();

            Debug.Log(
                "[DEV Phase 3] Integration controls initialized."
            );
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                MoveToSunlight();
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                MoveToEclipse();
            }

            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                MovePastEclipse();
            }

            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                ToggleSolarArray();
            }

            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                ToggleBatteryBank();
            }

            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                PrintCurrentState();
            }
        }

        private void MoveToSunlight()
        {
            orbitSystem.SetOrbitProgress(
                settings.SunlightProgress,
                "Phase 3 sunlight test"
            );

            PublishStep(
                "SUNLIGHT TEST",
                $"Orbit moved to " +
                $"{settings.SunlightProgress * 100f:0}%."
            );
        }

        private void MoveToEclipse()
        {
            orbitSystem.SetOrbitProgress(
                settings.EclipseProgress,
                "Phase 3 eclipse test"
            );

            PublishStep(
                "ECLIPSE TEST",
                $"Orbit moved to " +
                $"{settings.EclipseProgress * 100f:0}%."
            );
        }

        private void MovePastEclipse()
        {
            orbitSystem.SetOrbitProgress(
                settings.PostEclipseProgress,
                "Phase 3 post-eclipse test"
            );

            PublishStep(
                "SUNLIGHT RESTORED TEST",
                $"Orbit moved to " +
                $"{settings.PostEclipseProgress * 100f:0}%."
            );
        }

        private void ToggleSolarArray()
        {
            machineSystem.ToggleEnabled(
                settings.SolarArrayMachineId,
                "Phase 3 integration test"
            );

            PublishStep(
                "SOLAR ARRAY TOGGLE",
                "Solar Array enabled state toggled."
            );
        }

        private void ToggleBatteryBank()
        {
            machineSystem.ToggleEnabled(
                settings.BatteryMachineId,
                "Phase 3 integration test"
            );

            PublishStep(
                "BATTERY BANK TOGGLE",
                "Battery Bank enabled state toggled."
            );
        }

        private void PrintCurrentState()
        {
            OrbitSnapshot orbit =
                orbitSystem.CurrentSnapshot;

            OrbitLightingSnapshot lighting =
                lightingSystem.CurrentSnapshot;

            EarthRegionDefinition region =
                earthRegionSystem.CurrentRegion;

            string regionName =
                region != null
                    ? region.DisplayName
                    : "Unclassified";

            string description =
                $"Orbit {orbit.OrbitProgress * 100f:0.0}% | " +
                $"{lighting.State} | " +
                $"Region {regionName} | " +
                $"Signals {radioVisibilitySystem.VisibleSignals.Count} | " +
                $"Generation {powerSystem.CurrentGeneration:0.##} | " +
                $"Battery {powerSystem.BatteryCharge:0.##}";

            PublishStep(
                "STATE SNAPSHOT",
                description
            );
        }

        private void PublishStep(
            string stepName,
            string description)
        {
            eventBus.Publish(
                new Phase3TestStepEvent(
                    stepName,
                    description
                )
            );

            Debug.Log(
                $"[DEV Phase 3] {stepName}: {description}"
            );
        }
    }
}
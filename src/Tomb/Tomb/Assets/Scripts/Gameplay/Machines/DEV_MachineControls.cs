using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Services;

namespace Tomb.Gameplay.Machines
{
    public sealed class DEV_MachineControls : MonoBehaviour
    {
        [Header("Direct Damage Test")]
        [Min(0.01f)]
        [SerializeField]
        private float conditionDamageAmount = 20f;

        private MachineSystem machineSystem;
        private MachineMaintenanceSystem maintenanceSystem;

        private IReadOnlyList<MachineState> machines;
        private int selectedIndex;

        private MachineState SelectedMachine
        {
            get
            {
                if (machines == null || machines.Count == 0)
                    return null;

                return machines[selectedIndex];
            }
        }

        private void Start()
        {
            machineSystem =
                CoreServices.Get<MachineSystem>();

            maintenanceSystem =
                CoreServices.Get<MachineMaintenanceSystem>();

            machines = machineSystem.Machines;
            selectedIndex = 0;

            LogSelectedMachine();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.PageDown))
                SelectNextMachine();

            if (Input.GetKeyDown(KeyCode.PageUp))
                SelectPreviousMachine();

            if (SelectedMachine == null)
                return;

            string machineId =
                SelectedMachine.Definition.MachineId;

            if (Input.GetKeyDown(KeyCode.F6))
            {
                machineSystem.ToggleEnabled(
                    machineId,
                    "Developer test"
                );
            }

            if (Input.GetKeyDown(KeyCode.F7))
            {
                machineSystem.Damage(
                    machineId,
                    conditionDamageAmount,
                    "Developer damage test"
                );
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                bool repaired =
                    maintenanceSystem.TryRepair(
                        machineId,
                        "Developer repair test"
                    );

                Debug.Log(
                    repaired
                        ? $"[DEV Machines] Repaired " +
                          $"{SelectedMachine.Definition.DisplayName}"
                        : $"[DEV Machines] Repair failed for " +
                          $"{SelectedMachine.Definition.DisplayName}"
                );
            }
        }

        private void SelectNextMachine()
        {
            if (machines == null || machines.Count == 0)
                return;

            selectedIndex =
                (selectedIndex + 1) % machines.Count;

            LogSelectedMachine();
        }

        private void SelectPreviousMachine()
        {
            if (machines == null || machines.Count == 0)
                return;

            selectedIndex--;

            if (selectedIndex < 0)
                selectedIndex = machines.Count - 1;

            LogSelectedMachine();
        }

        private void LogSelectedMachine()
        {
            if (SelectedMachine == null)
                return;

            Debug.Log(
                $"[DEV Machines] Selected: " +
                $"{SelectedMachine.Definition.DisplayName} " +
                $"[{SelectedMachine.Definition.MachineId}]"
            );
        }
    }
}
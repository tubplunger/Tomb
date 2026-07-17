using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Services;

namespace Tomb.Gameplay.Machines
{
    public sealed class DEV_MachineControls : MonoBehaviour
    {
        [Header("Condition Change")]
        [Min(0.01f)]
        [SerializeField]
        private float conditionAmount = 20f;

        private MachineSystem machineSystem;
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

            machines = machineSystem.Machines;

            selectedIndex = 0;

            LogSelectedMachine();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                SelectNextMachine();
            }

            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                SelectPreviousMachine();
            }

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
                    conditionAmount,
                    "Developer test"
                );
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                machineSystem.Repair(
                    machineId,
                    conditionAmount,
                    "Developer test"
                );
            }
        }

        private void SelectNextMachine()
        {
            if (machines == null || machines.Count == 0)
                return;

            selectedIndex++;

            if (selectedIndex >= machines.Count)
                selectedIndex = 0;

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
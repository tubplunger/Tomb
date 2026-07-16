using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Services;

namespace Tomb.Gameplay.Machines
{
    public sealed class DEV_MachineControls : MonoBehaviour
    {
        [Header("Test Machine")]
        [SerializeField]
        private string machineId = "oxygen_generator";

        [Header("Condition Change")]
        [Min(0.01f)]
        [SerializeField]
        private float conditionAmount = 20f;

        private MachineSystem machineSystem;

        private void Start()
        {
            machineSystem =
                CoreServices.Get<MachineSystem>();

            Debug.Log(
                $"[DEV Machines] Controls initialized for: " +
                $"{machineId}"
            );
        }

        private void Update()
        {
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
    }
}
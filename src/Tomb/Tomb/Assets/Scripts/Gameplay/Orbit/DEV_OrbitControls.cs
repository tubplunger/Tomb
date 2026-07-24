using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Services;

namespace Tomb.Gameplay.Orbit
{
    public sealed class DEV_OrbitControls : MonoBehaviour
    {
        [Header("Manual Orbit Step")]
        [Range(0.01f, 0.5f)]
        [SerializeField]
        private float progressStep = 0.1f;

        private OrbitSystem orbitSystem;

        private void Start()
        {
            orbitSystem =
                CoreServices.Get<OrbitSystem>();

            Debug.Log(
                "[DEV Orbit] Controls initialized."
            );
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                orbitSystem.AdvanceByNormalizedAmount(
                    -progressStep,
                    "Developer orbit control"
                );
            }

            if (Input.GetKeyDown(KeyCode.Period))
            {
                orbitSystem.AdvanceByNormalizedAmount(
                    progressStep,
                    "Developer orbit control"
                );
            }

            if (Input.GetKeyDown(KeyCode.Slash))
            {
                orbitSystem.SetOrbitProgress(
                    0f,
                    "Developer orbit reset"
                );
            }
        }
    }
}
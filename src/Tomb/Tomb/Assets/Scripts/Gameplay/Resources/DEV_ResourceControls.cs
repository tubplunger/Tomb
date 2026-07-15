using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Services;

namespace Tomb.Gameplay.Resources
{
    public sealed class DEV_ResourceControls : MonoBehaviour
    {
        [Header("Test Resource")]
        [SerializeField]
        private string resourceId = "oxygen";

        [Header("Test Amount")]
        [Min(0.01f)]
        [SerializeField]
        private float changeAmount = 10f;

        private ResourceSystem resourceSystem;

        private void Start()
        {
            resourceSystem =
                CoreServices.Get<ResourceSystem>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                Debug.Log("[DEV Resources] Consuming resource.");

                resourceSystem.Consume(
                    resourceId,
                    changeAmount,
                    "Developer test"
                );
            }

            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                Debug.Log("[DEV Resources] Adding resource.");

                resourceSystem.Add(
                    resourceId,
                    changeAmount,
                    "Developer test"
                );
            }

            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                Debug.Log("[DEV Resources] Emptying resource.");

                resourceSystem.SetAmount(
                    resourceId,
                    0f,
                    "Developer test"
                );
            }
        }
    }
}
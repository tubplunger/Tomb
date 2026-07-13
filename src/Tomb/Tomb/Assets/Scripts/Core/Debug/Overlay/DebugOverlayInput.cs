using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Services;

namespace Tomb.Core.Debugging.Overlay
{
    public sealed class DebugOverlayInput : MonoBehaviour
    {
        private DebugOverlaySystem overlaySystem;

        private void Start ()
        {
            overlaySystem = CoreServices.Get<DebugOverlaySystem>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                overlaySystem.Toggle();
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Services;
using Tomb.Gameplay.Radio.Responses;

namespace Tomb.Presentation.Radio
{
    public sealed class RadioResponseRunner :
        MonoBehaviour
    {
        private RadioResponseSystem
            responseSystem;

        private void Start()
        {
            responseSystem =
                CoreServices.Get<
                    RadioResponseSystem>();
        }

        private void Update()
        {
            responseSystem?.Tick(
                Time.deltaTime
            );
        }
    }
}
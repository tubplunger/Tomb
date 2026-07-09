using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;
using Tomb.Core.Services;

namespace Tomb.Core.Save
{
    public sealed class DEV_SaveControls : MonoBehaviour
    {
        private EventBus eventBus;

        private void Start()
        {
            eventBus = CoreServices.Get<EventBus>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                eventBus.Publish(new SaveRequestedEvent());
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                eventBus.Publish(new LoadRequestedEvent());
            }
        }
    }
}
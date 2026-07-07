using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;
using Tomb.Core.Services;
using Tomb.Core.Debugging;

namespace Tomb.Core.Bootstrap
{
    public sealed class ProjectInitializedListener : MonoBehaviour
    {
        private EventBus eventBus;
        private DebugLogger debugLogger;

        private void Start()
        {
            eventBus = CoreServices.Get<EventBus>();
            debugLogger = CoreServices.Get<DebugLogger>();

            eventBus.Subscribe<ProjectInitializedEvent>(OnProjectInitialized);

            debugLogger.Log("ProjectInitializedListener subscribed.", "Bootstrap Test");
        }

        private void OnDestroy()
        {
            eventBus?.Unsubscribe<ProjectInitializedEvent>(OnProjectInitialized);
        }

        private void OnProjectInitialized(ProjectInitializedEvent initializedEvent)
        {
            debugLogger.Log(initializedEvent.Message, "Bootstrap Test");
        }
    }
}

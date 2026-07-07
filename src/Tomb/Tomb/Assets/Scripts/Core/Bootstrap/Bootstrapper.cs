using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;
using Tomb.Core.Services;
using Tomb.Core.Debugging;

namespace Tomb.Core.Bootstrap
{
    public sealed class Bootstrapper : MonoBehaviour
    {
        private ServiceRegistry serviceRegistry;
        private EventBus eventBus;
        private DebugLogger debugLogger;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            InitializeServices();
            PublishStartupEvent();
        }

        private void InitializeServices()
        {
            serviceRegistry = new ServiceRegistry();
            CoreServices.Initialize(serviceRegistry);

            eventBus = new EventBus();
            serviceRegistry.Register(eventBus);

            debugLogger = new DebugLogger(eventBus);
            serviceRegistry.Register(debugLogger);

            debugLogger.Log("Core services initialized.", "Bootstrap");
        }

        private void PublishStartupEvent()
        {
            eventBus.Publish(new ProjectInitializedEvent("Tomb project foundation initialized."));
            debugLogger.Log("Project initialized event published.", "Bootstrap");
        }

        private void OnDestroy()
        {
            debugLogger?.Dispose();
            eventBus?.Clear();
            serviceRegistry?.Clear();
        }
    }
}

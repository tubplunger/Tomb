using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;
using Tomb.Core.Services;
using Tomb.Core.Debugging;
using Tomb.Core.Time;
using Tomb.Core.Save;

namespace Tomb.Core.Bootstrap
{
    public sealed class Bootstrapper : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private TimeSettings timeSettings;

        private ServiceRegistry serviceRegistry;
        private EventBus eventBus;
        private DebugLogger debugLogger;
        private GameTimeSystem gameTimeSystem;
        private SaveSystem saveSystem;

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

            if (timeSettings == null)
            {
                UnityEngine.Debug.LogError("[Bootstrap] Missing TimeSettings asset.");
                return;
            }

            gameTimeSystem = new GameTimeSystem(eventBus, debugLogger, timeSettings);
            serviceRegistry.Register(gameTimeSystem);

            saveSystem = new SaveSystem(eventBus, debugLogger);
            serviceRegistry.Register(saveSystem);

            saveSystem.Register(gameTimeSystem);

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
            saveSystem?.Dispose();
        }
    }
}
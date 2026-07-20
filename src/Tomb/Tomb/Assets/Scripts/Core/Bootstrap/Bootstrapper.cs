using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;
using Tomb.Core.Services;
using Tomb.Core.Debugging;
using Tomb.Core.Time;
using Tomb.Core.Save;
using Tomb.Core.Debugging.Overlay;
using Tomb.Core.Debugging.Timeline;
using Tomb.Gameplay.Resources;
using Tomb.Gameplay.Machines;
using Tomb.Gameplay.Power;

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
        private DebugOverlaySystem debugOverlaySystem;
        private EventTimelineSystem eventTimelineSystem;
        private static Bootstrapper instance;

        [SerializeField]
        private ResourceCatalog resourceCatalog;

        private ResourceSystem resourceSystem;

        [SerializeField]
        private MachineCatalog machineCatalog;

        private MachineSystem machineSystem;

        [SerializeField]
        private SurvivalConsumptionProfile survivalConsumptionProfile;

        private MachineProcessingSystem machineProcessingSystem;
        private SurvivalConsumptionSystem survivalConsumptionSystem;

        [SerializeField]
        private PowerSettings powerSettings;

        private PowerSystem powerSystem;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

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

            eventTimelineSystem = new EventTimelineSystem(eventBus);
            serviceRegistry.Register(eventTimelineSystem);

            debugLogger = new DebugLogger(eventBus);
            serviceRegistry.Register(debugLogger);

            debugOverlaySystem = new DebugOverlaySystem(eventBus);
            serviceRegistry.Register(debugOverlaySystem);

            if (timeSettings == null)
            {
                UnityEngine.Debug.LogError("[Bootstrap] Missing TimeSettings asset.");
                return;
            }

            if (resourceCatalog == null)
            {
                UnityEngine.Debug.LogError(
                    "[Bootstrap] Missing ResourceCatalog asset."
                );

                return;
            }

            resourceSystem = new ResourceSystem(
                eventBus,
                debugLogger,
                resourceCatalog
            );

            serviceRegistry.Register(resourceSystem);

            if (machineCatalog == null)
            {
                UnityEngine.Debug.LogError(
                    "[Bootstrap] Missing MachineCatalog asset."
                );

                return;
            }

            machineSystem = new MachineSystem(
                eventBus,
                debugLogger,
                machineCatalog
            );

            serviceRegistry.Register(machineSystem);

            if (powerSettings == null)
            {
                UnityEngine.Debug.LogError(
                    "[Bootstrap] Missing PowerSettings asset."
                );

                return;
            }

            powerSystem = new PowerSystem(
                eventBus,
                debugLogger,
                machineSystem,
                powerSettings
            );

            serviceRegistry.Register(powerSystem);

            if (survivalConsumptionProfile == null)
            {
                UnityEngine.Debug.LogError(
                    "[Bootstrap] Missing SurvivalConsumptionProfile asset."
                );

                return;
            }

            machineProcessingSystem =
                new MachineProcessingSystem(
                    eventBus,
                    debugLogger,
                    machineSystem,
                    resourceSystem
                );

            serviceRegistry.Register(machineProcessingSystem);

            survivalConsumptionSystem =
                new SurvivalConsumptionSystem(
                    eventBus,
                    debugLogger,
                    resourceSystem,
                    survivalConsumptionProfile
                );

            serviceRegistry.Register(survivalConsumptionSystem);

            gameTimeSystem = new GameTimeSystem(eventBus, debugLogger, timeSettings);
            serviceRegistry.Register(gameTimeSystem);

            saveSystem = new SaveSystem(eventBus, debugLogger);
            serviceRegistry.Register(saveSystem);

            saveSystem.Register(gameTimeSystem);
            saveSystem.Register(resourceSystem);
            saveSystem.Register(machineSystem);
            saveSystem.Register(powerSystem);
            saveSystem.Register(machineProcessingSystem);

            debugLogger.Log("Core services initialized.", "Bootstrap");
        }

        private void PublishStartupEvent()
        {
            eventBus.Publish(new ProjectInitializedEvent("Tomb project foundation initialized."));
            debugLogger.Log("Project initialized event published.", "Bootstrap");
        }

        private void OnDestroy()
        {
            if (instance != this)
                return;

            saveSystem?.Dispose();
            debugLogger?.Dispose();
            eventTimelineSystem?.Dispose();
            machineProcessingSystem?.Dispose();
            powerSystem?.Dispose();
            survivalConsumptionSystem?.Dispose();

            eventBus?.Clear();
            serviceRegistry?.Clear();

            instance = null;
        }
    }
}
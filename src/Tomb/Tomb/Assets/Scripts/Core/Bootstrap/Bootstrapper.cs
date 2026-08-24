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
using Tomb.Gameplay.Orbit;
using Tomb.Gameplay.Earth;
using Tomb.Gameplay.Radio;
using Tomb.Gameplay.Radio.Broadcasts;
using Tomb.Gameplay.Story;

namespace Tomb.Core.Bootstrap
{
    public sealed class Bootstrapper : MonoBehaviour
    {
        //wah

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

        private MachineMaintenanceSystem machineMaintenanceSystem;

        [SerializeField]
        private OrbitSettings orbitSettings;

        private OrbitSystem orbitSystem;

        [SerializeField]
        private OrbitLightingSettings orbitLightingSettings;

        [SerializeField]
        private SolarOrbitIntegrationSettings
            solarOrbitIntegrationSettings;

        private OrbitLightingSystem orbitLightingSystem;
        private SolarPowerOrbitIntegration solarPowerOrbitIntegration;

        [Header("Earth Regions")]
        [SerializeField]
        private EarthRegionCatalog earthRegionCatalog;

        [SerializeField]
        private EarthRegionVisibilitySettings
            earthRegionVisibilitySettings;

        [Header("Radio")]
        [SerializeField]
        private RadioSignalCatalog radioSignalCatalog;

        private EarthRegionSystem earthRegionSystem;
        private RadioVisibilitySystem radioVisibilitySystem;

        [SerializeField]
        private RadioReceiverSettings radioReceiverSettings;
        private RadioReceiverSystem radioReceiverSystem;

        [Header("Broadcasts")]
        [SerializeField]
        private BroadcastCatalog broadcastCatalog;

        private StoryFlagSystem storyFlagSystem;
        private BroadcastLibrarySystem broadcastLibrarySystem;

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

            storyFlagSystem =
                new StoryFlagSystem(
                    eventBus,
                    debugLogger
                );

            serviceRegistry.Register(storyFlagSystem);

            debugOverlaySystem = new DebugOverlaySystem(eventBus);
            serviceRegistry.Register(debugOverlaySystem);

            if (timeSettings == null)
            {
                UnityEngine.Debug.LogError("[Bootstrap] Missing TimeSettings asset.");
                return;
            }

            gameTimeSystem = new GameTimeSystem(eventBus, debugLogger, timeSettings);
            serviceRegistry.Register(gameTimeSystem);

            if (orbitSettings == null)
            {
                UnityEngine.Debug.LogError(
                    "[Bootstrap] Missing OrbitSettings asset."
                );

                return;
            }

            orbitSystem = new OrbitSystem(
                eventBus,
                debugLogger,
                orbitSettings
            );

            serviceRegistry.Register(orbitSystem);

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

            machineMaintenanceSystem =
                new MachineMaintenanceSystem(
                    eventBus,
                    debugLogger,
                    machineSystem,
                    resourceSystem
                );

            serviceRegistry.Register(machineMaintenanceSystem);

            if (orbitLightingSettings == null)
            {
                UnityEngine.Debug.LogError(
                    "[Bootstrap] Missing OrbitLightingSettings."
                );

                return;
            }

            orbitLightingSystem =
                new OrbitLightingSystem(
                    eventBus,
                    debugLogger,
                    orbitSystem,
                    orbitLightingSettings
                );

            serviceRegistry.Register(
                orbitLightingSystem
            );

            if (solarOrbitIntegrationSettings == null)
            {
                UnityEngine.Debug.LogError(
                    "[Bootstrap] Missing " +
                    "SolarOrbitIntegrationSettings."
                );

                return;
            }

            solarPowerOrbitIntegration =
                new SolarPowerOrbitIntegration(
                    eventBus,
                    debugLogger,
                    orbitLightingSystem,
                    powerSystem,
                    solarOrbitIntegrationSettings
                );

            serviceRegistry.Register(
                solarPowerOrbitIntegration
            );

            earthRegionSystem =
                new EarthRegionSystem(
                    eventBus,
                    debugLogger,
                    orbitSystem,
                    earthRegionCatalog,
                    earthRegionVisibilitySettings
                );

            serviceRegistry.Register(
                earthRegionSystem
            );

            radioVisibilitySystem =
                new RadioVisibilitySystem(
                    eventBus,
                    debugLogger,
                    earthRegionSystem,
                    orbitLightingSystem,
                    radioSignalCatalog
                );

            serviceRegistry.Register(
                radioVisibilitySystem
            );

            if (radioReceiverSettings == null)
            {
                UnityEngine.Debug.LogError(
                    "[Bootstrap] Missing RadioReceiverSettings."
                );

                return;
            }

            radioReceiverSystem =
                new RadioReceiverSystem(
                    eventBus,
                    debugLogger,
                    radioVisibilitySystem,
                    machineSystem,
                    radioSignalCatalog,
                    radioReceiverSettings
                );

            serviceRegistry.Register(
                radioReceiverSystem
            );

            survivalConsumptionSystem =
                new SurvivalConsumptionSystem(
                    eventBus,
                    debugLogger,
                    resourceSystem,
                    survivalConsumptionProfile
                );

            serviceRegistry.Register(survivalConsumptionSystem);

            if (broadcastCatalog == null)
            {
                UnityEngine.Debug.LogError(
                    "[Bootstrap] Missing BroadcastCatalog."
                );

                return;
            }

            broadcastLibrarySystem =
                new BroadcastLibrarySystem(
                    eventBus,
                    debugLogger,
                    gameTimeSystem,
                    radioReceiverSystem,
                    storyFlagSystem,
                    broadcastCatalog
                );

            serviceRegistry.Register(
                broadcastLibrarySystem
            );

            saveSystem = new SaveSystem(eventBus, debugLogger);
            serviceRegistry.Register(saveSystem);

            saveSystem.Register(gameTimeSystem);
            saveSystem.Register(orbitSystem);
            saveSystem.Register(resourceSystem);
            saveSystem.Register(machineSystem);
            saveSystem.Register(powerSystem);
            saveSystem.Register(machineProcessingSystem);
            saveSystem.Register(machineMaintenanceSystem);
            saveSystem.Register(storyFlagSystem);
            saveSystem.Register(broadcastLibrarySystem);
            saveSystem.Register(radioReceiverSystem);

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
            machineMaintenanceSystem?.Dispose();
            machineProcessingSystem?.Dispose();
            powerSystem?.Dispose();
            survivalConsumptionSystem?.Dispose();
            solarPowerOrbitIntegration?.Dispose();
            broadcastLibrarySystem?.Dispose();
            radioReceiverSystem?.Dispose();
            radioVisibilitySystem?.Dispose();
            earthRegionSystem?.Dispose();
            orbitLightingSystem?.Dispose();
            orbitSystem?.Dispose();

            eventBus?.Clear();
            serviceRegistry?.Clear();

            instance = null;
        }
    }
}
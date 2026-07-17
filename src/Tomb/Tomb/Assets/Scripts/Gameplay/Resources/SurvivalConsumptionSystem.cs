using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Time;

namespace Tomb.Gameplay.Resources
{
    public sealed class SurvivalConsumptionSystem
    {
        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;
        private readonly ResourceSystem resourceSystem;
        private readonly SurvivalConsumptionProfile profile;

        public SurvivalConsumptionSystem(
            EventBus eventBus,
            DebugLogger debugLogger,
            ResourceSystem resourceSystem,
            SurvivalConsumptionProfile profile)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;
            this.resourceSystem = resourceSystem;
            this.profile = profile;

            this.eventBus.Subscribe<GameHourPassedEvent>(
                OnGameHourPassed
            );

            debugLogger.Log(
                "Survival consumption system initialized.",
                "Resources"
            );
        }

        private void OnGameHourPassed(
            GameHourPassedEvent hourEvent)
        {
            foreach (ResourceAmount consumption
                     in profile.HourlyConsumption)
            {
                if (consumption == null ||
                    consumption.Resource == null)
                {
                    continue;
                }

                resourceSystem.Consume(
                    consumption.ResourceId,
                    consumption.Amount,
                    "Astronaut survival consumption"
                );
            }

            resourceSystem.AddAll(
                profile.HourlyOutputs,
                1f,
                "Astronaut waste production"
            );

            eventBus.Publish(
                new SurvivalConsumptionProcessedEvent(
                    hourEvent.CurrentTime.Day,
                    hourEvent.CurrentTime.Hour
                )
            );
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<GameHourPassedEvent>(
                OnGameHourPassed
            );
        }
    }
}
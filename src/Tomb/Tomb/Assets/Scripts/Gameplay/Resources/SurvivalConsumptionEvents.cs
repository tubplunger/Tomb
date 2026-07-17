using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Resources
{
    public readonly struct SurvivalConsumptionProcessedEvent
        : IGameEvent
    {
        public readonly int Day;
        public readonly int Hour;

        public SurvivalConsumptionProcessedEvent(
            int day,
            int hour)
        {
            Day = day;
            Hour = hour;
        }
    }
}
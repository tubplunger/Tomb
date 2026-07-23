using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Station
{
    public readonly struct StationTestStepEvent : IGameEvent
    {
        public readonly string StepName;
        public readonly string Description;

        public StationTestStepEvent(
            string stepName,
            string description)
        {
            StepName = stepName;
            Description = description;
        }
    }
}
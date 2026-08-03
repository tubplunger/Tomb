using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Orbit
{
    public readonly struct Phase3TestStepEvent :
        IGameEvent
    {
        public readonly string StepName;
        public readonly string Description;

        public Phase3TestStepEvent(
            string stepName,
            string description)
        {
            StepName = stepName;
            Description = description;
        }
    }
}
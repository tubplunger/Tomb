using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Gameplay.Orbit
{
    [Serializable]
    public readonly struct OrbitLightingSnapshot
    {
        public readonly OrbitLightingState State;
        public readonly float OrbitProgress;
        public readonly float GenerationMultiplier;
        public readonly float MinutesUntilTransition;
        public readonly OrbitLightingState NextState;

        public bool IsInSunlight =>
            State == OrbitLightingState.Sunlight;

        public bool IsInEclipse =>
            State == OrbitLightingState.Eclipse;

        public OrbitLightingSnapshot(
            OrbitLightingState state,
            float orbitProgress,
            float generationMultiplier,
            float minutesUntilTransition,
            OrbitLightingState nextState)
        {
            State = state;
            OrbitProgress = orbitProgress;
            GenerationMultiplier = generationMultiplier;
            MinutesUntilTransition = minutesUntilTransition;
            NextState = nextState;
        }

        public override string ToString()
        {
            return
                $"{State} | " +
                $"Solar {GenerationMultiplier * 100f:0}% | " +
                $"{MinutesUntilTransition:0.0}m until {NextState}";
        }
    }
}
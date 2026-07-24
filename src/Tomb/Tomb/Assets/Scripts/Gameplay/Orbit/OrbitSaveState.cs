using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Gameplay.Orbit
{
    [Serializable]
    public sealed class OrbitSaveState
    {
        public float orbitProgress;
        public int completedOrbits;
    }
}
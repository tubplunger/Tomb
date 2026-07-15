using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Gameplay.Resources
{
    [Serializable]
    public sealed class ResourceSaveState
    {
        public List<ResourceSaveEntry> resources = new();
    }

    [Serializable]
    public sealed class ResourceSaveEntry
    {
        public string resourceId;
        public float currentAmount;
    }
}
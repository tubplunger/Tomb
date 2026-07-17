using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Gameplay.Resources
{
    [Serializable]
    public sealed class ResourceAmount
    {
        [SerializeField]
        private ResourceDefinition resource;

        [Min(0f)]
        [SerializeField]
        private float amount;

        public ResourceDefinition Resource => resource;
        public float Amount => amount;

        public string ResourceId =>
            resource != null
                ? resource.ResourceId
                : string.Empty;
    }
}
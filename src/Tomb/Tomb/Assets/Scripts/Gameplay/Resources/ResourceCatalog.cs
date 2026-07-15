using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Resources
{
    [CreateAssetMenu(
        fileName = "ResourceCatalog",
        menuName = "Tomb/Resources/Recourse Catalog"
        )]
    public sealed class ResourceCatalog : ScriptableObject
    {
        [SerializeField]
        private List<ResourceDefinition> resources = new();

        public IReadOnlyList<ResourceDefinition> Resources =>
            resources;
    }
}

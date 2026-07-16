using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Machines
{
    [CreateAssetMenu(
        fileName = "MachineCatalog",
        menuName = "Tomb/Machines/Machine Catalog"
        )]
    public sealed class MachineCatalog : ScriptableObject
    {
        [SerializeField]
        private List<MachineDefinition> machines = new();

        public IReadOnlyList<MachineDefinition> Machines => machines;
    }
}
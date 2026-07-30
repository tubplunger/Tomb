using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Radio
{
    [CreateAssetMenu(
        fileName = "RadioSignalCatalog",
        menuName = "Tomb/Radio/Radio Signal Catalog"
    )]
    public sealed class RadioSignalCatalog :
        ScriptableObject
    {
        [SerializeField]
        private List<RadioSignalDefinition> signals =
            new();

        public IReadOnlyList<RadioSignalDefinition>
            Signals => signals;
    }
}
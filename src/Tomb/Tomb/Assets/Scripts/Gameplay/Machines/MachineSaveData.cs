using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Gameplay.Machines
{
    [Serializable]
    public sealed class MachineSaveState
    {
        public List<MachineSaveEntry> machines = new();
    }

    [Serializable]
    public sealed class MachineSaveEntry
    {
        public string machineId;
        public bool isEnabled;
        public float currentCondition;
    }
}
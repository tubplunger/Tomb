using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Gameplay.Machines
{
    [Serializable]
    public sealed class MachineProcessingSaveState
    {
        public List<MachineProcessingSaveEntry> machines = new();
    }

    [Serializable]
    public sealed class MachineProcessingSaveEntry
    {
        public string machineId;
        public int elapsedGameMinutes;
    }
}
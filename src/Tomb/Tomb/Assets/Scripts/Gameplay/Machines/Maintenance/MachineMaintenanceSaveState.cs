using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Gameplay.Machines
{
    [Serializable]
    public sealed class MachineMaintenanceSaveState
    {
        public List<MachineMaintenanceSaveEntry> machines = new();
    }

    [Serializable]
    public sealed class MachineMaintenanceSaveEntry
    {
        public string machineId;
        public int elapsedGameMinutes;
        public bool criticalWarningActive;
    }
}
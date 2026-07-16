using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Machines
{
    public enum MachineCategory
    {
        LifeSupport,
        Environmental,
        Communications,
        Utilities,
        Power,
        Other
    }

    public enum MachinePriority
    {
        Critical,
        Essential,
        Normal,
        Operational
    }

    public enum MachineStatus
    {
        Operational,
        Disabled,
        Broken,
        NoPower,
        MissingInputs,
        Maintenance
    }
}
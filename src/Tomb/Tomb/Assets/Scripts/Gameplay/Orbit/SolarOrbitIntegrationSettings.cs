using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Orbit
{
    [CreateAssetMenu(
        fileName = "SolarOrbitIntegrationSettings",
        menuName = "Tomb/Orbit/Solar Power Integration Settings"
    )]
    public sealed class SolarOrbitIntegrationSettings :
        ScriptableObject
    {
        [SerializeField]
        private string solarArrayMachineId =
            "solar_array";

        public string SolarArrayMachineId =>
            solarArrayMachineId;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Tomb.Gameplay.Resources;

namespace Tomb.Gameplay.Machines
{
    [CreateAssetMenu(
        fileName = "MachineMaintenanceProfile",
        menuName = "Tomb/Machines/Machine Maintenance Profile"
    )]
    public sealed class MachineMaintenanceProfile : ScriptableObject
    {
        [Header("Degradation")]
        [Min(1)]
        [SerializeField]
        private int degradationIntervalGameMinutes = 60;

        [Min(0f)]
        [SerializeField]
        private float degradationPerInterval = 1f;

        [SerializeField]
        private bool degradeWhileDisabled = true;

        [Header("Warnings")]
        [Range(0.01f, 1f)]
        [SerializeField]
        private float criticalConditionNormalized = 0.25f;

        [Header("Repair")]
        [Min(0.01f)]
        [SerializeField]
        private float conditionRestoredPerRepair = 20f;

        [SerializeField]
        private List<ResourceAmount> repairCosts = new();

        public int DegradationIntervalGameMinutes =>
            Mathf.Max(1, degradationIntervalGameMinutes);

        public float DegradationPerInterval =>
            Mathf.Max(0f, degradationPerInterval);

        public bool DegradeWhileDisabled =>
            degradeWhileDisabled;

        public float CriticalConditionNormalized =>
            criticalConditionNormalized;

        public float ConditionRestoredPerRepair =>
            conditionRestoredPerRepair;

        public IReadOnlyList<ResourceAmount> RepairCosts =>
            repairCosts;

        public string GetRepairCostSummary()
        {
            if (repairCosts == null || repairCosts.Count == 0)
                return "No repair cost";

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < repairCosts.Count; i++)
            {
                ResourceAmount cost = repairCosts[i];

                if (cost == null || cost.Resource == null)
                    continue;

                if (builder.Length > 0)
                    builder.Append(", ");

                builder.Append(cost.Amount.ToString("0.##"));
                builder.Append(' ');
                builder.Append(cost.Resource.DisplayName);
            }

            return builder.Length > 0
                ? builder.ToString()
                : "No repair cost";
        }
    }
}
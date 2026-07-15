using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Resources
{
    [CreateAssetMenu(
        fileName = "ResourceDefinition",
        menuName = "Tomb/Resources/Resource Definition"
        )]
    public sealed class ResourceDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string resourceId;
        [SerializeField] private string displayName;
        [SerializeField] private string unitLabel;

        [Header("Starting Values")]
        [Min(0f)]
        [SerializeField] private float startingAmount;

        [Min(0.01f)]
        [SerializeField] private float maximumAmount = 100f;

        [Header("Warnings")]
        [Range(0f, 1f)]
        [SerializeField] private float lowThresholdNormalized = 0.25f;

        public string ResourceId => resourceId;
        public string DisplayName => displayName;
        public string UnitLabel => unitLabel;

        public float StartingAmount =>
            Mathf.Clamp(startingAmount, 0f, maximumAmount);

        public float MaximumAmount => maximumAmount;

        public float LowThresholdNormalized =>
            lowThresholdNormalized;

        public float LowThresholdAmount =>
            maximumAmount * lowThresholdNormalized;

        private void OnValidate()
        {
            maximumAmount = Mathf.Max(0.01f, maximumAmount);
            startingAmount = Mathf.Clamp(
                startingAmount,
                0f,
                maximumAmount
            );

            resourceId = resourceId?.Trim();
            displayName = displayName?.Trim();
            unitLabel = unitLabel?.Trim();
        }
    }
}

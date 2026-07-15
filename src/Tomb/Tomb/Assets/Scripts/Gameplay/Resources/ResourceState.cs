using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Resources
{
    public sealed class ResourceState
    {
        public ResourceDefinition Definition { get; }

        public float CurrentAmount { get; private set; }

        public float MaximumAmount =>
            Definition.MaximumAmount;

        public float NormalizedAmount =>
            MaximumAmount <= 0f
                ? 0f
                : CurrentAmount / MaximumAmount;

        public bool IsEmpty =>
            CurrentAmount <= 0f;

        public bool IsLow =>
            CurrentAmount > 0f &&
            CurrentAmount <= Definition.LowThresholdAmount;

        public ResourceState(ResourceDefinition definition)
        {
            Definition = definition;
            CurrentAmount = definition.StartingAmount;
        }

        public float SetAmount(float newAmount)
        {
            float previousAmount = CurrentAmount;

            CurrentAmount = Mathf.Clamp(
                newAmount,
                0f,
                MaximumAmount
            );

            return CurrentAmount - previousAmount;
        }

        public float Add(float amount)
        {
            if (amount <= 0f)
                return 0f;

            return SetAmount(CurrentAmount + amount);
        }

        public float Consume(float amount)
        {
            if (amount <= 0f)
                return 0f;

            float previousAmount = CurrentAmount;

            CurrentAmount = Mathf.Max(
                0f,
                CurrentAmount - amount
            );

            return previousAmount - CurrentAmount;
        }

        public bool HasAtLeast(float amount)
        {
            return amount <= 0f ||
                   CurrentAmount >= amount;
        }
    }
}
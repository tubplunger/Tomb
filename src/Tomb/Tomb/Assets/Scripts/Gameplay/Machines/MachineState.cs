using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Machines
{
    public sealed class MachineState
    {
        public MachineDefinition Definition { get; }

        public bool IsEnabled { get; private set; }

        public float CurrentCondition { get; private set; }

        public float MaximumCondition =>
            Definition.MaximumCondition;

        public float NormalizedCondition =>
            MaximumCondition <= 0f
                ? 0f
                : CurrentCondition / MaximumCondition;

        public float Efficiency =>
            Mathf.Clamp01(NormalizedCondition);

        public bool IsBroken =>
            CurrentCondition <= 0f;

        public MachineStatus Status
        {
            get
            {
                if (IsBroken)
                    return MachineStatus.Broken;

                if (!IsEnabled)
                    return MachineStatus.Disabled;

                return MachineStatus.Operational;
            }
        }

        public MachineState(MachineDefinition definition)
        {
            Definition = definition;
            IsEnabled = definition.StartingEnabled;
            CurrentCondition = definition.StartingCondition;
        }

        public bool SetEnabled(bool enabled)
        {
            if (!Definition.CanBeDisabled && !enabled)
                return false;

            if (IsEnabled == enabled)
                return false;

            IsEnabled = enabled;
            return true;
        }

        public float SetCondition(float condition)
        {
            float previousCondition = CurrentCondition;

            CurrentCondition = Mathf.Clamp(
                condition,
                0f,
                MaximumCondition
            );

            return CurrentCondition - previousCondition;
        }

        public float Damage(float amount)
        {
            if (amount <= 0f)
                return 0f;

            float previousCondition = CurrentCondition;

            CurrentCondition = Mathf.Max(
                0f,
                CurrentCondition - amount
            );

            return previousCondition - CurrentCondition;
        }

        public float Repair(float amount)
        {
            if (amount <= 0f)
                return 0f;

            float previousCondition = CurrentCondition;

            CurrentCondition = Mathf.Min(
                MaximumCondition,
                CurrentCondition + amount
            );

            return CurrentCondition - previousCondition;
        }
    }
}
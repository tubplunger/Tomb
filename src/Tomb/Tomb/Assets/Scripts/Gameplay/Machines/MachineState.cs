using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Machines
{
    public sealed class MachineState
    {
        private bool hasPower = true;
        private bool hasRequiredInputs = true;
        private bool isInMaintenance;

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

        public bool HasPower => hasPower;

        public bool HasRequiredInputs => hasRequiredInputs;

        public bool IsInMaintenance => isInMaintenance;

        public MachineStatus Status
        {
            get
            {
                if (IsBroken)
                    return MachineStatus.Broken;

                if (!IsEnabled)
                    return MachineStatus.Disabled;

                if (isInMaintenance)
                    return MachineStatus.Maintenance;

                if (!hasPower)
                    return MachineStatus.NoPower;

                if (!hasRequiredInputs)
                    return MachineStatus.MissingInputs;

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

        public bool SetPowerAvailable(bool available)
        {
            if (hasPower == available)
                return false;

            hasPower = available;
            return true;
        }

        public bool SetInputsAvailable(bool available)
        {
            if (hasRequiredInputs == available)
                return false;

            hasRequiredInputs = available;
            return true;
        }

        public bool SetMaintenance(bool maintenance)
        {
            if (isInMaintenance == maintenance)
                return false;

            isInMaintenance = maintenance;
            return true;
        }
    }
}
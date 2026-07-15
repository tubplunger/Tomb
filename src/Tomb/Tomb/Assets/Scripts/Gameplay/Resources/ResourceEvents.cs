using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Resources
{
    public readonly struct ResourceChangedEvent : IGameEvent
    {
        public readonly string ResourceId;
        public readonly float PreviousAmount;
        public readonly float CurrentAmount;
        public readonly float MaximumAmount;
        public readonly float ChangeAmount;
        public readonly string Reason;

        public ResourceChangedEvent(
            string resourceId,
            float previousAmount,
            float currentAmount,
            float maximumAmount,
            float changeAmount,
            string reason)
        {
            ResourceId = resourceId;
            PreviousAmount = previousAmount;
            CurrentAmount = currentAmount;
            MaximumAmount = maximumAmount;
            ChangeAmount = changeAmount;
            Reason = reason;
        }
    }

    public readonly struct ResourceLowEvent : IGameEvent
    {
        public readonly string ResourceId;
        public readonly float CurrentAmount;
        public readonly float MaximumAmount;

        public ResourceLowEvent(
            string resourceId,
            float currentAmount,
            float maximumAmount)
        {
            ResourceId = resourceId;
            CurrentAmount = currentAmount;
            MaximumAmount = maximumAmount;
        }
    }

    public readonly struct ResourceRecoveredEvent : IGameEvent
    {
        public readonly string ResourceId;
        public readonly float CurrentAmount;
        public readonly float MaximumAmount;

        public ResourceRecoveredEvent(
            string resourceId,
            float currentAmount,
            float maximumAmount)
        {
            ResourceId = resourceId;
            CurrentAmount = currentAmount;
            MaximumAmount = maximumAmount;
        }
    }

    public readonly struct ResourceEmptyEvent : IGameEvent
    {
        public readonly string ResourceId;

        public ResourceEmptyEvent(string resourceId)
        {
            ResourceId = resourceId;
        }
    }

    public readonly struct ResourceRestoredFromSaveEvent : IGameEvent
    {
        public readonly int RestoredResourceCount;

        public ResourceRestoredFromSaveEvent(
            int restoredResourceCount)
        {
            RestoredResourceCount = restoredResourceCount;
        }
    }
}
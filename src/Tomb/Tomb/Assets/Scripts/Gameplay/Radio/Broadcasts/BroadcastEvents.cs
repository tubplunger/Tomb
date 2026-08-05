using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    public readonly struct BroadcastEligibilityUpdatedEvent :
        IGameEvent
    {
        public readonly int EligibleBroadcastCount;

        public BroadcastEligibilityUpdatedEvent(
            int eligibleBroadcastCount)
        {
            EligibleBroadcastCount =
                eligibleBroadcastCount;
        }
    }

    public readonly struct BroadcastBecameEligibleEvent :
        IGameEvent
    {
        public readonly BroadcastDefinition Broadcast;

        public BroadcastBecameEligibleEvent(
            BroadcastDefinition broadcast)
        {
            Broadcast = broadcast;
        }
    }

    public readonly struct BroadcastBecameIneligibleEvent :
        IGameEvent
    {
        public readonly BroadcastDefinition Broadcast;
        public readonly BroadcastRuntimeStatus Status;

        public BroadcastBecameIneligibleEvent(
            BroadcastDefinition broadcast,
            BroadcastRuntimeStatus status)
        {
            Broadcast = broadcast;
            Status = status;
        }
    }

    public readonly struct BroadcastCompletionRecordedEvent :
        IGameEvent
    {
        public readonly BroadcastDefinition Broadcast;
        public readonly int PlayCount;

        public BroadcastCompletionRecordedEvent(
            BroadcastDefinition broadcast,
            int playCount)
        {
            Broadcast = broadcast;
            PlayCount = playCount;
        }
    }
}
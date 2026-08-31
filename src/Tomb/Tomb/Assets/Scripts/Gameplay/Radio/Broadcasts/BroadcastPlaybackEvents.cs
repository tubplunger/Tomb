using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    public readonly struct BroadcastStartedEvent :
        IGameEvent
    {
        public readonly BroadcastDefinition Broadcast;

        public BroadcastStartedEvent(
            BroadcastDefinition broadcast)
        {
            Broadcast = broadcast;
        }
    }

    public readonly struct BroadcastSegmentStartedEvent :
        IGameEvent
    {
        public readonly BroadcastDefinition Broadcast;
        public readonly int SegmentIndex;

        public BroadcastSegmentStartedEvent(
            BroadcastDefinition broadcast,
            int segmentIndex)
        {
            Broadcast = broadcast;
            SegmentIndex = segmentIndex;
        }
    }

    public readonly struct BroadcastInterruptedEvent :
        IGameEvent
    {
        public readonly BroadcastDefinition Broadcast;

        public BroadcastInterruptedEvent(
            BroadcastDefinition broadcast)
        {
            Broadcast = broadcast;
        }
    }

    public readonly struct BroadcastPlaybackCompletedEvent :
        IGameEvent
    {
        public readonly BroadcastDefinition Broadcast;

        public BroadcastPlaybackCompletedEvent(
            BroadcastDefinition broadcast)
        {
            Broadcast = broadcast;
        }
    }
}
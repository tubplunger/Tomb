using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;
using Tomb.Gameplay.Radio.Broadcasts;

namespace Tomb.Gameplay.Radio.Responses
{
    public readonly struct
        RadioResponseWindowOpenedEvent :
        IGameEvent
    {
        public readonly BroadcastDefinition Broadcast;

        public RadioResponseWindowOpenedEvent(
            BroadcastDefinition broadcast)
        {
            Broadcast = broadcast;
        }
    }

    public readonly struct
        RadioResponseSelectedEvent :
        IGameEvent
    {
        public readonly BroadcastDefinition Broadcast;
        public readonly RadioResponseDefinition Response;

        public RadioResponseSelectedEvent(
            BroadcastDefinition broadcast,
            RadioResponseDefinition response)
        {
            Broadcast = broadcast;
            Response = response;
        }
    }

    public readonly struct
        RadioResponseExpiredEvent :
        IGameEvent
    {
        public readonly BroadcastDefinition Broadcast;

        public RadioResponseExpiredEvent(
            BroadcastDefinition broadcast)
        {
            Broadcast = broadcast;
        }
    }
}
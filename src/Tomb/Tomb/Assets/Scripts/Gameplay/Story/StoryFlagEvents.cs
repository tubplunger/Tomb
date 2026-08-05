using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Story
{
    public readonly struct StoryFlagChangedEvent :
        IGameEvent
    {
        public readonly string FlagId;
        public readonly bool IsSet;
        public readonly string Reason;

        public StoryFlagChangedEvent(
            string flagId,
            bool isSet,
            string reason)
        {
            FlagId = flagId;
            IsSet = isSet;
            Reason = reason;
        }
    }
}
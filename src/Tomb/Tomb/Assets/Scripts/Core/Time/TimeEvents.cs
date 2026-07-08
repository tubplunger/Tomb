using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Core.Time
{
    public readonly struct GameMinutePassedEvent : IGameEvent
    {
        public readonly GameTime CurrentTime;

        public GameMinutePassedEvent(GameTime currentTime)
        {
            CurrentTime = currentTime;
        }
    }

    public readonly struct GameHourPassedEvent : IGameEvent
    {
        public readonly GameTime CurrentTime;

        public GameHourPassedEvent(GameTime currentTime)
        {
            CurrentTime = currentTime;
        }
    }

    public readonly struct GameDayPassedEvent : IGameEvent
    {
        public readonly GameTime CurrentTime;

        public GameDayPassedEvent(GameTime currentTime)
        {
            CurrentTime = currentTime;
        }
    }

    public readonly struct GamePausedEvent : IGameEvent
    {
    }

    public readonly struct GameResumedEvent : IGameEvent
    {
    }

    public readonly struct GameTimeScaleChangedEvent : IGameEvent
    {
        public readonly float NewTimeScale;

        public GameTimeScaleChangedEvent(float newTimeScale)
        {
            NewTimeScale = newTimeScale;
        }
    }

    public readonly struct GameTimeTickEvent : IGameEvent
    {
        public readonly GameTime CurrentTime;

        public GameTimeTickEvent(GameTime currentTime)
        {
            CurrentTime = currentTime;
        }
    }
}

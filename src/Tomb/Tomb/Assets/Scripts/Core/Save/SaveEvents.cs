using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Core.Save
{
    public readonly struct SaveRequestedEvent : IGameEvent
    {

    }

    public readonly struct LoadRequestedEvent : IGameEvent
    {

    }

    public readonly struct GameSavedEvent : IGameEvent
    {
        public readonly string FilePath;

        public GameSavedEvent(string filePath)
        {
            FilePath = filePath;
        }
    }

    public readonly struct GameLoadedEvent : IGameEvent
    {
        public readonly string FilePath;

        public GameLoadedEvent(string filePath)
        {
            FilePath = filePath;
        }
    }

    public readonly struct SaveFailedEvent : IGameEvent
    {
        public readonly string Reason;

        public SaveFailedEvent(string reason)
        {
            Reason = reason;
        }
    }

    public readonly struct LoadFailedEvent : IGameEvent
    {
        public readonly string Reason;

        public LoadFailedEvent(string reason)
        {
            Reason = reason;
        }
    }

    public readonly struct AllSaveDataRestoredEvent : IGameEvent
    {
        public readonly string FilePath;

        public AllSaveDataRestoredEvent(string filePath)
        {
            FilePath = filePath;
        }
    }
}

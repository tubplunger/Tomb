using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;

namespace Tomb.Gameplay.Story
{
    public sealed class StoryFlagSystem : ISaveable
    {
        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;

        private readonly HashSet<string>
            activeFlags = new();

        public string SaveKey => "story_flags";

        public Type SaveStateType =>
            typeof(StoryFlagSaveState);

        public IReadOnlyCollection<string> ActiveFlags =>
            activeFlags;

        public StoryFlagSystem(
            EventBus eventBus,
            DebugLogger debugLogger)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;

            debugLogger.Log(
                "Story flag system initialized.",
                "Story"
            );
        }

        public bool HasFlag(string flagId)
        {
            return !string.IsNullOrWhiteSpace(flagId) &&
                   activeFlags.Contains(flagId);
        }

        public bool SetFlag(
            string flagId,
            bool value = true,
            string reason = "Unspecified")
        {
            if (string.IsNullOrWhiteSpace(flagId))
                return false;

            string normalizedId = flagId.Trim();

            bool changed;

            if (value)
            {
                changed =
                    activeFlags.Add(normalizedId);
            }
            else
            {
                changed =
                    activeFlags.Remove(normalizedId);
            }

            if (!changed)
                return false;

            eventBus.Publish(
                new StoryFlagChangedEvent(
                    normalizedId,
                    value,
                    reason
                )
            );

            debugLogger.Log(
                $"Story flag '{normalizedId}' = {value}. " +
                $"Reason: {reason}",
                "Story"
            );

            return true;
        }

        public object CaptureState()
        {
            StoryFlagSaveState saveState =
                new StoryFlagSaveState();

            foreach (string flag in activeFlags)
            {
                saveState.activeFlags.Add(flag);
            }

            return saveState;
        }

        public void RestoreState(object state)
        {
            if (state is not StoryFlagSaveState saveState)
                return;

            activeFlags.Clear();

            foreach (string flag in saveState.activeFlags)
            {
                if (!string.IsNullOrWhiteSpace(flag))
                {
                    activeFlags.Add(flag.Trim());
                }
            }

            debugLogger.Log(
                $"Restored {activeFlags.Count} story flags.",
                "Story"
            );
        }
    }
}
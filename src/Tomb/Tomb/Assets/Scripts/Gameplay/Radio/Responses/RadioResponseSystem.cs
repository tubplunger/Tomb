using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;
using Tomb.Gameplay.Radio.Broadcasts;
using Tomb.Gameplay.Story;

namespace Tomb.Gameplay.Radio.Responses
{
    public sealed class RadioResponseSystem :
        ISaveable
    {
        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;

        private readonly StoryFlagSystem storyFlagSystem;
        private readonly BroadcastLibrarySystem
            broadcastLibrarySystem;

        private BroadcastRuntimeState activeBroadcast;

        private RadioResponseState state =
            RadioResponseState.None;

        private float remainingSeconds;

        public string SaveKey =>
            "radio_response";

        public Type SaveStateType =>
            typeof(RadioResponseSaveState);

        public RadioResponseState State =>
            state;

        public BroadcastRuntimeState ActiveBroadcast =>
            activeBroadcast;

        public float RemainingSeconds =>
            remainingSeconds;

        public bool IsWaiting =>
            state == RadioResponseState.Waiting;

        public RadioResponseSystem(
            EventBus eventBus,
            DebugLogger debugLogger,
            StoryFlagSystem storyFlagSystem,
            BroadcastLibrarySystem broadcastLibrarySystem)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;
            this.storyFlagSystem =
                storyFlagSystem;

            this.broadcastLibrarySystem =
                broadcastLibrarySystem;

            eventBus.Subscribe<
                BroadcastPlaybackCompletedEvent>(
                OnBroadcastCompleted
            );

            eventBus.Subscribe<
                BroadcastInterruptedEvent>(
                OnBroadcastInterrupted
            );

            eventBus.Subscribe<
                AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );

            debugLogger.Log(
                "Radio response system initialized.",
                "Radio"
            );
        }

        public void Tick(float deltaTime)
        {
            if (!IsWaiting)
                return;

            remainingSeconds -= deltaTime;

            if (remainingSeconds > 0f)
                return;

            ExpireResponse();
        }

        public bool SelectResponse(
            string responseId)
        {
            if (!IsWaiting ||
                activeBroadcast == null)
            {
                return false;
            }

            foreach (var response
                     in activeBroadcast.Definition.ResponseOptions)
            {
                if (response == null)
                    continue;

                if (response.ResponseId != responseId)
                    continue;

                return SelectResponse(
                    response
                );
            }

            return false;
        }

        public bool SelectResponse(
            RadioResponseDefinition response)
        {
            if (!IsWaiting ||
                activeBroadcast == null ||
                response == null)
            {
                return false;
            }

            if (!IsResponseAvailable(response))
                return false;

            ApplyResponse(response);

            state =
                RadioResponseState.Chosen;

            eventBus.Publish(
                new RadioResponseSelectedEvent(
                    activeBroadcast.Definition,
                    response
                )
            );

            debugLogger.Log(
                $"Radio response selected: " +
                $"{response.DisplayText}",
                "Radio"
            );

            activeBroadcast = null;
            remainingSeconds = 0f;

            state =
                RadioResponseState.None;

            return true;
        }

        public bool IsResponseAvailable(
            RadioResponseDefinition response)
        {
            if (response == null ||
                !response.Enabled)
            {
                return false;
            }

            foreach (string flag
                     in response.RequiredFlags)
            {
                if (!storyFlagSystem.HasFlag(flag))
                    return false;
            }

            foreach (string flag
                     in response.BlockedFlags)
            {
                if (storyFlagSystem.HasFlag(flag))
                    return false;
            }

            return true;
        }

        private void ApplyResponse(
            RadioResponseDefinition response)
        {
            foreach (string flag
                     in response.FlagsToSet)
            {
                storyFlagSystem.SetFlag(
                    flag,
                    true,
                    $"Radio response: {response.ResponseId}"
                );
            }

            foreach (string flag
                     in response.FlagsToClear)
            {
                storyFlagSystem.SetFlag(
                    flag,
                    false,
                    $"Radio response: {response.ResponseId}"
                );
            }
        }

        private void OpenResponseWindow(
            BroadcastRuntimeState broadcast)
        {
            activeBroadcast = broadcast;

            remainingSeconds =
                broadcast.Definition
                    .ResponseWindowSeconds;

            state =
                RadioResponseState.Waiting;

            eventBus.Publish(
                new RadioResponseWindowOpenedEvent(
                    broadcast.Definition
                )
            );

            debugLogger.Log(
                $"Response window opened for: " +
                $"{broadcast.Definition.Title}",
                "Radio"
            );
        }

        private void ExpireResponse()
        {
            if (activeBroadcast == null)
                return;

            BroadcastDefinition definition =
                activeBroadcast.Definition;

            foreach (string flag
                     in definition.FlagsSetIfIgnored)
            {
                storyFlagSystem.SetFlag(
                    flag,
                    true,
                    $"Ignored broadcast: " +
                    $"{definition.BroadcastId}"
                );
            }

            state =
                RadioResponseState.Expired;

            eventBus.Publish(
                new RadioResponseExpiredEvent(
                    definition
                )
            );

            debugLogger.Log(
                $"Response expired for: " +
                $"{definition.Title}",
                "Radio"
            );

            activeBroadcast = null;
            remainingSeconds = 0f;

            state =
                RadioResponseState.None;
        }

        private void OnBroadcastCompleted(
            BroadcastPlaybackCompletedEvent completedEvent)
        {
            BroadcastDefinition definition =
                completedEvent.Broadcast;

            if (!definition.ExpectsResponse)
                return;

            if (!broadcastLibrarySystem.TryGetBroadcast(
                    definition.BroadcastId,
                    out BroadcastRuntimeState runtime))
            {
                return;
            }

            OpenResponseWindow(runtime);
        }

        private void OnBroadcastInterrupted(
            BroadcastInterruptedEvent interruptedEvent)
        {
            if (activeBroadcast == null)
                return;

            if (activeBroadcast.Definition.BroadcastId !=
                interruptedEvent.Broadcast.BroadcastId)
            {
                return;
            }

            activeBroadcast = null;
            remainingSeconds = 0f;
            state = RadioResponseState.None;
        }

        private void OnAllSaveDataRestored(
            AllSaveDataRestoredEvent restoredEvent)
        {
            if (!IsWaiting ||
                activeBroadcast == null)
            {
                return;
            }

            if (!activeBroadcast.Definition.ExpectsResponse)
            {
                activeBroadcast = null;
                state = RadioResponseState.None;
                remainingSeconds = 0f;
            }
        }

        public object CaptureState()
        {
            return new RadioResponseSaveState
            {
                activeBroadcastId =
                    activeBroadcast != null
                        ? activeBroadcast.Definition.BroadcastId
                        : string.Empty,

                remainingSeconds =
                    remainingSeconds,

                waitingForResponse =
                    IsWaiting
            };
        }

        public void RestoreState(object stateObject)
        {
            if (stateObject is not RadioResponseSaveState
                saveState)
            {
                return;
            }

            activeBroadcast = null;

            remainingSeconds =
                Math.Max(
                    0f,
                    saveState.remainingSeconds
                );

            state =
                RadioResponseState.None;

            if (!saveState.waitingForResponse)
                return;

            if (string.IsNullOrWhiteSpace(
                    saveState.activeBroadcastId))
            {
                return;
            }

            if (!broadcastLibrarySystem.TryGetBroadcast(
                    saveState.activeBroadcastId,
                    out BroadcastRuntimeState broadcast))
            {
                return;
            }

            activeBroadcast = broadcast;
            state = RadioResponseState.Waiting;

            debugLogger.Log(
                "Radio response state restored.",
                "Radio"
            );
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<
                BroadcastPlaybackCompletedEvent>(
                OnBroadcastCompleted
            );

            eventBus.Unsubscribe<
                BroadcastInterruptedEvent>(
                OnBroadcastInterrupted
            );

            eventBus.Unsubscribe<
                AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    public sealed class BroadcastPlaybackSystem :
        ISaveable
    {
        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;

        private readonly RadioReceiverSystem
            radioReceiverSystem;

        private readonly BroadcastLibrarySystem
            librarySystem;

        private BroadcastRuntimeState activeBroadcast;

        private int currentSegmentIndex;

        private float segmentElapsedSeconds;

        private BroadcastPlaybackState playbackState =
            BroadcastPlaybackState.Idle;

        public string SaveKey =>
            "broadcast_playback";

        public Type SaveStateType =>
            typeof(BroadcastPlaybackSaveState);

        public BroadcastRuntimeState ActiveBroadcast =>
            activeBroadcast;

        public int CurrentSegmentIndex =>
            currentSegmentIndex;

        public float SegmentElapsedSeconds =>
            segmentElapsedSeconds;

        public BroadcastPlaybackState PlaybackState =>
            playbackState;

        public bool IsPlaying =>
            playbackState ==
                BroadcastPlaybackState.WaitingForSegment ||
            playbackState ==
                BroadcastPlaybackState.PlayingSegment;

        public BroadcastPlaybackSystem(
            EventBus eventBus,
            DebugLogger debugLogger,
            RadioReceiverSystem radioReceiverSystem,
            BroadcastLibrarySystem librarySystem)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;
            this.radioReceiverSystem =
                radioReceiverSystem;
            this.librarySystem =
                librarySystem;

            eventBus.Subscribe<RadioSignalTunedEvent>(
                OnSignalTuned
            );

            eventBus.Subscribe<RadioSignalUntunedEvent>(
                OnSignalUntuned
            );

            eventBus.Subscribe<
                BroadcastEligibilityUpdatedEvent>(
                OnEligibilityUpdated
            );

            eventBus.Subscribe<
                AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );

            debugLogger.Log(
                "Broadcast playback system initialized.",
                "Radio"
            );
        }

        public void Tick(float deltaTime)
        {
            if (activeBroadcast == null)
                return;

            if (!IsPlaying)
                return;

            BroadcastDefinition definition =
                activeBroadcast.Definition;

            if (currentSegmentIndex >=
                definition.TranscriptSegments.Count)
            {
                CompleteBroadcast();
                return;
            }

            BroadcastTranscriptSegment segment =
                definition.TranscriptSegments[
                    currentSegmentIndex
                ];

            if (segment == null)
            {
                AdvanceSegment();
                return;
            }

            segmentElapsedSeconds += deltaTime;

            if (playbackState ==
                BroadcastPlaybackState.WaitingForSegment)
            {
                if (segmentElapsedSeconds <
                    segment.DelayBeforeSeconds)
                {
                    return;
                }

                segmentElapsedSeconds = 0f;

                playbackState =
                    BroadcastPlaybackState.PlayingSegment;

                eventBus.Publish(
                    new BroadcastSegmentStartedEvent(
                        definition,
                        currentSegmentIndex
                    )
                );

                return;
            }

            if (playbackState ==
                BroadcastPlaybackState.PlayingSegment)
            {
                if (segmentElapsedSeconds <
                    segment.EffectiveDurationSeconds)
                {
                    return;
                }

                AdvanceSegment();
            }

            foreach (BroadcastRuntimeState state
                in librarySystem.Broadcasts)
            {
                if (state.RuntimeCooldownSeconds > 0f)
                {
                    state.RuntimeCooldownSeconds =
                        Math.Max(
                            0f,
                            state.RuntimeCooldownSeconds -
                            deltaTime
                        );
                }
            }
        }

        private void TryStartBroadcastForTunedSignal()
        {
            RadioSignalRuntimeState tuned =
                radioReceiverSystem.TunedSignal;

            if (tuned == null)
                return;

            if (activeBroadcast != null)
                return;

            foreach (BroadcastRuntimeState candidate
                     in librarySystem.EligibleBroadcasts)
            {
                if (candidate.Definition.Signal == null)
                    continue;

                if (candidate.Definition.Signal.SignalId !=
                    tuned.Definition.SignalId)
                {
                    continue;
                }

                StartBroadcast(candidate);
                return;
            }
        }

        private void StartBroadcast(
            BroadcastRuntimeState broadcast)
        {
            activeBroadcast = broadcast;

            currentSegmentIndex = 0;
            segmentElapsedSeconds = 0f;

            playbackState =
                BroadcastPlaybackState.WaitingForSegment;

            eventBus.Publish(
                new BroadcastStartedEvent(
                    broadcast.Definition
                )
            );

            debugLogger.Log(
                $"Broadcast started: " +
                $"{broadcast.Definition.Title}",
                "Radio"
            );
        }

        private void AdvanceSegment()
        {
            currentSegmentIndex++;
            segmentElapsedSeconds = 0f;

            if (activeBroadcast == null)
                return;

            if (currentSegmentIndex >=
                activeBroadcast.Definition
                    .TranscriptSegments.Count)
            {
                CompleteBroadcast();
                return;
            }

            playbackState =
                BroadcastPlaybackState.WaitingForSegment;
        }

        private void CompleteBroadcast()
        {
            if (activeBroadcast == null)
                return;

            BroadcastRuntimeState completed =
                activeBroadcast;

            playbackState =
                BroadcastPlaybackState.Completed;

            librarySystem.RecordCompletion(
                completed.Definition.BroadcastId
            );

            eventBus.Publish(
                new BroadcastPlaybackCompletedEvent(
                    completed.Definition
                )
            );

            debugLogger.Log(
                $"Broadcast completed: " +
                $"{completed.Definition.Title}",
                "Radio"
            );

            activeBroadcast = null;
            currentSegmentIndex = 0;
            segmentElapsedSeconds = 0f;

            playbackState =
                BroadcastPlaybackState.Idle;

            TryStartBroadcastForTunedSignal();
        }

        private void InterruptBroadcast()
        {
            if (activeBroadcast == null)
                return;

            BroadcastDefinition interrupted =
                activeBroadcast.Definition;

            playbackState =
                BroadcastPlaybackState.Interrupted;

            eventBus.Publish(
                new BroadcastInterruptedEvent(
                    interrupted
                )
            );

            debugLogger.Log(
                $"Broadcast interrupted: " +
                $"{interrupted.Title}",
                "Radio"
            );

            activeBroadcast = null;
            currentSegmentIndex = 0;
            segmentElapsedSeconds = 0f;

            playbackState =
                BroadcastPlaybackState.Idle;
        }

        private void OnSignalTuned(
            RadioSignalTunedEvent tunedEvent)
        {
            TryStartBroadcastForTunedSignal();
        }

        private void OnSignalUntuned(
            RadioSignalUntunedEvent untunedEvent)
        {
            if (activeBroadcast == null)
                return;

            if (activeBroadcast.Definition.Signal == null)
                return;

            if (activeBroadcast.Definition.Signal.SignalId !=
                untunedEvent.Signal.SignalId)
            {
                return;
            }

            InterruptBroadcast();
        }

        private void OnEligibilityUpdated(
            BroadcastEligibilityUpdatedEvent updateEvent)
        {
            TryStartBroadcastForTunedSignal();
        }

        private void OnAllSaveDataRestored(
            AllSaveDataRestoredEvent restoredEvent)
        {
            ValidateRestoredPlayback();
        }

        private void ValidateRestoredPlayback()
        {
            if (activeBroadcast == null)
            {
                TryStartBroadcastForTunedSignal();
                return;
            }

            RadioSignalRuntimeState tuned =
                radioReceiverSystem.TunedSignal;

            if (tuned == null ||
                activeBroadcast.Definition.Signal == null ||
                activeBroadcast.Definition.Signal.SignalId !=
                    tuned.Definition.SignalId)
            {
                InterruptBroadcast();
            }
        }

        public object CaptureState()
        {
            BroadcastPlaybackSaveState saveState =
                new BroadcastPlaybackSaveState();

            saveState.activeBroadcastId =
                activeBroadcast != null
                    ? activeBroadcast.Definition.BroadcastId
                    : string.Empty;

            saveState.currentSegmentIndex =
                currentSegmentIndex;

            saveState.segmentElapsedSeconds =
                segmentElapsedSeconds;

            saveState.wasPlaying =
                IsPlaying;

            return saveState;
        }

        public void RestoreState(object state)
        {
            if (state is not BroadcastPlaybackSaveState
                saveState)
            {
                return;
            }

            activeBroadcast = null;

            currentSegmentIndex =
                Math.Max(
                    0,
                    saveState.currentSegmentIndex
                );

            segmentElapsedSeconds =
                Math.Max(
                    0f,
                    saveState.segmentElapsedSeconds
                );

            if (!string.IsNullOrWhiteSpace(
                    saveState.activeBroadcastId) &&
                librarySystem.TryGetBroadcast(
                    saveState.activeBroadcastId,
                    out BroadcastRuntimeState restored))
            {
                activeBroadcast = restored;

                playbackState =
                    saveState.wasPlaying
                        ? BroadcastPlaybackState.PlayingSegment
                        : BroadcastPlaybackState.Interrupted;
            }
            else
            {
                playbackState =
                    BroadcastPlaybackState.Idle;
            }

            debugLogger.Log(
                "Broadcast playback state restored.",
                "Radio"
            );
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<RadioSignalTunedEvent>(
                OnSignalTuned
            );

            eventBus.Unsubscribe<RadioSignalUntunedEvent>(
                OnSignalUntuned
            );

            eventBus.Unsubscribe<
                BroadcastEligibilityUpdatedEvent>(
                OnEligibilityUpdated
            );

            eventBus.Unsubscribe<
                AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );
        }
    }
}
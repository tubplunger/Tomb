using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;
using Tomb.Core.Time;
using Tomb.Gameplay.Story;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    public sealed class BroadcastLibrarySystem : ISaveable
    {
        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;
        private readonly GameTimeSystem timeSystem;
        private readonly RadioReceiverSystem
            radioReceiverSystem;
        private readonly StoryFlagSystem storyFlagSystem;

        private readonly Dictionary<string,
            BroadcastRuntimeState> statesById = new();

        private readonly List<BroadcastRuntimeState>
            orderedStates = new();

        private readonly List<BroadcastRuntimeState>
            eligibleBroadcasts = new();

        public string SaveKey => "broadcast_library";

        public Type SaveStateType =>
            typeof(BroadcastSaveState);

        public IReadOnlyList<BroadcastRuntimeState>
            Broadcasts => orderedStates;

        public IReadOnlyList<BroadcastRuntimeState>
            EligibleBroadcasts => eligibleBroadcasts;

        public BroadcastLibrarySystem(
            EventBus eventBus,
            DebugLogger debugLogger,
            GameTimeSystem timeSystem,
            RadioReceiverSystem radioReceiverSystem,
            StoryFlagSystem storyFlagSystem,
            BroadcastCatalog catalog)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;
            this.timeSystem = timeSystem;
            this.radioReceiverSystem =
                radioReceiverSystem;
            this.storyFlagSystem = storyFlagSystem;

            InitializeFromCatalog(catalog);

            eventBus.Subscribe<GameDayPassedEvent>(
                OnGameDayPassed
            );

            eventBus.Subscribe<
                RadioReceiverUpdatedEvent>(
                OnRadioReceiverUpdated
            );

            eventBus.Subscribe<StoryFlagChangedEvent>(
                OnStoryFlagChanged
            );

            eventBus.Subscribe<
                AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );

            RecalculateEligibility(false);

            debugLogger.Log(
                $"Broadcast library initialized with " +
                $"{orderedStates.Count} broadcasts.",
                "Radio"
            );
        }

        public bool TryGetBroadcast(
            string broadcastId,
            out BroadcastRuntimeState state)
        {
            if (string.IsNullOrWhiteSpace(broadcastId))
            {
                state = null;
                return false;
            }

            return statesById.TryGetValue(
                broadcastId,
                out state
            );
        }

        public void RecordCompletion(
            string broadcastId)
        {
            if (!TryGetBroadcast(
                    broadcastId,
                    out BroadcastRuntimeState state))
            {
                debugLogger.Log(
                    $"Unknown broadcast: {broadcastId}",
                    "Radio"
                );

                return;
            }

            state.RecordCompletion();

            eventBus.Publish(
                new BroadcastCompletionRecordedEvent(
                    state.Definition,
                    state.PlayCount
                )
            );

            RecalculateEligibility(true);
        }

        public void RecalculateEligibility(
            bool publishEvents = true)
        {
            eligibleBroadcasts.Clear();

            foreach (BroadcastRuntimeState state
                     in orderedStates)
            {
                bool wasEligible =
                    state.IsEligible;

                EvaluateBroadcast(state);

                if (state.IsEligible)
                {
                    eligibleBroadcasts.Add(state);
                }

                if (!publishEvents ||
                    wasEligible == state.IsEligible)
                {
                    continue;
                }

                if (state.IsEligible)
                {
                    eventBus.Publish(
                        new BroadcastBecameEligibleEvent(
                            state.Definition
                        )
                    );
                }
                else
                {
                    eventBus.Publish(
                        new BroadcastBecameIneligibleEvent(
                            state.Definition,
                            state.Status
                        )
                    );
                }
            }

            eligibleBroadcasts.Sort(
                CompareEligibleBroadcasts
            );

            if (publishEvents)
            {
                eventBus.Publish(
                    new BroadcastEligibilityUpdatedEvent(
                        eligibleBroadcasts.Count
                    )
                );
            }
        }

        private void EvaluateBroadcast(
            BroadcastRuntimeState state)
        {
            BroadcastDefinition definition =
                state.Definition;

            if (!definition.Enabled)
            {
                SetStatus(
                    state,
                    BroadcastRuntimeStatus.Disabled,
                    "Broadcast asset disabled"
                );

                return;
            }

            if (definition.Signal == null)
            {
                SetStatus(
                    state,
                    BroadcastRuntimeStatus.SignalUnavailable,
                    "No signal assigned"
                );

                return;
            }

            if (!radioReceiverSystem.TryGetSignalState(
                    definition.Signal.SignalId,
                    out RadioSignalRuntimeState signalState) ||
                !signalState.IsReceivable)
            {
                SetStatus(
                    state,
                    BroadcastRuntimeStatus.SignalUnavailable,
                    "Source signal is not receivable"
                );

                return;
            }

            int currentDay =
                timeSystem.CurrentTime.Day;

            if (currentDay < definition.MinimumGameDay)
            {
                SetStatus(
                    state,
                    BroadcastRuntimeStatus.TooEarly,
                    $"Available on day " +
                    $"{definition.MinimumGameDay}"
                );

                return;
            }

            if (definition.MaximumGameDay > 0 &&
                currentDay > definition.MaximumGameDay)
            {
                SetStatus(
                    state,
                    BroadcastRuntimeStatus.Expired,
                    $"Expired after day " +
                    $"{definition.MaximumGameDay}"
                );

                return;
            }

            if (!HasAllRequiredFlags(definition))
            {
                SetStatus(
                    state,
                    BroadcastRuntimeStatus.RequiredFlagsMissing,
                    "Required story flags are missing"
                );

                return;
            }

            if (HasAnyBlockedFlag(definition))
            {
                SetStatus(
                    state,
                    BroadcastRuntimeStatus.BlockedByFlag,
                    "A blocking story flag is active"
                );

                return;
            }

            if (state.PlayCount >=
                definition.MaximumPlayCount)
            {
                SetStatus(
                    state,
                    BroadcastRuntimeStatus.PlayLimitReached,
                    "Maximum play count reached"
                );

                return;
            }

            SetStatus(
                state,
                BroadcastRuntimeStatus.Eligible,
                "Eligible"
            );
        }

        private bool HasAllRequiredFlags(
            BroadcastDefinition definition)
        {
            foreach (string flag
                     in definition.RequiredFlags)
            {
                if (!storyFlagSystem.HasFlag(flag))
                    return false;
            }

            return true;
        }

        private bool HasAnyBlockedFlag(
            BroadcastDefinition definition)
        {
            foreach (string flag
                     in definition.BlockedFlags)
            {
                if (storyFlagSystem.HasFlag(flag))
                    return true;
            }

            return false;
        }

        private static void SetStatus(
            BroadcastRuntimeState state,
            BroadcastRuntimeStatus status,
            string reason)
        {
            state.Status = status;
            state.StatusReason = reason;
        }

        private static int CompareEligibleBroadcasts(
            BroadcastRuntimeState first,
            BroadcastRuntimeState second)
        {
            int priorityComparison =
                second.Definition.Priority.CompareTo(
                    first.Definition.Priority
                );

            if (priorityComparison != 0)
                return priorityComparison;

            return string.Compare(
                first.Definition.BroadcastId,
                second.Definition.BroadcastId,
                StringComparison.Ordinal
            );
        }

        private void InitializeFromCatalog(
            BroadcastCatalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(
                    nameof(catalog)
                );
            }

            foreach (BroadcastDefinition definition
                     in catalog.Broadcasts)
            {
                if (definition == null)
                    continue;

                if (string.IsNullOrWhiteSpace(
                        definition.BroadcastId))
                {
                    debugLogger.Log(
                        $"Broadcast '{definition.name}' " +
                        "has no ID.",
                        "Radio"
                    );

                    continue;
                }

                if (statesById.ContainsKey(
                        definition.BroadcastId))
                {
                    debugLogger.Log(
                        $"Duplicate broadcast ID rejected: " +
                        $"{definition.BroadcastId}",
                        "Radio"
                    );

                    continue;
                }

                BroadcastRuntimeState state =
                    new BroadcastRuntimeState(
                        definition
                    );

                statesById.Add(
                    definition.BroadcastId,
                    state
                );

                orderedStates.Add(state);
            }
        }

        private void OnGameDayPassed(
            GameDayPassedEvent dayEvent)
        {
            RecalculateEligibility(true);
        }

        private void OnRadioReceiverUpdated(
            RadioReceiverUpdatedEvent receiverEvent)
        {
            RecalculateEligibility(true);
        }

        private void OnStoryFlagChanged(
            StoryFlagChangedEvent flagEvent)
        {
            RecalculateEligibility(true);
        }

        private void OnAllSaveDataRestored(
            AllSaveDataRestoredEvent restoredEvent)
        {
            RecalculateEligibility(true);
        }

        public object CaptureState()
        {
            BroadcastSaveState saveState =
                new BroadcastSaveState();

            foreach (BroadcastRuntimeState state
                     in orderedStates)
            {
                saveState.broadcasts.Add(
                    new BroadcastSaveEntry
                    {
                        broadcastId =
                            state.Definition.BroadcastId,

                        playCount =
                            state.PlayCount,

                        hasCompleted =
                            state.HasCompleted
                    }
                );
            }

            return saveState;
        }

        public void RestoreState(object state)
        {
            if (state is not BroadcastSaveState saveState)
                return;

            foreach (BroadcastSaveEntry entry
                     in saveState.broadcasts)
            {
                if (entry == null ||
                    string.IsNullOrWhiteSpace(
                        entry.broadcastId))
                {
                    continue;
                }

                if (!TryGetBroadcast(
                        entry.broadcastId,
                        out BroadcastRuntimeState runtime))
                {
                    debugLogger.Log(
                        $"Save contains unknown broadcast: " +
                        $"{entry.broadcastId}",
                        "Radio"
                    );

                    continue;
                }

                runtime.Restore(
                    entry.playCount,
                    entry.hasCompleted
                );
            }

            debugLogger.Log(
                "Broadcast runtime state restored.",
                "Radio"
            );
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<GameDayPassedEvent>(
                OnGameDayPassed
            );

            eventBus.Unsubscribe<
                RadioReceiverUpdatedEvent>(
                OnRadioReceiverUpdated
            );

            eventBus.Unsubscribe<StoryFlagChangedEvent>(
                OnStoryFlagChanged
            );

            eventBus.Unsubscribe<
                AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );
        }
    }
}
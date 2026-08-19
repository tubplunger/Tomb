using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;
using Tomb.Gameplay.Machines;

namespace Tomb.Gameplay.Radio
{
    public sealed class RadioReceiverSystem : ISaveable
    {
        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;

        private readonly RadioVisibilitySystem
            visibilitySystem;

        private readonly MachineSystem machineSystem;
        private readonly RadioReceiverSettings settings;

        private readonly Dictionary<string,
            RadioSignalRuntimeState> statesById = new();

        private readonly List<RadioSignalRuntimeState>
            orderedStates = new();

        private RadioSignalRuntimeState tunedSignal;

        private float currentFrequencyMHz;

        public string SaveKey =>
            "radio_receiver";

        public Type SaveStateType =>
            typeof(RadioReceiverSaveState);

        public IReadOnlyList<RadioSignalRuntimeState>
            Signals => orderedStates;

        public RadioSignalRuntimeState TunedSignal =>
            tunedSignal;

        public bool IsReceiverOperational =>
            GetCommunicationsMachineOperational();

        public float CurrentFrequencyMHz =>
            currentFrequencyMHz;

        public RadioReceiverSystem(
            EventBus eventBus,
            DebugLogger debugLogger,
            RadioVisibilitySystem visibilitySystem,
            MachineSystem machineSystem,
            RadioSignalCatalog catalog,
            RadioReceiverSettings settings)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;

            this.visibilitySystem =
                visibilitySystem;

            this.machineSystem =
                machineSystem;

            this.settings =
                settings;

            InitializeFromCatalog(catalog);

            eventBus.Subscribe<
                RadioVisibilityUpdatedEvent>(
                OnVisibilityUpdated
            );

            eventBus.Subscribe<
                MachineStateChangedEvent>(
                OnMachineStateChanged
            );

            eventBus.Subscribe<
                MachineConditionChangedEvent>(
                OnMachineConditionChanged
            );

            eventBus.Subscribe<
                AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );

            currentFrequencyMHz =
                settings.MinimumFrequencyMHz;

            Recalculate(false);

            debugLogger.Log(
                $"Radio receiver initialized with " +
                $"{orderedStates.Count} signal definitions.",
                "Radio"
            );
        }

        public bool TryGetSignalState(
            string signalId,
            out RadioSignalRuntimeState state)
        {
            if (string.IsNullOrWhiteSpace(signalId))
            {
                state = null;
                return false;
            }

            return statesById.TryGetValue(
                signalId,
                out state
            );
        }

        public RadioSignalRuntimeState
            GetNearestReceivableSignal()
        {
            RadioSignalRuntimeState nearest = null;
            float nearestDifference =
                float.MaxValue;

            foreach (RadioSignalRuntimeState state
                     in orderedStates)
            {
                if (!state.IsReceivable)
                    continue;

                float difference =
                    Mathf.Abs(
                        state.Definition.FrequencyMHz -
                        currentFrequencyMHz
                    );

                if (difference < nearestDifference)
                {
                    nearestDifference = difference;
                    nearest = state;
                }
            }

            return nearest;
        }

        public float GetFrequencyProximity(
            RadioSignalRuntimeState state)
        {
            if (state == null)
                return 0f;

            float difference =
                Mathf.Abs(
                    state.Definition.FrequencyMHz -
                    currentFrequencyMHz
                );

            const float proximityWindow = 5f;

            return Mathf.Clamp01(
                1f -
                difference / proximityWindow
            );
        }

        public void StepFrequencyCoarse(
            int direction,
            string reason = "Unspecified")
        {
            if (direction == 0)
                return;

            SetFrequency(
                currentFrequencyMHz +
                settings.CoarseFrequencyStepMHz *
                Mathf.Sign(direction),
                reason
            );
        }

        public void SetFrequency(
            float frequencyMHz,
            string reason = "Unspecified")
        {
            float clampedFrequency =
                Mathf.Clamp(
                    frequencyMHz,
                    settings.MinimumFrequencyMHz,
                    settings.MaximumFrequencyMHz
                );

            if (Mathf.Approximately(
                    currentFrequencyMHz,
                    clampedFrequency))
            {
                return;
            }

            currentFrequencyMHz =
                clampedFrequency;

            EvaluateFrequencyTuning(reason);

            eventBus.Publish(
                new RadioFrequencyChangedEvent(
                    currentFrequencyMHz
                )
            );

            eventBus.Publish(
                new RadioReceiverUpdatedEvent(
                    CountActiveDetectedSignals(),
                    tunedSignal != null
                        ? tunedSignal.Definition.SignalId
                        : string.Empty
                )
            );
        }

        public void StepFrequency(
            int direction,
            string reason = "Unspecified")
        {
            if (direction == 0)
                return;

            float nextFrequency =
                currentFrequencyMHz +
                settings.FrequencyStepMHz *
                Mathf.Sign(direction);

            SetFrequency(
                nextFrequency,
                reason
            );
        }

        private int CountActiveDetectedSignals()
        {
            int count = 0;

            foreach (RadioSignalRuntimeState state
                     in orderedStates)
            {
                if (state.Status ==
                        RadioSignalReceiverStatus.Detected ||
                    state.Status ==
                        RadioSignalReceiverStatus.Tuned)
                {
                    count++;
                }
            }

            return count;
        }

        private void EvaluateFrequencyTuning(
            string reason)
        {
            RadioSignalRuntimeState bestMatch = null;
            float bestDifference = float.MaxValue;

            foreach (RadioSignalRuntimeState state
                     in orderedStates)
            {
                if (!state.IsReceivable)
                    continue;

                float difference =
                    Mathf.Abs(
                        state.Definition.FrequencyMHz -
                        currentFrequencyMHz
                    );

                if (difference >
                    settings.TuningToleranceMHz)
                {
                    continue;
                }

                if (difference < bestDifference)
                {
                    bestDifference = difference;
                    bestMatch = state;
                }
            }

            if (bestMatch == null)
            {
                Untune();
                return;
            }

            TuneSignal(
                bestMatch.Definition.SignalId
            );
        }

        public bool TuneSignal(string signalId)
        {
            if (!TryGetSignalState(
                    signalId,
                    out RadioSignalRuntimeState state))
            {
                debugLogger.Log(
                    $"Tune failed: unknown signal '{signalId}'.",
                    "Radio"
                );

                return false;
            }

            if (!visibilitySystem.IsSignalVisible(signalId))
            {
                debugLogger.Log(
                    $"Tune failed for '{state.Definition.DisplayName}': " +
                    "signal is not currently in range.",
                    "Radio"
                );

                return false;
            }

            if (!IsReceiverOperational)
            {
                debugLogger.Log(
                    $"Tune failed for '{state.Definition.DisplayName}': " +
                    "receiver is offline.",
                    "Radio"
                );

                return false;
            }

            if (state.CurrentStrength <
                settings.MinimumReceivableStrength)
            {
                debugLogger.Log(
                    $"Tune failed for '{state.Definition.DisplayName}': " +
                    $"strength {state.CurrentStrength:0.00} is below " +
                    $"minimum {settings.MinimumReceivableStrength:0.00}.",
                    "Radio"
                );

                return false;
            }

            if (!state.HasBeenDetected)
            {
                debugLogger.Log(
                    $"Tune failed for '{state.Definition.DisplayName}': " +
                    "signal has not been detected yet.",
                    "Radio"
                );

                return false;
            }

            if (tunedSignal == state)
                return true;

            if (tunedSignal != null)
            {
                RadioSignalRuntimeState previous =
                    tunedSignal;

                previous.IsTuned = false;

                eventBus.Publish(
                    new RadioSignalUntunedEvent(
                        previous.Definition
                    )
                );
            }

            tunedSignal = state;
            state.IsTuned = true;

            eventBus.Publish(
                new RadioSignalTunedEvent(
                    state.Definition
                )
            );

            Recalculate(true);

            debugLogger.Log(
                $"Tuned signal: " +
                $"{state.Definition.DisplayName} " +
                $"({state.Definition.FrequencyMHz:0.0} MHz)",
                "Radio"
            );

            return true;
        }

        public void Untune()
        {
            if (tunedSignal == null)
                return;

            RadioSignalRuntimeState previous =
                tunedSignal;

            tunedSignal.IsTuned = false;
            tunedSignal = null;

            eventBus.Publish(
                new RadioSignalUntunedEvent(
                    previous.Definition
                )
            );

            Recalculate(true);
        }

        private void InitializeFromCatalog(
            RadioSignalCatalog catalog)
        {
            foreach (RadioSignalDefinition definition
                     in catalog.Signals)
            {
                if (definition == null ||
                    string.IsNullOrWhiteSpace(
                        definition.SignalId))
                {
                    continue;
                }

                if (statesById.ContainsKey(
                        definition.SignalId))
                {
                    debugLogger.Log(
                        $"Duplicate receiver signal ID rejected: " +
                        $"{definition.SignalId}",
                        "Radio"
                    );

                    continue;
                }

                RadioSignalRuntimeState state =
                    new RadioSignalRuntimeState(
                        definition
                    );

                statesById.Add(
                    definition.SignalId,
                    state
                );

                orderedStates.Add(state);
            }
        }

        private void Recalculate(
            bool publishEvents)
        {
            bool receiverOperational =
                GetCommunicationsMachineOperational();

            int detectedCount = 0;

            foreach (RadioSignalRuntimeState state
                     in orderedStates)
            {
                RadioSignalReceiverStatus
                    previousStatus =
                        state.Status;

                bool wasDetected =
                    previousStatus ==
                        RadioSignalReceiverStatus.Detected ||
                    previousStatus ==
                        RadioSignalReceiverStatus.Tuned;

                bool geographicallyAvailable =
                    visibilitySystem.IsSignalVisible(
                        state.Definition.SignalId
                    );

                float strength =
                    CalculateSignalStrength(
                        state,
                        geographicallyAvailable
                    );

                Debug.Log(
                    $"[RECEIVER RECALCULATE] " +
                    $"{state.Definition.SignalId} | " +
                    $"Visible={geographicallyAvailable} | " +
                    $"Strength={strength:0.00}"
                );

                state.CurrentStrength = strength;

                state.StaticAmount =
                    CalculateStaticAmount(
                        strength,
                        receiverOperational
                    );

                if (!geographicallyAvailable)
                {
                    state.Status =
                        RadioSignalReceiverStatus.Unavailable;
                }
                else if (!receiverOperational)
                {
                    state.Status =
                        RadioSignalReceiverStatus.ReceiverOffline;
                }
                else if (strength <
                    settings.MinimumReceivableStrength)
                {
                    state.Status =
                        RadioSignalReceiverStatus.TooWeak;
                }
                else
                {
                    if (strength >=
                        settings.AutomaticDetectionThreshold)
                    {
                        state.HasBeenDetected = true;
                    }

                    if (state.IsTuned)
                    {
                        state.Status =
                            RadioSignalReceiverStatus.Tuned;
                    }
                    else if (state.HasBeenDetected)
                    {
                        state.Status =
                            RadioSignalReceiverStatus.Detected;
                    }
                    else
                    {
                        state.Status =
                            RadioSignalReceiverStatus.Available;
                    }
                }

                if (state.Definition.SignalId ==
                    "na_emergency_broadcast")
                {
                    Debug.Log(
                        $"[NA FINAL STATE] " +
                        $"Visible={geographicallyAvailable} | " +
                        $"Strength={state.CurrentStrength:0.00} | " +
                        $"Status={state.Status} | " +
                        $"Detected={state.HasBeenDetected} | " +
                        $"ReceiverOperational={receiverOperational}"
                    );
                }

                bool isDetectedNow =
                    state.Status ==
                        RadioSignalReceiverStatus.Detected ||
                    state.Status ==
                        RadioSignalReceiverStatus.Tuned;

                if (isDetectedNow)
                    detectedCount++;

                if (!publishEvents)
                    continue;

                if (!wasDetected && isDetectedNow)
                {
                    eventBus.Publish(
                        new RadioSignalDetectedEvent(
                            state.Definition,
                            state.CurrentStrength
                        )
                    );

                    debugLogger.Log(
                        $"Signal detected: " +
                        $"{state.Definition.DisplayName}",
                        "Radio"
                    );
                }
                else if (wasDetected &&
                         !isDetectedNow)
                {
                    eventBus.Publish(
                        new RadioSignalDetectionLostEvent(
                            state.Definition
                        )
                    );

                    debugLogger.Log(
                        $"Signal reception lost: " +
                        $"{state.Definition.DisplayName}",
                        "Radio"
                    );
                }
            }

            if (tunedSignal != null)
            {
                bool shouldRemainTuned =
                    visibilitySystem.IsSignalVisible(
                        tunedSignal.Definition.SignalId
                    ) &&
                    IsReceiverOperational &&
                    tunedSignal.CurrentStrength >=
                        settings.MinimumReceivableStrength;

                if (!shouldRemainTuned)
                {
                    RadioSignalRuntimeState previous =
                        tunedSignal;

                    previous.IsTuned = false;
                    tunedSignal = null;

                    if (publishEvents)
                    {
                        eventBus.Publish(
                            new RadioSignalUntunedEvent(
                                previous.Definition
                            )
                        );
                    }
                }
            }

            if (publishEvents)
            {
                eventBus.Publish(
                    new RadioReceiverUpdatedEvent(
                        detectedCount,
                        tunedSignal != null
                            ? tunedSignal.Definition.SignalId
                            : string.Empty
                    )
                );
            }
        }

        private float CalculateSignalStrength(
            RadioSignalRuntimeState state,
            bool geographicallyAvailable)
        {
            if (!geographicallyAvailable)
                return 0f;

            return Mathf.Clamp01(
                state.Definition.BaseSignalStrength
            );
        }

        private float CalculateStaticAmount(
            float signalStrength,
            bool receiverOperational)
        {
            if (!receiverOperational)
                return 1f;

            float conditionNormalized =
                GetReceiverConditionNormalized();

            float weaknessStatic =
                1f - signalStrength;

            float damageStatic =
                (1f - conditionNormalized) *
                settings.DamagedReceiverStaticMultiplier;

            return Mathf.Clamp01(
                settings.BaseStaticAmount +
                weaknessStatic +
                damageStatic
            );
        }

        private bool
            GetCommunicationsMachineOperational()
        {
            if (!machineSystem.TryGetMachine(
                    settings.CommunicationsMachineId,
                    out MachineState communications))
            {
                return false;
            }

            return communications.IsEnabled &&
                   !communications.IsBroken &&
                   communications.HasPower &&
                   !communications.IsInMaintenance;
        }

        private float
            GetReceiverConditionNormalized()
        {
            if (!machineSystem.TryGetMachine(
                    settings.CommunicationsMachineId,
                    out MachineState communications))
            {
                return 0f;
            }

            return communications.NormalizedCondition;
        }

        private void OnVisibilityUpdated(
            RadioVisibilityUpdatedEvent visibilityEvent)
        {
            Debug.Log(
                $"[RADIO RECEIVER] Visibility update received. " +
                $"Visible signals: {visibilityEvent.VisibleSignalCount}"
            );

            Recalculate(true);
        }

        private void OnMachineStateChanged(
            MachineStateChangedEvent stateEvent)
        {
            if (stateEvent.MachineId ==
                settings.CommunicationsMachineId)
            {
                Recalculate(true);
            }
        }

        private void OnMachineConditionChanged(
            MachineConditionChangedEvent conditionEvent)
        {
            if (conditionEvent.MachineId ==
                settings.CommunicationsMachineId)
            {
                Recalculate(true);
            }
        }

        private void OnAllSaveDataRestored(
            AllSaveDataRestoredEvent restoredEvent)
        {
            Recalculate(true);
        }

        public object CaptureState()
        {
            RadioReceiverSaveState saveState =
                new RadioReceiverSaveState();

            foreach (RadioSignalRuntimeState state
                     in orderedStates)
            {
                if (state.HasBeenDetected)
                {
                    saveState.detectedSignalIds.Add(
                        state.Definition.SignalId
                    );
                }
            }

            saveState.tunedSignalId =
            tunedSignal != null
                ? tunedSignal.Definition.SignalId
                : string.Empty;

            saveState.currentFrequencyMHz =
                currentFrequencyMHz;

            return saveState;
        }

        public void RestoreState(object state)
        {
            if (state is not RadioReceiverSaveState saveState)
                return;

            foreach (RadioSignalRuntimeState signalState
                     in orderedStates)
            {
                signalState.HasBeenDetected = false;
                signalState.IsTuned = false;
            }

            foreach (string detectedId
                     in saveState.detectedSignalIds)
            {
                if (TryGetSignalState(
                        detectedId,
                        out RadioSignalRuntimeState signal))
                {
                    signal.HasBeenDetected = true;
                }
            }

            tunedSignal = null;

            if (!string.IsNullOrWhiteSpace(
                    saveState.tunedSignalId) &&
                TryGetSignalState(
                    saveState.tunedSignalId,
                    out RadioSignalRuntimeState tuned))
            {
                tunedSignal = tuned;
                tuned.IsTuned = true;
            }

            currentFrequencyMHz =
            Mathf.Clamp(
                saveState.currentFrequencyMHz,
                settings.MinimumFrequencyMHz,
                settings.MaximumFrequencyMHz
            );

            debugLogger.Log(
                "Radio receiver state restored.",
                "Radio"
            );
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<
                RadioVisibilityUpdatedEvent>(
                OnVisibilityUpdated
            );

            eventBus.Unsubscribe<
                MachineStateChangedEvent>(
                OnMachineStateChanged
            );

            eventBus.Unsubscribe<
                MachineConditionChangedEvent>(
                OnMachineConditionChanged
            );

            eventBus.Unsubscribe<
                AllSaveDataRestoredEvent>(
                OnAllSaveDataRestored
            );
        }
    }
}
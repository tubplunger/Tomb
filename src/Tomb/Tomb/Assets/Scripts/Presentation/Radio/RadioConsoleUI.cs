using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tomb.Core.Events;
using Tomb.Core.Services;
using Tomb.Gameplay.Radio;
using Tomb.Gameplay.Radio.Broadcasts;
using Tomb.Gameplay.Radio.Responses;

namespace Tomb.Presentation.Radio
{
    public sealed class RadioConsoleUI :
        MonoBehaviour
    {
        //test

        [Header("State")]
        [SerializeField]
        private TMP_Text receiverStateText;

        [SerializeField]
        private TMP_Text frequencyText;

        [Header("Signal")]
        [SerializeField]
        private TMP_Text signalNameText;

        [SerializeField]
        private TMP_Text signalStrengthText;

        [SerializeField]
        private TMP_Text staticText;

        [SerializeField]
        private TMP_Text signalStatusText;

        [Header("Broadcast")]
        [SerializeField]
        private TMP_Text speakerText;

        [SerializeField]
        private TMP_Text transcriptText;

        [Header("Controls")]
        [SerializeField]
        private Button frequencyDownButton;

        [SerializeField]
        private Button frequencyUpButton;

        [Header("Keyboard")]
        [SerializeField]
        private KeyCode frequencyDownKey =
            KeyCode.LeftBracket;

        [SerializeField]
        private KeyCode frequencyUpKey =
            KeyCode.RightBracket;

        [SerializeField]
        private AudioSource radioAudioSource;

        [Header("Responses")]
        [SerializeField]
        private GameObject responseArea;

        [SerializeField]
        private TMP_Text responseTimerText;

        [SerializeField]
        private Transform responseButtonContainer;

        [SerializeField]
        private RadioResponseButtonView responseButtonPrefab;

        private RadioResponseSystem responseSystem;
        private readonly List<RadioResponseButtonView> responseButtons = new();

        private EventBus eventBus;
        private RadioReceiverSystem receiverSystem;

        private void Start()
        {
            responseArea.SetActive(false);

            try
            {
                eventBus =
                    CoreServices.Get<EventBus>();

                receiverSystem =
                    CoreServices.Get<RadioReceiverSystem>();

                responseSystem =
                    CoreServices.Get<RadioResponseSystem>();
            }
            catch (System.Exception exception)
            {
                Debug.LogError(
                    "[RadioConsoleUI] Radio services are unavailable. " +
                    "Make sure the Bootstrap scene is loaded first. " +
                    $"Reason: {exception.Message}"
                );

                enabled = false;
                return;
            }

            frequencyDownButton.onClick.AddListener(
                StepDown
            );

            frequencyUpButton.onClick.AddListener(
                StepUp
            );

            eventBus.Subscribe<
                RadioReceiverUpdatedEvent>(
                OnReceiverUpdated
            );

            eventBus.Subscribe<
                RadioFrequencyChangedEvent>(
                OnFrequencyChanged
            );

            eventBus.Subscribe<
                RadioSignalTunedEvent>(
                OnSignalTuned
            );

            eventBus.Subscribe<
                RadioSignalUntunedEvent>(
                OnSignalUntuned
            );

            eventBus.Subscribe<BroadcastStartedEvent>(
                OnBroadcastStarted
            );

            eventBus.Subscribe<
                BroadcastSegmentStartedEvent>(
                OnBroadcastSegmentStarted
            );

            eventBus.Subscribe<
                BroadcastInterruptedEvent>(
                OnBroadcastInterrupted
            );

            eventBus.Subscribe<
                BroadcastPlaybackCompletedEvent>(
                OnBroadcastCompleted
            );

            eventBus.Subscribe<
                RadioResponseWindowOpenedEvent>(
                OnResponseWindowOpened
            );

            eventBus.Subscribe<
                RadioResponseSelectedEvent>(
                OnResponseSelected
            );

            eventBus.Subscribe<
                RadioResponseExpiredEvent>(
                OnResponseExpired
            );

            Refresh();
        }

        private void Update()
        {
            bool coarse =
                Input.GetKey(
                    KeyCode.LeftShift
                ) ||
                Input.GetKey(
                    KeyCode.RightShift
                );

            if (Input.GetKeyDown(frequencyDownKey))
            {
                if (coarse)
                {
                    receiverSystem.StepFrequencyCoarse(
                        -1,
                        "Player coarse radio control"
                    );
                }
                else
                {
                    StepDown();
                }
            }

            if (Input.GetKeyDown(frequencyUpKey))
            {
                if (coarse)
                {
                    receiverSystem.StepFrequencyCoarse(
                        1,
                        "Player coarse radio control"
                    );
                }
                else
                {
                    StepUp();
                }
            }

            if (responseSystem != null &&
                responseSystem.IsWaiting)
            {
                responseTimerText.text =
                    $"Response Window: " +
                    $"{responseSystem.RemainingSeconds:0.0}s";
            }
        }

        private void OnBroadcastStarted(
            BroadcastStartedEvent startedEvent)
        {
            BroadcastDefinition broadcast =
                startedEvent.Broadcast;

            speakerText.text =
                broadcast.Speaker != null
                    ? broadcast.Speaker.DisplayName
                    : "Unknown";

            transcriptText.text =
                "...";
        }

        private void OnBroadcastSegmentStarted(
            BroadcastSegmentStartedEvent segmentEvent)
        {
            BroadcastDefinition broadcast =
                segmentEvent.Broadcast;

            if (segmentEvent.SegmentIndex < 0 ||
                segmentEvent.SegmentIndex >=
                broadcast.TranscriptSegments.Count)
            {
                return;
            }

            BroadcastTranscriptSegment segment =
                broadcast.TranscriptSegments[
                    segmentEvent.SegmentIndex
                ];

            if (segment == null)
                return;

            if (radioAudioSource != null)
            {
                radioAudioSource.Stop();

                if (segment.AudioClip != null)
                {
                    radioAudioSource.clip =
                        segment.AudioClip;

                    radioAudioSource.Play();
                }
            }

            speakerText.text =
                broadcast.Speaker != null
                    ? broadcast.Speaker.DisplayName
                    : "Unknown";

            transcriptText.text =
                segment.Text;
        }

        private void OnBroadcastInterrupted(
            BroadcastInterruptedEvent interruptedEvent)
        {
            radioAudioSource?.Stop();

            transcriptText.text =
                "[SIGNAL LOST]";
        }

        private void OnBroadcastCompleted(
            BroadcastPlaybackCompletedEvent completedEvent)
        {
            transcriptText.text =
                "[TRANSMISSION COMPLETE]";
        }

        private void StepDown()
        {
            receiverSystem.StepFrequency(
                -1,
                "Player radio control"
            );
        }

        private void StepUp()
        {
            receiverSystem.StepFrequency(
                1,
                "Player radio control"
            );
        }

        private void OnResponseWindowOpened(
    RadioResponseWindowOpenedEvent openedEvent)
        {
            BuildResponseOptions(
                openedEvent.Broadcast
            );
        }

        private void BuildResponseOptions(
            BroadcastDefinition broadcast)
        {
            ClearResponseButtons();

            responseArea.SetActive(true);

            foreach (RadioResponseDefinition response
                     in broadcast.ResponseOptions)
            {
                if (response == null)
                    continue;

                RadioResponseButtonView button =
                    Instantiate(
                        responseButtonPrefab,
                        responseButtonContainer
                    );

                bool available =
                    responseSystem.IsResponseAvailable(
                        response
                    );

                button.Initialize(
                    response,
                    available,
                    SelectResponse
                );

                responseButtons.Add(button);
            }
        }

        private void SelectResponse(
            RadioResponseDefinition response)
        {
            responseSystem.SelectResponse(response);
        }

        private void OnResponseSelected(
            RadioResponseSelectedEvent selectedEvent)
        {
            transcriptText.text =
                $"[RESPONSE SENT] " +
                $"{selectedEvent.Response.DisplayText}";

            HideResponseArea();
        }

        private void OnResponseExpired(
            RadioResponseExpiredEvent expiredEvent)
        {
            transcriptText.text =
                "[NO RESPONSE SENT]";

            HideResponseArea();
        }

        private void HideResponseArea()
        {
            responseArea.SetActive(false);
            ClearResponseButtons();
        }

        private void ClearResponseButtons()
        {
            foreach (RadioResponseButtonView button
                     in responseButtons)
            {
                if (button != null)
                {
                    Destroy(
                        button.gameObject
                    );
                }
            }

            responseButtons.Clear();
        }

        private void OnReceiverUpdated(
            RadioReceiverUpdatedEvent receiverEvent)
        {
            Refresh();
        }

        private void OnFrequencyChanged(
            RadioFrequencyChangedEvent frequencyEvent)
        {
            Refresh();
        }

        private void OnSignalTuned(
            RadioSignalTunedEvent tunedEvent)
        {
            Refresh();
        }

        private void OnSignalUntuned(
            RadioSignalUntunedEvent untunedEvent)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (receiverSystem == null)
                return;

            receiverStateText.text =
                receiverSystem.IsReceiverOperational
                    ? "RECEIVER: ONLINE"
                    : "RECEIVER: OFFLINE";

            frequencyText.text =
                $"{receiverSystem.CurrentFrequencyMHz:0.0} MHz";

            RadioSignalRuntimeState tuned =
                receiverSystem.TunedSignal;

            if (tuned == null)
            {
                RadioSignalRuntimeState nearest =
                    receiverSystem
                        .GetNearestReceivableSignal();

                if (nearest == null)
                {
                    signalNameText.text =
                        "NO SIGNAL";

                    signalStrengthText.text =
                        "Strength: 0%";

                    staticText.text =
                        "Static: 100%";

                    signalStatusText.text =
                        "SEARCHING";

                    return;
                }

                float proximity =
                    receiverSystem.GetFrequencyProximity(
                        nearest
                    );

                float apparentStrength =
                    nearest.CurrentStrength *
                    proximity;

                signalNameText.text =
                    proximity > 0.25f
                        ? "UNIDENTIFIED CARRIER"
                        : "NO SIGNAL LOCK";

                signalStrengthText.text =
                    $"Strength: " +
                    $"{apparentStrength * 100f:0}%";

                staticText.text =
                    $"Static: " +
                    $"{(1f - apparentStrength) * 100f:0}%";

                signalStatusText.text =
                    "SEARCHING";

                return;
            }
        }

        private void OnDestroy()
        {
            frequencyDownButton?.onClick.RemoveListener(
                StepDown
            );

            frequencyUpButton?.onClick.RemoveListener(
                StepUp
            );

            eventBus?.Unsubscribe<
                RadioReceiverUpdatedEvent>(
                OnReceiverUpdated
            );

            eventBus?.Unsubscribe<
                RadioFrequencyChangedEvent>(
                OnFrequencyChanged
            );

            eventBus?.Unsubscribe<
                RadioSignalTunedEvent>(
                OnSignalTuned
            );

            eventBus?.Unsubscribe<
                RadioSignalUntunedEvent>(
                OnSignalUntuned
            );

            eventBus?.Unsubscribe<
                BroadcastStartedEvent>(
                OnBroadcastStarted
            );

            eventBus?.Unsubscribe<
                BroadcastSegmentStartedEvent>(
                OnBroadcastSegmentStarted
            );

            eventBus?.Unsubscribe<
                BroadcastInterruptedEvent>(
                OnBroadcastInterrupted
            );

            eventBus?.Unsubscribe<
                BroadcastPlaybackCompletedEvent>(
                OnBroadcastCompleted
            );
        }
    }
}
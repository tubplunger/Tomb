using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tomb.Core.Events;
using Tomb.Core.Services;
using Tomb.Gameplay.Radio;

namespace Tomb.Presentation.Radio
{
    public sealed class RadioConsoleUI :
        MonoBehaviour
    {
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

        private EventBus eventBus;
        private RadioReceiverSystem receiverSystem;

        private void Start()
        {
            try
            {
                eventBus =
                    CoreServices.Get<EventBus>();

                receiverSystem =
                    CoreServices.Get<RadioReceiverSystem>();
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
        }
    }
}
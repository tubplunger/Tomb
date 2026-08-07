using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Services;

namespace Tomb.Gameplay.Radio
{
    public sealed class DEV_RadioReceiverControls :
        MonoBehaviour
    {
        private RadioReceiverSystem receiverSystem;

        private IReadOnlyList<RadioSignalRuntimeState>
            signals;

        private int selectedIndex;

        private void Start()
        {
            receiverSystem =
                CoreServices.Get<RadioReceiverSystem>();

            signals = receiverSystem.Signals;

            LogSelection();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SelectPrevious();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                SelectNext();
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                TuneSelected();
            }

            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                receiverSystem.Untune();
            }
        }

        private void SelectNext()
        {
            if (signals == null || signals.Count == 0)
                return;

            selectedIndex =
                (selectedIndex + 1) %
                signals.Count;

            LogSelection();
        }

        private void SelectPrevious()
        {
            if (signals == null || signals.Count == 0)
                return;

            selectedIndex--;

            if (selectedIndex < 0)
                selectedIndex =
                    signals.Count - 1;

            LogSelection();
        }

        private void TuneSelected()
        {
            if (signals == null || signals.Count == 0)
                return;

            RadioSignalRuntimeState selected =
                signals[selectedIndex];

            bool tuned =
                receiverSystem.TuneSignal(
                    selected.Definition.SignalId
                );

            Debug.Log(
                tuned
                    ? $"[DEV Radio] Tuned: " +
                      $"{selected.Definition.DisplayName}"
                    : $"[DEV Radio] Could not tune: " +
                      $"{selected.Definition.DisplayName}"
            );
        }

        private void LogSelection()
        {
            if (signals == null || signals.Count == 0)
                return;

            RadioSignalRuntimeState selected =
                signals[selectedIndex];

            Debug.Log(
                $"[DEV Radio] Selected: " +
                $"{selected.Definition.DisplayName} | " +
                $"{selected.Definition.FrequencyMHz:0.0} MHz | " +
                $"{selected.Status}"
            );
        }
    }
}
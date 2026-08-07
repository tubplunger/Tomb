using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Gameplay.Radio
{
    public readonly struct RadioSignalDetectedEvent :
        IGameEvent
    {
        public readonly RadioSignalDefinition Signal;
        public readonly float Strength;

        public RadioSignalDetectedEvent(
            RadioSignalDefinition signal,
            float strength)
        {
            Signal = signal;
            Strength = strength;
        }
    }

    public readonly struct RadioSignalDetectionLostEvent :
        IGameEvent
    {
        public readonly RadioSignalDefinition Signal;

        public RadioSignalDetectionLostEvent(
            RadioSignalDefinition signal)
        {
            Signal = signal;
        }
    }

    public readonly struct RadioSignalTunedEvent :
        IGameEvent
    {
        public readonly RadioSignalDefinition Signal;

        public RadioSignalTunedEvent(
            RadioSignalDefinition signal)
        {
            Signal = signal;
        }
    }

    public readonly struct RadioSignalUntunedEvent :
        IGameEvent
    {
        public readonly RadioSignalDefinition Signal;

        public RadioSignalUntunedEvent(
            RadioSignalDefinition signal)
        {
            Signal = signal;
        }
    }

    public readonly struct RadioReceiverUpdatedEvent :
        IGameEvent
    {
        public readonly int DetectedSignalCount;
        public readonly string TunedSignalId;

        public RadioReceiverUpdatedEvent(
            int detectedSignalCount,
            string tunedSignalId)
        {
            DetectedSignalCount =
                detectedSignalCount;

            TunedSignalId =
                tunedSignalId;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Radio
{
    public sealed class RadioSignalRuntimeState
    {
        public RadioSignalDefinition Definition { get; }

        public RadioSignalReceiverStatus Status
        {
            get;
            internal set;
        }

        public bool HasBeenDetected
        {
            get;
            internal set;
        }

        public float CurrentStrength
        {
            get;
            internal set;
        }

        public float StaticAmount
        {
            get;
            internal set;
        }

        public bool IsTuned
        {
            get;
            internal set;
        }

        public bool IsReceivable =>
            Status == RadioSignalReceiverStatus.Available ||
            Status == RadioSignalReceiverStatus.Detected ||
            Status == RadioSignalReceiverStatus.Tuned;

        public RadioSignalRuntimeState(
            RadioSignalDefinition definition)
        {
            Definition = definition;

            Status =
                RadioSignalReceiverStatus.Unavailable;
        }
    }
}
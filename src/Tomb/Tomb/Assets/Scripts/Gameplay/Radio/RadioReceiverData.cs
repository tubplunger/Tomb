using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Gameplay.Radio
{
    [Serializable]
    public sealed class RadioReceiverSaveState
    {
        public List<string> detectedSignalIds =
            new();

        public string tunedSignalId;

        public float currentFrequencyMHz;
    }
}
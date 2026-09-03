using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Gameplay.Radio.Responses
{
    [Serializable]
    public sealed class RadioResponseSaveState
    {
        public string activeBroadcastId;

        public float remainingSeconds;

        public bool waitingForResponse;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    [Serializable]
    public sealed class BroadcastSaveState
    {
        public List<BroadcastSaveEntry> broadcasts =
            new();
    }

    [Serializable]
    public sealed class BroadcastSaveEntry
    {
        public string broadcastId;
        public int playCount;
        public bool hasCompleted;
    }
}
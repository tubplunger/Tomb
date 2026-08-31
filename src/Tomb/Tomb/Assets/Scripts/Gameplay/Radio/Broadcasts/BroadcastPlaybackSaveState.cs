using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    [Serializable]
    public sealed class BroadcastPlaybackSaveState
    {
        public string activeBroadcastId;

        public int currentSegmentIndex;

        public float segmentElapsedSeconds;

        public bool wasPlaying;
    }
}
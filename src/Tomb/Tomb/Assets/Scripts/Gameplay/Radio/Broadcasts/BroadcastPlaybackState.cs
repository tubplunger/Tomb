using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    public enum BroadcastPlaybackState
    {
        Idle,
        WaitingForSegment,
        PlayingSegment,
        Interrupted,
        Completed
    }
}
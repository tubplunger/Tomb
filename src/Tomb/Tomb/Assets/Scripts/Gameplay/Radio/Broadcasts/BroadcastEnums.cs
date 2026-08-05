using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    public enum BroadcastPriority
    {
        Background = 0,
        Normal = 100,
        Important = 200,
        Critical = 300
    }

    public enum BroadcastRepeatMode
    {
        Never,
        Limited,
        Unlimited
    }

    public enum BroadcastRuntimeStatus
    {
        Eligible,
        SignalUnavailable,
        TooEarly,
        Expired,
        RequiredFlagsMissing,
        BlockedByFlag,
        PlayLimitReached,
        Disabled
    }
}
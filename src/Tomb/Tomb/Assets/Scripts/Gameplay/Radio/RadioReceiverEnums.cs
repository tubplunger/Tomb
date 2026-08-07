using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Radio
{
    public enum RadioSignalReceiverStatus
    {
        Unavailable,
        Available,
        Detected,
        Tuned,
        TooWeak,
        ReceiverOffline
    }
}
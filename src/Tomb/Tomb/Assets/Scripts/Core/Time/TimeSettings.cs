using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Core.Time
{
    [CreateAssetMenu(fileName = "TimeSettings", menuName = "Tomb/Settings/Time Settings")]
    public sealed class TimeSettings : ScriptableObject
    {
        [Header("Game Time")]
        [Min(0.01f)] public float realSecondsPerGameMinute = 1f;

        [Header("Ticking")]
        [Min(0.01f)] public float tickIntervalRealSeconds = 1f;

        [Header("Start Time")]
        [Range(0, 23)] public int startingHour = 8;
        [Range(0, 59)] public int startingMinute = 0;
        [Min(1)] public int startingDay = 1;
    }
}

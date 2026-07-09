using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Core.Time
{
    [Serializable]
    public sealed class GameTimeSaveState
    {
        public int day;
        public int hour;
        public int minute;
        public bool isPaused;
        public float timeScale;
    }
}
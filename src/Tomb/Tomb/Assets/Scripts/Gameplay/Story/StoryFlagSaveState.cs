using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Gameplay.Story
{
    [Serializable]
    public sealed class StoryFlagSaveState
    {
        public List<string> activeFlags = new();
    }
}

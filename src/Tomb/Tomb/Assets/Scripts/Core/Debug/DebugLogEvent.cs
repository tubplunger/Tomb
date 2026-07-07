using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Core.Debugging
{
    public readonly struct DebugLogEvent : IGameEvent
    {
        public readonly string Message;
        public readonly string Category;

        public DebugLogEvent(string message, string category = "General")
        {
            Message = message;
            Category = category;
        }
    }
}

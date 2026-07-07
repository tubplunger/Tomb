using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Core.Bootstrap
{
    public readonly struct ProjectInitializedEvent : IGameEvent
    {
        public readonly string Message;

        public ProjectInitializedEvent(string message)
        {
            Message = message;
        }
    }
}

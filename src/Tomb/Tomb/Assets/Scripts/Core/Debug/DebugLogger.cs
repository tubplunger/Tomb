using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Events;

namespace Tomb.Core.Debugging
{
    public sealed class DebugLogger
    {
        private readonly EventBus eventBus;

        public DebugLogger(EventBus eventBus)
        {
            this.eventBus = eventBus;
            this.eventBus.Subscribe<DebugLogEvent>(OnDebugLog);
        }

        public void Log(string message, string category = "General")
        {
            eventBus.Publish(new DebugLogEvent(message, category));
        }

        private void OnDebugLog(DebugLogEvent logEvent)
        {
            UnityEngine.Debug.Log($"[{logEvent.Category}] {logEvent.Message}");
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<DebugLogEvent>(OnDebugLog);
        }
    }
}

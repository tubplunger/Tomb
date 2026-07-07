using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Core.Events
{
    public sealed class EventBus
    {
        private readonly Dictionary<Type, Delegate> eventTable = new();

        public void Subscribe<T>(Action<T> listener) where T : IGameEvent
        {
            Type eventType = typeof(T);

            if (eventTable.TryGetValue(eventType, out Delegate existingDelegate))
            {
                eventTable[eventType] = Delegate.Combine(existingDelegate, listener);
            }
            else
            {
                eventTable[eventType] = listener;
            }
        }

        public void Unsubscribe<T>(Action<T> listener) where T : IGameEvent
        {
            Type eventType = typeof(T);

            if (!eventTable.TryGetValue(eventType, out Delegate existingDelegate))
                return;

            Delegate updatedDelegate = Delegate.Remove(existingDelegate, listener);

            if (updatedDelegate == null)
            {
                eventTable.Remove(eventType);
            }
            else
            {
                eventTable[eventType] = updatedDelegate;
            }
        }

        public void Publish<T>(T gameEvent) where T : IGameEvent
        {
            Type eventType = typeof(T);

            if (eventTable.TryGetValue(eventType, out Delegate existingDelegate))
            {
                if (existingDelegate is Action<T> callback)
                {
                    callback.Invoke(gameEvent);
                }
            }
        }

        public void Clear()
        {
            eventTable.Clear();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Core.Events
{
    public sealed class EventBus
    {
        private readonly Dictionary<Type, Delegate> eventTable = new();

        public event Action<IGameEvent> EventPublished;

        public void Subscribe<T>(Action<T> listener) where T : IGameEvent
        {
            if (listener == null)
                return;

            Type eventType = typeof(T);

            if (eventTable.TryGetValue(eventType, out Delegate existingDelegate))
            {
                eventTable[eventType] =
                    Delegate.Combine(existingDelegate, listener);
            }
            else
            {
                eventTable[eventType] = listener;
            }
        }

        public void Unsubscribe<T>(Action<T> listener) where T : IGameEvent
        {
            if (listener == null)
                return;

            Type eventType = typeof(T);

            if (!eventTable.TryGetValue(eventType, out Delegate existingDelegate))
                return;

            Delegate updatedDelegate =
                Delegate.Remove(existingDelegate, listener);

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
            if (gameEvent == null)
                return;

            NotifyObservers(gameEvent);

            Type eventType = typeof(T);

            if (!eventTable.TryGetValue(eventType, out Delegate existingDelegate))
                return;

            if (existingDelegate is not Action<T> callbacks)
                return;

            foreach (Delegate callbackDelegate in callbacks.GetInvocationList())
            {
                try
                {
                    ((Action<T>)callbackDelegate).Invoke(gameEvent);
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogException(exception);
                }
            }
        }

        private void NotifyObservers(IGameEvent gameEvent)
        {
            if (EventPublished == null)
                return;

            foreach (Delegate observerDelegate in EventPublished.GetInvocationList())
            {
                try
                {
                    ((Action<IGameEvent>)observerDelegate).Invoke(gameEvent);
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogException(exception);
                }
            }
        }

        public void Clear()
        {
            eventTable.Clear();
            EventPublished = null;
        }
    }
}
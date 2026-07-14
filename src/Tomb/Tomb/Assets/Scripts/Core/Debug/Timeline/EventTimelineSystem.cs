using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Tomb.Core.Events;

namespace Tomb.Core.Debugging.Timeline
{
    public sealed class EventTimelineSystem
    {
        private const int DefaultMaximumEntries = 500;

        private readonly EventBus eventBus;
        private readonly List<EventTimelineEntry> entries = new();

        private long nextSequenceNumber = 1;
        private int maximumEntries;

        public IReadOnlyList<EventTimelineEntry> Entries => entries;

        public event Action TimelineChanged;

        public EventTimelineSystem(
            EventBus eventBus,
            int maximumEntries = DefaultMaximumEntries)
        {
            this.eventBus = eventBus;
            this.maximumEntries = Math.Max(1, maximumEntries);

            this.eventBus.EventPublished += OnEventPublished;
        }

        public void Clear()
        {
            entries.Clear();
            TimelineChanged?.Invoke();
        }

        public void SetMaximumEntries(int newMaximum)
        {
            maximumEntries = Math.Max(1, newMaximum);
            TrimOldEntries();

            TimelineChanged?.Invoke();
        }

        private void OnEventPublished(IGameEvent gameEvent)
        {
            string eventType = gameEvent.GetType().Name;
            string details = EventTimelineFormatter.Format(gameEvent);

            EventTimelineEntry entry = new EventTimelineEntry(
                nextSequenceNumber,
                DateTime.UtcNow,
                eventType,
                details
            );

            nextSequenceNumber++;

            entries.Add(entry);
            TrimOldEntries();

            TimelineChanged?.Invoke();
        }

        private void TrimOldEntries()
        {
            int overflow = entries.Count - maximumEntries;

            if (overflow > 0)
            {
                entries.RemoveRange(0, overflow);
            }
        }

        public void Dispose()
        {
            eventBus.EventPublished -= OnEventPublished;
            TimelineChanged = null;
        }
    }
}
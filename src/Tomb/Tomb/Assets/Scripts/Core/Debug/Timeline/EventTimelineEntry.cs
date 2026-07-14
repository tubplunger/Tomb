using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Core.Debugging.Timeline
{
    public sealed class EventTimelineEntry
    {
        public long SequenceNumber { get; }
        public DateTime RecordedAtUtc { get; }
        public string EventType { get; }
        public string EventDetails { get; }

        public EventTimelineEntry(
            long sequenceNumber,
            DateTime recordedAtUtc,
            string eventType,
            string eventDetails)
        {
            SequenceNumber = sequenceNumber;
            RecordedAtUtc = recordedAtUtc;
            EventType = eventType;
            EventDetails = eventDetails;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Tomb.Core.Debugging.Timeline
{
    public sealed class DebugEventTimelineEntryView : MonoBehaviour
    {
        [SerializeField] private TMP_Text entryText;

        public void Display(EventTimelineEntry entry)
        {
            if (entry == null || entryText == null)
                return;

            string recordedTime =
                entry.RecordedAtUtc.ToLocalTime().ToString("HH:mm:ss.fff");

            string details = string.IsNullOrWhiteSpace(entry.EventDetails)
                ? string.Empty
                : $" | {entry.EventDetails}";

            entryText.text =
                $"#{entry.SequenceNumber:0000} | " +
                $"{recordedTime} | " +
                $"{entry.EventType}" +
                details;
        }
    }
}
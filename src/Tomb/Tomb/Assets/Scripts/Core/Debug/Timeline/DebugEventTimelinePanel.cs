using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Tomb.Core.Services;

namespace Tomb.Core.Debugging.Timeline
{
    public sealed class DebugEventTimelinePanel : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Transform entryContainer;
        [SerializeField] private DebugEventTimelineEntryView entryPrefab;
        [SerializeField] private TMP_Text eventCountText;
        [SerializeField] private Button clearButton;
        [SerializeField] private ScrollRect scrollRect;

        private readonly List<DebugEventTimelineEntryView> spawnedEntries = new();

        private EventTimelineSystem timelineSystem;
        private bool refreshQueued;

        private void Start()
        {
            timelineSystem = CoreServices.Get<EventTimelineSystem>();

            timelineSystem.TimelineChanged += QueueRefresh;
            clearButton.onClick.AddListener(ClearTimeline);

            Refresh();
        }

        private void OnEnable()
        {
            refreshQueued = true;
        }

        private void LateUpdate()
        {
            if (!refreshQueued)
                return;

            refreshQueued = false;
            Refresh();
        }

        private void OnDestroy()
        {
            if (timelineSystem != null)
            {
                timelineSystem.TimelineChanged -= QueueRefresh;
            }

            clearButton?.onClick.RemoveListener(ClearTimeline);
        }

        private void QueueRefresh()
        {
            refreshQueued = true;
        }

        private void Refresh()
        {
            if (timelineSystem == null)
                return;

            EnsureEntryCount(timelineSystem.Entries.Count);

            for (int i = 0; i < timelineSystem.Entries.Count; i++)
            {
                spawnedEntries[i].gameObject.SetActive(true);
                spawnedEntries[i].Display(timelineSystem.Entries[i]);
            }

            for (int i = timelineSystem.Entries.Count;
                 i < spawnedEntries.Count;
                 i++)
            {
                spawnedEntries[i].gameObject.SetActive(false);
            }

            eventCountText.text =
                $"Events: {timelineSystem.Entries.Count}";

            Canvas.ForceUpdateCanvases();

            if (entryContainer is RectTransform containerRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
            }

            Canvas.ForceUpdateCanvases();

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private void EnsureEntryCount(int requiredCount)
        {
            while (spawnedEntries.Count < requiredCount)
            {
                DebugEventTimelineEntryView entry =
                    Instantiate(entryPrefab, entryContainer);

                spawnedEntries.Add(entry);
            }
        }

        private void ClearTimeline()
        {
            timelineSystem.Clear();
        }
    }
}
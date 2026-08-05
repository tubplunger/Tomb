using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Tomb.Core.Debugging.Lists;
using Tomb.Core.Events;
using Tomb.Core.Services;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    public sealed class DebugBroadcastsPanel :
        DebugListPanelBase
    {
        [Header("Rows")]
        [SerializeField]
        private DebugBroadcastRowView rowPrefab;

        [Header("Summary")]
        [SerializeField]
        private TMP_Text eligibleCountText;

        private readonly List<DebugBroadcastRowView>
            rows = new();

        private EventBus eventBus;
        private BroadcastLibrarySystem librarySystem;

        protected override void InitializePanel()
        {
            eventBus =
                CoreServices.Get<EventBus>();

            librarySystem =
                CoreServices.Get<
                    BroadcastLibrarySystem>();

            eventBus.Subscribe<
                BroadcastEligibilityUpdatedEvent>(
                OnEligibilityUpdated
            );

            eventBus.Subscribe<
                BroadcastCompletionRecordedEvent>(
                OnCompletionRecorded
            );

            BuildRows();
        }

        protected override void RefreshList()
        {
            foreach (DebugBroadcastRowView row in rows)
            {
                row.Refresh();
            }

            eligibleCountText.text =
                $"Eligible: " +
                $"{librarySystem.EligibleBroadcasts.Count} / " +
                $"{librarySystem.Broadcasts.Count}";

            RebuildListLayout();
        }

        private void BuildRows()
        {
            foreach (BroadcastRuntimeState broadcast
                     in librarySystem.Broadcasts)
            {
                DebugBroadcastRowView row =
                    SpawnRow(rowPrefab);

                if (row == null)
                    continue;

                row.Initialize(broadcast);
                rows.Add(row);
            }

            RebuildListLayout();
        }

        private void OnEligibilityUpdated(
            BroadcastEligibilityUpdatedEvent updateEvent)
        {
            QueueRefresh();
        }

        private void OnCompletionRecorded(
            BroadcastCompletionRecordedEvent completionEvent)
        {
            QueueRefresh();
        }

        private void OnDestroy()
        {
            eventBus?.Unsubscribe<
                BroadcastEligibilityUpdatedEvent>(
                OnEligibilityUpdated
            );

            eventBus?.Unsubscribe<
                BroadcastCompletionRecordedEvent>(
                OnCompletionRecorded
            );
        }
    }
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Tomb.Core.Events;
using Tomb.Core.Services;
using Tomb.Core.Debugging.Lists;

namespace Tomb.Gameplay.Resources
{
    public sealed class DebugResourcesPanel
        : DebugListPanelBase
    {
        [Header("Resources")]
        [SerializeField]
        private DebugResourceRowView rowPrefab;

        [SerializeField]
        private TMP_Text resourceCountText;

        private readonly List<DebugResourceRowView> rows = new();

        private EventBus eventBus;
        private ResourceSystem resourceSystem;

        protected override void InitializePanel()
        {
            eventBus = CoreServices.Get<EventBus>();
            resourceSystem = CoreServices.Get<ResourceSystem>();

            eventBus.Subscribe<ResourceChangedEvent>(
                OnResourceChanged
            );

            BuildRows();
        }

        protected override void RefreshList()
        {
            foreach (DebugResourceRowView row in rows)
            {
                row.Refresh();
            }

            RebuildListLayout();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        private void OnDestroy()
        {
            eventBus?.Unsubscribe<ResourceChangedEvent>(
                OnResourceChanged
            );
        }

        private void BuildRows()
        {
            foreach (ResourceState resource
                     in resourceSystem.Resources)
            {
                DebugResourceRowView row =
                    SpawnRow(rowPrefab);

                if (row == null)
                    continue;

                row.Initialize(resource);
                rows.Add(row);
            }

            resourceCountText.text =
                $"Resources: {rows.Count}";

            RebuildListLayout();
        }

        private void OnResourceChanged(
            ResourceChangedEvent resourceChangedEvent)
        {
            QueueRefresh();
        }
    }
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Tomb.Core.Events;
using Tomb.Core.Services;

namespace Tomb.Gameplay.Resources
{
    public sealed class DebugResourcesPanel : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField]
        private Transform rowContainer;

        [SerializeField]
        private DebugResourceRowView rowPrefab;

        [SerializeField]
        private TMP_Text resourceCountText;

        private readonly List<DebugResourceRowView>
            rows = new();

        private EventBus eventBus;
        private ResourceSystem resourceSystem;

        private bool refreshQueued;

        private void Start()
        {
            eventBus =
                CoreServices.Get<EventBus>();

            resourceSystem =
                CoreServices.Get<ResourceSystem>();

            eventBus.Subscribe<ResourceChangedEvent>(
                OnResourceChanged
            );

            BuildRows();
            RefreshRows();
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
            RefreshRows();
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
                    Instantiate(
                        rowPrefab,
                        rowContainer
                    );

                row.Initialize(resource);
                rows.Add(row);
            }

            resourceCountText.text =
                $"Resources: {rows.Count}";

            RebuildLayout();
        }

        private void OnResourceChanged(
            ResourceChangedEvent resourceChangedEvent)
        {
            refreshQueued = true;
        }

        private void RefreshRows()
        {
            foreach (DebugResourceRowView row in rows)
            {
                row.Refresh();
            }

            RebuildLayout();
        }

        private void RebuildLayout()
        {
            Canvas.ForceUpdateCanvases();

            if (rowContainer is RectTransform rectTransform)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    rectTransform
                );
            }
        }
    }
}
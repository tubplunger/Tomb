using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tomb.Core.Debugging.Lists
{
    public abstract class DebugListPanelBase : MonoBehaviour
    {
        [Header("List")]
        [SerializeField]
        private Transform rowContainer;

        private bool initialized;
        private bool refreshQueued;

        protected Transform RowContainer => rowContainer;

        protected virtual void Start()
        {
            InitializePanel();

            initialized = true;
            refreshQueued = true;
        }

        protected virtual void OnEnable()
        {
            if (initialized)
            {
                refreshQueued = true;
            }
        }

        protected virtual void LateUpdate()
        {
            if (!initialized || !refreshQueued)
                return;

            refreshQueued = false;
            RefreshList();
        }

        protected abstract void InitializePanel();

        protected abstract void RefreshList();

        protected void QueueRefresh()
        {
            refreshQueued = true;
        }

        protected T SpawnRow<T>(T rowPrefab)
            where T : Component
        {
            if (rowPrefab == null)
            {
                Debug.LogError(
                    $"[{GetType().Name}] Missing row prefab."
                );

                return null;
            }

            if (rowContainer == null)
            {
                Debug.LogError(
                    $"[{GetType().Name}] Missing row container."
                );

                return null;
            }

            return Instantiate(rowPrefab, rowContainer);
        }

        protected void RebuildListLayout()
        {
            Canvas.ForceUpdateCanvases();

            if (rowContainer is RectTransform containerRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    containerRect
                );
            }

            Canvas.ForceUpdateCanvases();
        }
    }
}
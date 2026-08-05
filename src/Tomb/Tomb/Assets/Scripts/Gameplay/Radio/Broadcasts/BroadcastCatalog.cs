using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    [CreateAssetMenu(
        fileName = "BroadcastCatalog",
        menuName = "Tomb/Radio/Broadcast Catalog"
    )]
    public sealed class BroadcastCatalog : ScriptableObject
    {
        [SerializeField]
        private List<BroadcastDefinition> broadcasts =
            new();

        public IReadOnlyList<BroadcastDefinition>
            Broadcasts => broadcasts;

        public BroadcastDefinition FindById(
            string broadcastId)
        {
            if (string.IsNullOrWhiteSpace(broadcastId))
                return null;

            foreach (BroadcastDefinition broadcast
                     in broadcasts)
            {
                if (broadcast != null &&
                    broadcast.BroadcastId == broadcastId)
                {
                    return broadcast;
                }
            }

            return null;
        }

        private void OnValidate()
        {
            HashSet<string> usedIds = new();

            foreach (BroadcastDefinition broadcast
                     in broadcasts)
            {
                if (broadcast == null)
                    continue;

                if (string.IsNullOrWhiteSpace(
                        broadcast.BroadcastId))
                {
                    Debug.LogWarning(
                        $"Broadcast '{broadcast.name}' " +
                        "has no Broadcast ID.",
                        broadcast
                    );

                    continue;
                }

                if (!usedIds.Add(
                        broadcast.BroadcastId))
                {
                    Debug.LogWarning(
                        $"Duplicate Broadcast ID: " +
                        $"{broadcast.BroadcastId}",
                        broadcast
                    );
                }
            }
        }
    }
}
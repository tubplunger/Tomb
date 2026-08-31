using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    public sealed class BroadcastRuntimeState
    {
        public BroadcastDefinition Definition { get; }

        public int PlayCount { get; private set; }

        public bool HasCompleted { get; private set; }

        public BroadcastRuntimeStatus Status
        {
            get;
            internal set;
        }

        public string StatusReason
        {
            get;
            internal set;
        }

        public bool IsEligible =>
            Status == BroadcastRuntimeStatus.Eligible;

        public float RuntimeCooldownSeconds
        {
            get;
            internal set;
        }

        public BroadcastRuntimeState(
            BroadcastDefinition definition)
        {
            Definition = definition;
            Status = BroadcastRuntimeStatus.Disabled;
            StatusReason = "Not yet evaluated";
        }

        public void RecordCompletion()
        {
            PlayCount++;
            HasCompleted = true;
        }

        public void Restore(
            int playCount,
            bool hasCompleted)
        {
            PlayCount =
                UnityEngine.Mathf.Max(0, playCount);

            HasCompleted = hasCompleted;
        }
    }
}
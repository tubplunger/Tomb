using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Gameplay.Radio.Responses;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    [CreateAssetMenu(
        fileName = "BroadcastDefinition",
        menuName = "Tomb/Radio/Broadcast Definition"
    )]
    public sealed class BroadcastDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string broadcastId;

        [SerializeField]
        private string title;

        [TextArea]
        [SerializeField]
        private string description;

        [Header("Source")]
        [SerializeField]
        private RadioSignalDefinition signal;

        [SerializeField]
        private SpeakerDefinition speaker;

        [Header("Scheduling")]
        [SerializeField]
        private BroadcastPriority priority =
            BroadcastPriority.Normal;

        [Min(1)]
        [SerializeField]
        private int minimumGameDay = 1;

        [Tooltip(
            "Zero means that the broadcast never expires by day."
        )]
        [Min(0)]
        [SerializeField]
        private int maximumGameDay;

        [Header("Repeat Rules")]
        [SerializeField]
        private BroadcastRepeatMode repeatMode =
            BroadcastRepeatMode.Never;

        [Tooltip(
            "Used only when Repeat Mode is Limited."
        )]
        [Min(1)]
        [SerializeField]
        private int maximumPlayCount = 1;

        [Header("Story Requirements")]
        [SerializeField]
        private List<string> requiredFlags = new();

        [SerializeField]
        private List<string> blockedFlags = new();

        [Header("Content")]
        [SerializeField]
        private List<BroadcastTranscriptSegment>
            transcriptSegments = new();

        [Header("Development")]
        [SerializeField]
        private bool enabled = true;

        [Header("Player Response")]
        [SerializeField]
        private bool expectsResponse;

        [Min(0f)]
        [SerializeField]
        private float responseWindowSeconds = 20f;

        [SerializeField]
        private List<RadioResponseDefinition>
            responseOptions = new();

        [SerializeField]
        private List<string>
            flagsSetIfIgnored = new();

        public string BroadcastId => broadcastId;

        public string Title =>
            string.IsNullOrWhiteSpace(title)
                ? broadcastId
                : title;

        public string Description => description;

        public RadioSignalDefinition Signal => signal;

        public SpeakerDefinition Speaker => speaker;

        public BroadcastPriority Priority => priority;

        public int MinimumGameDay =>
            Mathf.Max(1, minimumGameDay);

        public int MaximumGameDay =>
            Mathf.Max(0, maximumGameDay);

        public BroadcastRepeatMode RepeatMode =>
            repeatMode;

        public int MaximumPlayCount =>
            repeatMode == BroadcastRepeatMode.Limited
                ? Mathf.Max(1, maximumPlayCount)
                : repeatMode == BroadcastRepeatMode.Never
                    ? 1
                    : int.MaxValue;

        public IReadOnlyList<string> RequiredFlags =>
            requiredFlags;

        public IReadOnlyList<string> BlockedFlags =>
            blockedFlags;

        public IReadOnlyList<BroadcastTranscriptSegment>
            TranscriptSegments => transcriptSegments;

        public bool Enabled => enabled;

        public bool ExpectsResponse =>
            expectsResponse;

        public float ResponseWindowSeconds =>
            Mathf.Max(
                0f,
                responseWindowSeconds
            );

        public IReadOnlyList<RadioResponseDefinition>
            ResponseOptions => responseOptions;

        public IReadOnlyList<string>
            FlagsSetIfIgnored => flagsSetIfIgnored;

        public float EstimatedDurationSeconds
        {
            get
            {
                float duration = 0f;

                foreach (BroadcastTranscriptSegment segment
                         in transcriptSegments)
                {
                    if (segment == null)
                        continue;

                    duration +=
                        segment.DelayBeforeSeconds +
                        segment.DisplayDurationSeconds;
                }

                return duration;
            }
        }

        private void OnValidate()
        {
            broadcastId = broadcastId?.Trim();
            title = title?.Trim();

            minimumGameDay =
                Mathf.Max(1, minimumGameDay);

            maximumGameDay =
                Mathf.Max(0, maximumGameDay);

            maximumPlayCount =
                Mathf.Max(1, maximumPlayCount);

            CleanFlagList(requiredFlags);
            CleanFlagList(blockedFlags);
        }

        private static void CleanFlagList(
            List<string> flags)
        {
            if (flags == null)
                return;

            for (int i = flags.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(flags[i]))
                {
                    flags.RemoveAt(i);
                    continue;
                }

                flags[i] = flags[i].Trim();
            }
        }
    }
}
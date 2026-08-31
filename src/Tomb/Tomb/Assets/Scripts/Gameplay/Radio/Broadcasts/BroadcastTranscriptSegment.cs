using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    [Serializable]
    public sealed class BroadcastTranscriptSegment
    {
        [TextArea(2, 8)]
        [SerializeField]
        private string text;

        [Min(0f)]
        [SerializeField]
        private float delayBeforeSeconds;

        [Min(0f)]
        [SerializeField]
        private float displayDurationSeconds = 4f;

        [SerializeField]
        private AudioClip audioClip;

        public string Text => text;

        public float DelayBeforeSeconds =>
            Mathf.Max(0f, delayBeforeSeconds);

        public float DisplayDurationSeconds =>
            Mathf.Max(0f, displayDurationSeconds);

        public AudioClip AudioClip => audioClip;

        public float EffectiveDurationSeconds
        {
            get
            {
                if (audioClip != null)
                {
                    return Mathf.Max(
                        displayDurationSeconds,
                        audioClip.length
                    );
                }

                return displayDurationSeconds;
            }
        }
    }
}
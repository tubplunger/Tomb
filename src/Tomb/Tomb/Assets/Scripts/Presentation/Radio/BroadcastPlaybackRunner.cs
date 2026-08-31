using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Services;
using Tomb.Gameplay.Radio.Broadcasts;

namespace Tomb.Presentation.Radio
{
    public sealed class BroadcastPlaybackRunner :
        MonoBehaviour
    {
        private BroadcastPlaybackSystem
            playbackSystem;

        private void Start()
        {
            playbackSystem =
                CoreServices.Get<
                    BroadcastPlaybackSystem>();
        }

        private void Update()
        {
            playbackSystem?.Tick(
                Time.deltaTime
            );
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Services;
using Tomb.Gameplay.Story;

namespace Tomb.Gameplay.Radio.Broadcasts
{
    public sealed class DEV_BroadcastControls :
        MonoBehaviour
    {
        [Header("Test Broadcast")]
        [SerializeField]
        private string broadcastId =
            "na_survivor_first_call";

        [Header("Test Flag")]
        [SerializeField]
        private string storyFlagId =
            "na_survivor_first_call_completed";

        private BroadcastLibrarySystem librarySystem;
        private StoryFlagSystem storyFlagSystem;

        private void Start()
        {
            librarySystem =
                CoreServices.Get<
                    BroadcastLibrarySystem>();

            storyFlagSystem =
                CoreServices.Get<StoryFlagSystem>();

            Debug.Log(
                "[DEV Broadcasts] Controls initialized."
            );
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                librarySystem.RecordCompletion(
                    broadcastId
                );
            }

            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                storyFlagSystem.SetFlag(
                    storyFlagId,
                    true,
                    "Developer broadcast test"
                );
            }

            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                storyFlagSystem.SetFlag(
                    storyFlagId,
                    false,
                    "Developer broadcast test"
                );
            }
        }
    }
}
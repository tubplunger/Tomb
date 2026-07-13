using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Tomb.Core.Save;
using Tomb.Core.Services;
using Tomb.Core.Time;

namespace Tomb.Core.Debugging.Overlay
{
    public sealed class DebugOverviewPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text currentTimeText;
        [SerializeField] private TMP_Text timeScaleText;
        [SerializeField] private TMP_Text pauseStateText;
        [SerializeField] private TMP_Text savePathText;

        private GameTimeSystem timeSystem;
        private SaveSystem saveSystem;

        private void Start()
        {
            timeSystem = CoreServices.Get<GameTimeSystem>();
            saveSystem = CoreServices.Get<SaveSystem>();
        }

        private void Update()
        {
            if (timeSystem == null)
                return;

            currentTimeText.text =
                $"Current Time: {timeSystem.CurrentTime}";

            timeScaleText.text =
                $"Time Scale: {timeSystem.TimeScale:0.##}x";

            pauseStateText.text =
                $"Paused: {timeSystem.IsPaused}";

            savePathText.text =
                $"Save Path: {saveSystem.SavePath}";
        }
    }
}
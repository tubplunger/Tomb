using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Services;

namespace Tomb.Core.Time
{
    public sealed class DEV_TimeControls : MonoBehaviour
    {
        private GameTimeSystem timeSystem;

        private void Start()
        {
            timeSystem = CoreServices.Get<GameTimeSystem>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (timeSystem.IsPaused)
                    timeSystem.Resume();
                else
                    timeSystem.Pause();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
                timeSystem.SetTimeScale(1f);

            if (Input.GetKeyDown(KeyCode.Alpha2))
                timeSystem.SetTimeScale(5f);

            if (Input.GetKeyDown(KeyCode.Alpha3))
                timeSystem.SetTimeScale(20f);
        }
    }
}
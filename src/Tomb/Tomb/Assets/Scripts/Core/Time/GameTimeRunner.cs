using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Services;

namespace Tomb.Core.Time
{
    public sealed class GameTimeRunner : MonoBehaviour
    {
        private GameTimeSystem timeSystem;

        private void Start()
        {
            timeSystem = CoreServices.Get<GameTimeSystem>();
        }

        private void Update()
        {
            timeSystem.Tick(UnityEngine.Time.deltaTime);
        }
    }
}

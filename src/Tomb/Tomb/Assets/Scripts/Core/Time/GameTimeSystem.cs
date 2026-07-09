using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;

namespace Tomb.Core.Time
{
    public sealed class GameTimeSystem : Tomb.Core.Save.ISaveable
    {
        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;
        private readonly TimeSettings settings;

        private GameTime currentTime;

        private float accumulatedRealSeconds;
        private float tickTimer;

        private bool isPaused;
        private float timeScale = 1f;

        public GameTime CurrentTime => currentTime;
        public bool IsPaused => isPaused;
        public float TimeScale => timeScale;

        public string SaveKey => "game_time";

        public GameTimeSystem(EventBus eventbus, DebugLogger debugLogger, TimeSettings settings)
        {
            this.eventBus = eventbus;
            this.debugLogger = debugLogger;
            this.settings = settings;

            currentTime = new GameTime(
                settings.startingDay,
                settings.startingHour,
                settings.startingMinute
            );

            debugLogger.Log($"Time system initialized at {currentTime}.", "Time");
        }

        public void Tick(float deltaTime)
        {
            if (isPaused)
                return;

            float scaledDeltaTime = deltaTime * timeScale;

            accumulatedRealSeconds += scaledDeltaTime;
            tickTimer += scaledDeltaTime;

            while (accumulatedRealSeconds >= settings.realSecondsPerGameMinute)
            {
                accumulatedRealSeconds -= settings.realSecondsPerGameMinute;
                AdvanceMinute();
            }

            if (tickTimer >= settings.tickIntervalRealSeconds)
            {
                tickTimer = 0f;
                eventBus.Publish(new GameTimeTickEvent(currentTime));
            }
        }

        public void Pause()
        {
            if (isPaused)
                return;

            isPaused = true;
            eventBus.Publish(new GamePausedEvent());
            debugLogger.Log("Game time paused.", "Time");
        }

        public void Resume()
        {
            if (!isPaused)
                return;

            isPaused = false;
            eventBus.Publish(new GameResumedEvent());
            debugLogger.Log("Game time resumed.", "Time");
        }

        public void SetTimeScale(float newTimeScale)
        {
            if (newTimeScale < 0f)
                newTimeScale = 0f;

            timeScale = newTimeScale;
            eventBus.Publish(new GameTimeScaleChangedEvent(timeScale));
            debugLogger.Log($"Game time scale changed to {timeScale}.", "Time");
        }

        private void AdvanceMinute()
        {
            int previousHour = currentTime.Hour;
            int previousDay = currentTime.Day;

            currentTime.AddMinutes(1);

            eventBus.Publish(new GameMinutePassedEvent(currentTime));

            if (currentTime.Hour != previousHour)
            {
                eventBus.Publish(new GameHourPassedEvent(currentTime));
                debugLogger.Log($"Hour passed: {currentTime}", "Time");
            }

            if (currentTime.Day != previousDay)
            {
                eventBus.Publish(new GameDayPassedEvent(currentTime));
                debugLogger.Log($"Day passed: {currentTime}", "Time");
            }
        }

        public object CaptureState()
        {
            return new GameTimeSaveState
            {
                day = currentTime.Day,
                hour = currentTime.Hour,
                minute = currentTime.Minute,
                isPaused = isPaused,
                timeScale = timeScale
            };
        }

        public void RestoreState(object state)
        {
            if (state is not GameTimeSaveState saveState)
                return;

            currentTime = new GameTime(saveState.day, saveState.hour, saveState.minute);
            isPaused = saveState.isPaused;
            timeScale = saveState.timeScale;

            debugLogger.Log($"Time restored to {currentTime}.", "Time");
        }
    }
}

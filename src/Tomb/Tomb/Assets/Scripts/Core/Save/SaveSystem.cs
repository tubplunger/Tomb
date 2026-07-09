using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Tomb.Core.Debugging;
using Tomb.Core.Events;

namespace Tomb.Core.Save
{
    public sealed class SaveSystem
    {
        private const string SaveVersion = "0.1.0";
        private const string SaveFileName = "tomb_save.json";

        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;

        private readonly Dictionary<string, ISaveable> saveables = new();

        public string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public SaveSystem(EventBus eventBus, DebugLogger debugLogger)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;

            this.eventBus.Subscribe<SaveRequestedEvent>(OnSaveRequested);
            this.eventBus.Subscribe<LoadRequestedEvent>(OnLoadRequested);

            debugLogger.Log($"Save system initialized. Path: {SavePath}", "Save");
        }

        public void Register(ISaveable saveable)
        {
            if (saveable == null)
                return;

            if (string.IsNullOrEmpty(saveable.SaveKey))
            {
                debugLogger.Log("Attempted to register saveable with empty Save Key.", "Save");
                return;
            }

            saveables[saveable.SaveKey] = saveable;
            debugLogger.Log($"Registered saveable: {saveable.SaveKey}", "Save");
        }

        public void Unregister(ISaveable saveable)
        {
            if (saveable == null)
                return;

            if (saveables.ContainsKey(saveable.SaveKey))
            {
                saveables.Remove(saveable.SaveKey);
                debugLogger.Log($"Unregistered saveable: {saveable.SaveKey}", "Save");
            }
        }

        public void Save()
        {
            try
            {
                SaveData saveData = new SaveData
                {
                    saveVersion = SaveVersion,
                    savedAt = DateTime.UtcNow.ToString("o")
                };

                foreach (KeyValuePair<string, ISaveable> pair in saveables)
                {
                    object state = pair.Value.CaptureState();

                    if (state == null)
                        continue;

                    string json = JsonUtility.ToJson(state, true);
                    saveData.SetSystemState(pair.Key, json);
                }

                string finalJson = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(SavePath, finalJson);

                debugLogger.Log($"Game saved to {SavePath}", "Save");
                eventBus.Publish(new GameSavedEvent(SavePath));
            }
            catch (Exception exception)
            {
                debugLogger.Log($"Save failed: {exception.Message}", "Save");
                eventBus.Publish(new SaveFailedEvent(exception.Message));
            }
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    string reason = $"No save file found at {SavePath}";
                    debugLogger.Log(reason, "Save");
                    eventBus.Publish(new LoadFailedEvent(reason));
                    return;
                }

                string json = File.ReadAllText(SavePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);

                if (saveData == null)
                {
                    string reason = "Save file could not be parsed.";
                    debugLogger.Log(reason, "Save");
                    eventBus.Publish(new LoadFailedEvent(reason));
                    return;
                }

                foreach (KeyValuePair<string, ISaveable> pair in saveables)
                {
                    if (!saveData.TryGetSystemState(pair.Key, out string systemJson))
                        continue;

                    Type stateType = pair.Value.CaptureState().GetType();
                    object restoredState = JsonUtility.FromJson(systemJson, stateType);

                    pair.Value.RestoreState(restoredState);
                }

                debugLogger.Log($"Game loaded from {SavePath}", "Save");
                eventBus.Publish(new GameLoadedEvent(SavePath));
            }
            catch (Exception exception)
            {
                debugLogger.Log($"Load failed: {exception.Message}", "Save");
                eventBus.Publish(new LoadFailedEvent(exception.Message));
            }
        }

        private void OnSaveRequested(SaveRequestedEvent saveRequestedEvent)
        {
            Save();
        }

        private void OnLoadRequested(LoadRequestedEvent loadRequestedEvent)
        {
            Load();
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<SaveRequestedEvent>(OnSaveRequested);
            eventBus.Unsubscribe<LoadRequestedEvent>(OnLoadRequested);
        }
    }
}

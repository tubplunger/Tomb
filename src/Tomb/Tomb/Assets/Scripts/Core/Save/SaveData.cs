using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Core.Save
{
    [Serializable]
    public sealed class SaveData
    {
        public string saveVersion;
        public string savedAt;
        public List<SystemStateEntry> systemStates = new();

        public void SetSystemState(string key, string json)
        {
            for (int i = 0; i < systemStates.Count; i++)
            {
                if (systemStates[i].key == key)
                {
                    systemStates[i].json = json;
                    return;
                }
            }

            systemStates.Add(new SystemStateEntry
            {
                key = key,
                json = json
            });
        }

        public bool TryGetSystemState(string key, out string json)
        {
            for (int i = 0; i < systemStates.Count; i++)
            {
                if (systemStates[i].key == key)
                {
                    json = systemStates[i].json;
                    return true;
                }
            }

            json = null;
            return false;
        }
    }

    [Serializable]
    public sealed class SystemStateEntry
    {
        public string key;
        public string json;
    }
}
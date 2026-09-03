using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Radio.Responses
{
    [CreateAssetMenu(
        fileName = "RadioResponseDefinition",
        menuName = "Tomb/Radio/Radio Response"
    )]
    public sealed class RadioResponseDefinition :
        ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string responseId;

        [SerializeField]
        private string displayText;

        [Header("Story Effects")]
        [SerializeField]
        private List<string> flagsToSet = new();

        [SerializeField]
        private List<string> flagsToClear = new();

        [Header("Availability")]
        [SerializeField]
        private List<string> requiredFlags = new();

        [SerializeField]
        private List<string> blockedFlags = new();

        [Header("Development")]
        [SerializeField]
        private bool enabled = true;

        public string ResponseId =>
            responseId;

        public string DisplayText =>
            displayText;

        public IReadOnlyList<string>
            FlagsToSet => flagsToSet;

        public IReadOnlyList<string>
            FlagsToClear => flagsToClear;

        public IReadOnlyList<string>
            RequiredFlags => requiredFlags;

        public IReadOnlyList<string>
            BlockedFlags => blockedFlags;

        public bool Enabled => enabled;

        private void OnValidate()
        {
            responseId =
                responseId?.Trim();

            displayText =
                displayText?.Trim();

            CleanList(flagsToSet);
            CleanList(flagsToClear);
            CleanList(requiredFlags);
            CleanList(blockedFlags);
        }

        private static void CleanList(
            List<string> list)
        {
            if (list == null)
                return;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(
                        list[i]))
                {
                    list.RemoveAt(i);
                    continue;
                }

                list[i] =
                    list[i].Trim();
            }
        }
    }
}
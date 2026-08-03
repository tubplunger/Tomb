using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Tomb.Gameplay.Earth;

namespace Tomb.Gameplay.Radio
{
    public sealed class DebugSignalRowView :
        MonoBehaviour
    {
        [SerializeField]
        private TMP_Text signalNameText;

        [SerializeField]
        private TMP_Text sourceRegionText;

        [SerializeField]
        private TMP_Text lightingRequirementText;

        [SerializeField]
        private TMP_Text statusText;

        private RadioSignalDefinition signal;
        private EarthRegionCatalog regionCatalog;
        private RadioVisibilitySystem visibilitySystem;

        public void Initialize(
            RadioSignalDefinition signalDefinition,
            EarthRegionCatalog earthRegionCatalog,
            RadioVisibilitySystem radioSystem)
        {
            signal = signalDefinition;
            regionCatalog = earthRegionCatalog;
            visibilitySystem = radioSystem;

            Refresh();
        }

        public void Refresh()
        {
            if (signal == null)
                return;

            EarthRegionDefinition region =
                regionCatalog.FindById(
                    signal.SourceRegionId
                );

            signalNameText.text =
                signal.DisplayName;

            sourceRegionText.text =
                region != null
                    ? region.DisplayName
                    : signal.SourceRegionId;

            lightingRequirementText.text =
                signal.RequiresSunlight
                    ? "Requires Sunlight"
                    : "Any Lighting";

            statusText.text =
                visibilitySystem.IsSignalVisible(
                    signal.SignalId)
                    ? "VISIBLE"
                    : "HIDDEN";
        }
    }
}
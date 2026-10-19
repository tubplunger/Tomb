using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Tomb.Presentation.Station
{
    public sealed class StationOverviewUI : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField] private TMP_Text oxygenText;
        [SerializeField] private TMP_Text waterText;
        [SerializeField] private TMP_Text batteryText;

        [Header("Orbit")]
        [SerializeField] private TMP_Text orbitText;

        [Header("Station Systems")]
        [SerializeField] private TMP_Text lifeSupportText;
        [SerializeField] private TMP_Text powerGridText;
        [SerializeField] private TMP_Text communicationsText;

        private void Awake()
        {
            ShowWaitingState();
        }

        private void ShowWaitingState()
        {
            SetText(oxygenText, "--");
            SetText(waterText, "--");
            SetText(batteryText, "--");
            SetText(orbitText, "--");

            SetText(lifeSupportText, "CHECKING");
            SetText(powerGridText, "CHECKING");
            SetText(communicationsText, "CHECKING");
        }

        public void DisplayResources(
            float oxygenPercent,
            float waterPercent,
            float batteryPercent)
        {
            SetText(oxygenText,
                $"{oxygenPercent:0}%");

            SetText(waterText,
                $"{waterPercent:0}%");

            SetText(batteryText,
                $"{batteryPercent:0}%");
        }

        public void DisplayOrbit(int completedOrbits)
        {
            SetText(orbitText,
                $"#{completedOrbits + 1}");
        }

        public void DisplaySystemStatuses(
            string lifeSupport,
            string powerGrid,
            string communications)
        {
            SetText(lifeSupportText, lifeSupport);
            SetText(powerGridText, powerGrid);
            SetText(communicationsText, communications);
        }

        private static void SetText(
            TMP_Text target,
            string value)
        {
            if (target != null)
                target.text = value;
        }
    }
}
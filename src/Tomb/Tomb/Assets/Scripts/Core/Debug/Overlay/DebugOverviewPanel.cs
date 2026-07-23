using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Tomb.Core.Save;
using Tomb.Core.Services;
using Tomb.Core.Time;
using Tomb.Gameplay.Machines;
using Tomb.Gameplay.Power;
using Tomb.Gameplay.Resources;

namespace Tomb.Core.Debugging.Overlay
{
    public sealed class DebugOverviewPanel : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private TMP_Text currentTimeText;
        [SerializeField] private TMP_Text timeScaleText;
        [SerializeField] private TMP_Text pauseStateText;
        [SerializeField] private TMP_Text savePathText;

        [Header("Station")]
        [SerializeField] private TMP_Text stationStatusText;
        [SerializeField] private TMP_Text resourceWarningText;
        [SerializeField] private TMP_Text powerSummaryText;
        [SerializeField] private TMP_Text machineSummaryText;

        private GameTimeSystem timeSystem;
        private SaveSystem saveSystem;
        private ResourceSystem resourceSystem;
        private MachineSystem machineSystem;
        private PowerSystem powerSystem;

        private void Start()
        {
            timeSystem =
                CoreServices.Get<GameTimeSystem>();

            saveSystem =
                CoreServices.Get<SaveSystem>();

            resourceSystem =
                CoreServices.Get<ResourceSystem>();

            machineSystem =
                CoreServices.Get<MachineSystem>();

            powerSystem =
                CoreServices.Get<PowerSystem>();
        }

        private void Update()
        {
            if (timeSystem == null)
                return;

            RefreshCoreInformation();
            RefreshStationInformation();
        }

        private void RefreshCoreInformation()
        {
            currentTimeText.text =
                $"Current Time: {timeSystem.CurrentTime}";

            timeScaleText.text =
                $"Time Scale: {timeSystem.TimeScale:0.##}x";

            pauseStateText.text =
                $"Paused: {timeSystem.IsPaused}";

            savePathText.text =
                $"Save Path: {saveSystem.SavePath}";
        }

        private void RefreshStationInformation()
        {
            int emptyResources = 0;
            int lowResources = 0;

            StringBuilder warningBuilder =
                new StringBuilder();

            foreach (ResourceState resource
                     in resourceSystem.Resources)
            {
                if (resource.IsEmpty)
                {
                    emptyResources++;

                    AppendWarning(
                        warningBuilder,
                        $"{resource.Definition.DisplayName}: EMPTY"
                    );
                }
                else if (resource.IsLow)
                {
                    lowResources++;

                    AppendWarning(
                        warningBuilder,
                        $"{resource.Definition.DisplayName}: LOW"
                    );
                }
            }

            int brokenMachines = 0;
            int unpoweredMachines = 0;
            int blockedMachines = 0;
            int disabledMachines = 0;

            foreach (MachineState machine
                     in machineSystem.Machines)
            {
                switch (machine.Status)
                {
                    case MachineStatus.Broken:
                        brokenMachines++;
                        break;

                    case MachineStatus.NoPower:
                        unpoweredMachines++;
                        break;

                    case MachineStatus.MissingInputs:
                        blockedMachines++;
                        break;

                    case MachineStatus.Disabled:
                        disabledMachines++;
                        break;
                }
            }

            bool powerShortage =
                powerSystem.CurrentServedDemand + 0.0001f <
                powerSystem.CurrentDemand;

            bool stationCritical =
                emptyResources > 0 ||
                brokenMachines > 0 ||
                powerShortage;

            bool stationWarning =
                lowResources > 0 ||
                unpoweredMachines > 0 ||
                blockedMachines > 0;

            string stationState =
                stationCritical
                    ? "CRITICAL"
                    : stationWarning
                        ? "WARNING"
                        : "NOMINAL";

            stationStatusText.text =
                $"Station Status: {stationState}";

            resourceWarningText.text =
                warningBuilder.Length > 0
                    ? $"Resource Alerts: {warningBuilder}"
                    : "Resource Alerts: None";

            powerSummaryText.text =
                $"Power: " +
                $"{powerSystem.CurrentGeneration:0.##} generated | " +
                $"{powerSystem.CurrentDemand:0.##} demanded | " +
                $"{powerSystem.CurrentServedDemand:0.##} served | " +
                $"{powerSystem.BatteryCharge:0.##} / " +
                $"{powerSystem.BatteryCapacity:0.##} stored";

            machineSummaryText.text =
                $"Machines: " +
                $"{brokenMachines} broken | " +
                $"{unpoweredMachines} without power | " +
                $"{blockedMachines} missing inputs | " +
                $"{disabledMachines} disabled";
        }

        private static void AppendWarning(
            StringBuilder builder,
            string warning)
        {
            if (builder.Length > 0)
                builder.Append(" | ");

            builder.Append(warning);
        }
    }
}
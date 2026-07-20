using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Tomb.Gameplay.Machines;

namespace Tomb.Gameplay.Power
{
    public sealed class DebugPowerRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text roleText;
        [SerializeField] private TMP_Text rateText;
        [SerializeField] private TMP_Text priorityText;
        [SerializeField] private TMP_Text stateText;

        private MachineState machine;

        public void Initialize(MachineState machineState)
        {
            machine = machineState;
            Refresh();
        }

        public void Refresh()
        {
            if (machine == null)
                return;

            MachinePowerProfile profile =
                machine.Definition.PowerProfile;

            nameText.text =
                machine.Definition.DisplayName;

            priorityText.text =
                $"Priority: {machine.Definition.Priority}";

            stateText.text =
                machine.Status.ToString().ToUpperInvariant();

            if (profile == null)
            {
                roleText.text = "STORAGE";
                rateText.text = "Battery system";
                return;
            }

            if (profile.IsProducer && profile.IsConsumer)
            {
                roleText.text = "HYBRID";

                rateText.text =
                    $"+{profile.GenerationPerGameMinute:0.##} / " +
                    $"-{profile.DemandPerGameMinute:0.##}";
            }
            else if (profile.IsProducer)
            {
                roleText.text = "PRODUCER";

                rateText.text =
                    $"+{profile.GenerationPerGameMinute:0.##} per min";
            }
            else
            {
                roleText.text = "CONSUMER";

                rateText.text =
                    $"-{profile.DemandPerGameMinute:0.##} per min";
            }
        }
    }
}
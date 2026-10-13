using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tomb.Presentation.Station
{
    public sealed class StationOperationsTabController
        : MonoBehaviour
    {
        public enum OperationsTab
        {
            Overview,
            Resources,
            Power,
            Equipment
        }

        [Header("Content Panels")]
        [SerializeField] private GameObject overviewPanel;
        [SerializeField] private GameObject resourcesPanel;
        [SerializeField] private GameObject powerPanel;
        [SerializeField] private GameObject equipmentPanel;

        [Header("Navigation Buttons")]
        [SerializeField] private Button overviewButton;
        [SerializeField] private Button resourcesButton;
        [SerializeField] private Button powerButton;
        [SerializeField] private Button equipmentButton;

        [Header("Button Colors")]
        [SerializeField]
        private Color normalColor =
            new Color(0.15f, 0.22f, 0.28f);

        [SerializeField]
        private Color selectedColor =
            new Color(0.20f, 0.42f, 0.52f);

        private OperationsTab currentTab =
            OperationsTab.Overview;

        public OperationsTab CurrentTab => currentTab;

        private void Awake()
        {
            overviewButton.onClick.AddListener(
                ShowOverview);

            resourcesButton.onClick.AddListener(
                ShowResources);

            powerButton.onClick.AddListener(
                ShowPower);

            equipmentButton.onClick.AddListener(
                ShowEquipment);

            ShowTab(OperationsTab.Overview);
        }

        public void ShowOverview()
        {
            ShowTab(OperationsTab.Overview);
        }

        public void ShowResources()
        {
            ShowTab(OperationsTab.Resources);
        }

        public void ShowPower()
        {
            ShowTab(OperationsTab.Power);
        }

        public void ShowEquipment()
        {
            ShowTab(OperationsTab.Equipment);
        }

        public void ShowTab(OperationsTab tab)
        {
            currentTab = tab;

            overviewPanel.SetActive(
                tab == OperationsTab.Overview);

            resourcesPanel.SetActive(
                tab == OperationsTab.Resources);

            powerPanel.SetActive(
                tab == OperationsTab.Power);

            equipmentPanel.SetActive(
                tab == OperationsTab.Equipment);

            RefreshButtonColors();
        }

        private void RefreshButtonColors()
        {
            SetButtonColor(
                overviewButton,
                currentTab == OperationsTab.Overview);

            SetButtonColor(
                resourcesButton,
                currentTab == OperationsTab.Resources);

            SetButtonColor(
                powerButton,
                currentTab == OperationsTab.Power);

            SetButtonColor(
                equipmentButton,
                currentTab == OperationsTab.Equipment);
        }

        private void SetButtonColor(
            Button button,
            bool selected)
        {
            if (button == null)
                return;

            ColorBlock colors = button.colors;

            colors.normalColor =
                selected ? selectedColor : normalColor;

            colors.selectedColor =
                selected ? selectedColor : normalColor;

            button.colors = colors;

            if (button.targetGraphic != null)
            {
                button.targetGraphic.color =
                    colors.normalColor;
            }
        }

        private void OnDestroy()
        {
            overviewButton?.onClick.RemoveListener(
                ShowOverview);

            resourcesButton?.onClick.RemoveListener(
                ShowResources);

            powerButton?.onClick.RemoveListener(
                ShowPower);

            equipmentButton?.onClick.RemoveListener(
                ShowEquipment);
        }
    }
}
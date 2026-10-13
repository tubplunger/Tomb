using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Presentation.Station
{
    public sealed class DEV_OperationsInterfaceToggle
        : MonoBehaviour
    {
        [SerializeField]
        private GameObject operationsInterface;

        [SerializeField]
        private GameObject radioInterface;

        [SerializeField]
        private KeyCode toggleKey = KeyCode.F6;

        private void Update()
        {
            if (!Input.GetKeyDown(toggleKey))
                return;

            if (operationsInterface == null)
                return;

            bool shouldOpen =
                !operationsInterface.activeSelf;

            operationsInterface.SetActive(shouldOpen);

            if (shouldOpen && radioInterface != null)
            {
                radioInterface.SetActive(false);
            }
        }
    }
}
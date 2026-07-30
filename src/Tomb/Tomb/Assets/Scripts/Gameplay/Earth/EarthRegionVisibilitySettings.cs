using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Earth
{
    [CreateAssetMenu(
        fileName = "EarthRegionVisibilitySettings",
        menuName = "Tomb/Earth/Region Visibility Settings"
    )]
    public sealed class EarthRegionVisibilitySettings :
        ScriptableObject
    {
        [Tooltip(
            "Maximum spherical angle between the " +
            "station ground position and a region's " +
            "radio reference point."
        )]
        [Range(1f, 90f)]
        [SerializeField]
        private float visibilityAngleDegrees = 35f;

        public float VisibilityAngleDegrees =>
            visibilityAngleDegrees;
    }
}
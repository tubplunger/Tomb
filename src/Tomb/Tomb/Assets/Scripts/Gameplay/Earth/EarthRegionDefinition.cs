using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Earth
{
    [CreateAssetMenu(
        fileName = "EarthRegionDefinition",
        menuName = "Tomb/Earth/Earth Region"
    )]
    public sealed class EarthRegionDefinition :
        ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string regionId;

        [SerializeField]
        private string displayName;

        [TextArea]
        [SerializeField]
        private string description;

        [Header("Geographic Bounds")]
        [Range(-90f, 90f)]
        [SerializeField]
        private float minimumLatitude;

        [Range(-90f, 90f)]
        [SerializeField]
        private float maximumLatitude;

        [Range(-180f, 180f)]
        [SerializeField]
        private float minimumLongitude;

        [Range(-180f, 180f)]
        [SerializeField]
        private float maximumLongitude;

        [Header("Radio Reference Point")]
        [Range(-90f, 90f)]
        [SerializeField]
        private float radioLatitude;

        [Range(-180f, 180f)]
        [SerializeField]
        private float radioLongitude;

        public string RegionId =>
            regionId;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName)
                ? regionId
                : displayName;

        public string Description =>
            description;

        public float MinimumLatitude =>
            minimumLatitude;

        public float MaximumLatitude =>
            maximumLatitude;

        public float MinimumLongitude =>
            minimumLongitude;

        public float MaximumLongitude =>
            maximumLongitude;

        public float RadioLatitude =>
            radioLatitude;

        public float RadioLongitude =>
            radioLongitude;

        public bool Contains(
            float latitude,
            float longitude)
        {
            bool latitudeInside =
                latitude >= minimumLatitude &&
                latitude <= maximumLatitude;

            if (!latitudeInside)
                return false;

            float normalizedLongitude =
                NormalizeLongitude(longitude);

            float normalizedMinimum =
                NormalizeLongitude(minimumLongitude);

            float normalizedMaximum =
                NormalizeLongitude(maximumLongitude);

            // Normal region that does not cross the date line.
            if (normalizedMinimum <= normalizedMaximum)
            {
                return normalizedLongitude >= normalizedMinimum &&
                       normalizedLongitude <= normalizedMaximum;
            }

            // Region crossing the ±180° date line.
            return normalizedLongitude >= normalizedMinimum ||
                   normalizedLongitude <= normalizedMaximum;
        }

        private static float NormalizeLongitude(
            float longitude)
        {
            return Mathf.Repeat(
                longitude + 180f,
                360f
            ) - 180f;
        }

        private void OnValidate()
        {
            if (maximumLatitude < minimumLatitude)
            {
                maximumLatitude =
                    minimumLatitude;
            }

            radioLatitude =
                Mathf.Clamp(
                    radioLatitude,
                    -90f,
                    90f
                );

            minimumLongitude =
                NormalizeLongitude(
                    minimumLongitude
                );

            maximumLongitude =
                NormalizeLongitude(
                    maximumLongitude
                );

            radioLongitude =
                NormalizeLongitude(
                    radioLongitude
                );
        }
    }
}
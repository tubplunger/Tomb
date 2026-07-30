using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Gameplay.Earth
{
    public static class GeographicMath
    {
        public static float CalculateAngularDistanceDegrees(
            float latitudeA,
            float longitudeA,
            float latitudeB,
            float longitudeB)
        {
            float latitudeARadians =
                latitudeA * Mathf.Deg2Rad;

            float latitudeBRadians =
                latitudeB * Mathf.Deg2Rad;

            float latitudeDifference =
                (latitudeB - latitudeA) *
                Mathf.Deg2Rad;

            float longitudeDifference =
                Mathf.DeltaAngle(
                    longitudeA,
                    longitudeB
                ) * Mathf.Deg2Rad;

            float sinLatitude =
                Mathf.Sin(
                    latitudeDifference * 0.5f
                );

            float sinLongitude =
                Mathf.Sin(
                    longitudeDifference * 0.5f
                );

            float haversine =
                sinLatitude * sinLatitude +
                Mathf.Cos(latitudeARadians) *
                Mathf.Cos(latitudeBRadians) *
                sinLongitude *
                sinLongitude;

            haversine =
                Mathf.Clamp01(haversine);

            float centralAngle =
                2f *
                Mathf.Asin(
                    Mathf.Sqrt(haversine)
                );

            return centralAngle *
                   Mathf.Rad2Deg;
        }
    }
}
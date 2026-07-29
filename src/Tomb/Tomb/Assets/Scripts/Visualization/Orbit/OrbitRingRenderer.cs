using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Visualization.Orbit
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class OrbitRingRenderer :
        MonoBehaviour
    {
        [SerializeField]
        private LineRenderer lineRenderer;

        private void Reset()
        {
            lineRenderer =
                GetComponent<LineRenderer>();
        }

        public void BuildRing(
            float orbitRadius,
            float inclinationDegrees,
            int segmentCount,
            float ringWidth)
        {
            if (lineRenderer == null)
            {
                lineRenderer =
                    GetComponent<LineRenderer>();
            }

            if (lineRenderer == null)
                return;

            segmentCount =
                Mathf.Max(16, segmentCount);

            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.positionCount =
                segmentCount;

            lineRenderer.startWidth =
                ringWidth;

            lineRenderer.endWidth =
                ringWidth;

            float inclinationRadians =
                inclinationDegrees *
                Mathf.Deg2Rad;

            for (int index = 0;
                 index < segmentCount;
                 index++)
            {
                float progress =
                    index /
                    (float)segmentCount;

                float angleRadians =
                    progress *
                    Mathf.PI *
                    2f;

                Vector3 point =
                    CalculateOrbitPosition(
                        angleRadians,
                        inclinationRadians,
                        orbitRadius
                    );

                lineRenderer.SetPosition(
                    index,
                    point
                );
            }
        }

        public static Vector3 CalculateOrbitPosition(
            float orbitAngleRadians,
            float inclinationRadians,
            float orbitRadius)
        {
            float horizontalOrbitValue =
                Mathf.Sin(orbitAngleRadians);

            float x =
                Mathf.Cos(orbitAngleRadians) *
                orbitRadius;

            float y =
                horizontalOrbitValue *
                Mathf.Sin(inclinationRadians) *
                orbitRadius;

            float z =
                horizontalOrbitValue *
                Mathf.Cos(inclinationRadians) *
                orbitRadius;

            return new Vector3(
                x,
                y,
                z
            );
        }
    }
}
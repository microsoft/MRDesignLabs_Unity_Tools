//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using UnityEngine;

namespace HUX.Utilities
{
    /// <summary>
    /// Moves an object about in a random way
    /// </summary>
    public class RandomMotion : MonoBehaviour
    {
        public float Radius = 1f;
        public float Frequency = 3f;
        public float MinDistance = 0.1f;
        public float VelocityInertia = 2f;
        public float Velocity = 0.5f;
        public Vector3 PositionScale = Vector3.one;
        public Vector3 DirectionScale = Vector3.one;
        public float DirectionInertia = 1.5f;
        public Transform Bounds;
        public Transform Target;

        private void OnEnable()
        {
            startupPosition = transform.position;
            target = Bounds.TransformPoint(Vector3.Scale(PositionScale, Random.onUnitSphere * Radius));
        }

        private void Update ()
        {
            if (Time.time > nextTargetTime || Vector3.Distance (transform.position, target) < MinDistance)
            {
                nextTargetTime = Time.time + Random.Range (Frequency / 2, Frequency);
                int numIterations = 0;
                while (Vector3.Distance(transform.position, target) < MinDistance)
                {
                    numIterations++;
                    target = Bounds.TransformPoint(Vector3.Scale(PositionScale, Random.onUnitSphere * Radius));
                    if (numIterations > 10)
                        break;
                }
            }

            Vector3 newVelocity = (target - transform.position) * Velocity;
            velocity = Vector3.Lerp(velocity, newVelocity, Time.deltaTime / VelocityInertia);
            transform.position += (velocity * Time.deltaTime);
            Quaternion newDirection = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            direction = Quaternion.Lerp(direction, newDirection, Time.deltaTime * 100 / DirectionInertia);
            transform.rotation = direction;
            if (DirectionScale != Vector3.one)
            {
                transform.eulerAngles = Vector3.Scale(transform.eulerAngles, DirectionScale);
            }

            if (Target != null)
            {
                Target.position = target;
            }
        }

        private void OnDrawGizmos ()
        {
            Gizmos.DrawSphere(target, 0.01f);
            Gizmos.DrawWireSphere(Bounds.position, Radius);
        }

        private float nextTargetTime;
        private Vector3 target;
        private Vector3 velocity;
        private Quaternion direction;
        private Vector3 startupPosition;
    }
}
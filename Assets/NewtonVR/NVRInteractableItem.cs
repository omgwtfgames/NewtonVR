﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace NewtonVR
{
    public class NVRInteractableItem : NVRInteractable
    {
        [Tooltip("If you have a specific point you'd like the object held at, create a transform there and set it to this variable")]
        public Transform InteractionPoint;

        [Tooltip("If this is a child object, remove it from it's parents transform hierarchy when picked up")]
        public bool DeparentOnPickup = true;

        public UnityEvent OnBeginInteractionEvent;
        public UnityEvent OnEndInteractionEvent;

        protected float AttachedRotationMagic = 20f;
        protected float AttachedPositionMagic = 3000f;
        
        protected Transform PickupTransform;

        protected override void Awake()
        {
            base.Awake();
            this.Rigidbody.maxAngularVelocity = 100f;
        }

        public override void OnNewPosesApplied()
        {
            base.OnNewPosesApplied();

            if (IsAttached == true)
            {
                Quaternion RotationDelta;
                Vector3 PositionDelta;

                float angle;
                Vector3 axis;

                if (InteractionPoint != null)
                {
                    RotationDelta = AttachedHand.transform.rotation * Quaternion.Inverse(InteractionPoint.rotation);
                    PositionDelta = (AttachedHand.transform.position - InteractionPoint.position);
                }
                else
                {
                    RotationDelta = PickupTransform.rotation * Quaternion.Inverse(this.transform.rotation);
                    PositionDelta = (PickupTransform.position - this.transform.position);
                }

                RotationDelta.ToAngleAxis(out angle, out axis);

                if (angle > 180)
                    angle -= 360;

                if (angle != 0)
                {
                    Vector3 AngularTarget = angle * axis;
                    this.Rigidbody.angularVelocity = Vector3.MoveTowards(this.Rigidbody.angularVelocity, AngularTarget, 10f * (deltaPoses * 1000));
                }
                
                Vector3 VelocityTarget = PositionDelta / deltaPoses;
                this.Rigidbody.velocity = Vector3.MoveTowards(this.Rigidbody.velocity, VelocityTarget, 10f);
            }
        }

        public override void BeginInteraction(NVRHand hand)
        {
            base.BeginInteraction(hand);
            OnBeginInteractionEvent.Invoke();

            Vector3 closestPoint = Vector3.zero;
            float shortestDistance = float.MaxValue;
            for (int index = 0; index < Colliders.Length; index++)
            {
                Vector3 closest = Colliders[index].bounds.ClosestPoint(AttachedHand.transform.position);
                float distance = Vector3.Distance(AttachedHand.transform.position, closest);

                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestPoint = closest;
                }
            }

            if (DeparentOnPickup) transform.parent = null;

            PickupTransform = new GameObject(string.Format("[{0}] NVRPickupTransform", this.gameObject.name)).transform;
            PickupTransform.parent = hand.transform;
            PickupTransform.position = this.transform.position;
            PickupTransform.rotation = this.transform.rotation;
        }

        public override void EndInteraction()
        {
            base.EndInteraction();
            OnEndInteractionEvent.Invoke();

            if (PickupTransform != null)
                Destroy(PickupTransform.gameObject);
        }
    }
}
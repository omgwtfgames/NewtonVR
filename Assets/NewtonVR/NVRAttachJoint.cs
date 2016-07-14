using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using NewtonVR;

namespace NewtonVR
{
    public class NVRAttachJoint : MonoBehaviour
    {
        public NVRInteractableItem AttachedItem;
        public NVRAttachPoint AttachedPoint;

        public delegate void AttachJointHandler(NVRAttachPoint point);
        // init handlers with an empty dummy methods to avoid null checks
        public event AttachJointHandler OnAttach = delegate { };
        public event AttachJointHandler OnDetach = delegate { };

        public bool IsAttached { get { return AttachedItem != null; } }

        public float PullRange = 0.1f;
        public float AttachRange = 0.1f;
        public float DropDistance = 0.1f;
        public bool SnapToAttachPoint = false;
        public bool SetKinematicWhenAttached = false;
        bool _itemKinematicStatePreAttach;

        protected virtual void OnTriggerStay(Collider col)
        {
            if (IsAttached == false)
            {
                NVRAttachPoint point = AttachPointMapper.GetAttachPoint(col);
                if (point != null && point.IsAttached == false)
                {
                    float distance = Vector3.Distance(point.transform.position, this.transform.position);

                    if (distance < AttachRange)
                    {
                        Attach(point);
                    }
                    else
                    {
                        point.PullTowards(this.transform.position);
                    }
                }
            }
        }

        protected virtual void FixedUpdate()
        {
            if (IsAttached == true)
            {
                FixedUpdateAttached();
            }
        }

        protected virtual void FixedUpdateAttached()
        {
            float distance = Vector3.Distance(AttachedPoint.transform.position, this.transform.position);

            if (distance > DropDistance)
            {
                Detach();
            }
            else
            {
                AttachedPoint.PullTowards(this.transform.position);
            }
        }

        protected virtual void Attach(NVRAttachPoint point)
        {
            OnAttach(point);
            point.Attached(this);

            AttachedItem = point.Item;
            AttachedPoint = point;

            if (SnapToAttachPoint) AttachedItem.transform.position = transform.position + (AttachedItem.transform.position - AttachedPoint.transform.position);
            _itemKinematicStatePreAttach = AttachedItem.Rigidbody.isKinematic;
            if (SetKinematicWhenAttached) AttachedItem.Rigidbody.isKinematic = true;
        }

        protected virtual void Detach()
        {
            OnDetach(AttachedPoint);
            AttachedPoint.Detached(this);

            if (SetKinematicWhenAttached) AttachedItem.Rigidbody.isKinematic = _itemKinematicStatePreAttach;
            // AttachedItem.EndInteraction();

            AttachedItem = null;
            AttachedPoint = null;

        }
    }
}
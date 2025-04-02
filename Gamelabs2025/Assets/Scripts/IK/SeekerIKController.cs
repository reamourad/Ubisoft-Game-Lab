using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Player.IK
{
    public class SeekerIKController : MonoBehaviour
    {
        [SerializeField] TwoBoneIKConstraint leftHand;
        [SerializeField] TwoBoneIKConstraint rightHand;
        
        [SerializeField] Transform ikHintRight;
        [SerializeField] Transform ikHintLeft;
        
        [SerializeField] Transform ikTargetRight;
        [SerializeField] Transform ikTargetLeft;

        
        private Transform leftHandTargetRef;
        private Transform rightHandTargetRef;
        
        public void SetTarget(Transform leftTarget, Transform rightTarget)
        {
           leftHandTargetRef = leftTarget;
           rightHandTargetRef = rightTarget;
        }

        void FixedUpdate()
        {
            if (leftHandTargetRef != null)
            {
                leftHand.weight = 1;
                ikTargetLeft.position = leftHandTargetRef.position;
                ikTargetLeft.rotation = leftHandTargetRef.rotation;
            }
            else
            {
                leftHand.weight = 0;
            }
            
            if (rightHandTargetRef != null)
            {
                rightHand.weight = 1;
                ikTargetRight.position = rightHandTargetRef.position;
                ikTargetRight.rotation = rightHandTargetRef.rotation;
            }
            else
            {
                rightHand.weight = 0;
            }
        }
        
        public void StopIK()
        {
            leftHand.weight = 0;
            rightHand.weight = 0;
        }
    }
}
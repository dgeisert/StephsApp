// Rotator Track Grab Attach|GrabAttachMechanics|50090
namespace VRTK.GrabAttachMechanics
{
    using UnityEngine;
    //[AddComponentMenu("VRTK/Scripts/Interactions/Grab Attach Mechanics/VRTK_RotatorTrackGrabAttach")]
    public class NoGrabAttach : VRTK_BaseGrabAttach
    {
        protected override void Initialise()
        {
            tracked = true;
            climbable = false;
            kinematic = false;
            FlipSnapHandles();
        }
        public override void SetInitialAttachPoint(Transform givenInitialAttachPoint)
        {
        }
        public override bool StartGrab(GameObject grabbingObject, GameObject givenGrabbedObject, Rigidbody givenControllerAttachPoint)
        {
            return false;
        }
        public override void StopGrab(bool applyGrabbingObjectVelocity)
        {
        }
    }
}
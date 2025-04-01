using UnityEngine;
using UnityEngine.Serialization;

namespace Player.IK
{
    public class SeekerLocators : MonoBehaviour
    {
        [FormerlySerializedAs("SeekerVacuumHeadNonOwner")] public Transform SeekerHeadNonOwner;
        [FormerlySerializedAs("SeekerVacuumBodyNonOwner")] public Transform SeekerBodyNonOwner;
    }
}
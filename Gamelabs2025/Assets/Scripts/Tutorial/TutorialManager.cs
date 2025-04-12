using Player;
using StateManagement;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace Tutorial
{
    public class TutorialManager : SingletonBehaviour<TutorialManager>
    {
        [SerializeField] private GameObject SeekerTutorialPrefab;
        [SerializeField] private GameObject HiderTutorialPrefab;

        private GameObject spawnedCanvas;
    
        public void SpawnTutorialUI(PlayerRole.RoleType role)
        {
            if (role == PlayerRole.RoleType.Seeker)
            {
                spawnedCanvas = Instantiate(SeekerTutorialPrefab, Vector3.zero, Quaternion.identity);
            }
            else if (role == PlayerRole.RoleType.Hider)
            {
                spawnedCanvas = Instantiate(HiderTutorialPrefab, Vector3.zero, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Player role not set or unknown.");
                return;
            }
        }
    }
}

using Player;
using StateManagement;
using UnityEngine;

public class PlayerTutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject SeekerTutorialPrefab;
    [SerializeField] private GameObject HiderTutorialPrefab;

    private GameObject spawnedCanvas;

    private void Start()
    {
        SpawnTutorialUI();
    }

    private void SpawnTutorialUI()
    {
        var role = GameLookupMemory.MyLocalPlayerRole;

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
        spawnedCanvas.transform.SetParent(GameObject.Find("Canvas")?.transform, false);
    }

    public void OnTutorialComplete()
    {
        Debug.Log("Tutorial completed.");

        // Destroy the UI when done
        if (spawnedCanvas != null)
        {
            Destroy(spawnedCanvas);
        }
    }
}
using UnityEngine;

public class PlayerTutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject tutorialCanvasPrefab;

    private GameObject spawnedCanvas;

    private void Start()
    {
        SpawnTutorialUI();
    }

    private void SpawnTutorialUI()
    {
        spawnedCanvas = Instantiate(tutorialCanvasPrefab, Vector3.zero, Quaternion.identity);
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
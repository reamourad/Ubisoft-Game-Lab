using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    [Header("References")]
    public NavigationGraph graph;
    public GameObject ghostPrefab;

    [Header("Settings")]
    public float spawnInterval = 10f;
    private int initialGhostCount;

    private List<GameObject> activeGhosts = new List<GameObject>();

    void Start()
    {
        // Count all existing ghosts at the start
        initialGhostCount = FindObjectsByType<COMP476HiderMovement>(FindObjectsSortMode.None).Length;
        
        // Start periodic check
        StartCoroutine(SpawnMissingGhostsRoutine());
    }

    IEnumerator SpawnMissingGhostsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            CleanupDestroyedGhosts();

            int missingCount = initialGhostCount - FindObjectsByType<COMP476HiderMovement>(FindObjectsSortMode.None).Length;
            if (missingCount > 0)
            {
                for (int i = 0; i < missingCount; i++)
                {
                    SpawnGhost();
                }
            }
        }
    }

    void SpawnGhost()
    {
        var navNodes = graph.GetComponentsInChildren<NavigationNode>();
        if (navNodes.Length == 0) return;

        NavigationNode spawnNode = navNodes[Random.Range(0, navNodes.Length)];
        GameObject ghost = Instantiate(ghostPrefab, spawnNode.transform.position, Quaternion.identity);
        activeGhosts.Add(ghost);
    }

    void CleanupDestroyedGhosts()
    {
        activeGhosts.RemoveAll(g => g == null);
    }
}

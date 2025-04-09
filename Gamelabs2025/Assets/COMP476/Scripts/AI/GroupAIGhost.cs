using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GroupAIGhost : MonoBehaviour
{
    public static GroupAIGhost Instance { get; private set; }

    [Tooltip("Cohesion factor ranging from 0.75 (very scattered) to 1.25 (very tight)")]
    public float CohesionFactor = 1f;

    [Tooltip("How often the cohesion factor updates (in seconds)")]
    public float updateInterval = 1f;

    [Tooltip("Maximum distance between ghosts to be considered tightly grouped")]
    public float maxGroupRadius = 20f;

    private float _nextUpdateTime = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // enforce singleton
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (Time.time >= _nextUpdateTime)
        {
            UpdateCohesionFactor();
            _nextUpdateTime = Time.time + updateInterval;
        }
    }

    private void UpdateCohesionFactor()
    {
        var ghosts = FindObjectsByType<COMP476HiderMovement>(FindObjectsSortMode.None).ToList();

        if (ghosts.Count <= 1)
        {
            CohesionFactor = 1f; // no group effect
            return;
        }

        // Calculate average distance between each pair of ghosts
        float totalDistance = 0f;
        int pairCount = 0;

        for (int i = 0; i < ghosts.Count; i++)
        {
            for (int j = i + 1; j < ghosts.Count; j++)
            {
                totalDistance += Vector3.Distance(ghosts[i].transform.position, ghosts[j].transform.position);
                pairCount++;
            }
        }

        float averageDistance = (pairCount > 0) ? totalDistance / pairCount : 0f;

        // Normalize using fuzzy logic between 0.75 and 1.25
        float t = Mathf.Clamp01(averageDistance / maxGroupRadius);
        CohesionFactor = Mathf.Lerp(1.25f, 0.75f, t);
    }

    /// <summary>
    /// Gets the current cohesion factor, defaulting to 1f if singleton is not present.
    /// </summary>
    public static float GetCohesionFactor()
    {
        return Instance != null ? Instance.CohesionFactor : 1f;
    }
}

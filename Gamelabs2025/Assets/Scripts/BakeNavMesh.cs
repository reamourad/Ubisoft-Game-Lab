using Unity.AI.Navigation;
using UnityEngine;

public class BakeNavMesh : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;

    void Start()
    {
        NavMeshBake();
    }

    public void NavMeshBake()
    {
        navMeshSurface.BuildNavMesh(); // Rebakes the NavMesh at runtime
    }
}

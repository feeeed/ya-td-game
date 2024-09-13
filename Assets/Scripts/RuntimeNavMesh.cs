using Unity.AI.Navigation;
using UnityEngine;

public class RuntimeNavMesh : MonoBehaviour
{
    public static RuntimeNavMesh Main;

    public NavMeshSurface navMesh;
    

    void Awake()
    {
        Main = this;
        navMesh.BuildNavMesh();
    }
    public void UpdateNavMesh()
    {
        navMesh.UpdateNavMesh(navMesh.navMeshData);
    }
}

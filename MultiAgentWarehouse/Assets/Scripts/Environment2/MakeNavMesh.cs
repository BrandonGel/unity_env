using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;

public class MakeNavMesh : MonoBehaviour
{
    public SpawnShape spawnShape = SpawnShape.Box;
    public Vector3 boxSize = Vector3.zero;
    public Vector3 center = Vector3.zero;
    public Vector3 spawningOffset = Vector3.zero;
    public NavMeshQueryFilter filter;
    public float spawn_radius = 0;
    public float meshMaxDist = 0;
    public Dictionary<string, int> meshTypeID = new Dictionary<string, int>();
    public enum SpawnShape
    {
        Circle,
        Box,
    }
    public NavMeshSurface navMeshSurface;

    public (Vector3, Quaternion) FindValidNavMeshSpawnPoint()
    {
        // Sample a random point
        Vector3 randomPoint;
        switch (spawnShape)
        {
            case SpawnShape.Circle:
                randomPoint = center + Random.insideUnitSphere * spawn_radius;
                break;
            case SpawnShape.Box:
                float halfWidth = boxSize.x * 0.5f;
                float halfHeight = boxSize.z * 0.5f;
                randomPoint = center + new Vector3(Random.Range(-halfWidth, halfWidth), 0f, Random.Range(-halfHeight, halfHeight));
                break;
            default:
                randomPoint = center;
                break;
        }
        // Sample a random angle
        float randomYAngle = Random.Range(0f, 360f);
        Quaternion orientation = Quaternion.Euler(0f, randomYAngle, 0f);

        // Check to find valid position in Mesh
        NavMeshHit hit;
        filter.agentTypeID = navMeshSurface.agentTypeID;
        filter.areaMask = NavMesh.AllAreas;
        if (NavMesh.SamplePosition(randomPoint, out hit, meshMaxDist, filter))
        {
            return (hit.position, orientation); //Return a valid position in the mesh
        }
        return (center, Quaternion.identity); // Else return just the center

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        switch (spawnShape)
        {
            case SpawnShape.Circle:
                Gizmos.DrawWireSphere(transform.localPosition + spawningOffset, spawn_radius);
                break;
            case SpawnShape.Box:
                Vector3 size = new Vector3(boxSize.x, 0, boxSize.z);
                Gizmos.DrawWireCube(transform.localPosition + spawningOffset, size);
                break;
            default:
                break;
        }
    }

    private int? GetNavMeshAgentID(string name)
    {
        // Loop through all registered agent types
        for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
        {
            // Get the settings for the current agent type
            NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(i);

            // Compare the name of the current agent type with the desired name
            if (name == NavMesh.GetSettingsNameFromID(settings.agentTypeID))
            {
                return settings.agentTypeID;
            }
        }
        return null;
    }

    public void setParameters(SpawnShape spawnShape = SpawnShape.Box, Vector3 boxSize = default, Vector3 offset = default, float spawn_radius = 0)
    {
        this.spawnShape = spawnShape;
        switch (spawnShape)
        {
            case SpawnShape.Circle:
                meshMaxDist = spawn_radius;
                break;
            case SpawnShape.Box:
                float halfWidth = boxSize.x * 0.5f;
                float halfHeight = boxSize.z * 0.5f;
                meshMaxDist = Mathf.Sqrt(halfWidth*halfWidth + halfHeight*halfHeight);
                break;
            default:
                meshMaxDist = 0;
                Debug.LogWarning("Invalid spawn shape specified. Radius is 0.");
                break;
        }
        if (boxSize != default)
            this.boxSize = boxSize;
        if (offset != default)
            this.spawningOffset = offset;
        if (spawn_radius != 0)
            this.spawn_radius = spawn_radius;
        center = transform.localPosition + spawningOffset;
    }

    public void StartMesh(string agentTypeName = "SRS 1P")
    {
        // navMeshSurface = GetComponent<NavMeshSurface>();
        // NavMeshSurface[] navMeshSurfaces = GetComponents<NavMeshSurface>();
        
        // if (!meshTypeID.ContainsKey(agentTypeName))
        // {
        //     meshTypeID.Add(agentTypeName, meshTypeID.Count);
        // }

        if (navMeshSurface != null)
        {
            // navMeshSurface = navMeshSurfaces[meshTypeID[agentTypeName]];
            // Configure NavMeshSurface properties (optional, but recommended)
            int? AgentID = GetNavMeshAgentID(agentTypeName);
            if (AgentID.HasValue)
            {
                navMeshSurface.agentTypeID = AgentID.Value; // Set the agent type
                navMeshSurface.collectObjects = CollectObjects.All; // Or CollectObjects.Children
                navMeshSurface.defaultArea = 0; // Default walkable area

                // Bake the NavMesh
                navMeshSurface.BuildNavMesh();
            }
            else
            {
                Debug.LogWarning("Agent type ID not found for: " + agentTypeName);
            }
        }
        else
        {
            Debug.LogError("NavMeshSurface component not found!");
        }
    }

}
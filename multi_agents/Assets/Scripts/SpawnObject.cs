using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using multiagent;

public class SpawnObject : MonoBehaviour
{
    public enum SpawnShape
    {
        Circle,
        Box,
    }

    public GameObject robot = null;
    public GameObject[] robots;
    public GameObject Ground = null;
    public int spawnCount = 0;
    public float spawnRadius = 0;
    public int num_spawn_tries = 100;

    public SpawnShape spawnShape = SpawnShape.Box; // Default to be a box
    public Vector2 boxSize = new Vector2(0, 0);
    public Vector3 spawningOffset = new Vector3(0, 0, 0);
    public NavMeshSurface navMeshSurface;
    public bool debug_mesh = false;
    private Robot robotObjrobot;
    public float tol = 0.1f;
    public void SpawnRobots(bool init = true)
    {
        robotObjrobot = robot.GetComponent<Robot>();
        for (int i = 0; i < spawnCount; i++)
        {
            bool isOverlapping = false;
            for (int j = 0; j < num_spawn_tries; j++)
            {
                (Vector3 randomPoint, Quaternion orientation) = FindValidNavMeshSpawnPoint(transform.position, spawnRadius);
                Vector3 halfExtents = robotObjrobot.transform.localScale * (0.5f + tol); // 0.5 is half the size of the robot's dimension while tol is a minimum tolerance or spacing
                halfExtents.y = 0; // Set the height to be zero (don't care any overlap in height)
                isOverlapping = Physics.CheckBox(randomPoint, halfExtents, orientation);
                if (isOverlapping == false)
                {
                    if (init)
                    {
                        GameObject robotInstance = Instantiate(robot, randomPoint, orientation);
                        robotInstance.transform.parent = gameObject.transform.Find("Robots").transform;

                    }
                    else
                    {
                        GameObject robotInstance = robots[i];
                        robotInstance.transform.localPosition = randomPoint;
                        robotInstance.transform.localRotation = orientation;
                    }
                    break;
                    
                }
            }
            if (isOverlapping)
            {
                Debug.Log("Couldn't find valid position");
            }
        }
    }

    public (Vector3, Quaternion) FindValidNavMeshSpawnPoint(Vector3 center, float radius)
    {
        // Sample a random point
        Vector3 randomPoint;
        switch (spawnShape)
        {
            case SpawnShape.Circle:
                randomPoint = center + Random.insideUnitSphere * radius;
                break;
            case SpawnShape.Box:
                float halfWidth = boxSize.x * 0.5f;
                float halfHeight = boxSize.y * 0.5f;
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
        int myLayer = LayerMask.NameToLayer("Walkable");
        if (NavMesh.SamplePosition(randomPoint, out hit, radius, myLayer))
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
                Gizmos.DrawWireSphere(transform.position + spawningOffset, spawnRadius);
                break;
            case SpawnShape.Box:
                Vector3 size = new Vector3(boxSize.x, 0, boxSize.y);
                Gizmos.DrawWireCube(transform.position + spawningOffset, size);
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

    void StartMesh()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        if (navMeshSurface != null)
        {
            // Configure NavMeshSurface properties (optional, but recommended)
            string agentTypeName = "SRS 1P"; // The name of your agent type in the Navigation window
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

    void GetGround()
    {
        if (Ground != null)
        {
            Transform ground = Ground.transform;
            spawningOffset = ground.localPosition;
            boxSize.x = 2 * Mathf.Abs(spawningOffset.x);
            boxSize.y = 2 * Mathf.Abs(spawningOffset.z);
            spawnRadius = Mathf.Max(boxSize.x, boxSize.y) / 2;
        }
        else
        {
            Debug.LogError("Ground component not found!");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        robots = new GameObject[spawnCount];
        GetGround();
        StartMesh();
        SpawnRobots();
    }

    // Update is called once per frame
    void Update()
    {

    }
    

}

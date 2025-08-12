using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;
using multiagent;
using UnityEditor.Callbacks;
using multiagent.util;

public class Environment : MonoBehaviour
{
    public enum SpawnShape
    {
        Circle,
        Box,
    }

    public GameObject robot = null;
    public GameObject[] robots;
    public Dictionary<string,List<Goal[]>> goals;
    public Vector3[] robotsPosition;
    public Quaternion[] robotsQuaternion;
    public GameObject Ground = null;
    public int spawnCount = 0;
    public float spawnRadius = 0;
    public int num_spawn_tries = 100;

    public SpawnShape spawnShape = SpawnShape.Box; // Default to be a box
    public Vector2 boxSize = new Vector2(0, 0);
    public Vector3 spawningOffset = new Vector3(0, 0, 0);
    public NavMeshSurface navMeshSurface;
    public bool debug_mesh = false;
    public float tol = 0.1f;
    public int CurrentEpisode = 1;
    public int StepCount = 0; 
    public Data dataClass;
    public csv_exporter CSVexporter;
    [SerializeField] public bool useCSVExporter = false;
    public void SpawnRobots(bool init = true)
    {
        Robot robotObj = robot.GetComponent<Robot>();
        for (int i = 0; i < spawnCount; i++)
        {
            bool isOverlapping = false;
            for (int j = 0; j < num_spawn_tries; j++)
            {
                (Vector3 randomPoint, Quaternion orientation) = FindValidNavMeshSpawnPoint(spawningOffset + transform.localPosition, spawnRadius);
                Vector3 halfExtents = robotObj.transform.localScale * (0.5f + tol); // 0.5 is half the size of the robot's dimension while tol is a minimum tolerance or spacing
                halfExtents.y = 0; // Set the height to be zero (don't care any overlap in height)
                isOverlapping = Physics.CheckBox(randomPoint, halfExtents, orientation);
                if (isOverlapping == false)
                {
                    if (init)
                    {
                        GameObject robotInstance = Instantiate(robot, randomPoint, orientation);
                        robotInstance.transform.parent = gameObject.transform.Find("Robots").transform;
                        robots[i] = robotInstance;
                    }
                    else
                    {
                        Rigidbody robotRigidBody = robots[i].GetComponent<Rigidbody>();
                        robotRigidBody.position = randomPoint;
                        robotRigidBody.rotation = orientation;
                        Robot robotComponent = robots[i].GetComponent<Robot>();
                        robotComponent.updateSpawnState(randomPoint, orientation);
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

    
    public void InitGoals()
    {
        int ii = 0;
        goals = new Dictionary<string,List<Goal[]>>();
        
        // Pickups
        goals.Add("Pickups", new List<Goal[]>());
        ii = 0;
        foreach (Transform child in transform.Find("Pickups"))
        {
            Goal[] pickup =  new Goal[2];
            Goal paletteDropOff = child.transform.Find("Drop Palette").GetComponent<Goal>();
            Goal batteryPickUp = child.transform.Find("Get Battery").GetComponent<Goal>();

            pickup[0] = paletteDropOff;
            pickup[1] = batteryPickUp;
            goals["Pickups"].Add(pickup);
            ii += 1;
        }

        // Dropouts
        goals.Add("Dropoff", new List<Goal[]>());
        ii = 0;
        foreach (Transform child in transform.Find("Dropoffs"))
        {
            Goal[] dropoff = new Goal[2];
            Goal palettePickUp = child.transform.Find("Get Palette").GetComponent<Goal>();
            Goal batteryDropOff = child.transform.Find("Drop Battery").GetComponent<Goal>();

            dropoff[0] = palettePickUp;
            dropoff[1] = batteryDropOff;
            goals["Dropoff"].Add(dropoff);
            ii += 1;
        }
    }

    public void AssignGoals(int i)
    {
        int numberDropoffs = transform.Find("Dropoffs").childCount;
        int numberPickUps = transform.Find("Pickups").childCount;
        
        Robot robotComponent = robots[i].GetComponent<Robot>();
        (_, int _goalID, int goal_type, _) = robotComponent.getGoal();
        Vector3 goal = Vector3.zero;
        Goal goalObj = null; 
        switch (goal_type)
        {
            case 0:
                _goalID = Random.Range(0, numberPickUps - 1);
                goalObj = goals["Pickups"][_goalID][1];
                goal_type = 1;
                break;
            case 1:
                _goalID = Random.Range(0, numberDropoffs - 1);
                goalObj = goals["Dropoff"][_goalID][1];
                goal_type = 2;
                break;
            case 2:
                goalObj = goals["Dropoff"][_goalID][0];
                goal_type = 3;
                break;
            case 3:
                _goalID = Random.Range(0, numberPickUps - 1);
                goalObj = goals["Pickups"][_goalID][0];
                goal_type = 4;
                break;
            case 4:
                goalObj = goals["Pickups"][_goalID][1];
                goal_type = 1;
                break;
            default:
                break;
        }
        if (goalObj != null)
            goal = goalObj.position;
        robotComponent.setGoal(goal,_goalID,goal_type,goalObj);

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
                Gizmos.DrawWireSphere(transform.localPosition + spawningOffset, spawnRadius);
                break;
            case SpawnShape.Box:
                Vector3 size = new Vector3(boxSize.x, 0, boxSize.y);
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

    void adjustRobotHeight(float height = 0f)
    {
        for (int i = 0; i < spawnCount; i++)
            {
                Rigidbody robotRigidBody = robots[i].GetComponent<Rigidbody>();
                robotRigidBody.position = new Vector3(
                    robotRigidBody.position.x,
                    robotRigidBody.position.y + height,
                    robotRigidBody.position.z
                );
            }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("OnEpisodeBeing()");
        robots = new GameObject[spawnCount];
        robotsPosition = new Vector3[spawnCount];
        robotsQuaternion = new Quaternion[spawnCount];
        StepCount = 0;
        CurrentEpisode = 1;
        GetGround();
        StartMesh();
        SpawnRobots();
        InitGoals();
        CSVexporter = new csv_exporter();
        dataClass = new Data(spawnCount);
        List<(float,Vector3, Vector3)> entries = new List<(float, Vector3, Vector3)>();
        for (int i = 0; i < spawnCount; i++)
        {
            AssignGoals(i);
            Robot robotComponent = robots[i].GetComponent<Robot>();
            if (useCSVExporter)
            {
                (float currentTime, Vector3 s, Vector3 ds) = robotComponent.getState();
                entries.Add((currentTime,s, ds));    
            }
        }
        if (useCSVExporter)
        {
            dataClass.addEntry(entries);    
        }
        
    }

    // Check terminal conditions
    void FixedUpdate()
    {
        StepCount += 1;

        List<(float, Vector3,Vector3)> entries = new List<(float, Vector3,Vector3)>();
        for (int i = 0; i < spawnCount; i++)
        {
            Robot robotComponent = robots[i].GetComponent<Robot>();
            if (useCSVExporter)
            {
                (float currentTime, Vector3 s, Vector3 ds) = robotComponent.getState();
                entries.Add((currentTime, s, ds));
            }
            

            // Goal Assignment Condition
            (_, _, _, bool goalReached) = robotComponent.getGoal();
            if (goalReached)
            {
                AssignGoals(i);
            }
        }

        if (useCSVExporter)
        {
            dataClass.addEntry(entries);
        }

        // Termination Condition
        bool allRobotTerminalCond = false;
        for (int i = 0; i < spawnCount; i++)
        {
            Robot robotComponent = robots[i].GetComponent<Robot>();
            allRobotTerminalCond |= robotComponent.checkTerminalCondition();
        }
        if (allRobotTerminalCond)
        {
            adjustRobotHeight(10f);
            SpawnRobots(false);
            if (useCSVExporter)
            {
                CSVexporter.transferData(dataClass, CurrentEpisode);
                dataClass.clear();
            }
            for (int i = 0; i < spawnCount; i++)
                {
                    AssignGoals(i);
                    Robot robotComponent = robots[i].GetComponent<Robot>();
                    if (useCSVExporter)
                    {
                        (float currentTime, Vector3 s, Vector3 ds) = robotComponent.getState();
                        entries.Add((currentTime, s, ds));
                    }
                }
            Debug.Log("Episode " + CurrentEpisode + " is over!");
            StepCount = 0;
            CurrentEpisode += 1;
        }

    }


}

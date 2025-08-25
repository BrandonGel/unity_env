using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;
using multiagent.agent;
using multiagent.util;
using multiagent.goal;
using multiagent.palette;
using multiagent.camera;
using System.Collections;



public class Environment : MonoBehaviour
{
    public enum SpawnShape
    {
        Circle,
        Box,
    }

    public GameObject camera;
    public GameObject robot = null;
    public GameObject[] robots;
    public Dictionary<string,List<Goal[]>> goals;
    private goalClass _goalClass;
    private Vector3[] _robotsPosition;
    private Quaternion[] _robotsQuaternion;
    public GameObject Ground = null;
    public int debugAnimateBoxOption = 0; //0 - None, 1 - box with opacity, 2 - palette & crate gameobjects
    public GameObject boxPrefab = null; 
    public GameObject[] boxes = null; 
    public GameObject topBoxPrefab = null; // Used for SpawnBoxes (visual the crate)
    public GameObject bottomBoxPrefab = null; // Used for SpawnBoxes (visual the palette)
    public palette[] palettes = null;
    public int num_of_agents = 1;
    public float spawn_radius = 0;
    public int num_spawn_tries = 100;

    public SpawnShape spawnShape = SpawnShape.Box; // Default to be a box
    public Vector2 boxSize = new Vector2(0, 0);
    public Vector3 spawningOffset = new Vector3(0, 0, 0);
    public NavMeshSurface navMeshSurface;
    public float tol = 0.5f;
    public int CurrentEpisode = 1;
    public int StepCount = 0; 
    public csv_exporter CSVexporter;
    [SerializeField] public bool useCSVExporter = false;
    public void SpawnRobots(bool init = true)
    {
        Robot robotObj = robot.GetComponent<Robot>();
        for (int i = 0; i < num_of_agents; i++)
        {
            bool isOverlapping = false;

            for (int j = 0; j < num_spawn_tries; j++)
            {
                (Vector3 randomPoint, Quaternion orientation) = FindValidNavMeshSpawnPoint(spawningOffset + transform.localPosition, spawn_radius);
                Vector3 halfExtents = robotObj.transform.localScale * (0.5f + tol); // 0.5 is half the size of the robot's dimension while tol is a minimum tolerance or spacing
                halfExtents.y = 0; // Set the height to be zero (don't care any overlap in height)
                isOverlapping = Physics.CheckBox(randomPoint, halfExtents, orientation);
                if (isOverlapping == false)
                {
                    if (init)
                    {
                        GameObject robotInstance = Instantiate(robot, randomPoint, orientation);
                        robotInstance.name = Util.getNewName(robotInstance,i+1);
                        robotInstance.transform.parent = gameObject.transform.Find("Robots").transform;
                        robotInstance.GetComponent<Robot>().setID(i);
                        robotInstance.GetComponent<Robot>().initExtra();
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

    public void SpawnBoxes()
    {
        switch (debugAnimateBoxOption)
        {
            case 1:
                for (int i = 0; i < num_of_agents; i++)
                {
                    Vector3 position = new Vector3(
                       boxPrefab.transform.position.x,
                       boxPrefab.transform.position.y + 0.25f,
                       boxPrefab.transform.position.z
                   );
                    GameObject box = Instantiate(boxPrefab, position, Quaternion.identity);

                    box.transform.parent = gameObject.transform.Find("Robots").Find(robots[i].name);
                    boxes[i] = box;
                }
                break;
            case 2:
                for (int i = 0; i < num_of_agents; i++)
                {
                    Vector3 bottomBoxScale = bottomBoxPrefab.GetComponent<Transform>().localScale;
                    Vector3 bottomBoxCenter = bottomBoxPrefab.GetComponent<BoxCollider>().center;
                    Vector3 bottomBoxSize = bottomBoxPrefab.GetComponent<BoxCollider>().size;

                    Vector3 topBoxScale = topBoxPrefab.GetComponent<Transform>().localScale;
                    Vector3 topBoxCenter = topBoxPrefab.GetComponent<BoxCollider>().center;
                    Vector3 topBoxSize = topBoxPrefab.GetComponent<BoxCollider>().size;

                    Vector3 firstWaypointbottom = new Vector3(
                        bottomBoxCenter.x,
                        bottomBoxCenter.y + 0.25f,
                        bottomBoxCenter.z
                    );

                    float y = bottomBoxSize.y * bottomBoxScale.y + topBoxSize.y * topBoxScale.y;

                    Vector3 firstWaypointtop = firstWaypointbottom + new Vector3(0, y, 0);
                    GameObject topBox = Instantiate(topBoxPrefab, firstWaypointtop, Quaternion.identity);
                    GameObject bottomBox = Instantiate(bottomBoxPrefab, firstWaypointbottom, Quaternion.identity);

                    topBox.GetComponent<movingObject>().setOffset(firstWaypointtop);
                    bottomBox.GetComponent<movingObject>().setOffset(firstWaypointbottom);

                    GameObject dummyPaletteObjFolder = new GameObject("Palette (" + (i + 1) + ")");
                    dummyPaletteObjFolder.GetComponent<Transform>().position = Vector3.zero;
                    string robotName = robots[i].name;

                    dummyPaletteObjFolder.transform.parent = gameObject.transform.Find("Palettes");
                    topBox.transform.parent = dummyPaletteObjFolder.transform;
                    bottomBox.transform.parent = dummyPaletteObjFolder.transform;
                    palettes[i] = new palette(dummyPaletteObjFolder);
                    palettes[i].assignRobot(robots[i]);
                }
                break;
            default:
                break;
        }
        
    }

    public void AnimatePalette(int i)
    {
        Robot robotComponent = robots[i].GetComponent<Robot>();
        Goal _goalClass = robotComponent.getGoal();
        int goalType = _goalClass.goalType;
        switch (debugAnimateBoxOption)
        {
            case 1:
                boxes[i].GetComponent<VirtualBox>().getPalette(goalType);
                break;
            case 2:
                palettes[i].getPalette(robotComponent, _goalClass, goalType);
                break;
        }
    }

    public void ResetPalette(int i)
    {
        switch (debugAnimateBoxOption)
        {
            case 1:
                boxes[i].GetComponent<VirtualBox>().resetParameters();
                break;
            case 2:
                palettes[i].resetParameters();
                break;
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
                Gizmos.DrawWireSphere(transform.localPosition + spawningOffset, spawn_radius);
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
            spawn_radius = Mathf.Max(boxSize.x, boxSize.y) / 2;
        }
        else
        {
            Debug.LogError("Ground component not found!");
        }
    }

    void setPosition(Vector3[] positions = default, Quaternion[] orientations = default, float addedHeight = 0f)
    {
        Vector3 position;
        Quaternion orientation;
        for (int i = 0; i < num_of_agents; i++)
        {
            if (positions == default)
            {
                position = _robotsPosition[i];
            }
            else
            {
                position = positions[i];
            }
            if (orientations == default)
            {
                orientation = _robotsQuaternion[i];
            }
            else
            {
                orientation = orientations[i];
            }

            Rigidbody robotRigidBody = robots[i].GetComponent<Rigidbody>();
            robotRigidBody.position = position + new Vector3(0f, addedHeight, 0f);
            robotRigidBody.rotation = orientation;
        }
    }

    void getPosition()
    {
        
        for (int i = 0; i < num_of_agents; i++)
        {
            Rigidbody robotRigidBody = robots[i].GetComponent<Rigidbody>();
            _robotsPosition[i] = robotRigidBody.position;
            _robotsQuaternion[i] = robotRigidBody.rotation;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("OnEpisodeBeing()");
        Parameters.readConfigFile();
        Parameters.setParam(this);
        robots = new GameObject[num_of_agents];
        palettes = new palette[num_of_agents];
        boxes = new GameObject[num_of_agents];
        _robotsPosition = new Vector3[num_of_agents];
        _robotsQuaternion = new Quaternion[num_of_agents];
        _goalClass = new goalClass();
        _goalClass.initialize(transform, Parameters.param.goalParams);
        StepCount = 0;
        CurrentEpisode = 1;
        GetGround();
        StartMesh();
        SpawnRobots();
        SpawnBoxes();
        _goalClass.InitGoals();
        camera.GetComponent<Camera_Follow>().getPlayers(robots);
        CSVexporter = new csv_exporter();
        List<(float,Vector3, Vector3)> entries = new List<(float, Vector3, Vector3)>();
        for (int i = 0; i < num_of_agents; i++)
        {
            _goalClass.AssignGoals(i, robots[i], true);
            Robot robotComponent = robots[i].GetComponent<Robot>();
            (float currentTime, Vector3 s, Vector3 ds) = robotComponent.getState();
            entries.Add((currentTime,s, ds));    
        }    
    }

    // Check terminal conditions
    void FixedUpdate()
    {
        StepCount += 1;

        List<(float, Vector3,Vector3)> entries = new List<(float, Vector3,Vector3)>();
        for (int i = 0; i < num_of_agents; i++)
        {
            Robot robotComponent = robots[i].GetComponent<Robot>();
            (float currentTime, Vector3 s, Vector3 ds) = robotComponent.getState();
            entries.Add((currentTime, s, ds));


            // Goal Assignment Condition
            if (robotComponent.getGoalReached())
            {
                AnimatePalette(i);
                if (!robotComponent.checkWait())
                {
                    _goalClass.AssignGoals(i, robots[i]);
                }
            }
        }

        // Termination Condition
        bool allRobotTerminalCond = false;
        for (int i = 0; i < num_of_agents; i++)
        {
            Robot robotComponent = robots[i].GetComponent<Robot>();
            allRobotTerminalCond |= robotComponent.checkTerminalCondition();
        }
        if (allRobotTerminalCond)
        {
            getPosition();
            setPosition(default,default,10f);
            SpawnRobots(false);
            _goalClass.InitGoals();
            for (int i = 0; i < num_of_agents; i++)
            {

                Robot robotComponent = robots[i].GetComponent<Robot>();
                if (useCSVExporter)
                {
                    CSVexporter.transferData(robotComponent.aData, CurrentEpisode);
                }

                robotComponent.initExtra();
                _goalClass.AssignGoals(i, robots[i],true);
                ResetPalette(i);
                robotComponent.EndEpisode();
                (float currentTime, Vector3 s, Vector3 ds) = robotComponent.getState();
                entries.Add((currentTime, s, ds));
            }
            Debug.Log("Episode " + CurrentEpisode + " is over!");
            StepCount = 0;
            CurrentEpisode += 1;
        }

    }

}

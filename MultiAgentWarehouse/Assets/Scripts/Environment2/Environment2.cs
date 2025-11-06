using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using multiagent.parameterJson;
using multiagent.robot;
using multiagent.agent;
using multiagent.camera;
using multiagent.util;
using UnityEngine.Assertions;
using Unity.Barracuda;
public class Environment2 : MonoBehaviour
{
    public new GameObject camera;
    public MakeObstacles mo;
    public MakeStartsGoals msg;
    public environmentJson envJson = new environmentJson();
    public TaskGeneration tg;
    public MakeRobots2 mr;
    public MakeNavMesh mn_agent, mn_dynamic_obstacle;
    public MakeDynamicObstacles md;
    public parameterJson paramJson = new parameterJson();
    public scheduleJson scheduleJson = new scheduleJson();
    public ReplayPath replayPath = new ReplayPath();
    public Environment2Agent environment2Agent;
    public float t = 0f;

    public int maxTimeSteps = 1000;
    public int decisionPeriod = 1;
    public int n_tasks = 1;
    public int num_agents = 1;
    public float task_freq = 1f;
    public int tasks_obs_space = 1;
    public int robots_obs_space = 1;
    public string savePath;
    public Vector3 scaling;
    public delegate void taskAssignmentMethod();
    public taskAssignmentMethod taskAssignment = default;
    private string _configFile = "config.json";
    public csv_exporter CSVexporter = new csv_exporter();
    [SerializeField] public bool useCSVExporter = false;
    private Dictionary<string, List<PositionOrientation>> agentPoses = new Dictionary<string, List<PositionOrientation>>();
    public bool alreadyCreated = false;
    public bool verbose = false;
    public bool normalizeObservations = false;
    public List<int> episodeNumbers = new List<int>();
    public int episodeIndex = 0;
    public bool endRun = false;

    void Awake()
    {
        readConfig(_configFile);
        parameters param = paramJson.GetParameter();
        verbose = param.unityParams.verbose;
        tasks_obs_space = 2 * (param.goalParams.goals.Count + param.goalParams.starts.Count) + 4;
        maxTimeSteps = param.agentParams.maxTimeSteps;
        decisionPeriod = param.agentParams.decisionPeriod;
        Robot2 robotTemplate = mr.robot_prefab.GetComponent<Robot2>();
        robots_obs_space = robotTemplate.calculateObservationSize(robotTemplate.getObservationSize(), param.agentParams.rayParams.rayDirections, param.agentParams.rayParams.maxRayDegrees);
        normalizeObservations = param.unityParams.normalizeObservations;
        savePath = Directory.GetParent(Application.dataPath).FullName;;
        if (!param.unityParams.useShadow)
        {
            if (verbose)
                Debug.Log("Disabling Shadows");
            QualitySettings.shadows = ShadowQuality.Disable;
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                light.shadows = LightShadows.None;
            }
        }
    }

    public void setConfigFile(string file)
    {
        _configFile = file;
    }

    public string getConfigFile()
    {
        return _configFile;
    }

    public void readConfig(string file)
    {
        envJson.ReadJson(file);
        paramJson.ReadJson(file);
        scheduleJson.ReadJson(file);
        Config conf = envJson.GetConfig();
        parameters param = paramJson.GetParameter();
        Root root = envJson.GetRoot();
        if (envJson.conf.mode.Contains("generate"))
        {
            task_freq = param.goalParams.task_freq;
            if (envJson.conf.mode.Contains("envjson"))
            {
                n_tasks = root.n_tasks;
                num_agents = root.agents.Count;
            }
            else if (envJson.conf.mode.Contains("paramjson"))
            {
                n_tasks = param.goalParams.n_tasks;
                num_agents = param.agentParams.num_of_agents;
            }
        }
        else if (envJson.conf.mode.Contains("csv"))
        {
            Config config = envJson.GetConfig();
            csv_data reader = new csv_data();
            num_agents = reader.CountRobotCSVFiles(config.csvpath, verbose: param.unityParams.verbose);
            n_tasks = root.tasks.Count;
        }
        else
        {
            n_tasks = root.n_tasks;
            task_freq = root.task_freq[0];
            num_agents = root.agents.Count;
        }
        useCSVExporter = param.unityParams.useCSVExporter & (envJson.conf.mode != "csv");

    }

    public string getEpisodePath(int episodeNumber)
    {
        return Path.Combine(savePath, "episode_" + episodeNumber.ToString("D4"),"env_" + environment2Agent.getID());
    }

    public void init()
    {
        t = 0f;
        if (episodeNumbers.Count > 0)
        {
            if (episodeIndex < episodeNumbers.Count)
            {
                environment2Agent.setCurrentEpisode(episodeNumbers[episodeIndex]);
                episodeIndex += 1;
            }
            else
            {
                useCSVExporter = false;
                endRun = true;
                Debug.Log("All episodes completed. Ending run.");
                return;
            }
        }

        Root root = envJson.GetRoot();
        parameters param = paramJson.GetParameter();
        int[] dims = root.map.dimensions;
        float[] scale = root.map.scale;
        scaling = new Vector3(scale[0], 1, scale[1]);
        Map envMap = envJson.GetMap();

        // Create the World
        if (!alreadyCreated)
        {
            mo.DestroyAll();
            if (envJson.conf.world_mode == "image")
            {
                Vector3 Imagescaling = new Vector3(dims[0] * scale[0], 1, dims[1] * scale[1]);
                mo.GenerateWorld(envJson.conf.imagepath, Imagescaling);
            }
            else if (envJson.conf.world_mode == "grid")
            {
                mo.CreateWorld(_configFile);
            }
        }


        // Start and Goal Initialization
        msg.DestroyAll();
        msg.initStartLocation(envMap.start_locations, envMap.goal_locations, envMap.non_task_endpoints, param.goalParams, scaling, param.goalParams.showRenderer);

        // NavMesh Initialization
        Vector3 boxSize = new Vector3(dims[0] * scale[0], 1, dims[1] * scale[1]);
        Vector3 mapCenter = new Vector3((dims[0] * scale[0]) / 2, 0, (dims[1] * scale[1]) / 2);

        // Robot Initialization & Task Generation Initialization
        tg = new TaskGeneration(
            n_tasks,
            task_freq,
            msg.getStartLocations(),
            msg.getGoalLocations(),
            msg.getNontaskLocations(),
            param.goalParams.verbose
        );

        mn_agent.setParameters(mn_agent.spawnShape, boxSize, mapCenter, 0);
        mn_agent.StartMesh("SRS 1P");
        mn_dynamic_obstacle.setParameters(mn_dynamic_obstacle.spawnShape, boxSize, mapCenter, 0);
        mn_dynamic_obstacle.StartMesh("Humanoid");
        if (!alreadyCreated)
        {
            mr.DestroyAll();
            mr.setParameters(param.agentParams.num_spawn_tries, param.agentParams.min_spacing, boxSize);
            md.DestroyAll();
            md.setParameters(param.dynamicObstacleParams.num_spawn_tries, param.dynamicObstacleParams.min_spacing);
        }
        if (envJson.conf.mode.Contains("generate"))
        {
            tg.GenerateTasks();
            if (envJson.conf.mode.Contains("envjson"))
            {
                mr.initStartLocation(root.agents.Count, default, mn_agent.FindValidNavMeshSpawnPoint, scaling, !alreadyCreated);
                // md.initStartLocation(root.n_tasks, default, mn_dynamic_obstacle.FindValidNavMeshSpawnPoint, scaling, !alreadyCreated);
            }
            else if (envJson.conf.mode.Contains("paramjson"))
            {
                mr.initStartLocation(param.agentParams.num_of_agents, default, mn_agent.FindValidNavMeshSpawnPoint, scaling, !alreadyCreated);
                md.initStartLocation(param.dynamicObstacleParams.num_of_dyn_obs, default, mn_dynamic_obstacle.FindValidNavMeshSpawnPoint, scaling, !alreadyCreated);
            }
        }
        else if (envJson.conf.mode.Contains("download"))
        {
            List<TaskData> tasks = root.tasks;
            tg.DownloadTasks(tasks);
            mr.initStartLocation(0, root.agents, default, scaling, !alreadyCreated);
            if (envJson.conf.mode.Contains("replay"))
            {
                mr.setCommandInput(false);
            }
        }
        else if (envJson.conf.mode == "csv")
        {

            List<TaskData> tasks = root.tasks;
            tg.DownloadTasks(tasks);
            string csvpath = getEpisodePath(environment2Agent.getCurrentEpisode());
            csv_data reader = new csv_data();
            List<AgentData> agentDataList = reader.ReadAllCSVFirstPositionAsAgentData(csvpath, verbose: param.unityParams.verbose);
            agentPoses = reader.ReadAllCSVPositionsAndOrientations(csvpath, verbose: param.unityParams.verbose);
            mr.initStartLocation(0, agentDataList, default, scaling, !alreadyCreated);
            mr.setCommandInput(false);
        }
        mr.updateRobotParameters(param);
        mr.ResetAll();
        List<float[]> robotSpawnLocations = mr.getSpawnLocations();
        tg.GenerateNonEndpointTasks(robotSpawnLocations);


        // Task Assignment Declaration
        if (param.goalParams.task_mode.ToLower() == "EarlyStart".ToLower())
        {
            taskAssignment = AssignTaskEarlyStart;
        }

        // Camera Assignment
        camera.GetComponent<Camera_Follow>().getPlayers(mr.robots.ToArray());
        alreadyCreated = true;
    }

    void Start()
    {
        // init();
        CSVexporter = new csv_exporter();
    }

    void QuitApplication()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitApplication();
        }
    }

    void FixedUpdate()
    {
        t += Time.fixedDeltaTime;
        bool terminal_state = false;
        md.step();
        if (envJson.conf.mode.Contains("download"))
        {
            if (envJson.conf.mode.Contains("replay"))
            {
                terminal_state = replayPath.updatePath(t, mr.robots, scheduleJson.data.schedule, scaling);
            }
        }
        else if (envJson.conf.mode == "csv")
        {
            terminal_state = replayPath.updateCSVPath(t, mr.robots, agentPoses, scaling);
        }
        else
        {
            tg.CheckAvailableTasks(t);
            if (taskAssignment != default)
            {
                taskAssignment();
            }
        }
        
        if (terminal_state || environment2Agent.StepCount >= maxTimeSteps)
        {
            if (verbose)
                Debug.Log("Resetting Environment at Step: " + environment2Agent.StepCount);
            environment2Agent.EndEpisode();            
        }
    }

    public void AssignTaskEarlyStart()
    {
        foreach (GameObject robot in mr.robots)
        {

            if (robot.GetComponent<Robot2>().getTaskReached() || robot.GetComponent<Robot2>().checkIsIdle())
            {
                tg.AssignTaskEarlyStart(robot);
            }
            else
            {
                if (robot.GetComponent<Robot2>().taskClass == null)
                {
                    tg.AssignTaskEarlyStart(robot);
                }
            }
        }

    }

    public void exportEpisodeData(int CurrentEpisode)
    {
        if (useCSVExporter)
        {
            if (verbose)
                Debug.Log("Exporting Episode Data");
            string robotSavePath = getEpisodePath(CurrentEpisode);
            if (!Directory.Exists(robotSavePath))
            {
                Directory.CreateDirectory(robotSavePath);
            }
            for (int i = 0; i < mr.robots.Count; i++)
            {
                Robot2 robotComponent = mr.robots[i].GetComponent<Robot2>();
                CSVexporter.transferData(robotComponent.aData, CurrentEpisode, robotSavePath);
            }
        }
    }

    public void setSavePath(string savePath)
    {
        this.savePath = savePath;
    }

    public void setEpisodeNumber(List<int> episodeNumbers)
    {
        if (episodeNumbers.Count > 0)
        {
            this.episodeNumbers = episodeNumbers;
            episodeIndex = 0;
        }
    }

    public bool isEndRun()
    {
        return endRun;
    }
}
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using multiagent.parameterJson;
using multiagent.robot;
using multiagent.camera;
using UnityEngine.Assertions;
public class Environment2 : MonoBehaviour
{
    public new GameObject camera;
    public MakeObstacles mo;
    public MakeStartsGoals msg;
    public environmentJson envJson = new environmentJson();
    public TaskGeneration tg;
    public MakeRobots2 mr;
    public MakeNavMesh mn;
    public parameterJson paramJson = new parameterJson();
    public scheduleJson scheduleJson = new scheduleJson();
    public ReplayPath replayPath = new ReplayPath();
    public Environment2Agent environment2Agent;
    public float t = 0f;

    public int maxTimeSteps = 1000;
    public int decisionPeriod = 1;
    public int n_tasks = 1;
    public float task_freq = 1f;
    public int tasks_obs_space = 1;
    public int max_allowable_num_agents = 2048;
    public int max_allowable_num_tasks = 2048;    public Vector3 scaling;
    public delegate void taskAssignmentMethod();
    taskAssignmentMethod taskAssignment = default;
    public string configFile = "config2.json";

    void Awake()
    {
        readConfig(configFile);
        parameters param = paramJson.GetParameter();
        tasks_obs_space = 3 * (param.goalParams.goals.Count + param.goalParams.starts.Count) + 2;
        max_allowable_num_agents = param.unityParams.max_allowable_num_agents;
        max_allowable_num_tasks = param.unityParams.max_allowable_num_tasks;
        maxTimeSteps = param.agentParams.maxTimeSteps;
        decisionPeriod = param.agentParams.decisionPeriod;
    }

    public void readConfig(string file)
    {
        envJson.ReadJson(file);
        paramJson.ReadJson(file);
        scheduleJson.ReadJson(file);
        parameters param = paramJson.GetParameter();
        Root root = envJson.GetRoot();
        if (envJson.conf.mode == "generate")
        {
            n_tasks = param.goalParams.n_tasks;
            task_freq = param.goalParams.task_freq;
        }
        else
        {
            n_tasks = root.n_tasks;
            task_freq = root.task_freq[0];
        }
        Assert.IsTrue(max_allowable_num_tasks >= n_tasks, "The number of tasks in the parameter file exceeds the maximum allowable number of tasks in unity parameters");

    }

    public void init()
    {
        t = 0f;
        Root root = envJson.GetRoot();
        parameters param = paramJson.GetParameter();
        int[] dims = root.map.dimensions;
        float[] scale = root.map.scale;
        scaling = new Vector3(scale[0], 1, scale[1]);
        Map envMap = envJson.GetMap();

        // Create the World
        mo.DestroyAll();
        if (envJson.conf.world_mode == "image")
        {
            Vector3 Imagescaling = new Vector3(dims[0] * scale[0], 1, dims[1] * scale[1]);
            mo.GenerateWorld(envJson.conf.imagepath, Imagescaling);
        }
        else if (envJson.conf.world_mode == "grid")
        {
            mo.CreateWorld(configFile);
        }

        // Start and Goal Initialization
        msg.initStartLocation(envMap.start_locations, envMap.goal_locations, envMap.non_task_endpoints, param.goalParams, scaling);

        // NavMesh Initialization
        Vector3 boxSize = new Vector3(dims[0] * scale[0], 1, dims[1] * scale[1]);
        Vector3 mapCenter = new Vector3((dims[0] * scale[0]) / 2, 0, (dims[1] * scale[1]) / 2);
        mn.setParameters(MakeNavMesh.SpawnShape.Box, boxSize, mapCenter, 0);
        mn.StartMesh();

        // Robot Initialization & Task Generation Initialization
        tg = new TaskGeneration(
            n_tasks,
            task_freq,
            msg.getStartLocations(),
            msg.getGoalLocations()
        );
        mr.DestroyAll();
        mr.setParameters(param.agentParams.num_spawn_tries, param.agentParams.min_spacing, boxSize);
        if (envJson.conf.mode.Contains("generate"))
        {
            tg.GenerateTasks();
            if (envJson.conf.mode.Contains("envjson"))
            {
                mr.initStartLocation(root.agents.Count, default, mn.FindValidNavMeshSpawnPoint, scaling);
            }
            else if (envJson.conf.mode.Contains("paramjson"))
            {
                mr.initStartLocation(param.agentParams.num_of_agents, default, mn.FindValidNavMeshSpawnPoint, scaling);
            }
        }
        else if (envJson.conf.mode.Contains("download"))
        {
            List<TaskData> tasks = root.tasks;
            tg.DownloadTasks(tasks);
            mr.initStartLocation(0, root.agents, default, scaling);
            if (envJson.conf.mode.Contains("replay"))
            {
                mr.setCommandInput(false);
            }
        }
        else if (envJson.conf.mode == "csv")
        {
            List<TaskData> tasks = root.tasks;
            tg.DownloadTasks(tasks);
            // mr.initStartLocation(0, ###, default, default);
            mr.setCommandInput(false);
        }
        mr.updateRobotParameters(param);

        // Task Assignment Declaration
        if (param.goalParams.task_mode == "EarlyStart")
        {
            taskAssignment = AssignTaskEarlyStart;
        }

        // Camera Assignment
        camera.GetComponent<Camera_Follow>().getPlayers(mr.robots.ToArray());
    }

    void Start()
    {
        init();
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

        if (taskAssignment != default)
        {
            taskAssignment();
        }

        if (envJson.conf.mode.Contains("download"))
        {
            if (envJson.conf.mode.Contains("replay"))
            {
                replayPath.updatePath(t, mr.robots, scheduleJson.data.schedule, scaling);
            }
            
        }
    }

    public void AssignTaskEarlyStart()
    {
        tg.CheckAvailableTasks(t);
        foreach (GameObject robot in mr.robots)
        {
            try
            {
                if (robot.GetComponent<Robot>().getTaskReached())
                {
                    tg.AssignTaskEarlyStart(robot);
                }
                else
                {
                    if (robot.GetComponent<Robot>().taskClass == null)
                    {
                        tg.AssignTaskEarlyStart(robot);
                    }
                }
            }
            catch (System.Exception e)
            {
                if (robot.GetComponent<Robot2>().getTaskReached())
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
    }
}
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using multiagent.parameterJson;
using multiagent.robot;
using multiagent.camera;
public class Environment2 : MonoBehaviour
{
    public new GameObject camera;
    public MakeObstacles mo;
    public MakeStartsGoals msg;
    public environmentJson envJson = new environmentJson();
    public TaskGeneration tg;
    public MakeRobots mr;
    public MakeNavMesh mn;
    public parameterJson paramJson = new parameterJson();
    public scheduleJson scheduleJson = new scheduleJson();
    public ReplayPath replayPath = new ReplayPath();
    public Environment2Agent environment2Agent;
    public float t = 0f;

    public int maxTimeSteps = 1000;
    public int decisionPeriod = 1;
    public int n_tasks = 1;
    public int tasks_obs_space = 1;
    public Vector3 scaling;

    void Awake()
    {
        envJson.ReadJson();
        paramJson.ReadJson();
        scheduleJson.ReadJson();

        parameters param = paramJson.GetParameter();
        tasks_obs_space = 3 * (param.goalParams.goals.Count + param.goalParams.starts.Count) + 2;
        maxTimeSteps = param.agentParams.maxTimeSteps;
        decisionPeriod = param.agentParams.decisionPeriod;

        Root root = envJson.GetRoot();
        n_tasks = root.n_tasks;
    }


    void Start()
    {


        Root root = envJson.GetRoot();
        parameters param = paramJson.GetParameter();

        int[] dims = root.map.dimensions;

        float[] scale = root.map.scale;
        Vector3 Imagescaling = new Vector3(dims[0] * scale[0], 1, dims[1] * scale[1]);
        scaling = new Vector3(scale[0], 1, scale[1]);

        Map envMap = envJson.GetMap();



        mo.GenerateWorld(envJson.conf.imagepath, Imagescaling);
        // mo.CreateWorld("config2.json");



        msg.initStartLocation(envMap.start_locations, envMap.goal_locations, envMap.non_task_endpoints, param.goalParams, scaling);

        tg = new TaskGeneration(
            root.n_tasks,
            root.task_freq[0],
            msg.getStartLocations(),
            msg.getGoalLocations()
        );



        // tg.GenerateTasks();

        List<TaskData> tasks = root.tasks;
        tg.DownloadTasks(tasks);

        Vector3 boxSize = new Vector3(dims[0] * scale[0], 1, dims[1] * scale[1]);
        Vector3 mapCenter = new Vector3((dims[0] * scale[0]) / 2, 0, (dims[1] * scale[1]) / 2);
        mn.setParameters(MakeNavMesh.SpawnShape.Box, boxSize, mapCenter, 0);
        mn.StartMesh();

        mr.setParameters(param.agentParams.num_spawn_tries, param.agentParams.min_spacing, boxSize);
        mr.initStartLocation(0, root.agents, default, scaling);
        // mr.initStartLocation(root.agents.Count, default, mn.FindValidNavMeshSpawnPoint, scaling);
        // mr.initStartLocation(param.agentParams.num_of_agents, default, mn.FindValidNavMeshSpawnPoint, scaling);
        mr.updateRobotParameters(param);


        List<float> rewards = mr.getReward();

        camera.GetComponent<Camera_Follow>().getPlayers(mr.robots.ToArray());

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
        AssignTaskEarlyStart();

        // If updatePath should be static, ensure the method is static in ReplayPath
        replayPath.updatePath(t, mr.robots, scheduleJson.data.schedule, scaling);


    }

    public void AssignTaskEarlyStart()
    {
        tg.CheckAvailableTasks(t);
        foreach (GameObject robot in mr.robots)
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
    }
}
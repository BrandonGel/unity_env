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
    public float t = 0f;


    void Start()
    {
        envJson.ReadJson();
        paramJson.ReadJson();

        Root root = envJson.GetRoot();
        int[] dims = root.map.dimensions;
        parameters param = paramJson.GetParameter();
        float[] scale = root.map.scale;
        Vector3 Imagescaling = new Vector3(dims[0] * scale[0], 1, dims[1] * scale[1]);
        Vector3 scaling = new Vector3(scale[0], 1, scale[1]);

        mo.GenerateWorld("Processed_image.png", Imagescaling);

        Map envMap = envJson.GetMap();

        msg.initStartLocation(envMap.start_locations, envMap.goal_locations, envMap.non_task_endpoints, param.goalParams, scaling);
        tg = new TaskGeneration(
            envJson.root.n_tasks,
            envJson.root.task_freq[0],
            msg.getStartLocations(),
            msg.getGoalLocations()
        );



        List<TaskData> tasks = root.tasks;
        tg.GenerateTasks();
        tg.DownloadTasks(tasks);

        Vector3 boxSize = new Vector3(dims[0] * scale[0],1, dims[1] * scale[1]);
        Vector3 mapCenter = new Vector3((dims[0] * scale[0]) / 2, 0, (dims[1] * scale[1]) / 2);
        mn.setParameters(MakeNavMesh.SpawnShape.Box, boxSize, mapCenter, 0);
        mn.StartMesh();
        
        mr.setParameters(param.agentParams.num_spawn_tries, param.agentParams.min_spacing,boxSize);
        // mr.initStartLocation(root.agents.Count, root.agents,mn.FindValidNavMeshSpawnPoint, scaling);
        // mr.initStartLocation(root.agents.Count, default, mn.FindValidNavMeshSpawnPoint, scaling);
        mr.initStartLocation(param.agentParams.num_of_agents, default, mn.FindValidNavMeshSpawnPoint, scaling);
        mr.updateRobotParameters(param);
        List<float> rewards = mr.getReward();

        camera.GetComponent<Camera_Follow>().getPlayers(mr.robots.ToArray());

    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        t += Time.fixedDeltaTime;
        tg.CheckAvailableGoals(t);
        foreach (GameObject robot in mr.robots)
        {
            if (robot.GetComponent<Robot>().getTaskReached())
            {
                tg.AssignGoals(robot);
            }
            else
            {
                if (robot.GetComponent<Robot>().taskClass == null)
                {
                    tg.AssignGoals(robot);    
                }             
            }
            
        }
    }
}
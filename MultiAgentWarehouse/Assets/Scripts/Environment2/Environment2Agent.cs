using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using multiagent.task;
using multiagent.taskgoal;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
public class Environment2Agent : Agent
{
    Environment2 env;
    [HideInInspector] public int CurrentEpisode = 0;
    [HideInInspector] public float CumulativeReward = 0f;
    BufferSensorComponent bufferSensor;
    Vector3 scaling;
    void Awake()
    {
        env = transform.GetComponent<Environment2>();
        MaxStep = env.maxTimeSteps;
        GetComponent<DecisionRequester>().DecisionPeriod = env.decisionPeriod;
        bufferSensor = GetComponent<BufferSensorComponent>();
        bufferSensor.MaxNumObservables = env.max_allowable_num_tasks;
        bufferSensor.ObservableSize = env.tasks_obs_space;
        scaling = env.scaling;
    }


    public override void OnEpisodeBegin()
    {
        CurrentEpisode += 1;
        CumulativeReward = 0f;
        if (CurrentEpisode > 1)
        {
            env.readConfig(env.configFile);
            env.init();
        }
        Debug.Log("Episode: " + CurrentEpisode);

        // env.tg.GenerateTasks();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        List<Task> tasks = env.tg.GetIncompleteTasks();
        if (tasks.Count == 0)
        {
            bufferSensor.AppendObservation(new float[bufferSensor.ObservableSize]);
            return;
        }
        foreach (Task task in tasks)
        {
            List<GameObject> taskpoint = task.taskpoint;
            float[] task_obs = new float[bufferSensor.ObservableSize];
            int ii = 0;
            foreach (GameObject goal in taskpoint)
            {
                int[] tileArr = goal.GetComponent<Goal>().getTile();
                Vector3 tile = new Vector3(tileArr[0] * scaling.x, 0, tileArr[1] * scaling.z);
                task_obs[3 * ii] = tile.x;
                task_obs[3 * ii + 1] = tile.y;
                task_obs[3 * ii + 2] = tile.z;
                ii += 1;
            }
            task_obs[3 * taskpoint.Count] = task.assignedRobotID;
            task_obs[3 * taskpoint.Count + 1] = task.task_ind;
            bufferSensor.AppendObservation(task_obs);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        ;
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
    }
    
}
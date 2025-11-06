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
using multiagent.robot;
using multiagent.util;
using System;
public class Environment2Agent : Agent
{
    Environment2 env;
    [HideInInspector] public int CurrentEpisode = 0;
    [HideInInspector] public float CumulativeReward = 0f;
    private int _id = 0;
    public float CumulativeMeanReward = 0f;
    BufferSensorComponent bufferSensor;
    BufferSensorComponent agentBufferSensor;
    BufferSensorComponent timeBufferSensor;
    public List<GameObject> robots = null;
    Vector3 scaling = Vector3.one;
    public bool verbose = false;
    public bool normalizeObservations = false;

    void Awake()
    {
        env = transform.GetComponent<Environment2>();
        MaxStep = env.maxTimeSteps;
        GetComponent<DecisionRequester>().DecisionPeriod = env.decisionPeriod;
        BufferSensorComponent[] sensors = GetComponents<BufferSensorComponent>();
        foreach (BufferSensorComponent sensor in sensors)
        {
            if (sensor.SensorName == "BufferSensor")
            {
                bufferSensor = sensor;
                bufferSensor.MaxNumObservables = env.n_tasks;
                bufferSensor.ObservableSize = env.tasks_obs_space;
                bufferSensor.SensorName = "BufferSensor_" + getID();
            }
            else if (sensor.SensorName == "AgentBufferSensor")
            {
                agentBufferSensor = sensor;
                agentBufferSensor.MaxNumObservables = env.num_agents;
                agentBufferSensor.ObservableSize = env.robots_obs_space;
                agentBufferSensor.SensorName = "AgentBufferSensor_" + getID();
            }
            else if (sensor.SensorName == "TimeBufferSensor")
            {
                timeBufferSensor = sensor;
                timeBufferSensor.MaxNumObservables = 1;
                timeBufferSensor.ObservableSize = 3;
                timeBufferSensor.SensorName = "TimeBufferSensor_" + getID();
            }
        }
        normalizeObservations = env.normalizeObservations;

        int num_agents = env.num_agents;
        var behaviorParams = GetComponent<BehaviorParameters>();
        var actionSpec = ActionSpec.MakeContinuous(3 * num_agents);
        behaviorParams.BrainParameters.ActionSpec = actionSpec;
        verbose = env.verbose;
    }


    public override void OnEpisodeBegin()
    {
        
        env.t = 0f;
        scaling = env.scaling;
        if (CurrentEpisode > 0)
        {
            env.exportEpisodeData(CurrentEpisode);
        }

        CurrentEpisode += 1;
        CumulativeReward = 0f;
        env.readConfig(env.getConfigFile());
        env.init();
        robots = env.mr.getRobots();
        if (verbose && !env.isEndRun())
            Debug.Log("Episode: " + CurrentEpisode);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        List<Task> tasks = env.tg.getAllTasks();
        // tasks.AddRange(env.tg.getNonEndpointTasks());
        foreach (Task task in tasks)
        {
            List<GameObject> taskpoint = task.taskpoint;
            float[] task_obs = new float[bufferSensor.ObservableSize];
            int ii = 0;
            foreach (GameObject goal in taskpoint)
            {
                int[] tileArr = goal.GetComponent<Goal>().getTile();
                Vector2 tile = new Vector3(tileArr[0] * scaling.x, tileArr[1] * scaling.z);
                if (normalizeObservations)
                {
                    tile.x = tile.x / env.mr.boxSize.x;
                    tile.y = tile.y / env.mr.boxSize.z;
                }
                task_obs[2 * ii] = tile.x;
                task_obs[2 * ii + 1] = tile.y;
                ii += 1;
            }
            if (ii < (bufferSensor.ObservableSize - 3) / 2)
            {
                for (int jj = ii; jj < bufferSensor.ObservableSize / 2 - 1; jj++)
                {
                    task_obs[2 * jj] = task_obs[2 * ii - 2];
                    task_obs[2 * jj + 1] = task_obs[2 * ii - 1];
                }
            }

            task_obs[bufferSensor.ObservableSize - 4] = task.assignedRobotID;
            task_obs[bufferSensor.ObservableSize - 3] = task.task_ind;
            task_obs[bufferSensor.ObservableSize - 2] = task.taskID;
            task_obs[bufferSensor.ObservableSize - 1] = task.isCompleted() ? 1f : 0f;
            bufferSensor.AppendObservation(task_obs);
        }

        foreach (GameObject robot in robots)
        {
            Robot2 robotObj = robot.GetComponent<Robot2>();
            float[] agent_obs = robotObj.CollectObservations(normalizeObservations);
            agentBufferSensor.AppendObservation(agent_obs);
        }

        timeBufferSensor.AppendObservation(new float[3] {StepCount*Time.fixedDeltaTime, StepCount, env.maxTimeSteps });

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        float[] continuousActionsOut = new float[2];
        continuousActionsOut[0] = 0;
        continuousActionsOut[1] = 0;
        if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.RightArrow))
        {
            continuousActionsOut[0] = 1;
            continuousActionsOut[1] = -1;
        }
        else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftArrow))
        {
            continuousActionsOut[0] = 1;
            continuousActionsOut[1] = 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.RightArrow))
        {
            continuousActionsOut[0] = -1;
            continuousActionsOut[1] = -1;
        }
        else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.LeftArrow))
        {
            continuousActionsOut[0] = -1;
            continuousActionsOut[1] = 1;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            continuousActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            continuousActionsOut[1] = -1;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            continuousActionsOut[1] = 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            continuousActionsOut[0] = -1;
        }

        var continuousRobotActionsOut = actionsOut.ContinuousActions;
        var discreteRobotActionsOut = actionsOut.DiscreteActions;
        int ii = 0;
        foreach (GameObject robot in robots)
        {
            continuousRobotActionsOut[ii] = continuousActionsOut[0];
            continuousRobotActionsOut[ii + 1] = continuousActionsOut[1];
            continuousRobotActionsOut[ii + 2] = 0;
            ii += 3;
        }

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (robots == null)
        {
            return;
        }   
        
        int ii = 0;
        foreach (GameObject robot in robots)
        {
            float[] continuousActions = new float[2] { actions.ContinuousActions[3 * ii], actions.ContinuousActions[3 * ii + 1] };
            robot.GetComponent<Robot2>().Step(continuousActions);
            ii += 1;
        }

        if (env.taskAssignment == default)
        {
            List<int> tasks = new List<int>();
            for (int i = 0; i < robots.Count; i++)
            {
                int taskInd = (int)(Mathf.Round(actions.ContinuousActions[3 * i + 2]));
                tasks.Add(taskInd);
            }
            env.tg.assignTask(robots, tasks);
        }

    }

    void Update()
    {

    }

    void FixedUpdate()
    {
    }

    public float getReward(int id)
    {
        if (id < robots.Count)
        {
            return robots[id].GetComponent<Robot2>().CumulativeReward;
            // return robots[id].GetComponent<Robot2>().Reward;
        }
        else
        {
            Debug.Log("The robot ID exceeds the number of robots");
            return 0f;
        }
    }

    public float getMeanReward()
    {
        float reward = 0f;
        for (int i = 0; i < robots.Count; i++)
        {
            reward += robots[i].GetComponent<Robot2>().CumulativeReward;
            // reward += robots[i].GetComponent<Robot2>().Reward;   
        }
        float meanReward = reward / robots.Count;
        return meanReward;

    }

    public void setID(int id)
    {
        _id = id;
    }
    public int getID()
    {
        return _id;
    }

    public int getCurrentEpisode()
    {
        return CurrentEpisode;
    }

    public void setCurrentEpisode(int episode)
    {
        CurrentEpisode = episode;
    }
}
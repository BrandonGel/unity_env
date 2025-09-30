

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using multiagent.parameterJson;
using multiagent.task;
using multiagent.taskgoal;
using multiagent.util;

public class MakeStartsGoals : MonoBehaviour
{
    public List<List<int[]>> start_locations = new List<List<int[]>>();
    public List<List<int[]>> goal_locations = new List<List<int[]>>();
    public List<int[]> non_task_endpoints = new List<int[]>();
    public List<List<GameObject>> starts = new List<List<GameObject>>();
    public List<List<GameObject>> goals = new List<List<GameObject>>();
    public List<GameObject> non_tasks = new List<GameObject>();

    public GameObject start_prefab; // Assign your obstacle prefab here
    public GameObject goal_prefab; // Assign your obstacle prefab here
    public GameObject non_task_prefab; // Assign your obstacle prefab here
    public float goalWait = 1f;
    public int goalDelayWait = 0;
    public float goalDelayPenalty = 0f;
    public float goalWaitProbability = 1f;
    public bool showRenderer = false;

    public void initStartLocation(List<List<int[]>> start_locations, List<List<int[]>> goal_locations, List<int[]> non_task_endpoints = default, goalParameters goalParams = default, Vector3 scaling = default, bool showRenderer = true)
    {
        int i;
        this.start_locations = start_locations;
        // Vector3 startingSpawnPosition = start_prefab.transform.localScale / 2;
        // Vector3 startingSpawnPosition = new Vector3(0.5f,start_prefab.transform.localScale.y/2, 0.5f);
        Vector3 startingSpawnPosition = new Vector3(0f, start_prefab.transform.localScale.y / 2, 0f);
        scaling = scaling == default ? Vector3.one : scaling;
        i = 0;
        foreach (List<int[]> locs in start_locations)
        {
            List<GameObject> gameobject_list = new List<GameObject>();
            int j = 0;
            foreach (int[] loc in locs)
            {
                Vector3 pos = new Vector3(loc[0], 0, loc[1]) + startingSpawnPosition;
                pos = Vector3.Scale(pos, scaling);
                GameObject start = Instantiate(start_prefab, pos, Quaternion.identity);
                float goalWait = goalParams != default ? goalParams.starts[j].sampleGoalWait() : 0f;
                float goalDelayPenalty = goalParams != default ? goalParams.starts[j].sampleGoalPenalty() : 0f;
                float goalWaitProbability = goalParams != default ? goalParams.starts[j].sampleGoalProb() : 0f;

                start.GetComponent<Goal>().setParameters(i, j, loc, goalWait, goalDelayPenalty, goalWaitProbability);
                start.transform.localScale = scaling;
                start.transform.parent = gameObject.transform.Find("Starts").transform;
                start.transform.localPosition = pos;
                start.transform.localRotation = Quaternion.identity;
                gameobject_list.Add(start);

                if(!showRenderer)
                {
                    Util.enableRenderer(start.GetComponent<Renderer>(), false);
                }

                j += 1;
            }
            starts.Add(gameobject_list);
            i += 1;
        }


        this.goal_locations = goal_locations;
        // Vector3 endingSpawnPosition = goal_prefab.transform.localScale / 2;
        // Vector3 endingSpawnPosition = new Vector3(0.5f, goal_prefab.transform.localScale.y/2, 0.5f);
        Vector3 endingSpawnPosition = new Vector3(0f, goal_prefab.transform.localScale.y / 2, 0f);
        i = 0;
        foreach (List<int[]> locs in goal_locations)
        {
            List<GameObject> gameobject_list = new List<GameObject>();
            int j = 0;
            foreach (int[] loc in locs)
            {
                Vector3 pos = new Vector3(loc[0], 0, loc[1]) + endingSpawnPosition;
                pos = Vector3.Scale(pos, scaling);
                GameObject goal = Instantiate(goal_prefab, pos, Quaternion.identity);
                float goalWait = goalParams != default ? goalParams.goals[j].sampleGoalWait() : 0f;
                float goalDelayPenalty = goalParams != default ? goalParams.goals[j].sampleGoalPenalty() : 0f;
                float goalWaitProbability = goalParams != default ? goalParams.goals[j].sampleGoalProb() : 0f;
                goal.GetComponent<Goal>().setParameters(i, j, loc, goalWait, goalDelayPenalty, goalWaitProbability);
                goal.transform.localScale = scaling;
                goal.transform.parent = gameObject.transform.Find("Goals").transform;
                goal.transform.localPosition = pos;
                goal.transform.localRotation = Quaternion.identity;
                gameobject_list.Add(goal);

                if(!showRenderer)
                {
                    Util.enableRenderer(goal.GetComponent<Renderer>(), false);
                }

                j += 1;
            }
            goals.Add(gameobject_list);
            i += 1;
        }

        this.non_task_endpoints = non_task_endpoints;
        // Vector3 nontaskSpawnPosition = non_task_prefab.transform.localScale / 2;
        // Vector3 nontaskSpawnPosition = new Vector3(0.5f, non_task_prefab.transform.localScale.y / 2, 0.5f);
        Vector3 nontaskSpawnPosition = new Vector3(0f, non_task_prefab.transform.localScale.y / 2, 0f);
        i = 0;
        foreach (int[] loc in non_task_endpoints)
        {
            Vector3 pos = new Vector3(loc[0], 0, loc[1]) + nontaskSpawnPosition;
            pos = Vector3.Scale(pos, scaling);
            GameObject non_task = Instantiate(non_task_prefab, pos, Quaternion.identity);
            non_task.transform.localScale = scaling;
            non_task.transform.parent = gameObject.transform.Find("Nontasks").transform;
            non_task.transform.localPosition = pos;
            non_task.transform.localRotation = Quaternion.identity;
            non_tasks.Add(non_task);

            if(!showRenderer)
            {
                Util.enableRenderer(non_task.GetComponent<Renderer>(), false);
            }

            i += 1;
        }

    }

    public List<List<GameObject>> getStartLocations()
    {
        return starts;
    }

    public List<List<GameObject>> getGoalLocations()
    {
        return goals;
    }

    public List<GameObject> getNontaskLocations()
    {
        return non_tasks;
    }

    public void DestroyAll()
    {
        foreach (Transform child in gameObject.transform.Find("Starts").transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in gameObject.transform.Find("Goals").transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in gameObject.transform.Find("Nontasks").transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        starts = new List<List<GameObject>>();
        goals = new List<List<GameObject>>();
        non_tasks = new List<GameObject>();
    }
}

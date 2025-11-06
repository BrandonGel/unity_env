using System.Collections.Generic;
using UnityEngine;
using System;
using multiagent.dynamic_obstacle;
using multiagent.parameterJson;

public class MakeDynamicObstacles : MonoBehaviour
{
    public List<float[]> spawnlocations = new List<float[]>();
    public GameObject dyn_obs_prefab;
    public List<GameObject> dynamic_obstacles = new List<GameObject>();
    public int num_spawn_tries = 1;
    public Vector3 boxSize = new Vector3(1f, 0.5f, 1f);
    public float min_spacing = 0.1f;
    public int num_of_goal_points = 3;


    public void initStartLocation(int num_of_dyn_obs_agents = 0, List<DynObsData> dyn_obs_agents = default, Func<(Vector3, Quaternion)> findValidPoint = default, Vector3 scaling = default, bool instaniate = true, bool verbose = false)
    {
        if (dyn_obs_prefab == default && dyn_obs_agents == default && num_of_dyn_obs_agents == 0 && findValidPoint == default)
        {
            return;
        }
        // Vector3 offset = new Vector3(0.5f, 0, 0.5f);
        Vector3 offset = new Vector3(0f, 0.5f, 0f);
        spawnlocations = new List<float[]>();
        if (instaniate)
        {
            dynamic_obstacles = new List<GameObject>();
        }


        if (dyn_obs_agents != default)
        {
            int i = 0;
            foreach (DynObsData dyn_obs_agent in dyn_obs_agents)
            {
                List<Vector3> goal_points = new List<Vector3>();
                for (int j = 0; j < num_of_goal_points; j++)
                {
                    (Vector3 goal_pos, Quaternion goal_orientation) = findValidPoint();
                    goal_pos = new Vector3(goal_pos.x, 0, goal_pos.z) + offset;
                    goal_pos = Vector3.Scale(goal_pos, scaling);
                    goal_points.Add(goal_pos);
                }

                int[] loc = dyn_obs_agent.start;
                Vector3 pos = new Vector3(loc[0], 0, loc[1]) + offset;
                pos = Vector3.Scale(pos, scaling);
                if (instaniate)
                {
                    GameObject dyn_obs = Instantiate(dyn_obs_prefab, pos, Quaternion.identity);
                    dyn_obs.transform.localScale = scaling;
                    dyn_obs.transform.parent = gameObject.transform.Find("Dynamic Obstacles").transform;
                    dyn_obs.GetComponent<human_operator>().setID(i);
                    dyn_obs.GetComponent<human_operator>().boxSize = boxSize;
                    dyn_obs.GetComponent<human_operator>().reset();
                    dynamic_obstacles[i].GetComponent<human_operator>().setGoal(goal_points);
                    dyn_obs.name = "Dynamic_Obstacle_" + i;
                    dynamic_obstacles.Add(dyn_obs);
                }
                else
                {
                    dynamic_obstacles[i].transform.position = pos;
                    dynamic_obstacles[i].transform.rotation = Quaternion.identity;
                    dynamic_obstacles[i].GetComponent<human_operator>().setCollisionOn(false);
                    dynamic_obstacles[i].GetComponent<human_operator>().reset();
                    dynamic_obstacles[i].GetComponent<human_operator>().setGoal(goal_points);
                }
                
                i += 1;
            }
        }
        else if (num_of_dyn_obs_agents > 0 && findValidPoint != default)
        {
            List<Vector3> position = new List<Vector3>();
            for (int i = 0; i < num_of_dyn_obs_agents; i++)
            {
                List<Vector3> goal_points = new List<Vector3>();
                for (int j = 0; j < num_of_goal_points; j++)
                {
                    (Vector3 goal_pos, Quaternion goal_orientation) = findValidPoint();
                    goal_pos = new Vector3(goal_pos.x, 0, goal_pos.z) + offset;
                    goal_pos = Vector3.Scale(goal_pos, scaling);
                    goal_points.Add(goal_pos);
                }
                
                bool isOverlapping = false;
                Vector3 pos = Vector3.zero;
                Quaternion orientation = Quaternion.identity;
                int count = 0;
                Collider[] intersecting = new Collider[0];
                float radius = dyn_obs_prefab.GetComponent<CapsuleCollider>().radius; // 0.5 is half the size of the robot's dimension while tol is a minimum tolerance or spacing
                for (int j = 0; j < num_spawn_tries; j++)
                {
                    (pos, orientation) = findValidPoint();

                    for (int k = 0; k < position.Count; k++)
                    {
                        if (Vector3.Distance(pos, position[k]) < radius)
                        {
                            isOverlapping = true;
                            break;
                        }
                    }

                    if (!isOverlapping || position.Count == 0)
                    {
                        position.Add(pos);
                        break;
                    }
                    // Debug.Log("Overlap detected for robot " + i + " at position " + pos + " , retrying...");
                    isOverlapping = false;
                    count += 1;
                }
                if (count == num_spawn_tries)
                {
                    Debug.LogWarning("Could not find non-overlapping spawn point for robot " + i + ". Placing it anyway.");
                }
                if (instaniate)
                {
                    GameObject dyn_obs = Instantiate(dyn_obs_prefab, pos, orientation);
                    dyn_obs.transform.localScale = Vector3.Scale(dyn_obs_prefab.transform.localScale, scaling);
                    dyn_obs.transform.parent = gameObject.transform.Find("Dynamic Obstacles").transform;
                    dyn_obs.GetComponent<human_operator>().setID(i);
                    dyn_obs.GetComponent<human_operator>().boxSize = boxSize;
                    dyn_obs.GetComponent<human_operator>().setCollisionOn(false);
                    dyn_obs.GetComponent<human_operator>().setGoal(goal_points);
                    dyn_obs.name = "Dynamic_Obstacle_" + i;
                    dynamic_obstacles.Add(dyn_obs);
                }
                else
                {
                    dynamic_obstacles[i].GetComponent<human_operator>().updateSpawnState(pos, orientation);
                    dynamic_obstacles[i].GetComponent<human_operator>().setCollisionOn(false);
                    dynamic_obstacles[i].GetComponent<human_operator>().reset();
                    dynamic_obstacles[i].GetComponent<human_operator>().setGoal(goal_points);
                }

            }

            for (int i = 0; i < num_of_dyn_obs_agents; i++)
            {
                dynamic_obstacles[i].GetComponent<human_operator>().setCollisionOn(true);
            }

        }
    }

    public void updateDynamicObstacleParameters(parameters param)
    {
        foreach (GameObject dyn_obs_agent in dynamic_obstacles)
        {
            human_operator dynObsObj = dyn_obs_agent.GetComponent<human_operator>();
            dynObsObj.updateHumanOperatorParameters(param);
        }
    }

    public void setParameters(int num_spawn_tries = 1, float min_spacing = 0.1f, Vector3 boxSize = default)
    {
        this.num_spawn_tries = num_spawn_tries;
        this.min_spacing = min_spacing;
        this.boxSize = boxSize;
    }


    public List<GameObject> getDynamicObstacles()
    {
        return dynamic_obstacles;
    }


    public void DestroyAll()
    {
        foreach (Transform child in gameObject.transform.Find("Dynamic Obstacles").transform)
        {
            Destroy(child.gameObject);
        }
        dynamic_obstacles = new List<GameObject>();
        spawnlocations = new List<float[]>();
    }
    
    public void step()
    {
        foreach (GameObject dyn_obs in dynamic_obstacles)
        {
            dyn_obs.GetComponent<human_operator>().Step();
        }
    }
}
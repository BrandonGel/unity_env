
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using multiagent.robot;
using multiagent.parameterJson;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine.PlayerLoop;
public class MakeRobots : MonoBehaviour
{
    public List<List<int[]>> spawnlocations = new List<List<int[]>>();
    public GameObject robot_prefab; // Assign your obstacle prefab here
    public List<GameObject> robots = new List<GameObject>();
    public int num_spawn_tries = 1;
    public Vector3 boxSize = new Vector3(1f, 0.5f, 1f);
    public float min_spacing = 0.1f;

    public void initStartLocation(int num_of_agents = 0, List<AgentData> agents = default, Func<(Vector3, Quaternion)> findValidPoint = default, Vector3 scaling = default)
    {
        if (agents == default && num_of_agents == 0 && findValidPoint == default)
        {
            Debug.LogError("No agent data provided");
            return;
        }
        Vector3 offset = new Vector3(0.5f, 0, 0.5f);

        if (agents != default)
        {
            num_of_agents = agents.Count;
            spawnlocations = new List<List<int[]>>();
            robots = new List<GameObject>();
            int i = 0;
            foreach (AgentData agent in agents)
            {
                int[] loc = agent.start;
                Vector3 pos = new Vector3(loc[0], 0, loc[1]) + offset;
                pos = Vector3.Scale(pos, scaling);
                GameObject robot = Instantiate(robot_prefab, pos, Quaternion.identity);
                robot.transform.localScale = scaling;
                robot.transform.parent = gameObject.transform.Find("Robots").transform;
                robot.GetComponent<Robot>().setID(i);
                robot.GetComponent<Robot>().boxSize = boxSize;
                robots.Add(robot);
                i += 1;
            }
        }
        else if (num_of_agents > 0 && findValidPoint != default)
        {
            robots = new List<GameObject>();
            for (int i = 0; i < num_of_agents; i++)
            {
                bool isOverlapping = false;
                Vector3 pos = Vector3.zero;
                Quaternion orientation = Quaternion.identity;
                for (int j = 0; j < num_spawn_tries; j++)
                {
                    (pos, orientation) = findValidPoint();
                    Vector3 halfExtents = Vector3.Scale(robot_prefab.transform.localScale, scaling) * (0.5f + min_spacing); // 0.5 is half the size of the robot's dimension while tol is a minimum tolerance or spacing
                    isOverlapping = Physics.CheckBox(pos, halfExtents, orientation);
                    if (!isOverlapping)
                        break;
                }
                GameObject robot = Instantiate(robot_prefab, pos, orientation);
                robot.transform.localScale = Vector3.Scale(robot_prefab.transform.localScale, scaling);
                robot.transform.parent = gameObject.transform.Find("Robots").transform;
                robot.GetComponent<Robot>().setID(i);
                robot.GetComponent<Robot>().boxSize = boxSize;
                robots.Add(robot);
            }

        }
    }

    public void updateRobotParameters(parameters param )
    {
        foreach (GameObject robot in robots)
        {
            Robot robotObj = robot.GetComponent<Robot>();
            robotObj.updateAgentParameters(param);
        }
    }

    public void setParameters(int num_spawn_tries = 1, float min_spacing = 0.1f, Vector3 boxSize = default)
    {
        this.num_spawn_tries = num_spawn_tries;
        this.min_spacing = min_spacing;
        this.boxSize = boxSize;
    }

    public List<float> getReward()
    {
        List<float> rewards = new List<float>();
        foreach (GameObject robot in robots)
        {
            rewards.Add(robot.GetComponent<Robot>().GetCumulativeReward());
        }
        return rewards;
    }   

    public List<GameObject> getRobots()
    {
        return robots;
    }
    }

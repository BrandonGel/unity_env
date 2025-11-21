
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using multiagent.robot;
using multiagent.parameterJson;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine.PlayerLoop;
using Unity.Mathematics;
public class MakeRobots2 : MonoBehaviour
{
    public List<float[]> spawnlocations = new List<float[]>();
    public GameObject robot_prefab; // Assign your robot prefab here
    public List<GameObject> robots = new List<GameObject>();
    public int num_spawn_tries = 1;
    public Vector3 boxSize = new Vector3(1f, 0.5f, 1f);
    public float min_spacing = 0.1f;

    public void initStartLocation(int num_of_agents = 0, List<AgentData> agents = default, Func<(Vector3, Quaternion)> findValidPoint = default, Vector3 scaling = default, bool instaniate = true, bool verbose = false)
    {
        if (agents == default && num_of_agents == 0 && findValidPoint == default)
        {
            Debug.LogError("No agent data provided");
            return;
        }
        // Vector3 offset = new Vector3(0.5f, 0, 0.5f);
        Vector3 offset = new Vector3(0f, 0.5f, 0f);
        spawnlocations = new List<float[]>();
        if (instaniate)
        {
            robots = new List<GameObject>();
        }

        if (agents != default)
        {
            num_of_agents = agents.Count;

            int i = 0;
            foreach (AgentData agent in agents)
            {
                float[] loc = agent.start;
                Vector3 pos = new Vector3(loc[0], 0, loc[1]) + offset;
                pos = Vector3.Scale(pos, scaling);
                if (instaniate)
                {
                    GameObject robot = Instantiate(robot_prefab, pos, Quaternion.identity);
                    robot.transform.parent = gameObject.transform.Find("Robots").transform;
                    robot.transform.localScale = scaling;
                    robot.GetComponent<Robot2>().setID(i + 1);
                    robot.GetComponent<Robot2>().setBoxSize(boxSize);
                    robot.name = "Robot_" + (i + 1);
                    robot.GetComponent<Robot2>().reset();
                    robots.Add(robot);
                }
                else
                {
                    robots[i].transform.position = pos;
                    robots[i].transform.rotation = Quaternion.identity;
                }
                i += 1;
            }
        }
        else if (num_of_agents > 0 && findValidPoint != default)
        {
            List<Vector3> position = new List<Vector3>();
            for (int i = 0; i < num_of_agents; i++)
            {
                bool isOverlapping = false;
                Vector3 pos = Vector3.zero;
                Quaternion orientation = Quaternion.identity;
                int count = 0;
                Vector3 halfExtents = robot_prefab.GetComponent<BoxCollider>().size; // 0.5 is half the size of the robot's dimension while tol is a minimum tolerance or spacing
                float radius = MathF.Sqrt(halfExtents.x * halfExtents.x + halfExtents.z * halfExtents.z);
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
                    GameObject robot = Instantiate(robot_prefab, pos, orientation);
                    robot.transform.parent = gameObject.transform.Find("Robots").transform;
                    robot.transform.localScale = Vector3.Scale(robot_prefab.transform.localScale, scaling);
                    robot.GetComponent<Robot2>().setID(i+1);
                    robot.GetComponent<Robot2>().setBoxSize(boxSize);
                    robot.GetComponent<Robot2>().setCollisionOn(false);
                    robot.name = "Robot_" + (i + 1);
                    robots.Add(robot);
                }
                else
                {
                    robots[i].GetComponent<Robot2>().updateSpawnState(pos, orientation);
                    robots[i].GetComponent<Robot2>().setCollisionOn(false);
                }
            }

            for (int i = 0; i < num_of_agents; i++)
            {
                robots[i].GetComponent<Robot2>().setCollisionOn(true);
            }

        }

        for (int i = 0; i < num_of_agents; i++)
        {
            float[] loc = new float[2]
            {
                robots[i].transform.position.x,
                robots[i].transform.position.z
            };
            spawnlocations.Add(loc);
        }
    }

    public void updateRobotParameters(parameters param)
    {
        foreach (GameObject robot in robots)
        {
            Robot2 robotObj = robot.GetComponent<Robot2>();
            robotObj.updateAgentParameters(param);
        }
    }

    public void setParameters(int num_spawn_tries = 1, float min_spacing = 0.1f, Vector3 boxSize = default)
    {
        this.num_spawn_tries = num_spawn_tries;
        this.min_spacing = min_spacing;
        this.boxSize = boxSize;
    }


    public List<GameObject> getRobots()
    {
        return robots;
    }

    public List<float[]> getSpawnLocations()
    {
        return spawnlocations;
    }

    public void setCommandInput(bool allowCommandsInput)
    {
        foreach (GameObject robot in robots)
        {
            Robot2 robotObj = robot.GetComponent<Robot2>();
            robotObj.setAllowCommandsInput(allowCommandsInput);
        }
    }

    public void DestroyAll()
    {
        foreach (Transform child in gameObject.transform.Find("Robots").transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in gameObject.transform.Find("Paths").transform)
        {
            Destroy(child.gameObject);
        }
        robots = new List<GameObject>();
        spawnlocations = new List<float[]>();
    }

    public void ResetAll()
    {
        foreach (GameObject robot in robots)
        {
            Robot2 robotObj = robot.GetComponent<Robot2>();
            robotObj.reset();
        }
    }   
}

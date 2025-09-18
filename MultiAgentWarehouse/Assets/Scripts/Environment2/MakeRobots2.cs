
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
    public List<List<int[]>> spawnlocations = new List<List<int[]>>();
    public GameObject robot_prefab; // Assign your obstacle prefab here
    public List<GameObject> robots = new List<GameObject>();
    public int num_spawn_tries = 1;
    public Vector3 boxSize = new Vector3(1f, 0.5f, 1f);
    public float min_spacing = 0.1f;

    public void initStartLocation(int num_of_agents = 0, List<AgentData> agents = default, Func<(Vector3, Quaternion)> findValidPoint = default, Vector3 scaling = default, bool instaniate = true)
    {
        if (agents == default && num_of_agents == 0 && findValidPoint == default)
        {
            Debug.LogError("No agent data provided");
            return;
        }
        // Vector3 offset = new Vector3(0.5f, 0, 0.5f);
        Vector3 offset = new Vector3(0f, 0.5f, 0f);
        if(instaniate)
        {
            robots = new List<GameObject>();
        }

        if (agents != default)
        {
            num_of_agents = agents.Count;
            spawnlocations = new List<List<int[]>>();
            int i = 0;
            foreach (AgentData agent in agents)
            {
                int[] loc = agent.start;
                Vector3 pos = new Vector3(loc[0], 0, loc[1]) + offset;
                pos = Vector3.Scale(pos, scaling);
                if (instaniate)
                {
                    GameObject robot = Instantiate(robot_prefab, pos, Quaternion.identity);
                    robot.transform.parent = gameObject.transform.Find("Robots").transform;
                    robot.transform.localScale = scaling;
                    robot.GetComponent<Robot2>().setID(i);
                    robot.GetComponent<Robot2>().boxSize = boxSize;
                    robot.name = "Robot_" + i;
                    robots.Add(robot);
                }
                else
                {
                    robots[i].transform.position = pos;
                    robots[i].transform.rotation = Quaternion.identity;
                    robots[i].GetComponent<Robot2>().reset();
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
                float radius = MathF.Sqrt(halfExtents.x*halfExtents.x + halfExtents.z*halfExtents.z);
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
                    count +=1;
                }
                // Debug.Log("i: " + i + " count: " + count);
                if (count == num_spawn_tries)
                {
                    Debug.LogWarning("Could not find non-overlapping spawn point for robot " + i + ". Placing it anyway.");
                }
                if(instaniate)
                {
                    GameObject robot = Instantiate(robot_prefab, pos, orientation);
                    robot.transform.parent = gameObject.transform.Find("Robots").transform;
                    robot.transform.localScale = Vector3.Scale(robot_prefab.transform.localScale, scaling);
                    robot.GetComponent<Robot2>().setID(i);
                    robot.GetComponent<Robot2>().boxSize = boxSize;
                    robot.GetComponent<Robot2>().setCollisionOn(false);
                    robot.name = "Robot_" + i;
                    robots.Add(robot);
                }
                else{
                    robots[i].transform.position = pos;
                    robots[i].transform.rotation = Quaternion.identity;
                    robots[i].GetComponent<Robot2>().setCollisionOn(false);
                    robots[i].GetComponent<Robot2>().reset();
                }   
            }

            for (int i = 0; i < num_of_agents; i++)
            {
                robots[i].GetComponent<Robot2>().setCollisionOn(true);
            }

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
        spawnlocations = new List<List<int[]>>();
    }
}

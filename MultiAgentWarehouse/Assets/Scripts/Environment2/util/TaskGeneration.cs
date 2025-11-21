using UnityEngine;
using System.Collections.Generic;
using multiagent.robot;
using multiagent.task;
using multiagent.taskgoal;
using UnityEngine.Assertions;
using multiagent.util;
using UnityEngine;
public class TaskGeneration
{
    int n_tasks_started = 0;
    int n_tasks, n_non_task;
    float task_freq;
    List<List<GameObject>> starts;
    List<List<GameObject>> goals;
    List<GameObject> non_tasks;
    List<Task> non_endpoint_tasks = new List<Task>(); //Tasks for agents to go to non-task endpoints
    List<int> robot_non_endpoint_id = new List<int>(); //Tasks for agents to go to non-task endpoints
    List<Task> tasks = new List<Task>();
    List<Task> available_tasks = new List<Task>();
    List<Task> incompleted_tasks = new List<Task>();
    List<int> csv_task_time_indices = new List<int>();
    System.Random random = new System.Random();
    bool verbose = false;
    bool showRenderer = false;

    public TaskGeneration(int n_tasks, float task_freq, List<List<GameObject>> starts, List<List<GameObject>> goals, List<GameObject> non_tasks, bool verbose = false)
    {
        this.n_tasks = n_tasks;
        this.task_freq = task_freq;
        this.starts = starts;
        this.goals = goals;
        this.non_tasks = non_tasks;
        this.n_non_task = non_tasks.Count;
        this.verbose = verbose;
    }

    public void GenerateNonEndpointTasks(List<float[]> robotSpawnLocations)
    {
        non_endpoint_tasks = new List<Task>();
        robot_non_endpoint_id = new List<int>();
        for (int i = 0; i < non_tasks.Count; i++)
        {
            List<GameObject> taskpoint = new List<GameObject>
            {
                non_tasks[i]
            };
            Task new_task = new Task(
                0,
                "non_task_" + (i + 1).ToString(),
                taskpoint,
                -(i + 1),
                verbose);
            non_endpoint_tasks.Add(new_task);
        }


        for (int j = 0; j < robotSpawnLocations.Count; j++)
        {
            for (int i = 0; i < non_tasks.Count; i++)
            {
                if (Mathf.Abs(robotSpawnLocations[j][0] - non_tasks[i].transform.position.x) < 1e-3 && Mathf.Abs(robotSpawnLocations[j][1] - non_tasks[i].transform.position.z) < 1e-3)
                {
                    robot_non_endpoint_id.Add(i);
                    break;
                }
            }
        }

        if (verbose)
            Debug.Log(non_endpoint_tasks.Count + " non-endpoint tasks generated ");
    }

    public void GenerateTasks()
    {
        n_tasks_started = 0;
        tasks = new List<Task>();
        for (int i = 0; i < n_tasks; i++)
        {
            int x = Random.Range(0, starts.Count);
            int y = Random.Range(0, goals.Count);

            List<GameObject> taskpoint = new List<GameObject>
            {
                starts[x][0],
                goals[y][0],
                goals[y][1],
                starts[x][1]
            };
            Task new_task = new Task(
                i * task_freq,
                "task_" + (i + 1).ToString(),
                taskpoint,
                i + 1,
                verbose);
            tasks.Add(new_task);
        }
    }

    public void DownloadTasks(List<TaskData> tasksData)
    {
        // TO DO
        n_tasks_started = 0;
        tasks = new List<Task>();
        Dictionary<(int, int), int> start_dict = new Dictionary<(int, int), int>();
        Dictionary<(int, int), int> goal_dict = new Dictionary<(int, int), int>();
        for (int j = 0; j < starts.Count; j++)
        {
            int[] tileArr = starts[j][0].GetComponent<Goal>().getTile();
            (int, int) key = (tileArr[0], tileArr[1]);
            start_dict[key] = j;
        }
        for (int j = 0; j < goals.Count; j++)
        {
            int[] tileArr = goals[j][0].GetComponent<Goal>().getTile();
            (int, int) key = (tileArr[0], tileArr[1]);
            goal_dict[key] = j;
        }


        for (int i = 0; i < n_tasks; i++)
        {
            List<int[]> waypoints = tasksData[i].waypoints;
            (int, int) startKey = (waypoints[0][0], waypoints[0][1]);
            (int, int) goalKey = (waypoints[1][0], waypoints[1][1]);
            // Debug.Log(start_dict[key]);
            int x = start_dict[startKey];
            int y = goal_dict[goalKey];

            List<GameObject> taskpoint = new List<GameObject>
            {
                starts[x][0],
                goals[y][0],
                goals[y][1],
                starts[x][1]
            };
            Task new_task = new Task(
                tasksData[i].start_time,
                tasksData[i].task_name,
                taskpoint,
                i + 1);
            tasks.Add(new_task);
        }
    }

    public void DownloadCSVTasks(List<GameObject> robots)
    {
        tasks = new List<Task>();
        n_tasks = robots.Count;
        for (int i = 0; i < n_tasks; i++)
        {
            int x = Random.Range(0, starts.Count);
            int y = Random.Range(0, goals.Count);

            List<GameObject> taskpoint = new List<GameObject>
            {
                starts[x][0],
                goals[y][0],
                goals[y][1],
                starts[x][1]
            };
            Task new_task = new Task(
                0,
                "task_" + (i + 1).ToString(),
                taskpoint,
                i + 1,
                verbose);
            new_task.assigned(i, robots[i].transform);
            tasks.Add(new_task);
        }
    }

    public void updateCSVTasks(float t,Dictionary<string, List<PositionOrientation>> agentPoses)
    {
        int robotID = 0;

        if (csv_task_time_indices.Count == 0)
        {
            for (int i = 0; i < agentPoses.Count; i++)
            {
                csv_task_time_indices.Add(0);
            }
        }

        int agent_index = 0;
        foreach (var agent in agentPoses)
        {
            List<PositionOrientation> agentPosesList = agent.Value;
            for (int i = csv_task_time_indices[agent_index]; i < agentPosesList.Count; i++)
            {
                PositionOrientation pos = agentPosesList[i];
                if (pos.time > t)
                {
                    break;
                }
                csv_task_time_indices[agent_index] = i + 1;
                csv_task_time_indices[agent_index] = Mathf.Min(csv_task_time_indices[agent_index], agentPosesList.Count - 1);
                

                if (pos.goalCategory == -1)
                {
                    tasks[agent_index].setTaskInd(-1);
                    continue;
                }

                GameObject goalClass = null;
                int goalID = pos.goalId;
                int goalType = pos.goalType;
                int task_ind = -1;
                if (pos.goalCategory == 0)
                {
                    goalClass = starts[goalID][goalType];
                    task_ind = goalType == 0 ? 0 : 3;
                }
                else if (pos.goalCategory == 1)
                {
                    goalClass = goals[goalID][goalType];
                    task_ind = goalType == 0 ? 1 : 2;
                }

                int current_task_ind = tasks[agent_index].getTaskInd();
                int[] goalTile = goalClass.GetComponent<Goal>().getTile();
                if(tasks[agent_index].getShowRenderer() && current_task_ind != task_ind)
                {
                    Transform goalClassParent = goalClass.transform.parent;
                    goalClass= UnityEngine.Object.Instantiate(goalClass);
                    goalClass.transform.parent = goalClassParent;
                    goalClass.GetComponent<Goal>().setParameters(pos.goalCategory, pos.goalId, pos.goalType, goalTile, 0f, 0f, 0f);
                    tasks[agent_index].taskpoint[task_ind] = goalClass;
                }
                if(!tasks[agent_index].getShowRenderer())
                {
                    tasks[agent_index].taskpoint[task_ind] = goalClass;
                }
                tasks[agent_index].setTaskInd(task_ind);
                
                
            }
            agent_index++;
        }
    }
    public void CheckAvailableTasks(float t)
    {
        for (int i = n_tasks_started; i < tasks.Count; i++)
        {
            if (tasks[i].start_time - 1e-5 <= t)
            {
                n_tasks_started += 1; ; // Mark as available
                available_tasks.Add(tasks[i]);
                incompleted_tasks.Add(tasks[i]);
            }
        }
        if (verbose && available_tasks.Count > 0)
            Debug.Log(available_tasks.Count + " available tasks ");
    }

    public List<Task> GetIncompleteTasks()
    {
        for (int i = incompleted_tasks.Count - 1; i >= 0; i--)
        {
            if (incompleted_tasks[i].isCompleted())
            {
                incompleted_tasks.RemoveAt(i);
            }
        }

        return incompleted_tasks;
    }

    public Task getNonEndpointTask()
    {
        if (non_endpoint_tasks.Count < 1)
        {
            return null;
        }
        int ind = random.Next(non_endpoint_tasks.Count);
        Task task = non_endpoint_tasks[ind];
        non_endpoint_tasks.RemoveAt(ind);
        return task;
    }

    public void AssignTaskEarlyStart(GameObject robot)
    {
        try
        {
            Robot robotComponent = robot.GetComponent<Robot>();
            if (available_tasks.Count < 1)
            {
                if (robot_non_endpoint_id.Count > 0)
                {
                    Task non_endpoint_task = non_endpoint_tasks[robot_non_endpoint_id[0]];
                    robotComponent.setGoal(non_endpoint_task);
                    non_endpoint_task.assigned(robotComponent.getID(), robot.transform);
                    if (verbose)
                        Debug.Log("Robot " + robotComponent.getID() + " assigned to non-endpoint task " + robotComponent.taskClass.task_name);
                    return;
                }
                robotComponent.setGoal(null);
                return;
            }
            if (robot_non_endpoint_id.Count > robotComponent.getID())
            {
                non_endpoint_tasks[robot_non_endpoint_id[robotComponent.getID()]].resetAll();
            }
            robotComponent.setGoal(available_tasks[0]); // Reset goal before assigning new one
            available_tasks[0].assigned(robotComponent.getID(), robot.transform);
            available_tasks.RemoveAt(0);
        }
        catch
        {
            Robot2 robotComponent = robot.GetComponent<Robot2>();


            if (available_tasks.Count < 1)
            {
                if (robotComponent.checkIsIdle() && robotComponent.taskClass != null && robotComponent.taskClass.checkNonEndpointTask())
                {
                    return;
                }

                if (robot_non_endpoint_id.Count > 0)
                {
                    Task non_endpoint_task = non_endpoint_tasks[robot_non_endpoint_id[robotComponent.getID()]];
                    robotComponent.setGoal(non_endpoint_task);
                    non_endpoint_task.assigned(robotComponent.getID(), robot.transform);
                    if (verbose)
                        Debug.Log("Robot " + robotComponent.getID() + " assigned to non-endpoint task " + robotComponent.taskClass.task_name);
                    return;
                }
                robotComponent.setGoal(null);
                return;
            }
            if (robot_non_endpoint_id.Count > robotComponent.getID())
            {
                non_endpoint_tasks[robot_non_endpoint_id[robotComponent.getID()]].resetAll();
            }
            robotComponent.setGoal(available_tasks[0]); // Reset goal before assigning new one
            available_tasks[0].assigned(robotComponent.getID(), robot.transform);
            available_tasks.RemoveAt(0);
            if (verbose)
                Debug.Log("Robot " + robotComponent.getID() + " assigned to task " + robotComponent.taskClass.task_name);
        }
    }

    public void assignTask(List<GameObject> robots, List<int> taskInds)
    {
        for (int i = 0; i < robots.Count; i++)
        {
            Assert.IsTrue(taskInds[i] <= n_tasks && taskInds[i] >= -n_non_task, "Robot Assignment Task index out of range");
            int taskInd;
            Task task;
            // No assignment if taskInd is -1
            if (taskInds[i] == 0)
            {
                continue;
            }
            else if (taskInds[i] < 0)
            {
                taskInd = -(taskInds[i] + 1);
                task = non_endpoint_tasks[taskInd];
            }
            else
            {
                taskInd = taskInds[i] - 1;
                task = tasks[taskInd];
                // No assignment if task is already completed
                if (tasks[taskInd].isCompleted())
                {
                    continue;
                }
            }

            try
            {
                Robot robotComponent = robots[i].GetComponent<Robot>();
                robotComponent.setGoal(task); // Reset goal before assigning new one
                task.assigned(robotComponent.getID(), robots[i].transform);
            }
            catch
            {
                Robot2 robotComponent = robots[i].GetComponent<Robot2>();
                robotComponent.setGoal(task); // Reset goal before assigning new one
                task.assigned(robotComponent.getID(), robots[i].transform);
            }
        }
        // Remove assigned tasks from available tasks in descending order to avoid index shifting
        for (int i = available_tasks.Count - 1; i >= 0; i--)
        {
            if (available_tasks[i].isAssigned())
            {
                available_tasks.RemoveAt(i);
            }
        }
    }

    public List<Task> getAllTasks()
    {
        return tasks;
    }

    public List<Task> getAvailableTasks()
    {
        return available_tasks;
    }

    public List<Task> getNonEndpointTasks()
    {
        return non_endpoint_tasks;
    }

    public void setShowRenderer(bool showRenderer)
    {
        this.showRenderer = showRenderer;
        foreach (Task task in tasks)
        {
            task.setShowRenderer(showRenderer);
        }
        foreach (Task task in non_endpoint_tasks)
        {
            task.setShowRenderer(showRenderer);
        }
    }

    public void activateRenderer()
    {
        foreach (Task task in tasks)
        {
            task.activateRenderer();
        }
    }

}
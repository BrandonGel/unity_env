using UnityEngine;
using System.Collections.Generic;
using multiagent.robot;
using multiagent.task;
using multiagent.taskgoal;
using UnityEngine.Assertions;
public class TaskGeneration
{
    int n_tasks_started = 0;
    int n_tasks;
    float task_freq;
    List<List<GameObject>> starts;
    List<List<GameObject>> goals;
    // List<List<GameObject>> goals;
    List<Task> tasks = new List<Task>();
    List<Task> available_tasks = new List<Task>();
    List<Task> incompleted_tasks = new List<Task>();
    System.Random random = new System.Random();
    bool verbose = false;


    public TaskGeneration(int n_tasks, float task_freq, List<List<GameObject>> starts, List<List<GameObject>> goals, bool verbose = false)
    {
        this.n_tasks = n_tasks;
        this.task_freq = task_freq;
        this.starts = starts;
        this.goals = goals;
        this.verbose = verbose;
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
                "task_" + (i+1).ToString(),
                taskpoint,
                i+1,
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
                i+1);
            tasks.Add(new_task);
        }
    }

    public void CheckAvailableTasks(float t)
    {
        for (int i = n_tasks_started; i < tasks.Count; i++)
        {
            if (tasks[i].start_time-1e-5 <= t)
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

    public void AssignTaskEarlyStart(GameObject robot)
    {
        try
        {
            Robot robotComponent = robot.GetComponent<Robot>();
            if (available_tasks.Count < 1)
            {
                robotComponent.setGoal(null);
                return;
            }
            robotComponent.setGoal(available_tasks[0]); // Reset goal before assigning new one
            available_tasks[0].assigned(robotComponent.getID());
            available_tasks.RemoveAt(0);
        }
        catch
        {
            Robot2 robotComponent = robot.GetComponent<Robot2>();
            if (available_tasks.Count < 1)
            {
                robotComponent.setGoal(null);
                return;
            }
            robotComponent.setGoal(available_tasks[0]); // Reset goal before assigning new one
            available_tasks[0].assigned(robotComponent.getID());
            available_tasks.RemoveAt(0);
            if (verbose)
                Debug.Log("Robot " + robotComponent.getID() + " assigned to task " + robotComponent.taskClass.task_name);
        }
    }

    public void assignTask(List<GameObject> robots, List<int> taskInds)
    {
        for (int i = 0; i < robots.Count; i++)
        {
            Assert.IsTrue(taskInds[i] <= n_tasks && taskInds[i] >= 0, "Robot Assignment Task index out of range");
            int taskInd = taskInds[i]-1;
            // No assignment if taskInd is -1
            if(taskInd < 0)
            {
                continue;
            }

            // No assignment if task is already completed
            if(tasks[taskInd].isCompleted())
            {
                continue;
            }

            try
            {
                Robot robotComponent = robots[i].GetComponent<Robot>();
                robotComponent.setGoal(tasks[taskInd]); // Reset goal before assigning new one
                tasks[taskInd].assigned(robotComponent.getID());
            }
            catch
            {
                Robot2 robotComponent = robots[i].GetComponent<Robot2>();
                robotComponent.setGoal(tasks[taskInd]); // Reset goal before assigning new one
                tasks[taskInd].assigned(robotComponent.getID());
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


}
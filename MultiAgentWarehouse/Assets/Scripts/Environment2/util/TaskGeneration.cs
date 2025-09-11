using UnityEngine;
using System.Collections.Generic;
using multiagent.robot;
using multiagent.task;
using multiagent.taskgoal;
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


    public TaskGeneration(int n_tasks, float task_freq, List<List<GameObject>> starts, List<List<GameObject>> goals)
    {
        this.n_tasks = n_tasks;
        this.task_freq = task_freq;
        this.starts = starts;
        this.goals = goals;
    }

    public void GenerateTasks()
    {
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
                "task_" + i.ToString(),
                taskpoint);
            tasks.Add(new_task);
        }
    }

    public void DownloadTasks(List<TaskData> tasksData)
    {
        // TO DO
        tasks = new List<Task>();
        Dictionary<(int,int), int> start_dict = new Dictionary<(int,int), int>();
        Dictionary<(int,int), int> goal_dict = new Dictionary<(int,int), int>();
        for (int j = 0; j < starts.Count; j++)
        {
            int[] tileArr = starts[j][0].GetComponent<Goal>().getTile();
            (int,int) key = (tileArr[0], tileArr[1]);
            start_dict[key] = j;
        }
        for (int j = 0; j < goals.Count; j++)
        {
            int[] tileArr = goals[j][0].GetComponent<Goal>().getTile();
            (int,int) key = (tileArr[0], tileArr[1]);
            goal_dict[key] = j;
        }


        for (int i = 0; i < n_tasks; i++)
        {
            List<int[]> waypoints = tasksData[i].waypoints;
            (int,int) startKey = (waypoints[0][0], waypoints[0][1]);
            (int,int) goalKey = (waypoints[1][0], waypoints[1][1]);
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
                taskpoint);
            tasks.Add(new_task);
        }
    }

    public void CheckAvailableTasks(float t)
    {
        for (int i = n_tasks_started; i < tasks.Count; i++)
        {
            if (tasks[i].start_time <= t)
            {
                n_tasks_started += 1; ; // Mark as available
                available_tasks.Add(tasks[i]);
                incompleted_tasks.Add(tasks[i]);
            }
        }
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
        Robot robotComponent = robot.GetComponent<Robot>();

        if (available_tasks.Count < 1)
        {
            robotComponent.setGoal(null);
            return;
        }
        Debug.Log(available_tasks.Count + " available tasks");
        robotComponent.setGoal(available_tasks[0]); // Reset goal before assigning new one
        available_tasks[0].assigned(robotComponent.getID());
        available_tasks.RemoveAt(0);
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
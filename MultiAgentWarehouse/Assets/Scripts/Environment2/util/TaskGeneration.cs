using UnityEngine;
using System.IO;
using System.Collections.Generic;
using multiagent.robot;
public class TaskGeneration
{
    int n_tasks_started = 0;
    int n_tasks;
    float task_freq;
    List<List<GameObject>> starts;
    List<List<GameObject>> goals;
    // List<List<GameObject>> goals;
    List<task> tasks = new List<task>();
    List<task> available_tasks = new List<task>();
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
        tasks = new List<task>();
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
            task new_task = new task(
                i * task_freq,
                "task_" + i.ToString(),
                taskpoint);
            tasks.Add(new_task);
        }
    }

    public void DownloadTasks(List<TaskData> tasksData)
    {
        // TO DO
        tasks = new List<task>();
        for (int i = 0; i < n_tasks; i++)
        {
            List<int[]> waypoints = tasksData[i].waypoints;
            int x = 0;
            int y = 0;
            for (int j = 0; j < starts.Count; j++)
            {
                if (waypoints[0][0] == starts[j][0].transform.position.x && waypoints[0][1] == starts[j][0].transform.position.z)
                {
                    x = j;
                    break;
                }
            }
            for (int j = 0; j < goals.Count; j++)
            {
                if (waypoints[1][0] == goals[j][0].transform.position.x && waypoints[1][1] == goals[j][0].transform.position.z)
                {
                    y = j;
                    break;
                }
            }
            List<GameObject> taskpoint = new List<GameObject>
            {
                starts[x][0],
                goals[y][0],
                goals[y][1],
                starts[x][1]
            };


            task new_task = new task(
                tasksData[i].start_time,
                tasksData[i].task_name,
                taskpoint);
            tasks.Add(new_task);
        }
    }

    public void CheckAvailableGoals(float t)
    {
        for (int i = n_tasks_started; i < tasks.Count; i++)
        {
            if (tasks[i].start_time <= t)
            {
                n_tasks_started += 1; ; // Mark as available
                available_tasks.Add(tasks[i]);
            }
        }
    }

    public void AssignGoals(GameObject robot)
    {
        Robot robotComponent = robot.GetComponent<Robot>();

        if (available_tasks.Count < 1 )
        {
            robotComponent.setGoal(null);
            return;
        }

        for (int i = 0; i < available_tasks.Count; i++)
        {
            robotComponent.setGoal(available_tasks[0]); // Reset goal before assigning new one
            available_tasks[0].assigned(robotComponent.getID());
            available_tasks.RemoveAt(0);
        }
    }

    public List<task> getAllTasks()
    {
        return tasks;
    }

    public List<task> getAvailableTasks()
    {
        return available_tasks;
    }


}
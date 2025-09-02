

using System.Collections.Generic;
using UnityEngine;
using multiagent.util;
using System;
using UnityEngine.PlayerLoop;
using System.Linq;


public class task
{
    public float start_time = 0f;
    public string task_name = "";
    public List<GameObject> taskpoint = new List<GameObject>();
    public int assignedRobotID = -1;
    private bool _busy = false;
    private bool _completed = false;
    private int task_ind = -1;

    public task(float start_time, string task_name, List<GameObject> taskpoint)
    {
        this.start_time = start_time;
        this.task_name = task_name;
        this.taskpoint = taskpoint;
        assignedRobotID = -1;
        _busy = false;
        _completed = false;
        task_ind = 0;
    }

    public bool isAssigned()
    {
        if (assignedRobotID >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void exchange(int oldRobotID, int newRobotID)
    {
        UnityEngine.Assertions.Assert.IsTrue(oldRobotID >= 0);
        UnityEngine.Assertions.Assert.IsTrue(newRobotID >= 0);
        assignedRobotID = newRobotID;
    }

    public void removed()
    {
        assignedRobotID = -1;
    }

    public void assigned(int robotID)
    {
        UnityEngine.Assertions.Assert.IsTrue(robotID >= 0);
        assignedRobotID = robotID;
    }

    public void moveNext()
    {
        if (task_ind + 1 < taskpoint.Count)
        {
            task_ind += 1;
        }
        else
        {
            _completed = true;
        }

    }
    public void reachGoal()
    {
        _busy = true;
        taskpoint[task_ind].GetComponent<goal>().setBusy();
    }

    public bool isBusy()
    {
        // Return false if already not busy
        if (!_busy)
        {
            return false;
        }

        // Update busy status
        _busy = taskpoint[task_ind].GetComponent<goal>().isBusy();
        if (!_busy)
        {
            moveNext();
        }
        return _busy;
    }

    public bool isCompleted()
    {
        return _completed;
    }

    public void resetAll()
    {
        assignedRobotID = -1;
        _busy = false;
        _completed = false;
    }

    public GameObject getCurrentGoal()
    {
        GameObject currentGoal = taskpoint[task_ind];
        return currentGoal;
    }

    public int getCurrentGoalID()
    {
        return taskpoint[task_ind].GetComponent<goal>().goalID;
    }
    public int getCurrentGoalType()
    {
        return taskpoint[task_ind].GetComponent<goal>().goalType;
    }



}

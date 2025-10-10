

using System.Collections.Generic;
using UnityEngine;
using multiagent.util;
using System;
using UnityEngine.PlayerLoop;
using System.Linq;
using multiagent.taskgoal;

namespace multiagent.task
{
    public class Task
    {
        public float start_time = 0f;
        public string task_name = "";
        public List<GameObject> taskpoint = new List<GameObject>();
        public int assignedRobotID = -1;
        private bool _busy = false;
        private bool _completed = false;
        public int task_ind = -1;
        public int taskID = 0;
        public bool is_non_endpoint_task = false;
        public bool verbose = false;

        public Task(float start_time, string task_name, List<GameObject> taskpoint, int taskID = -1, bool verbose = false)
        {
            this.start_time = start_time;
            this.task_name = task_name;
            this.taskpoint = taskpoint;
            assignedRobotID = -1;
            _busy = false;
            _completed = false;
            task_ind = 0;
            this.taskID = taskID;
            this.is_non_endpoint_task = task_name.Contains("non_task");
            Debug.Log("Task: " + task_name + " is_non_endpoint_task: " + is_non_endpoint_task);
            this.verbose = verbose;
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
            if (verbose)
                Debug.Log("Task: " + task_name + " assigned to Robot " + assignedRobotID);
        }

        public void moveNext()
        {
            if (task_ind + 1 < taskpoint.Count)
            {
                if (verbose)
                    Debug.Log("Task: " + task_name + " moving to next goal " + (task_ind + 1) + " by Robot " + assignedRobotID);
                task_ind += 1;
            }
            else
            {
                // Debug.Log("Task: " + task_name + " completed by Robot " + assignedRobotID);
                _completed = true;
            }

        }
        public void reachGoal()
        {
            if (verbose)
                Debug.Log("Task: " + task_name + " Goal " + task_ind + " reached by Robot " + assignedRobotID);
            _busy = true;
            taskpoint[task_ind].GetComponent<Goal>().setBusy();
        }

        public bool isBusy()
        {
            // Return false if already not busy
            if (!_busy)
            {
                return false;
            }

            // Update busy status
            _busy = taskpoint[task_ind].GetComponent<Goal>().isBusy();
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
            return taskpoint[task_ind].GetComponent<Goal>().goalID;
        }
        public int getCurrentGoalType()
        {
            return taskpoint[task_ind].GetComponent<Goal>().goalType;
        }

        public bool checkNonEndpointTask()
        {
            return is_non_endpoint_task;
        }
    }
}
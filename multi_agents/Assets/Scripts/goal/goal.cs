//Author: Angel Ortiz
//Date: 08/15/17

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NUnit.Framework;
using UnityEngine.Assertions;
using multiagent.goal;


public class Goal : MonoBehaviour
{

    public Vector3 position;
    public Vector2 position2D;
    public bool inUsed = false;
    public int robotID = -1;
    public int startTimeStep = 0;
    public int counter = 0;
    public int totalTimesteps = 1;
    public float goalWait = 100f;
    public float goalWaitProbability = 1f;
    public int goalID = -1;
    public int goalType = 0;
    public data history;
    public goalClass goalData;
    public class data
    {
        public bool[] inUsed;
        public int[] robotID;
        public data(int totalTimesteps)
        {
            inUsed = new bool[totalTimesteps];
            robotID = new int[totalTimesteps];
        }

        public void set(int idx, bool inUsed, int robotID)
        {
            this.inUsed[idx] = inUsed;
            this.robotID[idx] = robotID;
        }
    }

    void Start()
    {
        position = transform.position;
        position2D = new Vector2(transform.position.x, transform.position.z);
    }

    public void setInUsed(int robotID, int startTimeStep)
    {
        UnityEngine.Assertions.Assert.IsTrue(robotID >= 0);
        UnityEngine.Assertions.Assert.IsTrue(startTimeStep >= 0);
        inUsed = true;
        this.robotID = robotID;
        this.startTimeStep = startTimeStep;
        history.set(counter,inUsed,robotID);
    }

    public void completed(int startTimeStep)
    {
        UnityEngine.Assertions.Assert.IsTrue(robotID >= 0);
        UnityEngine.Assertions.Assert.IsTrue(startTimeStep >= 0);
        inUsed = false;
        robotID = -1;
        this.startTimeStep = startTimeStep;
        history.set(counter,inUsed,robotID);
    }

    public void resetData(int totalTimesteps=1)
    {
        history = new data(totalTimesteps);
    }

    void Update()
    {
        counter += 1;
        // history.set(counter,inUsed,robotID);
    }

}

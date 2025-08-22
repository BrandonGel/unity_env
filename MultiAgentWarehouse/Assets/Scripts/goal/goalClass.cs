

using System.Collections.Generic;
using UnityEngine;
using multiagent.util;
using System;
using UnityEngine.PlayerLoop;

namespace multiagent.goal
{
    public class goalClass
    {
        public Vector3 position = default;
        public int goalID = -1;
        public int goalType = 0;
        public float goalWait = 0f;
        public float goalWaitProbability = 1f;
        public Goal goalObj = null;
        public int[] inUsed; // 0 - not in used, 1 - assigned, 2 - completed, 3 - switched
        public bool anyAssigned = false;
        public data history;
        public goalClass(Goal goalObj = null, int numAgents = 1)
        {
            if (goalObj != null)
            {
                this.position = goalObj.transform.position;
                this.goalID = goalObj.goalID;
                this.goalType = goalObj.goalType;
                this.goalWait = goalObj.goalWait;
                this.goalWaitProbability = goalObj.goalWaitProbability;
                this.goalObj = goalObj;
            }
            inUsed = new int[numAgents];
            Array.Fill(inUsed,0);
            Util.enableRenderer(this.goalObj.GetComponent<Renderer>(), false);
        }

        public void assigned(int robotID, int startTimeStep)
        {
            UnityEngine.Assertions.Assert.IsTrue(robotID >= 0);
            UnityEngine.Assertions.Assert.IsTrue(startTimeStep >= 0);
            inUsed[robotID] = 1;
        }

        public void completed(int robotID, int startTimeStep)
        {
            UnityEngine.Assertions.Assert.IsTrue(robotID >= 0);
            UnityEngine.Assertions.Assert.IsTrue(startTimeStep >= 0);
            inUsed[robotID] = 2;
        }
        
        public void switched(int robotID, int startTimeStep)
        {
            UnityEngine.Assertions.Assert.IsTrue(robotID >= 0);
            UnityEngine.Assertions.Assert.IsTrue(startTimeStep >= 0);
            inUsed[robotID] = 3;
        }

        public void updateInUsed()
        {
            anyAssigned = false;
            for (int ii = 0; ii < inUsed.Length; ii++)
            {
                if (inUsed[ii] > 1)
                {
                    inUsed[ii] = 0;
                    continue;
                }
                else if (inUsed[ii] == 1)
                {
                    anyAssigned = true;
                    Util.enableRenderer(this.goalObj.GetComponent<Renderer>(), true);
                }
            }
            history.set(inUsed);
            if (!anyAssigned)
            {
                Util.enableRenderer(this.goalObj.GetComponent<Renderer>(), false);
            }
        }

        public void resetData(int numAgents = 1)
        {
            history = new data(numAgents);
            inUsed = new int[numAgents];
            Array.Fill(inUsed, 0);
            Util.enableRenderer(this.goalObj.GetComponent<Renderer>(), false);
        }

        public class data
        {
            int numAgents;
            public List<int[]> inUsedList;
            public data(int numAgents)
            {
                inUsedList = new List<int[]>();
                this.numAgents= numAgents;
            }

            public void set(int[] inUsed)
            {
                UnityEngine.Assertions.Assert.IsTrue(inUsed.Length == numAgents);
                inUsedList.Add(inUsed);
            }
        }
    }

}

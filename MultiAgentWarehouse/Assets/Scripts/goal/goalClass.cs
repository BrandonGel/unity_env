

using System.Collections.Generic;
using UnityEngine;


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
        public goalClass(Goal goalObj = null)
        {
            if (goalObj != null)
            {
                this.position = goalObj.position;
                this.goalID = goalObj.goalID;
                this.goalType = goalObj.goalType;
                this.goalWait = goalObj.goalWait;
                this.goalWaitProbability = goalObj.goalWaitProbability;
                this.goalObj = goalObj;
            }
        }

    }

}

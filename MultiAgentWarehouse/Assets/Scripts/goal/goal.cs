

using System.Collections.Generic;
using UnityEngine;
using multiagent.util;
using System;
using UnityEngine.PlayerLoop;
using System.Linq;

namespace multiagent.goal
{
    public class Goal : MonoBehaviour
    {

        public Vector3 position;
        private Dictionary<int, bool> anyAssigned = new Dictionary<int, bool>();
        private int _counter = 0;
        private int _lastRobotID = -1;
        private bool _busy = false;
        public float goalWait = 1f;
        public int goalDelayWait = 0;
        public float goalDelayPenalty = 0f;
        public float goalWaitProbability = 1f;
        public int goalID = -1;
        public int goalType = 0;
        System.Random random;

        void Start()
        {
            position = transform.position;
        }

        public int getCounter()
        {
            return _counter;
        }

        public void assigned(int robotID)
        {
            UnityEngine.Assertions.Assert.IsTrue(robotID >= 0);
            anyAssigned[robotID] = true;
            Util.enableRenderer(GetComponent<Renderer>(), true);
        }

        public void completed(int robotID)
        {
            UnityEngine.Assertions.Assert.IsTrue(robotID >= 0);
            anyAssigned.Remove(robotID);
            _lastRobotID= robotID;
            _busy = true;
            // Turn off the renderer when there is no robot assigned to this goal location
            if (anyAssigned.Count == 0)
            {
                Util.enableRenderer(GetComponent<Renderer>(), false);
            }
        }

        public bool isBusy()
        {
            return _busy;
        }

        public void resetAll()
        {
            anyAssigned = new Dictionary<int, bool>();
            Util.enableRenderer(GetComponent<Renderer>(), false);
            _counter = 0;
            random = new System.Random();
        }

        void FixedUpdate()
        {
            // If the goal is not busy (loading/unloading the cargo), then skip
            if (!_busy)
            {
                return;
            }

            _counter += 1;

            // If the goal is currently busy (loading/unloading the cargo), then skip till time duration
            float waitTime = goalWait + goalDelayWait*goalDelayPenalty;
            if (_counter * Time.fixedDeltaTime < waitTime)
            {
                return;
            }

            float x = (float)random.NextDouble();
            if (x < goalWaitProbability)
            {
                goalDelayWait += 1;
                return;
            }

            if (goalDelayWait > 0)
            {
                Debug.Log("Delay Occurred for Robot " + _lastRobotID + " at Goal " + goalID + " | Delay Time: " + goalDelayWait*goalDelayPenalty);
            }

            goalDelayWait =0;
            _busy = false;
            
        }



    }
}


using System.Collections.Generic;
using UnityEngine;
using multiagent.util;
using System;
using UnityEngine.PlayerLoop;
using System.Linq;

namespace multiagent.taskgoal
{
    public class Goal : MonoBehaviour
    {
        private int _counter = 0;
        private System.Random random;
        private bool _busy = false;
        private bool _showRenderer = false;
        private int[] tile;
        public float goalWait = 1f;
        public int goalDelayWait = 0;
        public float goalDelayPenalty = 0f;
        public float goalWaitProbability = 1f;
        public int goalID = -1;
        public int goalType = 0;
        public Vector3 initialPosition = Vector3.zero;

        void Start()
        {
            // Util.enableRenderer(GetComponent<Renderer>(), false);
            random = new System.Random();
            initialPosition = transform.position;
        }

        public int getCounter()
        {
            return _counter;
        }

        public void setBusy()
        {
            _busy = true;
        }

        public bool isBusy()
        {
            return _busy;
        }

        public int[] getTile()
        {
            return tile;
        }

        public void setParameters(int goalID, int goalType, int[] tile, float goalWait = 0f, float goalDelayPenalty = 0f, float goalWaitProbability = 1f)
        {
            this.goalID = goalID;
            this.goalType = goalType;
            this.tile = tile;
            this.goalWait = goalWait;
            this.goalDelayPenalty = goalDelayPenalty;
            this.goalWaitProbability = goalWaitProbability;
        }

        public void resetAll()
        {
            Util.enableRenderer(GetComponent<Renderer>(), false);
            _counter = 0;
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
            float waitTime = goalWait + goalDelayWait * goalDelayPenalty;
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

            // if (goalDelayWait > 0)
            // {
            //     Debug.Log("Delay Occurred for Robot " + _lastRobotID + " at Goal " + goalID + " | Delay Time: " + goalDelayWait*goalDelayPenalty);
            // }

            goalDelayWait = 0;
            _busy = false;
        }

        public void setShowRenderer(bool showRenderer)
        {
            _showRenderer = showRenderer;
            Util.enableRenderer(GetComponent<Renderer>(), showRenderer);
        }

        public bool getShowRenderer()
        {
            return _showRenderer;
        }

        public void setPosition(Vector3 position)
        {
            transform.position = position;
        }

        public Vector3 getPosition()
        {
            return transform.position;
        }

        public void resetPosition()
        {
            transform.position = initialPosition;
        }
    }


}

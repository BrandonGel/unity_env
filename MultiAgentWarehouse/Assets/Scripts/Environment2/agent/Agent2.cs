using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using System.Collections.Generic;
using System;
using multiagent.task;
using multiagent.taskgoal;

namespace multiagent.robot
{

    public class Agent2Template : MonoBehaviour
    {
        [SerializeField] private Vector3 _goal;
        public Vector3 boxSize = new Vector3(1f, 0.5f, 1f);
        [HideInInspector] public int CurrentEpisode = 0;
        [HideInInspector] public float CumulativeReward = 0f;
        public float[] minLim, maxLim;
        private int _id = -1;
        public Task taskClass = null;
        private bool _goalReached = false;
        private bool _wait = false;
        [SerializeField] private float _collisionEnterReward = -1f;
        [SerializeField] private float _collisionStayReward = -0.05f;
        [SerializeField] private float _timeReward = -2f;
        [SerializeField] private float _goalReward = 1f;
        public Material collisionMaterial;
        private Dictionary<Renderer, Material> originalColors = new Dictionary<Renderer, Material>();
        private bool collisionOn = true;
        private bool isColliding = false;
        public int MaxStep = 1000;
        public int StepCount = 0;
        public float Reward = 0;
        public int DecisionPeriod = 0;
        private int _collisionTagID = 0;
        public bool verbose = false;
        public bool _isIdle = true;

        public void setDecisionRequestParams(int maxTimeSteps, int decisionPeriod)
        {
            MaxStep = maxTimeSteps;
            DecisionPeriod = decisionPeriod;
        }

        public void Reset()
        {
            StepCount = 0;
            CumulativeReward = 0;
            Reward = 0;
            _collisionTagID = 0;
            _goalReached = false;
            _wait = false;
        }

        public void step(float[] action)
        {

        }

        private void Update()
        {
            ;
        }

        public void AddReward(float reward)
        {
            Reward += reward;
            CumulativeReward += reward;
        }

        public void addTimeReward()
        {
            AddReward(_timeReward / MaxStep);
        }

        public void addGoalReward()
        {
            AddReward(_goalReward);
        }

        public virtual void setGoal(Task taskClass = null)
        {

            this.taskClass = taskClass;
            if (taskClass == null)
            {
                this._goal = transform.position;
                _isIdle = true;
            }
            else
            {
                this._goal = taskClass.getCurrentGoal().transform.position;
                _isIdle = taskClass.checkNonEndpointTask();
            }
        }


        public virtual void GoalReached()
        {
            if (_goalReached == false)
            {
                _goalReached = true;
                AddReward(_goalReward);
                taskClass.reachGoal();
                _wait = true;
            }
        }

        public bool getGoalReached()
        {
            return _goalReached;
        }

        public bool getTaskReached()
        {
            if (taskClass == null)
            {
                return false;
            }
            _isIdle = taskClass.checkNonEndpointTask() || taskClass.isCompleted() == true;
            return taskClass.isCompleted();
        }

        public Vector3 getGoalPos()
        {
            return _goal;
        }

        public bool checkIsIdle()
        {
            return _isIdle;
        }


        public virtual bool checkWait()
        {
            // If no task assigned, return false
            if (taskClass == null)
            {
                _wait = false;
                _goalReached = false;
                return false;
            }

            // If already waiting, return true
            if (_wait == true && taskClass.isBusy() == false)
            {
                if (taskClass.isCompleted())
                {
                    setGoal(null);
                }
                else
                {
                    _goal = taskClass.getCurrentGoal().transform.position;
                }
                _goalReached = false;
                _wait = false;
                return false;
            }
            _wait = taskClass.isBusy();
            return _wait;

        }

        private void OnTriggerStay(Collider other)
        {
            bool isCenterInside = other.bounds.Contains(transform.position);
            bool isIDmatched = false;
            bool isTypeMatched = false;
            bool isNameMatched = false;
            if (other.gameObject.GetComponent<Goal>() != null && taskClass != null)
            {
                isIDmatched = other.gameObject.GetComponent<Goal>().goalID == taskClass.getCurrentGoal().GetComponent<Goal>().goalID;
                isTypeMatched = other.gameObject.GetComponent<Goal>().goalType == taskClass.getCurrentGoal().GetComponent<Goal>().goalType;
                isNameMatched = other.gameObject.name == taskClass.getCurrentGoal().name;
            }

            if (other.gameObject.CompareTag("Goal") && isIDmatched && isTypeMatched && isNameMatched && isCenterInside)
            {
                GoalReached();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {


            isColliding = true;
            if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Human"))
            {
                if(collisionOn==false){
                    Collider thisCollider = this.GetComponent<Collider>();
                    Collider otherCollider = collision.gameObject.GetComponent<Collider>();
                    if(thisCollider!=null && otherCollider!=null){
                        Physics.IgnoreCollision(thisCollider, otherCollider);
                    }
                    return;
                }
                AddReward(_collisionEnterReward);
                changeMaterialColor("c");
                switch (collision.gameObject.tag)
                {
                    case "Wall":
                        _collisionTagID = 1;
                        break;
                    case "Player":
                        _collisionTagID = 2;
                        break;
                    case "Human":
                        _collisionTagID = 3;
                        break;
                    default:
                        _collisionTagID = 0;
                        break;
                }
                if (verbose)
                {
                    if (collision.gameObject.CompareTag("Player"))
                    {
                        Robot2 otherRobot = collision.gameObject.GetComponent<Robot2>();
                        if (otherRobot != null)
                        {
                            Debug.Log("Collision with Robot " + otherRobot.getID() + " by Robot " + getID());
                        }
                    }
                    else
                    {
                        Debug.Log("Collision with " + collision.gameObject.tag + " by Robot " + getID());
                    }
                }   
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Human"))
            {
                if(collisionOn==false){
                    Collider thisCollider = this.GetComponent<Collider>();
                    Collider otherCollider = collision.gameObject.GetComponent<Collider>();
                    if(thisCollider!=null && otherCollider!=null){
                        Physics.IgnoreCollision(thisCollider, otherCollider);
                    }
                    return;
                }
                AddReward(_collisionStayReward * Time.fixedDeltaTime);
                if (verbose)
                {
                    if (collision.gameObject.CompareTag("Player"))
                    {
                        Robot2 otherRobot = collision.gameObject.GetComponent<Robot2>();
                        if (otherRobot != null)
                        {
                            Debug.Log("Colliding with Robot " + otherRobot.getID() + " by Robot " + getID());
                        }
                    }
                    else
                    {
                        Debug.Log("Colliding with " + collision.gameObject.tag + " by Robot " + getID());
                    }
                }
                    
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            isColliding = false;
            if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Human"))
            {
                _collisionTagID = 0;
                if(collisionOn==false){
                    Collider thisCollider = this.GetComponent<Collider>();
                    Collider otherCollider = collision.gameObject.GetComponent<Collider>();
                    if(thisCollider!=null && otherCollider!=null){
                        Physics.IgnoreCollision(thisCollider, otherCollider);
                    }
                    return;
                }

                changeMaterialColor();
                if (verbose)
                {
                    if (collision.gameObject.CompareTag("Player"))
                    {
                        Robot2 otherRobot = collision.gameObject.GetComponent<Robot2>();
                        if (otherRobot != null)
                        {
                            Debug.Log("Collision ended with Robot " + otherRobot.getID() + " by Robot " + getID());
                        }
                    }
                    else
                    {
                        Debug.Log("Collision ended with " + collision.gameObject.tag + " by Robot " + getID());
                    }
                }   
                    
            }
        }

        public void changeMaterialColor(String c = default)
        {
            foreach (KeyValuePair<Renderer, Material> entry in originalColors)
            {
                if (entry.Key != null && entry.Key.material != null)
                {
                    if (c == default)
                    {
                        entry.Key.material = entry.Value;
                    }
                    else if (c == "c")
                    {
                        entry.Key.material = collisionMaterial;
                    }

                }
            }
        }

        public virtual bool checkTerminalCondition()
        {

            if (StepCount == MaxStep - 1)
            {
                return true;
            }
            return false;
        }


        public void initExtra()
        {
            foreach (Renderer childRenderer in transform.Find("Body").GetComponentsInChildren<Renderer>())
            {
                if (childRenderer.material != null && !childRenderer.name.Contains("Particle System"))
                {
                    originalColors[childRenderer] = childRenderer.material;
                }
            }
        }

        public void setID(int id)
        {
            _id = id;
        }

        public int getID()
        {
            return _id;
        }

        public void setBoxSize(Vector3 boxSize)
        {
            this.boxSize = boxSize;
        }

        public void modifyReward(float collisionEnterReward = -1f, float collisionStayReward = -0.05f, float timeReward = -2f, float goalReward = 1f)
        {
            _collisionEnterReward = collisionEnterReward;
            _collisionStayReward = collisionStayReward;
            _timeReward = timeReward;
            _goalReward = goalReward;
        }

        public void setCollisionOn(bool collisionOn)
        {
            this.collisionOn = collisionOn;
        }

        public bool checkCollision()
        {
            return isColliding;
        }

        public int getCollisionTagID()
        {
            return _collisionTagID;
        }

        public void setCollisionTagID(int collisionTagID)
        {
            _collisionTagID = collisionTagID;
            if(collisionTagID > 0)
            {
                changeMaterialColor("c");
            }
            else
            {
                changeMaterialColor();
            }
        }
    }

}

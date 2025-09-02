using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using multiagent.goal;
using System.Collections.Generic;
using System;

namespace multiagent.agent
{
    public enum ctrlType
    {
        Position, //Position input
        Velocity, //Velocity input
        Accleration, //acceleration input
        Position_Velocity,
            
        }
    public class AgentTemplate : Agent
    {
        [SerializeField] private Vector3 _goal;
        public Vector3 boxSize, spawningOffset;
        [HideInInspector] public int CurrentEpisode = 0;
        [HideInInspector] public float CumulativeReward = 0f;
        public float[] minLim, maxLim;
        private bool _goalReached = false;
        public Goal _goalClass = null;
        private int _id = -1;
        
        private bool _wait = false;
        [SerializeField] private float _collisionEnterReward = -1f;
        [SerializeField] private float _collisionStayReward = -0.05f;
        [SerializeField] private float _timeReward = -2f;
        [SerializeField]private float _goalReward = 1f;
        public Material collisionMaterial;
        private Dictionary<Renderer, Material> originalColors = new Dictionary<Renderer, Material>();

        public void setDecisionRequestParams(int maxTimeSteps, int decisionPeriod)
        {
            MaxStep = maxTimeSteps;
            GetComponent<DecisionRequester>().DecisionPeriod = decisionPeriod;
        }

        public override void Initialize()
        {
            ;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            ;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            ;
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            ;
        }

        public virtual void MoveAgent(ActionSegment<float> action)
        {
            ;
        }


        private void Update()
        {
            ;
        }

        public void addTimeReward()
        {
            AddReward(_timeReward / MaxStep);
        }

        public void addGoalReward()
        {
            AddReward(_goalReward);
        }

        public virtual void setGoal(Goal _goalClass = null)
        {
            
            this._goalClass = _goalClass;
            if (_goalClass == null)
            {
                this._goal = Vector3.zero;
            }
            else
            {
                this._goal = _goalClass.position;    
            }
            _goalReached = false;
        }

        public virtual Goal getGoal()
        {
            return _goalClass;
        }

        public virtual void GoalReached()
        {
            if (_goalReached == false)
            {
                _goalReached = true;
                AddReward(_goalReward);
                CumulativeReward = GetCumulativeReward();

            }
        }

        public bool getGoalReached()
        {
            return _goalReached;
        }

        public void setGoalReached(bool b)
        {
            _goalReached = b;
            _wait = true;
            _goalClass.completed(_id);
        }

        public Vector3 getGoalPos()
        {
            return _goal;
        }


        public virtual bool checkWait()
        {
            if(_wait && _goalClass != null)
            {
                _wait = _wait & _goalClass.isBusy();
                return _wait;
            }
            return _wait;
        }

        private void OnTriggerStay(Collider other)
        {
            bool isCenterInside = other.bounds.Contains(transform.position);
            if (other.gameObject.CompareTag("Goal") && _goalClass== other.GetComponent<Goal>() && isCenterInside)
            {
                GoalReached();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Player"))
            {
                AddReward(_collisionEnterReward);
                // Get all Renderer components in children (including inactive ones if desired)
                changeMaterialColor("c");
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Player"))
            {
                AddReward(_collisionStayReward * Time.fixedDeltaTime);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Player"))
            {
                changeMaterialColor();
            }
        }

        public void changeMaterialColor(String c = default)
        {
            foreach (KeyValuePair<Renderer, Material> entry in originalColors)
                {
                    if (entry.Key != null && entry.Key.material != null)
                    {
                        if(c == default)
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

        public virtual void GetGround()
        {
            Transform ground = transform.parent.transform.parent.Find("Ground").GetComponent<Transform>();
            spawningOffset = ground.localPosition;
            boxSize.x = 2 * Mathf.Abs(ground.localPosition.x);
            boxSize.z = 2 * Mathf.Abs(ground.localPosition.z);
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

        public void modifyReward(float collisionEnterReward =-1f, float collisionStayReward=-0.05f, float timeReward = -2f, float goalReward =1f)
        {
            _collisionEnterReward = collisionEnterReward;
            _collisionStayReward = collisionStayReward;
            _timeReward = timeReward;
            _goalReward = goalReward;
        }

    }

}

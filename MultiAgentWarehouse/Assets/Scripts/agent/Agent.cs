using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using multiagent.goal;

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
        private Renderer _renderer;

        public Vector3 boxSize, spawningOffset;
        [HideInInspector] public int CurrentEpisode = 0;
        [HideInInspector] public float CumulativeReward = 0f;
        private Color _robotColor;
        public float[] minLim, maxLim;
        private bool _goalReached = false;
        public goalClass _goalClass = new goalClass();
        private int _id = -1;
        
        private bool _wait = false;
        private int _waitCounter = 0;
        private float _collisionEnterReward = -0.05f;
        private float _collisionStayReward = -0.01f;


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

        public virtual void setGoal(goalClass _goalClass = null)
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

        public virtual goalClass getGoal()
        {
            return _goalClass;
        }

        public virtual void GoalReached()
        {
            if (_goalReached == false)
            {
                _goalReached = true;
                AddReward(1.0f);
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
        }

        public Vector3 getGoalPos()
        {
            return _goal;
        }

        public virtual void startWaitCounter()
        {
            _wait = true;
            _waitCounter = 0;
        }

        public virtual void incrementWaitCounter(int counter = 1)
        {
            _waitCounter += counter;
            if (_waitCounter*Time.fixedDeltaTime >= _goalClass.goalWait)
            {
                _wait = false;
                _waitCounter = 0;
            }
        }

        public virtual bool checkWait()
        {
            return _wait;
        }

        public virtual int getWaitCounter()
        {
            return _waitCounter;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Goal") && _goalClass.goalObj == other.GetComponent<Goal>())
            {
                GoalReached();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Player"))
            {
                AddReward(_collisionEnterReward);
                if (_renderer != null)
                {
                    _renderer.material.color = Color.red;
                }
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
                if (_renderer != null)
                {
                    _renderer.material.color = _robotColor;
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
            _renderer = GetComponent<Renderer>();
            _goalClass = new goalClass();
        }

        public void setID(int id)
        {
            _id = id;
        }

        public int getID()
        {
            return _id;
        }
    }

}

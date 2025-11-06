using UnityEngine;
using System;
using multiagent.agent;
using multiagent.parameterJson;
using UnityEngine.Assertions;
using System.Collections.Generic;
using multiagent.robot;
using UnityEngine.UI;
using UnityEngine.AI;
namespace multiagent.dynamic_obstacle
{

    public class human_operator : MonoBehaviour
    {
        private int _goal_idx= 0;
        [SerializeField] private Vector3 _goal;
        [HideInInspector] public int CurrentEpisode = 0;
        [SerializeField] float currentSpeed = 0;
        [SerializeField] float currentRotationSpeed = 0;
        [SerializeField] public float maxSpeed = 1f; // meters per second
        [SerializeField] public float maxRotationSpeed = 3.3f; // radians per second
        [SerializeField] public float maxAcceleration = 10f; // meters per second^2
        [SerializeField] public float maxRotationAccleration = 33f; // radians per second^2
        [SerializeField] public bool infiniteAcceleration = false;
        [SerializeField] Vector3 _state = default;
        [SerializeField] Vector3 _dstate = default;
        [SerializeField] Vector3 _ddstate = default;
        [SerializeField] public Vector2 actionInput;
        public bool verbose = false;
        public Vector3 boxSize = new Vector3(1f, 0.5f, 1f);
        [SerializeField] public bool absoluteCoordinate = false;
        public Vector2 u;
        public int StepCount = 0;
        private int _id = -1;

        private Vector3 newSpawnPosition, accelerationVector, rotationAccelerationVector;
        private Quaternion newSpawnOrientation;
        private Rigidbody _rigidbody;
        public agentData doData;
        public int lineRendererMaxPathPositionListCount = -1;
        public float lineRendererMinPathDistance = -1f;
        public float lineRendererWidth = 25f;
        private bool collisionOn = true;
        private bool isColliding = false;
        public bool useCSVExport = true;
        public int CSVRate = 1;
        public int collisionTagID = 0;
        public NavMeshAgent navMeshAgent;
        private List<Vector3> _goal_points = new List<Vector3>();

        public void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            updateState();
            newSpawnPosition = transform.position;
            newSpawnOrientation = transform.rotation;
            setGoal();
            doData = new agentData(getID());
        }

        private void updateState()
        {
            plant(u);
            if (absoluteCoordinate)
            {
                _state = new Vector3(transform.localPosition.x, transform.localPosition.z, transform.localRotation.eulerAngles.y * MathF.PI / 180);
                _dstate = new Vector3(
                    _rigidbody.linearVelocity.x,
                    _rigidbody.linearVelocity.z,
                    _rigidbody.angularVelocity.y
                );

            }
            else
            {
                _state = Vector3.zero;
                _dstate = new Vector3(
                    transform.InverseTransformDirection(_rigidbody.linearVelocity).x,
                    transform.InverseTransformDirection(_rigidbody.linearVelocity).z,
                    transform.InverseTransformDirection(_rigidbody.angularVelocity).y
                );
                _ddstate = (_dstate - _ddstate) / Time.deltaTime;
            }
            move_to_next_goal();
        }

        public (float[], float) checkConstraint(float v, float w)
        {
            float U = MathF.Pow(MathF.Abs(v), 2) + MathF.Pow(MathF.Abs(w), 2);
            if (U > 1f)
            {
                v /= U;
                w /= U;
            }
            return (new float[] { v, w }, U);
        }

        public void Step()
        {
            StepCount += 1;
            u = Vector2.zero;

            Vector2 abs_goalPos = new Vector2(getGoalPos().x, getGoalPos().z);
            

            // actionInput = new Vector2(action[0], action[1]);
            // if (absoluteCoordinate)
            // {
            //     u = new Vector2(_state[0] + action[0], _state[1] + action[1]);
            // }
            // else
            // {
            //     u = new Vector2( action[0], action[1]);
            // }
        }

        public void plant(Vector2 act)
        {
            // Velocity Control
            (float[] action, float U) = checkConstraint(act[0], -act[1]);
            float speedCoefficent = action[0];
            float desiredSpeed = maxSpeed * speedCoefficent;
            currentSpeed = transform.InverseTransformDirection(_rigidbody.linearVelocity).x;
            float speedDifference = desiredSpeed - currentSpeed;
            if (!infiniteAcceleration)
            {
                speedDifference = Mathf.Clamp(speedDifference, Time.fixedDeltaTime * -maxAcceleration, Time.fixedDeltaTime * maxAcceleration);
            }
            Vector3 velocityDifference = transform.right * speedDifference;
            accelerationVector = velocityDifference / Time.fixedDeltaTime;
            _rigidbody.AddForce(velocityDifference, ForceMode.VelocityChange);

            float rotationCoefficent = action[1];
            float desiredRotation = maxRotationSpeed * rotationCoefficent;
            currentRotationSpeed = transform.InverseTransformDirection(_rigidbody.angularVelocity).y;
            float rotationDifference = desiredRotation - currentRotationSpeed;
            if (!infiniteAcceleration)
            {
                rotationDifference = Mathf.Clamp(rotationDifference, Time.fixedDeltaTime * -maxRotationAccleration, Time.fixedDeltaTime * maxRotationAccleration);
            }
            Vector3 angularDifference = transform.InverseTransformDirection(transform.up) * rotationDifference;
            rotationAccelerationVector = angularDifference / Time.fixedDeltaTime;
            _rigidbody.AddTorque(_rigidbody.inertiaTensor.y * rotationAccelerationVector, ForceMode.Force);

            Vector3 localVelocity = transform.InverseTransformDirection(_rigidbody.linearVelocity);
            localVelocity.z = 0;
            _rigidbody.linearVelocity = transform.TransformDirection(localVelocity);
        }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void FixedUpdate()
        {
            updateState();
            addInfo();
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

        public void setGoal(List<Vector3> goal_points = default)
        {
            _goal_points = new List<Vector3>
            {
                transform.position
            };

            if (goal_points != default)
            {
                _goal_points.AddRange(goal_points);
            }
            _goal_idx = 0;
            _goal = _goal_points[0];
            move_to_next_goal();
        }
        
        public void move_to_next_goal()
        {
            if (_goal_points.Count <= 1)
            {
                return;
            }
            if (Vector3.Distance(transform.position, _goal) > 0.25f)
            {
                Debug.Log("Not close enough to goal yet");
                return;
            }
            Debug.Log("Reached goal, moving to next goal");

            _goal_idx += 1;
            _goal_idx %= _goal_points.Count;

            _goal = _goal_points[_goal_idx];
            Debug.Log("New goal idx: " + _goal_idx + " goal position: " + _goal);
            navMeshAgent.SetDestination(_goal);
        }   
        

        public Vector3 getGoalPos()
        {
            return _goal;
        }

        public virtual void reset()
        {
            CurrentEpisode++;
            StepCount = 0;
            collisionTagID = 0;
            Debug.Log("Resetting Dynamic Obstacle " + getID() + " to position " + newSpawnPosition);
            transform.position = newSpawnPosition;
            transform.rotation = newSpawnOrientation;
            navMeshAgent.Warp(newSpawnPosition);
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            accelerationVector = Vector3.zero;
            rotationAccelerationVector = Vector3.zero;
            updateState();
            _ddstate = Vector3.zero;
            doData.setID(getID());
            doData.clear();
            addInfo();
        }

        public void addInfo()
        {
            if (!useCSVExport || StepCount % CSVRate != 0 || (StepCount < doData.getSize()))
            {
                return;
            }
            subAgentData sData = new subAgentData();

            Vector3 abs_state = new Vector3(transform.localPosition.x, transform.localPosition.z, transform.localRotation.eulerAngles.y * MathF.PI / 180);
            Vector3 abs_dstate = new Vector3(
                    _rigidbody.linearVelocity.x,
                    _rigidbody.linearVelocity.z,
                    _rigidbody.angularVelocity.y
                );
            sData.add(Mathf.Round(StepCount * Time.deltaTime * 1000) / 1000); // Current Time
            sData.add(abs_state); // State (x,y,theta)
            sData.add(abs_dstate); // Derivative of State (x',y',theta')
            sData.add(actionInput); // Action Input (v,w)
            Vector2 abs_goalPos = new Vector2(getGoalPos().x,getGoalPos().z);
            sData.add(abs_goalPos); // Goal Position (xg, yg)
            sData.add(collisionTagID); // Collision Tag ID
            if (doData.header.Count == 0)
            {
                List<String> header = new List<String> {
                "time",
                "x", "y", "theta",
                 "vx", "vy", "w",
                 "Uv", "Uw",
                 "xg", "yg",
                  "goalID", "goalType", "collisionTagID", "reward", "safetyViolated", "U_constraint"
                };
                doData.setHeader(header);
            }
            doData.addEntry(sData);
        }

        public void setCollisionOn(bool collisionOn)
        {
            this.collisionOn = collisionOn;
        }

        private void OnCollisionEnter(Collision collision)
        {
            isColliding = true;
            if (collision.gameObject.CompareTag("Player"))
            {
                if(collisionOn==false){
                    Collider thisCollider = this.GetComponent<Collider>();
                    Collider otherCollider = collision.gameObject.GetComponent<Collider>();
                    if(thisCollider!=null && otherCollider!=null){
                        Physics.IgnoreCollision(thisCollider, otherCollider);
                    }
                    return;
                }
                switch (collision.gameObject.tag)
                {
                    case "Player":
                        collisionTagID = 2;
                        break;
                    default:
                        collisionTagID = 0;
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
            if (collision.gameObject.CompareTag("Player"))
            {
                if(collisionOn==false){
                    Collider thisCollider = this.GetComponent<Collider>();
                    Collider otherCollider = collision.gameObject.GetComponent<Collider>();
                    if(thisCollider!=null && otherCollider!=null){
                        Physics.IgnoreCollision(thisCollider, otherCollider);
                    }
                    return;
                }
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
            if (collision.gameObject.CompareTag("Player"))
            {
                if (collisionOn == false)
                {
                    Collider thisCollider = this.GetComponent<Collider>();
                    Collider otherCollider = collision.gameObject.GetComponent<Collider>();
                    if (thisCollider != null && otherCollider != null)
                    {
                        Physics.IgnoreCollision(thisCollider, otherCollider);
                    }
                    return;
                }

                collisionTagID = 0;
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
        
        public void updateSpawnState(Vector3 position, Quaternion orientation)
        {
            newSpawnPosition = position;
            newSpawnOrientation = orientation;

        }

        public void updateHumanOperatorParameters(parameters param)
        {
            useCSVExport = param.unityParams.useCSVExporter;
            CSVRate = param.unityParams.CSVRate;
            _rigidbody.mass = param.agentParams.dynamicsParams.bodyMass;
            _rigidbody.linearDamping = param.agentParams.dynamicsParams.linearDrag;
            _rigidbody.angularDamping = param.agentParams.dynamicsParams.angularDrag;
            maxSpeed = param.agentParams.maxSpeed;
            maxRotationSpeed = param.agentParams.maxRotationSpeed;
            maxAcceleration = param.agentParams.maxAcceleration;
            maxRotationAccleration = param.agentParams.maxRotationAccleration;
            infiniteAcceleration = param.agentParams.infiniteAcceleration;
            absoluteCoordinate = param.agentParams.absoluteCoordinate;
            verbose = param.agentParams.verbose;
            lineRendererMaxPathPositionListCount = param.agentParams.lineRendererMaxPathPositionListCount;
            lineRendererMinPathDistance = param.agentParams.lineRendererMinPathDistance;
            lineRendererWidth = param.agentParams.lineRendererWidth;
            setCollisionOn(param.agentParams.allowedCollisionOn);
            Assert.IsTrue(maxSpeed > 0, "maxSpeed must be positive");
            Assert.IsTrue(maxRotationSpeed > 0, "maxRotationSpeed must be positive");
            Assert.IsTrue(maxAcceleration > 0, "maxAcceleration must be positive");
            Assert.IsTrue(maxRotationAccleration > 0, "maxRotationAccleration must be positive");

            navMeshAgent.speed = maxSpeed;
            navMeshAgent.angularSpeed = maxRotationSpeed * 180 / Mathf.PI; // Convert to degrees
            navMeshAgent.acceleration = maxAcceleration;
        }

    }
}
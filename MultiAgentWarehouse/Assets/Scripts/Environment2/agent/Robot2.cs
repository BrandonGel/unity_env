using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using System;
using multiagent.controller;
using multiagent.agent;
using multiagent.parameterJson;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace multiagent.robot
{

    public class Robot2 : Agent2Template
    {
        [SerializeField] public float maxSpeed = .7f; // meters per second
        [SerializeField] public float maxRotationSpeed = 2.11f; // degrees per second
        [SerializeField] public float maxAcceleration = 2.72f; // degrees per second
        [SerializeField] public float maxRotationAccleration = 8.23f; // degrees per second
        private float _bodyRadius = 0.331f; // meters
        private float _safetyRadius = 0.273f; // meters
        private int _safetyViolated = 0; // meters
        private Controller control = new Controller();
        public ctrlOption _controllerName;
        private string _controllerNameStr;
        public ctrlType _controllerType;
        private string _controllerTypeStr;
        private string controlPath = "control.txt";
        private Rigidbody _rigidbody;
        private bool isControllerInit = false;
        [SerializeField] public Vector2 actionInput;
        [SerializeField] public bool velocityControl = true;

        [SerializeField] float currentSpeed = 0;
        [SerializeField] float currentRotationSpeed = 0;
        [SerializeField] float currentAcceleration = 0;
        [SerializeField] float currentRotationAcceleration = 0;

        [SerializeField] Vector3 _state = default;
        [SerializeField] Vector3 _dstate = default;
        [SerializeField] Vector3 _ddstate = default;
        [SerializeField] public bool absoluteCoordinate = false;

        public agentData aData;
        public Vector2 u;
        [SerializeField] float U_constraint = 0;
        public GameObject arrow;
        public bool debugArrow = false;
        private bool usingArrow = false;
        GameObject arrowObj, arrowObj2, arrowObj3, arrowObj4, arrowObj5;
        private Vector3 newSpawnPosition, accelerationVector, rotationAccelerationVector;
        private Quaternion newSpawnOrientation;
        RayPerceptionSensorComponent3D m_rayPerceptionSensorComponent3D;
        private bool allowCommandsInput = true;
        public int obs_size = 13;


        public void Awake()
        {
            // _controllerNameStr = Enum.GetName(_controllerName.GetType(), _controllerName);
            // _controllerTypeStr = Enum.GetName(_controllerType.GetType(), _controllerType);
            _rigidbody = GetComponent<Rigidbody>();
            initExtra();

            // Initialize the constraints
            if (velocityControl)
            {
                minLim = new float[] {
                -maxSpeed,
                -maxRotationSpeed,
                };
                maxLim = new float[] {
                    maxSpeed,
                    maxRotationSpeed,
                };
            }
            else
            {
                minLim = new float[] {
                -maxAcceleration,
                -maxRotationAccleration
                };
                maxLim = new float[] {
                    maxAcceleration,
                    maxRotationAccleration
                };
            }
            changeMaterialColor();
            updateState();
            generateArrow();
            newSpawnPosition = transform.position;
            newSpawnOrientation = transform.rotation;
            setGoal();
            aData = new agentData(getID());
            m_rayPerceptionSensorComponent3D = transform.Find("Body").Find("Dummy Lidar").GetComponent<RayPerceptionSensorComponent3D>();
        }

        private void generateArrow()
        {

            if (debugArrow == true && usingArrow == false)
            {
                Vector3 robotSize = GetComponent<BoxCollider>().size;
                Vector3 robotScale = GetComponent<Transform>().localScale;
                float yoffset = robotSize.y * robotScale.y / 2;

                usingArrow = true;
                Vector3 arrowPosition = transform.position;
                Quaternion arrowOrientation = transform.rotation;

                // Linear Velocity Arrow
                arrowObj = Instantiate(arrow, arrowPosition, arrowOrientation);
                arrowObj.GetComponent<ArrowGenerator>().setParam("r", -maxSpeed, maxSpeed, Color.red, yoffset);
                arrowObj.transform.parent = gameObject.transform;

                // Angular Velocity Arrow
                arrowObj2 = Instantiate(arrow, arrowPosition, arrowOrientation);
                arrowObj2.GetComponent<ArrowGenerator>().setParam("u", -maxRotationSpeed, maxRotationSpeed, Color.red, yoffset, true);
                arrowObj2.transform.parent = gameObject.transform;

                // Linear Acceleration Arrow
                arrowObj3 = Instantiate(arrow, arrowPosition, arrowOrientation);
                arrowObj3.GetComponent<ArrowGenerator>().setParam("r", -maxAcceleration, maxAcceleration, Color.blue, yoffset);
                arrowObj3.transform.parent = gameObject.transform;

                // Angular Velocity Arrow
                arrowObj4 = Instantiate(arrow, arrowPosition, arrowOrientation);
                arrowObj4.GetComponent<ArrowGenerator>().setParam("u", -maxRotationAccleration, maxRotationAccleration, Color.blue, yoffset, true);
                arrowObj4.transform.parent = gameObject.transform;

                // Goal Arrow
                arrowObj5 = Instantiate(arrow, arrowPosition, arrowOrientation);
                arrowObj5.GetComponent<ArrowGenerator>().setParam("", -1, 1, Color.green, yoffset);
                arrowObj5.transform.parent = gameObject.transform;
            }
            else if (debugArrow == true && usingArrow == true)
            {
                arrowObj.GetComponent<ArrowGenerator>().scaleArrow(currentSpeed);
                arrowObj2.GetComponent<ArrowGenerator>().scaleArrow(currentRotationSpeed);
                arrowObj3.GetComponent<ArrowGenerator>().scaleArrow(currentAcceleration);
                arrowObj4.GetComponent<ArrowGenerator>().scaleArrow(currentRotationAcceleration);

                Vector3 goalPos = getGoalPos();
                Vector3 goalVector = new Vector3(goalPos.x, 0, goalPos.z) - new Vector3(transform.position.x, 0, transform.position.z);
                arrowObj5.GetComponent<ArrowGenerator>().scaleArrow(goalVector.magnitude, goalVector);
            }
            else if (debugArrow == false && usingArrow == true)
            {
                Destroy(arrowObj);
                Destroy(arrowObj2);
                Destroy(arrowObj3);
                Destroy(arrowObj4);
                Destroy(arrowObj5);
                usingArrow = false;
            }
        }

        public virtual void reset()
        {
            CurrentEpisode++;
            StepCount = 0;
            CumulativeReward = 0;
            Reward = 0;
            transform.position = newSpawnPosition;
            transform.rotation = newSpawnOrientation;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            accelerationVector = Vector3.zero;
            rotationAccelerationVector = Vector3.zero;
            updateState();
            changeMaterialColor();
            _ddstate = Vector3.zero;
            aData.clear();
            addInfo();
        }

        public float[] getLidarData(bool useNormalized=true)
        {
            if (m_rayPerceptionSensorComponent3D == null)
            {
                Debug.LogError("Lidar Sensor not found!");
                return new float[0];
            }


            var rayOutputs = RayPerceptionSensor.Perceive(m_rayPerceptionSensorComponent3D.GetRayPerceptionInput()).RayOutputs;
            int lengthOfRayOutputs = rayOutputs.Length;            
            float[] rayObs = new float[3*lengthOfRayOutputs];
            for (int i = 0; i < lengthOfRayOutputs; i++)
            {
                GameObject goHit = rayOutputs[i].HitGameObject;
                var rayDirection = rayOutputs[i].EndPositionWorld - rayOutputs[i].StartPositionWorld;
                rayDirection = transform.InverseTransformDirection(rayDirection);
                var scaledRayLength = rayDirection.magnitude;
                float rayHitDistance = rayOutputs[i].HitFraction * scaledRayLength;
                float rayAngle = Mathf.Atan2(rayDirection.z, rayDirection.x) ;
                float normalizedRayAngle = rayAngle/Mathf.PI; // Normalize angle to be within [-1, 1]
                // float cosRayAngle = Mathf.Cos(rayAngle);
                // float sinRayAngle = Mathf.Sin(rayAngle);

                // Check for safety violation
                _safetyViolated = 0;
                if( rayHitDistance <  _safetyRadius)
                {
                    _safetyViolated = 1;
                }

                if( useNormalized)
                {
                    rayObs[3*i] = normalizedRayAngle; // Angle of the ray w.r.t the robot forward direction
                    rayObs[3*i+1] = rayOutputs[i].HitFraction; // Normalize the distance
                }
                else
                {
                    rayObs[3*i] = rayAngle*Mathf.Rad2Deg; // Angle of the ray w.r.t the robot forward direction
                    rayObs[3*i+1] = rayHitDistance; // Normalize the distance
                }
                if(goHit != null)
                {
                    switch(goHit.tag)
                    {
                        case "Wall":
                            rayObs[3*i+2] = 1f;
                            break;
                        case "Robot":
                            rayObs[3*i+2] = 2f;
                            break;
                        case "Obstacle":
                            rayObs[3*i+2] = 3f;
                            break;
                        default:
                            rayObs[3*i+2] = 0.0f;
                            break;
                    }
                }
                else
                {
                    rayObs[3*i+2] = 0.0f;
                }
                    
            }
            return rayObs;

        }


        public float[] CollectObservations(bool useNormalized=true)
        {
            float robotPosX_normalized = transform.localPosition.x;
            float robotPosZ_normalized = transform.localPosition.z;
            float angleRotation_normalized = (360f-transform.localRotation.eulerAngles.y + 180f)% 360f  - 180f; // Minus for right hand rule
            float currentSpeed_normalized = currentSpeed;
            float currentRotationSpeed_normalized = currentRotationSpeed;
            float currentAcceleration_normalized = currentAcceleration;
            float currentRotationAcceleration_normalized = currentRotationAcceleration;
            
            // float sinAngle = Mathf.Sin(angleRotation_normalized*Mathf.Deg2Rad); // Sine of the angle ->[-180, 180) -> [-1, 1]
            // float cosAngle = Mathf.Cos(angleRotation_normalized*Mathf.Deg2Rad); // Cosine of the angle ->[-180, 180) -> [-1, 1]
            if( useNormalized)
            {
                robotPosX_normalized= transform.localPosition.x / boxSize.x;
                robotPosZ_normalized= transform.localPosition.z / boxSize.z;    
                angleRotation_normalized = angleRotation_normalized/180f; // Normalize angle [-180, 180) -> [-1, 1)
                currentSpeed_normalized = currentSpeed/ maxSpeed;
                currentRotationSpeed_normalized = currentRotationSpeed / maxRotationSpeed;
                currentAcceleration_normalized = currentAcceleration / maxAcceleration;
                currentRotationAcceleration_normalized = currentRotationAcceleration / maxRotationAccleration;
            }
            
        
            Vector3 goalPos = Vector3.zero;
            float goalPosX_normalized = 0; 
            float goalPosZ_normalized = 0;
            int taskID = 0;
            int task_ind = -1;
            int completed = 0;
            if (taskClass != null)
            {
                goalPos = getGoalPos();
                taskID = taskClass.taskID;
                goalPosX_normalized = goalPos.x;
                goalPosZ_normalized = goalPos.z;
                task_ind = taskClass.task_ind;
                completed = taskClass.isCompleted() ? 1 : 0;

                if( useNormalized)
                {
                    goalPosX_normalized = goalPos.x / boxSize.x;
                    goalPosZ_normalized = goalPos.z / boxSize.z;
                }
            }

            
            float[] observation = new float[] {
                robotPosX_normalized,
                robotPosZ_normalized,
                angleRotation_normalized,
                currentSpeed_normalized,
                currentRotationSpeed_normalized,
                currentAcceleration_normalized,
                currentRotationAcceleration_normalized,
                taskID,
                goalPosX_normalized,
                goalPosZ_normalized,
                task_ind,
                completed,
                Reward
            };
            float[] lidarData = getLidarData(useNormalized);

            float[] robotObs = new float[observation.Length + lidarData.Length];
            observation.CopyTo(robotObs, 0);
            lidarData.CopyTo(robotObs, observation.Length);
            return robotObs;
        }

        public float[] Heuristic()
        {
            float[] continuousActionsOut = new float[2];
            continuousActionsOut[0] = 0;
            continuousActionsOut[1] = 0;
            if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftArrow))
            {
                continuousActionsOut[0] = 1;
                continuousActionsOut[1] = -1;
            }
            else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.RightArrow))
            {
                continuousActionsOut[0] = 1;
                continuousActionsOut[1] = 1;
            }
            else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.LeftArrow))
            {
                continuousActionsOut[0] = -1;
                continuousActionsOut[1] = -1;
            }
            else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.RightArrow))
            {
                continuousActionsOut[0] = -1;
                continuousActionsOut[1] = 1;
            }
            else if (Input.GetKey(KeyCode.UpArrow))
            {
                continuousActionsOut[0] = 1;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                continuousActionsOut[1] = -1;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                continuousActionsOut[1] = 1;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                continuousActionsOut[0] = -1;
            }
            return continuousActionsOut;
        }

        public void Step(float[] actions)
        {
            Reward = 0;
            StepCount += 1;
            if (!isControllerInit)
            {
                control.InitControl(actions.Length, minLim, maxLim, _controllerNameStr, controlPath);
                isControllerInit = true;
            }

            if (!checkWait() && allowCommandsInput)
            {
                MoveAgent(actions);
            }
            else
            {
                u = Vector2.zero;
            }

            addTimeReward();
            CumulativeReward += Reward;
        }

        public (float[], float) checkConstraint(float v, float w)
        {
            float U = MathF.Abs(v) + MathF.Abs(w);
            if (U > 1f)
            {
                v /= U;
                w /= U;
            }
            return (new float[] { v, w }, U);
        }

        public void plant(Vector2 act)
        {
            (float[] action, float U) = checkConstraint(act[0], act[1]);
            if (velocityControl) // Velocity Plant Model
            {
                float speedCoefficent = action[0];
                float desiredSpeed = maxSpeed * speedCoefficent;
                currentSpeed = transform.InverseTransformDirection(_rigidbody.linearVelocity).x;
                float speedDifference = desiredSpeed - currentSpeed;
                Vector3 velocityDifference = transform.right * speedDifference;
                accelerationVector = velocityDifference / Time.fixedDeltaTime;
                _rigidbody.AddForce(velocityDifference, ForceMode.VelocityChange);

                float rotationCoefficent = action[1];
                float desiredRotation = maxRotationSpeed * rotationCoefficent;
                currentRotationSpeed = transform.InverseTransformDirection(_rigidbody.angularVelocity).y;
                float rotationDifference = desiredRotation - currentRotationSpeed;
                Vector3 angularDifference = transform.InverseTransformDirection(transform.up) * rotationDifference;
                rotationAccelerationVector = angularDifference / Time.fixedDeltaTime;
                _rigidbody.AddTorque(_rigidbody.inertiaTensor.y * rotationAccelerationVector, ForceMode.Force);
                
            }
            else // Acceleration Plant Model w/ Constraint
            {
                float acceleartionCoefficent = action[0];
                currentAcceleration = maxAcceleration * acceleartionCoefficent;
                currentSpeed = transform.InverseTransformDirection(_rigidbody.linearVelocity).x;
                float projectedSpeed = currentSpeed + currentAcceleration * Time.fixedDeltaTime;
                if (MathF.Abs(projectedSpeed) > maxSpeed)
                {
                    projectedSpeed = MathF.Sign(projectedSpeed) * maxSpeed;
                    currentAcceleration = (projectedSpeed - currentSpeed) / Time.fixedDeltaTime;
                }

                float rotationAcceleartionCoefficent = action[1];
                currentRotationAcceleration = maxRotationAccleration * rotationAcceleartionCoefficent;
                currentRotationSpeed = transform.InverseTransformDirection(_rigidbody.angularVelocity).y;
                float projectedRotationSpeed = currentRotationSpeed + currentRotationAcceleration * Time.fixedDeltaTime;
                if (Math.Abs(projectedRotationSpeed) > maxRotationSpeed)
                {
                    projectedRotationSpeed = MathF.Sign(projectedRotationSpeed) * maxRotationSpeed;
                    currentRotationAcceleration = (projectedRotationSpeed - currentSpeed) / Time.fixedDeltaTime;
                }
                (float[] projectedAction, float U2) = checkConstraint(projectedSpeed / maxSpeed, projectedRotationSpeed / maxRotationSpeed);
                if (U2 > 1f)
                {
                    projectedSpeed = projectedAction[0] * maxSpeed;
                    projectedRotationSpeed = projectedAction[1] * maxRotationSpeed;
                    currentAcceleration = (projectedSpeed - currentSpeed) / Time.fixedDeltaTime;
                    currentRotationAcceleration = (projectedRotationSpeed - currentRotationSpeed) / Time.fixedDeltaTime;
                }
                U = U2;

                accelerationVector = transform.right * currentAcceleration;
                _rigidbody.AddForce(accelerationVector, ForceMode.Acceleration);

                rotationAccelerationVector = transform.up * currentRotationAcceleration;
                _rigidbody.AddTorque(_rigidbody.inertiaTensor.y * rotationAccelerationVector, ForceMode.Force);
            }
            U_constraint = U;

            Vector3 localVelocity = transform.InverseTransformDirection(_rigidbody.linearVelocity);
            localVelocity.z = 0;
            _rigidbody.linearVelocity = transform.TransformDirection(localVelocity);
        }

        public void MoveAgent(float[] action)
        {
            actionInput = new Vector2(action[0], action[1]);
            // Vector3[] act;
            // if (absoluteCoordinate)
            // {
            //     act = new Vector3[] {
            //         new Vector3(_state[0], _state[1], 0),
            //         new Vector3(_state[0] + action[0], _state[1] + action[1], 0)
            //     };
            // }
            // else
            // {
            //     act = new Vector3[] {
            //         new Vector3(_state[0], _state[1], 0),
            //         new Vector3(action[0], action[1], 0)
            //     };
            // }

            // Vector3[] S = new Vector3[] { _state }; // State information
            // Vector3[] dS = new Vector3[] { _dstate }; // Time derivative of State information
            // Vector3[] ddesS = null; //TODO:  Desired State information
            // if (!velocityControl)
            // {
            //     ddesS = new Vector3[] { new Vector3(0.0f, 0f, 0.0f) };
            // }
            // Debug.Log("Act: " + act[0] + " " + act[1] );

            // Vector2 u;
            if (absoluteCoordinate)
            {
                u = new Vector2(_state[0] + action[0], _state[1] + action[1]);
            }
            else
            {
                u = new Vector2( action[0], action[1]);
            }

            // u = control.GetControl(act, S, ddesS, dS);
            // Debug.Log("u: " + u[0] + " " + u[1] );
            
            // Debug.Log($"Goal: {act[0]}, {act[1]} | Control {u}");
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
        }
        private void Update()
        {
            generateArrow();
        }

        private void FixedUpdate()
        {
            updateState();
            addInfo();
        }

        public void addInfo()
        {
            subAgentData sData = new subAgentData();
            
            Vector3 abs_state = new Vector3(transform.localPosition.x, transform.localPosition.z, transform.localRotation.eulerAngles.y * MathF.PI / 180);
            Vector3 abs_dstate = new Vector3(
                    _rigidbody.linearVelocity.x,
                    _rigidbody.linearVelocity.z,
                    _rigidbody.angularVelocity.y
                );
            sData.add(StepCount * Time.deltaTime); // Current Time
            sData.add(abs_state); // State (x,y,theta)
            sData.add(abs_dstate); // Derivative of State (x',y',theta')
            sData.add(actionInput); // Action Input (v,w)
            Vector2 abs_goalPos = new Vector2(getGoalPos().x,getGoalPos().z);
            sData.add(abs_goalPos); // Goal Position (xg, yg)
            if (taskClass != null)
            {
                sData.add(taskClass.getCurrentGoalID()); // Current Goal ID
                sData.add(taskClass.getCurrentGoalType()); // Current Goal Type
            }
            else
            {
                sData.add(0);
                sData.add(-1);
            }
            sData.add(collisionTagID); // Collision Tag ID
            sData.add(Reward); // Reward
            sData.add(_safetyViolated); // Safety Violated
            sData.add(U_constraint); // Control Constraint
            float[] lidarData = getLidarData(false);
            sData.add(lidarData); // Lidar Data (rayAngle, rayDistance, rayTag) * num_rays

            
            if (aData.header.Count == 0)
            {
                List<String> header = new List<String> {
                "time", 
                "x", "y", "theta",
                 "vx", "vy", "w", 
                 "Uv", "Uw", 
                 "xg", "yg",
                  "goalID", "goalType", "collisionTagID", "reward", "safetyViolated", "U_constraint"
                }; 
                for (int i = 0; i < lidarData.Length/3; i++)
                {
                    header.Add($"rayAngle_{i}");
                    header.Add($"rayDistance_{i}");
                    header.Add($"rayTag_{i}");
                }
                aData.setHeader(header);
            }
            aData.addEntry(sData);
        }

        public override bool checkTerminalCondition()
        {

            if (StepCount == MaxStep - 1)
            {
                return true;
            }
            return false;
        }

        public void updateSpawnState(Vector3 position, Quaternion orientation)
        {
            newSpawnPosition = position;
            newSpawnOrientation = orientation;

        }

        public (float, Vector3, Vector3) getState()
        {
            float currentTime = StepCount * Time.fixedDeltaTime;
            Vector3 s = new Vector3(transform.localPosition.x, transform.localPosition.z, transform.localRotation.eulerAngles.y * MathF.PI / 180);
            Vector3 ds = new Vector3(
                _rigidbody.linearVelocity.x,
                _rigidbody.linearVelocity.z,
                _rigidbody.angularVelocity.y
            );
            return (currentTime, s, ds);
        }

        public void setAllowCommandsInput(bool allow)
        {
            allowCommandsInput = allow;
        }

        public int calculateObservationSize(int obsSize, int num_rays=0)
        {
            obsSize += 3 * (num_rays * 2 + 1);
            return obsSize;
        }

        public void updateAgentParameters(parameters param)
        {
            maxSpeed = param.agentParams.maxSpeed;
            maxRotationSpeed = param.agentParams.maxRotationSpeed;
            maxAcceleration = param.agentParams.maxAcceleration;
            maxRotationAccleration = param.agentParams.maxRotationAccleration;
            velocityControl = param.agentParams.velocityControl;
            absoluteCoordinate = param.agentParams.absoluteCoordinate;
            debugArrow = param.agentParams.debugArrow;
            _safetyRadius = param.agentParams.safetyRadius;
            Assert.IsTrue(maxSpeed > 0, "maxSpeed must be positive");
            Assert.IsTrue(maxRotationSpeed > 0, "maxRotationSpeed must be positive");
            Assert.IsTrue(maxAcceleration > 0, "maxAcceleration must be positive");
            Assert.IsTrue(maxRotationAccleration > 0, "maxRotationAccleration must be positive");
            Assert.IsTrue(_safetyRadius >= 0, "safetyRadius must be non-negative");
            Assert.IsTrue(_safetyRadius > _bodyRadius, "safetyRadius must be greater than bodyRadius");
            setDecisionRequestParams(param.agentParams.maxTimeSteps, param.agentParams.decisionPeriod);
            modifyReward(
                param.agentParams.rewardParams.collisionEnterReward,
                param.agentParams.rewardParams.collisionStayReward,
                param.agentParams.rewardParams.timeReward,
                param.agentParams.rewardParams.goalReward);
            if (m_rayPerceptionSensorComponent3D != null)
            {
                m_rayPerceptionSensorComponent3D.RayLength = param.agentParams.rayParams.rayLength;
                m_rayPerceptionSensorComponent3D.RaysPerDirection = param.agentParams.rayParams.rayDirections;
                m_rayPerceptionSensorComponent3D.SphereCastRadius = param.agentParams.rayParams.sphereCastRadius;
                m_rayPerceptionSensorComponent3D.MaxRayDegrees = param.agentParams.rayParams.maxRayDegrees;
                obs_size = calculateObservationSize(obs_size, param.agentParams.rayParams.rayDirections);
            }
        }

        void Start()
        {
            m_rayPerceptionSensorComponent3D.name = "RayPerceptionSensor_" + getID();
        }
    }

}

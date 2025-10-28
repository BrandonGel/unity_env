using UnityEngine;
using Unity.MLAgents.Sensors;
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
        [SerializeField] public float maxRotationSpeed = 2.11f; // radians per second
        [SerializeField] public float maxAcceleration = 2.72f; // meters per second^2
        [SerializeField] public float maxRotationAccleration = 8.23f; // radians per second^2
        private float _bodyRadius = 0.331f; // meters
        private float _safetyRadius = 0.273f; // meters
        private int _safetyViolated = 0; // meters
        private Rigidbody _rigidbody;
        [SerializeField] public Vector2 actionInput;
        public string controllerType = "velocity";
        [SerializeField] public bool infiniteAcceleration = true;

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
        public bool debugOnlyDirection = false;
        public bool debugOnlySpeed = false;
        public bool debugOnlyAcceleration = false;
        public bool debugOnlyGoal = false;
        public bool debugArrow2DMode = false;
        public float[] debugArrowParams;
        private bool usingArrow = false;
        GameObject arrowObj0,arrowObj1, arrowObj2, arrowObj3, arrowObj4, arrowObj5;
        private Vector3 newSpawnPosition, accelerationVector, rotationAccelerationVector;
        private Quaternion newSpawnOrientation;
        RayPerceptionSensorComponent3D m_rayPerceptionSensorComponent3D;
        private bool allowCommandsInput = true;
        private int _obs_size = 15;
        public bool useCSVExport = true;
        public int CSVRate = 1;
        public bool useRadian = false;
        public int lineRendererMaxPathPositionListCount = -1;
        public float lineRendererMinPathDistance = -1f;
        public float lineRendererWidth = 25f;
        public bool allowedlightingOn = true;

        float[] lidarData,lidarDataUnnormalized,safetyData;

        public void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            initExtra();
            changeMaterialColor();
            updateState();
            generateArrow();
            newSpawnPosition = transform.position;
            newSpawnOrientation = transform.rotation;
            setGoal();
            m_rayPerceptionSensorComponent3D = transform.Find("Body").Find("Dummy Lidar").GetComponent<RayPerceptionSensorComponent3D>();
            aData = new agentData(getID());
        }


        private void generateArrow()
        {
            bool anyDebugArrow = debugArrow || debugOnlyDirection || debugOnlySpeed || debugOnlyAcceleration || debugOnlyGoal;
            if (anyDebugArrow == true && usingArrow == false)
            {
                Vector3 robotSize = GetComponent<BoxCollider>().size;
                Vector3 robotScale = GetComponent<Transform>().localScale;
                // float yoffset = robotSize.y * robotScale.y / 2;

                usingArrow = true;
                Vector3 arrowPosition = transform.position;
                Quaternion arrowOrientation = transform.rotation;

                //Arrow Orientation
                if (debugOnlyDirection || debugArrow)
                {
                    arrowObj0 = Instantiate(arrow, arrowPosition, arrowOrientation);
                    arrowObj0.GetComponent<ArrowGenerator>().setParam("r", -1, 1, Color.black, debugArrowParams, debugArrow2DMode);
                    arrowObj0.transform.parent = gameObject.transform;
                    arrowObj0.GetComponent<ArrowGenerator>().GenerateArrow();
                }
                if (debugOnlySpeed || debugArrow)
                {
                    // Linear Velocity Arrow
                    arrowObj1 = Instantiate(arrow, arrowPosition, arrowOrientation);
                    arrowObj1.GetComponent<ArrowGenerator>().setParam("r", -maxSpeed, maxSpeed, Color.red, debugArrowParams, debugArrow2DMode);
                    arrowObj1.transform.parent = gameObject.transform;
                    arrowObj1.GetComponent<ArrowGenerator>().GenerateArrow();

                    // Angular Velocity Arrow
                    arrowObj2 = Instantiate(arrow, arrowPosition, arrowOrientation);
                    arrowObj2.GetComponent<ArrowGenerator>().setParam("u", -maxRotationSpeed, maxRotationSpeed, Color.red, debugArrowParams, debugArrow2DMode);
                    arrowObj2.transform.parent = gameObject.transform;
                    arrowObj2.GetComponent<ArrowGenerator>().GenerateArrow();
                }
                if (debugOnlyAcceleration || debugArrow)
                {
                    // Linear Acceleration Arrow
                    arrowObj3 = Instantiate(arrow, arrowPosition, arrowOrientation);
                    arrowObj3.GetComponent<ArrowGenerator>().setParam("r", -maxAcceleration, maxAcceleration, Color.blue, debugArrowParams, debugArrow2DMode);
                    arrowObj3.transform.parent = gameObject.transform;
                    arrowObj3.GetComponent<ArrowGenerator>().GenerateArrow();

                    // Angular Velocity Arrow
                    arrowObj4 = Instantiate(arrow, arrowPosition, arrowOrientation);
                    arrowObj4.GetComponent<ArrowGenerator>().setParam("u", -maxRotationAccleration, maxRotationAccleration, Color.blue, debugArrowParams, debugArrow2DMode);
                    arrowObj4.transform.parent = gameObject.transform;
                    arrowObj4.GetComponent<ArrowGenerator>().GenerateArrow();
                }
                if (debugOnlyGoal || debugArrow)
                {
                    // Goal Arrow
                    arrowObj5 = Instantiate(arrow, arrowPosition, arrowOrientation);
                    arrowObj5.GetComponent<ArrowGenerator>().setParam("r", -1, 1, Color.green, debugArrowParams, debugArrow2DMode);
                    arrowObj5.transform.parent = gameObject.transform;
                    arrowObj5.GetComponent<ArrowGenerator>().GenerateArrow();
                }

            }
            if (anyDebugArrow == true && usingArrow == true)
            {
                if (debugOnlySpeed || debugArrow)
                {
                    arrowObj1.GetComponent<ArrowGenerator>().scaleArrow(currentSpeed);
                    arrowObj2.GetComponent<ArrowGenerator>().scaleArrow(currentRotationSpeed);
                }
                if (debugOnlyAcceleration || debugArrow)
                {
                    arrowObj3.GetComponent<ArrowGenerator>().scaleArrow(currentAcceleration);
                    arrowObj4.GetComponent<ArrowGenerator>().scaleArrow(currentRotationAcceleration);
                }
                if (debugOnlyGoal || debugArrow)
                {
                    Vector3 goalVector = Vector3.zero;
                    if (taskClass != null)
                    {
                        Vector3 goalPos = getGoalPos();
                        goalVector = new Vector3(goalPos.x, 0, goalPos.z) - new Vector3(transform.position.x, 0, transform.position.z);
                    }
                    arrowObj5.GetComponent<ArrowGenerator>().scaleArrow(goalVector.magnitude, goalVector);
                }
            }
            if (anyDebugArrow == false && usingArrow == true)
            {
                if (debugOnlyDirection || debugArrow)
                {
                    Destroy(arrowObj0);
                }
                if (debugOnlySpeed || debugArrow)
                {
                    Destroy(arrowObj1);
                    Destroy(arrowObj2);
                }
                if (debugOnlyAcceleration || debugArrow)
                {
                    Destroy(arrowObj3);
                    Destroy(arrowObj4);
                }
                if (debugOnlyGoal || debugArrow)
                {
                    Destroy(arrowObj5);
                }
                usingArrow = false;
            }
        }

        public virtual void reset()
        {
            CurrentEpisode++;
            Reset();
            transform.position = newSpawnPosition;
            transform.rotation = newSpawnOrientation;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            accelerationVector = Vector3.zero;
            rotationAccelerationVector = Vector3.zero;
            updateState();
            changeMaterialColor();
            setGoal();
            _ddstate = Vector3.zero;
            aData.setID(getID());
            aData.clear();
            addInfo();
        }

        public (float[],float[]) getLidarData(bool useNormalized=true)
        {
            if (m_rayPerceptionSensorComponent3D == null)
            {
                Debug.LogError("Lidar Sensor not found!");
                return (new float[0], new float[0]);
            }
            Dictionary <float, bool> angleDict = new Dictionary<float, bool>();

            var rayOutputs = RayPerceptionSensor.Perceive(m_rayPerceptionSensorComponent3D.GetRayPerceptionInput()).RayOutputs;
            int lengthOfRayOutputs = rayOutputs.Length;            
            float[] rayObs,safetyObs;
            if (m_rayPerceptionSensorComponent3D.MaxRayDegrees == 180f)
            {
                rayObs = new float[3 * (lengthOfRayOutputs - 1)];
                safetyObs = new float[lengthOfRayOutputs - 1];
            }
            else
            {
                rayObs = new float[3 * lengthOfRayOutputs];
                safetyObs = new float[lengthOfRayOutputs];
            }

            _safetyViolated =  0;
            for (int i = 0; i < lengthOfRayOutputs; i++)
            {
                GameObject goHit = rayOutputs[i].HitGameObject;
                var rayDirection = rayOutputs[i].EndPositionWorld - rayOutputs[i].StartPositionWorld;
                rayDirection = transform.InverseTransformDirection(rayDirection);
                var scaledRayLength = rayDirection.magnitude;
                float rayHitDistance = rayOutputs[i].HitFraction * scaledRayLength;
                float normalizedRayAngle = Mathf.Atan2(rayDirection.z, rayDirection.x);

                float roundAngle = Mathf.Round(normalizedRayAngle * 100) / 100;
                if (angleDict.ContainsKey(roundAngle))
                {
                    continue;
                }
                angleDict[roundAngle] = true;

                // float cosRayAngle = Mathf.Cos(rayAngle);
                // float sinRayAngle = Mathf.Sin(rayAngle);



                // Check for safety violation
                if (rayHitDistance < _safetyRadius)
                {
                    safetyObs[i] = 1f; // Safety Violation
                    _safetyViolated = 1;
                }

                // Store Lidar Data
                if (useNormalized)
                {
                    rayObs[3 * i] = normalizedRayAngle / Mathf.PI; // Angle of the ray w.r.t the robot forward direction
                    rayObs[3 * i + 1] = rayOutputs[i].HitFraction; // Normalize the distance
                }
                else
                {
                    if (useRadian)
                    {
                        rayObs[3 * i] = normalizedRayAngle; // Angle of the ray w.r.t the robot forward direction
                    }
                    else
                    {
                        rayObs[3 * i] = normalizedRayAngle * Mathf.Rad2Deg; // Angle of the ray w.r.t the robot forward direction
                    }
                    rayObs[3 * i + 1] = rayHitDistance; // Normalize the distance
                }
                if (goHit != null)
                {
                    switch (goHit.tag)
                    {
                        case "Wall":
                            rayObs[3 * i + 2] = 1f;
                            break;
                        case "Robot":
                            rayObs[3 * i + 2] = 2f;
                            break;
                        case "Human":
                            rayObs[3 * i + 2] = 3f;
                            break;
                        default:
                            rayObs[3 * i + 2] = 0.0f;
                            break;
                    }
                }
                else
                {
                    rayObs[3 * i + 2] = 0.0f;
                }

            }
            return (rayObs, safetyObs);

        }


        public float[] CollectObservations(bool useNormalized=true)
        {
            float robotPosX_normalized = transform.localPosition.x;
            float robotPosZ_normalized = transform.localPosition.z;
            float angleRotation_normalized = (360f-transform.localRotation.eulerAngles.y + 180f)% 360f  - 180f; // Minus for right hand rule                    
            float currentSpeed_normalized = new Vector2(_rigidbody.linearVelocity.x, _rigidbody.linearVelocity.z).magnitude;
            float currentRotationSpeed_normalized = -_rigidbody.angularVelocity.y;
            float currentAcceleration_normalized = currentAcceleration;
            float currentRotationAcceleration_normalized = currentRotationAcceleration;

            // float sinAngle = Mathf.Sin(angleRotation_normalized*Mathf.Deg2Rad); // Sine of the angle ->[-180, 180) -> [-1, 1]
            // float cosAngle = Mathf.Cos(angleRotation_normalized*Mathf.Deg2Rad); // Cosine of the angle ->[-180, 180) -> [-1, 1]
            float angleNormalizedFactor = 180f;
            if (useRadian)
            {
                angleRotation_normalized = angleRotation_normalized * Mathf.Deg2Rad; // Convert to Radian
                angleNormalizedFactor = Mathf.PI;
            }
            
            if (useNormalized)
            {
                robotPosX_normalized = transform.localPosition.x / boxSize.x;
                robotPosZ_normalized = transform.localPosition.z / boxSize.z;
                angleRotation_normalized = angleRotation_normalized / angleNormalizedFactor; // Normalize angle [-180, 180) -> [-1, 1)
                currentSpeed_normalized = currentSpeed / maxSpeed;
                currentRotationSpeed_normalized = currentRotationSpeed / maxRotationSpeed;
                currentAcceleration_normalized = currentAcceleration / maxAcceleration;
                currentRotationAcceleration_normalized = currentRotationAcceleration / maxRotationAccleration;
            }
            
        
            Vector3 goalPos = Vector3.zero;
            float goalPosX_normalized = 0; 
            float goalPosZ_normalized = 0;
            int taskID = 0;
            int task_ind = -1;
            int task_busy = 0;
            int completed = 0;
            if (taskClass != null)
            {
                goalPos = getGoalPos();
                taskID = taskClass.taskID;
                goalPosX_normalized = goalPos.x;
                goalPosZ_normalized = goalPos.z;
                task_ind = taskClass.task_ind;
                task_busy = taskClass.getBusy() ? 1 : 0;
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
                getCollisionTagID(),
                taskID,
                goalPosX_normalized,
                goalPosZ_normalized,
                task_ind,
                task_busy,
                completed,                
                Reward
            };
            (lidarData, safetyData) = getLidarData(useNormalized);

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
            StepCount += 1;

            if (!checkWait() && allowCommandsInput)
            {
                MoveAgent(actions);
            }
            else
            {
                u = Vector2.zero;
            }

            addTimeReward();
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
            if(controllerType == "position")
            {
                transform.position = new Vector3(
                    transform.position.x + act.x,
                    transform.position.y,
                    transform.position.z + act.y
                );

                float angle = Mathf.Atan2(act.y, act.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, angle, 0);
                U_constraint=0;
                return;
            }

            (float[] action, float U) = checkConstraint(act[0], -act[1]);
            if (controllerType == "velocity") // Velocity Plant Model
            {
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
            if (absoluteCoordinate)
            {
                u = new Vector2(_state[0] + action[0], _state[1] + action[1]);
            }
            else
            {
                u = new Vector2( action[0], action[1]);
            }
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
            if (!useCSVExport || StepCount % CSVRate != 0 || (StepCount < aData.getSize()))
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
            sData.add(Mathf.Round(StepCount * Time.deltaTime*1000)/1000); // Current Time
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
            sData.add(getCollisionTagID()); // Collision Tag ID
            sData.add(Reward); // Reward
            sData.add(_safetyViolated); // Safety Violated
            sData.add(U_constraint); // Control Constraint

            (lidarDataUnnormalized, safetyData) = getLidarData(false);
            sData.add(lidarDataUnnormalized); // Lidar Data (rayAngle, rayDistance, rayTag) * num_rays

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
                for (int i = 0; i < lidarDataUnnormalized.Length/3; i++)
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

        public int calculateObservationSize(int obsSize, int num_rays=0, float angle=180)
        {
            if (angle < 0 || angle > 180)
            {
                angle = 180;
            }
            if (angle == 180f)
            {
                obsSize += 3 * num_rays * 2;
            }
            else
            {
                obsSize += 3 * (num_rays * 2 + 1);
            }
            
            return obsSize;
        }

        public void updateAgentParameters(parameters param)
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
            controllerType = param.agentParams.controllerType.ToLower();
            infiniteAcceleration = param.agentParams.infiniteAcceleration;
            absoluteCoordinate = param.agentParams.absoluteCoordinate;
            debugArrow = param.agentParams.arrowParams.debugArrow;
            debugOnlyDirection = param.agentParams.arrowParams.debugOnlyDirection;
            debugOnlySpeed = param.agentParams.arrowParams.debugOnlySpeed;
            debugOnlyAcceleration = param.agentParams.arrowParams.debugOnlyAcceleration;
            debugOnlyGoal = param.agentParams.arrowParams.debugOnlyGoal;
            debugArrow2DMode = param.agentParams.arrowParams.debugArrow2DMode;
            _safetyRadius = param.agentParams.safetyRadius;
            verbose = param.agentParams.verbose;
            useRadian = param.unityParams.useRadian;
            lineRendererMaxPathPositionListCount = param.agentParams.lineRendererMaxPathPositionListCount;
            lineRendererMinPathDistance = param.agentParams.lineRendererMinPathDistance;
            lineRendererWidth = param.agentParams.lineRendererWidth;
            debugArrowParams = new float[5]{
                param.agentParams.arrowParams.stemLength,
                param.agentParams.arrowParams.stemWidth,
                param.agentParams.arrowParams.tipLength,
                param.agentParams.arrowParams.tipWidth,
                param.agentParams.arrowParams.yOffset
            };
            allowedlightingOn = param.agentParams.allowedlightingOn;
            setCollisionOn(param.agentParams.allowedCollisionOn);
            foreach (Transform child in transform.Find("Body"))
            {
                if (child.name.Contains("Light"))
                {
                    foreach (Transform child2 in child)
                    {
                        var lightComponent = child2.GetComponent<Light>();
                        if (lightComponent != null)
                        {
                            lightComponent.enabled = allowedlightingOn;
                        }
                    }
                }
            }
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
                _obs_size = calculateObservationSize(_obs_size, param.agentParams.rayParams.rayDirections, param.agentParams.rayParams.maxRayDegrees);
            }
        }
        
        public int getObservationSize()
        {
            return _obs_size;
        }

        void Start()
        {
            m_rayPerceptionSensorComponent3D.name = "RayPerceptionSensor_" + getID();
        }
    }

}

using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using System;
using multiagent.controller;
using multiagent.agent;
using multiagent.parameterJson;

namespace multiagent.robot
{

    public class Robot2 : Agent2Template
    {
        [SerializeField] public float maxSpeed = .7f; // meters per second
        [SerializeField] public float maxRotationSpeed = 2.11f; // degrees per second
        [SerializeField] public float maxAcceleration = 2.72f; // degrees per second
        [SerializeField] public float maxRotationAccleration = 8.23f; // degrees per second
        private float _bodyRadius = 0.331f; // meters
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
            m_rayPerceptionSensorComponent3D.name = "RayPerceptionSensor_" + getID();
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


        public void CollectObservations()
        {
            float robotPosX_normalized = transform.localPosition.x / boxSize.x;
            float robotPosZ_normalized = transform.localPosition.z / boxSize.z;
            // Normalize position to be within [-1, 1]
            float angleRotation = (transform.localRotation.eulerAngles.y + 180f) % 360f - 180f;
            // float angleRotation_normalized = angleRotation/180f; // Normalize angle [-180, 180) -> [-1, 1)
            float sinAngle = Mathf.Sin(angleRotation); // Sine of the angle ->[-180, 180) -> [-1, 1]
            float cosAngle = Mathf.Cos(angleRotation); // Cosine of the angle ->[-180, 180) -> [-1, 1]
            float[] observation = new float[] {
                robotPosX_normalized,
                robotPosZ_normalized,
                sinAngle,
                cosAngle,
                currentSpeed / maxSpeed,
                currentRotationSpeed / maxRotationSpeed,
                currentAcceleration / maxAcceleration,
                currentRotationAcceleration / maxRotationAccleration,
            };

            // var rayOutputs = RayPerceptionSensor.Perceive(m_rayPerceptionSensorComponent3D.GetRayPerceptionInput()).RayOutputs;
            // int lengthOfRayOutputs = rayOutputs.Length;
            // for (int i = 0; i < lengthOfRayOutputs; i++)
            // {
            //     GameObject goHit = rayOutputs[i].HitGameObject;
            //     if (goHit != null)
            //     {
            //         var rayDirection = rayOutputs[i].EndPositionWorld - rayOutputs[i].StartPositionWorld;
            //         var scaledRayLength = rayDirection.magnitude;
            //         float rayHitDistance = rayOutputs[i].HitFraction * scaledRayLength;

            //         // Print info:
            //         string dispStr = "";
            //         dispStr = dispStr + "__RayPerceptionSensor - HitInfo__:\r\n";
            //         dispStr = dispStr + "GameObject name: " + goHit.name + "\r\n";
            //         dispStr = dispStr + "Hit distance of Ray: " + rayHitDistance + "\r\n";
            //         dispStr = dispStr + "GameObject tag: " + goHit.tag + "\r\n";
            //         Debug.Log(dispStr);
            //     }
            // }

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
            Vector3[] act;
            if (absoluteCoordinate)
            {
                act = new Vector3[] {
                    new Vector3(_state[0], _state[1], 0),
                    new Vector3(_state[0] + action[0], _state[1] + action[1], 0)
                };
            }
            else
            {
                act = new Vector3[] {
                    new Vector3(_state[0], _state[1], 0),
                    new Vector3(action[0], action[1], 0)
                };
            }

            Vector3[] S = new Vector3[] { _state }; // State information
            Vector3[] dS = new Vector3[] { _dstate }; // Time derivative of State information
            Vector3[] ddesS = null; //TODO:  Desired State information
            if (!velocityControl)
            {
                ddesS = new Vector3[] { new Vector3(0.0f, 0f, 0.0f) };
            }
            // Debug.Log("Act: " + act[0] + " " + act[1] );

            // Vector2 u;
            // if (absoluteCoordinate)
            // {
            //     u = new Vector2(_state[0] + action[0], _state[1] + action[1]);
            // }
            // else
            // {
            //     u = new Vector2( action[0], action[1]);
            // }

            u = control.GetControl(act, S, ddesS, dS);
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
            sData.add(StepCount * Time.deltaTime);
            sData.add(_state);
            sData.add(_dstate);
            sData.add(actionInput);
            sData.add(getGoalPos());
            if (taskClass != null)
            {
                sData.add(taskClass.getCurrentGoalID());
                sData.add(taskClass.getCurrentGoalType());
            }
            else
            {
                sData.add(0);
                sData.add(-1);
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

        public void updateAgentParameters(parameters param)
        {
            maxSpeed = param.agentParams.maxSpeed;
            maxRotationSpeed = param.agentParams.maxRotationSpeed;
            maxAcceleration = param.agentParams.maxAcceleration;
            maxRotationAccleration = param.agentParams.maxRotationAccleration;
            velocityControl = param.agentParams.velocityControl;
            absoluteCoordinate = param.agentParams.absoluteCoordinate;
            debugArrow = param.agentParams.debugArrow;
            setDecisionRequestParams(param.agentParams.maxTimeSteps, param.agentParams.decisionPeriod);
            modifyReward(
                param.agentParams.rewardParams.collisionEnterReward,
                param.agentParams.rewardParams.collisionStayReward,
                param.agentParams.rewardParams.timeReward,
                param.agentParams.rewardParams.goalReward);
        }
    }

}

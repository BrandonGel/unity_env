using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.AI;
using System.Collections;
using Unity.Burst.Intrinsics;
using System;
using UnityEngine.UIElements;
using UnityEditor.UI;
using multiagent.controller;
using Unity.MLAgents.Policies;
using UnityEditorInternal;
using Unity.MLAgents.Integrations.Match3;
using multiagent.goal;

namespace multiagent.agent
{

    public class Robot : AgentTemplate
    {
        [SerializeField] private float _maxSpeed = .7f; // meters per second
        [SerializeField] private float _maxRotationSpeed = 2.11f; // degrees per second
        [SerializeField] private float _maxAcceleration = 2.72f; // degrees per second
        [SerializeField] private float _maxRotationAccleration = 8.23f; // degrees per second
        private float _bodyRadius = 0.331f; // meters
        private Controller control = new Controller();
        public ctrlOption _controllerName;
        private string _controllerNameStr;
        public ctrlType _controllerType;
        private string _controllerTypeStr;
        private Rigidbody _rigidbody;
        private bool isControllerInit = false;
        [SerializeField] private bool velocityControl = true;

        [SerializeField] float currentSpeed = 0;
        [SerializeField] float currentRotationSpeed = 0;
        [SerializeField] float currentAcceleration = 0;
        [SerializeField] float currentRotationAcceleration = 0;

        [SerializeField] Vector3 _state = default;
        [SerializeField] Vector3 _dstate = default;
        [SerializeField] Vector3 _ddstate = default;
        [SerializeField] bool _absoluteCoordinate = false;

        [SerializeField] float U_constraint = 0;
        public GameObject arrow;
        public bool debugArrow = false;
        private bool usingArrow = false;
        GameObject arrowObj, arrowObj2, arrowObj3, arrowObj4, arrowObj5;
        private Vector3 newSpawnPosition, accelerationVector, rotationAccelerationVector;
        private Quaternion newSpawnOrientation;

        public override void Initialize()
        {
            _controllerNameStr = Enum.GetName(_controllerName.GetType(), _controllerName);
            _controllerTypeStr = Enum.GetName(_controllerType.GetType(), _controllerType);
            _rigidbody = GetComponent<Rigidbody>();
            initExtra();
            
            // Initialize the constraints
            if (velocityControl)
            {
                minLim = new float[] {
                -_maxSpeed,
                -_maxRotationSpeed,
                };
                maxLim = new float[] {
                    _maxSpeed,
                    _maxRotationSpeed,
                };
            }
            else
            {
                minLim = new float[] {
                -_maxAcceleration,
                -_maxRotationAccleration
                };
                maxLim = new float[] {
                    _maxAcceleration,
                    _maxRotationAccleration
                };
            }

            updateState();
            generateArrow();
            newSpawnPosition = transform.position;
            newSpawnOrientation = transform.rotation;
            setGoal();
        }

        private void generateArrow()
        {

            if (debugArrow == true && usingArrow == false)
            {
                usingArrow = true;
                Vector3 arrowPosition = transform.position;
                Quaternion arrowOrientation = transform.rotation;

                // Linear Velocity Arrow
                arrowObj = Instantiate(arrow, arrowPosition, arrowOrientation);
                arrowObj.GetComponent<ArrowGenerator>().setParam("r", -_maxSpeed, _maxSpeed, Color.red);
                arrowObj.transform.parent = gameObject.transform;

                // Angular Velocity Arrow
                arrowObj2 = Instantiate(arrow, arrowPosition, arrowOrientation);
                arrowObj2.GetComponent<ArrowGenerator>().setParam("u", -_maxRotationSpeed, _maxRotationSpeed, Color.red, true);
                arrowObj2.transform.parent = gameObject.transform;

                // Linear Acceleration Arrow
                arrowObj3 = Instantiate(arrow, arrowPosition, arrowOrientation);
                arrowObj3.GetComponent<ArrowGenerator>().setParam("r", -_maxAcceleration, _maxAcceleration, Color.blue);
                arrowObj3.transform.parent = gameObject.transform;

                // Angular Velocity Arrow
                arrowObj4 = Instantiate(arrow, arrowPosition, arrowOrientation);
                arrowObj4.GetComponent<ArrowGenerator>().setParam("u", -_maxRotationAccleration, _maxRotationAccleration, Color.blue, true);
                arrowObj4.transform.parent = gameObject.transform;

                // Goal Arrow
                arrowObj5 = Instantiate(arrow, arrowPosition, arrowOrientation);
                arrowObj5.GetComponent<ArrowGenerator>().setParam("", -1, 1, Color.green);
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

        public override void OnEpisodeBegin()
        {
            CurrentEpisode++;
            CumulativeReward = 0f;
            transform.position = newSpawnPosition;
            transform.rotation = newSpawnOrientation;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            accelerationVector = Vector3.zero;
            rotationAccelerationVector = Vector3.zero;
            updateState();
            GetGround();
            _ddstate = Vector3.zero;
        }


        public override void CollectObservations(VectorSensor sensor)
        {


            float robotPosX_normalized = transform.localPosition.x / boxSize.x;
            float robotPosZ_normalized = transform.localPosition.z / boxSize.z;

            float angleRotation = (transform.localRotation.eulerAngles.y + 180f) % 360f - 180f;
            // float angleRotation_normalized = angleRotation/180f; // Normalize angle [-180, 180) -> [-1, 1)
            float sinAngle = Mathf.Sin(angleRotation); // Sine of the angle ->[-180, 180) -> [-1, 1]
            float cosAngle = Mathf.Cos(angleRotation); // Cosine of the angle ->[-180, 180) -> [-1, 1]

            sensor.AddObservation(robotPosX_normalized);
            sensor.AddObservation(robotPosZ_normalized);
            sensor.AddObservation(sinAngle);
            sensor.AddObservation(cosAngle);
            // Debug.Log(robotPosX_normalized + " " + robotPosZ_normalized + " " + angleRotation);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
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

        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            if (!isControllerInit)
            {
                control.InitControl(actions.ContinuousActions.Length, minLim, maxLim, _controllerNameStr);
                isControllerInit = true;
            }

            int decisionPeriod = GetComponent<DecisionRequester>().DecisionPeriod;
            if (!checkWait(decisionPeriod))
            {
                MoveAgent(actions.ContinuousActions);
            }
            else
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
            

            AddReward(-2f / MaxStep);
            CumulativeReward = GetCumulativeReward();
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
                float desiredSpeed = _maxSpeed * speedCoefficent;
                currentSpeed = transform.InverseTransformDirection(_rigidbody.linearVelocity).x;
                float speedDifference = desiredSpeed - currentSpeed;
                Vector3 velocityDifference = transform.right * speedDifference;
                accelerationVector = velocityDifference / Time.deltaTime;
                _rigidbody.AddForce(velocityDifference, ForceMode.VelocityChange);

                float rotationCoefficent = action[1];
                float desiredRotation = _maxRotationSpeed * rotationCoefficent;
                currentRotationSpeed = transform.InverseTransformDirection(_rigidbody.angularVelocity).y;
                float rotationDifference = desiredRotation - currentRotationSpeed;
                Vector3 angularDifference = transform.up * rotationDifference;
                rotationAccelerationVector = angularDifference / Time.deltaTime;
                _rigidbody.AddTorque(_rigidbody.inertiaTensor.y * rotationAccelerationVector, ForceMode.Force);
            }
            else // Acceleration Plant Model w/ Constraint
            {
                float acceleartionCoefficent = action[0];
                currentAcceleration = _maxAcceleration * acceleartionCoefficent;
                currentSpeed = transform.InverseTransformDirection(_rigidbody.linearVelocity).x;
                float projectedSpeed = currentSpeed + currentAcceleration * Time.deltaTime;
                if (MathF.Abs(projectedSpeed) > _maxSpeed)
                {
                    projectedSpeed = MathF.Sign(projectedSpeed) * _maxSpeed;
                    currentAcceleration = (projectedSpeed - currentSpeed) / Time.deltaTime;
                }

                float rotationAcceleartionCoefficent = action[1];
                currentRotationAcceleration = _maxRotationAccleration * rotationAcceleartionCoefficent;
                currentRotationSpeed = transform.InverseTransformDirection(_rigidbody.angularVelocity).y;
                float projectedRotationSpeed = currentRotationSpeed + currentRotationAcceleration * Time.deltaTime;
                if (Math.Abs(projectedRotationSpeed) > _maxRotationSpeed)
                {
                    projectedRotationSpeed = MathF.Sign(projectedRotationSpeed) * _maxRotationSpeed;
                    currentRotationAcceleration = (projectedRotationSpeed - currentSpeed) / Time.deltaTime;
                }
                (float[] projectedAction, float U2) = checkConstraint(projectedSpeed / _maxSpeed, projectedRotationSpeed / _maxRotationSpeed);
                if (U2 > 1f)
                {
                    projectedSpeed = projectedAction[0] * _maxSpeed;
                    projectedRotationSpeed = projectedAction[1] * _maxRotationSpeed;
                    currentAcceleration = (projectedSpeed - currentSpeed) / Time.deltaTime;
                    currentRotationAcceleration = (projectedRotationSpeed - currentRotationSpeed) / Time.deltaTime;
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

        public override void MoveAgent(ActionSegment<float> action)
        {
            Vector3[] act;
            if (_absoluteCoordinate)
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
            // if (_absoluteCoordinate)
            // {
            //     u = new Vector2(_state[0] + action[0], _state[1] + action[1]);
            // }
            // else
            // {
            //     u = new Vector2( action[0], action[1]);
            // }

            Vector2 u = control.GetControl(act, S, ddesS, dS);
            // Debug.Log("u: " + u[0] + " " + u[1] );
            plant(u);
            // Debug.Log($"Goal: {act[0]}, {act[1]} | Control {u}");
        }

        private void updateState()
        {
            if (_absoluteCoordinate)
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
        }

        public override void GoalReached()
        {
            if (getGoalReached() == false)
            {
                setGoalReached(true);
                AddReward(1.0f);
                CumulativeReward = GetCumulativeReward();
                startWaitCounter(); 
            }
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

    }

}

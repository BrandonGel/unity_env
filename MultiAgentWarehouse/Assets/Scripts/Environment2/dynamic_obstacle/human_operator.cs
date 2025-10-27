using UnityEngine;
using System;
using multiagent.parameterJson;
public class human_operator : MonoBehaviour
{

    [SerializeField] public float maxSpeed = 1f; // meters per second
    [SerializeField] public float maxRotationSpeed = 3.3f; // radians per second
    [SerializeField] public float maxAcceleration = 10f; // meters per second^2
    [SerializeField] public float maxRotationAccleration = 33f; // radians per second^2

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void updateAgentParameters(parameters param)
    {
        // maxSpeed = param.maxSpeed;
        // maxRotationSpeed = param.maxRotationSpeed;
        // maxAcceleration = param.maxAcceleration;
        // maxRotationAccleration = param.maxRotationAccleration;
    }
}

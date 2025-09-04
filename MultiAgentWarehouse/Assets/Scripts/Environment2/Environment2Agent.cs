using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
public class Environment2Agent : Agent
{
    void Start()
    {


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

    void Update()
    {

    }

    void FixedUpdate()
    {

    }
}
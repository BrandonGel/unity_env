using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;
using multiagent.goal;


public class Goal : MonoBehaviour
{

    public Vector3 position;
    public Vector2 position2D;
    
    public int robotID = -1;
    public int startTimeStep = 0;
    public int counter = 0;
    public int totalTimesteps = 1;
    public float goalWait = 1000f;
    public float goalWaitProbability = 1f;
    public int goalID = -1;
    public int goalType = 0;
    
    

    void Start()
    {
        position = transform.position;
        position2D = new Vector2(transform.position.x, transform.position.z);
    }

    



}

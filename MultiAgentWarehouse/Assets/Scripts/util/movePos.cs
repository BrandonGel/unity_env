using UnityEngine;
using multiagent.agent;
using multiagent.util;

namespace multiagent.palette
{
    public class movingObject : MonoBehaviour
    {

        public Vector3[] waypoints;
        public Vector3 offset = default;
        int[] pathIdx;
        float timeLength = 0;
        int numMovingSteps = 1;
        bool on = false;
        int currentMovingStep = 0;
        int currentPathIdx = 0;
        public GameObject robotObj;
        public void setWaypoints(Vector3[] waypoints, float timeLength = 0, bool on = true)
        {
            this.waypoints = new Vector3[waypoints.Length];
            this.timeLength = timeLength;
            this.numMovingSteps = Mathf.Max((int)(timeLength / Time.deltaTime), 1);

            for (int i = 0; i < waypoints.Length; i++)
            {
                this.waypoints[i] = waypoints[i] + offset;
            }
            this.pathIdx = new int[Mathf.Max(1, waypoints.Length)];
            for (int i = 0; i < pathIdx.Length; i++)
            {
                pathIdx[i] = (int)(numMovingSteps / waypoints.Length * i);
            }
            currentMovingStep = 0;
            currentPathIdx = 0;
            this.on = on;
            enableRenderer(true);
        }

        public void assignRobot(GameObject robotObj)
        {
            this.robotObj = robotObj;
        }

        public void setOffset(Vector3 offset)
        {
            this.offset = offset;
        }

        public void enableRenderer(bool turnon = true)
        {

            foreach (Transform child in GetComponent<Transform>()) {
                Renderer rend = child.gameObject.GetComponent< Renderer >();
                if (rend != null)
                {
                    child.gameObject.GetComponent< Renderer >().enabled = turnon;
                }
            }
            GetComponent< Renderer >().enabled = turnon;

        }

        void Start()
        {
            waypoints = null;
        }

        public void resetParameters()
        {
            waypoints = null;
            pathIdx = null;
            timeLength = 0;
            numMovingSteps = 1;
            currentMovingStep = 0;
            currentPathIdx = 0;
        }
        void Update()
        {
            
            if (waypoints == null)
            {
                enableRenderer(this.on);
                if (robotObj != null)
                {

                    Transform robotTransform = robotObj.GetComponent<Transform>();
                    Vector3 robotPosition = robotTransform.position;
                    Vector3 robotScale = robotObj.GetComponent<Transform>().localScale;
                    Vector3 robotCenter = robotObj.GetComponent<BoxCollider>().center;
                    Vector3 robotSize = robotObj.GetComponent<BoxCollider>().size;
                    transform.position = new Vector3(
                            robotTransform.position.x,
                            robotTransform.position.y + robotSize.y * robotScale.y / 2,
                            robotTransform.position.z
                        ) + offset;
                    transform.rotation = robotTransform.rotation;
                    return;
                }
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;

            }
            else
            {
                //Break when robot completed the wait time
                if (!robotObj.GetComponent<Robot>().checkWait())
                {
                    waypoints = null;
                    return;
                }

                // Sync up with the robot wait time
                currentMovingStep = robotObj.GetComponent<Robot>().getWaitCounter();

                // Move onto the next path
                if (currentMovingStep > pathIdx[currentPathIdx + 1])
                {
                    currentPathIdx = Mathf.Min(currentPathIdx + 1, waypoints.Length - 2);
                }


                Vector3 point1 = waypoints[currentPathIdx];
                Vector3 point2 = waypoints[currentPathIdx + 1];
                Vector3 dir = point2 - point1;
                float frac = Util.linearInterpolate(pathIdx[currentPathIdx], pathIdx[currentPathIdx + 1], currentMovingStep);
                Vector3 newPos = point1 + frac * dir;
                transform.position = newPos;
                transform.rotation = robotObj.GetComponent<Transform>().rotation;
            }

        }




    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;
using multiagent.goal;
using multiagent.util;
using multiagent.agent;

namespace multiagent.palette
{
    public class palette
    {
        public GameObject combinedTopBottom, topBox, bottomBox;

        public Vector3 bottomBoxOffset, topBoxOffset;

        public palette(GameObject combinedTopBottom)
        {
            this.combinedTopBottom = combinedTopBottom;
            topBox = combinedTopBottom.transform.Find("crate(Clone)").gameObject;
            bottomBox = combinedTopBottom.transform.Find("PalletEmpty(Clone)").gameObject;
            Util.SetBoxProperties(topBox, false, false, false, null);
            Util.SetBoxProperties(bottomBox, false, false, false, null);
            bottomBoxOffset = new Vector3(
                0,
                bottomBox.transform.position.y,
                0
            );
            topBoxOffset = new Vector3(
                0,
                topBox.transform.position.y,
                0
            );
            enableRenderer(topBox, false);
            enableRenderer(bottomBox, false);
            // MeshRenderer render = topBox.GetComponent<MeshRenderer>();
            // render.enabled = false;
            // render = bottomBox.GetComponent<MeshRenderer>();
            // render.enabled = false;
        }

        public void resetParameters()
        {
            topBox.GetComponent<movingObject>().resetParameters();
            bottomBox.GetComponent<movingObject>().resetParameters();
        }

        public void assignRobot(GameObject robot)
        {
            topBox.GetComponent<movingObject>().assignRobot(robot);
            bottomBox.GetComponent<movingObject>().assignRobot(robot);
        }

        public void getPalette(Robot robotComponent, goalClass _goalClass, int goalType)
        {
            float goalWait = _goalClass.goalWait;
            Vector3 goalPosition = _goalClass.position;


            Transform robotTransform = robotComponent.GetComponent<Transform>();
            Vector3 robotPosition = robotTransform.position;
            Vector3 robotScale = robotComponent.GetComponent<Transform>().localScale;
            Vector3 robotCenter = robotComponent.GetComponent<BoxCollider>().center;
            Vector3 robotSize = robotComponent.GetComponent<BoxCollider>().size;
            Vector3[] waypoints;
            switch (goalType)
            {
                case 1:
                    // enableRenderer(topBox, true);
                    // enableRenderer(bottomBox, true);
                    waypoints = new Vector3[3];
                    waypoints[0] = new Vector3(
                        goalPosition.x - 1,
                        goalPosition.y + 1,
                        robotPosition.z
                    );
                    waypoints[1] = new Vector3(
                        robotPosition.x,
                        goalPosition.y + 1,
                        robotPosition.z
                    );
                    waypoints[2] = new Vector3(
                        robotTransform.position.x,
                        robotTransform.position.y + robotSize.y * robotScale.y / 2,
                        robotTransform.position.z
                    );
                    topBox.GetComponent<movingObject>().setWaypoints(waypoints, goalWait, true);
                    bottomBox.GetComponent<movingObject>().setWaypoints(waypoints, goalWait, true);
                    break;
                case 2:

                    waypoints = new Vector3[3];
                    waypoints[0] = new Vector3(
                        robotPosition.x,
                        goalPosition.y + robotSize.y * robotScale.y,
                        robotPosition.z
                    );
                    waypoints[1] = new Vector3(
                        robotPosition.x,
                        goalPosition.y + 1,
                        robotPosition.z
                    );
                    waypoints[2] = new Vector3(
                        robotPosition.x,
                        goalPosition.y + 1,
                        goalPosition.z
                    );
                    topBox.GetComponent<movingObject>().setWaypoints(waypoints, goalWait, false);
                    bottomBox.GetComponent<movingObject>().setWaypoints(waypoints, goalWait, false);
                    break;
                case 3:
                waypoints = new Vector3[3];
                    waypoints[0] = new Vector3(
                        robotPosition.x,
                        goalPosition.y + 1,
                        goalPosition.z
                    );
                    waypoints[1] = new Vector3(
                        robotPosition.x,
                        goalPosition.y + 1,
                        robotPosition.z
                    );
                    waypoints[2] = new Vector3(
                        robotPosition.x,
                        goalPosition.y + robotSize.y * robotScale.y,
                        robotPosition.z
                    );
                    bottomBox.GetComponent<movingObject>().setWaypoints(waypoints, goalWait, true);
                    break;
                case 4:
                    waypoints = new Vector3[3];
                    waypoints[0] = new Vector3(
                        robotTransform.position.x,
                        robotTransform.position.y + robotSize.y * robotScale.y / 2,
                        robotTransform.position.z
                    );
                    waypoints[1] = new Vector3(
                        robotPosition.x,
                        goalPosition.y + 1,
                        robotPosition.z
                    );
                    waypoints[2] = new Vector3(
                        goalPosition.x - 1,
                        goalPosition.y + 1,
                        robotPosition.z
                    );
                    bottomBox.GetComponent<movingObject>().setWaypoints(waypoints, goalWait, false);
                    break;
                default:
                    break;
            }
        }

        public void enableRenderer(GameObject box, bool turnon = true)
        {

            foreach (Transform child in box.GetComponent<Transform>()) {
                Renderer rend = child.gameObject.GetComponent< Renderer >();
                if (rend != null)
                {
                    child.gameObject.GetComponent< Renderer >().enabled = turnon;
                }
            }
            box.GetComponent< Renderer >().enabled = turnon;

        }

        public void setOffset(Vector3 bottomBoxOffset, Vector3 topBoxOffset)
        {
            topBox.GetComponent<movingObject>().setOffset(topBoxOffset);
            bottomBox.GetComponent<movingObject>().setOffset(bottomBoxOffset);
        }
    }
}

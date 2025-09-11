using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using multiagent.robot;
using multiagent.task;
using multiagent.util;

public class ReplayPath
{

    public void updatePath(float t,List<GameObject> robots, Dictionary<string, List<Position>> schedule, Vector3 scale = default)
    {
        if (scale == default)
        {
            scale = new Vector3(1, 1, 1);
        }

        for (int robotId = 0; robotId < robots.Count; robotId++)
        {
            string robotKey = schedule.Keys.ElementAt(robotId);
            GameObject robot = robots[robotId];
            List<Position> traj = schedule[robotKey];

            // Get New Position
            int idx = Mathf.Min((int)t, traj.Count - 1);
            int idxNext = Mathf.Min((int)t + 1, traj.Count - 1);
            Vector3 pos1 = new Vector3(traj[idx].x , 0, traj[idx].y );
            Vector3 pos2 = new Vector3(traj[idxNext].x,  0, traj[idxNext].y );
            float frac = Util.linearInterpolate(traj[idx].t, traj[idxNext].t, t);
            Vector3 newPosition = Util.interpolate(pos1, pos2, frac);
            newPosition = Vector3.Scale(newPosition, scale);    

            // Get Heading
            Vector3 prevPosition = robot.transform.position;
            float currentHeading = robot.transform.localRotation.eulerAngles.y;
            float targetHeading;
            if (prevPosition == newPosition)
            {
                targetHeading = currentHeading;
            }
            else
            {
                targetHeading = Util.CalculateHeading(prevPosition, newPosition);
            }
            
            float bestHeading = targetHeading;

            // Update Robot
        
            float headingRotationSpeed = robot.GetComponent<Robot>().maxRotationSpeed*360f/Mathf.PI; // degrees per second
            robot.transform.rotation = Quaternion.RotateTowards(
                                            robot.transform.rotation,
                                            Quaternion.Euler(0, bestHeading, 0),
                                            headingRotationSpeed * Time.deltaTime
                                        );
            robot.transform.position = newPosition;
        }

    }

}
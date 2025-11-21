using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using multiagent.robot;
using multiagent.task;
using multiagent.util;

public class ReplayPath
{

    public bool updatePath(float t,List<GameObject> robots, Dictionary<string, List<Position>> schedule, Vector3 scale = default)
    {
        if (scale == default)
        {
            scale = new Vector3(1, 1, 1);
        }

        if (schedule == null || schedule.Count == 0)
        {
            return true;
        }

        int totalAgents = 0;
        int agentsAtEnd = 0;

        for (int robotId = 0; robotId < robots.Count && robotId < schedule.Count; robotId++)
        {
            string robotKey = schedule.Keys.ElementAt(robotId);
            GameObject robot = robots[robotId];
            
            if (!schedule.ContainsKey(robotKey))
            {
                continue;
            }

            List<Position> traj = schedule[robotKey];

            if (traj == null || traj.Count == 0)
            {
                continue;
            }

            totalAgents++;

            // Get New Position
            int idx = Mathf.Min((int)t, traj.Count - 1);
            int idxNext = Mathf.Min((int)t + 1, traj.Count - 1);
            
            // Check if we're at the end of the trajectory
            bool isAtEnd = idx == traj.Count - 1;
            
            if (isAtEnd) // In termination state
            {
                agentsAtEnd++;
                robot.GetComponent<Robot2>().StepCount = 0;
                continue;
            }
            
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
            float headingRotationSpeed = robot.GetComponent<Robot2>().maxRotationSpeed * 360f / Mathf.PI; // degrees per second
            robot.transform.rotation = Quaternion.RotateTowards(
                                                robot.transform.rotation,
                                                Quaternion.Euler(0, bestHeading, 0),
                                                headingRotationSpeed * Time.deltaTime
                                            );
            robot.transform.position = newPosition;  
            robot.GetComponent<Robot2>().StepCount += 1;
        }

        // Return true only if all agents have reached the end of their trajectories
        return totalAgents > 0 && agentsAtEnd == totalAgents;
    }

    public bool updateCSVPath(float t, List<GameObject> robots, Dictionary<string, List<PositionOrientation>> agentPoses, Vector3 scale = default)
    {
        if (scale == default)
        {
            scale = new Vector3(1, 1, 1);
        }

        if (agentPoses == null || agentPoses.Count == 0)
        {
            return true;
        }

        // Get list of robot keys in order
        List<string> robotKeys = agentPoses.Keys.ToList();
        int totalAgents = 0;
        int agentsAtEnd = 0;

        for (int robotId = 0; robotId < robots.Count && robotId < robotKeys.Count; robotId++)
        {
            string robotKey = robotKeys[robotId];
            GameObject robot = robots[robotId];
            
            if (!agentPoses.ContainsKey(robotKey))
            {
                continue;
            }

            List<PositionOrientation> traj = agentPoses[robotKey];

            if (traj == null || traj.Count == 0)
            {
                continue;
            }

            totalAgents++;

            // Find the correct indices based on time
            int idx = 0;
            int idxNext = 0;
            
            // Find the index where time is less than or equal to current time
            for (int i = 0; i < traj.Count; i++)
            {
                if (traj[i].time <= t)
                {
                    idx = i;
                }
                else
                {
                    break;
                }
            }

            // Check if we're at the end of the trajectory
            bool isAtEnd = idx >= traj.Count - 1;
            
            if (isAtEnd)
            {
                agentsAtEnd++;
                // Use the last position
                PositionOrientation lastPos = traj[traj.Count - 1];
                Vector3 finalPosition = new Vector3(lastPos.x, 0, lastPos.y);
                finalPosition = Vector3.Scale(finalPosition, scale);
                
                // Use theta directly for orientation (convert from radians to degrees)
                float finalHeading = lastPos.theta * Mathf.Rad2Deg;
                
                robot.transform.position = finalPosition;
                robot.transform.rotation = Quaternion.Euler(0, finalHeading, 0);
                robot.GetComponent<Robot2>().StepCount = 0;
                continue;
            }

            idxNext = idx + 1;

            // Get positions for interpolation
            PositionOrientation pos1 = traj[idx];
            PositionOrientation pos2 = traj[idxNext];
            
            Vector3 vecPos1 = new Vector3(pos1.x, 0, pos1.y);
            Vector3 vecPos2 = new Vector3(pos2.x, 0, pos2.y);

            // Interpolate position
            float frac = Util.linearInterpolate(pos1.time, pos2.time, t);
            Vector3 newPosition = Util.interpolate(vecPos1, vecPos2, frac);
            newPosition = Vector3.Scale(newPosition, scale);

            // Interpolate orientation (theta)
            float theta1 = pos1.theta * Mathf.Rad2Deg; // Convert radians to degrees
            float theta2 = pos2.theta * Mathf.Rad2Deg;
            
            // Handle angle wrapping for interpolation
            float angleDiff = Mathf.DeltaAngle(theta1, theta2);
            float targetHeading = theta1 + angleDiff * frac;
            
            // Normalize to 0-360 range
            if (targetHeading < 0)
                targetHeading += 360f;
            if (targetHeading >= 360f)
                targetHeading -= 360f;

            // Update Robot with smooth rotation
            float headingRotationSpeed = robot.GetComponent<Robot2>().maxRotationSpeed * 360f / Mathf.PI; // degrees per second
            robot.transform.rotation = Quaternion.RotateTowards(
                                                robot.transform.rotation,
                                                Quaternion.Euler(0, targetHeading, 0),
                                                headingRotationSpeed * Time.deltaTime
                                            );
            robot.transform.position = newPosition;

            // Update Robot Velocity
            Vector3 velocity1 = new Vector3(pos1.vx, 0, pos1.vy);
            Vector3 velocity2 = new Vector3(pos2.vx, 0, pos2.vy);
            Vector3 velocity = Util.interpolate(velocity1, velocity2, frac);
            velocity = Vector3.Scale(velocity, scale);
            robot.GetComponent<Robot2>().set_linearVelocity(velocity);  

            // Update Robot Angular Velocity
            float angularVelocity1 = pos1.w;
            float angularVelocity2 = pos2.w;
            float angularVelocity = Util.linearInterpolate(angularVelocity1, angularVelocity2, frac);
            robot.GetComponent<Robot2>().set_angularVelocity(new Vector3(0, angularVelocity, 0));   

            // Update Robot Collision Tag ID
            int collisionTagID = pos2.collisionTagID;
            robot.GetComponent<Robot2>().setCollisionTagID(collisionTagID);
            robot.GetComponent<Robot2>().StepCount += 1;
        }

        // Return true only if all agents have reached the end of their trajectories
        return totalAgents > 0 && agentsAtEnd == totalAgents;
    }

}
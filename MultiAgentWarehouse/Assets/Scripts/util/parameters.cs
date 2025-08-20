using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Enumeration;
using System.IO;
using SimpleJSON;
using multiagent;
using multiagent.agent;
namespace multiagent.util
{
    public static class Parameters
    {
        [System.Serializable]
        public class parameters
        {
            public int num_of_agents = 1;
            public int num_spawn_tries = 100;
            public float maxSpeed = 0.7f;
            public float maxRotationSpeed = 2.11f;
            public float maxAcceleration = 2.72f;
            public float maxRotationAccleration = 8.23f;
            public bool velocityControl = true;
            public bool absoluteCoordinate = false;
        }

        static parameters param;

        // Read a config file and update the parameters of the environment
        public static void readConfigFile(string fileName = "config.json")
        {
            string jsonText = "";
            string filepath = fileName;
            if (!fileName.Contains("/") && !fileName.Contains("\\")) // Convert char to string for Contains()
            {
                string assetsPath = Application.streamingAssetsPath;
                string projectPath = Directory.GetParent(assetsPath).FullName;
                filepath = Path.Combine(projectPath, "Scripts", fileName);
            }

            if (File.Exists(filepath))
            {
                jsonText = File.ReadAllText(filepath);
            }
            else
            {
                Debug.Log("file locanum_of_agentstion: " + filepath);
                Debug.LogError("File not found and no defaultJson assigned in the inspector");
            }
            param = JsonUtility.FromJson<parameters>(jsonText);
        }

        public static void setParam(Environment _env)
        {
            _env.num_of_agents = param.num_of_agents;
            _env.num_spawn_tries = param.num_spawn_tries;
            _env.robot.GetComponent<Robot>().maxSpeed = param.maxSpeed;
            _env.robot.GetComponent<Robot>().maxRotationSpeed = param.maxRotationSpeed;
            _env.robot.GetComponent<Robot>().maxAcceleration = param.maxAcceleration;
            _env.robot.GetComponent<Robot>().maxRotationAccleration = param.maxRotationAccleration;
            _env.robot.GetComponent<Robot>().velocityControl = param.velocityControl;
            _env.robot.GetComponent<Robot>().absoluteCoordinate = param.absoluteCoordinate;            
        }
    }
    


}


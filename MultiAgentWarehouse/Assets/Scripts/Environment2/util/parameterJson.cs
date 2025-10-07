using UnityEngine;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;

namespace multiagent.parameterJson
{
    [System.Serializable]
    public class goalParameter
    {
        public float[] goalWait = new float[2] { 5f, 5f };
        public float[] goalWaitProbability = new float[2] { 1f, 1f };
        public float[] goalWaitPenalty = new float[2] { 1f, 1f };

        System.Random random = new System.Random();

        public float sampleGoalWait()
        {
            float x = (float)random.NextDouble();
            float wait = goalWait[0] + x * (goalWait[1] - goalWait[0]);
            return wait;
        }

        public float sampleGoalProb()
        {
            float x = (float)random.NextDouble();
            float waitProb = goalWaitProbability[0] + x * (goalWaitProbability[1] - goalWaitProbability[0]);
            return waitProb;
        }

        public float sampleGoalPenalty()
        {
            float x = (float)random.NextDouble();
            float waitPenalty = goalWaitPenalty[0] + x * (goalWaitPenalty[1] - goalWaitProbability[0]);
            return waitPenalty;
        }

    }

    [System.Serializable]
    public class arrowParameters
    {
        public bool debugArrow = false;
        public bool debugOnlyDirection = false;
        public bool debugOnlySpeed = false;
        public bool debugOnlyAcceleration = false;
        public bool debugOnlyGoal = false;
        public bool debugArrow2DMode = false;
        public float stemLength = 1f;
        public float stemWidth = 0.1f;
        public float tipLength = 0.25f;
        public float tipWidth = 0.2f;
        public float yOffset = 12;
    }

    [System.Serializable]
    public class goalParameters
    {
        public int n_tasks = 1;
        public float task_freq = 1;
        public string task_mode = "";
        public bool verbose = false;
        public bool showRenderer = true;
        public List<goalParameter> goals = new List<goalParameter>();
        public List<goalParameter> starts = new List<goalParameter>();
    }

    [System.Serializable]
    public class rewardParameters
    {
        public float collisionEnterReward = -1f;
        public float collisionStayReward = -0.05f;
        public float timeReward = -2f;
        public float goalReward = 1f;
    }

    [System.Serializable]
    public class lidarParameters
    {
        public float rayLength = 25f;
        public int rayDirections = 1;
        public float sphereCastRadius = 0.5f;
        public float maxRayDegrees = 180f;
    }

    [System.Serializable]
    public class agentsParameters
    {
        public int num_of_agents = 1;
        public float min_spacing = 0.1f;
        public int num_spawn_tries = 100;
        public float maxSpeed = 0.7f;
        public float maxRotationSpeed = 2.11f;
        public float maxAcceleration = 2.72f;
        public float maxRotationAccleration = 8.23f;
        public bool velocityControl = true;
        public bool infiniteAcceleration = true;
        public bool absoluteCoordinate = false;
        public int seed = 42;
        public int maxTimeSteps = 5001;
        public int decisionPeriod = 5;
        public float safetyRadius = 1f;
        public bool verbose = false;
        public int lineRendererMaxPathPositionListCount = -1;
        public float lineRendererMinPathDistance = -1;
        public float lineRendererWidth = 25f;
        public bool allowedlightingOn = false;
        public bool allowedCollisionOn = true;
        public arrowParameters arrowParams = new arrowParameters();
        public rewardParameters rewardParams = new rewardParameters();
        public lidarParameters rayParams = new lidarParameters();
    }

    [System.Serializable]
    public class unityParameters
    {
        public int seed = 42;
        public float timescale = 5f;
        public float fixed_timestep = 0.2f;
        public int num_envs = 1;
        public string dataPath = "";
        public bool useCSVExporter = false;
        public int CSVRate = 1;
        public bool verbose = false;
        public bool useShadow = true;
        public bool normalizeObservations = false;
        public bool useRadian = true;
        public bool showGUI = false;
        public bool useOrthographic = false;
    }

    [System.Serializable]
    public class recordingParameters
    {
        public bool startRecordingOnPlay = false;
        public int screenshotWidth = 1920;
        public int screenshotHeight = 1080;
        public string recordingDir = "Recordings";
        public int actionFrameRate = 5;
        public bool useFullScreenResolution = false;
    }


    [System.Serializable]
    public class parameters
    {
        public unityParameters unityParams = new unityParameters();
        public agentsParameters agentParams = new agentsParameters();
        public goalParameters goalParams = new goalParameters();
        public recordingParameters recordingParams = new recordingParameters();

    }

    public class parameterJson
    {
        public parameters param;
        config conf;

        // Read a config file and update the parameters of the environment
        public void ReadJson(string fileName = "config.json")
        {
            string jsonText = "";
            string filepath = fileName;
            if (File.Exists(filepath))
            {
                jsonText = File.ReadAllText(filepath);
            }
            else
            {
                Debug.Log("File: " + filepath);
                Debug.LogError("Config filepath was not found!!!!");
            }
            conf = JsonUtility.FromJson<config>(jsonText);

            filepath = conf.filepath;
            if (File.Exists(filepath))
            {
                jsonText = File.ReadAllText(filepath);
            }
            else
            {
                Debug.Log("File: " + filepath);
                Debug.LogError("Parameter filepath in the config file was not found!!!");
            }
            param = JsonUtility.FromJson<parameters>(jsonText);
        }

        public parameters GetParameter()
        {
            return param;
        }
    }
}


using UnityEngine;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using multiagent.util;
using UnityEngine.Perception.Randomization.Samplers; 
using Newtonsoft.Json.Linq;

namespace multiagent.parameterJson
{
    [System.Serializable]
    public class CollisionScenario
    {
        public dynamicObstacleParameters dynamicObstacleParams;
        public collisionParameters collisionParams;
    }

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
    public class noiseParameters
    {
        public float position_alpha = 0;
        public float position_beta = 0;
        public float orientation_alpha = 0;
        public float orientation_beta = 0;
        public float velocity_alpha = 0;
        public float velocity_beta = 0;
        public float angular_velocity_alpha = 0;
        public float angular_velocity_beta = 0;
        public float lidar_alpha = 0;
        public float lidar_beta = 0;
        public string class_bias = "everyone";
        public float class_bias_radius = 0;
        public float num_of_agents = 0;

        public NormalSampler position_sampler = new NormalSampler();
        public NormalSampler orientation_sampler = new NormalSampler();
        public NormalSampler velocity_sampler = new NormalSampler();
        public NormalSampler angular_velocity_sampler = new NormalSampler();
        public NormalSampler lidar_sampler = new NormalSampler();

        public noiseParameters()
        {
            position_sampler = new NormalSampler();
            orientation_sampler = new NormalSampler();
            velocity_sampler = new NormalSampler();
            angular_velocity_sampler = new NormalSampler();
            lidar_sampler = new NormalSampler();
            position_sampler.mean = 0f;
            position_sampler.standardDeviation = 1f;
            position_sampler.range = new FloatRange(-3f, 3f);
            orientation_sampler.mean = 0f;
            orientation_sampler.standardDeviation = 1f;
            orientation_sampler.range = new FloatRange(-3f, 3f);
            velocity_sampler.mean = 0f;
            velocity_sampler.standardDeviation = 1f;
            velocity_sampler.range = new FloatRange(-3f, 3f);
            angular_velocity_sampler.mean = 0f;
            angular_velocity_sampler.standardDeviation = 1f;
            angular_velocity_sampler.range = new FloatRange(-3f, 3f);
            lidar_sampler.mean = 0f;
            lidar_sampler.standardDeviation = 1f;
            lidar_sampler.range = new FloatRange(-3f, 3f);
        }




        public List<float> sample_noise(int num_of_position_samples = 2, int num_of_orientation_samples = 1, int num_of_velocity_samples = 1, int num_of_angular_velocity_samples = 1, int num_of_lidar_samples = 1)
        {
            float[] positions_noise = sample_position(num_of_position_samples);
            float[] orientations_noise = sample_orientation(num_of_orientation_samples);
            float[] velocities_noise = sample_velocity(num_of_velocity_samples);
            float[] angular_velocities_noise = sample_angular_velocity(num_of_angular_velocity_samples);
            float[] lidars_noise = sample_lidar(num_of_lidar_samples);
            List<float> noise = new List<float>();
            noise.AddRange(positions_noise);
            noise.AddRange(orientations_noise);
            noise.AddRange(velocities_noise);
            noise.AddRange(angular_velocities_noise);
            noise.AddRange(lidars_noise);
            return noise;
        }
        
        public float[] sample_position(int num_of_samples = 1)
        {
            float[] positions_noise = new float[num_of_samples];
            for (int i = 0; i < num_of_samples; i++)
            {
                positions_noise[i] = position_sampler.Sample() * position_alpha + position_beta;
            }
            return positions_noise;
        }

        public float[] sample_orientation(int num_of_samples = 1)
        {
            float[] orientations_noise = new float[num_of_samples];
            for (int i = 0; i < num_of_samples; i++)
            {
                orientations_noise[i] = orientation_sampler.Sample() * orientation_alpha + orientation_beta;
            }
            return orientations_noise;
        }
        public float[] sample_velocity(int num_of_samples = 1)
        {
            float[] velocities_noise = new float[num_of_samples];
            for (int i = 0; i < num_of_samples; i++)
            {
                velocities_noise[i] = velocity_sampler.Sample() * velocity_alpha + velocity_beta;
            }
            return velocities_noise;
        }

        public float[] sample_angular_velocity(int num_of_samples = 1)
        {
            float[] angular_velocities_noise = new float[num_of_samples];
            for (int i = 0; i < num_of_samples; i++)
            {
                angular_velocities_noise[i] = angular_velocity_sampler.Sample() * angular_velocity_alpha + angular_velocity_beta;
            }
            return angular_velocities_noise;
        }

        public float[] sample_lidar(int num_of_samples = 1)
        {
            float[] lidars_noise = new float[num_of_samples];
            for (int i = 0; i < num_of_samples; i++)
            {
                lidars_noise[i] = lidar_sampler.Sample() * lidar_alpha + lidar_beta;
            }
            return lidars_noise;
        }
    }

    [System.Serializable]
    public class disturbanceParameters : otherCollisionTypeParameters
    {
        public float position_alpha = 0;
        public float position_beta = 0;

        public NormalSampler position_sampler = new NormalSampler();

        public disturbanceParameters()
        {
            position_sampler = new NormalSampler();
            position_sampler.mean = 0f;
            position_sampler.standardDeviation = 1f;
            position_sampler.range = new FloatRange(-3f, 3f);
        }


        public Vector2 sample_disturbance(GameObject gameObject = null)
        {
            
            if (this.class_bias_tag == "everyone")
            {
                Vector2 cartesian_velocity_disturbance = new Vector2();
                cartesian_velocity_disturbance.x = position_sampler.Sample() * position_alpha + position_beta;
                cartesian_velocity_disturbance.y = position_sampler.Sample() * position_alpha + position_beta;
                return cartesian_velocity_disturbance;
            }
            else
            {
                if (gameObject == null)
                {
                    Debug.LogError("GameObject is null when sampling disturbance with class bias.");
                    return Vector2.zero;
                }
                AgentCheckNearbyObjects();
                List<GameObject> nearbyPlayers = Util.GetNearbyGameObjectsByRaycast(gameObject, this.class_bias_radius, 36, -1, this.class_bias_tag);
                GameObject nearbyPlayer = Util.GetNearestGameObject(gameObject, nearbyPlayers);
                Vector3 directionToPlayer = nearbyPlayer.transform.position - gameObject.transform.position;
                Vector2 directionToPlayer2D = new Vector2(directionToPlayer.x, directionToPlayer.z).normalized;
                Debug.Log("Direction to player for disturbance: " + directionToPlayer2D);
                Vector2 cartesian_velocity_disturbance = directionToPlayer2D * (position_sampler.Sample() * position_alpha + position_beta);
                return cartesian_velocity_disturbance;
            }
        }
    }


    [System.Serializable]
    public class otherCollisionTypeParameters
    {
        public float probability = 0;
        public float time_frequency = -1;
        public float[] time_length = new float[2] { 0f, 0f };
        public string class_bias_tag = "everyone";
        public float class_bias_radius = 0;
        public int number_of_affected = 1;
        
        public float last_time = 0;
        public int num_of_agents = 0;
        public List<GameObject> agents = new List<GameObject>();
        public List<List<GameObject>> agentNearbyObjects = new List<List<GameObject>>();
        public List<int> agentNearbyObjectsInd = new List<int>();
        // List<(int, float, float, float)> affected_values = new List<(int, float, float, float)>();
        Dictionary<GameObject, (float,float)> affected_values = new Dictionary<GameObject, (float,float)>();
        public void check_collision_param_time(float t)
        {
            remove_agent_from_affected(t);
            if (t - last_time >= time_frequency  && num_of_agents > 0)
            {
                sample_collision(t);
                last_time = t;
            }

        }

        public void AgentCheckNearbyObjects()
        {
            if (this.class_bias_tag == "everyone")
                return;
            agentNearbyObjects = new List<List<GameObject>>();
            agentNearbyObjectsInd = new List<int>();
            foreach (GameObject agent in agents)
            {
                List<GameObject> nearbyObjs = Util.GetNearbyGameObjects(agent, this.class_bias_radius, -1, this.class_bias_tag);
                if(nearbyObjs.Contains(agent))
                {
                    nearbyObjs.Remove(agent);
                }
                if(nearbyObjs.Count > 0)
                {
                    agentNearbyObjects.Add(nearbyObjs);    
                    agentNearbyObjectsInd.Add(agents.IndexOf(agent));
                }
            }
        }

        public void remove_agent_from_affected(float t)
        {
            List<GameObject> agents_to_remove = new List<GameObject>();
            foreach (var kvp in affected_values)
            {
                GameObject agent = kvp.Key;
                (float agent_last_time, float agent_time_length) = kvp.Value;
                if (t >= agent_last_time + agent_time_length)
                {
                    agents_to_remove.Add(agent);
                }
            }
            foreach (GameObject agent in agents_to_remove)
            {
                affected_values.Remove(agent);
            }
        }

        public void sample_collision(float t)
        {
            // Get the affected agents
            List<GameObject> affected_agents = new List<GameObject>();
            List<int> affected_agents_ind = new List<int>();
            int num_of_affected = Util.sample_num_uniformly(this.number_of_affected, this.probability);

            if (this.class_bias_tag == "everyone")
            {
                affected_agents_ind = Util.sample_affected_agents_efficient(num_of_affected, this.num_of_agents);
                foreach (int ind in affected_agents_ind)
                {
                    GameObject agent = agents[ind];
                    if (affected_values.ContainsKey(agent) == true)
                    {
                        (float agent_last_time, float agent_time_length) = affected_values[agent];
                        if (t < agent_last_time + agent_time_length)
                        {
                            continue;
                        }
                        affected_values.Remove(agent);
                    }
                    affected_agents.Add(agents[ind]);
                }
            }
            else
            {
                AgentCheckNearbyObjects();
                affected_agents_ind = Util.sample_affected_agents_efficient(num_of_affected, this.agentNearbyObjects.Count);
                foreach (int ind in affected_agents_ind)
                {
                    GameObject agent = agents[agentNearbyObjectsInd[ind]];
                    if (affected_values.ContainsKey(agent) == true)
                    {
                        (float agent_last_time, float agent_time_length) = affected_values[agent];
                        if (t < agent_last_time + agent_time_length)
                        {
                            continue;
                        }
                        affected_values.Remove(agent);
                    }
                    affected_agents.Add(agents[agentNearbyObjectsInd[ind]]);
                }
            }


            // Get the affected time & direction for each affected agent
            List<float> affected_time = Util.sample_float_list(affected_agents.Count, this.time_length);
            for (int i = 0; i < affected_agents.Count; i++)
            {
                affected_values[affected_agents[i]] = (t, affected_time[i]);
                Debug.Log("Agent " + affected_agents[i].name + " is affected for " + affected_time[i] + " seconds.");
            }
        }

        public Dictionary<GameObject, (float,float)> get_affected_agents()
        {
            return affected_values;
        }
        
        public void set_agents(List<GameObject> agents )
        {
            this.agents = agents;
            this.num_of_agents = agents.Count;
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
    public class dynamicsParameters
    {
        public float bodyMass = 50.0f;
        public float linearDrag = 0.0f;
        public float angularDrag = 0.0f;
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
        public string controllerType = "velocity";
        public bool infiniteVelcoity = true;
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
        public dynamicsParameters dynamicsParams = new dynamicsParameters();
        public arrowParameters arrowParams = new arrowParameters();
        public rewardParameters rewardParams = new rewardParameters();
        public lidarParameters rayParams = new lidarParameters();
    }

    [System.Serializable]
    public class dynamicObstacleParameters
    {
        public int num_of_dyn_obs = 1;
        public float min_spacing = 0.1f;
        public int num_spawn_tries = 100;
        public float maxSpeed = 0.7f;
        public float maxRotationSpeed = 2.11f;
        public float maxAcceleration = 2.72f;
        public float maxRotationAccleration = 8.23f;
        public bool infiniteAcceleration = true;
        public bool absoluteCoordinate = false;
        public bool verbose = false;
        public int lineRendererMaxPathPositionListCount = -1;
        public float lineRendererMinPathDistance = -1;
        public float lineRendererWidth = 25f;
        public bool allowedCollisionOn = true;
        public string movement_type = "random_walk";
        public float movement_delay = 2f;
        public dynamicsParameters dynamicsParams = new dynamicsParameters();
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
        public bool endlessMode = false;
    }

    [System.Serializable]
    public class collisionParameters
    {
        public noiseParameters noise = new noiseParameters();
        public disturbanceParameters disturbance = new disturbanceParameters();
        public otherCollisionTypeParameters delay = new otherCollisionTypeParameters();
        public otherCollisionTypeParameters lidar_malfunctioning = new otherCollisionTypeParameters();
        public otherCollisionTypeParameters adversary_dynamic_obstacle = new otherCollisionTypeParameters();
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
        public dynamicObstacleParameters dynamicObstacleParams = new dynamicObstacleParameters();
        public collisionParameters collisionParams = new collisionParameters();
    }

    public class parameterJson
    {
        public parameters param;
        Config conf;

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
            conf = JsonUtility.FromJson<Config>(jsonText);

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
            ReadCollisionJsonFromConfig();
        }

        public parameters GetParameter()
        {
            return param;
        }

        // Read a collision scenario JSON from conf.collisionpath and merge into current parameters
        public void ReadCollisionJsonFromConfig()
        {
            if (conf == null)
            {
                Debug.LogError("Config not loaded. Call ReadJson() first before applying collision scenario.");
                return;
            }
            if (string.IsNullOrEmpty(conf.collisionpath))
            {
                Debug.LogWarning("Config.collisionpath is empty. Skipping collision scenario load.");
                return;
            }
            string filepath = conf.collisionpath;
            if (!File.Exists(filepath))
            {
                Debug.LogError("Collision scenario file not found at: " + filepath);
                return;
            }
            string jsonText = File.ReadAllText(filepath);
            CollisionScenario scenario = null;
            try
            {
                scenario = JsonUtility.FromJson<CollisionScenario>(jsonText);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to parse collision scenario JSON: " + ex.Message);
                return;
            }
            // Best-effort: patch known key typos/mismatches from raw JSON
            try
            {
                JObject root = JObject.Parse(jsonText);
                var cp = root["collisionParams"] as JObject;
                if (cp != null)
                {
                    var noise = cp["noise"] as JObject;
                    if (noise != null)
                    {
                        // Handle "positin_beta" -> "position_beta"
                        if (noise["positin_beta"] != null)
                        {
                            float posBeta;
                            if (float.TryParse(noise["positin_beta"].ToString(), out posBeta))
                            {
                                if (scenario.collisionParams == null) scenario.collisionParams = new collisionParameters();
                                if (scenario.collisionParams.noise == null) scenario.collisionParams.noise = new noiseParameters();
                                scenario.collisionParams.noise.position_beta = posBeta;
                            }
                        }
                        // Handle "class_bias_tag" in noise -> map to class_bias
                        if (noise["class_bias_tag"] != null)
                        {
                            string cls = noise["class_bias_tag"].ToString();
                            if (scenario.collisionParams == null) scenario.collisionParams = new collisionParameters();
                            if (scenario.collisionParams.noise == null) scenario.collisionParams.noise = new noiseParameters();
                            scenario.collisionParams.noise.class_bias = cls;
                        }
                    }
                }
            }
            catch { /* ignore best-effort patches */ }
            if (scenario == null)
            {
                Debug.LogError("Parsed collision scenario is null.");
                return;
            }
            if (param == null)
            {
                param = new parameters();
            }
            // Merge only differing fields from scenario into current params
            if (scenario.dynamicObstacleParams != null)
            {
                if (param.dynamicObstacleParams == null) param.dynamicObstacleParams = new dynamicObstacleParameters();
                MergeDynamicObstacleParams(param.dynamicObstacleParams, scenario.dynamicObstacleParams);
            }
            if (scenario.collisionParams != null)
            {
                if (param.collisionParams == null) param.collisionParams = new collisionParameters();
                MergeCollisionParams(param.collisionParams, scenario.collisionParams);
            }
            Debug.Log("Collision Scenario from: " + filepath);
        }

        // Merge helpers: copy only differing values from src -> dst
        static bool Diff(float a, float b, float eps = 1e-6f) { return Mathf.Abs(a - b) > eps; }
        static bool Diff(int a, int b) { return a != b; }
        static bool Diff(bool a, bool b) { return a != b; }
        static bool Diff(string a, string b) { return a != b; }

        void MergeFloatArray(ref float[] dst, float[] src)
        {
            if (src == null) return;
            if (dst == null || dst.Length != src.Length)
            {
                dst = src;
                return;
            }
            for (int i = 0; i < src.Length; i++)
            {
                if (Diff(dst[i], src[i])) dst[i] = src[i];
            }
        }

        void MergeNoise(noiseParameters dst, noiseParameters src)
        {
            if (src == null || dst == null) return;
            if (Diff(dst.position_alpha, src.position_alpha)) dst.position_alpha = src.position_alpha;
            if (Diff(dst.position_beta, src.position_beta)) dst.position_beta = src.position_beta;
            if (Diff(dst.orientation_alpha, src.orientation_alpha)) dst.orientation_alpha = src.orientation_alpha;
            if (Diff(dst.orientation_beta, src.orientation_beta)) dst.orientation_beta = src.orientation_beta;
            if (Diff(dst.velocity_alpha, src.velocity_alpha)) dst.velocity_alpha = src.velocity_alpha;
            if (Diff(dst.velocity_beta, src.velocity_beta)) dst.velocity_beta = src.velocity_beta;
            if (Diff(dst.angular_velocity_alpha, src.angular_velocity_alpha)) dst.angular_velocity_alpha = src.angular_velocity_alpha;
            if (Diff(dst.angular_velocity_beta, src.angular_velocity_beta)) dst.angular_velocity_beta = src.angular_velocity_beta;
            if (Diff(dst.lidar_alpha, src.lidar_alpha)) dst.lidar_alpha = src.lidar_alpha;
            if (Diff(dst.lidar_beta, src.lidar_beta)) dst.lidar_beta = src.lidar_beta;
            if (Diff(dst.class_bias, src.class_bias)) dst.class_bias = src.class_bias;
            if (Diff(dst.class_bias_radius, src.class_bias_radius)) dst.class_bias_radius = src.class_bias_radius;
            if (Diff(dst.num_of_agents, src.num_of_agents)) dst.num_of_agents = src.num_of_agents;
        }

        void MergeOtherCollision(otherCollisionTypeParameters dst, otherCollisionTypeParameters src)
        {
            if (src == null || dst == null) return;
            if (Diff(dst.probability, src.probability)) dst.probability = src.probability;
            if (Diff(dst.time_frequency, src.time_frequency)) dst.time_frequency = src.time_frequency;
            MergeFloatArray(ref dst.time_length, src.time_length);
            if (Diff(dst.class_bias_tag, src.class_bias_tag)) dst.class_bias_tag = src.class_bias_tag;
            if (Diff(dst.class_bias_radius, src.class_bias_radius)) dst.class_bias_radius = src.class_bias_radius;
            if (Diff(dst.number_of_affected, src.number_of_affected)) dst.number_of_affected = src.number_of_affected;
        }

        void MergeDisturbance(disturbanceParameters dst, disturbanceParameters src)
        {
            if (src == null || dst == null) return;
            MergeOtherCollision(dst, src);
            if (Diff(dst.position_alpha, src.position_alpha)) dst.position_alpha = src.position_alpha;
            if (Diff(dst.position_beta, src.position_beta)) dst.position_beta = src.position_beta;
        }

        void MergeCollisionParams(collisionParameters dst, collisionParameters src)
        {
            if (src.noise != null)
            {
                if (dst.noise == null) dst.noise = new noiseParameters();
                MergeNoise(dst.noise, src.noise);
            }
            if (src.disturbance != null)
            {
                if (dst.disturbance == null) dst.disturbance = new disturbanceParameters();
                MergeDisturbance(dst.disturbance, src.disturbance);
            }
            if (src.delay != null)
            {
                if (dst.delay == null) dst.delay = new otherCollisionTypeParameters();
                MergeOtherCollision(dst.delay, src.delay);
            }
            if (src.lidar_malfunctioning != null)
            {
                if (dst.lidar_malfunctioning == null) dst.lidar_malfunctioning = new otherCollisionTypeParameters();
                MergeOtherCollision(dst.lidar_malfunctioning, src.lidar_malfunctioning);
            }
            if (src.adversary_dynamic_obstacle != null)
            {
                if (dst.adversary_dynamic_obstacle == null) dst.adversary_dynamic_obstacle = new otherCollisionTypeParameters();
                MergeOtherCollision(dst.adversary_dynamic_obstacle, src.adversary_dynamic_obstacle);
            }
        }

        void MergeDynamicObstacleParams(dynamicObstacleParameters dst, dynamicObstacleParameters src)
        {
            if (src == null || dst == null) return;
            if (Diff(dst.num_of_dyn_obs, src.num_of_dyn_obs)) dst.num_of_dyn_obs = src.num_of_dyn_obs;
            if (Diff(dst.min_spacing, src.min_spacing)) dst.min_spacing = src.min_spacing;
            if (Diff(dst.num_spawn_tries, src.num_spawn_tries)) dst.num_spawn_tries = src.num_spawn_tries;
            if (Diff(dst.maxSpeed, src.maxSpeed)) dst.maxSpeed = src.maxSpeed;
            if (Diff(dst.maxRotationSpeed, src.maxRotationSpeed)) dst.maxRotationSpeed = src.maxRotationSpeed;
            if (Diff(dst.maxAcceleration, src.maxAcceleration)) dst.maxAcceleration = src.maxAcceleration;
            if (Diff(dst.maxRotationAccleration, src.maxRotationAccleration)) dst.maxRotationAccleration = src.maxRotationAccleration;
            if (Diff(dst.infiniteAcceleration, src.infiniteAcceleration)) dst.infiniteAcceleration = src.infiniteAcceleration;
            if (Diff(dst.absoluteCoordinate, src.absoluteCoordinate)) dst.absoluteCoordinate = src.absoluteCoordinate;
            if (Diff(dst.verbose, src.verbose)) dst.verbose = src.verbose;
            if (Diff(dst.lineRendererMaxPathPositionListCount, src.lineRendererMaxPathPositionListCount)) dst.lineRendererMaxPathPositionListCount = src.lineRendererMaxPathPositionListCount;
            if (Diff(dst.lineRendererMinPathDistance, src.lineRendererMinPathDistance)) dst.lineRendererMinPathDistance = src.lineRendererMinPathDistance;
            if (Diff(dst.lineRendererWidth, src.lineRendererWidth)) dst.lineRendererWidth = src.lineRendererWidth;
            if (Diff(dst.allowedCollisionOn, src.allowedCollisionOn)) dst.allowedCollisionOn = src.allowedCollisionOn;
            if (Diff(dst.movement_type, src.movement_type)) dst.movement_type = src.movement_type;
            if (Diff(dst.movement_delay, src.movement_delay)) dst.movement_delay = src.movement_delay;
            if (src.dynamicsParams != null)
            {
                if (dst.dynamicsParams == null) dst.dynamicsParams = new dynamicsParameters();
                if (Diff(dst.dynamicsParams.bodyMass, src.dynamicsParams.bodyMass)) dst.dynamicsParams.bodyMass = src.dynamicsParams.bodyMass;
                if (Diff(dst.dynamicsParams.linearDrag, src.dynamicsParams.linearDrag)) dst.dynamicsParams.linearDrag = src.dynamicsParams.linearDrag;
                if (Diff(dst.dynamicsParams.angularDrag, src.dynamicsParams.angularDrag)) dst.dynamicsParams.angularDrag = src.dynamicsParams.angularDrag;
            }
        }
    }
}


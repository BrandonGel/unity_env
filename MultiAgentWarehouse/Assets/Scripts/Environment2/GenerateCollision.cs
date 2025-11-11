
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using multiagent.util;
using multiagent.parameterJson;
using multiagent.robot;
using multiagent.dynamic_obstacle;
using JetBrains.Annotations;

namespace multiagent.collisionGeneratorSpace
{
    public class collisionGenerator
    {
        public collisionParameters collisionParams = new collisionParameters();
        public noiseParameters noise = new noiseParameters();
        public disturbanceParameters disturbance = new disturbanceParameters();
        public otherCollisionTypeParameters delay = new otherCollisionTypeParameters();
        public otherCollisionTypeParameters lidar_malfunctioning = new otherCollisionTypeParameters();
        public otherCollisionTypeParameters adversary_dynamic_obstacle = new otherCollisionTypeParameters();
        public List<GameObject> robotsObjList = new List<GameObject>();
        public List<GameObject> dynamicObstaclesObjList = new List<GameObject>();

        public collisionGenerator(collisionParameters collisionParams = default)
        {
            if (collisionParams != default)
            {
                this.collisionParams =  collisionParams;
            }
            noise = this.collisionParams.noise;
            disturbance = this.collisionParams.disturbance;
            delay = this.collisionParams.delay;
            lidar_malfunctioning = this.collisionParams.lidar_malfunctioning;
            adversary_dynamic_obstacle = this.collisionParams.adversary_dynamic_obstacle;
            
        }

        public void set_collision_params(collisionParameters collisionParams, List<GameObject> robotsObjList, List<GameObject> dynamicObstaclesObjList)
        {
            int num_of_agents = robotsObjList.Count;
            int num_of_dynamic_obstacles = dynamicObstaclesObjList.Count;
            this.robotsObjList = robotsObjList;
            this.dynamicObstaclesObjList = dynamicObstaclesObjList;
            this.collisionParams = collisionParams;
            this.collisionParams.noise.num_of_agents = num_of_agents;
            this.collisionParams.disturbance.set_agents(robotsObjList);
            this.collisionParams.delay.set_agents(robotsObjList);
            this.collisionParams.lidar_malfunctioning.set_agents(robotsObjList);
            this.collisionParams.adversary_dynamic_obstacle.set_agents(dynamicObstaclesObjList);
            noise = this.collisionParams.noise;
            disturbance = this.collisionParams.disturbance;
            delay = this.collisionParams.delay;
            lidar_malfunctioning = this.collisionParams.lidar_malfunctioning;
            adversary_dynamic_obstacle = this.collisionParams.adversary_dynamic_obstacle;
        }

        public void check_collision_param_time(float t)
        {
            disturbance.check_collision_param_time(t);
            delay.check_collision_param_time(t);
            lidar_malfunctioning.check_collision_param_time(t);
            adversary_dynamic_obstacle.check_collision_param_time(t);

            Dictionary<GameObject, (float,float)> disturbance_affected_values = disturbance.get_affected_agents();
            Dictionary<GameObject, (float,float)> delay_affected_values = delay.get_affected_agents();
            Dictionary<GameObject, (float,float)> lidar_malfunctioning_affected_values = lidar_malfunctioning.get_affected_agents();
            Dictionary<GameObject, (float,float)> adversary_dynamic_obstacle_affected_values = adversary_dynamic_obstacle.get_affected_agents();

            foreach (var agent in robotsObjList)
            {
                Robot2 robotObj = agent.GetComponent<Robot2>();
                robotObj.clear_collisionEffectset();

                List<float> noise_effect_in = noise.sample_noise(2, 1, 1, 1, robotObj.num_lidar_rays);
                robotObj.set_collisionEffectset(noise_effect_in: noise_effect_in);
            }

            foreach (var dynamic_obstacle in dynamicObstaclesObjList)
            {
                human_operator humanOperatorObj = dynamic_obstacle.GetComponent<human_operator>();
                humanOperatorObj.adversary_mode = false;
                humanOperatorObj.adversary_radius = 0f;
                humanOperatorObj.adversary_target_tag = "Everyone";
            }

            foreach (var kvp in disturbance_affected_values)
            {
                GameObject agent = kvp.Key;
                Robot2 robotObj = agent.GetComponent<Robot2>();
                Vector2 disturbance_effect = disturbance.sample_disturbance(agent);
                robotObj.set_collisionEffectset(disturbance_effect_in: disturbance_effect);
            }

            foreach (var kvp in delay_affected_values)
            {
                GameObject agent = kvp.Key;
                Robot2 robotObj = agent.GetComponent<Robot2>();
                robotObj.set_collisionEffectset(delay_effect_in: true);
            }

            foreach (var kvp in lidar_malfunctioning_affected_values)
            {
                GameObject agent = kvp.Key;
                Robot2 robotObj = agent.GetComponent<Robot2>();
                robotObj.set_collisionEffectset(lidar_malfunctioning_effect_in: true);
            }

            foreach (var kvp in adversary_dynamic_obstacle_affected_values)
            {
                GameObject dynamic_obstacle = kvp.Key;
                human_operator humanOperatorObj = dynamic_obstacle.GetComponent<human_operator>();
                humanOperatorObj.adversary_mode = true;
                humanOperatorObj.adversary_radius = adversary_dynamic_obstacle.class_bias_radius;
                humanOperatorObj.adversary_target_tag = adversary_dynamic_obstacle.class_bias_tag;
            }
        }
    }
}

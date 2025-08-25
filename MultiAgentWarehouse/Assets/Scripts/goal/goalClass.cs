

using System.Collections.Generic;
using UnityEngine;
using multiagent.agent;
using multiagent.util;
using static multiagent.util.Parameters;
using Unity.VisualScripting;
namespace multiagent.goal
{
    public class goalClass
    {
        public goalParameters goalParams;
        public Transform transform;
        private int num_of_goals = 1;
        private float task_frequency = 1;
        private int num_of_goals_started = 0;
        private int num_of_goals_completed = 0;

        public Dictionary<string, List<Goal[]>> goals = new Dictionary<string, List<Goal[]>>();

        public void initialize(Transform transform, goalParameters goalParams)
        {
            this.transform = transform;
            this.goalParams = goalParams;
            InitGoals();
        }


        public void InitGoals()
        {
            num_of_goals_started = 0;
            num_of_goals_completed = 0;
            int ii = 0;
            goals = new Dictionary<string, List<Goal[]>>();

            // Pickups
            goals.Add("Pickups", new List<Goal[]>());
            ii = 0;
            Transform childTransform;
            foreach (Transform child in transform.Find("Pickups"))
            {
                Goal[] pickup = new Goal[2];

                // Drop Palette (Step 4)
                childTransform = child.transform.Find("Drop Palette");
                Goal paletteDropOff = childTransform.GetComponent<Goal>();
                paletteDropOff.goalID = ii;
                paletteDropOff.goalType = 4;
                paletteDropOff.goalWait = goalParams.dropPalette.sampleGoalWait();
                paletteDropOff.goalWaitProbability = goalParams.dropPalette.sampleGoalProb();
                paletteDropOff.goalDelayPenalty = goalParams.dropPalette.sampleGoalPenalty();
                paletteDropOff.resetAll();

                // Get Battery (Step 1)
                childTransform = child.transform.Find("Get Battery");
                Goal batteryPickUp = childTransform.GetComponent<Goal>();
                batteryPickUp.goalID = ii;
                batteryPickUp.goalType = 1;
                batteryPickUp.goalWait = goalParams.getbattery.sampleGoalWait();
                batteryPickUp.goalWaitProbability = goalParams.getbattery.sampleGoalProb();
                batteryPickUp.goalDelayPenalty = goalParams.getbattery.sampleGoalPenalty();
                batteryPickUp.resetAll();

                pickup[0] = paletteDropOff;
                pickup[1] = batteryPickUp;
                goals["Pickups"].Add(pickup);
                ii += 1;
            }

            // Dropouts
            goals.Add("Dropoff", new List<Goal[]>());
            ii = 0;
            foreach (Transform child in transform.Find("Dropoffs"))
            {
                Goal[] dropoff = new Goal[2];

                // Pickup Palette (Step 3)
                childTransform = child.transform.Find("Get Palette");
                Goal palettePickUp = childTransform.GetComponent<Goal>();
                palettePickUp.goalID = ii;
                palettePickUp.goalType = 3;
                palettePickUp.goalWait = goalParams.getPalette.sampleGoalWait();
                palettePickUp.goalWaitProbability = goalParams.getPalette.sampleGoalProb();
                palettePickUp.goalDelayPenalty = goalParams.getPalette.sampleGoalPenalty();
                palettePickUp.resetAll();

                // Drop Battery (Step 2)
                childTransform = child.transform.Find("Drop Battery");
                Goal batteryDropOff = childTransform.GetComponent<Goal>();
                batteryDropOff.goalID = ii;
                batteryDropOff.goalType = 2;
                batteryDropOff.goalWait = goalParams.dropBattery.sampleGoalWait();
                batteryDropOff.goalWaitProbability = goalParams.dropBattery.sampleGoalProb();
                batteryDropOff.goalDelayPenalty = goalParams.dropBattery.sampleGoalPenalty();
                batteryDropOff.resetAll();

                dropoff[0] = palettePickUp;
                dropoff[1] = batteryDropOff;
                goals["Dropoff"].Add(dropoff);
                ii += 1;
            }
        }

        public void AssignGoals(int i, GameObject robot, bool reset = false)
        {
            int numberDropoffs = transform.Find("Dropoffs").childCount;
            int numberPickUps = transform.Find("Pickups").childCount;

            Robot robotComponent = robot.GetComponent<Robot>();
            Goal _goal = robotComponent.getGoal();

            int goalID = -1;
            int goalType = 0;
            if (robot.GetComponent<Robot>()._goalClass != null && !reset)
            {
                goalID = robot.GetComponent<Robot>()._goalClass.goalID;
                goalType = _goal.goalType;
            }

            // Prevent assigning new goals if the total number of goals have been started
            if (num_of_goals_started >= num_of_goals && goalType == 0 )
            {
                robotComponent.setGoal(null);
                return;
            }
            switch (goalType)
            {
                case 0:
                    goalID = Random.Range(0, numberPickUps);
                    _goal = goals["Pickups"][goalID][1];
                    num_of_goals_started += 1;
                    break;
                case 1: // Step 1 -> Step 2: Drop Battery & Palette
                    goalID = Random.Range(0, numberDropoffs);
                    _goal = goals["Dropoff"][goalID][1];
                    break;
                case 2: // Step 2 -> Step 3: Get Palette 
                    _goal = goals["Dropoff"][goalID][0];
                    break;
                case 3: // Step 3 -> Step 4: Drop Palette
                    goalID = Random.Range(0, numberPickUps);
                    _goal = goals["Pickups"][goalID][0];
                    break;
                case 4: // Step 4 -> Step 1: Get Battery
                    _goal = goals["Pickups"][goalID][1];
                    num_of_goals_started += 1;
                    num_of_goals_completed += 1;
                    break;
                default:
                    break;
            }

            // Prevent assigning new goals if the total number of goals have been completed
            if (num_of_goals_started >= num_of_goals &&  goalType == 4)
            {
                robotComponent.setGoal(null);
                return;
            }

            
            _goal.assigned(i);
            robotComponent.setGoal(_goal);

        }
        
        public bool allGoalsCompleted()
        {
            return num_of_goals_completed >= num_of_goals;
        }

    }

}

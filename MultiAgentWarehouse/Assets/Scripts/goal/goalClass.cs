

using System.Collections.Generic;
using UnityEngine;
using multiagent.agent;
using multiagent.util;
using static multiagent.util.Parameters;
namespace multiagent.goal
{
    public class goalClass
    {
        public goalParameters goalParams;
        public Transform transform;

        public Dictionary<string, List<Goal[]>> goals = new Dictionary<string, List<Goal[]>>();

        public void initialize(Transform transform, goalParameters goalParams)
        {
            this.transform = transform;
            this.goalParams = goalParams;
            InitGoals();
        }


        public void InitGoals()
        {
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
                Debug.Log(ii + "1: " + paletteDropOff.goalWait + " " + batteryPickUp.goalWait);
                Debug.Log(ii + "2: " + paletteDropOff.goalWaitProbability + " " + batteryPickUp.goalWaitProbability);
                Debug.Log(ii + "3: " + paletteDropOff.goalDelayPenalty + " " + batteryPickUp.goalDelayPenalty);
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
                Debug.Log(ii + "4: " + palettePickUp.goalWait + " " + batteryDropOff.goalWait);
                Debug.Log(ii + "5: " + palettePickUp.goalWaitProbability + " " + batteryDropOff.goalWaitProbability);
                Debug.Log(ii + "6: " + palettePickUp.goalDelayPenalty + " " + batteryDropOff.goalDelayPenalty);
            }
        }

        public void AssignGoals(int i, GameObject robot, bool reset =false)
        {
            int numberDropoffs = transform.Find("Dropoffs").childCount;
            int numberPickUps = transform.Find("Pickups").childCount;

            Robot robotComponent = robot.GetComponent<Robot>();
            Goal _goal = robotComponent.getGoal();
        
            int goalID = -1;
            int goalType = 0;
            if (robot.GetComponent<Robot>()._goalClass != null)
            {
                goalID = robot.GetComponent<Robot>()._goalClass.goalID;
                goalType = _goal.goalType;
            }
            switch (goalType)
            {
                case 0:
                    goalID = Random.Range(0, numberPickUps);
                    _goal = goals["Pickups"][goalID][1];
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
                    break;
                default:
                    break;
            }
            _goal.assigned(i);
            robotComponent.setGoal(_goal);

        }

    }

}

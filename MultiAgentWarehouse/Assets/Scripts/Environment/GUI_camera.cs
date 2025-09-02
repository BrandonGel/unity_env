using UnityEngine;
using multiagent.camera;
namespace multiagent
{
    public class GUI_camera : MonoBehaviour
    {
        [SerializeField] private Environment _env;
        [SerializeField] private Camera_Follow _camera;

        private GUIStyle _defaultStyle = new GUIStyle();
        private GUIStyle _positivieStyle = new GUIStyle();
        private GUIStyle _negativeStyle = new GUIStyle();


        [Range(0,360)][SerializeField] public float yawAngle = 0; // rotate y axis
        [Range(0,360)][SerializeField] public float pitchAngle = 0; // rotate x axis
        [SerializeField] public float offsetDistance;
        [SerializeField] public int playerIndex = 0;
        Vector3 directionVector;
        Vector3 newpos;
        public GameObject[] players = null;



        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _defaultStyle.fontSize = 50;
            _defaultStyle.normal.textColor = Color.yellow;

            _positivieStyle.fontSize = 50;
            _positivieStyle.normal.textColor = Color.green;

            _negativeStyle.fontSize = 50;
            _negativeStyle.normal.textColor = Color.red;

            
        }

        private void OnGUI()
        {
            string debugEpisode = "{Episode}: " + _env.CurrentEpisode + " - Step: " + _env.StepCount;


            // string debugRobot = "Looking at Robot: " + _camera.playerIndex;
            // string debugReward = "Reward: " + _env.CumulativeReward.ToString();

            // GUIStyle rewardStyle = _robot.CumulativeReward < 0 ? _negativeStyle : _positivieStyle;


            GUI.Label(new Rect(20, 20, 500, 30), debugEpisode, _defaultStyle);
            // GUI.Label(new Rect(20, 60, 500, 30), debugRobot, _defaultStyle);
            
        }



        // Update is called once per frame
        void Update()
        {
            if (players == null)
            {
                players = _env.robots;
            }
        }
    }
}
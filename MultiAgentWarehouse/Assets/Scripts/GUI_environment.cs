using UnityEngine;
using multiagent.camera;

namespace multiagent
{
    public class GUI_environment : MonoBehaviour
    {
        [SerializeField] private Environment _env;
        [SerializeField] private GameObject _camera;
        [SerializeField] private RegisterStringLogSideChannel _register;
        Vector3 envCameraPos;
        Quaternion envCameraRot;
        private GUIStyle _defaultStyle = new GUIStyle();
        private GUIStyle _positivieStyle = new GUIStyle();
        private GUIStyle _negativeStyle = new GUIStyle();
        

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _defaultStyle.fontSize = 50;
            _defaultStyle.normal.textColor = Color.yellow;

            _positivieStyle.fontSize = 50;
            _positivieStyle.normal.textColor = Color.green;

            _negativeStyle.fontSize = 50;
            _negativeStyle.normal.textColor = Color.red;

            envCameraPos = _camera.transform.position;
            envCameraRot = _camera.transform.rotation;

        }

        private void OnGUI()
        {
            string debugEpisode = "Episode: " + _env.CurrentEpisode + " - Step: " + _env.StepCount;

            string debugPlayers = "Players: ";
            if (_env.robots != null)
            {
                debugPlayers += _env.robots.Length;
            }
            // string debugMessage = "Message: " + _register.getIncomingMsg();
            // string debugReward = "Reward: " + _env.CumulativeReward.ToString();

            // GUIStyle rewardStyle = _robot.CumulativeReward < 0 ? _negativeStyle : _positivieStyle;


            GUI.Label(new Rect(20, 20, 500, 30), debugEpisode, _defaultStyle);
            GUI.Label(new Rect(20, 60, 500, 30), debugPlayers, _defaultStyle);
            // GUI.Label(new Rect(20, 100, 500, 30), debugMessage, _defaultStyle);
            // GUI.Label(new Rect(20, 60 , 500, 30), debugReward, rewardStyle);
        }

        public void clickEnvCamera()
        {
            _camera.transform.position = envCameraPos;
            _camera.transform.rotation = envCameraRot;
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
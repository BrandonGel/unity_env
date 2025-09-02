using UnityEngine;
using multiagent.camera;


public class GUI_environment2 : MonoBehaviour
{
    [SerializeField] private Environment2 _env;
    [SerializeField] private GameObject _camera;
    [SerializeField] private GameObject _camera2;
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
        // // Episode & Step Count information
        // string debugEpisode = "Episode: " + _env.CurrentEpisode + " - Step: " + _env.StepCount;

        // //Number of Players information
        // string debugPlayers = "Players: ";
        // if (_env.robots != null)
        // {
        //     debugPlayers += _env.robots.Length;
        // }
        
        
        // // Camera GUI & Reward Information
        // bool mainCameraDisplayed = _camera.GetComponent<Camera>().enabled;
        // bool playerCameraDisplayed = _camera2.GetComponent<Camera>().enabled;
        // string debugDisplay = "";
        // float reward = 0;
        // string debugReward = "Reward: ";
        // GUIStyle rewardStyle = _env.CumulativeMeanReward < 0 ? _negativeStyle : _positivieStyle;
        // if (mainCameraDisplayed)
        // {
        //     debugDisplay += "Main Display";
        //     reward = _env.CumulativeMeanReward;
        // }
        // else if (playerCameraDisplayed)
        // {
        //     int robotID = _camera2.GetComponent<Camera_Follow>().playerIndex;
        //     debugDisplay += "Robot " + robotID.ToString() + " View";
        //     reward += _env.getReward(robotID);
        // }
        // reward = Mathf.Round(reward*10000)/10000;
        // debugReward += reward.ToString();   

        // // Message Information
        // // string debugMessage = "Message: " + _register.getIncomingMsg();

        // GUI.Label(new Rect(20, 20, 500, 30), debugEpisode, _defaultStyle);
        // GUI.Label(new Rect(20, 60, 500, 30), debugPlayers, _defaultStyle);
        // GUI.Label(new Rect(20, 100, 500, 30), debugDisplay, _defaultStyle);
        // GUI.Label(new Rect(20, 140 , 500, 30), debugReward, rewardStyle);
        // // GUI.Label(new Rect(20, 100, 500, 30), debugMessage, _defaultStyle);
        
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

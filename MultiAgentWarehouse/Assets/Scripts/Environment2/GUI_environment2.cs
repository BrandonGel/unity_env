using UnityEngine;
using multiagent.camera;
using System.Collections.Generic;
using Unity.VisualScripting;


public class GUI_environment2 : MonoBehaviour
{
    [SerializeField] private GameObject _envHead;
    [SerializeField] private Camera _mainCamera;
    private List<Camera> _envCameras = new List<Camera>();
    private List<Environment2Agent> _envAgents = new List<Environment2Agent>();
    float fov = 60f;
    Vector3 envCameraPos;
    Quaternion envCameraRot;
    private GUIStyle _defaultStyle = new GUIStyle();
    private GUIStyle _positivieStyle = new GUIStyle();
    private GUIStyle _negativeStyle = new GUIStyle();
    public int envID = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _defaultStyle.fontSize = 50;
        _defaultStyle.normal.textColor = Color.yellow;

        _positivieStyle.fontSize = 50;
        _positivieStyle.normal.textColor = Color.green;

        _negativeStyle.fontSize = 50;
        _negativeStyle.normal.textColor = Color.red;

        envCameraPos = _mainCamera.transform.position;
        envCameraRot = _mainCamera.transform.rotation;

        _mainCamera.enabled = true;
        foreach (Transform child in _envHead.transform)
        {
            if (child.name.Contains("Environment"))
            {
                child.gameObject.transform.Find("Camera").GetComponent<Camera>().enabled = false;
                _envCameras.Add(child.gameObject.transform.Find("Camera").GetComponent<Camera>());
                _envAgents.Add(child.gameObject.GetComponent<Environment2Agent>());
            }
        }
        fov = _mainCamera.fieldOfView;

    }

    private void OnGUI()
    {
        // Episode & Step Count information
        string debugEpisode = "Env: " + envID + " - Ep: " + _envAgents[envID].CurrentEpisode + " - Step: " + _envAgents[envID].StepCount;

        //Number of Players information
        string debugPlayers = "Players: ";
        if (_envAgents[envID].robots != null)
        {
            debugPlayers += _envAgents[envID].robots.Count;
        }


        // // Camera GUI & Reward Information
        bool mainCameraDisplayed = _mainCamera.GetComponent<Camera>().enabled;
        string debugDisplay = "";
        float reward = 0;
        string debugReward = "Reward: ";
        if (mainCameraDisplayed)
        {
            debugDisplay += "Main Display";
            reward =  _envAgents[envID].getMeanReward();
        }
        else 
        {
            int robotID =  _envCameras[envID].GetComponent<Camera_Follow>().playerIndex;
            debugDisplay += "Robot " + robotID.ToString() + " View";
            reward +=  _envAgents[envID].getReward(robotID);
        }
        reward = Mathf.Round(reward * 10000) / 10000;
        GUIStyle rewardStyle = reward < 0 ? _negativeStyle : _positivieStyle;
        debugReward += reward.ToString();

        GUI.Label(new Rect(20, 20, 500, 30), debugEpisode, _defaultStyle);
        GUI.Label(new Rect(20, 60, 500, 30), debugPlayers, _defaultStyle);
        GUI.Label(new Rect(20, 100, 500, 30), debugDisplay, _defaultStyle);
        GUI.Label(new Rect(20, 140, 500, 30), debugReward, rewardStyle);

    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _mainCamera.enabled = !_mainCamera.enabled;
            _envCameras[envID].enabled = !_envCameras[envID].enabled;
        }
        if (Input.GetKeyDown(KeyCode.Tab) && _mainCamera.enabled)
        {
            envID += 1;
            envID %= _envCameras.Count;
        }
        if (_mainCamera.enabled == true)
        {
            Vector3 envCenter = _envHead.GetComponent<Environment2Head>().envCenters[envID];
            Vector3 envSize = _envHead.GetComponent<Environment2Head>().envSizes[envID];
            float maxLength = Mathf.Max(envSize.x, envSize.z);

            Camera.main.fieldOfView = fov;
            if (maxLength == envSize.x)
            {
                float currentAspectRatio = (float)Screen.width / Screen.height;
                Camera.main.fieldOfView = 2f * Mathf.Atan(Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) / currentAspectRatio) * Mathf.Rad2Deg;
            }

            envCenter.y = 1.1f*maxLength / (2 * Mathf.Tan(Mathf.Deg2Rad * fov / 2));
            _mainCamera.transform.position = envCenter;
        }
    }
}

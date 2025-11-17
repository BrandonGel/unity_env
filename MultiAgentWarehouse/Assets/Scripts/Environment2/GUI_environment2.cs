using UnityEngine;
using System;
using multiagent.camera;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using multiagent.agent;
public class GUI_environment2 : MonoBehaviour
{
    [SerializeField] private GameObject _envHead;
    [SerializeField] private Camera _mainCamera;
    [Range(0,360)][SerializeField] public float yawAngle = 0; // rotate y axis
    [Range(-90, 90)][SerializeField] public float rollAngle = 0; // rotate x axis
    public float dx = 0; // rotate y axis
    public float dy = 0; // rotate x axis
    private List<Camera> _envCameras = new List<Camera>();
    private List<Environment2Agent> _envAgents = new List<Environment2Agent>();
    float fov = 60f;
    Vector3 envCameraPos;
    Quaternion envCameraRot;
    private GUIStyle _defaultStyle = new GUIStyle();
    private GUIStyle _positivieStyle = new GUIStyle();
    private GUIStyle _negativeStyle = new GUIStyle();
    public int envID = 0;
    public bool useOrthographic = false;


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

        useOrthographic =_envHead.GetComponent<Environment2Head>().useOrthographic;
        // _mainCamera.orthographic = useOrthographic;

        Scene currentScene = SceneManager.GetActiveScene();
        foreach (Transform child in _envHead.transform)
        // foreach (GameObject rootGameObject in currentScene.GetRootGameObjects())
        {
        //     Transform child = rootGameObject.transform;
            if (child.name.Contains("Environment2"))
            {
                Camera envCamera = child.gameObject.transform.Find("Camera").GetComponent<Camera>();
                envCamera.enabled = false;
                envCamera.orthographic = useOrthographic;
                _envCameras.Add(envCamera);
                _envAgents.Add(child.gameObject.GetComponent<Environment2Agent>());
            }
        }
        fov = _mainCamera.fieldOfView;

    }

    private void OnGUI()
    {
        if (!_envHead.GetComponent<Environment2Head>().showGUI)
        {
            return;
        }

        if (_envHead.GetComponent<Environment2Head>().showGUITime)
        {
            
            string debugTime = "Time: " +  Mathf.Round(_envAgents[envID].StepCount*Time.fixedDeltaTime*100)/100 +"s";
            // float unix_time = _envAgents[envID].getUnixTime();
            // if (unix_time > 0f)
            // {
            //     DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            //     DateTime dateTime = epoch.AddSeconds(unix_time).ToLocalTime();
            //     debugTime = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            // }
            GUI.Label(new Rect(20, 20, 500, 30), debugTime, _defaultStyle);
            return;
        }

        // Episode & Step Count information
        string debugEpisode = "Env: " + envID + " - Ep: " + _envAgents[envID].CurrentEpisode + " - Step: " + _envAgents[envID].StepCount + "/" + (_envAgents[envID].MaxStep-1)  ;
        debugEpisode += " - Time: " +  Mathf.Round(_envAgents[envID].StepCount*Time.fixedDeltaTime*100)/100 + "/" + Mathf.Round(100*(_envAgents[envID].MaxStep-1)*Time.fixedDeltaTime)/100;

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

    private void change_poses()
    {
        if (Input.GetKey(KeyCode.A))
        {
            yawAngle += -0.5f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            yawAngle += 0.5f;
        }
        if(Input.GetKey(KeyCode.W))
        {
            rollAngle += 0.5f;
        }
        else if(Input.GetKey(KeyCode.S))
        {
            rollAngle += -0.5f;
        }
        yawAngle %= 360;
        rollAngle = Mathf.Clamp(rollAngle, -89.9f, 89.9f);
        

        if (Input.GetKey(KeyCode.J))
        {
            dx += -1f;
        }
        else if (Input.GetKey(KeyCode.L))
        {
            dx += 1f;
        }
        if (Input.GetKey(KeyCode.I))
        {
            dy += 0.5f;
        }
        else if (Input.GetKey(KeyCode.K))
        {
            dy += -1f;
        }
        
        if (Input.GetKey(KeyCode.R))
        {
            yawAngle = 0;
            rollAngle = 0;
            dx = 0;
            dy = 0;
        }
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
            yawAngle = 0;
            rollAngle = 0;
            dx = 0;
            dy = 0;
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

            envCenter.y = 1.1f * maxLength / (2 * Mathf.Tan(Mathf.Deg2Rad * fov / 2));

            if (useOrthographic)
            {
                _mainCamera.orthographicSize = envSize.z / 2 + 1;
            }

            change_poses();


            _mainCamera.transform.localRotation =  Quaternion.Euler(90+rollAngle, yawAngle%360f, 0);
            _mainCamera.transform.position = envCenter + new Vector3(dx, 0, dy);
        }
    }
}

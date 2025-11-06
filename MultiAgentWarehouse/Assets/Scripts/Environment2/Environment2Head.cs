using UnityEngine;
using multiagent.parameterJson;
using multiagent.util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
public class Environment2Head : MonoBehaviour
{
    public string configFile = "config.json";
    public parameterJson paramJson = new parameterJson();
    public environmentJson envJson = new environmentJson();
    public GameObject environment2Prefab;
    public List<GameObject> envs = new List<GameObject>();
    public List<Vector3> envCenters = new List<Vector3>();
    public List<Vector3> envSizes = new List<Vector3>();
    public RegisterStringLogSideChannel registerStringLogSideChannel;
    public string savePath,dataPath;
    public bool verbose = false;
    public int num_envs = 1;
    public int CurrentEpisode = 1;
    public Dictionary<string, float> registerMsg = new Dictionary<string, float>();
    public bool showGUI = true;
    public bool useOrthographic = true;
    public ScreenRecorder screenRecorder;
    public bool startRecordingOnPlay = false;
    void Awake()
    {
        paramJson.ReadJson(configFile);
        envJson.ReadJson(configFile);
        Config conf = envJson.GetConfig();
        verbose = paramJson.GetParameter().unityParams.verbose;

        parameters param = paramJson.GetParameter();
        UnityEngine.Random.InitState(param.unityParams.seed);
        Time.timeScale = param.unityParams.timescale;
        Time.fixedDeltaTime = param.unityParams.fixed_timestep;
        num_envs = param.unityParams.num_envs;

        showGUI = param.unityParams.showGUI;
        useOrthographic = param.unityParams.useOrthographic;
        dataPath = param.unityParams.dataPath;
        startRecordingOnPlay = paramJson.param.recordingParams.startRecordingOnPlay;
        if (paramJson.param.unityParams.useCSVExporter || startRecordingOnPlay)
        {
            if (conf.mode != "csv" && !envJson.conf.mode.Contains("replay"))
            {
                BuildSaveDirectory(dataPath);
            }
            else
            {
                savePath = conf.csvpath;
                startRecordingOnPlay = false;
            }
        }

        if (!param.unityParams.useShadow)
        {
            if (verbose)
                Debug.Log("Disabling Shadows");
            QualitySettings.shadows = ShadowQuality.Disable;
            Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                light.shadows = LightShadows.None;
            }
        }

        Root root = envJson.GetRoot();
        int[] dims = root.map.dimensions;
        if (envJson.conf.world_mode == "image")
        {
            float[] scale = root.map.scale;
            dims = new int[] { (int)(scale[0] * dims[0]), (int)(scale[1] * dims[1]) };
        }

        int rows = (int)Mathf.Sqrt(num_envs);
        for (int i = 0; i < num_envs; i++)
        {
            int r = i / rows;
            int c = i % rows;
            float offsetX = c * dims[0] * 1.1f;
            float offsetZ = r * dims[1] * 1.1f;
            GameObject env = Instantiate(environment2Prefab, new Vector3(offsetX, 0, offsetZ), Quaternion.identity);
            env.name = "Environment2_" + i;
            env.GetComponent<Environment2>().setConfigFile(configFile);
            env.GetComponent<Environment2Agent>().setID(i);
            env.GetComponent<Environment2>().setSavePath(savePath);
            env.transform.parent = gameObject.transform;
            envCenters.Add(new Vector3(offsetX + dims[0] * 0.5f, 0, offsetZ + dims[1] * 0.5f));
            envSizes.Add(new Vector3(dims[0], 0, dims[1]));
            envs.Add(env);
        }

        registerMsg["new_map"] = 0f;
        screenRecorder.GetComponent<ScreenRecorder>().setParams(
            startRecordingOnPlay,
            paramJson.param.recordingParams.screenshotWidth,
            paramJson.param.recordingParams.screenshotHeight,
            paramJson.param.recordingParams.recordingDir,
            paramJson.param.recordingParams.actionFrameRate,
            paramJson.param.recordingParams.useFullScreenResolution);

        
        if (startRecordingOnPlay)
        {
            string screenRecorderSavePath = Path.Combine(savePath, "episode_" + CurrentEpisode.ToString("D4"));
            screenRecorder.BuildScreenshotDirectory(screenRecorderSavePath);
        }
    }

    void updateEnv()
    {
        (string parameter, float[] values) = registerStringLogSideChannel.getParseMsg();
        if (registerMsg.ContainsKey(parameter.ToLower()))
        {
            switch (parameter.ToLower())
            {
                case "new_map":
                    if (values.Length == 1 && values[0] == 1f)
                    {
                        if (verbose)
                            Debug.Log("Received new_map command");
                        foreach (Transform env in transform)
                        {
                            env.GetComponent<Environment2>().alreadyCreated = false;
                        }
                        registerMsg["new_map"] = 0f;
                    }
                    break;
            }
        }
        int CurrentEpisode = envs[0].GetComponent<Environment2Agent>().getCurrentEpisode();
        if (startRecordingOnPlay && CurrentEpisode != this.CurrentEpisode)
        {
            this.CurrentEpisode = CurrentEpisode;
            string screenRecorderSavePath = Path.Combine(savePath, "episode_" + CurrentEpisode.ToString("D4"));
            screenRecorder.BuildScreenshotDirectory(screenRecorderSavePath);
            screenRecorder.resetCounter();
        }
        
    }

    private void BuildSaveDirectory(string dataPath="")
	{
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        savePath = Path.Combine(dataPath, "data",timestamp);
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
	}


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        updateEnv();
    }
}

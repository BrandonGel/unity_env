using UnityEngine;
using multiagent.parameterJson;
using multiagent.util;
using System.Collections.Generic;
using Unity.Barracuda;
public class Environment2Head : MonoBehaviour
{
    public string configFile = "config.json";
    public parameterJson paramJson = new parameterJson();
    public environmentJson envJson = new environmentJson();
    public GameObject environment2Prefab;
    public List<Vector3> envCenters = new List<Vector3>();
    public List<Vector3> envSizes = new List<Vector3>();
    public RegisterStringLogSideChannel registerStringLogSideChannel;
    public bool verbose = false;
    public int num_envs = 1;
    void Awake()
    {
        paramJson.ReadJson(configFile);
        envJson.ReadJson(configFile);
        verbose = paramJson.GetParameter().unityParams.verbose;

        parameters param = paramJson.GetParameter();
        Random.InitState(param.unityParams.seed);
        Time.timeScale = param.unityParams.timescale;
        Time.fixedDeltaTime = param.unityParams.fixed_timestep;
        num_envs = param.unityParams.num_envs;

        
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
            float offsetX = c * dims[0]  * 1.1f;
            float offsetZ = r * dims[1] * 1.1f;
            GameObject env = Instantiate(environment2Prefab, new Vector3(offsetX, 0, offsetZ), Quaternion.identity);
            env.name = "Environment2_" + i;
            env.GetComponent<Environment2>().configFile = configFile;
            env.GetComponent<Environment2Agent>().setID(i);
            env.transform.parent = gameObject.transform;
            envCenters.Add(new Vector3(offsetX + dims[0] * 0.5f, 0, offsetZ + dims[1] * 0.5f));
            envSizes.Add(new Vector3(dims[0], 0, dims[1]));
        }
    }

    void updateEnv()
    {
        (string parameter, float[] values) = registerStringLogSideChannel.getParseMsg();
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

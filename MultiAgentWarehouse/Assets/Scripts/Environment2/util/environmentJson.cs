using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class Config
{
    public string envpath = "";
    public string filepath = "";
    public string collisionpath = "";
    public string imagepath = "";
    public string schedulepath = "";
    public string csvfolder = "";
    public string csvpath = "";
    public string dataPath = "";
    public string mode = "";
    public string world_mode = "";
}

[System.Serializable]
public class AgentData
{
    public string name;
    public float[] start;
}   

[System.Serializable]
public class TaskData
{
    public float start_time;
    public string task_name;
    public List<int[]> waypoints;
}


[System.Serializable]
public class DynObsData
{
    public string name;
    public int[] start;
}   

[System.Serializable]
public class Map
{

    public int[] dimensions;
    public int[] offset = {0,0};
    public float[] scale = {1.0f,1.0f};
    public List<List<int[]>> goal_locations;
    public List<int[]> non_task_endpoints = new List<int[]>();
    public List<int[]> obstacles;
    public List<List<int[]>> start_locations;
}

[System.Serializable]
public class Root
{
    public List<AgentData> agents;   
    public Map map;
    public int n_delays_per_agent;
    public int n_tasks;
    public int[] task_freq;
    public List<TaskData> tasks;
}

public class environmentJson
{
    public Root root;
    public Config conf;
    // Read the JSON file and deserialize it into the Root object
    public void ReadJson(string fileName = "config.json")
    {
        string jsonText = "";
        string filepath = fileName;
        if (File.Exists(filepath))
        {
            jsonText = File.ReadAllText(filepath);
        }
        else
        {
            Debug.Log("File: " + filepath);
            Debug.LogError("Config filepath was not found!!!!");
        }
        conf = JsonUtility.FromJson<Config>(jsonText);


        string filePath = conf.envpath;
        // Check if the file exists
        if (!File.Exists(filePath))
        {
            Debug.LogError("JSON file not found at " + filePath);
            return;
        }

        // Read and deserialize the JSON file
        string json;
        try
        {
            json = File.ReadAllText(filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to read JSON file: " + ex.Message);
            return;
        }

        // Deserialize JSON
        try
        {
            root = JsonConvert.DeserializeObject<Root>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to deserialize JSON content: " + ex.Message);
            return;
        }

        // Check if deserialization was successful
        if (root == null || root.map == null || root.map.obstacles == null)
        {
            Debug.LogError("Deserialized JSON content is null or has missing fields.");
            return;
        }
    }

    public int[] GetDimensions()
    {
        return root.map.dimensions;
    }

    public Map GetMap()
    {
        return root.map;
    }

    public Root GetRoot()
    {
        return root;
    }
    
    public Config GetConfig()
    {
        return conf;
    }

}

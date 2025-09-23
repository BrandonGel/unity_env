using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;

[System.Serializable]
public class Position
{
    public float t { get; set; }
    public float x { get; set; }
    public float y { get; set; }
}

public class Schedule
{
    public Dictionary<string, List<Position>> schedule { get; set; }
}
public class scheduleJson
{
    public Schedule data;
    public config conf;
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
        conf = JsonUtility.FromJson<config>(jsonText);


        string filePath = conf.schedulepath;
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
            data = JsonConvert.DeserializeObject<Schedule>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to deserialize JSON content: " + ex.Message);
            return;
        }

        // Check if deserialization was successful
        if (data == null)
        {
            Debug.LogError("Deserialized JSON content is null or has missing fields.");
            return;
        }
    }

    public Schedule GetSchedule()
    {
        return data;
    }

}

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Menu : MonoBehaviour
{
    private string configFile;
    private bool useReplay = true;
    private bool useRandomSpawn = true;
    [SerializeField] private Toggle useReplayToggle;
    [SerializeField] private Toggle useRandomSpawnToggle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnPlayButton()
    {
        SceneManager.LoadScene(1);
    }

    public void OnQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
    }

    public void ReadStringInput(string s)
    {
        configFile = s;
        Debug.Log("Config File: " + configFile);
    }

    public void ToggleReplay(bool b)
    {
        useReplay = b;
        if (useReplay == true)
        {
            useRandomSpawn = false;
            useRandomSpawnToggle.isOn = false;
        }
        Debug.Log("Use Replay: " + useReplay);
    }
    public void ToggleRandomSpawn(bool b)
    {
        useRandomSpawn = b;
        if (useRandomSpawn == true)
        {
            useReplay = false;
            useReplayToggle.isOn = false;
        }
        Debug.Log("Use Random Spawn: " + useRandomSpawn);
    }
}

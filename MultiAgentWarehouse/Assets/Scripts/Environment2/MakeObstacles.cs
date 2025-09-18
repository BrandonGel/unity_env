using UnityEngine;
using System.IO;
using System.Collections.Generic;
using multiagent.util;

public class MakeObstacles : MonoBehaviour
{
    public GameObject obstaclePrefab; // Assign your obstacle prefab here
    private GameObject ground;
    public Material groundMaterial;
    public Material obstacleMaterial;
    private Vector3  scale;
    private environmentJson envJson = new environmentJson();
    private int worldX, worldY, worldZ;

    

    public void CreateRandomObstacles(int x, int z, float obstacleDensity)
    {
        worldX = x;
        worldY = 1;
        worldZ = z;
    }

    public void GenerateWorld(string path, Vector3 scale = default)
    {
        Texture2D texture = Util.LoadTexture(path);
        if (texture == null)
        {
            Debug.LogError("Failed to load texture from " + path);
            return;
        }

        Color[] pixels = texture.GetPixels();
        worldX = texture.width;
        worldY = 1;
        worldZ = texture.height;

        if (scale == default)
            this.scale = new Vector3(1, 1, 1);
        else
            this.scale = new Vector3(
                scale.x / worldX,
                scale.y/worldY,
                scale.z / worldZ
            );

        Vector3[] spawnPoints = new Vector3[pixels.Length];
        // Vector3 startingSpawnPosition = obstaclePrefab.transform.localScale / 2;
        // Vector3 startingSpawnPosition = new Vector3(0.5f, obstaclePrefab.transform.localScale.y/2, 0.5f);
        Vector3 startingSpawnPosition = new Vector3(0f, obstaclePrefab.transform.localScale.y/2, 0f);
        Vector3 currentPosition = startingSpawnPosition;

        int counter = 0;
        for (int z = 0; z < worldZ; z++)
        {
            for (int x = 0; x < worldX; x++)
            {
                spawnPoints[counter] = currentPosition;
                Color c = pixels[counter];
                if (c.Equals(Color.black))
                {
                    CreateObstacle(currentPosition);
                }

                counter++;
                currentPosition.x += 1;

            }
            currentPosition.x = startingSpawnPosition.x;
            currentPosition.z += 1;
        }

        CreateBorderObstacles(new int[] { -1, worldX }, new int[] { -1, worldZ });
        CreateGround();
    }

    public void CreateWorld(string path)
    {
        if(path == "" || path == null)
        {
            Debug.LogError("No path provided for environment texture");
            return;
        }
        envJson.ReadJson(path);
        List<int[]>  obs = envJson.root.map.obstacles;
        int[] dimensions = envJson.root.map.dimensions;
        float[] scale = envJson.root.map.scale;

        worldX = dimensions[0];
        worldY = 1;
        worldZ = dimensions[1];

        this.scale = new Vector3(
                scale[0],
                1,
                scale[1] 
            );

        // Vector3 startingSpawnPosition = obstaclePrefab.transform.localScale / 2;
        // Vector3 startingSpawnPosition = new Vector3(0.5f, obstaclePrefab.transform.localScale.y/2, 0.5f);
        Vector3 startingSpawnPosition = new Vector3(0f, obstaclePrefab.transform.localScale.y/2, 0f);

        for (int i = 0; i < obs.Count; i++)
        {
            int[] loc = obs[i];
            Vector3 pos = new Vector3(loc[0], 0, loc[1]) + startingSpawnPosition;
            CreateObstacle(pos);
        }
        CreateBorderObstacles(new int[] { -1, worldX }, new int[] { -1, worldZ });
        CreateGround();
    }

    private void CreateBorderObstacles(int[] xRange, int[] zRange)
    {
        // Vector3 startingSpawnPosition = obstaclePrefab.transform.localScale / 2;
        // Vector3 startingSpawnPosition = new Vector3(0.5f, obstaclePrefab.transform.localScale.y / 2, 0.5f);
        Vector3 startingSpawnPosition = new Vector3(0f, obstaclePrefab.transform.localScale.y / 2, 0f);
        Vector3 position = new Vector3();

        // Bottom and top borders
        for (int i = xRange[0] + 1; i <= xRange[1] - 1; i++)
        {
            position = new Vector3(i, 0f, zRange[0]) + startingSpawnPosition;
            CreateObstacle(position);
            position = new Vector3(i, 0f, zRange[1]) + startingSpawnPosition;
            CreateObstacle(position);
        }

        // Left and right borders
        for (int j = zRange[0]; j <= zRange[1]; j++)
        {
            position = new Vector3(xRange[0], 0f, j) + startingSpawnPosition;
            CreateObstacle(position);
            position = new Vector3(xRange[1], 0f, j) + startingSpawnPosition;
            CreateObstacle(position);
        }
    }

    private void CreateGround()
    {
        // Example: Generate a simple plane with the texture
        ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "GroundPlane";
        ground.transform.parent = gameObject.transform.Find("Ground").transform;
        Vector3 position = new Vector3((worldX - 1) / 2f, 0, (worldZ - 1) / 2f); // Center the plane
        position = Vector3.Scale(position, this.scale);
        ground.transform.localPosition = position;
        Vector3 groundScale = new Vector3(worldX / 10f, 1, worldZ / 10f); // Unity's plane is 10x10 units
        groundScale = Vector3.Scale(groundScale, this.scale);
        ground.transform.localScale = groundScale;  
        ground.GetComponent<Renderer>().material= groundMaterial;
    }

    private void CreateObstacle(Vector3 currentPosition, Color color = default)
    {
        currentPosition = Vector3.Scale(currentPosition, this.scale);
        GameObject obs = Instantiate(obstaclePrefab, currentPosition, Quaternion.identity);
        obs.transform.localScale = this.scale;
        obs.transform.parent = gameObject.transform.Find("Obstacles").transform;
        obs.transform.localPosition = currentPosition;
        obs.transform.localRotation = Quaternion.identity;
        obs.GetComponent<Renderer>().material= obstacleMaterial;
        if (color != default)
            obs.GetComponent<Renderer>().material.color = color;
    }

    public void ScaleEnvironment(Vector3 scale)
    {
        Transform ground = gameObject.transform.Find("Ground");
        Transform obstacles = gameObject.transform.Find("Obstacles");

        if (ground != null)
        {
            ground.localScale = scale;
        }

        if (obstacles != null)
        {
            obstacles.localScale = scale;
        }
    }

    public void DestroyAll()
    {
        foreach (Transform child in gameObject.transform.Find("Obstacles"))
        {
            Destroy(child.gameObject);
        }
        Destroy(ground);
    }
}
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
    private List<List<Matrix4x4>> Batches = new List<List<Matrix4x4>>();
    private int batchSize = 1000; // Maximum number of instances per batch for DrawMeshInstanced
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
        Vector3 position = startingSpawnPosition;

        int counter = 0;
        for (int z = 0; z < worldZ; z++)
        {
            for (int x = 0; x < worldX; x++)
            {
                spawnPoints[counter] = position;
                Color c = pixels[counter];
                if (c.Equals(Color.black))
                {
                    CreateObstacle(position);
                    // AddObstacles(position, Quaternion.identity, this.scale);
                }

                counter++;
                position.x += 1;

            }
            position.x = startingSpawnPosition.x;
            position.z += 1;
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
            Vector3 position = new Vector3(loc[0], 0, loc[1]) + startingSpawnPosition;
            CreateObstacle(position);
            // AddObstacles(position, Quaternion.identity, this.scale);
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
            // AddObstacles(position, Quaternion.identity, this.scale);
            position = new Vector3(i, 0f, zRange[1]) + startingSpawnPosition;
            CreateObstacle(position);
            // AddObstacles(position, Quaternion.identity, this.scale);
        }

        // Left and right borders
        for (int j = zRange[0]; j <= zRange[1]; j++)
        {
            position = new Vector3(xRange[0], 0f, j) + startingSpawnPosition;
            CreateObstacle(position);
            // AddObstacles(position, Quaternion.identity, this.scale);
            position = new Vector3(xRange[1], 0f, j) + startingSpawnPosition;
            CreateObstacle(position);
            // AddObstacles(position, Quaternion.identity, this.scale);
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

    private void AddObstacles(Vector3 currentPosition, Quaternion rotation , Vector3 scale)
    {
        if (Batches.Count == 0 || Batches[Batches.Count-1].Count >= batchSize)
        {
            Batches.Add(new List<Matrix4x4>());
        }
        currentPosition = Vector3.Scale(currentPosition, scale);
        Matrix4x4 matrix = Matrix4x4.TRS(currentPosition, rotation, scale);
        Batches[Batches.Count-1].Add(matrix);
    }

    public void OnRenderObject()
    {
        if (obstaclePrefab == null || obstaclePrefab.GetComponent<MeshFilter>() == null)
            return;

        Mesh mesh = obstaclePrefab.GetComponent<MeshFilter>().sharedMesh;
        Material material = obstacleMaterial;

        foreach (var batch in Batches)
        {
            if (batch.Count > 0)
            {
                Graphics.DrawMeshInstanced(mesh, 0, material, batch);
            }
        }
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
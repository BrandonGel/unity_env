using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ArrowGenerator : MonoBehaviour
{
    public float stemLength = 0.5f;
    public float stemWidth = 0.25f;
    public float tipLength = 0.25f;
    public float tipWidth = 0.25f;

    [Range(2, 360)] public int numberAxialPoints = 36;
 
    [System.NonSerialized]
    public List<Vector3> verticesList;
    [System.NonSerialized]
    public List<int> trianglesList;
    Mesh mesh;
    public Material Material; 

    void Start()
    {
        //make sure Mesh Renderer has a material
        mesh = new Mesh();
        this.GetComponent<MeshFilter>().mesh = mesh;
        GenerateArrow();
    }

    void Update()
    {
        // GenerateArrow();
        
    }

    void GenerateArrow()
    {
        //setup
        verticesList = new List<Vector3>();
        trianglesList = new List<int>();

        //stem setup
        Vector3 stemOrigin = new Vector3(0, 0.5f, 0);
        float stemHalfWidth = stemWidth/2f;

        // directional vector
        Vector3 directionalVector = Vector3.right;
        Vector3 normalVector = Vector3.forward;;

        //tip setup
        Vector3 tipOrigin = stemLength * directionalVector + stemOrigin;
        float tipHalfWidth = tipWidth / 2;

        float angle = 360f / numberAxialPoints;
        verticesList.Add(tipOrigin);
        verticesList.Add(tipOrigin + (tipLength * directionalVector));
        for (int ii = 0; ii < numberAxialPoints; ii++)
        {
            // Vector3 vector = Quaternion.AngleAxis(angle * ii, directionalVector) * normalVector;
            // verticesList.Add(stemOrigin + (stemHalfWidth * vector));
            // verticesList.Add(verticesList[-1] + (stemLength * directionalVector));

            Vector3 vector = Quaternion.AngleAxis(angle * ii, directionalVector) * normalVector;
            Vector3 vertex = tipOrigin + (tipHalfWidth * vector);
            verticesList.Add(vertex);
            // Debug.Log("ii: " + vertex);
        }

        for (int ii = 0; ii < numberAxialPoints; ii++)
        {

            if (ii < numberAxialPoints - 1)
            {
                trianglesList.Add(0);
                trianglesList.Add(ii + 2);
                trianglesList.Add(ii + 3);
                trianglesList.Add(ii + 3);
                trianglesList.Add(ii + 2);
                trianglesList.Add(0);

                trianglesList.Add(1);
                trianglesList.Add(ii + 2);
                trianglesList.Add(ii + 3);
                trianglesList.Add(ii + 3);
                trianglesList.Add(ii + 2);
                trianglesList.Add(1);
            }
            else
            {
                trianglesList.Add(0);
                trianglesList.Add(ii + 2);
                trianglesList.Add(2);
                trianglesList.Add(2);
                trianglesList.Add(ii + 2);
                trianglesList.Add(0);

                trianglesList.Add(1);
                trianglesList.Add(ii + 2);
                trianglesList.Add(2);
                trianglesList.Add(2);
                trianglesList.Add(ii + 2);
                trianglesList.Add(1);
            }
        }
        mesh.vertices = verticesList.ToArray();
        mesh.triangles = trianglesList.ToArray();

        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.parent = gameObject.transform;
        cylinder.transform.localPosition = stemOrigin+(stemLength/2f*directionalVector);
        cylinder.transform.localRotation = Quaternion.AngleAxis(90f, normalVector);
        Vector3 scale = new Vector3(stemWidth,stemLength/2f,stemWidth);
        cylinder.transform.localScale = scale;
        cylinder.GetComponent<Renderer>().material.color = Material.color;
    }

  
}
 
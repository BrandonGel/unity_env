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
    public float minLim = -1;
    public float maxLim = 1;
    public string dir = "r";
    public bool noShowZero = false;
    Vector3 directionalVector,normalVector,stemOrigin,tipOrigin;
    [Range(2, 360)] public int numberAxialPoints = 36;

    [System.NonSerialized]
    public List<Vector3> verticesList;
    [System.NonSerialized]
    public List<int> trianglesList;
    Mesh mesh;
    GameObject cylinder;
    public Material Material;
    Color color;

    void Start()
    {
        //make sure Mesh Renderer has a material
        mesh = new Mesh();
        this.GetComponent<MeshFilter>().mesh = mesh;
        GenerateArrow();
    }

    (Vector3, Vector3) getDirection(string dir = "r")
    {
        if (dir == "r")
        {
            return (Vector3.right, Vector3.forward);
        }
        else if (dir == "u")
        {
            return (Vector3.up, Vector3.right);
        }
        else if (dir == "f")
        {
            return (Vector3.forward, Vector3.up);
        }
        return (Vector3.zero, Vector3.up);
    }

    void GenerateArrow()
    {
        //setup
        verticesList = new List<Vector3>();
        trianglesList = new List<int>();

        //stem setup
        stemOrigin = new Vector3(0, 0.5f, 0);

        // directional vector
        (directionalVector, normalVector) = getDirection(dir);

        //tip setup
        tipOrigin = stemLength * directionalVector + stemOrigin;
        float tipHalfWidth = tipWidth / 2;

        float angle = 360f / numberAxialPoints;
        verticesList.Add(tipOrigin);
        verticesList.Add(tipOrigin + (tipLength * directionalVector));
        for (int ii = 0; ii < numberAxialPoints; ii++)
        {
            Vector3 vector = Quaternion.AngleAxis(angle * ii, directionalVector) * normalVector;
            Vector3 vertex = tipOrigin + (tipHalfWidth * vector);
            verticesList.Add(vertex);
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
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.SetColor("_Color", color); 

        cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.parent = gameObject.transform;
        cylinder.transform.localPosition = stemOrigin + (stemLength / 2f * directionalVector);
        Vector3 forwardVector = Vector3.Cross(directionalVector, normalVector); // Or any other non-parallel vector
        cylinder.transform.localRotation = Quaternion.LookRotation(forwardVector, directionalVector);
        Vector3 localScale = new Vector3(stemWidth, stemLength / 2f, stemWidth);
        cylinder.transform.localScale = localScale;
        cylinder.GetComponent<Renderer>().material.color = color;
    }

    public void setParam(string dir = "r", float minLim = -1, float maxLim = 1, Color color = default, bool noShowZero = false)
    {
        this.dir = dir;
        this.minLim = minLim;
        this.maxLim = maxLim;
        this.noShowZero = noShowZero;
        this.color = color;
    }
    public void scaleArrow(float dummyValue)
    {
        float directionSign = Mathf.Sign(dummyValue);
        float scale = Mathf.Abs(dummyValue) / maxLim;
        
        Vector3 newCylinderPos = stemOrigin + directionSign*scale*(stemLength / 2f * directionalVector);
        cylinder.transform.localPosition = newCylinderPos;
        cylinder.transform.localScale = new Vector3(stemWidth, scale*stemLength / 2f, stemWidth);


        Vector3[] vertices = mesh.vertices;
        for (var i = 0; i < vertices.Length; i++)
        {
            if (noShowZero && scale <= 1e-6)
            {
                vertices[i] = new Vector3(0f,0f,0f);
                continue;
            }

            vertices[i] = verticesList[i] -(1f-scale)*stemLength * directionalVector ;
            if (directionSign < 0)
            {
                vertices[i] = Quaternion.AngleAxis(180, normalVector) * (vertices[i]-stemOrigin)+stemOrigin;
            }
        }

        mesh.vertices = vertices;
    }

}
 
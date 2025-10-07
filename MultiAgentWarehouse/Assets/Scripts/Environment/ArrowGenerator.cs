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
    public float yOffset = 0;
    public bool useNoCylinder = true;
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
            return (Vector3.down, Vector3.right);
        }
        else if (dir == "f")
        {
            return (Vector3.forward, Vector3.up);
        }
        return (Vector3.right, Vector3.up);
    }

    void GenerateArrowWithCylinder()
    {
        //setup
        verticesList = new List<Vector3>();
        trianglesList = new List<int>();

        //stem setup
        stemOrigin = new Vector3(0, yOffset, 0);

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
        meshRenderer.material.SetColor("_BaseColor", color); 

        cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.parent = gameObject.transform;
        cylinder.transform.localPosition = stemOrigin + (stemLength / 2f * directionalVector);
        Vector3 forwardVector = Vector3.Cross(directionalVector, normalVector); // Or any other non-parallel vector
        cylinder.transform.localRotation = Quaternion.LookRotation(forwardVector, directionalVector);
        Vector3 localScale = new Vector3(stemWidth, stemLength / 2f, stemWidth);
        cylinder.transform.localScale = localScale;
        cylinder.GetComponent<Renderer>().material.color = color;
    }

    void GenerateArrowWithoutCylinder()
    {
        //setup
        verticesList = new List<Vector3>();
        trianglesList = new List<int>();

        //stem setup
        stemOrigin = new Vector3(0, yOffset + 1f, 0);

        // directional vector
        (directionalVector, normalVector) = getDirection(dir);

        //tip setup
        tipOrigin = stemLength * directionalVector + stemOrigin;
        float tipHalfWidth = tipWidth / 2;


        verticesList.Add(tipOrigin + (tipLength * directionalVector));
        verticesList.Add(tipOrigin + tipHalfWidth * normalVector);
        verticesList.Add(tipOrigin - tipHalfWidth * normalVector);
        trianglesList.Add(0);
        trianglesList.Add(2);
        trianglesList.Add(1);

        float stemHalfWidth = stemWidth / 2;
        verticesList.Add(stemOrigin + stemHalfWidth * normalVector);
        verticesList.Add(stemOrigin - stemHalfWidth * normalVector);
        verticesList.Add(verticesList[3] + (stemLength * directionalVector));
        verticesList.Add(verticesList[4] + (stemLength * directionalVector));
        trianglesList.Add(3);
        trianglesList.Add(6);
        trianglesList.Add(4);
        trianglesList.Add(3);
        trianglesList.Add(5);
        trianglesList.Add(6);


        mesh.vertices = verticesList.ToArray();
        mesh.triangles = trianglesList.ToArray();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.SetColor("_Color", color);
        meshRenderer.material.SetColor("_BaseColor", color);
    }

    void GenerateArrow()
    {
        if (useNoCylinder)
        {
            GenerateArrowWithoutCylinder();
        }
        else
        {
            GenerateArrowWithCylinder();
        }
    }

    public void setParam(string dir = "r", float minLim = -1, float maxLim = 1, Color color = default, float[] floatParams = default,  bool useNoCylinder = true)
    {
        this.dir = dir;
        this.minLim = minLim;
        this.maxLim = maxLim;
        this.color = color;
        this.stemLength = floatParams[0];
        this.stemWidth = floatParams[1];
        this.tipLength = floatParams[2];
        this.tipWidth = floatParams[3];
        this.yOffset = floatParams[4];
        this.useNoCylinder = useNoCylinder;
    }
    
    public void ScaleArrowWithCylinder(float dummyValue, Vector3 direction = default)
    {
        if (direction != default)
        {
            (directionalVector, normalVector) = getDirection(dir);
            Vector3 binormalVector = Vector3.Cross(directionalVector, normalVector).normalized;

            Vector3 newDirectionalVector = transform.InverseTransformDirection(direction).normalized;
            float signedAngle = Vector3.SignedAngle(directionalVector, newDirectionalVector, binormalVector);

            directionalVector = newDirectionalVector;
            normalVector = Quaternion.AngleAxis(signedAngle, binormalVector) * normalVector;

            Vector3 forwardVector = Vector3.Cross(directionalVector, normalVector); // Or any other non-parallel vector
            cylinder.transform.localRotation = Quaternion.LookRotation(forwardVector, directionalVector);
        }

        float directionSign = Mathf.Sign(dummyValue);
        float scale = Mathf.Clamp(Mathf.Abs(dummyValue) / maxLim, 0, 1);

        Vector3 newCylinderPos = stemOrigin + directionSign * scale * (stemLength / 2f * directionalVector);
        cylinder.transform.localPosition = newCylinderPos;
        cylinder.transform.localScale = new Vector3(stemWidth, scale * stemLength / 2f, stemWidth);


        Vector3[] vertices = mesh.vertices;
        for (var i = 0; i < vertices.Length; i++)
        {
            if (
                scale <= 1e-3)
            {
                vertices[i] = new Vector3(0f, 0f, 0f);
                continue;
            }

            vertices[i] = verticesList[i] - (1f - scale) * stemLength * directionalVector;
            if (directionSign < 0)
            {
                vertices[i] = Quaternion.AngleAxis(180, normalVector) * (vertices[i] - stemOrigin) + stemOrigin;
            }

            if (direction != default)
            {
                vertices[i] = Quaternion.FromToRotation(Vector3.right, directionalVector) * (vertices[i] - stemOrigin) + stemOrigin;
            }

        }

        mesh.vertices = vertices;
    }

    public void ScaleArrowWithoutCylinder(float dummyValue, Vector3 direction = default)
    {
        
        if (direction != default)
        {
            (directionalVector, normalVector) = getDirection(dir);
            Vector3 binormalVector = Vector3.Cross(directionalVector, normalVector).normalized;

            Vector3 newDirectionalVector = transform.InverseTransformDirection(direction).normalized;
            float signedAngle = Vector3.SignedAngle(directionalVector, newDirectionalVector, binormalVector);

            directionalVector = newDirectionalVector;
            normalVector = Quaternion.AngleAxis(signedAngle, binormalVector) * normalVector;
        }

        float scale = Mathf.Clamp(Mathf.Abs(dummyValue) / maxLim, 0, 1);

        Vector3[] vertices = mesh.vertices;

        //tip setup
        tipOrigin = scale*stemLength * directionalVector + stemOrigin;
        float tipHalfWidth = tipWidth / 2;
        vertices[0] = tipOrigin + tipLength * directionalVector;
        vertices[1] = tipOrigin + tipHalfWidth * normalVector;
        vertices[2] = tipOrigin - tipHalfWidth * normalVector;

        float stemHalfWidth = stemWidth / 2;
        vertices[3] = stemOrigin + stemHalfWidth * normalVector;
        vertices[4] = stemOrigin - stemHalfWidth * normalVector;
        vertices[5] = vertices[3] + (scale*stemLength * directionalVector);
        vertices[6] = vertices[4] + (scale*stemLength * directionalVector);

        for (var i = 0; i < vertices.Length; i++)
        {
            if (scale <= 1e-3)
            {
                vertices[i] = new Vector3(0f, 0f, 0f);
                continue;
            }
        }

        mesh.vertices = vertices;
    }

    public void scaleArrow(float dummyValue, Vector3 direction = default)
    {
        float scale = Mathf.Clamp(Mathf.Abs(dummyValue) / maxLim, 0, 1);
        
        if (scale <= 1e-3)
        {
            GetComponent<MeshRenderer>().enabled = false;
            return;
        }
        else
        {
            GetComponent<MeshRenderer>().enabled = true;
        }

        if (useNoCylinder)
        {
            ScaleArrowWithoutCylinder(dummyValue, direction);
        }
        else
        {
            ScaleArrowWithCylinder(dummyValue, direction);
        }
    }

}
 
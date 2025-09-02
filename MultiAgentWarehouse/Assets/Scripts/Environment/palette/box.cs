using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VirtualBox : MonoBehaviour
{
    private Vector3 offset;
    public Material defaultMaterial;
    public Material paletteMaterial;
    public Material paletteBoxMaterial;
    private Renderer _renderer;
    private int _goalType=-1;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        enableRenderer(false);
        offset = new Vector3(
            transform.position.x,
            transform.position.y,
            transform.position.z
        );
    }

    public void enableRenderer(bool turnon = true)
    {
        _renderer.enabled = turnon;
    }


    public void getPalette(int goalType)
    {   
        if(_goalType == goalType)
            return;
        _goalType = goalType;
        switch (goalType)
        {
            case 0:
                changeColor(0);
                break;
            case 1:
                changeColor(2);
                break;
            case 2:
                changeColor(0);
                break;
            case 3:
                changeColor(1);
                break;
            case 4:
                changeColor(0);
                break;
            default:
                changeColor(0);
                break;
        }
    }

    public void resetParameters()
    {
        changeColor(0);
        enableRenderer(false);
    }

    public void changeColor(int colorCase)
    {
        switch (colorCase)
        {
            case 0:
                enableRenderer(false);
                if (_renderer != null)
                {
                    _renderer.material = defaultMaterial;
                }
                break;
            case 1:
                enableRenderer(true);
                if (_renderer != null)
                {
                    _renderer.material = paletteMaterial;
                }
                break;
            case 2:
                enableRenderer(true);
                if (_renderer != null)
                {
                    _renderer.material = paletteBoxMaterial;
                }
                break;
            default:
                enableRenderer(false);
                if (_renderer != null)
                {
                    _renderer.material = defaultMaterial;
                }
                break;
        }
    }

    void Update()
    {
        Transform robotTransform = transform.parent.GetComponent<Transform>();
        transform.position = new Vector3(
                robotTransform.position.x,
                robotTransform.position.y + robotTransform.position.y/ 2,
                robotTransform.position.z
            ) + offset;
        transform.rotation = robotTransform.rotation;
        return;
    }
}
 
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class PathTracker : MonoBehaviour
{
    [SerializeField] private Transform trackerTransform;
    [SerializeField] private LineRenderer lineRenderer;

    public int maxPathPositionListCount = 20;
    LineRenderer _lineRenderer;
    private List<Vector3> pathPositionList;
    private float trackTimer;
    private int counter;
    void Awake()
    {
        pathPositionList = new List<Vector3>();
    }

    void Start()
    {
        _lineRenderer = Instantiate(lineRenderer);
        _lineRenderer.transform.parent = transform.parent.parent.Find("Paths"); //Go from Robot to Robots to Environment to then Paths
        _lineRenderer.transform.position = Vector3.zero;
        _lineRenderer.transform.rotation = Quaternion.identity;
        pathPositionList.Add(trackerTransform.position);
        RefreshVisual();
    }

    void Update()
    {
        trackTimer -= Time.deltaTime;
        if (trackTimer <= 0f)
        {
            float trackTimerMax = .2f;
            trackTimer += trackTimerMax;

            Vector3 lastPathPosition = pathPositionList[pathPositionList.Count - 1];
            Vector3 newPathPosition = trackerTransform.position;

            float minPathDistance = .5f;
            if (Vector3.Distance(lastPathPosition, newPathPosition) > minPathDistance)
            {
                pathPositionList.Add(trackerTransform.position);
                RefreshVisual();

                // if (pathPositionList.Count > maxPathPositionListCount)
                // {
                //     pathPositionList.RemoveAt(0);
                // }

            }
        }
    }

    void RefreshVisual()
    {
        _lineRenderer.positionCount = pathPositionList.Count;
        _lineRenderer.SetPositions(pathPositionList.ToArray());
    }
}
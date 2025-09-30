using System;
using System.Collections;
using System.Collections.Generic;
using multiagent.agent;
using multiagent.robot;
using Unity.VisualScripting;
using UnityEngine;
using Robot = multiagent.robot.Robot;
namespace multiagent.parameterJson
{
    public class PathVisualize : MonoBehaviour
    {
        [SerializeField] private Transform trackerTransform;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Robot2 _robot;
        public int maxPathPositionListCount = 20;
        public float minPathDistance = 0.5f;
        public float lineRendererWidth = 25f;
        LineRenderer _lineRenderer;
        private List<Vector3> pathPositionList;
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
            Reset();
        }

        public void Reset()
        {
            pathPositionList.Clear();
            maxPathPositionListCount = _robot.GetComponent<Robot2>().lineRendererMaxPathPositionListCount;
            minPathDistance = _robot.GetComponent<Robot2>().lineRendererMinPathDistance;
            lineRendererWidth = _robot.GetComponent<Robot2>().lineRendererWidth;
            if (maxPathPositionListCount == 0)
            {
                return;
            }
            _lineRenderer.startWidth = lineRendererWidth;
            _lineRenderer.endWidth = lineRendererWidth;
            pathPositionList.Add(trackerTransform.position);
            RefreshVisual();
        }

        void FixedUpdate()
        {
            if (_robot != null && _robot.StepCount <= 1)
            {
                Reset();
            }

            if(maxPathPositionListCount == 0)
            {
                return;
            }

            Vector3 lastPathPosition = pathPositionList[pathPositionList.Count - 1];
            Vector3 newPathPosition = trackerTransform.position;
            if (Vector3.Distance(lastPathPosition, newPathPosition) >= minPathDistance)
            {
                pathPositionList.Add(trackerTransform.position);
                RefreshVisual();

                if (maxPathPositionListCount > -1 && pathPositionList.Count > maxPathPositionListCount)
                {
                    pathPositionList.RemoveAt(0);
                }

            }
            
        }

        void RefreshVisual()
        {
            _lineRenderer.positionCount = pathPositionList.Count;
            _lineRenderer.SetPositions(pathPositionList.ToArray());
        }
    }
}
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
        /// <summary>
        /// Smooth a path represented as a list of Vector3 positions using Chaikin's corner-cutting algorithm.
        /// </summary>
        /// <param name="inputPath">The original list of Vector3 positions</param>
        /// <param name="iterations">Number of smoothing iterations</param>
        /// <returns>A new, smoothed list of Vector3 positions</returns>
        public static List<Vector3> SmoothPath(List<Vector3> inputPath, int iterations = 6)
        {
            if (inputPath == null || inputPath.Count < 3 || iterations <= 0)
                return new List<Vector3>(inputPath);

            List<Vector3> path = new List<Vector3>(inputPath);

            for (int iter = 0; iter < iterations; iter++)
            {
                List<Vector3> newPath = new List<Vector3>();
                newPath.Add(path[0]); // Keep the first point

                for (int i = 0; i < path.Count - 1; i++)
                {
                    Vector3 p0 = path[i];
                    Vector3 p1 = path[i + 1];

                    Vector3 Q = Vector3.Lerp(p0, p1, 0.25f);
                    Vector3 R = Vector3.Lerp(p0, p1, 0.75f);

                    newPath.Add(Q);
                    newPath.Add(R);
                }

                newPath.Add(path[path.Count - 1]); // Keep the last point
                path = newPath;
            }

            return path;
        }

        void RefreshVisual()
        {
            
            List<Vector3> smoothPath = SmoothPath(pathPositionList);
            _lineRenderer.positionCount = smoothPath.Count;
            _lineRenderer.SetPositions(smoothPath.ToArray());
        }
    }
}
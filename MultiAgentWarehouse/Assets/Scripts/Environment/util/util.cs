using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


namespace multiagent.util
{
    public static class Util
    {
        public static Vector2 Round2(Vector2 vector2, int decimalPlaces = 2)
        {
            float multiplier = 1;
            for (int i = 0; i < decimalPlaces; i++)
            {
                multiplier *= 10f;
            }
            return new Vector2(
                Mathf.Round(vector2.x * multiplier) / multiplier,
                Mathf.Round(vector2.y * multiplier) / multiplier);
        }

        public static Vector3 Round3(Vector3 vector3, int decimalPlaces = 2)
        {
            float multiplier = 1;
            for (int i = 0; i < decimalPlaces; i++)
            {
                multiplier *= 10f;
            }
            return new Vector3(
                Mathf.Round(vector3.x * multiplier) / multiplier,
                Mathf.Round(vector3.y * multiplier) / multiplier,
                Mathf.Round(vector3.z * multiplier) / multiplier);
        }

        public static void SetBoxProperties(GameObject box, bool collidable, bool gravity, bool isOpaque, Material transparentMaterial = null)
        {
            Collider collider = box.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = collidable;
            }

            Rigidbody rb = box.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = gravity;
            }

            Renderer renderer = box.GetComponent<Renderer>();
            if (renderer != null && transparentMaterial != null)
            {
                // Create a new instance of the transparent material to modify it without affecting the original
                Material material = new Material(transparentMaterial);

                if (isOpaque)
                {
                    // Set color and transparency for opaque boxes
                    if (box.CompareTag("TopBox"))
                    {
                        material.color = new Color(0.3f, 0.8f, 0.3f, 1f); // Dark green for top box
                    }
                    else
                    {
                        material.color = new Color(0.5f, 1, 0.5f, 1f); // Light green for bottom box
                    }
                    material.SetFloat("_Metallic", 0.96f);
                    material.SetFloat("_Glossiness", 1f);
                }
                else
                {
                    // Set color and transparency for transparent boxes
                    if (box.CompareTag("TopBox"))
                    {
                        material.color = new Color(0.8f, 0.8f, 0.3f, 0.35f); // Dark yellow for top box
                    }
                    else
                    {
                        material.color = new Color(1, 1, 0.5f, 0.35f); // Light yellow for bottom box
                    }


                    // Ensure transparency and emission are enabled
                    material.SetFloat("_Mode", 3); // Transparent mode
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.renderQueue = 3000;
                }

                // Assign the modified material to the renderer
                renderer.material = material;
            }
        }

        // Change the name (removing (clone) from instaniate object and replace it with a number)
        public static string getNewName(GameObject obj, int idx = 0)
        {
            string name = obj.name;
            string subname = name.Substring(0, name.Length - 7);
            string newName = name.Substring(0, subname.Length) + " (" + idx.ToString() + ")";
            return newName;
        }

        public static float linearInterpolate(float a, float b, float x)
        {
            if (a == b)
            {
                return 0f;
            }
            float frac = (x - a) / (b - a);
            frac = Mathf.Clamp(frac, 0, 1);
            return frac;
        }
        public static Vector3 interpolate(Vector3 x, Vector3 y, float t)
        {
            Vector3 pos = x + (y - x) * t;
            return pos;
        }

        public static float CalculateHeading(Vector3 currentPosition, Vector3 nextPosition)
        {
            Vector2 direction = new Vector2(nextPosition.x - currentPosition.x, nextPosition.z - currentPosition.z);
            return -Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        public static float DetermineHeading(float currentHeading, float targetHeading)
        {
            float angleDifferenceForward = Mathf.DeltaAngle(currentHeading, targetHeading);
            float angleDifferenceBackward = Mathf.DeltaAngle(currentHeading, targetHeading + 180f);
            float bestHeading;
            // Decide whether to face forward or backward
            if (Mathf.Abs(angleDifferenceForward) <= Mathf.Abs(angleDifferenceBackward))
            {
                bestHeading = targetHeading;
            }
            else
            {
                bestHeading = targetHeading + 180f;
            }
            return bestHeading;
        }


        public static void enableRenderer(Renderer _renderer, bool turnon = true)
        {
            _renderer.enabled = turnon;
        }

        public static Texture2D LoadTexture(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("File not found at " + path);
                return null;
            }
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2); // size doesn't matter, LoadImage will replace it
            tex.LoadImage(fileData); // Loads PNG/JPG bytes into texture
            return tex;
        }

        /// <summary>
        /// Counts the number of directories matching the pattern "env#" (where # is a number) in the specified folder
        /// </summary>
        /// <param name="folderPath">Path to the folder to search in</param>
        /// <returns>Number of directories matching the "env#" pattern, or -1 if folder doesn't exist</returns>
        public static int CountEnvFolders(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.LogError($"Folder not found: {folderPath}");
                return -1;
            }

            try
            {
                string[] directories = Directory.GetDirectories(folderPath);
                int count = 0;
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^env_\d+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                foreach (string dirPath in directories)
                {
                    string dirName = Path.GetFileName(dirPath);
                    if (regex.IsMatch(dirName))
                    {
                        count++;
                    }
                }

                return count;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error counting env folders: {e.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Returns a list of all subdirectories in the specified folder path
        /// </summary>
        /// <param name="folderPath">Path to the folder to search in</param>
        /// <returns>List of subdirectory names (without full path), or null if folder doesn't exist</returns>
        public static List<string> GetSubdirectories(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.LogError($"Folder not found: {folderPath}");
                return null;
            }

            try
            {
                string[] directories = Directory.GetDirectories(folderPath);
                List<string> subdirNames = new List<string>();

                foreach (string dirPath in directories)
                {
                    string dirName = Path.GetFileName(dirPath);
                    subdirNames.Add(dirName);
                }

                return subdirNames;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting subdirectories: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Returns a list of all subdirectory full paths in the specified folder path
        /// </summary>
        /// <param name="folderPath">Path to the folder to search in</param>
        /// <returns>List of full paths to subdirectories, or null if folder doesn't exist</returns>
        public static List<string> GetSubdirectoryPaths(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.LogError($"Folder not found: {folderPath}");
                return null;
            }

            try
            {
                string[] directories = Directory.GetDirectories(folderPath);
                return new List<string>(directories);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting subdirectory paths: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parses out all numbers from a string and returns them as a list of integers
        /// </summary>
        /// <param name="input">Input string to parse</param>
        /// <returns>List of integers found in the string</returns>
        public static List<int> ParseNumbers(string input)
        {
            List<int> numbers = new List<int>();

            if (string.IsNullOrEmpty(input))
            {
                return numbers;
            }

            try
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\d+");
                System.Text.RegularExpressions.MatchCollection matches = regex.Matches(input);

                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (int.TryParse(match.Value, out int number))
                    {
                        numbers.Add(number);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing numbers from string: {e.Message}");
            }

            return numbers;
        }

        /// <summary>
        /// Parses out the first number found in a string and returns it as an integer
        /// </summary>
        /// <param name="input">Input string to parse</param>
        /// <returns>First integer found in the string, or -1 if no number is found</returns>
        public static int ParseFirstNumber(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return -1;
            }

            try
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\d+");
                System.Text.RegularExpressions.Match match = regex.Match(input);

                if (match.Success && int.TryParse(match.Value, out int number))
                {
                    return number;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing first number from string: {e.Message}");
            }

            return -1;
        }

        /// <summary>
        /// Parses out all numbers from a string and returns them as a list of floats
        /// </summary>
        /// <param name="input">Input string to parse</param>
        /// <returns>List of floats found in the string</returns>
        public static List<float> ParseFloatNumbers(string input)
        {
            List<float> numbers = new List<float>();

            if (string.IsNullOrEmpty(input))
            {
                return numbers;
            }

            try
            {
                // Match integers and floating point numbers (including decimals)
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"-?\d+\.?\d*");
                System.Text.RegularExpressions.MatchCollection matches = regex.Matches(input);

                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (float.TryParse(match.Value, out float number))
                    {
                        numbers.Add(number);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing float numbers from string: {e.Message}");
            }

            return numbers;
        }

        public static List<int> sample_affected_agents_efficient(int number_of_affected, int total_agents)
        {
            System.Random random = new System.Random();
            int count = Mathf.Max(Mathf.Min(number_of_affected, total_agents), 0);
            if (count == 0)
            {
                return new List<int>();
            }

            List<int> agents = new List<int>();
            // Create list of all agent indices from 0 to total_agents-1
            for (int i = 0; i < total_agents; i++)
            {
                agents.Add(i);
            }

            // Shuffle first 'count' elements using Fisher-Yates algorithm
            for (int i = 0; i < number_of_affected; i++)
            {
                int randomIndex = random.Next(i, total_agents);
                int temp = agents[i];
                agents[i] = agents[randomIndex];
                agents[randomIndex] = temp;
            }

            // Return first 'count' elements
            return agents.GetRange(0, count);
        }

        public static int sample_num_uniformly(int number_of_affected, float u)
        {
            System.Random random = new System.Random();

            int num_of_affected = 0;
            for (int i = 0; i < number_of_affected; i++)
            {
                float x = (float)random.NextDouble();
                if (x <= u)
                {
                    num_of_affected += 1;
                }
            }

            // Return first 'count' elements
            return num_of_affected;
        }
        public static List<float> sample_float_list(int number_of_affected, float[] time_length)
        {
            if (number_of_affected == 0)
            {
                return new List<float>();
            }

            System.Random random = new System.Random();
            List<float> time_samples = new List<float>();
            // Shuffle first 'count' elements using Fisher-Yates algorithm
            for (int i = 0; i < number_of_affected; i++)
            {
                float x = (float)random.NextDouble();
                float t = time_length[0] + (time_length[1] - time_length[0]) * x;
                time_samples.Add(t);
            }

            // Return first 'count' elements
            return time_samples;
        }

        public static List<GameObject> GetGameObjectsFromChild(Transform transform, string childName)
        {
            List<GameObject> objects = new List<GameObject>();

            if (transform == null)
            {
                Debug.LogWarning("Parent object is null");
                return objects;
            }

            Transform childTransform = transform.Find(childName);
            if (childTransform != null)
            {
                for (int i = 0; i < childTransform.childCount; i++)
                {
                    objects.Add(childTransform.GetChild(i).gameObject);
                }
            }
            else
            {
                Debug.LogWarning($"'{childName}' child not found in " + transform.name);
            }

            return objects;
        }

        public static List<GameObject> GetNearbyGameObjects(GameObject targetObject, float radius, int layerMask = -1, string tag = "everyone")
        {
            List<GameObject> nearbyObjects = new List<GameObject>();

            if(tag == "everyone")
            {
                return nearbyObjects;
            }

            if (targetObject == null)
            {
                Debug.LogWarning("Target object is null");
                return nearbyObjects;
            }

            Vector3 targetPosition = targetObject.transform.position;

            // Get all colliders in the radius
            Collider[] colliders = Physics.OverlapSphere(targetPosition, radius, layerMask);

            foreach (Collider col in colliders)
            {
                // Exclude the target object itself
                if (col.gameObject != targetObject && col.gameObject.CompareTag(tag))
                {
                    nearbyObjects.Add(col.gameObject);
                }
            }


            return nearbyObjects;
        }

        public static GameObject GetNearestGameObject(GameObject targetObject, List<GameObject> objectList)
        {
            GameObject nearestObject = null;
            float minDistance = float.MaxValue;

            if (targetObject == null || objectList == null || objectList.Count == 0)
            {
                Debug.LogWarning("Target object is null or object list is empty");
                return nearestObject;
            }

            // Collect all colliders on target (including children)
            Collider[] targetColliders = targetObject.GetComponentsInChildren<Collider>();
            bool hasTargetColliders = targetColliders != null && targetColliders.Length > 0;

            foreach (GameObject candidate in objectList)
            {
                if (candidate == null || candidate == targetObject)
                {
                    continue;
                }

                // Collect all colliders on candidate (including children)
                Collider[] candidateColliders = candidate.GetComponentsInChildren<Collider>();
                bool hasCandidateColliders = candidateColliders != null && candidateColliders.Length > 0;

                float candidateMinDistance = float.MaxValue;

                if (hasTargetColliders && hasCandidateColliders)
                {
                    // Compute minimum distance between any pair of colliders
                    foreach (Collider tc in targetColliders)
                    {
                        foreach (Collider cc in candidateColliders)
                        {
                            // If overlapping, effective distance is 0
                            Vector3 dir;
                            float penetrationDistance;
                            bool overlapped = Physics.ComputePenetration(
                                tc, tc.transform.position, tc.transform.rotation,
                                cc, cc.transform.position, cc.transform.rotation,
                                out dir, out penetrationDistance);

                            if (overlapped)
                            {
                                candidateMinDistance = 0f;
                                // Can't get smaller than 0, so short-circuit
                                break;
                            }
                            else
                            {
                                // Estimate closest separation using ClosestPoint on both colliders
                                Vector3 pointOnTarget = tc.ClosestPoint(cc.bounds.center);
                                Vector3 pointOnCandidate = cc.ClosestPoint(pointOnTarget);
                                float d = Vector3.Distance(pointOnTarget, pointOnCandidate);
                                if (d < candidateMinDistance)
                                {
                                    candidateMinDistance = d;
                                }
                            }
                        }

                        if (candidateMinDistance <= 0f)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    // Fallback to transform distance if either object lacks colliders
                    float d = Vector3.Distance(targetObject.transform.position, candidate.transform.position);
                    candidateMinDistance = d;
                }

                if (candidateMinDistance < minDistance)
                {
                    minDistance = candidateMinDistance;
                    nearestObject = candidate;
                }
            }

            return nearestObject;
        }

        public static List<GameObject> GetNearbyGameObjectsByRaycast(GameObject targetObject, float radius, int rays = 36, int layerMask = -1, string tag = "everyone", float sphereRadius = 0.1f, float height = -1f)
        {
            List<GameObject> nearbyObjects = new List<GameObject>();
            if (targetObject == null)
            {
                Debug.LogWarning("Target object is null");
                return nearbyObjects;
            }
            if (radius <= 0f || rays <= 0)
            {
                return nearbyObjects;
            }

            Vector3 origin = targetObject.transform.position;
            if (height > 0f)
            {
                origin.y = height;
                Debug.Log("origin height set to " + height);
            }
            HashSet<GameObject> seen = new HashSet<GameObject>();

            // Cast rays around the horizontal plane
            for (int i = 0; i < rays; i++)
            {
                float angle = (Mathf.PI * 2f) * (i / (float)rays);
                Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

                if (sphereRadius > 0f)
                {
                    RaycastHit[] hits = Physics.SphereCastAll(origin, sphereRadius, dir, radius, layerMask, QueryTriggerInteraction.Ignore);
                    foreach (RaycastHit hit in hits)
                    {
                        GameObject go = hit.collider != null ? hit.collider.gameObject : null;
                        if (go == null || go == targetObject)
                        {
                            continue;
                        }
                        if (tag != "everyone" && !go.CompareTag(tag))
                        {
                            continue;
                        }
                        if (seen.Add(go))
                        {
                            nearbyObjects.Add(go);
                        }
                    }
                }
                else
                {
                    RaycastHit[] hits = Physics.RaycastAll(origin, dir, radius, layerMask, QueryTriggerInteraction.Ignore);
                    foreach (RaycastHit hit in hits)
                    {
                        GameObject go = hit.collider != null ? hit.collider.gameObject : null;
                        if (go == null || go == targetObject)
                        {
                            continue;
                        }
                        if (tag != "everyone" && !go.CompareTag(tag))
                        {
                            continue;
                        }
                        if (seen.Add(go))
                        {
                            nearbyObjects.Add(go);
                        }
                    }
                }
            }

            return nearbyObjects;
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


    }
    

}

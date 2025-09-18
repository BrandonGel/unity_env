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
            Texture2D tex = new Texture2D(2, 2); // size doesn’t matter, LoadImage will replace it
            tex.LoadImage(fileData); // Loads PNG/JPG bytes into texture
            return tex;
        }
    }
}

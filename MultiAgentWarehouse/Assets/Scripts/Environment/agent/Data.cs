using System.Collections.Generic;
using UnityEngine;
using System;

namespace multiagent.agent
{
    public class subAgentData
    {
        public List<float> entry;
        
        public int size = 0;

        public subAgentData()
        {
            entry = new List<float>();      
        }

        public void add<T>(T value)
        {
            if (typeof(T) == typeof(int))
            {
                float v = (int)(object)value;
                entry.Add(v);
            }
            else if (typeof(T) == typeof(float))
            {
                entry.Add((float)(object)value);
            }
            else if (typeof(T) == typeof(Vector2))
            {
                entry.Add(((Vector2)(object)value).x);
                entry.Add(((Vector2)(object)value).y);
            }
            else if (typeof(T) == typeof(Vector3))
            {
                entry.Add(((Vector3)(object)value).x);
                entry.Add(((Vector3)(object)value).y);
                entry.Add(((Vector3)(object)value).z);
            }
            else if (typeof(T) == typeof(float[]))
            {
                float[] arr = (float[])(object)value;
                foreach (float v in arr)
                {
                    entry.Add(v);
                }
            }
            else if (typeof(T) == typeof(int[]))
            {
                int[] arr = (int[])(object)value;
                foreach (int v in arr)
                {
                    entry.Add((float)v);
                }
            }
            else if (typeof(T) == typeof(Vector2[]))
            {
                Vector2[] arr = (Vector2[])(object)value;
                foreach (Vector2 v in arr)
                {
                    entry.Add(v.x);
                    entry.Add(v.y);
                }
            }
            else if (typeof(T) == typeof(Vector3[]))
            {
                Vector3[] arr = (Vector3[])(object)value;
                foreach (Vector3 v in arr)
                {
                    entry.Add(v.x);
                    entry.Add(v.y);
                    entry.Add(v.z);
                }
            }
            else if (typeof(T) == typeof(Vector3Int))
            {
                entry.Add(((Vector3)(object)value).x);
                entry.Add(((Vector3)(object)value).y);
                entry.Add(((Vector3)(object)value).z);
            }
            else
            {
                throw new System.ArgumentException("Unsupported type");
            }
        }

        public List<float> get()
        {
            return entry;
        }

        public void clear()
        {
            entry.Clear();
        }
    }

    public class agentData
    {
        List<subAgentData> dataEntries;
        public int id;
        public int size;
        public List<String> header = new List<String>();


        public agentData(int id)
        {
            dataEntries = new List<subAgentData>();
            this.id = id;
            size = 0;
        }

        // Add the current data to all entries
        public void addEntry(subAgentData sub)
        {
            dataEntries.Add(sub);
            size += 1;
        }

        // Get a specific data entries for all timesteps
        public subAgentData readEntry(int entryID)
        {
            return dataEntries[entryID];
        }

        public void setHeader(List<String> header)
        {
            this.header = header;
        }

        public void clear()
        {
            size = 0;
            for (int ii = 0; ii < size; ii++)
            {
                dataEntries[ii].clear();
            }
        }

        public void setID(int id)
        {
            this.id = id;
        }

        public int getID()
        {
            return id;
        }

        public int getSize()
        {
            return size;
        }

        public subAgentData getLastEntry()
        {
            if(dataEntries.Count == 0)
            {
                return null;
            }
            return dataEntries[dataEntries.Count - 1];
        }

    }

}

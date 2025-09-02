

using System.Collections.Generic;
using UnityEngine;


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

        public void clear()
        {
            size = 0;
            for (int ii = 0; ii < size; ii++)
            {
                dataEntries[ii].clear();
            }
        }

    }

}

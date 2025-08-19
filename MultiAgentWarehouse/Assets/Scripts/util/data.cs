

using System.Collections.Generic;
using UnityEngine;


namespace multiagent.util
{
    public class Data
    {

        public class subData
        {
            public List<float> currentTime; //position & theta
            public List<Vector3> state; //position & theta
            public List<Vector3> dstate; //linear & angular velocity
            public int size = 0;

            public subData()
            {
                currentTime = new List<float>();
                state = new List<Vector3>();
                dstate = new List<Vector3>();
            }

            public void addSubEntry(float t, Vector3 s, Vector3 ds)
            {
                currentTime.Add(t);
                state.Add(s);
                dstate.Add(ds);
                size += 1;
            }

            public (float, Vector3, Vector3) getEntry(int idx)
            {
                if (idx>= size)
                {
                    Debug.Log(idx + " is greater than the size of the subdata");
                }
                float t = currentTime[idx];
                Vector3 s = state[idx];
                Vector3 ds = dstate[idx];
                return (t, s, ds);
            }

            public void clear()
            {
                currentTime.Clear();
                state.Clear();
                dstate.Clear();
                size = 0;
            }
        }
        subData[] dataEntries;

        public int size;

        public Data(int count)
        {
            dataEntries = new subData[count];
            for (int i = 0; i < count; i++)
            {
                dataEntries[i] = new subData();
            }
            size = count;
        }

        // Add the current data to all entries
        public void addEntry(List<(float,Vector3, Vector3)> entries)
        {
            int numEntries = entries.Count;
            for (int ii = 0; ii < numEntries; ii++)
            {
                (float t, Vector3 s, Vector3 ds) = entries[ii];
                dataEntries[ii].addSubEntry(t, s, ds);

            }
        }

        // Get a specific data entries for all timesteps
        public subData readEntry(int entryID)
        {
            return dataEntries[entryID];
        }

        // Get all data entries at a specified timestep
        public (float, Vector3, Vector3)[] readAtTimestep(int idx)
        {
            (float, Vector3, Vector3)[] dataAtT = new (float, Vector3, Vector3)[size];
            for (int ii = 0; ii < size; ii++)
            {
                (float t, Vector3 s, Vector3 ds) = dataEntries[ii].getEntry(idx);
                dataAtT[ii] = (t, s, ds);
            }

            return dataAtT;
        }

        // Get the a specific data entry at a specified timestep
        public (float, Vector3, Vector3) read(int entryID, int idx)
        {
            subData entry = dataEntries[entryID];
            (float t, Vector3 s, Vector3 ds) = entry.getEntry(idx);
            return (t, s, ds);
        }

        public void clear()
        {
            for (int ii = 0; ii < size; ii++)
            {
                dataEntries[ii].clear();
            }
        }

    }

}

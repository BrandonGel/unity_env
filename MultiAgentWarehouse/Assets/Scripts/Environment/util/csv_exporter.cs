using UnityEngine;
using System.IO;
using System.Collections.Generic;
using multiagent.agent;
using System.Linq;

namespace multiagent.util
{
    public class csv_exporter 
    {
        public List<string[]> dataToExport = new List<string[]>(); // Example data structure

        public void transferData(agentData dataclass, int CurrentEpisode = 1)
        {
            // Initalize the data export
            dataToExport = new List<string[]>();
            if(dataclass.header.Count == 0)
            {
                dataToExport.Add(new string[] { "time", "x", "y", "theta, vx, vy, omega" });
                Debug.LogWarning("Header is empty, adding default header");
            }
            else
            {
                dataToExport.Add(dataclass.header.ToArray());
            }
            

            // Iterate through the time step of the agent data
            for (int tt = 0; tt < dataclass.size; tt++)
            {
                subAgentData dataSubClass = dataclass.readEntry(tt);
                List<float> entry = dataSubClass.get();
                string[] stringEntry = entry.Select(f => f.ToString()).ToArray();
                dataToExport.Add(stringEntry);
            }
            ExportToCSV($"robot{dataclass.id}_episode{CurrentEpisode}");
            
        }

        public void ExportToCSV(string fileName)
        {
            string assetsPath = Application.dataPath;
            string projectPath = Directory.GetParent(assetsPath).FullName;
            string savePath = Path.Combine(projectPath, "data");
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            string filePath = Path.Combine(savePath, fileName + ".csv");

            // Create the header row
            string csvContent = string.Join(",", dataToExport[0]) + "\n";

            // Add data rows
            for (int i = 1; i < dataToExport.Count; i++)
            {
                csvContent += string.Join(",", dataToExport[i]) + "\n";
            }

            File.WriteAllText(filePath, csvContent);
            Debug.Log("CSV exported to: " + filePath);
        }
    }
}

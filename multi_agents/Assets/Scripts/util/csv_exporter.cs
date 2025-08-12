using UnityEngine;
using System.IO;
using System.Collections.Generic;
namespace multiagent.util
{
    public class csv_exporter 
    {
        public List<string[]> dataToExport = new List<string[]>(); // Example data structure

        public void transferData(Data dataclass,int CurrentEpisode = 1)
        {
            Debug.Log("Length: " + dataclass.size);
            for (int ii = 0; ii < dataclass.size; ii++)
            {
                dataToExport = new List<string[]>();
                Data.subData dataSubClass = dataclass.readEntry(ii);
                dataToExport.Add(new string[] { "time","x", "y", "theta, vx, vy, omega" });
                for (int tt = 0; tt < dataSubClass.size; tt++)
                {
                    (float t, Vector3 s, Vector3 ds) = dataSubClass.getEntry(tt);
                    dataToExport.Add(new string[]{
                    t.ToString(),
                    s.x.ToString(),
                    s.y.ToString(),
                    s.z.ToString(),
                    ds.x.ToString(),
                    ds.y.ToString(),
                    ds.z.ToString(),
                    });
                }
                ExportToCSV($"robot{ii}_episode{CurrentEpisode}");
                
            }
            
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

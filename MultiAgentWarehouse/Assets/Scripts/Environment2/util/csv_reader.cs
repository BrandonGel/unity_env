using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using multiagent.agent;
using System.Linq;

namespace multiagent.util
{
    /// <summary>
    /// Represents a position and orientation at a specific time
    /// </summary>
    [System.Serializable]
    public class PositionOrientation
    {
        public float unix_time;
        public float time;
        public float x;
        public float y;
        public float theta; // orientation in radians
        public float vx;
        public float vy;
        public float w;
        public int collisionTagID;
        public int goalCategory;
        public int goalId;
        public int goalType;
        public float xg;
        public float yg;
    }

    public class csv_reader
    {
        public List<string[]> data = new List<string[]>();
        public List<string> header = new List<string>();
        private bool hasHeader = false;
        private bool verbose = true;

        /// <summary>
        /// Reads a CSV file from the specified path
        /// </summary>
        /// <param name="filePath">Full path to the CSV file</param>
        /// <param name="hasHeaderRow">Whether the first row contains headers</param>
        /// <param name="verbose">Whether to display Debug.Log messages (default: true)</param>
        /// <returns>True if file was successfully read, false otherwise</returns>
        public bool ReadCSV(string filePath, bool hasHeaderRow = true, bool verbose = true)
        {
            this.verbose = verbose;

            if (!File.Exists(filePath))
            {
                Debug.LogError($"CSV file not found: {filePath}");
                return false;
            }

            data.Clear();
            header.Clear();
            hasHeader = hasHeaderRow;

            try
            {
                string[] lines = File.ReadAllLines(filePath);

                if (lines.Length == 0)
                {
                    Debug.LogWarning("CSV file is empty");
                    return false;
                }

                // Parse header if present
                if (hasHeaderRow)
                {
                    header = ParseCSVLine(lines[0]).ToList();
                }

                // Parse data rows
                int startIndex = hasHeaderRow ? 1 : 0;
                for (int i = startIndex; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    string[] row = ParseCSVLine(lines[i]);
                    data.Add(row);
                }

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error reading CSV file: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Parses a single CSV line into an array of strings, handling quoted values
        /// </summary>
        private string[] ParseCSVLine(string line)
        {
            List<string> fields = new List<string>();
            bool inQuotes = false;
            string currentField = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote
                        currentField += '"';
                        i++; // Skip next quote
                    }
                    else
                    {
                        // Toggle quote state
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // End of field
                    fields.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }

            // Add the last field
            fields.Add(currentField);

            return fields.ToArray();
        }

        /// <summary>
        /// Gets the number of data rows (excluding header)
        /// </summary>
        public int GetRowCount()
        {
            return data.Count;
        }

        /// <summary>
        /// Gets the number of columns
        /// </summary>
        public int GetColumnCount()
        {
            if (data.Count > 0)
                return data[0].Length;
            if (header.Count > 0)
                return header.Count;
            return 0;
        }

        /// <summary>
        /// Gets a specific row by index
        /// </summary>
        public string[] GetRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= data.Count)
            {
                Debug.LogError($"Row index {rowIndex} is out of range. Total rows: {data.Count}");
                return null;
            }
            return data[rowIndex];
        }

        /// <summary>
        /// Gets a specific cell value
        /// </summary>
        public string GetCell(int rowIndex, int columnIndex)
        {
            string[] row = GetRow(rowIndex);
            if (row == null || columnIndex < 0 || columnIndex >= row.Length)
            {
                Debug.LogError($"Column index {columnIndex} is out of range for row {rowIndex}");
                return null;
            }
            return row[columnIndex];
        }

        /// <summary>
        /// Gets a column by header name
        /// </summary>
        public List<string> GetColumn(string headerName)
        {
            if (!hasHeader)
            {
                Debug.LogError("CSV file does not have a header row");
                return null;
            }

            int columnIndex = header.IndexOf(headerName);
            if (columnIndex == -1)
            {
                Debug.LogError($"Header '{headerName}' not found");
                return null;
            }

            List<string> column = new List<string>();
            foreach (string[] row in data)
            {
                if (columnIndex < row.Length)
                    column.Add(row[columnIndex]);
            }
            return column;
        }

        /// <summary>
        /// Gets a column by index
        /// </summary>
        public List<string> GetColumn(int columnIndex)
        {
            List<string> column = new List<string>();
            foreach (string[] row in data)
            {
                if (columnIndex < row.Length)
                    column.Add(row[columnIndex]);
            }
            return column;
        }

        /// <summary>
        /// Converts CSV data to agentData format
        /// </summary>
        /// <param name="agentId">ID for the agent data</param>
        /// <param name="verbose">Whether to display Debug.Log messages (default: uses instance field)</param>
        /// <returns>agentData object or null if conversion fails</returns>
        public agentData ConvertToAgentData(int agentId, bool? verbose = null)
        {
            bool useVerbose = verbose ?? this.verbose;
            agentData aData = new agentData(agentId);

            // Set header if available
            if (hasHeader && header.Count > 0)
            {
                aData.setHeader(header);
            }

            // Convert each row to subAgentData
            foreach (string[] row in data)
            {
                subAgentData subData = new subAgentData();

                foreach (string value in row)
                {
                    if (float.TryParse(value, out float floatValue))
                    {
                        subData.add<float>(floatValue);
                    }
                    else
                    {
                        if (useVerbose) Debug.LogWarning($"Could not parse value '{value}' as float, skipping");
                    }
                }

                aData.addEntry(subData);
            }

            return aData;
        }

        /// <summary>
        /// Gets all rows as float arrays (converts string values to floats)
        /// </summary>
        public List<float[]> GetRowsAsFloats()
        {
            List<float[]> floatRows = new List<float[]>();

            foreach (string[] row in data)
            {
                List<float> floatRow = new List<float>();
                foreach (string value in row)
                {
                    if (float.TryParse(value, out float floatValue))
                    {
                        floatRow.Add(floatValue);
                    }
                    else
                    {
                        Debug.LogWarning($"Could not parse value '{value}' as float");
                        floatRow.Add(0f);
                    }
                }
                floatRows.Add(floatRow.ToArray());
            }

            return floatRows;
        }

        /// <summary>
        /// Clears all loaded data
        /// </summary>
        public void Clear()
        {
            data.Clear();
            header.Clear();
            hasHeader = false;
        }
    }

    public class csv_data
    {
        private DateTime? firstDateTime = null;

        /// <summary>
        /// Reads all CSV files from a directory and converts each into a separate agentData instance
        /// </summary>
        /// <param name="directoryPath">Path to the directory containing CSV files</param>
        /// <param name="hasHeaderRow">Whether the first row contains headers</param>
        /// <param name="searchPattern">File pattern to search for (default: "*.csv")</param>
        /// <param name="startAgentId">Starting ID for agent data (default: 0)</param>
        /// <param name="verbose">Whether to display Debug.Log messages (default: true)</param>
        /// <returns>Dictionary mapping file names to agentData instances</returns>
        public Dictionary<string, agentData> ReadAllCSVAsAgentData(string directoryPath, bool hasHeaderRow = true, string searchPattern = "*.csv", int startAgentId = 0, bool verbose = true)
        {
            Dictionary<string, agentData> agentDataDict = new Dictionary<string, agentData>();
            Debug.Log("verbose : " + verbose);
            if (!Directory.Exists(directoryPath))
            {
                Debug.LogError($"Directory not found: {directoryPath}");
                return agentDataDict;
            }

            try
            {
                string[] csvFiles = Directory.GetFiles(directoryPath, searchPattern);
                
                if (csvFiles.Length == 0)
                {
                    Debug.LogWarning($"No CSV files found in directory: {directoryPath}");
                    return agentDataDict;
                }

                if (verbose) Debug.Log($"Found {csvFiles.Length} CSV file(s) in directory");

                int agentId = startAgentId;
                int filesRead = 0;

                foreach (string filePath in csvFiles)
                {
                    try
                    {
                        // Use csv_reader to read each file
                        csv_reader reader = new csv_reader();
                        if (!reader.ReadCSV(filePath, hasHeaderRow, verbose))
                        {
                            Debug.LogWarning($"Failed to read CSV file: {filePath}");
                            continue;
                        }

                        // Convert this file's data to agentData
                        agentData aData = reader.ConvertToAgentData(agentId, verbose);
                        
                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        agentDataDict[fileName] = aData;
                        
                        filesRead++;
                        agentId++;
                        if (verbose) Debug.Log($"Converted {fileName} to agentData with ID {aData.getID()}, {aData.getSize()} entries");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error processing CSV file {filePath}: {e.Message}");
                        continue;
                    }
                }

                if (filesRead == 0)
                {
                    Debug.LogError("Failed to read any CSV files from directory");
                }
                else
                {
                    if (verbose) Debug.Log($"Successfully converted {filesRead} file(s) to agentData");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error reading CSV files from directory: {e.Message}");
            }

            return agentDataDict;
        }

        /// <summary>
        /// Reads all CSV files from a directory and converts the first entry (robot position) to AgentData format
        /// </summary>
        /// <param name="directoryPath">Path to the directory containing CSV files</param>
        /// <param name="hasHeaderRow">Whether the first row contains headers</param>
        /// <param name="searchPattern">File pattern to search for (default: "*.csv")</param>
        /// <param name="verbose">Whether to display Debug.Log messages (default: true)</param>
        /// <returns>List of AgentData objects with robot names and start positions</returns>
        public List<AgentData> ReadAllCSVFirstPositionAsAgentData(string directoryPath, bool hasHeaderRow = true, string searchPattern = "*.csv", bool verbose = true)
        {
            List<AgentData> agentDataList = new List<AgentData>();

            if (!Directory.Exists(directoryPath))
            {
                Debug.LogError($"Directory not found: {directoryPath}");
                return agentDataList;
            }

            try
            {
                string[] csvFiles = Directory.GetFiles(directoryPath, searchPattern);
                
                if (csvFiles.Length == 0)
                {
                    Debug.LogWarning($"No CSV files found in directory: {directoryPath}");
                    return agentDataList;
                }

                if (verbose) Debug.Log($"Found {csvFiles.Length} CSV file(s) in directory");

                int filesRead = 0;

                foreach (string filePath in csvFiles)
                {
                    try
                    {
                        // Use csv_reader to read each file
                        csv_reader reader = new csv_reader();
                        if (!reader.ReadCSV(filePath, hasHeaderRow, verbose))
                        {
                            Debug.LogWarning($"Failed to read CSV file: {filePath}");
                            continue;
                        }

                        // Check if we have data (header is already skipped by ReadCSV)
                        if (reader.data.Count == 0)
                        {
                            Debug.LogWarning($"CSV file has no data rows: {filePath}");
                            continue;
                        }

                        // Find x and y column indices from header
                        int xIndex = -1;
                        int yIndex = -1;

                        if (hasHeaderRow && reader.header.Count > 0)
                        {
                            // Use header to find 'x' and 'y' column indices
                            xIndex = reader.header.IndexOf("x");
                            yIndex = reader.header.IndexOf("y");
                        }
                        else
                        {
                            // If no header, assume x and y are at indices 1 and 2 (after time at index 0)
                            xIndex = 1;
                            yIndex = 2;
                        }

                        if (xIndex == -1 || yIndex == -1)
                        {
                            Debug.LogWarning($"Could not find 'x' or 'y' columns in CSV file: {filePath}");
                            continue;
                        }

                        // Get first data row (header is already skipped by ReadCSV, so data[0] is the first actual data row)
                        string[] firstRow = reader.data[0];
                        
                        if (firstRow.Length <= System.Math.Max(xIndex, yIndex))
                        {
                            Debug.LogWarning($"CSV file does not have enough columns: {filePath}");
                            continue;
                        }

                        // Parse x and y coordinates
                        if (float.TryParse(firstRow[xIndex], out float xFloat) && 
                            float.TryParse(firstRow[yIndex], out float yFloat))
                        {
                            float x = xFloat;
                            float y = yFloat;

                            // Create AgentData object
                            AgentData agentData = new AgentData();
                            agentData.name = Path.GetFileNameWithoutExtension(filePath);
                            agentData.start = new float[] { x, y };

                            agentDataList.Add(agentData);
                            filesRead++;
                            if (verbose) Debug.Log($"Extracted position from {agentData.name}: [{x}, {y}]");
                        }
                        else
                        {
                            Debug.LogWarning($"Could not parse position values from CSV file: {filePath}");
                            continue;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error processing CSV file {filePath}: {e.Message}");
                        continue;
                    }
                }

                if (filesRead == 0)
                {
                    Debug.LogError("Failed to extract positions from any CSV files");
                }
                else
                {
                    if (verbose) Debug.Log($"Successfully extracted positions from {filesRead} file(s)");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error reading CSV files from directory: {e.Message}");
            }

            return agentDataList;
        }

        /// <summary>
        /// Reads all CSV files from a directory and extracts all positions and orientations from each file
        /// </summary>
        /// <param name="directoryPath">Path to the directory containing CSV files</param>
        /// <param name="hasHeaderRow">Whether the first row contains headers</param>
        /// <param name="searchPattern">File pattern to search for (default: "*.csv")</param>
        /// <param name="verbose">Whether to display Debug.Log messages (default: true)</param>
        /// <returns>Dictionary mapping file names (without extension) to lists of PositionOrientation objects</returns>
        public Dictionary<string, List<PositionOrientation>> ReadAllCSVPositionsAndOrientations(string directoryPath, bool hasHeaderRow = true, string searchPattern = "*.csv", bool verbose = true)
        {
            Dictionary<string, List<PositionOrientation>> positionsDict = new Dictionary<string, List<PositionOrientation>>();

            if (!Directory.Exists(directoryPath))
            {
                Debug.LogError($"Directory not found: {directoryPath}");
                return positionsDict;
            }
            try
            {
                string[] csvFiles = Directory.GetFiles(directoryPath, searchPattern);

                if (csvFiles.Length == 0)
                {
                    Debug.LogWarning($"No CSV files found in directory: {directoryPath}");
                    return positionsDict;
                }

                if (verbose) Debug.Log($"Found {csvFiles.Length} CSV file(s) in directory");

                int filesRead = 0;

                foreach (string filePath in csvFiles)
                {
                    try
                    {
                        // Use csv_reader to read each file
                        csv_reader reader = new csv_reader();
                        if (!reader.ReadCSV(filePath, hasHeaderRow, verbose))
                        {
                            Debug.LogWarning($"Failed to read CSV file: {filePath}");
                            continue;
                        }

                        // Check if we have data (header is already skipped by ReadCSV)
                        if (reader.data.Count == 0)
                        {
                            Debug.LogWarning($"CSV file has no data rows: {filePath}");
                            continue;
                        }

                        // Find column indices from header
                        int timeIndex = -1;
                        int xIndex = -1;
                        int yIndex = -1;
                        int thetaIndex = -1;
                        int vxIndex = -1;
                        int vyIndex = -1;
                        int wIndex = -1;
                        int collisionTagIDIndex = -1;
                        int goalCategoryIndex = -1;
                        int goalIdIndex = -1;
                        int goalTypeIndex = -1;
                        int xgIndex = -1;
                        int ygIndex = -1;
                        if (hasHeaderRow && reader.header.Count > 0)
                        {
                            // Use header to find column indices
                            timeIndex = reader.header.IndexOf("time");
                            // Check if header contains "time" (case-sensitive)
                            bool hasTimeColumn = reader.header.Contains("time");
                            bool hasDatetimeColumn = reader.header.Contains("datatime");
                            if (hasTimeColumn)
                            {
                                timeIndex = reader.header.IndexOf("time");
                            }
                            else if (hasDatetimeColumn)
                            {
                                timeIndex = reader.header.IndexOf("datatime");
                            }
                            else
                            {
                                timeIndex = 0;
                            }
                            xIndex = reader.header.IndexOf("x");
                            yIndex = reader.header.IndexOf("y");
                            thetaIndex = reader.header.IndexOf("theta");
                            vxIndex = reader.header.IndexOf("vx");
                            vyIndex = reader.header.IndexOf("vy");
                            wIndex = reader.header.IndexOf("w");
                            collisionTagIDIndex = reader.header.IndexOf("collisionTagID");
                            goalCategoryIndex = reader.header.IndexOf("goalCategory");
                            goalIdIndex = reader.header.IndexOf("goalID");
                            goalTypeIndex = reader.header.IndexOf("goalType");
                            xgIndex = reader.header.IndexOf("xg");
                            ygIndex = reader.header.IndexOf("yg");
                        }
                        else
                        {
                            // If no header, assume standard order: time, x, y, theta
                            timeIndex = 0;
                            xIndex = 1;
                            yIndex = 2;
                            thetaIndex = 3;
                            vxIndex = 4;
                            vyIndex = 5;
                            wIndex = 6;
                            collisionTagIDIndex = 7;
                            goalCategoryIndex = 8;
                            goalIdIndex = 9;
                            goalTypeIndex = 10;
                            xgIndex = 11;
                            ygIndex = 12;
                        }

                        if (xIndex == -1 || yIndex == -1)
                        {
                            Debug.LogWarning($"Could not find 'x' or 'y' columns in CSV file: {filePath}");
                            continue;
                        }

                        // If theta column not found, warn but continue (theta will be 0)
                        if (thetaIndex == -1)
                        {
                            if (verbose) Debug.LogWarning($"Could not find 'theta' column in CSV file: {filePath}, using 0 as default");
                        }

                        List<PositionOrientation> positions = new List<PositionOrientation>();

                        // Extract all positions and orientations from all rows
                        foreach (string[] row in reader.data)
                        {
                            if (row.Length <= System.Math.Max(System.Math.Max(xIndex, yIndex), thetaIndex != -1 ? thetaIndex : 0))
                            {
                                if (verbose) Debug.LogWarning($"Row has insufficient columns, skipping");
                                continue;
                            }

                            // Parse time (optional, default to row index if not found)
                            float time = 0f;
                            float unix_time = 0f;
                            if (timeIndex != -1 && timeIndex < row.Length)
                            {
                                if (!float.TryParse(row[timeIndex], out time))
                                {
                                    if (DateTime.TryParse(row[timeIndex], out DateTime dateTime))
                                    {
                                        // Save the first datetime value as the t0 for offsetting subsequent times
                                        if (firstDateTime == null)
                                        {
                                            firstDateTime = dateTime;
                                        }
                                        unix_time =  (float)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                                        time = (float)(dateTime - firstDateTime.Value).TotalSeconds;
                                    }
                                    else
                                    {
                                        time = 0f;
                                        unix_time = 0f;
                                    }
                                }
                            }

                            // Parse x and y coordinates
                            if (float.TryParse(row[xIndex], out float x) &&
                                float.TryParse(row[yIndex], out float y))
                            {
                                // Parse theta (orientation), default to 0 if not found or not parseable
                                float theta = 0f;
                                float vx = 0f;
                                float vy = 0f;
                                float w = 0f;
                                int collisionTagID = 0;
                                int goalId = 0;
                                int goalCategory = -1;
                                int goalType = -1;
                                float xg = 0f;
                                float yg = 0f;
                                if (thetaIndex != -1 && thetaIndex < row.Length)
                                {
                                    if (!float.TryParse(row[thetaIndex], out theta))
                                    {
                                        theta = 0f;
                                    }
                                }
                                if (vxIndex != -1 && vxIndex < row.Length)
                                {
                                    if (!float.TryParse(row[vxIndex], out vx))
                                    {
                                        vx = 0f;
                                    }
                                }
                                if (vyIndex != -1 && vyIndex < row.Length)
                                {
                                    if (!float.TryParse(row[vyIndex], out vy))
                                    {
                                        vy = 0f;
                                    }
                                }
                                if (wIndex != -1 && wIndex < row.Length)
                                {
                                    if (!float.TryParse(row[wIndex], out w))
                                    {
                                        w = 0f;
                                    }
                                }
                                if (collisionTagIDIndex != -1 && collisionTagIDIndex < row.Length)
                                {
                                    if (!int.TryParse(row[collisionTagIDIndex], out collisionTagID))
                                    {
                                        collisionTagID = -1;
                                    }
                                }
                                if (goalCategoryIndex != -1 && goalCategoryIndex < row.Length)
                                {
                                    if (!int.TryParse(row[goalCategoryIndex], out goalCategory))
                                    {
                                        goalCategory = -1;
                                    }
                                }
                                if (goalIdIndex != -1 && goalIdIndex < row.Length)
                                {
                                    if (!int.TryParse(row[goalIdIndex], out goalId))
                                    {
                                        goalId = -1;
                                    }
                                }
                                if (goalTypeIndex != -1 && goalTypeIndex < row.Length)
                                {
                                    if (!int.TryParse(row[goalTypeIndex], out goalType))
                                    {
                                        goalType = -1;
                                    }
                                }
                                if (xgIndex != -1 && xgIndex < row.Length)
                                {
                                    if (!float.TryParse(row[xgIndex], out xg))
                                    {
                                        xg = 0f;
                                    }
                                }
                                if (ygIndex != -1 && ygIndex < row.Length)
                                {
                                    if (!float.TryParse(row[ygIndex], out yg))
                                    {
                                        yg = 0f;
                                    }
                                }
                                PositionOrientation pos = new PositionOrientation
                                {
                                    unix_time = unix_time,
                                    time = time,
                                    x = x,
                                    y = y,
                                    theta = theta,
                                    vx = vx,
                                    vy = vy,
                                    w = w,
                                    collisionTagID = collisionTagID,
                                    goalCategory = goalCategory,
                                    goalId = goalId,
                                    goalType = goalType,
                                    xg = xg,
                                    yg = yg
                                };
                                positions.Add(pos);
                            }
                            else
                            {
                                if (verbose) Debug.LogWarning($"Could not parse position values from row in CSV file: {filePath}");
                            }
                        }

                        if (positions.Count > 0)
                        {
                            string fileName = Path.GetFileNameWithoutExtension(filePath);
                            positionsDict[fileName] = positions;
                            filesRead++;
                            if (verbose) Debug.Log($"Extracted {positions.Count} positions from {fileName}");
                        }
                        else
                        {
                            Debug.LogWarning($"No valid positions extracted from CSV file: {filePath}");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error processing CSV file {filePath}: {e.Message}");
                        continue;
                    }
                }

                if (filesRead == 0)
                {
                    Debug.LogError("Failed to extract positions from any CSV files");
                }
                else
                {
                    if (verbose) Debug.Log($"Successfully extracted positions from {filesRead} file(s)");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error reading CSV files from directory: {e.Message}");
            }

            return positionsDict;
        }
    
        public int CountRobotCSVFiles(string directoryPath, string searchPattern = "*.csv", string filenamePrefixFilter = null, bool verbose = true)
        {
            if (!Directory.Exists(directoryPath))
            {
                Debug.LogError($"Directory not found: {directoryPath}");
                return 0;
            }

            try
            {
                string[] csvFiles = Directory.GetFiles(directoryPath, searchPattern);

                if (!string.IsNullOrEmpty(filenamePrefixFilter))
                {
                    csvFiles = csvFiles.Where(f => Path.GetFileNameWithoutExtension(f).StartsWith(filenamePrefixFilter)).ToArray();
                }

                int count = csvFiles.Length;
                if (verbose) Debug.Log($"Found {count} CSV file(s) in directory '{directoryPath}' (filter: '{filenamePrefixFilter ?? "none"}')");
                return count;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error counting CSV files in directory: {e.Message}");
                return 0;
            }
        }
    }
}

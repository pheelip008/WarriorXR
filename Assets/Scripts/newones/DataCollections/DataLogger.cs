using UnityEngine;
using System.IO;

public class DataLogger : MonoBehaviour
{
    public float logInterval = 1.0f; // seconds

    float timer = 0f;
    float startTime;

    StreamWriter writer;
    HeadZoneDetector zoneDetector;

    void Start()
    {
        zoneDetector = FindObjectOfType<HeadZoneDetector>();
        startTime = Time.time;

        string basePath = Application.persistentDataPath;
        string folderPath = Path.Combine(basePath, "StudyLogs", ExperimentConfig.Instance.participantID);
        Directory.CreateDirectory(folderPath);

        string fileName =
            ExperimentConfig.Instance.participantID + "_" +
            ExperimentConfig.Instance.condition + "_" +
            ExperimentConfig.Instance.trialNumber + ".csv";

        string fullPath = Path.Combine(folderPath, fileName);

        writer = new StreamWriter(fullPath, false);
        writer.WriteLine("Participant,Condition,Trial,Time,Zone");

        Debug.Log("Logging to: " + fullPath);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= logInterval)
        {
            timer = 0f;
            LogData();
        }
    }

    void LogData()
    {
        float timestamp = Time.time - startTime;
        string zone = GetZoneLabel();

        ExperimentConfig cfg = ExperimentConfig.Instance;

        string line =
            cfg.participantID + "," +
            cfg.condition + "," +
            cfg.trialNumber + "," +
            timestamp.ToString("F2") + "," +
            zone;

        writer.WriteLine(line);
    }

    string GetZoneLabel()
    {
        if (zoneDetector.inLeftZone && zoneDetector.inRightZone)
            return "Both";

        if (zoneDetector.inLeftZone)
            return "Left";

        if (zoneDetector.inRightZone)
            return "Right";

        return "None";
    }

    void OnApplicationQuit()
    {
        if (writer != null)
        {
            writer.Flush();
            writer.Close();
        }
    }
}

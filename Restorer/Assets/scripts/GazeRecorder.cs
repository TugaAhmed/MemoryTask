using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Tobii.XR;

[System.Serializable]
public class GazeDataPoint
{
    //Stores one frame of gaze data from the Tobii XR eye tracker
    public float time;
    public Vector3 gazeHitPoint;
    public string objectName;

    public Vector3 gazeOrigin;
    public Vector3 gazeDirection;

    public bool gazeIsValid;
    public bool isLeftEyeBlinking;
    public bool isRightEyeBlinking;

    public int levelIndex; //  Track level from SimpleMemoryGame
}


[System.Serializable]
public class BlinkEvent
{
    //Stores blink events separately
    public float time;
    public string eye; // "Left", "Right", or "Both"
    public int level;
}

public class GazeRecorder : MonoBehaviour
{
    [Header("Tobii Gaze Recorder")]
    public bool isRecording = true;
   

    [Header("Reference to SimpleMemoryGame")]
    public SimpleMemoryGame memoryGame; //  Assign in Inspector

    private List<GazeDataPoint> gazeDataList = new List<GazeDataPoint>();
   


    private List<BlinkEvent> blinkEvents = new List<BlinkEvent>();


    private bool lastLeftBlink = false;
    private bool lastRightBlink = false;


    void Update()
    {
        //If recording is enabled, it captures gaze and blink data
        if (!isRecording) return;

        RecordGazeData(); // Record data every frame
    }

    void RecordGazeData()
    {
        //Fetches current Tobii eye tracking data.
        var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);

        // ********************** blinks 
        bool leftBlink = eyeTrackingData.IsLeftEyeBlinking;
        bool rightBlink = eyeTrackingData.IsRightEyeBlinking;

        float currentTime = Time.time;

        // Detect start of blink (transition from false to true)
        if ((!lastLeftBlink && leftBlink) && (!lastRightBlink && rightBlink))
        {
            blinkEvents.Add(new BlinkEvent { time = currentTime, eye = "Both", level = memoryGame != null ? memoryGame.currentLevelIndex : -1 });
        }
        else
        {
            if (!lastLeftBlink && leftBlink)
            {
                blinkEvents.Add(new BlinkEvent { time = currentTime, eye = "Left", level = memoryGame != null ? memoryGame.currentLevelIndex : -1 });
            }
            if (!lastRightBlink && rightBlink)
            {
                blinkEvents.Add(new BlinkEvent { time = currentTime, eye = "Right", level = memoryGame != null ? memoryGame.currentLevelIndex : -1 });
            }
        }

        lastLeftBlink = leftBlink;
        lastRightBlink = rightBlink;


        //******************************************

        GazeDataPoint data = new GazeDataPoint
        {
            time = Time.time,
            gazeOrigin = eyeTrackingData.GazeRay.Origin,
            gazeDirection = eyeTrackingData.GazeRay.Direction,
            gazeIsValid = eyeTrackingData.GazeRay.IsValid,
            isLeftEyeBlinking = eyeTrackingData.IsLeftEyeBlinking,
            isRightEyeBlinking = eyeTrackingData.IsRightEyeBlinking,
            levelIndex = memoryGame != null ? memoryGame.currentLevelIndex : -1 //  Safe fallback
        };

        //raycasting to check if gaze hits an object in the scene, recording the hit point and object name
        if (eyeTrackingData.GazeRay.IsValid)
        {
            Ray gazeRay = new Ray(eyeTrackingData.GazeRay.Origin, eyeTrackingData.GazeRay.Direction);
            if (Physics.Raycast(gazeRay, out RaycastHit hitInfo))
            {
                data.gazeHitPoint = hitInfo.point;
                data.objectName = hitInfo.collider.gameObject.name;
            }
        }

        gazeDataList.Add(data);
    }

    void OnApplicationQuit()
    {
        SaveToCSV("GazeHitData.csv");
        SaveBlinkDataCSV("BlinkEvents.csv");
    }


    void SaveToCSV(string fileName)
    {
        // Parent of Game_Data (in build) or Assets (in editor)
        string gameFolder = Directory.GetParent(Application.dataPath).FullName;

        // Target folder next to the .exe
        string dataFolder = Path.Combine(gameFolder, "data");

        // Ensure folder exists
        if (!Directory.Exists(dataFolder))
            Directory.CreateDirectory(dataFolder);

        // Final file path
        string filePath = Path.Combine(dataFolder, fileName);

        StringBuilder csv = new StringBuilder();
        csv.AppendLine("time;gazeOrigin_x;gazeOrigin_y;gazeOrigin_z;gazeDirection_x;gazeDirection_y;gazeDirection_z;gazeIsValid;" +
            "isLeftEyeBlinking;isRightEyeBlinking;gaze_hit_x;gaze_hit_y;gaze_hit_z;object_name;level");

        foreach (var data in gazeDataList)
        {
            csv.AppendLine($"{data.time};" +
                           $"{data.gazeOrigin.x};{data.gazeOrigin.y};{data.gazeOrigin.z};" +
                           $"{data.gazeDirection.x};{data.gazeDirection.y};{data.gazeDirection.z};" +
                           $"{data.gazeIsValid};{data.isLeftEyeBlinking};{data.isRightEyeBlinking};" +
                           $"{data.gazeHitPoint.x};{data.gazeHitPoint.y};{data.gazeHitPoint.z};" +
                           $"{data.objectName};{data.levelIndex}");
        }

        File.WriteAllText(filePath, csv.ToString());
        Debug.Log($" Gaze data saved to: {filePath}");
    }

    void SaveBlinkDataCSV(string fileName)
    {
        string gameFolder = Directory.GetParent(Application.dataPath).FullName;
        string dataFolder = Path.Combine(gameFolder, "data");

        if (!Directory.Exists(dataFolder))
            Directory.CreateDirectory(dataFolder);

        string filePath = Path.Combine(dataFolder, fileName);

        StringBuilder csv = new StringBuilder();
        csv.AppendLine("time;eye;level");

        foreach (var blink in blinkEvents)
        {
            csv.AppendLine($"{blink.time};{blink.eye};{blink.level}");
        }

        File.WriteAllText(filePath, csv.ToString());
        Debug.Log($" Blink events saved to: {filePath}");
    }


}

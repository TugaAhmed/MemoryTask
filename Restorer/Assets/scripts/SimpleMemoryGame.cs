using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;

[System.Serializable]
public class LevelPerformance
{
    public int levelIndex;
    public float selectionStartTime;
    public float selectionEndTime;
    public float duration;
    public List<string> answers = new List<string>();
    public List<string> selectionTimestamps = new List<string>(); // NEW



    public string GetAccuracyString()
    {
        /// <summary>
        /// Returns all recorded answers (“yes/no”) in the level as a formatted comma-separated string.
        /// </summary>
        /// 
        return $"({string.Join(",", answers)})";
    }

    public string GetSelectionTimeString()
    {   /// <summary>
        ///Returns all timestamps for each selection in the level as a formatted comma-separated string.
        /// </summary>
        /// 
        return $"({string.Join(",", selectionTimestamps)})";
    }
}

public class SimpleMemoryGame : MonoBehaviour
{
    public List<GameObject> levelRoots;
    public Slider progressBar;

    [Header("Timing Settings")]
    [Tooltip("How long the memorisation phase lasts before spots are hidden.")]
    public float memorisationTime = 8f;   // default

    [Tooltip("Time allowed per hidden spot during the selection phase.")]
    public float timePerHiddenSpot = 5f;  // default

    public int currentLevelIndex = 0;
    private GameObject currentLevel;
    private List<GameObject> spots = new List<GameObject>();
    private List<GameObject> options = new List<GameObject>();
    private List<int> hiddenSpotIndices = new List<int>();
    private Dictionary<int, Texture> spotImageMap = new Dictionary<int, Texture>();
    private int currentHiddenSpotIndex = 0;

    private Coroutine pulseCoroutine = null;
    private Coroutine levelTimerCoroutine = null;

    private float levelTimeLimit ;

    private List<LevelPerformance> allLevelPerformances = new List<LevelPerformance>();
    private LevelPerformance currentLevelPerformance;

    public TextMeshProUGUI levelText;

    void Start()
    {
     
        // Initializes the game by loading the first level.
        LoadLevel(currentLevelIndex);
    }

    void LoadLevel(int index)
    {
        // Prepares and activates the specified level: sets UI, resets variables, loads spots and options
        // caches images, and starts the memory phase.
        
        if (index >= levelRoots.Count)
        {
            //Debug.Log("All levels completed!");
            SavePerformanceData();
            return;
        }
        
        if (levelText != null)
        {
            levelText.text = $"Level {index}";
        }
        
        currentLevel = levelRoots[index];
        currentLevel.SetActive(true);

        progressBar.value = 0f;

        spots.Clear();
        options.Clear();
        spotImageMap.Clear();
        currentHiddenSpotIndex = 0;

        //"SpotContainer" contains all the spots (places where images appear for memorisation)
        Transform spotContainer = currentLevel.transform.Find("SpotContainer");
        // "OptionContainer" contains all the option cubes (choices for the player for selection)
        Transform optionContainer = currentLevel.transform.Find("OptionContainer");

        foreach (Transform child in spotContainer)
            spots.Add(child.gameObject);

        foreach (Transform child in optionContainer)
        {
            options.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }

        hiddenSpotIndices = currentLevel.GetComponent<LevelConfig>().hiddenSpotIndices;
      
        levelTimeLimit = hiddenSpotIndices.Count * timePerHiddenSpot;
       
        CacheSpotImages();
        StartCoroutine(MemoryPhase());
    }

    void CacheSpotImages()
    {
        // Stores the original textures of all spots so the game can check correctness later

        for (int i = 0; i < spots.Count; i++)
        {
            Renderer rend = spots[i].GetComponent<Renderer>();
            Texture tex = rend.material.mainTexture;
            if (tex != null)
                spotImageMap[i] = tex;
            else
                Debug.LogWarning($"Spot {i} has no texture assigned!");
        }
    }

    IEnumerator MemoryPhase()
    {
        // Handles the memorisation phase and transition to the selection phase for a level
        // controls the timing and setup of the level's gameplay.

        yield return new WaitForSeconds(memorisationTime);
        HideSelectedSpots();
        ShowOptions();
        HighlightNextHiddenSpot();

        // Records start time and prepares lists to store answers and answers timestamps
        currentLevelPerformance = new LevelPerformance();
        currentLevelPerformance.levelIndex = currentLevelIndex;
        currentLevelPerformance.selectionStartTime = Time.time;

        //Ensures each hidden spot has a placeholder entry for future player selections
        for (int i = 0; i < hiddenSpotIndices.Count; i++)
        {
            currentLevelPerformance.answers.Add("none");
            currentLevelPerformance.selectionTimestamps.Add("none");
        }

        //Start selection phase countdown timer
        if (levelTimerCoroutine != null) StopCoroutine(levelTimerCoroutine);
        levelTimerCoroutine = StartCoroutine(LevelCountdownTimer(levelTimeLimit));
    }

    void HideSelectedSpots()
    {
        //Clears the textures of hidden spots so the player must remember them.
        foreach (int i in hiddenSpotIndices)
        {
            spots[i].GetComponent<Renderer>().material.mainTexture = null;
        }
    }

    void ShowOptions()
    {
        //Makes the option cubes visible for the player to select from.
        foreach (var option in options)
            option.SetActive(true);
    }

    void HighlightNextHiddenSpot()
    {
        // Starts pulsing animation to indicate which spot the player should fill next
        if (currentHiddenSpotIndex >= hiddenSpotIndices.Count)
        {
            //Debug.Log(" All spots filled!");
            CompleteLevelEarly();
            return;
        }

        int spotIndex = hiddenSpotIndices[currentHiddenSpotIndex];
        GameObject spot = spots[spotIndex];

        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseSpotLoop(spot));
    }

    public void OnOptionSelected(GameObject optionCube)
    {
        //Handles the player's selection of an option cube during the selection phase
        if (currentHiddenSpotIndex >= hiddenSpotIndices.Count) return;

        //Identifies which spot the player is filling and the texture they selected.
        int spotIndex = hiddenSpotIndices[currentHiddenSpotIndex];
        GameObject spot = spots[spotIndex];
        Texture selectedTexture = optionCube.GetComponent<Renderer>().material.mainTexture;

        //Apply the selected texture to the spot
        spot.GetComponent<Renderer>().material.mainTexture = selectedTexture;

        //Record selection time
        float selectionTime = Time.time - currentLevelPerformance.selectionStartTime;
        string formattedTime = selectionTime.ToString("F2");
        currentLevelPerformance.selectionTimestamps[currentHiddenSpotIndex] = formattedTime;

        //Check correctness of the selection
        int isCorrect = (selectedTexture == spotImageMap[spotIndex]) ? 1 : 0;
        if (isCorrect == 1)
        {
            // Update answer at correct index
            currentLevelPerformance.answers[currentHiddenSpotIndex] = "yes";
        }
        else
        {
            currentLevelPerformance.answers[currentHiddenSpotIndex] = "no";
        }

        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);

        //Advance to the next hidden spot
        currentHiddenSpotIndex++;
        HighlightNextHiddenSpot();
    }



    void CompleteLevelEarly()
    {
        //Ends the level immediately if the player fills all empty spots before the timer
        //records performance, and transitions to the next level
        if (levelTimerCoroutine != null) StopCoroutine(levelTimerCoroutine);

        currentLevelPerformance.selectionEndTime = Time.time;
        currentLevelPerformance.duration = currentLevelPerformance.selectionEndTime - currentLevelPerformance.selectionStartTime;

        allLevelPerformances.Add(currentLevelPerformance);
        StartCoroutine(AdvanceLevel());
    }

    IEnumerator AdvanceLevel()
    {
        //Disables the current level, updates the index, and loads the next level.
        yield return null;

        progressBar.value = 0f;
        progressBar.gameObject.SetActive(false);

        currentLevel.SetActive(false);
        currentLevelIndex++;
        LoadLevel(currentLevelIndex);
    }

    IEnumerator PulseSpotLoop(GameObject spot)
    {
        //Creates a looping pulsing animation for the current empty spot (grow/shrink effect).
        Vector3 originalScale = spot.transform.localScale;
        Vector3 enlargedScale = originalScale * 1.15f;
        float speed = 2f;

        while (true)
        {
            float t = 0f;
            while (t < 1f)
            {
                spot.transform.localScale = Vector3.Lerp(originalScale, enlargedScale, t);
                t += Time.deltaTime * speed;
                yield return null;
            }

            t = 0f;
            while (t < 1f)
            {
                spot.transform.localScale = Vector3.Lerp(enlargedScale, originalScale, t);
                t += Time.deltaTime * speed;
                yield return null;
            }
        }
    }

    IEnumerator LevelCountdownTimer(float duration)
    {
        //Runs the countdown timer for the level, updates the progress bar, and ends the level when time runs out

        float timeLeft = duration;
        progressBar.gameObject.SetActive(true);

        while (timeLeft > 0f)
        {
            progressBar.value = 1f - (timeLeft / duration);
            timeLeft -= Time.deltaTime;
            yield return null;

            if (currentHiddenSpotIndex >= hiddenSpotIndices.Count)
                yield break;
        }

        currentLevelPerformance.selectionEndTime = Time.time;
        currentLevelPerformance.duration = currentLevelPerformance.selectionEndTime - currentLevelPerformance.selectionStartTime;

        allLevelPerformances.Add(currentLevelPerformance);

        StartCoroutine(AdvanceLevel());
    }

    void OnApplicationQuit()
    {
        SavePerformanceData();
    }

    void SavePerformanceData()
    {
        // Writes all level performance data into a CSV file located in GameFolder/data/
        
        string gameFolder = Directory.GetParent(Application.dataPath).FullName;
        string dataFolder = Path.Combine(gameFolder, "data");

        // Ensure folder exists
        if (!Directory.Exists(dataFolder))
            Directory.CreateDirectory(dataFolder);

        // File path
        string filePath = Path.Combine(dataFolder, "UserPerformance.csv");

        // Build CSV
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Level;StartTime;EndTime;Duration;Accuracy;SelectionTimes");

        foreach (var perf in allLevelPerformances)
        {
            csv.AppendLine($"{perf.levelIndex};{perf.selectionStartTime:F2};{perf.selectionEndTime:F2};{perf.duration:F2};{perf.GetAccuracyString()};{perf.GetSelectionTimeString()}");
        }

        // Write file
        File.WriteAllText(filePath, csv.ToString());
        Debug.Log($"Performance data saved to: {filePath}");
    }


}

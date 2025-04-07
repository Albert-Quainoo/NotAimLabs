using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class CountdownTimer : MonoBehaviour
{
    public static CountdownTimer Instance;
    private float currentTime = 0f;
    public float startingtime = 60f;

    // UI Elements
    public GameObject trObject;
    public GameObject AccuracyObject;
    public GameObject ScoreObject;
    public GameObject Crosshair;
    public GameObject Finalscore;
    public GameObject ClickToRestart;
    public Text CountdownText;
    public Text finalScoreText;
    public GameObject PauseMenu;
    public GameObject DimOverlay;
    public Button restartButton;

    // Timer Visuals
    [SerializeField] private Color normalTimerColor = Color.white;
    [SerializeField] private Color warningTimerColor = Color.red;
    [SerializeField] private float warningThreshold = 10f;
    private bool isWarningActive = false;

    // Game State
    [SerializeField] public Score score;
    public bool isGameOver { get; private set; }
    private bool canRestart = false;
    public Target target;

    // List to store possible crosshairs found during search
    private List<GameObject> possibleCrosshairs = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ResetTimer();
        isGameOver = false;

        // Hide end-game UI at start
        if (Finalscore != null) Finalscore.SetActive(false);
        if (ClickToRestart != null) ClickToRestart.SetActive(false);

        // Show in-game UI
        if (ScoreObject != null) ScoreObject.SetActive(true);
        if (AccuracyObject != null) AccuracyObject.SetActive(true);

      
        if (Crosshair == null)
        {
            FindCrosshair();
        }
        else
        {
            Debug.Log($"Using inspector-assigned crosshair: {Crosshair.name}");
        }

        // Cache the reference for future use, even if this component gets disabled
        if (Crosshair != null)
        {
            Crosshair.SetActive(true);

            // Store the crosshair name in PlayerPrefs as a fallback
            PlayerPrefs.SetString("LastCrosshairName", Crosshair.name);
        }
        else
        {
            // Last resort - try to find by the last known name
            string lastCrosshairName = PlayerPrefs.GetString("LastCrosshairName", "");
            if (!string.IsNullOrEmpty(lastCrosshairName))
            {
                Crosshair = GameObject.Find(lastCrosshairName);
                if (Crosshair != null)
                {
                    Debug.Log($"Found crosshair using saved name: {lastCrosshairName}");
                    Crosshair.SetActive(true);
                }
            }
        }

        if (trObject != null) trObject.SetActive(true);

        if (Finalscore != null)
        {
            finalScoreText = Finalscore.GetComponent<Text>();
        }

        if (restartButton == null)
        {
            if (ClickToRestart != null)
            {
                restartButton = ClickToRestart.GetComponent<Button>();
            }
            else
            {
                GameObject buttonObj = GameObject.Find("ClickToRestart");
                if (buttonObj != null)
                {
                    restartButton = buttonObj.GetComponent<Button>();
                }
            }
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogError("Could not find restart button component!");
        }
    }

    void Update()
    {
        if (!isGameOver)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;

                // Clamp the time to prevent negative values
                currentTime = Mathf.Max(0f, currentTime);

                // Update timer display
                UpdateTimerDisplay();

                // Check for game over
                if (currentTime <= 0)
                {
                    EndGame();
                }
            }
        }

        // Handle restart
        else if (isGameOver && canRestart && Input.GetMouseButtonDown(0))
        {
            Debug.Log("Click detected, attempting restart");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void UpdateTimerDisplay()
    {
        if (CountdownText != null)
        {
            
            if (startingtime >= 60f)
            {
                int minutes = Mathf.FloorToInt(currentTime / 60f);
                int seconds = Mathf.FloorToInt(currentTime % 60f);
                CountdownText.text = string.Format("{0:}:{1:00}", minutes, seconds);
            }
            else
            {
                
                CountdownText.text = currentTime.ToString("F1");
            }

        
            if (currentTime <= warningThreshold && !isWarningActive)
            {
                isWarningActive = true;
                CountdownText.color = warningTimerColor;
            }
        }
    }

    void EndGame()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("EndGame called - disabling game elements");

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (PauseMenu != null) PauseMenu.SetActive(false);
        if (DimOverlay != null) DimOverlay.SetActive(true);

        // Hide gameplay elements
        if (trObject != null) trObject.SetActive(false);

        // Ensure crosshair is found and disabled
        FindCrosshair();

        if (Crosshair != null)
        {
            Debug.Log($"Disabling crosshair: {Crosshair.name}");
            Crosshair.SetActive(false);

           
            foreach (Transform child in Crosshair.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("Crosshair reference is still null in EndGame");

         
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            foreach (GameObject go in allObjects)
            {
                if (go.name.ToLower().Contains("crosshair") ||
                    go.name.ToLower().Contains("reticle") ||
                    go.name.ToLower().Contains("cursor"))
                {
                    Debug.Log($"Found and disabling: {go.name}");
                    go.SetActive(false);
                }
            }

            foreach (GameObject crosshair in possibleCrosshairs)
            {
                Debug.Log($"Found and disabling: {crosshair.name}");
                crosshair.SetActive(false);
            }
        }

        if (ScoreObject != null) ScoreObject.SetActive(false);

        // Hide the timer
        if (CountdownText != null)
        {
            CountdownText.gameObject.SetActive(false);
        }

        if (Finalscore != null && finalScoreText == null)
        {
            finalScoreText = Finalscore.GetComponent<Text>();
        }

        if (finalScoreText != null)
        {
            float scoreValue = 0;
            if (score != null)
            {
                scoreValue = score.score;
            }
            else if (Score.Instance != null)
            {
                scoreValue = Score.Instance.score;
            }

            finalScoreText.text = $"Final Score: {Mathf.RoundToInt(scoreValue)}";
            Debug.Log($"Set final score text to: {finalScoreText.text}");
        }

        if (target != null)
        {
            target.DisableTarget();
        }

        if (ClickToRestart != null)
        {
            ClickToRestart.SetActive(true);
        }

        canRestart = false;
        Invoke("EnableRestart", 0.5f);
    }

    void ResetTimer()
    {
        currentTime = startingtime;
        isWarningActive = false;
        if (CountdownText != null)
        {
            CountdownText.color = normalTimerColor;
            CountdownText.gameObject.SetActive(true);
        }
    }

    void EnableRestart()
    {
        canRestart = true;
    }

    public void RestartGame()
    {
        Debug.Log("RestartGame method called!");
        try
        {
            Debug.Log($"Current scene: {SceneManager.GetActiveScene().name}, buildIndex: {SceneManager.GetActiveScene().buildIndex}");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to restart game: {e.Message}");
        }
    }

    
    private void FindCrosshair()
    {
        if (Crosshair != null)
        {
            Debug.Log($"Crosshair already assigned: {Crosshair.name}");
            return;
        }

        // First try direct name
        Crosshair = GameObject.Find("Crosshair");
        if (Crosshair != null)
        {
            Debug.Log($"Found crosshair by name: {Crosshair.name}");
        }

       
        if (Crosshair == null)
        {
            Debug.Log("Searching for crosshair by name pattern...");
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject go in allObjects)
            {
                if (go.name.ToLower().Contains("crosshair") ||
                   go.name.ToLower().Contains("reticle") ||
                   go.name.ToLower().Contains("cursor"))
                {
                    Debug.Log($"Found potential crosshair: {go.name}");
                    possibleCrosshairs.Add(go);
                    Crosshair = go;
                    break;
                }
            }
        }

      
        if (Crosshair == null && GameObject.FindWithTag("Crosshair") != null)
        {
            Crosshair = GameObject.FindWithTag("Crosshair");
            Debug.Log($"Found crosshair by tag: {Crosshair.name}");
        }

        if (Crosshair == null)
        {
            Debug.LogWarning("Crosshair reference is still missing. Please assign it in the inspector or ensure it has 'Crosshair' in its name.");
        }
    }
}
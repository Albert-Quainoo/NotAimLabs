using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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
        if (Crosshair != null) Crosshair.SetActive(true);
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
            // Show minutes:seconds format if time is >= 60 seconds
            if (startingtime >= 60f)
            {
                int minutes = Mathf.FloorToInt(currentTime / 60f);
                int seconds = Mathf.FloorToInt(currentTime % 60f);
                CountdownText.text = string.Format("{0:}:{1:00}", minutes, seconds);
            }
            else
            {
                // Show seconds with decimal for times under 60 seconds
                CountdownText.text = currentTime.ToString("F1");
            }

            // Handle warning color change
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

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (PauseMenu != null) PauseMenu.SetActive(false);

        if (DimOverlay != null) DimOverlay.SetActive(true);


        // Hide gameplay elements
        if (trObject != null) trObject.SetActive(false);
        if (Crosshair != null) Crosshair.SetActive(false);
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


       if (finalScoreText != null && score != null)
        {
            finalScoreText.text = $"Final Score: {Mathf.RoundToInt(score.score)}";
        }

        if (target != null)
        {
            target.DisableTarget();
        }

        if (ClickToRestart!= null)
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
}
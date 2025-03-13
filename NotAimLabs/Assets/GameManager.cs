using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset static variables on Scene load
        Target.ResetAccuracy();

        if (Score.Instance != null)
        {
            Score.Instance.ResetScore();
        }

        ReinitializeUIReferences();

        FirstPersonController playerController = FindAnyObjectByType<FirstPersonController>();
        if (playerController != null)
        {
            playerController.cameraCanMove = true;
            playerController.playerCanMove = true;
        }
    }

    private void ReinitializeUIReferences()
    {
        CountdownTimer.Instance.trObject = GameObject.Find("TargetsUI");
        CountdownTimer.Instance.Crosshair = GameObject.Find("Crosshair");
        CountdownTimer.Instance.finalScore = GameObject.Find("FinalScore");
        CountdownTimer.Instance.Clicktorestart = GameObject.Find("ClickToRestart");
        CountdownTimer.Instance.ScoreObject = GameObject.Find("ScoreText");
        CountdownTimer.Instance.AccuracyObject = GameObject.Find("AccuracyText");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
}


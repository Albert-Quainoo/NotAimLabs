using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Pause : MonoBehaviour
{
    [Header("References")]
    public FirstPersonController fpsController;
    public GameObject PauseMenu;
    public GameObject Crosshair;
    public GameObject timerUI;
    public GameObject accuracyUI;
    public GameObject scoreUI;

    public static bool isGamePaused = false;

    private void Start()
    {
        // Ensure pause menu is hidden at start
        if (PauseMenu != null)
        {
            PauseMenu.SetActive(false);
        }
        
        // Initialize game state
        Time.timeScale = 1f;
        isGamePaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Only handle pause if we have a pause menu
        if (PauseMenu != null && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        // Set pause menu active
        PauseMenu.SetActive(true);

        // Hide gameplay UI
        if (Crosshair != null) Crosshair.SetActive(false);
        if (timerUI != null) timerUI.SetActive(false);
        if (accuracyUI != null) accuracyUI.SetActive(false);
        if (scoreUI != null) scoreUI.SetActive(false);

        // Disable player control
        if (fpsController != null)
        {
            fpsController.cameraCanMove = false;
            fpsController.playerCanMove = false;
        }

        // Set pause state
        Time.timeScale = 0f;
        isGamePaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        // Hide pause menu
        PauseMenu.SetActive(false);

        // Show gameplay UI
        if (Crosshair != null) Crosshair.SetActive(true);
        if (timerUI != null) timerUI.SetActive(true);
        if (accuracyUI != null) accuracyUI.SetActive(true);
        if (scoreUI != null) scoreUI.SetActive(true);

        // Enable player control
        if (fpsController != null)
        {
            fpsController.cameraCanMove = true;
            fpsController.playerCanMove = true;
        }

        // Resume game state
        Time.timeScale = 1f;
        isGamePaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        isGamePaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
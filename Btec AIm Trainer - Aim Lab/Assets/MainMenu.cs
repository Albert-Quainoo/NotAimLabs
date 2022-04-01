using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour

{
    public static bool isGamePaused = false;

    public void PlayGame()
    
    {
        Time.timeScale = 1f;     
        isGamePaused = false;
        


        SceneManager.LoadScene("SampleScene");

      }




    public void QuitGame()
    {
        Application.Quit();
    }
}

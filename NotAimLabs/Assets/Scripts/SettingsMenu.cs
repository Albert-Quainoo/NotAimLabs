using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


public class SettingsMenu : MonoBehaviour

{
    public AudioMixer AudioMixer;
    



    public void SetVolume(float volume)
    {

        AudioMixer.SetFloat("volume", Mathf.Log10(volume) * 20); 


    }







}



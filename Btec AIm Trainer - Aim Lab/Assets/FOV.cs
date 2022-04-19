using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FOV : MonoBehaviour
{

    public FirstPersonController Controller;
    public Camera PlayerCamera; 


    public void Setfov(float fov)
    {
     
        FirstPersonController Controller = fov;


        Camera.main.fieldOfView = fov;
        PlayerCamera = Camera.main;
        Controller.fov = fov;

        Camera.main.fieldOfView = (fov / 10);


    }
    
        
    
}

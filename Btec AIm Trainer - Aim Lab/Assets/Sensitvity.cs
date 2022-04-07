using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sensitvity : MonoBehaviour
{
   
    public FirstPersonController Controller;
   
    public void SetSensitvity(float sens)
    {

        Controller.mouseSensitivity = 0f + (sens / 10);


    }


}

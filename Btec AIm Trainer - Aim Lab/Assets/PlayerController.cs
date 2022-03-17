using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform cameraHolder;
    float verticalLookRotation;

    void Update()
    {
        Cursor.lockState = CursorLockMode.Locked;

        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X"));

        verticalLookRotation -= Input.GetAxisRaw("Mouse Y");
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        cameraHolder.localEulerAngles = new Vector3(verticalLookRotation, 0, 0);
    }
}

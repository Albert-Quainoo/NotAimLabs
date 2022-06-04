using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{

    private void Start()
    {

    }
    public void Hit()
    {
        transform.position = TargetBounds.Instance.GetRandomPosition();

         AudioSource audioSource = GetComponent<AudioSource>();

         audioSource.loop = false;
        audioSource.Play();

    }



}

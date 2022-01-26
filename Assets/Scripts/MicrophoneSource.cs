using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneSource : MonoBehaviour
{ 
    void Start()
    {
        foreach (var device in Microphone.devices) {
            Debug.Log("Microphone: " + device);
        }

        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = Microphone.Start("Line 1 (Virtual Audio Cable)", true, 60 * 60 - 1, 44100);
        audioSource.Play();
    }
}

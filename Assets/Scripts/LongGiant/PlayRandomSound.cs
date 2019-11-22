using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRandomSound : MonoBehaviour
{
    [SerializeField] AudioSource[] audioSources = new AudioSource[0];

    public void PlaySound()
    {
        if (audioSources.Length > 0)
        {
            AudioSource random = audioSources[Random.Range(0, audioSources.Length)];

            random.pitch = Random.Range(0.9f, 1.2f);
            random.Play();
        }
    }
}

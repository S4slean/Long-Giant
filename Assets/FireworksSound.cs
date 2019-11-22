using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireworksSound : MonoBehaviour
{
    public AudioSource OnBirthSound;
    public AudioSource OnDeathSound;

    private ParticleSystem ps;

    private int numberOfParticles;
 
    void Update()
    {
        if (!OnBirthSound && !OnDeathSound) { return; }

        var count = ps.particleCount;
        if (count < numberOfParticles)
        { //particle has died
            OnBirthSound.pitch = Random.Range(.8f, 1.2f);
            OnBirthSound.Play();
        }
        else if (count > numberOfParticles)
        { //particle has been born
            OnDeathSound.pitch = Random.Range(.8f, 1.2f);
            OnDeathSound.Play();
        }
        numberOfParticles = count;
    }

    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }
}

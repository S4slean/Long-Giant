using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    [SerializeField] float lifespan = 5f;
    TimerSystem timer;

    private void Start()
    {
        timer = new TimerSystem(lifespan, DestroyAtEndDelay);
        timer.StartTimer();
    }

    private void Update()
    {
        if (!timer.TimerOver)
            timer.UpdateTimer();
    }

    public void DestroyAtEndDelay()
    {
        Destroy(gameObject);
    }
}

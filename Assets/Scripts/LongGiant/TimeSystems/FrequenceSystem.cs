using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System used to generate X event per second (each event being separated from each other with 1/X seconds)
/// Examples : mana regeneration, life loss,...
/// </summary>
[System.Serializable]
public class FrequenceSystem 
{
    public FrequenceSystem(float _frequence)
    {
        frequence = _frequence;
    }

    public void SetUp(System.Action FrequenceEvent)
    {
        OnFrequenceEvent = FrequenceEvent;
        currentFrequenceTime = FrqTime;
    }

    [SerializeField] float frequence = 1;
    float currentFrequenceTime = 1;
    /// <summary>
    /// The time between two frequenceEvent
    /// </summary>
    public float FrqTime { get { return 1 / frequence; } }
    public float GetCurrentFrequenceTime { get { return currentFrequenceTime; } }

    System.Action OnFrequenceEvent;

    /// <summary>
    /// Puts the remaining frequence time to its maximum value
    /// </summary>
    public void ResetFrequence()
    {
        currentFrequenceTime = FrqTime;
    }

    /// <summary>
    /// Sets the frequence to a certain progression : 0 is end of the frequence, 1 is beginning (max value)
    /// </summary>
    /// <param name="coeff"></param>
    public void SetFrequenceProgression(float coeff)
    {
        currentFrequenceTime = Mathf.Lerp(0, FrqTime, coeff);
    }

    public void ChangeFrequence(float newFrequence)
    {
        frequence = newFrequence;
        ResetFrequence();
    }

    /// <summary>
    /// Updates the frequence time and plays the event as much as it is supposed to be played.
    /// </summary>
    public void UpdateFrequence()
    {
        currentFrequenceTime -= Time.deltaTime;

        while(currentFrequenceTime < 0)
        {
            currentFrequenceTime += FrqTime;
            OnFrequenceEvent?.Invoke();
        }
    }

    /// <summary>
    /// Updates the frequence time with a certain time scale and plays the event as much as it is supposed to be played.
    /// </summary>
    public void UpdateFrequence(float timeScale)
    {
        currentFrequenceTime -= Time.deltaTime * timeScale;

        while (currentFrequenceTime < 0)
        {
            currentFrequenceTime += FrqTime;
            OnFrequenceEvent?.Invoke();
        }
    }

    /// <summary>
    /// Updates the frequence time and plays the event as much as it is supposed to be played, only if canTrigger is true
    /// </summary>
    public void UpdateFrequence(bool canTrigger)
    {
        if(currentFrequenceTime > 0)
            currentFrequenceTime -= Time.deltaTime;

        if (canTrigger)
        {
            while (currentFrequenceTime < 0)
            {
                currentFrequenceTime += FrqTime;
                OnFrequenceEvent?.Invoke();
            }
        }
    }

    /// <summary>
    /// Updates the frequence time with a certain time scale and plays the event as much as it is supposed to be played, only if canTrigger is true.
    /// </summary>
    public void UpdateFrequence(float timeScale, bool canTrigger)
    {
        if (currentFrequenceTime > 0)
            currentFrequenceTime -= Time.deltaTime * timeScale;

        if (canTrigger)
        {
            while (currentFrequenceTime < 0)
            {
                currentFrequenceTime += FrqTime;
                OnFrequenceEvent?.Invoke();
            }
        }
    }
}

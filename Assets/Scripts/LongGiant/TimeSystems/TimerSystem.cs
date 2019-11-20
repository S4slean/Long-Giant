using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The timer class is used to simulate timers such as cooldowns or events needing to occur during a certain duration
/// It is composed of a constant value and a variable value
/// The variable value is set to the constant when the timer starts, and then goes to zero
/// At zero, the timer is over
/// Also able to repeat a certain number of time
/// </summary>
[System.Serializable]
public class TimerSystem
{
    [Tooltip("The duration of the cooldown, action,...")]
    [SerializeField] float duration = 1;
    float counter;
    [SerializeField] int numberOfIterations = 1;
    int remainingIterations;

    #region Initialization
    public TimerSystem(float _duration, System.Action onTimerEnded)
    {
        duration = _duration;
        OnTimerEnded = onTimerEnded;
    }

    public TimerSystem(float _duration, System.Action onTimerEnded, int _iterations, System.Action OnIterationEnd)
    {
        duration = _duration;
        OnTimerEnded = onTimerEnded;
        numberOfIterations = _iterations;
        OnIterationEnded = OnIterationEnd;
    }

    public void SetUp(System.Action onTimerEnded)
    {
        OnTimerEnded = onTimerEnded;
    }

    public void SetUp(System.Action onTimerEnded, System.Action OnIterationEnd)
    {
        OnTimerEnded = onTimerEnded;
        OnIterationEnded = OnIterationEnd;
    }
    #endregion

    #region Usability Functions
    /// <summary>
    /// Puts the timer and number of iteration to their maximum values
    /// </summary>
    public void StartTimer()
    {
        counter = duration;
        remainingIterations = numberOfIterations;
        if (remainingIterations == 0)
            remainingIterations = 1;
    }

    /// <summary>
    /// Updates the timer with the base Time.deltaTime
    /// </summary>
    public void UpdateTimer()
    {
        UpdateTimer(Time.deltaTime);
    }

    /// <summary>
    /// Counts down the timer with the inputed value
    /// Allows the use of other values than deltaTime, even though it is supposed to be the based value
    /// Example : passive makes all cooldown 1.5x shorter,...
    /// </summary>
    /// <param name="deltaTime"></param>
    public void UpdateTimer(float deltaTime)
    {
        if (counter > 0)
            counter -= deltaTime;

        if (counter < 0)
        {
            EndIteration();
        }
    }

    /// <summary>
    /// Ends the iteration, repeat if there are remaining iterations, end the timer if not
    /// </summary>
    public void EndIteration()
    {
        remainingIterations--;

        if (remainingIterations == 0)
        {
            OnIterationEnded?.Invoke();
            EndTimer();
        }
        else
            EndIterationAndRepeat();
    }

    /// <summary>
    /// Can be used externaly to interrupt the timer and put it at its maximum value
    /// </summary>
    public void EndTimer()
    {
        counter = 0;

        OnTimerEnded?.Invoke();
    }

    /// <summary>
    /// End an iteration, call the linked events and then repeat the timer
    /// </summary>
    public void EndIterationAndRepeat()
    {
        OnIterationEnded?.Invoke();
        counter = duration;
    }

    System.Action OnTimerEnded;
    System.Action OnIterationEnded;

    /// <summary>
    /// Remove a certain amount of time to the timer
    /// </summary>
    /// <param name="removedTime"></param>
    public void RemoveToTimer(float removedTime)
    {
        if (TimerOver)
            return;

        counter -= removedTime;
        if (counter < 0)
            EndIteration();
    }

    /// <summary>
    /// Add a certain amount of time to the timer
    /// </summary>
    /// <param name="addedTime"></param>
    public void AddToTimer(float addedTime)
    {
        AddToTimer(addedTime, false);
    }

    /// <summary>
    /// Add a certain amount of time to the timer, potentially overriding the maximum value of the timer
    /// </summary>
    /// <param name="addedTime"></param>
    public void AddToTimer(float addedTime, bool canOverrideMaxDuration)
    {
        counter += addedTime;
        if (!canOverrideMaxDuration && counter > duration)
            counter = duration;
    }

    /// <summary>
    /// Change the max timer valuer
    /// </summary>
    /// <param name="newTimerValue"></param>
    public void ChangeTimerValue(float newTimerValue)
    {
        duration = newTimerValue;
    }

    /// <summary>
    /// Change the number of iterations of this timer
    /// </summary>
    /// <param name="newIterationValue"></param>
    public void ChangeIterationValue(int newIterationValue)
    {
        numberOfIterations = newIterationValue;
    }

    /// <summary>
    /// Stops the timer and all iterations
    /// </summary>
    /// <param name="triggerEndTimer"></param>
    public void InterruptTimer(bool triggerEndTimer)
    {
        counter = 0;
        remainingIterations = 0;

        if(triggerEndTimer)
            OnTimerEnded?.Invoke();
    }
    #endregion

    #region Values Readability
    public bool TimerOver { get { return counter == 0; } }

    /// <summary>
    /// The timer Coefficient is a value going from 0 to 1 as the timer counts down
    /// 0 Is the start of the timer, 1 is the end
    /// Can be used for example to have a fill amount for the timer completion,...
    /// </summary>
    public float GetTimerCoefficient { get { return Mathf.Clamp(1 - counter / duration, 0, 1); } }

    public float GetTimerDuration { get { return duration; } }

    public float GetTimerCounter { get { return counter; } }
    #endregion
}

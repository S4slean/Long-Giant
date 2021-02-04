using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple script used to receive animation events calls - Mostly used to trigger the true attack of the Human.
/// </summary>
public class HumanAnimationCaller : MonoBehaviour
{
    [SerializeField] HumanScript relatedHuman = default;

    /// <summary>
    /// Laucnhed the related human attack, playing its effect (launching projectile...)
    /// </summary>
    public void LaunchTrueAttack()
    {
        relatedHuman.LaunchTrueAttack();
    }
}

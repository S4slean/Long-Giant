using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAnimationCaller : MonoBehaviour
{
    [SerializeField] HumanScript relatedHuman = default;

    public void LaunchTrueAttack()
    {
        relatedHuman.LaunchTrueAttack();
    }
}

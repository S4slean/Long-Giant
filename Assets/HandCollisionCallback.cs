using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HandCollisionCallback : MonoBehaviour
{
    public Action<Collision> OnCollision;

    private void OnCollisionEnter(Collision collision)
    {
        OnCollision?.Invoke(collision);
    }
}

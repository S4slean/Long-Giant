using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] float objSize;
    [SerializeField] float radius;
    [SerializeField] float spacing;
    [SerializeField] float minDist;
    [SerializeField] int maxNumber;

    bool generatedWorld = false;
    void Update()
    {
        if (!generatedWorld)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                generatedWorld = true;

                GameManager.gameManager.GetWorldGenerationManager.GenerateWorld(transform.position, radius);
            }
        }
    }
}

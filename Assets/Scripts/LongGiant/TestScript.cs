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
                /*List<Vector3> positions = CirclePositionsGenerator.GetAllPositionsInCircle(objSize, spacing, radius, minDist, maxNumber);

                int counter = 0;
                foreach (Vector3 pos in positions)
                {
                    Color color = Color.Lerp(Color.red, Color.white, (float)counter/ positions.Count);
                    Debug.DrawRay(transform.position + pos, Vector3.up * 1, color, 2f);
                    counter++;
                }*/
            }
        }
    }
}

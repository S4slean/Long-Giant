using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclePositionsGenerator : MonoBehaviour
{
    static int iterationsLimit = 500;

    public static List<Vector3> GetAllPositionsInCircle(float objectsRadius, float objectsSpacing, float zoneRadius)
    {
        List<Vector3> allPossiblePositions = new List<Vector3>();
        allPossiblePositions.Add(Vector3.zero);
        bool outOfRadius = false;
        int circleCounter = 1;
        Vector3 lateralStep = new Vector3(objectsRadius + objectsSpacing, 0, 0);
        Vector3 diagonalStep = Quaternion.Euler(0, 60, 0) * lateralStep;
        while (!outOfRadius && circleCounter < iterationsLimit)
        {
            int lateralBaseValue = circleCounter / 2;
            if (circleCounter % 2 != 0)
                lateralBaseValue++;
            int diagonalBaseValue = circleCounter / 2;

            bool alternate = true;
            int alternanceCounter = 0;
            for (int i = 0; i < circleCounter; i++)
            {
                int lateralValue = lateralBaseValue;
                int diagonalValue = diagonalBaseValue;
                if (alternate)
                {
                    lateralValue += alternanceCounter;
                    diagonalValue -= alternanceCounter;
                    alternanceCounter++;
                    alternate = false;
                }
                else
                {
                    lateralValue -= alternanceCounter;
                    diagonalValue += alternanceCounter;
                    alternate = true;
                }

                Vector3 newPos = lateralValue * lateralStep + diagonalValue * diagonalStep;
                if (newPos.magnitude < zoneRadius)
                {
                    allPossiblePositions.Add(newPos);
                    for (int j = 1; j < 6; j++)
                        allPossiblePositions.Add(Quaternion.Euler(0, 60 * j, 0) * newPos);
                }
            }
            circleCounter++;
        }

        return allPossiblePositions;
    }

    public static List<Vector3> GetAllPositionsInCircle(float objectsRadius, float objectsSpacing, float zoneRadius, float minimumDistanceWithCenter, int maxCount)
    {
        bool maxNumberOfSpotReached = false;

        List<Vector3> allPossiblePositions = new List<Vector3>();

        if (minimumDistanceWithCenter <= 0)
        {
            allPossiblePositions.Add(Vector3.zero);
            maxNumberOfSpotReached = allPossiblePositions.Count >= maxCount;
        }

        bool outOfRadius = false;
        int circleCounter = 1;
        Vector3 lateralStep = new Vector3(objectsRadius + objectsSpacing, 0, 0);
        Vector3 diagonalStep = Quaternion.Euler(0, 60, 0) * lateralStep;
        while (!outOfRadius && circleCounter < maxCount)
        {
            if (maxNumberOfSpotReached)
                break;

            int lateralBaseValue = circleCounter / 2;
            if (circleCounter % 2 != 0)
                lateralBaseValue++;
            int diagonalBaseValue = circleCounter / 2;

            bool alternate = true;
            int alternanceCounter = 0;
            for (int i = 0; i < circleCounter; i++)
            {
                int lateralValue = lateralBaseValue;
                int diagonalValue = diagonalBaseValue;
                if (alternate)
                {
                    lateralValue += alternanceCounter;
                    diagonalValue -= alternanceCounter;
                    alternanceCounter++;
                    alternate = false;
                }
                else
                {
                    lateralValue -= alternanceCounter;
                    diagonalValue += alternanceCounter;
                    alternate = true;
                }

                Vector3 baseNewPos = lateralValue * lateralStep + diagonalValue * diagonalStep;
                Vector3 newPos = baseNewPos;
                if (newPos.magnitude < zoneRadius && newPos.magnitude >= minimumDistanceWithCenter)
                {
                    allPossiblePositions.Add(newPos);

                    maxNumberOfSpotReached = allPossiblePositions.Count >= maxCount;
                    if (maxNumberOfSpotReached)
                        break;

                    for (int j = 1; j < 6; j++)
                    {
                        newPos = Quaternion.Euler(0, 60 * j, 0) * baseNewPos;
                        allPossiblePositions.Add(newPos);

                        maxNumberOfSpotReached = allPossiblePositions.Count >= maxCount;
                        if (maxNumberOfSpotReached)
                            break;
                    }
                }
            }
            circleCounter++;
        }

        return allPossiblePositions;
    }
    /*    public static List<Vector3> GetAllPositionsInCircle(float objectsRadius, float objectsSpacing, float zoneRadius, float minimumDistanceWithRadius, int maxCount)
{
    List<Vector3> allPossiblePositions = new List<Vector3>();
    //if (allPossiblePositions. >= minimumDistanceWithRadius)
    if (minimumDistanceWithRadius <= 0)
        allPossiblePositions.Add(Vector3.zero);
    bool outOfRadius = false;
    int circleCounter = 1;
    Vector3 lateralStep = new Vector3(objectsRadius + objectsSpacing, 0, 0);
    Vector3 diagonalStep = Quaternion.Euler(0, 60, 0) * lateralStep;
    while (!outOfRadius && circleCounter < iterationsLimit)
    {
        int lateralBaseValue = circleCounter / 2;
        if (circleCounter % 2 != 0)
            lateralBaseValue++;
        int diagonalBaseValue = circleCounter / 2;

        bool alternate = true;
        int alternanceCounter = 0;
        for (int i = 0; i < circleCounter; i++)
        {
            int lateralValue = lateralBaseValue;
            int diagonalValue = diagonalBaseValue;
            if (alternate)
            {
                lateralValue += alternanceCounter;
                diagonalValue -= alternanceCounter;
                alternanceCounter++;
                alternate = false;
            }
            else
            {
                lateralValue -= alternanceCounter;
                diagonalValue += alternanceCounter;
                alternate = true;
            }

            Vector3 newPos = lateralValue * lateralStep + diagonalValue * diagonalStep;
            if (newPos.magnitude < zoneRadius)
            {
                //if (newPos.magnitude >= minimumDistanceWithRadius)
                {

                    allPossiblePositions.Add(newPos);
                    for (int j = 1; j < 6; j++)
                    {
                        newPos = Quaternion.Euler(0, 60 * j, 0) * newPos;
                        //if (newPos.magnitude >= minimumDistanceWithRadius)
                            allPossiblePositions.Add(newPos);
                    }
                }
            }
        }
        circleCounter++;
    }

    return allPossiblePositions;
}*/
}

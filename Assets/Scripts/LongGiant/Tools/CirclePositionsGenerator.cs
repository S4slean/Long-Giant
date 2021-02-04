using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclePositionsGenerator : MonoBehaviour
{
    static int iterationsLimit = 500;

    /// <summary>
    /// Returns a list of positions within a certain circular area (between min and max radius), spaced with the inputed values.
    /// This generation system can be visualized as an Hexagonal Grid on which positions are selected.
    /// We move from X tiles on the right, and Y tiles on the up-right diagonal, which gives a unique tile.
    /// We then rotate this translation around cented of 60°, 5 times, to get 6 unique tiles positions.
    /// By running through tiles like this, this system generates locations within a circle.
    /// </summary>
    /// <param name="objectsRadius">The radius of each object placed within the circle</param>
    /// <param name="objectsSpacing">The spacing between each object placed within the circle</param>
    /// <param name="zoneRadius">The maximum reachable distance for positions</param>
    /// <param name="minimumDistanceWithCenter">The maximum reachable distance for positions</param>
    /// <param name="maxCount">The maximum number of positions</param>
    /// <returns></returns>
    public static List<Vector3> GetAllPositionsInCircle(float objectsRadius, float objectsSpacing, float zoneRadius, float minimumDistanceWithCenter, int maxCount)
    {
        bool maxNumberOfSpotReached = false;

        List<Vector3> allPossiblePositions = new List<Vector3>();

        //If minimumDistanceWithCenter is 0 or less, center position can be used
        if (minimumDistanceWithCenter <= 0)
        {
            allPossiblePositions.Add(Vector3.zero);
            maxNumberOfSpotReached = allPossiblePositions.Count >= maxCount;
        }
               
        bool outOfRadius = false;

        //Circle counter is the current "circle" on which we are generating positions on the hexagonal grid
        int circleCounter = 1;      
        
        //Lateral step is the right movement vector
        Vector3 lateralStep = new Vector3(objectsRadius + objectsSpacing, 0, 0);
        //Diagonal step is the up-right movement vector
        Vector3 diagonalStep = Quaternion.Euler(0, 60, 0) * lateralStep;

        //Keep running through the hexagonal grid circles while positions didn't exceed zone radius or reached max positions count
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

            //We move to the next hexagonal grid circle
            circleCounter++;
        }

        return allPossiblePositions;
    }
}

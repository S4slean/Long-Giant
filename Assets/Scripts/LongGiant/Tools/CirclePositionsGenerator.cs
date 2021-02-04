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
    /// <param name="maxCount">The maximum number of circles</param>
    /// <returns></returns>
    public static List<Vector3> GetAllPositionsInCircle(float objectsRadius, float objectsSpacing, float zoneRadius, float minimumDistanceWithCenter, int maxCount)
    {
        print(maxCount);

        bool maxNumberOfSpotReached = false;

        List<Vector3> allPossiblePositions = new List<Vector3>();

        //If minimumDistanceWithCenter is 0 or less, center position can be used
        if (minimumDistanceWithCenter <= 0)
        {
            allPossiblePositions.Add(Vector3.zero);
            maxNumberOfSpotReached = allPossiblePositions.Count >= maxCount;
        }
               
        bool allPositionsExceededRadius = false;

        //Circle counter is the current "circle" on which we are generating positions on the hexagonal grid
        int circleCounter = 1;      
        
        //Lateral step is the right movement vector
        Vector3 lateralStep = new Vector3(objectsRadius + objectsSpacing, 0, 0);
        //Diagonal step is the up-right movement vector
        Vector3 diagonalStep = Quaternion.Euler(0, 60, 0) * lateralStep;

        //Keep running through the hexagonal grid circles while positions didn't exceed zone radius or reached max circles count (security)
        while (!allPositionsExceededRadius && circleCounter < maxCount)
        {
            allPositionsExceededRadius = true;

            if (maxNumberOfSpotReached)
                break;


            //The path to reach any of the location of the current circle is composed of the lateralValue and the diagonalValue, and their sum is always equel to the circleCounter
            //This way, we're sure to get on the right hexagonal circle, and to cover every location of that circle

            //For each hexagonal grid circle, the idea is to firstly get the "middle" position of the top left side of the hexagon, 
            //and then get the locations toward the edge points of that side, by alternating toward one point and the other

            //This way, if we reach the maximum positions count, last positions will be generated in the middle of the hexagonal sides,
            //which has generally a better render than just going from one edge point to the other

            //Those 4 initialization lines get the "path to the center position" of the top left hexagonal side
            int lateralBaseValue = circleCounter / 2;
            if (circleCounter % 2 != 0)
                lateralBaseValue++;
            int diagonalBaseValue = circleCounter / 2;

            //As said, we then alternate toward each edge point of that line to join the edge points and complete the hexagonal circle 
            bool alternate = true;
            int alternanceCounter = 0;
            //The number of locations on the side of the hexagonal circle is always equal to the circle count
            for (int i = 0; i < circleCounter; i++)
            {
                int lateralValue = lateralBaseValue;
                int diagonalValue = diagonalBaseValue;
                if (alternate)
                {
                    //Here, lateral value will get increased and diagonal value will be decreased
                    //We're searching for points between the middle and the right edge point of the side, since we're more moving right than up-right
                    lateralValue += alternanceCounter;
                    diagonalValue -= alternanceCounter;
                    alternanceCounter++;
                    alternate = false;
                }
                else
                {
                    //Here, lateral value will get decreased and diagonal value will be increased
                    //We're searching for points between the middle and the up-right edge point of the side, since we're more moving up-right than right
                    lateralValue -= alternanceCounter; 
                    diagonalValue += alternanceCounter; 
                    alternate = true;
                }

                //We can then get the position of the point we're searching for by multipliying the movement counters with the step vectors
                Vector3 baseNewPos = lateralValue * lateralStep + diagonalValue * diagonalStep;
                Vector3 newPos = baseNewPos;
                //if the distance from center is too high or too low, we can ignore it
                if (newPos.magnitude < zoneRadius && newPos.magnitude >= minimumDistanceWithCenter)
                {
                    //We add the found location
                    allPossiblePositions.Add(newPos);
                    allPositionsExceededRadius = false;

                    //We check if max number of locations was reached
                    maxNumberOfSpotReached = allPossiblePositions.Count >= maxCount;
                    if (maxNumberOfSpotReached)
                        break;

                    //Insted of repeating the same process for each side of the hexagonal circle, we can just rotate the position vector of 60°, 5 times, 
                    //to get the same point on the different sides of the hexagone, completing the hexagonal circle completely
                    for (int j = 1; j < 6; j++)
                    {
                        newPos = Quaternion.Euler(0, 60 * j, 0) * baseNewPos;
                        allPossiblePositions.Add(newPos);

                        //We check if max number of locations was reached
                        maxNumberOfSpotReached = allPossiblePositions.Count >= maxCount;
                        if (maxNumberOfSpotReached)
                            break;
                    }
                }
                //Avoid loop from ending if all locations are too close
                else if(newPos.magnitude < zoneRadius)
                    allPositionsExceededRadius = false;
            }

            //We move to the next hexagonal grid circle
            circleCounter++;
        }

        print(allPositionsExceededRadius);

        return allPossiblePositions;
    }
}

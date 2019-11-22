using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The LineSmoother class contains generic operations necessary
/// for converting a provided set of points into a smoothed 3D line.
/// </summary>
public static class LineSmoother
{
    /// <summary>
    /// Create a smoothed line using a biased moving average operation.
    /// 
    /// Operation is biased towards endpoints - if the window being averaged extends past an
    /// endpoint, the endpoint will contribute more heavily to the averaged point.
    /// </summary>
    /// <param name="orderedPoints">Set of points making a rough line, in desired order.</param>
    /// <param name="avgPoints">Number of points to use when making an average.</param>
    /// <returns>Ordered set of points making up a smoothed line.</returns>
    public static List<Vector3> biasedMovingAverages(List<Vector3> orderedPoints, int avgPoints)
    {
        List<Vector3> smoothedPoints = new List<Vector3>();
        for (int i = 0; i < orderedPoints.Count; i++)
        {
            Vector3 avgPoint = new Vector3(0, 0, 0);
            for (int j = 0; j < avgPoints; j++)
            {
                int currentIndex = i + j - (avgPoints / 2);
                currentIndex = Mathf.Clamp(currentIndex, 0, orderedPoints.Count - 1);
                avgPoint += orderedPoints[currentIndex];
            }

            avgPoint /= avgPoints;
            smoothedPoints.Add(avgPoint);
        }
        return smoothedPoints;
    }

}

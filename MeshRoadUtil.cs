using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Common utility functions used for generating mesh roads.
/// </summary>
public static class MeshRoadUtil
{
    /// <summary>
    /// Generate triangle array for a standard mesh consisting of rectangular cross sections.
    /// </summary>
    /// <param name="numCrossSections">Number of rectangular cross sections "framing" the mesh.</param>
    /// <returns>Array of integers, each set of 3 representing a triangle in the mesh.</returns>
    public static int[] getStandardMeshTriangles(int numCrossSections)
    {
        int[] triangles = new int[((numCrossSections - 1) * 8 + 4) * 3];
        int numVertices = numCrossSections * 4;

        // Form appropriate triangles for each set of two cross sections
        for (int i = 0; i < numCrossSections - 1; i++)
        {
            int tOffset = i * 8 * 3;
            int vOffset = i * 4;

            // Top triangles
            pushTriangles(ref triangles, tOffset, vOffset, vOffset + 4, vOffset + 5, vOffset + 1);

            // Bottom triangles
            pushTriangles(ref triangles, tOffset + 6, vOffset + 2, vOffset + 3, vOffset + 7, vOffset + 6);

            // Left triangles
            pushTriangles(ref triangles, tOffset + 12, vOffset, vOffset + 2, vOffset + 6, vOffset + 4);

            // Right triangles
            pushTriangles(ref triangles, tOffset + 18, vOffset + 1, vOffset + 5, vOffset + 7, vOffset + 3);
        }

        // Special case: front triangles on first cross section
        pushTriangles(ref triangles, triangles.Length - 12, 0, 1, 3, 2);

        // Special case: rear triangles on last cross section
        pushTriangles(ref triangles, triangles.Length - 6, numVertices - 4, numVertices - 2,
            numVertices - 1, numVertices - 3);

        return triangles;
    }

    /// <summary>
    /// Create forward-facing triangles from a rectangle and insert them into the list of triangles.
    /// 
    /// "Front" side of the rectangle is formed by provided vertices in clockwise order.
    /// </summary>
    /// <param name="tOffset">Index at which to insert the first triangle.</param>
    /// <param name="vertA">Index of top left vertex.</param>
    /// <param name="vertB">Index of top right vertex.</param>
    /// <param name="vertC">Index of bottom left vertex.</param>
    /// <param name="vertD">Index of bottom right vertex.</param>
    private static void pushTriangles(ref int[] triangles, int tOffset, int vertA, int vertB, int vertC, int vertD)
    {
        triangles[tOffset] = vertA;
        triangles[tOffset + 1] = vertB;
        triangles[tOffset + 2] = vertC;

        triangles[tOffset + 3] = vertC;
        triangles[tOffset + 4] = vertD;
        triangles[tOffset + 5] = vertA;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Common utility functions used for generating mesh roads.
/// </summary>
public static class MeshRoadUtil
{
    /// <summary>
    /// Set of triangles forming a standard mesh.
    /// </summary>
    private class TriangleSet
    {
        // DATA MEMBERS
        /// <summary>
        /// Number of the current triangle being modified.
        /// </summary>
        private int currentTriangle;

        /// <summary>
        /// Array of vertices, each set of 3 forming a triangle in the mesh.
        /// </summary>
        private int[] trianglePoints;

        // FUNCTIONS
        /// <summary>
        /// Initialize a triangle set.
        /// </summary>
        /// <param name="numTriangles">Maximum number of triangles in the set.</param>
        public TriangleSet(int numTriangles)
        {
            currentTriangle = 0;
            trianglePoints = new int[numTriangles * 3];
        }

        /// <summary>
        /// Add 2 triangles to this set to form a given rectangle.
        /// </summary>
        /// <param name="vertA">Index of top left vertex.</param>
        /// <param name="vertB">Index of top right vertex.</param>
        /// <param name="vertC">Index of bottom left vertex.</param>
        /// <param name="vertD">Index of bottom right vertex.</param>
        public void pushTriangles(int vertA, int vertB, int vertC, int vertD)
        {
            int tOffset = currentTriangle * 3;

            trianglePoints[tOffset] = vertA;
            trianglePoints[tOffset + 1] = vertB;
            trianglePoints[tOffset + 2] = vertC;

            trianglePoints[tOffset + 3] = vertC;
            trianglePoints[tOffset + 4] = vertD;
            trianglePoints[tOffset + 5] = vertA;

            currentTriangle += 2;
        }

        /// <summary>
        /// Get the triangle point array from this set of triangles.
        /// </summary>
        /// <returns>Triangle point array from this set of triangles.</returns>
        public int[] getTrianglePoints()
        {
            return trianglePoints;
        }
    }

    // FUNCTIONS
    /// <summary>
    /// Generate triangle array for a standard mesh consisting of rectangular cross sections.
    /// </summary>
    /// <param name="numCrossSections">Number of rectangular cross sections "framing" the mesh.</param>
    /// <returns>Array of integers, each set of 3 representing a triangle in the mesh.</returns>
    public static int[] getStandardMeshTriangles(int numCrossSections)
    {
        int numVertices = numCrossSections * 4;
        TriangleSet triangleSet = new TriangleSet((numCrossSections - 1) * 8 + 4);

        // Form appropriate triangles for each set of two cross sections
        for (int i = 0; i < numCrossSections - 1; i++)
        {
            int vOffset = i * 4;

            // Top triangles
            triangleSet.pushTriangles(vOffset, vOffset + 4, vOffset + 5, vOffset + 1);

            // Bottom triangles
            triangleSet.pushTriangles(vOffset + 2, vOffset + 3, vOffset + 7, vOffset + 6);

            // Left triangles
            triangleSet.pushTriangles(vOffset, vOffset + 2, vOffset + 6, vOffset + 4);

            // Right triangles
            triangleSet.pushTriangles(vOffset + 1, vOffset + 5, vOffset + 7, vOffset + 3);
        }

        // Special case: front triangles on first cross section
        triangleSet.pushTriangles(0, 1, 3, 2);

        // Special case: rear triangles on last cross section
        triangleSet.pushTriangles(numVertices - 4, numVertices - 2, numVertices - 1, numVertices - 3);

        return triangleSet.getTrianglePoints();
    }

    /// <summary>
    /// Convert a point in terrain space to world space.
    /// </summary>
    /// <param name="terrainPoint"></param>
    /// <param name="terrain"></param>
    /// <returns></returns>
    public static Vector3 terrainPointToWorld(Vector3 terrainPoint, Terrain terrain)
    {
        // Terrain X (Width)  = World Z
        // Terrain Y (Height) = World X
        // Terrain Z          = World Y

        TerrainData data = terrain.terrainData;
        Vector3 positionBase = terrain.transform.position;
        float worldXOffset = (terrainPoint.y / data.heightmapHeight) * data.size.x;
        float worldYOffset = terrainPoint.z * data.size.y;
        float worldZOffset = (terrainPoint.x / data.heightmapWidth) * data.size.z;
        return positionBase + new Vector3(worldXOffset, worldYOffset, worldZOffset);
    }

    /// <summary>
    /// Calculate heightmap height for a world y coordinate for a given terrain.
    /// </summary>
    /// <param name="worldHeight">A world-space y coordinate.</param>
    /// <param name="terrain"></param>
    /// <returns>Heightmap height (between 0 and 1, inclusive).</returns>
    public static float terrainHeightFromWorld(float worldHeight, Terrain terrain)
    {
        float height = (worldHeight - terrain.transform.position.y) / terrain.terrainData.size.y;
        
        // Error check
        if (height < 0 || height > 1)
        {
            Debug.LogError("ERROR: Invalid terrain heightmap height " + height);
        }
        return Mathf.Clamp(height, 0, 1);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A MeshRoad is a custom-built road object generated in the editor using a
/// MeshRenderer and a MeshCollider.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class MeshRoad : MonoBehaviour
{
    // STATIC FUNCTIONS
    /// <summary>
    /// Create a new mesh road with required components attached.
    /// </summary>
    /// <param name="command"></param>
    [MenuItem("GameObject/Mesh Roads/Mesh Road", false, 10)]
    private static void createMeshRoad(MenuCommand command)
    {
        GameObject meshRoad = new GameObject("Mesh Road");
        GameObjectUtility.SetParentAndAlign(meshRoad, (GameObject) command.context);
        Undo.RegisterCreatedObjectUndo(meshRoad, "Create " + meshRoad.name);
        Selection.activeObject = meshRoad;

        // Attach required components
        meshRoad.AddComponent<MeshFilter>();
        meshRoad.AddComponent<MeshRenderer>();
        meshRoad.AddComponent<MeshCollider>();
        meshRoad.AddComponent<MeshRoad>();
    }


    // CONSTANTS
    /// <summary>
    /// Number of seconds that debug drawings remain in editor view.
    /// </summary>
    private const float DEBUG_DRAW_DURATION = 5;


    // OUTLETS
    /// <summary>
    /// Name of the road in the world map.
    /// </summary>
    public string roadName = "Mesh Road";

    /// <summary>
    /// Width of the road object.
    /// </summary>
    public float width = 10;

    /// <summary>
    /// Height of the road object.
    /// </summary>
    public float thickness = 0.1f;


    [Header("Terrain Fitting")]
    /// <summary>
    /// All terrain objects that should automatically mold themselves around this road.
    /// </summary>
    public Terrain[] shapedTerrains;

    /// <summary>
    /// Maximum distance of neighbor nodes from road to smooth.
    /// </summary>
    public int neighborSmoothingRadius = 1;


    [Header("Advanced Generation Options")]
    /// <summary>
    /// Number of points to average when smoothing road points.
    /// </summary>
    public int averageWindow = 20;


    // DATA MEMBERS
    /// <summary>
    /// Points on a line representing the path this road takes.
    /// </summary>
    private Vector3[] roadLinePoints;
    
    /// <summary>
    /// Generated mesh of this road.
    /// </summary>
    private Mesh mesh;

    /// <summary>
    /// Set of mesh vertices in local space.
    /// </summary>
    private Vector3[] vertices;

    /// <summary>
    /// Set of mesh triangles in local space.
    /// 
    /// Each value corresponds to a vertex in the vertices array, and each
    /// set of three values forms a triangle.
    /// 
    /// The "front" of a triangle (the part that's rendered) is formed by
    /// the vertices going clockwise.
    /// </summary>
    private int[] triangles;


    // FUNCTIONS
    /// <summary>
    /// Draw lines making up the original road line.
    /// </summary>
    public void debugRoadLine()
    {
        for (int i = 0; i < roadLinePoints.Length - 1; i++)
        {
            Vector3 a = transform.TransformPoint(roadLinePoints[i]);
            Vector3 b = transform.TransformPoint(roadLinePoints[i + 1]);
            Debug.DrawLine(a, b, Color.red, DEBUG_DRAW_DURATION, false);
        }
    }

    /// <summary>
    /// Draw outlines of road's cross sections
    /// </summary>
    public void debugCrossSections()
    {
        for (int i = 0; i < vertices.Length; i += 4)
        {
            Vector3 topLeft = transform.TransformPoint(vertices[i]);
            Vector3 topRight = transform.TransformPoint(vertices[i + 1]);
            Vector3 bottomLeft = transform.TransformPoint(vertices[i + 2]);
            Vector3 bottomRight = transform.TransformPoint(vertices[i + 3]);

            Debug.DrawLine(topLeft, topRight, Color.blue, DEBUG_DRAW_DURATION, false);
            Debug.DrawLine(bottomLeft, bottomRight, Color.blue, DEBUG_DRAW_DURATION, false);
            Debug.DrawLine(topLeft, bottomLeft, Color.blue, DEBUG_DRAW_DURATION, false);
            Debug.DrawLine(topRight, bottomRight, Color.blue, DEBUG_DRAW_DURATION, false);
        }
    }

    /// <summary>
    /// Clear points and mesh associated with this road.
    /// </summary>
    public void clear()
    {
        roadLinePoints = new Vector3[0];
        generate();
    }
    
    /// <summary>
    /// Regenerate this road upon changing its settings.
    /// </summary>
    public void generate()
    {
        // Create mesh
        mesh = new Mesh();
        mesh.name = roadName + " Custom Mesh";

        if (roadLinePoints.Length > 0)
        {
            generateShape();
            generateMesh();
        }

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    /// <summary>
    /// Calculate the road's vertices and form triangles
    /// </summary>
    private void generateShape()
    {
        // 4 vertices for each point along the road - each forms a cross section
        vertices = new Vector3[roadLinePoints.Length * 4];

        // 8 triangles for each set of 2 cross sections, plus 4 additional for the endpoints
        // Each triangle contains three points
        triangles = new int[((roadLinePoints.Length - 1) * 8 + 4) * 3];

        // For each point on the line, generate a rectangle (cross section)
        // perpendicular to the line to the next point.
        for (int i = 0; i < roadLinePoints.Length; i++)
        {
            Vector3 a = roadLinePoints[i];

            Vector3 fromPrevious = new Vector3();
            Vector3 toNext = new Vector3();

            if (i > 0)
            {
                fromPrevious = roadLinePoints[i] - roadLinePoints[i - 1];
            }
            if (i < roadLinePoints.Length - 1)
            {
                toNext = roadLinePoints[i + 1] - roadLinePoints[i];
            }
            Vector3 direction = ((fromPrevious.normalized + toNext.normalized) / 2).normalized;

            MeshRoadCrossSection crossSection = new MeshRoadCrossSection(a, direction, this);

            // Add vertices to set now
            vertices[i * 4] = crossSection.topLeft;
            vertices[i * 4 + 1] = crossSection.topRight;
            vertices[i * 4 + 2] = crossSection.bottomLeft;
            vertices[i * 4 + 3] = crossSection.bottomRight;
        }

        triangles = MeshRoadUtil.getStandardMeshTriangles(roadLinePoints.Length);
    }

    /// <summary>
    /// Generate the mesh using calculated vertices and triangles.
    /// </summary>
    private void generateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    /// <summary>
    /// Set the list of points making up this road's path.
    /// </summary>
    /// <param name="worldPoints">World points forming this road's path.</param>
    public void setRoadLinePoints(List<Vector3> worldPoints)
    {
        roadLinePoints = new Vector3[worldPoints.Count];
        for (int i = 0; i < worldPoints.Count; i++)
        {
            roadLinePoints[i] = transform.InverseTransformPoint(worldPoints[i]);
        }
    }

    /// <summary>
    /// Change the shape of affected terrains to fit the bottom of this road.
    /// </summary>
    public void fitTerrains()
    {
        foreach (Terrain terrain in shapedTerrains)
        {
            fitTerrainToRoad(terrain);
        }
    }

    /// <summary>
    /// Change the shape of a terrain to fit the bottom of this road.
    /// </summary>
    /// <param name="terrain">Terrain to be shaped.</param>
    private void fitTerrainToRoad(Terrain terrain)
    {
        TerrainData data = terrain.terrainData;
        int width = data.heightmapWidth;
        int height = data.heightmapHeight;

        // TODO: Optimize by not grabbing the entire heightmap, just what the road overlaps
        float[,] heightmap = data.GetHeights(0, 0, width, height);
        HashSet<Vector2> contactNodes = new HashSet<Vector2>(); // Keep track of nodes under the road
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 rayStart = MeshRoadUtil.terrainPointToWorld(new Vector3(x, y, 0), terrain);
                RaycastHit hit;
                if (GetComponent<MeshCollider>()
                    .Raycast(new Ray(rayStart, terrain.transform.up), out hit, data.size.y))
                {
                    contactNodes.Add(new Vector2(x, y));

                    // Calculate where to set height
                    heightmap[x, y] = MeshRoadUtil.terrainHeightFromWorld(hit.point.y, terrain);
                }
            }
        }
        data.SetHeights(0, 0, smoothNeighboringTerrain(heightmap, contactNodes, neighborSmoothingRadius));
    }

    /// <summary>
    /// Smooth terrain neighboring this road.
    /// </summary>
    /// <param name="heightmap">Original terrain heightmap.</param>
    /// <param name="contactNodes">Set of nodes that make contact </param>
    /// <param name="smoothingRadius">Maximum distance from road nodes to smooth.</param>
    /// <returns></returns>
    private float[,] smoothNeighboringTerrain(float[,] heightmap, HashSet<Vector2> contactNodes, int smoothingRadius)
    {
        float[,] updatedHeightmap = heightmap; // Clone?
        HashSet<Vector2> smoothedNodes = new HashSet<Vector2>(); // Keep track of nodes already smoothed

        int xMax = heightmap.GetUpperBound(0);
        int yMax = heightmap.GetUpperBound(1);

        // Find nodes in the neighboring radius
        foreach (Vector2 node in contactNodes)
        {
            int startX = (int) node.x;
            int startY = (int) node.y;
            for (int x = Mathf.Max(0, startX - smoothingRadius); x <= Mathf.Min(startX + smoothingRadius, xMax); x++)
            {
                for (int y = Mathf.Max(0, startY - smoothingRadius); y <= Mathf.Min(startY + smoothingRadius, yMax); y++)
                {
                    if (!smoothedNodes.Contains(new Vector2(x, y))
                        && !contactNodes.Contains(new Vector2(x, y)
                        ))
                    {
                        smoothedNodes.Add(new Vector2(startX, startY));
                    }
                }
            }
        }

        // Smooth nodes based on neighbors (and original value)
        foreach (Vector2 node in smoothedNodes)
        {
            int startX = (int) node.x;
            int startY = (int) node.y;

            float total = 0;
            int numNeighbors = 0;

            for (int x = Mathf.Max(0, startX - smoothingRadius); x <= Mathf.Min(startX + smoothingRadius, xMax); x++)
            {
                for (int y = Mathf.Max(0, startY - smoothingRadius); y <= Mathf.Min(startY + smoothingRadius, yMax); y++)
                {
                    total += heightmap[x, y];
                    numNeighbors++;
                }
            }

            updatedHeightmap[startX, startY] = total / numNeighbors;
        }

        return updatedHeightmap;
    }
}

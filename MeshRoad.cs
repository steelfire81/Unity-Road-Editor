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

    [Header("Advanced Generation Options")]
    /// <summary>
    /// Number of points to average when smoothing road points.
    /// </summary>
    public int averageWindow = 5;


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
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        if (roadLinePoints.Length > 0)
        {
            generateShape();
            generateMesh();
        }
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
        Vector3 direction = new Vector3();
        for (int i = 0; i < roadLinePoints.Length; i++)
        {
            Vector3 a = roadLinePoints[i];

            // Special case - last cross section should use same direction as previous
            if (i < roadLinePoints.Length - 1)
            {
                Vector3 b = roadLinePoints[i + 1];
                direction = b - a;
            }

            MeshRoadCrossSection crossSection = new MeshRoadCrossSection(a, direction, this);

            // Add vertices to set now
            vertices[i * 4] = crossSection.topLeft;
            vertices[i * 4 + 1] = crossSection.topRight;
            vertices[i * 4 + 2] = crossSection.bottomLeft;
            vertices[i * 4 + 3] = crossSection.bottomRight;
        }

        // Form appropriate triangles for each cross section
        for (int i = 0; i < roadLinePoints.Length - 1; i++)
        {
            int tOffset = i * 8 * 3;
            int vOffset = i * 4;

            // Top triangles
            triangles[tOffset] = vOffset;
            triangles[tOffset + 1] = vOffset + 4;
            triangles[tOffset + 2] = vOffset + 1;

            triangles[tOffset + 3] = vOffset + 4;
            triangles[tOffset + 4] = vOffset + 5;
            triangles[tOffset + 5] = vOffset + 1;

            // Bottom triangles
            triangles[tOffset + 6] = vOffset + 2;
            triangles[tOffset + 7] = vOffset + 3;
            triangles[tOffset + 8] = vOffset + 6;

            triangles[tOffset + 9] = vOffset + 3;
            triangles[tOffset + 10] = vOffset + 7;
            triangles[tOffset + 11] = vOffset + 6;

            // Left triangles
            triangles[tOffset + 12] = vOffset;
            triangles[tOffset + 13] = vOffset + 2;
            triangles[tOffset + 14] = vOffset + 4;

            triangles[tOffset + 15] = vOffset + 2;
            triangles[tOffset + 16] = vOffset + 6;
            triangles[tOffset + 17] = vOffset + 4;

            // Right triangles
            triangles[tOffset + 18] = vOffset + 1;
            triangles[tOffset + 19] = vOffset + 5;
            triangles[tOffset + 20] = vOffset + 3;

            triangles[tOffset + 21] = vOffset + 5;
            triangles[tOffset + 22] = vOffset + 7;
            triangles[tOffset + 23] = vOffset + 3;
        }

        // Special case: front triangles on first cross section
        int sOffset = triangles.Length - 12;
        triangles[sOffset] = 0;
        triangles[sOffset + 1] = 1;
        triangles[sOffset + 2] = 2;

        triangles[sOffset + 3] = 1;
        triangles[sOffset + 4] = 3;
        triangles[sOffset + 5] = 2;

        // Special case: back triangles on last cross section
        triangles[sOffset + 6] = vertices.Length - 4;
        triangles[sOffset + 7] = vertices.Length - 2;
        triangles[sOffset + 8] = vertices.Length - 3;

        triangles[sOffset + 9] = vertices.Length - 2;
        triangles[sOffset + 10] = vertices.Length - 1;
        triangles[sOffset + 11] = vertices.Length - 3;
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
}

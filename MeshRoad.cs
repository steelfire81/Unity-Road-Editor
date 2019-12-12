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
    public int averageWindow = 10;


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
}

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
    /// 
    /// TODO: Make dependent on segment instead of whole road (?)
    /// </summary>
    public float width = 10;

    // TODO: Smoothness

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
    /// Draw perpendiculars joining vertex sets.
    /// </summary>
    public void debugPerpendiculars()
    {
        for (int i = 0; i < vertices.Length; i += 2)
        {
            Vector3 a = transform.TransformPoint(vertices[i]);
            Vector3 b = transform.TransformPoint(vertices[i + 1]);
            Debug.DrawLine(a, b, Color.blue, DEBUG_DRAW_DURATION, false);
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
        vertices = new Vector3[roadLinePoints.Length * 2];
        triangles = new int[(roadLinePoints.Length - 1) * 6];

        // For each point on the line, draw a line segment perpendicular
        // to the line to the next point.  Then, add the endpoints of that
        // line segment to the list of vertices.
        Vector3 perpendicular = new Vector3();
        for (int i = 0; i < roadLinePoints.Length - 1; i++)
        {
            Vector3 a = roadLinePoints[i];
            Vector3 b = roadLinePoints[i + 1];

            Vector3 direction = b - a;
            perpendicular = Vector3.Cross(direction, gameObject.transform.up).normalized;

            vertices[i * 2] = a + (perpendicular * (width / 2));
            vertices[i * 2 + 1] = a - (perpendicular * (width / 2));
        }

        // Last perpendicular should face the same direction as the second to last
        Vector3 last = roadLinePoints[roadLinePoints.Length - 1];
        vertices[vertices.Length - 2] = last + (perpendicular * (width / 2));
        vertices[vertices.Length - 1] = last - (perpendicular * (width / 2));

        // For each set of four vertices, form two triangles
        for (int i = 0; i < roadLinePoints.Length - 1; i++)
        {
            int first = i * 6;

            int a = i * 2;
            int b = i * 2 + 1;
            int c = (i + 1) * 2;
            int d = (i + 1) * 2 + 1;

            // First triangle
            triangles[first] = a;
            triangles[first + 1] = c;
            triangles[first + 2] = b;

            // Second triangle
            triangles[first + 3] = b;
            triangles[first + 4] = c;
            triangles[first + 5] = d;
        }
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

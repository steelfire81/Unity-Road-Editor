using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom inspector elements for a Mesh Road.
/// </summary>
[CustomEditor(typeof(MeshRoad))]
public class MeshRoadEditor : Editor
{
    // DATA MEMBERS
    /// <summary>
    /// Whether or not drawing is currently enabled.
    /// </summary>
    private bool drawingEnabled;

    /// <summary>
    /// List of captured road line points from drawing.
    /// </summary>
    private List<Vector3> roadLinePoints;


    // FUNCTIONS
    /// <summary>
    /// Initialize the mesh road editor.
    /// </summary>
    private void OnEnable()
    {
        drawingEnabled = false;
        roadLinePoints = new List<Vector3>();
    }

    /// <summary>
    /// Draw custom inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MeshRoad road = (MeshRoad) target;

        // Add features to draw road
        drawingEnabled = EditorGUILayout.Toggle("Enable Drawing", drawingEnabled);
        
        // Add buttons to generate road
        if (GUILayout.Button("Generate Road"))
        {
            road.generate();
        }

        // Add debug buttons
        if (GUILayout.Button("Debug Road Line"))
        {
            road.debugRoadLine();
        }
        if (GUILayout.Button("Debug Perpendiculars"))
        {
            road.debugPerpendiculars();
        }
    }

    /// <summary>
    /// Draw a road line in the editor, if drawing is enabled.
    /// </summary>
    private void OnSceneGUI()
    {
        MeshRoad road = (MeshRoad) target;
        if (drawingEnabled)
        {
            // Maintain focus for this element while drawing is enabled
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            GUIUtility.hotControl = controlID;

            Vector2 mousePosition = Event.current.mousePosition;
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    roadLinePoints.Clear();
                    roadLinePoints.Add(editorToWorldPoint(mousePosition));

                    Event.current.Use();
                    break;

                case EventType.MouseDrag:
                    roadLinePoints.Add(editorToWorldPoint(mousePosition));

                    // DEBUG (Maybe leave in?)
                    Debug.DrawLine(roadLinePoints[roadLinePoints.Count - 2], roadLinePoints[roadLinePoints.Count - 1], Color.cyan, 5, false);

                    Event.current.Use();
                    break;

                case EventType.MouseUp:
                    // Finish drawing
                    drawingEnabled = false;
                    finalizeLine();

                    Event.current.Use();
                    break;
            }
        }
    }

    /// <summary>
    /// Convert the mouse's position in the editor to a point in the world.
    /// </summary>
    /// <param name="mousePosition">The mouse's position in the editor.</param>
    /// <returns>A world point.</returns>
    private Vector3 editorToWorldPoint(Vector2 mousePosition)
    {
        mousePosition.y = Camera.current.pixelHeight - mousePosition.y; // Mouse position y is inverted
        RaycastHit hit;
        Physics.Raycast(Camera.current.ScreenPointToRay(mousePosition), out hit);
        
        if (hit.collider)
        {
            // TODO: Only detect collision with terrain
            return hit.point;
        }
        else // ERROR
        {
            // TODO: End drawing
            return new Vector3();
        }
    }

    /// <summary>
    /// Simplify the drawn line and assign it to the road editor.
    /// </summary>
    private void finalizeLine()
    {
        MeshRoad road = (MeshRoad) target;

        List<Vector3> simplifiedLine = new List<Vector3>();
        LineUtility.Simplify(roadLinePoints, road.simplifyTolerance, simplifiedLine);
        road.setRoadLinePoints(simplifiedLine);
        roadLinePoints.Clear(); // No need to keep this data twice

        road.debugRoadLine();
    }
}

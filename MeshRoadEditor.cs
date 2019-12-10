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
    // CONSTANTS
    /// <summary>
    /// Highest valid layer index.
    /// </summary>
    private const int MAX_LAYER = 31;

    // DATA MEMBERS
    /// <summary>
    /// Whether or not drawing is currently enabled.
    /// </summary>
    private bool drawingEnabled;

    /// <summary>
    /// List of captured road line points from drawing.
    /// </summary>
    private List<Vector3> roadLinePoints;

    /// <summary>
    /// Array of layer names.
    /// </summary>
    private string[] layerNames;

    /// <summary>
    /// The layer on which the road can be drawn.
    /// </summary>
    private int targetLayer;


    // FUNCTIONS
    /// <summary>
    /// Initialize the mesh road editor.
    /// </summary>
    private void OnEnable()
    {
        drawingEnabled = false;
        roadLinePoints = new List<Vector3>();
        layerNames = getLayerNames();
        targetLayer = 0;
    }

    /// <summary>
    /// Draw custom inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MeshRoad road = (MeshRoad) target;

        // Target layer dropdown
        targetLayer = EditorGUILayout.Popup("Target Layer", targetLayer, getLayerNames());

        // Add features to draw road
        drawingEnabled = EditorGUILayout.Toggle("Enable Drawing", drawingEnabled);
        
        // Add buttons to generate road
        if (GUILayout.Button("Generate Road"))
        {
            road.generate();
        }

        // Add button to clear road
        if (GUILayout.Button("Clear Road"))
        {
            road.clear();
        }

        // Add debug buttons
        if (GUILayout.Button("Debug Road Line"))
        {
            road.debugRoadLine();
        }
        if (GUILayout.Button("Debug Cross Sections"))
        {
            road.debugCrossSections();
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

                    // Useful debug - draw the last line segment
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
        int layerMask = LayerMask.GetMask(layerNames[targetLayer]);
        mousePosition.y = Camera.current.pixelHeight - mousePosition.y; // Mouse position y is inverted
        RaycastHit hit;
        Physics.Raycast(Camera.current.ScreenPointToRay(mousePosition), out hit, Mathf.Infinity, layerMask);
        
        if (hit.collider)
        {
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

        List<Vector3> simplifiedLine = LineSmoother.biasedMovingAverages(roadLinePoints, road.averageWindow);

        road.setRoadLinePoints(simplifiedLine);
        roadLinePoints.Clear(); // No need to keep this data twice

        road.debugRoadLine();
    }

    /// <summary>
    /// Get list of layer names, both default and user-defined.
    /// </summary>
    /// <returns>Array containing names of all layers.</returns>
    private string[] getLayerNames()
    {
        List<string> layers = new List<string>();
        for (int i = 0; i <= MAX_LAYER; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (layerName != "")
                layers.Add(layerName);
        }
        return layers.ToArray();
    }
}

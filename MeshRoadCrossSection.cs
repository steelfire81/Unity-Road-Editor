using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Mesh Road Cross Section represents a rectangle perpendicular to
/// the path of a Mesh Road.
/// </summary>
public class MeshRoadCrossSection
{   
    // DATA MEMBERS
    /// <summary>
    /// Top left point on this cross section.
    /// </summary>
    public Vector3 topLeft { get; private set; }

    /// <summary>
    /// Top right point on this cross section.
    /// </summary>
    public Vector3 topRight { get; private set; }

    /// <summary>
    /// Bottom left point on this cross section.
    /// </summary>
    public Vector3 bottomLeft { get; private set; }

    /// <summary>
    /// Bottom right point on this cross section.
    /// </summary>
    public Vector3 bottomRight { get; private set; }


    /// <summary>
    /// Create a new cross section.
    /// </summary>
    /// <param name="center">Center point of this cross section.</param>
    /// <param name="direction">Direction this cross section is facing.</param>
    /// <param name="parent">Road to which this cross section belongs.</param>
    public MeshRoadCrossSection(Vector3 center, Vector3 direction, MeshRoad parent)
    {
        float halfWidth = parent.width / 2;
        float halfHeight = parent.thickness / 2;

        Vector3 perpendicular = Vector3.Cross(direction, parent.gameObject.transform.up).normalized;
        Vector3 upwardsPerpendicular = Vector3.Cross(perpendicular, direction).normalized;

        Vector3 left = center + (perpendicular * halfWidth);
        topLeft = left + (upwardsPerpendicular * halfHeight);
        bottomLeft = left - (upwardsPerpendicular * halfHeight);

        Vector3 right = center - (perpendicular * halfWidth);
        topRight = right + (upwardsPerpendicular * halfHeight);
        bottomRight = right - (upwardsPerpendicular * halfHeight);
    }
}

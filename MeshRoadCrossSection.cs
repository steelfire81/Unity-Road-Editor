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
    /// Center point of this cross section.
    /// </summary>
    public Vector3 center { get; private set; }
    
    /// <summary>
    /// Center point on this cross section's left side.
    /// </summary>
    public Vector3 centerLeft { get; private set; }

    /// <summary>
    /// Center point on this cross section's right side.
    /// </summary>
    public Vector3 centerRight { get; private set; }
    
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
    /// Top left point on this road's trigger collider.
    /// </summary>
    public Vector3 hitboxTopLeft { get; private set; }

    /// <summary>
    /// Top right point on this road's trigger collider.
    /// </summary>
    public Vector3 hitboxTopRight { get; private set; }


    /// <summary>
    /// Create a new cross section.
    /// </summary>
    /// <param name="center">Center point of this cross section.</param>
    /// <param name="direction">Direction this cross section is facing.</param>
    /// <param name="parent">Road to which this cross section belongs.</param>
    public MeshRoadCrossSection(Vector3 center, Vector3 direction, MeshRoad parent)
    {
        this.center = center;

        float halfWidth = parent.width / 2;
        float halfHeight = parent.thickness / 2;

        Vector3 perpendicular = Vector3.Cross(direction, parent.gameObject.transform.up).normalized;
        Vector3 upwardsPerpendicular = Vector3.Cross(perpendicular, direction).normalized;

        centerLeft = center + (perpendicular * halfWidth);
        topLeft = centerLeft + (upwardsPerpendicular * halfHeight);
        bottomLeft = centerLeft - (upwardsPerpendicular * halfHeight);

        centerRight = center - (perpendicular * halfWidth);
        topRight = centerRight + (upwardsPerpendicular * halfHeight);
        bottomRight = centerRight - (upwardsPerpendicular * halfHeight);
    }

    /// <summary>
    /// Switch the left side points of this cross section with another
    /// cross section's left side points.
    /// </summary>
    /// <param name="other"></param>
    public void swapLeftSide(MeshRoadCrossSection other)
    {
        Vector3 tmpCenterLeft = centerLeft;
        Vector3 tmpTopLeft = topLeft;
        Vector3 tmpBottomLeft = bottomLeft;

        centerLeft = other.centerLeft;
        topLeft = other.topLeft;
        bottomLeft = other.bottomLeft;

        other.centerLeft = tmpCenterLeft;
        other.topLeft = tmpTopLeft;
        other.bottomLeft = tmpBottomLeft;
    }

    /// <summary>
    /// Switch the right side points of this cross section with another
    /// cross section's right side points.
    /// </summary>
    /// <param name="other"></param>
    public void swapRightSide(MeshRoadCrossSection other)
    {
        Vector3 tmpCenterRight = centerRight;
        Vector3 tmpTopRight = topRight;
        Vector3 tmpBottomRight = bottomRight;

        centerRight = other.centerRight;
        topRight = other.topRight;
        bottomRight = other.bottomRight;

        other.centerRight = tmpCenterRight;
        other.topRight = tmpTopRight;
        other.bottomRight = tmpBottomRight;
    }
}

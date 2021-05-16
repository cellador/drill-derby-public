using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to programmatically define a rectangle as a UI element.
/// </summary>
[ExecuteInEditMode]
public class SimpleQuad : Graphic
{
    public Vector2[] pos = new Vector2[4];
    new Color color = new Color(0.9f,0.9f,0.9f,1f);

    /// <summary>
    /// Display the rectangle at world coordinates.
    /// </summary>
    /// <param name="newPos">New position in world coordinates.</param>
    /// <param name="width">Width of the rectangle.</param>
    /// <param name="height">Height of the rectangle.</param>
    public void ShowAtWorld(Vector3 newPos, float width = 10f, float height = 10f)
    {
        newPos = (Vector2) rectTransform.InverseTransformPoint(newPos);
        pos[0] = new Vector2(newPos[0]-width, newPos[1]+height);
        pos[1] = new Vector2(newPos[0]-width, newPos[1]-height);
        pos[2] = new Vector2(newPos[0]+width, newPos[1]-height);
        pos[3] = new Vector2(newPos[0]+width, newPos[1]+height);
        UpdateGeometry();
        enabled = true;
    }

    /// <summary>
    /// Display the rectangle at UI coordinates.
    /// </summary>
    /// <param name="newPos">New position in UI coordinates.</param>
    /// <param name="width">Width of the rectangle.</param>
    /// <param name="height">Height of the rectangle.</param>
    public void ShowAtUI(Vector3 newPos, float width = 10f, float height = 10f)
    {
        newPos = (Vector2) rectTransform.InverseTransformPoint(newPos*canvas.scaleFactor);
        pos[0] = new Vector2(newPos[0]-width, newPos[1]+height);
        pos[1] = new Vector2(newPos[0]-width, newPos[1]-height);
        pos[2] = new Vector2(newPos[0]+width, newPos[1]-height);
        pos[3] = new Vector2(newPos[0]+width, newPos[1]+height);
        UpdateGeometry();
        enabled = true;
    }

    /// <summary>
    /// Hide the rectangle.
    /// </summary>
    public void Hide()
    {
        enabled = false;
    }

    /// <summary>
    /// Defines where the rectangle is drawn when calling UpdateGeometry().
    /// </summary>
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        UIVertex vert = UIVertex.simpleVert;

        for (int i = 0; i < 4; i++)
        {
            vert.position = pos[i];
            vert.color = color;
            vh.AddVert(vert);
        }

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}

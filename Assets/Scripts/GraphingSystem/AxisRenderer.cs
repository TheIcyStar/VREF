using Palmmedia.ReportGenerator.Core;
using UnityEngine;

public class AxisRenderer : MonoBehaviour
{
    // public values assigned in inspector
    public LineRenderer xAxis, yAxis, zAxis;
    public Transform xStartArrow, xEndArrow, yStartArrow, yEndArrow, zStartArrow, zEndArrow;

    // does nothing for now
    public void InitializeAxes()
    {
        // pass
    }

    // updates all three axes based on new graph settings
    public void UpdateAxes(GraphSettings graphSettings)
    {
        // this is the exact same calculation as EquationGrapher.ScaleGraph
        // maybe logic can be moved to Graph manager somehow?
        // not a big deal
        float baseRange = 2f;
        float maxRange = Mathf.Max(graphSettings.xMax - graphSettings.xMin, graphSettings.yMax - graphSettings.yMin, graphSettings.zMax - graphSettings.zMin);
        float scaleFactor = baseRange / maxRange;

        float xLength = (graphSettings.xMax - graphSettings.xMin) * scaleFactor;
        float yLength = (graphSettings.yMax - graphSettings.yMin) * scaleFactor;
        float zLength = (graphSettings.zMax - graphSettings.zMin) * scaleFactor;

        float zOffset = -graphSettings.zMin * scaleFactor;

        UpdateAxis(xAxis, 0, xLength, zOffset);
        UpdateAxis(yAxis, 1, yLength, zOffset);
        UpdateAxis(zAxis, 2, zLength, zOffset);
    }

    // updates a specific axis's length
    private void UpdateAxis(LineRenderer line, int direction, float length, float zOffset)
    {
        Transform startArrow = xStartArrow;
        Transform endArrow = xEndArrow;
        Vector3 directionVector = Vector3.forward;

        switch(direction) {
            // x
            case 0:
                // defaults
                break;
            // y
            case 1:
                startArrow = yStartArrow;
                endArrow = yEndArrow;
                directionVector = Vector3.right;
                break;
            // z
            case 2:
                startArrow = zStartArrow;
                endArrow = zEndArrow;
                directionVector = Vector3.up;
                break;
        }

        line.positionCount = 2;
        Vector3 displacement = new Vector3(0, zOffset, 0);
        Vector3 axisStartPos = displacement - (directionVector * length / 2);
        Vector3 axisEndPos = displacement + (directionVector * length / 2);
        line.SetPositions(new Vector3[] { axisStartPos, axisEndPos });

        startArrow.localPosition = axisStartPos;
        endArrow.localPosition = axisEndPos;
    }
}

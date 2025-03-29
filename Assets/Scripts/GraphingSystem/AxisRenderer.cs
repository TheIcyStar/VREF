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
        // OLD IMPLEMENTATION, MAY NOT LINE UP CORRECTLY ANYMORE
        // -----------------------------------------------------
        // this scales each axis directly based on its range
        // ex: x [-2, 2], y [-100, 100] makes x axis VERY tiny
        // -----------------------------------------------------
        // float xLength = (graphSettings.xMax - graphSettings.xMin) * scaleFactor;
        // float yLength = (graphSettings.yMax - graphSettings.yMin) * scaleFactor;
        // float zLength = (graphSettings.zMax - graphSettings.zMin) * scaleFactor;

        // this is unscaled (the rendered graph will line up with this)
        float maxRange = 2f;

        UpdateAxis(xAxis, Vector3.forward, maxRange, xStartArrow, xEndArrow);
        UpdateAxis(yAxis, Vector3.right, maxRange, yStartArrow, yEndArrow);
        UpdateAxis(zAxis, Vector3.up, maxRange, zStartArrow, zEndArrow);
    }

    // updates a specific axis's length
    private void UpdateAxis(LineRenderer line, Vector3 direction, float length, Transform startArrow, Transform endArrow)
    {
        line.positionCount = 2;
        Vector3 axisStartPos = -(direction * length / 2);
        Vector3 axisEndPos = direction * length / 2;
        line.SetPositions(new Vector3[] { axisStartPos, axisEndPos });

        startArrow.localPosition = axisStartPos;
        endArrow.localPosition = axisEndPos;
    }
}

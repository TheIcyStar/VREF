using Palmmedia.ReportGenerator.Core;
using UnityEngine;

public class AxisRenderer : MonoBehaviour
{
    private LineRenderer xAxis, yAxis, zAxis;
    private Transform xArrow, yArrow, zArrow;

    // get the line renderers from the three axes objects
    public void InitializeAxes()
    {
        Transform xAxisObj = transform.GetChild(0);
        Transform yAxisObj = transform.GetChild(1);
        Transform zAxisObj = transform.GetChild(2);

        xAxis = xAxisObj.GetComponent<LineRenderer>();
        yAxis = yAxisObj.GetComponent<LineRenderer>();
        zAxis = zAxisObj.GetComponent<LineRenderer>();

        xArrow = xAxisObj.GetChild(0).transform;
        yArrow = yAxisObj.GetChild(0).transform;
        zArrow = zAxisObj.GetChild(0).transform;
    }

    // updates all three axes based on new graph settings
    public void UpdateAxes(GraphSettings graphSettings, float lowerGraphMargin)
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

        float yOffset = -graphSettings.yMin * scaleFactor;

        UpdateAxis(xAxis, Vector3.right, xLength, yOffset, xArrow, lowerGraphMargin);
        UpdateAxis(yAxis, Vector3.up, yLength, yOffset, yArrow, lowerGraphMargin);
        UpdateAxis(zAxis, Vector3.forward, zLength, yOffset, zArrow, lowerGraphMargin);
    }

    // updates a specific axis's length
    private void UpdateAxis(LineRenderer line, Vector3 direction, float length, float yOffset, Transform arrow, float lowerGraphMargin)
    {
        line.positionCount = 2;
        Vector3 displacement = new Vector3(0, yOffset, 0);
        line.SetPositions(new Vector3[] { displacement - (direction * length / 2), displacement + (direction * length / 2) });

        float scalePosition = (displacement + (direction * length / 2)).y;

        if (direction == Vector3.right) {
            arrow.localPosition = new Vector3(1, scalePosition, 0);
            arrow.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 0, 90);
        }
        else if (direction == Vector3.up) {
            arrow.localPosition = new Vector3(0, scalePosition - lowerGraphMargin, -1);
            arrow.rotation = Quaternion.LookRotation(direction);
        }
        else if (direction == Vector3.forward) {
            arrow.localPosition = new Vector3(0, scalePosition + lowerGraphMargin, 0);
            arrow.rotation = Quaternion.LookRotation(direction);
        }
    }
}

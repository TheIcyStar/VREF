using Palmmedia.ReportGenerator.Core;
using UnityEngine;

public class AxisRenderer : MonoBehaviour
{
    private LineRenderer xAxis, yAxis, zAxis;

    // get the line renderers from the three axes objects
    public void InitializeAxes()
    {
        xAxis = transform.GetChild(0).GetComponent<LineRenderer>();
        yAxis = transform.GetChild(1).GetComponent<LineRenderer>();
        zAxis = transform.GetChild(2).GetComponent<LineRenderer>();
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

        float yOffset = -graphSettings.yMin * scaleFactor;

        UpdateAxis(xAxis, Vector3.right, xLength, yOffset);
        UpdateAxis(yAxis, Vector3.up, yLength, yOffset);
        UpdateAxis(zAxis, Vector3.forward, zLength, yOffset);
    }

    // updates a specific axis's length
    private void UpdateAxis(LineRenderer line, Vector3 direction, float length, float yOffset)
    {
        line.positionCount = 2;
        Vector3 displacement = new Vector3(0, yOffset, 0);
        line.SetPositions(new Vector3[] { displacement - (direction * length / 2), displacement + (direction * length / 2) });
    }
}

using UnityEngine;

public struct GraphSettings
{
    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;
    public float step;

    public GraphSettings(float xMin, float xMax, float yMin, float yMax, float step)
    {
        this.xMin = xMin;
        this.xMax = xMax;
        this.yMin = yMin;
        this.yMax = yMax;
        this.step = step;
    }
}
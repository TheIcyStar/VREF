using UnityEngine;

public struct GraphSettings
{
    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;
    public float zMin;
    public float zMax;
    public float step;

    public GraphSettings(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax, float step)
    {
        this.xMin = xMin;
        this.xMax = xMax;
        this.yMin = yMin;
        this.yMax = yMax;
        this.zMin = zMin;
        this.zMax = zMax;
        this.step = step;
    }

    public string ToJSON() {
        return JsonUtility.ToJson(this);
    }
}
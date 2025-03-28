using UnityEngine;

public static class GraphUtils
{
    public static float CalculateScaleFactor(GraphSettings settings, float baseRange = 2f)
    {
        return baseRange / (CalculateMaxRange(settings) * 2);
    }

    public static float CalculateMaxRange(GraphSettings settings)
    {
        return Mathf.Max(
            Mathf.Abs(settings.xMin), Mathf.Abs(settings.xMax),
            Mathf.Abs(settings.yMin), Mathf.Abs(settings.yMax),
            Mathf.Abs(settings.zMin), Mathf.Abs(settings.zMax)
        );
    }
}

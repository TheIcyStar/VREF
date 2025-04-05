using NUnit.Framework;

public class IGraphRendererTests
{
    [Test]
    public void AxisMin_ReturnsCorrectMinimum()
    {
        var settings = new GraphSettings
        {
            xMin = -5f,
            yMin = -10f,
            zMin = -15f
        };

        Assert.AreEqual(-5f, IGraphRenderer.GetAxisMin(settings, GraphVariable.X));
        Assert.AreEqual(-10f, IGraphRenderer.GetAxisMin(settings, GraphVariable.Y));
        Assert.AreEqual(-15f, IGraphRenderer.GetAxisMin(settings, GraphVariable.Z));
    }

    [Test]
    public void AxisMax_ReturnsCorrectMaximum()
    {
        var settings = new GraphSettings
        {
            xMax = 5f,
            yMax = 10f,
            zMax = 15f
        };

        Assert.AreEqual(5f, IGraphRenderer.GetAxisMax(settings, GraphVariable.X));
        Assert.AreEqual(10f, IGraphRenderer.GetAxisMax(settings, GraphVariable.Y));
        Assert.AreEqual(15f, IGraphRenderer.GetAxisMax(settings, GraphVariable.Z));
    }

    [Test]
    public void AxisOrigin()
    {
        var settings = new GraphSettings
        {
            xMax = 0f,
            yMax = 0f,
            zMax = 0f
        };

        Assert.AreEqual(0f, IGraphRenderer.GetAxisMax(settings, GraphVariable.X));
        Assert.AreEqual(0f, IGraphRenderer.GetAxisMax(settings, GraphVariable.Y));
        Assert.AreEqual(0f, IGraphRenderer.GetAxisMax(settings, GraphVariable.Z));
    }
    [Test]
    public void AxisTwoDimensional()
    {
        var settings = new GraphSettings
        {
            xMax = 5f,
            yMax = 10f,
            zMax = 0f
        };

        Assert.AreEqual(5f, IGraphRenderer.GetAxisMax(settings, GraphVariable.X));
        Assert.AreEqual(10f, IGraphRenderer.GetAxisMax(settings, GraphVariable.Y));
        Assert.AreEqual(0f, IGraphRenderer.GetAxisMax(settings, GraphVariable.Z));
    }
}

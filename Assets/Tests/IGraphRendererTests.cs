using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
}

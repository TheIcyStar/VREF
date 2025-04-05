// using NUnit.Framework;
// using UnityEngine;
// using System.Collections.Generic;

// public class LineGraphRendererTests
// {
//     private GameObject testParent;
//     private Material testMaterial;

//     [SetUp]
//     public void Setup()
//     {
//         testParent = new GameObject("GraphParent");
//         testMaterial = new Material(Shader.Find("Sprites/Default"));
//     }

//     [TearDown]
//     public void Teardown()
//     {
//         Object.DestroyImmediate(testParent);
//     }

//     [Test]
//     public void LineGraphRenderer_RendersLine_WithValidInput()
//     {
//         var renderer = new LineGraphRenderer(testParent.transform, testMaterial);

//         var settings = new GraphSettings
//         {
//             xMin = 0,
//             xMax = 1,
//             yMin = 0,
//             yMax = 1,
//             step = 0.1f
//         };

//         var inputVars = new HashSet<GraphVariable> { GraphVariable.X };
     
//         var lineRenderers = testParent.GetComponentsInChildren<LineRenderer>();
//         Assert.IsNotEmpty(lineRenderers);
//         foreach (var line in lineRenderers)
//         {
//             Assert.IsTrue(line.enabled);
//             Assert.Greater(line.positionCount, 0);
//         }
//     }

// }

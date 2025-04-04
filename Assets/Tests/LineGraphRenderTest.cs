// using NUnit.Framework;
// using UnityEngine;

// public class LineGraphRendererTest
// {
//     Parser: ParseTreeNode

//     [Test]
//     public void RenderGraph_GeneratesCorrectLine_ForYEqualsX()
//     {
//         GameObject parentObject = new GameObject("GraphParent");
//         Material testMaterial = new Material(Shader.Find("Sprites/Default"));
//         LineGraphRenderer renderer = new LineGraphRenderer(parentObject.transform, testMaterial);

//         GraphSettings settings = new GraphSettings
//         {
//             xMin = 0f,
//             xMax = 1f,
//             yMin = 0f,
//             yMax = 1f,
//             step = 0.5f
//         };

//         GraphVariable inputVar = GraphVariable.X;
//         GraphVariable outputVar = GraphVariable.Y;
//         HashSet<GraphVariable> inputVars = new() { inputVar };

//         ParseTreeNode dummyTree = new DummyParseTreeNode();

//         var originalMethod = typeof(IGraphRenderer).GetMethod("EvaluateEquation");
//         var patchMethod = typeof(MockGraphRenderer).GetMethod("MockEvaluateEquation");
//         HarmonyLib.Harmony harmony = new("test.patch.graph");
//         harmony.Patch(originalMethod, prefix: new HarmonyLib.HarmonyMethod(patchMethod));

//         renderer.RenderGraph(dummyTree, settings, inputVars, outputVar);

//         var lines = parentObject.GetComponentsInChildren<LineRenderer>();
//         Assert.AreEqual(1, lines.Length, "Should have 1 line segment.");
//         Assert.AreEqual(3, lines[0].positionCount, "Segment should have 3 points: 0, 0.5, 1.");

//         Vector3[] expected = new Vector3[]
//         {
//             new Vector3(0f, 0f, 0f),
//             new Vector3(0.5f, 0f, 0.5f),
//             new Vector3(1f, 0f, 1f)
//         };

//         Vector3[] actual = new Vector3[3];
//         lines[0].GetPositions(actual);

//     }
// }

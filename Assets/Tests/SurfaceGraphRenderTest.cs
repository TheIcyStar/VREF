// using NUnit.Framework;
// using UnityEngine;

// public class SurfaceGraphRendererTests
// {
//     [SetUp]
//     public void Setup()
//     {
//         graphParentObj = new GameObject("GraphParent");
//         graphParent = graphParentObj.transform;

//         testMaterial = new Material(Shader.Find("Standard"));

//         renderer = new SurfaceGraphRenderer(graphParent, testMaterial);
//     }


//     [Test]
//     public void RenderGraph_GeneratesMeshObject()
//     {
//         var equationTree = new ParseTreeNode("=")
//         {
//             right = new ParseTreeNode("+")
//             {
//                 left = new ParseTreeNode("x"),
//                 right = new ParseTreeNode("y")
//             }
//         };

//         var settings = new GraphSettings
//         {
//             step = 1f,
//             axisMins = new Dictionary<GraphVariable, float>
//             {
//                 { GraphVariable.X, 0f },
//                 { GraphVariable.Y, 0f },
//                 { GraphVariable.Z, 0f }
//             },
//             axisMaxs = new Dictionary<GraphVariable, float>
//             {
//                 { GraphVariable.X, 2f },
//                 { GraphVariable.Y, 2f },
//                 { GraphVariable.Z, 10f }
//             }
//         };

//         var inputVars = new HashSet<GraphVariable> { GraphVariable.X, GraphVariable.Y };
//         var outputVar = GraphVariable.Z;
//         renderer.RenderGraph(equationTree, settings, inputVars, outputVar);
//         Assert.Greater(graphParent.childCount, 0);
//         var meshObj = graphParent.GetChild(0).gameObject;
//         var meshFilter = meshObj.GetComponent<MeshFilter>();
//         var meshRenderer = meshObj.GetComponent<MeshRenderer>();

//         Assert.IsNotNull(meshFilter);
//         Assert.IsNotNull(meshRenderer);
//         Assert.IsTrue(meshRenderer.enabled);
//         Assert.Greater(meshFilter.mesh.vertexCount, 0);
//         Assert.Greater(meshFilter.mesh.triangles.Length, 0);
//     }
// }

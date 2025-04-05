// using NUnit.Framework;
// using UnityEngine;
// using System.Collections.Generic;
// using System.Linq;

// namespace Tests
// {

//     public class SurfaceGraphRendererTests
//     {
//         private GameObject graphParent;
//         private Material testMaterial;
//         private SurfaceGraphRenderer renderer;
//         private GraphSettings settings;
//         private ParseTreeNode equationTree;
//         private HashSet<GraphVariable> inputVars;
//         private GraphVariable outputVar;

//         [SetUp]
//         public void Setup()
//         {
//             graphParent = new GameObject("GraphParent");

//             testMaterial = new Material(Shader.Find("Standard"));

//             renderer = new SurfaceGraphRenderer(graphParent.transform, testMaterial);

//             settings = new GraphSettings { step = 1f };

//             inputVars = new HashSet<GraphVariable> { GraphVariable.X, GraphVariable.Y };

//             outputVar = GraphVariable.Z;
//         }

//         [TearDown]
//         public void Teardown()
//         {
//             Object.DestroyImmediate(graphParent);
//             Object.DestroyImmediate(testMaterial);
//         }

//         [Test]
//         public void RenderGraph_GeneratesValidMeshSegments()
//         {
//             renderer.RenderGraph(equationTree, settings, inputVars, outputVar);

//             Assert.IsTrue(graphParent.transform.childCount > 0);

//             foreach (Transform child in graphParent.transform)
//             {
//                 var meshFilter = child.GetComponent<MeshFilter>();
//                 var meshRenderer = child.GetComponent<MeshRenderer>();

//                 Assert.IsNotNull(meshFilter);
//                 Assert.IsNotNull(meshRenderer);
//                 Assert.IsNotNull(meshFilter.mesh);

//                 Assert.Greater(meshFilter.mesh.vertexCount, 0);
//                 Assert.Greater(meshFilter.mesh.triangles.Length, 0);
//             }
//         }
//     }
// }

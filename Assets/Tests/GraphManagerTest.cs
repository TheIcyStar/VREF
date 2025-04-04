// using System;
// using System.Collections;
// using System.Collections.Generic;
// using NUnit.Framework;



// public class GraphManagerTest
// {
//     [SetUp]
//     public void Setup()
//     {
//         public DummyGraphSettings() : base(-10, 10, -10, 10, -10, 10, 0.1f);
//         equationUIGO.transform.forward = Vector3.forward;
//         graphManager.equationUITransform = equationUIGO.transform;

//         graphManager.defaultLineColor = new Material(Shader.Find("Standard"));
//         graphManager.defaultMeshColor = new Material(Shader.Find("Standard"));

//         graphManager.globalGraphSettings = new DummyGraphSettings();

//         graphManager.Start();
//     }

//     [Test]
//     public void TestCreateNewGraph()
//     {
//         DummyParseTreeNode dummyTree = new DummyParseTreeNode(1);
//         int initialGraphCount = graphManager.GetGraphs().Length;

//         graphManager.CreateNewGraph(dummyTree);

//         ParseTreeNode[] graphsAfter = graphManager.GetGraphs();
//         Assert.AreEqual(initialGraphCount + 1, graphsAfter.Length);

//         Transform createdGraphTransform = graphManagerGO.transform.Find("Graph 1");
//         Assert.IsNotNull(createdGraphTransform);

//         DummyGraphInstance instance = createdGraphTransform.GetComponent<DummyGraphInstance>();
//         Assert.IsNotNull(instance);
//         Assert.IsTrue(instance.initialized);
//         Assert.AreEqual(dummyTree, instance.storedTree);
//     }

// }

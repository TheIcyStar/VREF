// using UnityEngine;
// using NUnit.Framework;

// public class GraphInstanceTest
// {

//     [SetUp]
//     public void Setup()
//     {
//         testObject = new GameObject();
//         graphInstanceTest = testObject.AddComponent<GraphInstanceTest>();

//         mockGraphInstance = new GraphInstance();

//         graphInstanceTest.graphInstance = mockGraphInstance;

//         graphInstanceTest.xAxisMinUI = new TMP_InputField();
//         graphInstanceTest.xAxisMaxUI = new TMP_InputField();
//         graphInstanceTest.yAxisMinUI = new TMP_InputField();
//         graphInstanceTest.yAxisMaxUI = new TMP_InputField();
//         graphInstanceTest.zAxisMinUI = new TMP_InputField();
//         graphInstanceTest.zAxisMaxUI = new TMP_InputField();
//         graphInstanceTest.xRotationUI = new TMP_InputField();
//         graphInstanceTest.yRotationUI = new TMP_InputField();
//         graphInstanceTest.zRotationUI = new TMP_InputField();
//         graphInstanceTest.stepUI = new TMP_InputField();
//         graphInstanceTest.InitializeGraphInstance();
//     }

//     [Test]
//     public void TestInitializeGraphInstance()
//     {

//         Assert.NotNull(graphInstanceTest.graphInstance);
//         Assert.AreEqual(0, graphInstanceTest.xAxisMinUI.text);
//         Assert.AreEqual(10, graphInstanceTest.xAxisMaxUI.text);
//         Assert.AreEqual(-5, graphInstanceTest.yAxisMinUI.text);
//         Assert.AreEqual(5, graphInstanceTest.yAxisMaxUI.text);
//         Assert.AreEqual(0, graphInstanceTest.zAxisMinUI.text);
//         Assert.AreEqual(20, graphInstanceTest.zAxisMaxUI.text);
//         Assert.AreEqual("30", graphInstanceTest.xRotationUI.text);
//         Assert.AreEqual("45", graphInstanceTest.yRotationUI.text);
//         Assert.AreEqual("60", graphInstanceTest.zRotationUI.text);
//         Assert.AreEqual("0.1", graphInstanceTest.stepUI.text);
//     }

//     [Test]
//     public void TestSimulateUIInteraction()
//     {
//         graphInstanceTest.SimulateUIInteraction();
//         Assert.AreEqual("0", graphInstanceTest.xAxisMinUI.text);
//         Assert.AreEqual("10", graphInstanceTest.xAxisMaxUI.text);
//         Assert.AreEqual("-5", graphInstanceTest.yAxisMinUI.text);
//         Assert.AreEqual("5", graphInstanceTest.yAxisMaxUI.text);
//         Assert.AreEqual("0", graphInstanceTest.zAxisMinUI.text);
//         Assert.AreEqual("20", graphInstanceTest.zAxisMaxUI.text);
//         Assert.AreEqual("30", graphInstanceTest.xRotationUI.text);
//         Assert.AreEqual("45", graphInstanceTest.yRotationUI.text);
//         Assert.AreEqual("60", graphInstanceTest.zRotationUI.text);
//         Assert.AreEqual("0.1", graphInstanceTest.stepUI.text);

//     }

// }

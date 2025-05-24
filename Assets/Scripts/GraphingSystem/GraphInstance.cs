using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEditor.Graphs;
using UnityEngine;
using System.Linq;

public class GraphInstance : MonoBehaviour
{
    private class GraphData {
        public ParseTreeNode equation;
        public IGraphRenderer renderer;
        public GameObject visualObject;
        public HashSet<GraphVariable> inputVars;
        public GraphVariable outputVar;
        public Material material;
    }

    private List<GraphData> graphs = new();
    // graph settings passed in
    public GraphSettings graphSettings;
    // store original settings
    public GraphSettings originalSettings;
    // store references to scripts and objects
    [SerializeField] private AxisRenderer axisRenderer;
    [SerializeField] private GameObject graphVisualsObj;
    [SerializeField] private GameObject graphObj;

    public void AddEquation(ParseTreeNode equationTree, GraphSettings graphSettings)
    {
        // determine what equation type is by parsing the whole tree
        var (equationType, inputVars, outputVar) = GraphUtils.DetermineEquationType(equationTree);

        this.graphSettings = graphSettings;
        this.originalSettings = graphSettings;

        // create a new graph object to add the equation to
        GameObject graphVisual = new GameObject($"Graph Visual {graphs.Count + 1}");
        graphVisual.transform.SetParent(graphVisualsObj.transform, false);

        // pass in the gameobject to add renderers to
        // if(equationType == TYPE_LINE) graphRenderer = new LineGraphRenderer(graphVisual.transform, GraphManager.instance.defaultLineColor);
        // else if(equationType == TYPE_SURFACE) graphRenderer = new SurfaceGraphRenderer(graphVisual.transform, GraphManager.instance.defaultMeshColor);
        // else                          throw new GraphEvaluationException("Unsupported equation type attempting to be graphed.");

        Material mat = (equationType == EquationType.LINE) ? GraphManager.instance.defaultLineColor : GraphManager.instance.defaultMeshColor;

        IGraphRenderer graphRenderer = equationType switch {
            EquationType.LINE => new LineGraphRenderer(graphVisual.transform, mat),
            EquationType.SURFACE => new SurfaceGraphRenderer(graphVisual.transform, mat),
            _ => throw new GraphEvaluationException("Unsupported equation type."),
        };

        var graph = new GraphData {
            equation = equationTree,
            renderer = graphRenderer,
            visualObject = graphVisual,
            inputVars = inputVars,
            outputVar = outputVar,
            material = mat
        };

        graphs.Add(graph);

        axisRenderer.InitializeAxes();
        GraphManager.instance.RefreshAllUIText();
        ScaleGraph();

        VisualizeGraph(graph);
    }

    public void SetNewMaterial(Material newColor, ParseTreeNode equation) {
        var graph = graphs.Find(g => g.equation == equation);

        Destroy(graph.visualObject);

        GameObject newVisual = new GameObject($"Graph Visual {graphs.IndexOf(graph) + 1}");
        newVisual.transform.SetParent(graphVisualsObj.transform, false);

        EquationType equationType = graph.inputVars.Count switch {
            1 => EquationType.LINE,
            2 => EquationType.SURFACE,
            _ => throw new GraphEvaluationException("Unsupported variable amount.")
        };

        IGraphRenderer newRenderer = equationType switch {
            EquationType.LINE => new LineGraphRenderer(newVisual.transform, newColor),
            EquationType.SURFACE => new SurfaceGraphRenderer(newVisual.transform, newColor),
            _ => throw new GraphEvaluationException("Unsupported equation type.")
        };

        graph.visualObject = newVisual;
        graph.renderer = newRenderer;
        graph.material = newColor;

        VisualizeGraph(graph);
    }

    public Material GetMaterialForEquation(ParseTreeNode equation) {
        var graph = graphs.Find(g => g.equation == equation);
        return graph?.material;
    }

    public void RefreshAllGraphs() {
        foreach (var graph in graphs) {
            Object.Destroy(graph.visualObject);
            GameObject newVisual = new GameObject($"Graph Visual {graphs.IndexOf(graph) + 1}");
            newVisual.transform.SetParent(graphVisualsObj.transform, false);

            EquationType equationType = graph.inputVars.Count switch {
                1 => EquationType.LINE,
                2 => EquationType.SURFACE,
                _ => throw new GraphEvaluationException("Unsupported variable amount.")
            };

            IGraphRenderer newRenderer = equationType switch {
                EquationType.LINE => new LineGraphRenderer(newVisual.transform, graph.material),
                EquationType.SURFACE => new SurfaceGraphRenderer(newVisual.transform, graph.material),
                _ => throw new GraphEvaluationException("Unsupported equation type.")
            };

            graph.visualObject = newVisual;
            graph.renderer = newRenderer;

            VisualizeGraph(graph);
        }
    }

    public void RemoveEquation(ParseTreeNode equation) {
        var graph = graphs.Find(g => g.equation == equation);
        if (graph == null) return;

        Object.Destroy(graph.visualObject);
        graphs.Remove(graph);
    }

    public List<ParseTreeNode> GetEquations() {
        return graphs.Select(g => g.equation).ToList();
    }

    // scale the graph based on the largest range
    public void ScaleGraph()
    {
        // scale the graph object so that the largest dimension fits in view
        float scaleFactor = GraphUtils.CalculateScaleFactor(graphSettings);
        graphVisualsObj.transform.localScale = Vector3.one * scaleFactor;

        // scale the axes
        axisRenderer.UpdateAxes(graphSettings);
    }

    public void ReVisualize() {
        foreach(GraphData graph in graphs)
        {
            VisualizeGraph(graph);
        }
    }

    // renders the graph using the respective renderer and settings
    private void VisualizeGraph(GraphData graph) {
        graph.renderer.RenderGraph(graph.equation, graphSettings, graph.inputVars, graph.outputVar);
    }

    // rotate the graph object (called from the gizmo)
    public void GizmoRotateGraph(Vector3 rotation) {
        graphObj.transform.localRotation = Quaternion.Euler(rotation);
        //GraphManager.instance.RefreshUIText(xRotationUI, rotation.y);
        //GraphManager.instance.RefreshUIText(yRotationUI, rotation.z);
        //GraphManager.instance.RefreshUIText(zRotationUI, rotation.x);
    }
}

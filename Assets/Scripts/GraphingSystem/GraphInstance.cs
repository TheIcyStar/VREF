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

    // equation renderer types
    // change to enums later
    private const int TYPE_LINE = 0;
    private const int TYPE_SURFACE = 1;

    public void AddEquation(ParseTreeNode equationTree, GraphSettings graphSettings)
    {
        // determine what equation type is by parsing the whole tree
        var (equationType, inputVars, outputVar) = DetermineEquationType(equationTree);

        this.graphSettings = graphSettings;
        this.originalSettings = graphSettings;

        // create a new graph object to add the equation to
        GameObject graphVisual = new GameObject($"Graph Visual {graphs.Count + 1}");
        graphVisual.transform.SetParent(graphVisualsObj.transform, false);

        // pass in the gameobject to add renderers to
        // if(equationType == TYPE_LINE) graphRenderer = new LineGraphRenderer(graphVisual.transform, GraphManager.instance.defaultLineColor);
        // else if(equationType == TYPE_SURFACE) graphRenderer = new SurfaceGraphRenderer(graphVisual.transform, GraphManager.instance.defaultMeshColor);
        // else                          throw new GraphEvaluationException("Unsupported equation type attempting to be graphed.");

        Material mat = (equationType == TYPE_LINE) ? GraphManager.instance.defaultLineColor : GraphManager.instance.defaultMeshColor;

        IGraphRenderer graphRenderer = equationType switch {
            TYPE_LINE => new LineGraphRenderer(graphVisual.transform, mat),
            TYPE_SURFACE => new SurfaceGraphRenderer(graphVisual.transform, mat),
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

        int equationType = graph.inputVars.Count switch {
            1 => TYPE_LINE,
            2 => TYPE_SURFACE,
            _ => throw new GraphEvaluationException("Unsupported variable amount.")
        };

        IGraphRenderer newRenderer = equationType switch {
            TYPE_LINE => new LineGraphRenderer(newVisual.transform, newColor),
            TYPE_SURFACE => new SurfaceGraphRenderer(newVisual.transform, newColor),
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

            int equationType = graph.inputVars.Count switch {
                1 => TYPE_LINE,
                2 => TYPE_SURFACE,
                _ => throw new GraphEvaluationException("Unsupported variable amount.")
            };

            IGraphRenderer newRenderer = equationType switch {
                TYPE_LINE => new LineGraphRenderer(newVisual.transform, graph.material),
                TYPE_SURFACE => new SurfaceGraphRenderer(newVisual.transform, graph.material),
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

    // determines the type of equation by analyzing the parse tree
    // also returns the input and output variables
    // ONLY WORKS WITH [output var] = [f(input var)] FOR NOW
    private (int, HashSet<GraphVariable>, GraphVariable) DetermineEquationType(ParseTreeNode equationTree) {
        // initialize a set to store all the input vars
        HashSet<GraphVariable> inputVars = new HashSet<GraphVariable>();
        GraphVariable outputVar;

        if(equationTree == null || equationTree.token.text != "=") {
            throw new GraphEvaluationException("Explicit equation not found.");
        }

        // find output var (left of equal sign)
        outputVar = ConvertToGraphVariable(equationTree.left.token.text);

        // intialize equation type
        int equationType;

        // add all input variables to the set (right of equal sign)
        DetermineVariables(equationTree.right, inputVars);

        // check to see if there is no explicit variable
        if(inputVars.Contains(outputVar)) throw new GraphEvaluationException("Variable found on left and right hand side of equation.");

        // this logic needs to change to support constants, parametrics, planes, etc... in the future
        if (inputVars.Count == 1)                         equationType = TYPE_LINE;
        else if (inputVars.Count == 2)                    equationType = TYPE_SURFACE;
        else                                              throw new GraphEvaluationException("Unsupported variable amount in right hand side of equation.");

        return (equationType, inputVars, outputVar);
    }

    // finds all the variables in the given tree and adds them to a set
    // potentially could create the set during tree creation if this
    // full tree traversal ends up being too costly
    private void DetermineVariables(ParseTreeNode equationTree, HashSet<GraphVariable> variables) {
        if (equationTree == null) return;

        if (equationTree.token.type == TokenType.Variable) {
            variables.Add(ConvertToGraphVariable(equationTree.token.text));
        }

        DetermineVariables(equationTree.left, variables);
        DetermineVariables(equationTree.right, variables);
    }

    // converts a string to a GraphVariable
    // if we really want to optimize everything down the line
    // we can change the tree to use an enum for every possible
    // token to avoid using strings entirely
    private GraphVariable ConvertToGraphVariable(string stringVar)
    {
        return stringVar switch
        {
            "x" => GraphVariable.X,
            "y" => GraphVariable.Y,
            "z" => GraphVariable.Z,
            _ => throw new GraphEvaluationException("Unknown/missing variable attempting to be graphed.")
        };
    }
}

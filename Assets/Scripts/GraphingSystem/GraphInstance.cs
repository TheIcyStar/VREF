using System.Collections.Generic;
using TMPro;
using UnityEditor.Graphs;
using UnityEngine;

public class GraphInstance : MonoBehaviour
{
    // the list of equations on this graph object
    public List<ParseTreeNode> equationTrees = new();

    // renderer interface set to the correct renderer type
    private IGraphRenderer graphRenderer;
    // graph settings passed in
    private GraphSettings graphSettings;
    // store original settings
    private GraphSettings originalSettings;
    // set of input variables (independent variables)
    private HashSet<GraphVariable> inputVars;
    // single output var computed by the function (dependent variable)
    private GraphVariable outputVar;
    // store references to scripts and objects
    [SerializeField] private AxisRenderer axisRenderer;
    [SerializeField] private GameObject graphVisualsObj;
    [SerializeField] private GameObject graphObj;

    // all graph settings UI elements
    [SerializeField] private TMP_InputField xAxisMinUI;
    [SerializeField] private TMP_InputField xAxisMaxUI;
    [SerializeField] private TMP_InputField yAxisMinUI;
    [SerializeField] private TMP_InputField yAxisMaxUI;
    [SerializeField] private TMP_InputField zAxisMinUI;
    [SerializeField] private TMP_InputField zAxisMaxUI;
    [SerializeField] private TMP_InputField xRotationUI;
    [SerializeField] private TMP_InputField yRotationUI;
    [SerializeField] private TMP_InputField zRotationUI;
    [SerializeField] private TMP_InputField stepUI;

    // equation renderer types
    // change to enums later
    private const int TYPE_LINE = 0;
    private const int TYPE_SURFACE = 1;

    public void AddEquation(ParseTreeNode equationTree) 
    {
        // determine what equation type is by parsing the whole tree
        var (equationType, inputVars, outputVar) = DetermineEquationType(equationTree);

        equationTrees.Add(equationTree);
        this.graphSettings = GraphManager.instance.globalGraphSettings;
        this.originalSettings = GraphManager.instance.globalGraphSettings;
        this.inputVars = inputVars;
        this.outputVar = outputVar;

        // create a new graph object to add the equation to
        GameObject graphVisual = new GameObject($"Graph Visual {equationTrees.Count}");
        graphVisual.transform.SetParent(graphVisualsObj.transform, false);

        // pass in the gameobject to add renderers to
        if(equationType == TYPE_LINE) graphRenderer = new LineGraphRenderer(graphVisual.transform, GraphManager.instance.defaultLineColor);
        else if(equationType == TYPE_SURFACE) graphRenderer = new SurfaceGraphRenderer(graphVisual.transform, GraphManager.instance.defaultMeshColor);
        else                          throw new GraphEvaluationException("Unsupported equation type attempting to be graphed.");

        axisRenderer.InitializeAxes();
        RefreshAllUIText();
        ScaleGraph();
        VisualizeGraph(equationTree);
    }

    public void RefreshAllGraphs() {
        foreach(Transform child in graphVisualsObj.transform) {
            Destroy(child.gameObject);
        }

        List<ParseTreeNode> equationTreesCopy = new List<ParseTreeNode>(equationTrees);
        equationTrees.Clear();

        foreach(ParseTreeNode equation in equationTreesCopy.ToArray()) {
            AddEquation(equation);
        }
    }

    // scale the graph based on the largest range
    private void ScaleGraph()
    {
        // scale the graph object so that the largest dimension fits in view
        float scaleFactor = GraphUtils.CalculateScaleFactor(graphSettings);
        graphVisualsObj.transform.localScale = Vector3.one * scaleFactor;

        // scale the axes
        axisRenderer.UpdateAxes(graphSettings);
    }

    // renders the graph using the respective renderer and settings
    private void VisualizeGraph(ParseTreeNode equationTree) {
        graphRenderer.RenderGraph(equationTree, graphSettings, inputVars, outputVar);
    }

    // rotate the graph object (called from the gizmo)
    public void GizmoRotateGraph(Vector3 rotation) {
        graphObj.transform.localRotation = Quaternion.Euler(rotation);
        RefreshUIText(xRotationUI, rotation.y);
        RefreshUIText(yRotationUI, rotation.z);
        RefreshUIText(zRotationUI, rotation.x);
    }

    // updates the graph settings and re-renders the graph
    public void UpdateGraphSettings() {
        // eventually throw an exception when invalid values are input
        // just so the user can know
        if (float.TryParse(xAxisMinUI.text, out float val)) graphSettings.xMin = val; else RefreshUIText(xAxisMinUI, graphSettings.xMin);
        if (float.TryParse(xAxisMaxUI.text, out val)) graphSettings.xMax = val; else RefreshUIText(xAxisMaxUI, graphSettings.xMax);
        if (float.TryParse(yAxisMinUI.text, out val)) graphSettings.yMin = val; else RefreshUIText(yAxisMinUI, graphSettings.yMin);
        if (float.TryParse(yAxisMaxUI.text, out val)) graphSettings.yMax = val; else RefreshUIText(yAxisMaxUI, graphSettings.yMax);
        if (float.TryParse(zAxisMinUI.text, out val)) graphSettings.zMin = val; else RefreshUIText(zAxisMinUI, graphSettings.zMin);
        if (float.TryParse(zAxisMaxUI.text, out val)) graphSettings.zMax = val; else RefreshUIText(zAxisMaxUI, graphSettings.zMax);

        // values are not the same as unity's values
        float xRot = graphObj.transform.localRotation.y,
              yRot = graphObj.transform.localRotation.z, 
              zRot = graphObj.transform.localRotation.x;

        // negative ones are to make all three rotate clockwise
        if (float.TryParse(xRotationUI.text, out val)) zRot = -val; else RefreshUIText(xRotationUI, zRot);
        if (float.TryParse(yRotationUI.text, out val)) xRot = val; else RefreshUIText(yRotationUI, xRot);
        if (float.TryParse(zRotationUI.text, out val)) yRot = -val; else RefreshUIText(zRotationUI, yRot);

        graphObj.transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);

        if (float.TryParse(stepUI.text, out val) && val > 0.0001f) graphSettings.step = val; else RefreshUIText(stepUI, graphSettings.step);

        ScaleGraph();

        foreach(ParseTreeNode equation in equationTrees)
            VisualizeGraph(equation);
    }

    // updates the entire UI to have the current graph settings displayed
    private void RefreshAllUIText() {
        RefreshUIText(xAxisMinUI, graphSettings.xMin);
        RefreshUIText(xAxisMaxUI, graphSettings.xMax);
        RefreshUIText(yAxisMinUI, graphSettings.yMin);
        RefreshUIText(yAxisMaxUI, graphSettings.yMax);
        RefreshUIText(zAxisMinUI, graphSettings.zMin);
        RefreshUIText(zAxisMaxUI, graphSettings.zMax);
        RefreshUIText(xRotationUI, graphObj.transform.localRotation.y);
        RefreshUIText(yRotationUI, graphObj.transform.localRotation.z);
        RefreshUIText(zRotationUI, graphObj.transform.localRotation.x);
        RefreshUIText(stepUI, graphSettings.step);
    }

    // updates a specific UI element's text to a value
    private void RefreshUIText(TMP_InputField textField, float value) {
        textField.text = value.ToString("G");
    }

    // for now it just sets rotation to 0, 0, 0
    public void ResetToDefault() {
        // reset to original settings
        graphSettings = originalSettings;

        // reset rotation
        graphObj.transform.localRotation = Quaternion.Euler(0, 0, 0);

        // reset all UI
        RefreshAllUIText();
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

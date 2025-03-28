using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;

public class EquationGrapher : MonoBehaviour
{
    // the root node of the equation tree
    private ParseTreeNode equationTree;
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
    public AxisRenderer axisRenderer;
    public GameObject graphVisualsObj;
    public GameObject graphObj;

    // all graph settings UI elements
    public TMP_InputField xAxisMin;
    public TMP_InputField xAxisMax;
    public TMP_InputField yAxisMin;
    public TMP_InputField yAxisMax;
    public TMP_InputField zAxisMin;
    public TMP_InputField zAxisMax;
    public TMP_InputField xRotation;
    public TMP_InputField yRotation;
    public TMP_InputField zRotation;
    public TMP_InputField step;

    // equation renderer types
    // change to enums later
    private const int TYPE_LINE = 0;
    private const int TYPE_MESH = 1;

    // does nothing w/ def lin color for now
    public void InitializeGraph(ParseTreeNode equationTree, Material defaultLineColor, GraphSettings settings) 
    {
        // determine what equation type is by parsing the whole tree
        var (equationType, inputVars, outputVar) = DetermineEquationType(equationTree);

        this.equationTree = equationTree;
        this.graphSettings = settings;
        this.originalSettings = settings;
        this.inputVars = inputVars;
        this.outputVar = outputVar;

        // pass in the gameobject (1st child, the "graph" object) to add renderers to
        if(equationType == TYPE_LINE) graphRenderer = new LineGraphRenderer(graphVisualsObj.transform);
        else                          throw new GraphEvaluationException("Unsupported equation type attempting to be graphed.");

        axisRenderer.InitializeAxes();
        RefreshAllUIText();
        RefreshGraph();
    }

    private void RefreshGraph() {
        ScaleGraph();
        VisualizeGraph();
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
    private void VisualizeGraph() {
        graphRenderer.RenderGraph(equationTree, graphSettings, inputVars, outputVar);
    }

    // updates the graph settings and re-renders the graph
    public void UpdateGraphSettings() {
        // eventually throw an exception when invalid values are input
        // just so the user can know
        if (float.TryParse(xAxisMin.text, out float val)) graphSettings.xMin = val; else RefreshUIText(xAxisMin, graphSettings.xMin);
        if (float.TryParse(xAxisMax.text, out val)) graphSettings.xMax = val; else RefreshUIText(xAxisMax, graphSettings.xMax);
        if (float.TryParse(yAxisMin.text, out val)) graphSettings.yMin = val; else RefreshUIText(yAxisMin, graphSettings.yMin);
        if (float.TryParse(yAxisMax.text, out val)) graphSettings.yMax = val; else RefreshUIText(yAxisMax, graphSettings.yMax);
        if (float.TryParse(zAxisMin.text, out val)) graphSettings.zMin = val; else RefreshUIText(zAxisMin, graphSettings.zMin);
        if (float.TryParse(zAxisMax.text, out val)) graphSettings.zMax = val; else RefreshUIText(zAxisMax, graphSettings.zMax);

        float xRot = graphObj.transform.localRotation.x,
              yRot = graphObj.transform.localRotation.y, 
              zRot = graphObj.transform.localRotation.z;

        if (float.TryParse(xRotation.text, out val)) xRot = val; else RefreshUIText(xRotation, xRot);
        if (float.TryParse(yRotation.text, out val)) yRot = val; else RefreshUIText(yRotation, yRot);
        if (float.TryParse(zRotation.text, out val)) zRot = val; else RefreshUIText(zRotation, zRot);

        graphObj.transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);

        if (float.TryParse(step.text, out val) && val > 0.0001f) graphSettings.step = val; else RefreshUIText(step, graphSettings.step);

        RefreshGraph();
    }

    // updates the entire UI to have the current graph settings displayed
    private void RefreshAllUIText() {
        RefreshUIText(xAxisMin, graphSettings.xMin);
        RefreshUIText(xAxisMax, graphSettings.xMax);
        RefreshUIText(yAxisMin, graphSettings.yMin);
        RefreshUIText(yAxisMax, graphSettings.yMax);
        RefreshUIText(zAxisMin, graphSettings.zMin);
        RefreshUIText(zAxisMax, graphSettings.zMax);
        RefreshUIText(xRotation, graphObj.transform.localRotation.x);
        RefreshUIText(yRotation, graphObj.transform.localRotation.y);
        RefreshUIText(zRotation, graphObj.transform.localRotation.z);
        RefreshUIText(step, graphSettings.step);
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
        else if (inputVars.Count == 2)                    equationType = TYPE_MESH;
        else                                              throw new GraphEvaluationException("Unsupported variable amount in right hand side of equation.");

        return (equationType, inputVars, outputVar);
    }

    // finds all the variables in the given tree and adds them to a set
    // potentially could create the set during tree creation if this
    // full tree traversal ends up being too costly
    private void DetermineVariables(ParseTreeNode equationTree, HashSet<GraphVariable> variables) {
        if (equationTree == null) return;

        if (equationTree.token.type == EquationParser.TYPE_VARIABLE) {
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

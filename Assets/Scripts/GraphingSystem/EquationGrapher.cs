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
    // set of input variables (independent variables)
    private HashSet<GraphVariable> inputVars;
    // single output var computed by the function (dependent variable)
    private GraphVariable outputVar;
    // store references to scripts and objects
    public AxisRenderer axisRenderer;
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
        this.inputVars = inputVars;
        this.outputVar = outputVar;

        // pass in the gameobject (1st child, the "graph" object) to add renderers to
        if(equationType == TYPE_LINE) graphRenderer = new LineGraphRenderer(graphObj.transform);
        else                          throw new GraphEvaluationException("Unsupported equation type attempting to be graphed.");

        axisRenderer.InitializeAxes();
        axisRenderer.UpdateAxes(this.graphSettings);

        ScaleGraph();
        VisualizeGraph();
    }

    // scale the graph based on the largest range
    private void ScaleGraph()
    {
        // scale the graph object so that the largest dimension fits in view
        float baseRange = 2f;
        float maxRange = Mathf.Max(graphSettings.xMax - graphSettings.xMin, graphSettings.yMax - graphSettings.yMin, graphSettings.zMax - graphSettings.zMin);
        float scaleFactor = baseRange / maxRange;
        graphObj.transform.localScale = Vector3.one * scaleFactor;

        float zOffset = -graphSettings.zMin * scaleFactor;

        // calculate the z offset so that graph spawns above ground
        graphObj.transform.localPosition = new Vector3(0, zOffset, 0);
    }

    // renders the graph using the respective renderer and settings
    private void VisualizeGraph() {
        graphRenderer.RenderGraph(equationTree, graphSettings, inputVars, outputVar);
    }

    // updates the graph settings and re-renders the graph
    public void UpdateGraphSettings() {
        float val;

        if (float.TryParse(xAxisMin.text, out val)) graphSettings.xMin = val;
        if (float.TryParse(xAxisMax.text, out val)) graphSettings.xMax = val;
        if (float.TryParse(yAxisMin.text, out val)) graphSettings.yMin = val;
        if (float.TryParse(yAxisMax.text, out val)) graphSettings.yMax = val;
        if (float.TryParse(zAxisMin.text, out val)) graphSettings.zMin = val;
        if (float.TryParse(zAxisMax.text, out val)) graphSettings.zMax = val;

        if (float.TryParse(step.text, out val) && val > 0.0001f) graphSettings.step = val;

        ScaleGraph();
        VisualizeGraph();
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

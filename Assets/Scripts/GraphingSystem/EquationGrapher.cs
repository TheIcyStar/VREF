using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;

public class EquationGrapher : MonoBehaviour
{
    // the root node of the equation tree
    public ParseTreeNode equationTree;
    // renderer interface set to the correct renderer type
    private IGraphRenderer graphRenderer;
    // graph settings passed in
    private GraphSettings graphSettings;
    // set of input variables (independent variables)
    private HashSet<GraphVariable> inputVars;
    // single output var computed by the function (dependent variable)
    private GraphVariable outputVar;

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

    public void InitializeGraph(ParseTreeNode equationTree, IGraphRenderer renderer, GraphSettings settings, HashSet<GraphVariable> inputVars, GraphVariable outputVar) {
        this.equationTree = equationTree;
        this.graphRenderer = renderer;
        this.graphSettings = settings;
        this.inputVars = inputVars;
        this.outputVar = outputVar;
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
        this.transform.localScale = Vector3.one * scaleFactor;

        // calculate the z offset so that graph spawns above ground
        this.transform.position += new Vector3(0, -graphSettings.zMin * scaleFactor, 0);
    }

    // renders the graph using the respective renderer and settings
    private void VisualizeGraph() {
        graphRenderer.RenderGraph(equationTree, graphSettings, inputVars, outputVar);
    }

    // updates the graph settings and re-renders the graph
    public void UpdateGraphSettings() {
        //this.graphSettings = newSettings;
        VisualizeGraph();
    }
}

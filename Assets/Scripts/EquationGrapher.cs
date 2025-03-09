using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using UnityEngine;

public class EquationGrapher : MonoBehaviour
{
    // the root node of the equation tree
    public ParseTreeNode equationTree;
    // renderer interface set to the correct renderer type
    private IGraphRenderer graphRenderer;
    // graph settings passed in
    private GraphSettings graphSettings;

    public void InitializeGraph(ParseTreeNode equationTree, IGraphRenderer renderer, GraphSettings settings) {
        this.equationTree = equationTree;
        this.graphRenderer = renderer;
        this.graphSettings = settings;
        VisualizeGraph();
    }

    // renders the graph using the respective renderer and settings
    private void VisualizeGraph() {
        graphRenderer.RenderGraph(equationTree, graphSettings);
    }

    // updates the graph settings and re-renders the graph
    public void UpdateGraphSettings(GraphSettings newSettings) {
        this.graphSettings = newSettings;
        VisualizeGraph();
    }
}

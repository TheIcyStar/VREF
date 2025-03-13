using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class GraphManager : MonoBehaviour
{
    // global graph settings from options menu
    // storing default values for now
    public GraphSettings globalGraphSettings = new GraphSettings(-10, 10, -10, 10, 1);

    // equation renderer types
    private const int TYPE_LINE = 0;
    private const int TYPE_MESH = 1;

    // list of graphs currently in the scene, as well as their EquationGrapher to avoid using GetComponent
    private List<(GameObject, EquationGrapher)> graphs;

    public void Start() {
        graphs = new List<(GameObject, EquationGrapher)>();
    }

    public void CreateNewGraph(ParseTreeNode equationTree) {
        int equationType = DetermineEquationType(equationTree);
        var (graphObj, grapher) = CreateNewGraphObject();

        // attach the correct renderer
        // CHECK TYPES AND ADD CORRECT RENDERER IN THE FUTURE
        IGraphRenderer renderer = new LineGraphRenderer(graphObj.AddComponent<LineRenderer>());
        /*else if (equationType == TYPE_MESH) {
            renderer = new MeshGraphRenderer(graphObj.AddComponent<MeshFilter>(), graphObj.AddComponent<MeshRenderer>());
        }*/

        grapher.InitializeGraph(equationTree, renderer, globalGraphSettings);

        graphs.Add((graphObj, grapher));
    }

    // creates the graph object and attaches the script
    // eventually instead of adding an empty gameobject
    // we should have graph prefabs for each kind of graph
    private (GameObject, EquationGrapher) CreateNewGraphObject() {
        // create new object
        GameObject graphObj = new GameObject($"Graph {graphs.Count + 1}");
        
        // make it a child of the graph manager
        graphObj.transform.parent = this.transform;

        // add the script
        EquationGrapher grapher = graphObj.AddComponent<EquationGrapher>();

        // return both as grapher needs accessed
        return (graphObj, grapher);
    }

    // determines the type of equation by analyzing the parse tree
    private int DetermineEquationType(ParseTreeNode equationTree) {
        HashSet<string> variables = new HashSet<string>();
        DetermineVariables(equationTree, variables);

        // this logic needs to change to support parametrics, planes, etc... in the future
        // right now only [var] = [expression] will work
        if (variables.Count == 1) return TYPE_LINE;
        if (variables.Count == 2) return TYPE_LINE;
        if (variables.Count == 3) return TYPE_MESH;

        // default
        return TYPE_LINE;
    }

    // finds all the variables in the equation and adds them to a set
    // potentially could create the set during tree creation if this
    // full tree traversal ends up being too costly
    private void DetermineVariables(ParseTreeNode equationTree, HashSet<string> variables) {
        if (equationTree == null) return;

        if (equationTree.token.type == EquationParser.TYPE_VARIABLE) {
            variables.Add(equationTree.token.text);
        }

        DetermineVariables(equationTree.left, variables);
        DetermineVariables(equationTree.right, variables);
    }
}

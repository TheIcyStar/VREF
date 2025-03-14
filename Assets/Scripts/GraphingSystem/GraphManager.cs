using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class GraphManager : MonoBehaviour
{
    // graph prefab
    public GameObject graphPrefab;

    // dictionary mapping a graph gameobject to all its scripts
    // to avoid having to find them using GetComponent
    private Dictionary<GameObject, (EquationGrapher grapher, IGraphRenderer renderer)> graphs;

    // global graph settings from options menu
    // storing default values for now
    public GraphSettings globalGraphSettings = new GraphSettings(-10, 10, -10, 10, 1);

    // keep track of graph count solely for naming
    private int graphCount;

    // equation renderer types
    private const int TYPE_LINE = 0;
    private const int TYPE_MESH = 1;

    public void Start() {
        graphs = new();
        graphCount = 1;
    }

    // creates the graph object and attaches the script
    public void CreateNewGraph(ParseTreeNode equationTree) {
        // determine what equation type is by parsing the whole tree
        int equationType = DetermineEquationType(equationTree);

        // creates an instance of the prefab
        GameObject graphPrefabObj = Instantiate(graphPrefab, this.transform);

        // name the instance
        graphPrefabObj.name = $"Graph {graphCount}";
        graphCount++;

        // get the children objects
        GameObject graphObj = graphPrefabObj.transform.GetChild(0).gameObject; // graph
        GameObject axesObj = graphPrefabObj.transform.GetChild(1).gameObject;  // axes
        GameObject gridObj = graphPrefabObj.transform.GetChild(2).gameObject;  // gridlines

        // attach the equation grapher class (manages the state of each individual graph,
        // maybe the name of that script should be changed for clarity)
        EquationGrapher grapher = graphObj.AddComponent<EquationGrapher>();
        // axes script here
        // gridlines script here (maybe combine both?)

        // attach the correct renderer
        // CHECK TYPES AND ADD CORRECT RENDERER IN THE FUTURE
        IGraphRenderer renderer = new LineGraphRenderer(graphObj.AddComponent<LineRenderer>());
        /*else if (equationType == TYPE_MESH) {
            renderer = new MeshGraphRenderer(graphObj.AddComponent<MeshFilter>(), graphObj.AddComponent<MeshRenderer>());
        }*/

        // add the graph object and its corresponding scripts to the dictionary
        graphs[graphPrefabObj] = (grapher, renderer);

        grapher.InitializeGraph(equationTree, renderer, globalGraphSettings);
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

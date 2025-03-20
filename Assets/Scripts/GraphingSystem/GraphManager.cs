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
    public GraphSettings globalGraphSettings = new GraphSettings(-10, 10, -10, 10, -10, 10, .1f);

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
        var (equationType, inputVars, outputVar) = DetermineEquationType(equationTree);

        // create and name an instance of the prefab
        GameObject graphPrefabObj = Instantiate(graphPrefab, this.transform);
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

        // initialize renderer
        IGraphRenderer renderer;

        // attach the correct renderer
        if(equationType == TYPE_LINE) renderer = new LineGraphRenderer(graphObj.AddComponent<LineRenderer>());
        else                          renderer = new LineGraphRenderer(graphObj.AddComponent<LineRenderer>());  // default

        // add the graph object and its corresponding scripts to the dictionary
        graphs[graphPrefabObj] = (grapher, renderer);

        grapher.InitializeGraph(equationTree, renderer, globalGraphSettings, inputVars, outputVar);
    }

    // determines the type of equation by analyzing the parse tree
    // also returns the input and output variables
    // ONLY WORKS WITH [output var] = [f(input var)] FOR NOW
    private (int, HashSet<GraphVariable>, GraphVariable) DetermineEquationType(ParseTreeNode equationTree) {
        // initialize a set to store all the input vars
        HashSet<GraphVariable> inputVars = new HashSet<GraphVariable>();
        GraphVariable outputVar;

        // default to y = f(x)
        if(equationTree == null || equationTree.token.text != "=") {
            inputVars.Add(GraphVariable.X);
            outputVar = GraphVariable.Y;
            return (TYPE_LINE, inputVars, outputVar);
        }

        // find output var (left of equal sign)
        outputVar = ConvertToGraphVariable(equationTree.left.token.text);

        // intialize equation type
        int equationType;

        // add all input variables to the set (right of equal sign)
        DetermineVariables(equationTree.right, inputVars);

        // this logic needs to change to support parametrics, planes, etc... in the future
        if (inputVars.Count == 0 || inputVars.Count == 1) equationType = TYPE_LINE;
        else if (inputVars.Count == 2)                    equationType = TYPE_MESH;
        else                                              equationType = TYPE_LINE;

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
            _ => GraphVariable.Constant
        };
    }
}

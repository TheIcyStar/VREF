using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class GraphManager : MonoBehaviour
{
    // graph prefab
    public GameObject graphPrefab;

    // transform of the equation UI
    public Transform equationUITransform;
    // distance from the UI that all graphs will spawn in from
    [SerializeField] private float graphDisplacement = 4f;
    // distance from the ground the graph spawns above
    [SerializeField] private float lowerGraphMargin = .5f;
    // dictionary mapping a graph gameobject to all its scripts
    // to avoid having to find them using GetComponent
    private Dictionary<GameObject, (EquationGrapher grapher, AxisRenderer axisRenderer)> graphs;

    // global graph settings from options menu
    // storing default values for now
    public GraphSettings globalGraphSettings = new GraphSettings(-10, 20, -5, 10, -30, 0, .1f);
    // default material of graph lines
    public Material defaultLineColor;

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

        // create an instance of the prefab, place it past the UI, and name it
        GameObject graphPrefabObj = Instantiate(graphPrefab, this.transform);
        Vector3 forward = equationUITransform.forward;
        forward.y = 0;
        forward.Normalize();
        graphPrefabObj.transform.position = new Vector3(equationUITransform.position.x, lowerGraphMargin, equationUITransform.position.z) + forward * graphDisplacement;
        graphPrefabObj.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        graphPrefabObj.name = $"Graph {graphCount}";
        graphCount++;

        // get the children objects
        GameObject graphObj = graphPrefabObj.transform.GetChild(0).gameObject; // graph
        GameObject axesObj = graphPrefabObj.transform.GetChild(1).gameObject;  // axes
        GameObject gridObj = graphPrefabObj.transform.GetChild(2).gameObject;  // gridlines

        // access the equation grapher class (manages the state of each individual graph,
        // maybe the name of that script should be changed for clarity)
        // as well as the axis renderer and gridline renderer
        EquationGrapher grapher = graphObj.GetComponent<EquationGrapher>();
        AxisRenderer axisRenderer = axesObj.GetComponent<AxisRenderer>();
        // gridlines script here (maybe combine both?)

        // initialize renderer
        IGraphRenderer renderer;

        // pass in the gameobject to add renderers to
        if(equationType == TYPE_LINE) renderer = new LineGraphRenderer(graphObj.transform);
        else                          throw new GraphEvaluationException("Unsupported equation type attempting to be graphed.");

        // add the graph object and its corresponding scripts to the dictionary
        graphs[graphPrefabObj] = (grapher, axisRenderer);

        axisRenderer.InitializeAxes();
        axisRenderer.UpdateAxes(globalGraphSettings, lowerGraphMargin);
        grapher.InitializeGraph(equationTree, renderer, globalGraphSettings, inputVars, outputVar);
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

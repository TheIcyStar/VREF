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
    // distance from the ground that the origin of the graph spawns above
    [SerializeField] private float graphHeight = 1.1f;
    // dictionary that maps prefabs to their manager script
    private Dictionary<GameObject, EquationGrapher> graphs;

    // global graph settings from options menu
    // storing default values for now
    public GraphSettings globalGraphSettings = new GraphSettings(-10, 10, -10, 10, -10, 10, .1f);
    // default material of graph lines
    public Material defaultLineColor;

    // keep track of graph count solely for naming (for now)
    private int graphCount;

    public void Start() {
        graphs = new();
        graphCount = 1;
    }

    // creates the graph object
    public void CreateNewGraph(ParseTreeNode equationTree) {
        // create an instance of the prefab, place it past the UI, and name it
        GameObject graphPrefabObj = Instantiate(graphPrefab, this.transform);
        Vector3 forward = equationUITransform.forward;
        forward.y = 0;
        forward.Normalize();
        graphPrefabObj.transform.position = new Vector3(equationUITransform.position.x, graphHeight, equationUITransform.position.z) + forward * graphDisplacement;
        graphPrefabObj.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        graphPrefabObj.name = $"Graph {graphCount}";
        graphCount++;

        // get the script once
        EquationGrapher grapher = graphPrefabObj.GetComponent<EquationGrapher>();

        // initialize the graph
        grapher.InitializeGraph(equationTree, defaultLineColor, globalGraphSettings);

        // add the graph object to the dictionary
        graphs.Add(graphPrefabObj,  grapher);
    }
}

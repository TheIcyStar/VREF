using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class GraphManager : MonoBehaviour
{
    // graph prefab
    [SerializeField] private GameObject graphPrefab;
    // graph list element
    [SerializeField] private GameObject graphListElementPrefab;

    // transform of the equation UI
    [SerializeField] private Transform equationUITransform;
    // place where graph ui elements will be added
    [SerializeField] private Transform graphListContentTransform;

    // distance from the UI that all graphs will spawn in from
    [SerializeField] private float graphDisplacement = 4f;
    // distance from the ground that the origin of the graph spawns above
    [SerializeField] private float graphHeight = 1.1f;

    // list that stores the graph instances
    private List<GraphInstance> graphs;
    // monitor what graph is selected
    private GraphInstance selectedGraph = null;
    private GraphListElementManager selectedUI;

    // global graph settings from options menu
    // storing default values for now
    public GraphSettings globalGraphSettings = new GraphSettings(-10, 10, -10, 10, -10, 10, .1f);
    // default material of graph lines
    public Material defaultLineColor;
    // default material of meshes
    public Material defaultMeshColor;

    public static GraphManager instance;

    // keep track of graph count solely for naming (for now)
    private int graphCount;

    public void Awake() { //Ensures there's only one ServerConnection object
        if(graphPrefab == null){
            Destroy(this);
        } else {
            instance = this;
        }
    }

    public void Start() {
        graphs = new();
        graphCount = 1;
    }

     public ParseTreeNode[] GetGraphs() {
        ParseTreeNode[] equations = new ParseTreeNode[graphs.Count];
        int i = 0;

         // i changed this line to work with the list (marking this in case it doesnt work anymore)
        foreach(GraphInstance entry in graphs) {
            // i changed this line to work with the list (marking this in case it doesnt work anymore)
            equations[i] = entry.equationTrees.ElementAt(i);
            i++;
        }

        return equations;
    }

    public void BulkOverwriteGraphs(ParseTreeNode[] equations){
        //clean up objects
        // i changed this line to work with the list (marking this in case it doesnt work anymore)
        foreach(GraphInstance entry in graphs) {
            Destroy(entry.gameObject);
        }
        graphs = new();

        //Recreate them //todo?: Might need to start tracking individual graphs instead of just parse trees
        int debugCounter = 0;
        foreach(ParseTreeNode tree in equations){
            try {
                // i changed this line to work with the list (marking this in case it doesnt work anymore)
                AddGraph(tree);
            } catch (Exception e){
                Debug.Log($"Error while parsing fetched equation {debugCounter}: {e.Message}");
            }
            debugCounter++;
        }
    }

    // either creates the graph object or adds to the selected object
    public void AddGraph(ParseTreeNode newEquation) {
        // if a graph is currently selected, add a new equation to it
        if (selectedGraph != null) { selectedGraph.AddEquation(newEquation); return; }

        // create an instance of the prefab, place it past the UI, and name it
        GameObject graphPrefabObj = Instantiate(graphPrefab, this.transform);
        Vector3 forward = equationUITransform.forward;
        forward.y = 0;
        forward.Normalize();
        graphPrefabObj.transform.position = new Vector3(equationUITransform.position.x, graphHeight, equationUITransform.position.z) + forward * graphDisplacement;
        graphPrefabObj.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        graphPrefabObj.name = $"Graph {graphCount}";

        // get the script once
        GraphInstance graphInstance = graphPrefabObj.GetComponent<GraphInstance>();

        // initialize the graph
        graphInstance.AddEquation(newEquation);

        // instantiate the list element prefab
        GameObject graphListItem = Instantiate(graphListElementPrefab, graphListContentTransform);
        graphListItem.name = $"Graph {graphCount} List Element";

        // get the manager script
        GraphListElementManager graphUIMan = graphListItem.GetComponent<GraphListElementManager>();
        graphUIMan.Intialize(graphInstance, graphCount);

        graphs.Add(graphInstance);
        selectedGraph = graphInstance;
        selectedUI = graphUIMan;
        graphCount++;
    }

    public void SetSelectedGraph(GraphInstance graph, GraphListElementManager ui)
    {
        // same graph got selected, deselect old ui set pointers to null
        if (selectedGraph == graph) {
            if (selectedUI != null) selectedUI.DeselectGraph();
            selectedGraph = null; 
            selectedUI = null; 
        }
        // new graph is selected, deselect old ui, update pointers, and select the new one
        else {
            if (selectedUI != null) selectedUI.DeselectGraph();
            selectedGraph = graph; 
            selectedUI = ui; 
            selectedUI.SelectGraph(); 
        }
    }
}

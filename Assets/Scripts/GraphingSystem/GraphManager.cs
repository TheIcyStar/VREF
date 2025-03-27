using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using System;

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

    public static GraphManager instance;

    // keep track of graph count solely for naming (for now)
    private int graphCount;

    public void Awake() { //Ensures there's only one ServerConnection object
        if(graphPrefab == null){
            Destroy(this);
        } else {
            instance = this;
        }
        // if(instance != null & instance != this){
        //     Destroy(this);
        // } else {
        //     instance = this;
        // }
    }

    public void Start() {
        graphs = new();
        graphCount = 1;
    }

     public ParseTreeNode[] GetGraphs() {
        ParseTreeNode[] equations = new ParseTreeNode[graphs.Count];
        int i = 0;

        foreach(KeyValuePair<GameObject, EquationGrapher> entry in graphs) {
            equations[i] = entry.Value.equationTree;
            i++;
        }

        return equations;
    }

    public void BulkOverwriteGraphs(ParseTreeNode[] equations){
        Debug.Log("E");

        //clean up objects
        foreach(KeyValuePair<GameObject, EquationGrapher> entry in graphs) {
            Destroy(entry.Key);
        }
        graphs = new();

        Debug.Log("Cleared graphs");

        //Recreate them //todo?: Might need to start tracking individual graphs instead of just parse trees
        int debugCounter = 0;
        foreach(ParseTreeNode tree in equations){
            try {
                CreateNewGraph(tree);
            } catch (Exception e){
                Debug.Log($"Error while parsing fetched equation {debugCounter}: {e.Message}");
            }
            debugCounter++;
        }
        Debug.Log($"Updated {debugCounter} graphs");
    }

    // creates the graph object
    public void CreateNewGraph(ParseTreeNode equationTree) {
        // create an instance of the prefab, place it past the UI, and name it
        Debug.Log("A");
        if(this.graphPrefab == null){
            Debug.Log("It's null");
        } else {
            Debug.Log("It's NOT null");
        }
        GameObject graphPrefabObj = Instantiate(this.graphPrefab, this.transform);
        Debug.Log("B");
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
        Debug.Log("C");

        // add the graph object to the dictionary
        graphs.Add(graphPrefabObj,  grapher);
        Debug.Log("D");
    }
}

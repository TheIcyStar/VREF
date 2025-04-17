using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor.Graphs;
using TMPro;
using UnityEngine.UI;
using PlasticGui.WorkspaceWindow.IssueTrackers;

public class GraphManager : MonoBehaviour
{
    // graph prefab
    [SerializeField] private GameObject graphPrefab;
    // graph list element
    [SerializeField] private GameObject graphListElementPrefab;

    // transform of the equation UI
    [SerializeField] private Transform equationUITransform;
    // place where graph ui elements will be added
    [SerializeField] private RectTransform graphListContentTransform;

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

    // keyboard text to grab equation
    [SerializeField] private TMP_Text equationTextArea;
    [SerializeField] private GameObject equationListElementPrefab;

    // all graph settings UI elements
    [SerializeField] private TMP_InputField xAxisMinUI;
    [SerializeField] private TMP_InputField xAxisMaxUI;
    [SerializeField] private TMP_InputField yAxisMinUI;
    [SerializeField] private TMP_InputField yAxisMaxUI;
    [SerializeField] private TMP_InputField zAxisMinUI;
    [SerializeField] private TMP_InputField zAxisMaxUI;
    [SerializeField] private TMP_InputField xRotationUI;
    [SerializeField] private TMP_InputField yRotationUI;
    [SerializeField] private TMP_InputField zRotationUI;
    [SerializeField] private TMP_InputField stepUI;

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
        // ParseTreeNode[] equations = new ParseTreeNode[graphs.Count];
        // int i = 0;

        //  // i changed this line to work with the list (marking this in case it doesnt work anymore)
        // foreach(GraphInstance entry in graphs) {
        //     // i changed this line to work with the list (marking this in case it doesnt work anymore)
        //     equations[i] = entry.equationTrees.ElementAt(i);
        //     i++;
        // }

        // return equations;

        List<ParseTreeNode> allEquations = new();

        foreach (GraphInstance entry in graphs) {
            allEquations.AddRange(entry.GetEquations());
        }

        return allEquations.ToArray();
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
        GameObject graphPrefabObj = null;
        GameObject graphListItem = null;

        try {
            // if a graph is currently selected, add a new equation to it
            if (selectedGraph != null) {
                selectedGraph.AddEquation(newEquation, globalGraphSettings);
                AddEquationListElement(newEquation);
                return; 
            }

            // create an instance of the prefab, place it past the UI, and name it
            graphPrefabObj = Instantiate(graphPrefab, this.transform);
            PlaceGraphInFront(graphPrefabObj);
            graphPrefabObj.name = $"Graph {graphCount}";

            // get the script once
            GraphInstance graphInstance = graphPrefabObj.GetComponent<GraphInstance>();
            selectedGraph = graphInstance;

            // initialize the graph
            graphInstance.AddEquation(newEquation, globalGraphSettings);

            // instantiate the list element prefab
            graphListItem = Instantiate(graphListElementPrefab, graphListContentTransform);
            graphListItem.name = $"Graph {graphCount} List Element";

            // get the manager script
            GraphListElementManager graphUIMan = graphListItem.GetComponent<GraphListElementManager>();
            graphUIMan.Intialize(graphInstance, graphCount);

            graphs.Add(graphInstance);
            selectedUI = graphUIMan;
            graphCount++;

            AddEquationListElement(newEquation);
        }
        catch (GraphEvaluationException ge) {
            if(graphPrefabObj != null) Destroy(graphPrefabObj);
            if(graphListItem != null) Destroy(graphListItem);

            throw new GraphEvaluationException(ge.Message);
        }
    }

    private void AddEquationListElement(ParseTreeNode equation) {
        GameObject equationListItem = Instantiate(equationListElementPrefab, selectedUI.equationListObj.transform);
        EquationListElementManager elem = equationListItem.GetComponent<EquationListElementManager>();
        elem.equationText.text = equationTextArea.text;
        elem.Initialize(equation, selectedGraph);
        RefreshGraphListUI();
    }

    public void DeleteEquationUIElement(ParseTreeNode equation, GraphInstance instance, EquationListElementManager elem) {
        foreach (GraphInstance graphInstance in graphs) {
            if (instance == graphInstance) {
                instance.RemoveEquation(equation);
                Destroy(elem.gameObject);
                RefreshGraphListUI();
            }
        }
    }

    public void RefreshGraphListUI() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(graphListContentTransform);
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

            globalGraphSettings = new GraphSettings(
                graph.graphSettings.xMin,
                graph.graphSettings.xMax,
                graph.graphSettings.yMin,
                graph.graphSettings.yMax,
                graph.graphSettings.zMin,
                graph.graphSettings.zMax,
                graph.graphSettings.step
            );

            RefreshAllUIText();
        }
    }

    public void DeleteGraphObject(GraphInstance instance, GraphListElementManager ui) {
        foreach (GraphInstance graphInstance in graphs) {
            if (instance == graphInstance) {
                if(instance == selectedGraph) {
                    selectedGraph = null;
                    selectedUI = null;
                }
                Destroy(graphInstance.gameObject);
                Destroy(ui.gameObject);
                return;
            }
        }
        // throw an exception here
    }

    // places graph in front of the equation UI
    public void PlaceGraphInFront(GameObject gameObj) {
        Vector3 forward = equationUITransform.forward;
        forward.y = 0;
        forward.Normalize();
        gameObj.transform.position = new Vector3(equationUITransform.position.x, graphHeight, equationUITransform.position.z) + forward * graphDisplacement;
        gameObj.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    // updates the graph settings and re-renders the graph
    public void UpdateGraphSettings() {
        // eventually throw an exception when invalid values are input
        // just so the user can know
        if (float.TryParse(xAxisMinUI.text, out float val)) selectedGraph.graphSettings.xMin = val; else RefreshUIText(xAxisMinUI, selectedGraph.graphSettings.xMin);
        if (float.TryParse(xAxisMaxUI.text, out val)) selectedGraph.graphSettings.xMax = val; else RefreshUIText(xAxisMaxUI, selectedGraph.graphSettings.xMax);
        if (float.TryParse(yAxisMinUI.text, out val)) selectedGraph.graphSettings.yMin = val; else RefreshUIText(yAxisMinUI, selectedGraph.graphSettings.yMin);
        if (float.TryParse(yAxisMaxUI.text, out val)) selectedGraph.graphSettings.yMax = val; else RefreshUIText(yAxisMaxUI, selectedGraph.graphSettings.yMax);
        if (float.TryParse(zAxisMinUI.text, out val)) selectedGraph.graphSettings.zMin = val; else RefreshUIText(zAxisMinUI, selectedGraph.graphSettings.zMin);
        if (float.TryParse(zAxisMaxUI.text, out val)) selectedGraph.graphSettings.zMax = val; else RefreshUIText(zAxisMaxUI, selectedGraph.graphSettings.zMax);

        // values are not the same as unity's values
        float xRot = selectedGraph.gameObject.transform.GetChild(0).transform.localRotation.y,
              yRot = selectedGraph.gameObject.transform.GetChild(0).transform.localRotation.z, 
              zRot = selectedGraph.gameObject.transform.GetChild(0).transform.localRotation.x;

        // negative ones are to make all three rotate clockwise
        if (float.TryParse(xRotationUI.text, out val)) zRot = val; else RefreshUIText(xRotationUI, zRot);
        if (float.TryParse(yRotationUI.text, out val)) xRot = val; else RefreshUIText(yRotationUI, xRot);
        if (float.TryParse(zRotationUI.text, out val)) yRot = val; else RefreshUIText(zRotationUI, yRot);

        selectedGraph.gameObject.transform.GetChild(0).transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);

        if (float.TryParse(stepUI.text, out val) && val > 0.0001f) selectedGraph.graphSettings.step = val; else RefreshUIText(stepUI, selectedGraph.graphSettings.step);

        selectedGraph.ScaleGraph();

        selectedGraph.ReVisualize();
    }

    // updates the entire UI to have the current graph settings displayed
    public void RefreshAllUIText() {
        RefreshUIText(xAxisMinUI, selectedGraph.graphSettings.xMin);
        RefreshUIText(xAxisMaxUI, selectedGraph.graphSettings.xMax);
        RefreshUIText(yAxisMinUI, selectedGraph.graphSettings.yMin);
        RefreshUIText(yAxisMaxUI, selectedGraph.graphSettings.yMax);
        RefreshUIText(zAxisMinUI, selectedGraph.graphSettings.zMin);
        RefreshUIText(zAxisMaxUI, selectedGraph.graphSettings.zMax);
        RefreshUIText(xRotationUI, selectedGraph.gameObject.transform.GetChild(0).transform.localRotation.y);
        RefreshUIText(yRotationUI, selectedGraph.gameObject.transform.GetChild(0).transform.localRotation.z);
        RefreshUIText(zRotationUI, selectedGraph.gameObject.transform.GetChild(0).transform.localRotation.x);
        RefreshUIText(stepUI, selectedGraph.graphSettings.step);
    }

    // updates a specific UI element's text to a value
    public void RefreshUIText(TMP_InputField textField, float value) {
        textField.text = value.ToString("G");
    }

    // for now it just sets rotation to 0, 0, 0
    public void ResetToDefault() {
        // reset to original settings
        selectedGraph.graphSettings = selectedGraph.originalSettings;

        // reset rotation
        selectedGraph.gameObject.transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, 0);

        // reset all UI
        RefreshAllUIText();
    }
}

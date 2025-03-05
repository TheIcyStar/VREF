using UnityEngine;
using System.Collections.Generic;

public class GraphManager : MonoBehaviour
{
    // equation renderer types
    public const int TYPE_LINE = 0;
    public const int TYPE_MESH = 1;

    // list of graphs currently in the scene
    private List<GameObject> graphs;

    public void Start() {
        graphs = new List<GameObject>();
    }

    public void CreateNewGraph(ParseTreeNode equationTree) {
        int equationType = DetermineEquationType(equationTree);
        graph = CreateNewGraphObject();
        graph.equationTree = equationTree;
        graph.GetComponent<EquationGrapher>().GraphEquation();
        graphs.Add(graphs);
    }

    private GameObject CreateNewGraphObject() {
        // create object
        // attach script
        // could add other stuff too
        // should determine equation type and append the correct renderer (line vs mesh)
    }

    private int DetermineEquationType(ParseTreeNode equationTree) {

    }
}

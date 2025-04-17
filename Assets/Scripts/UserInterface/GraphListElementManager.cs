using Codice.Client.BaseCommands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphListElementManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI graphListName;
    [SerializeField] private TextMeshProUGUI selectButtonText;
    [SerializeField] private Image backgroundPanel;
    [SerializeField] public GameObject equationListObj;
    private GraphInstance attachedGraph;
    private bool selected = false;

    public void Intialize(GraphInstance instance, int graphCount) {
        attachedGraph = instance;
        graphListName.text = $"Graph {graphCount}";
        SelectGraph();
    }

    // button press
    public void SelectGraphField() {
        GraphManager.instance.SetSelectedGraph(attachedGraph, this);
    }

    public void SelectGraph() {
        selected = true;
        UpdateSelection();
    }

    public void DeselectGraph() {
        selected = false;
        UpdateSelection();
    }

    private void UpdateSelection() {
        if (selectButtonText != null)
            selectButtonText.text = selected ? "Deselect" : "Select";

        if (backgroundPanel != null)
            backgroundPanel.color = selected ? Color.gray : Color.white;
    }

    public void DeleteGraph() {
        GraphManager.instance.DeleteGraphObject(attachedGraph, this);
    }

    public void RepositionGraph() {
        GraphManager.instance.PlaceGraphInFront(attachedGraph.gameObject);
    }

    public void  ExpandHideEquationList() {
        equationListObj.SetActive(!equationListObj.activeSelf);
        GraphManager.instance.RefreshGraphListUI();
    }
}

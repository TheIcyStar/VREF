using TMPro;
using UnityEngine;

public class GraphListElementManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI graphListName;
    private GraphInstance attachedGraph;

    public void Intialize(GraphInstance instance, int graphCount) {
        attachedGraph = instance;
        graphListName.text = $"Graph {graphCount}";
    }

    public void SelectGraph() {
        GraphManager.instance.SetSelectedGraph(attachedGraph);
    }
}

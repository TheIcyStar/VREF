using TMPro;
using UnityEngine;

public class EquationListElementManager : MonoBehaviour
{
    [SerializeField] public TMP_Text equationText;
    private ParseTreeNode equationTree;
    private GraphInstance linkedInstance;

    public void Initialize(ParseTreeNode equation, GraphInstance instance) {
        equationTree = equation;
        linkedInstance = instance;
    }

    public void DeleteEquation() {
        GraphManager.instance.DeleteEquationUIElement(equationTree, linkedInstance, this);
    }
}

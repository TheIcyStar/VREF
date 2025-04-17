using TMPro;
using UnityEngine;

public class EquationListElementManager : MonoBehaviour
{
    [SerializeField] public TMP_Text equationText;
    private ParseTreeNode equationTree;
    private GraphInstance linkedInstance;
    
    [SerializeField] private Material color1;
    [SerializeField] private Material color2;
    [SerializeField] private Material color3;
    [SerializeField] private Material color4;
    [SerializeField] private Material color5;
    [SerializeField] private Material color6;

    [SerializeField] private TMP_Dropdown materialDropdown;
    
    private Material[] materialOptions;

    public void Initialize(ParseTreeNode equation, GraphInstance instance) {
        materialOptions = new Material[] { color1, color2, color3, color4, color5, color6 };

        equationTree = equation;
        linkedInstance = instance;

        materialDropdown.ClearOptions();

        string[] customNames = new string[] {
            "Black",
            "Red",
            "Green",
            "Blue",
            "Transparent Light Blue",
            "Shaded Light Blue"
        };

        for (int i = 0; i < materialOptions.Length; i++) {
            materialDropdown.options.Add(new TMP_Dropdown.OptionData { text = customNames[i] });
        }

        materialDropdown.onValueChanged.AddListener(OnMaterialChanged);
    }

    public void DeleteEquation() {
        GraphManager.instance.DeleteEquationUIElement(equationTree, linkedInstance, this);
    }

    private void OnMaterialChanged(int index) {
        Material selectedMat = materialOptions[index];
        linkedInstance.SetNewMaterial(selectedMat, equationTree);
    }
}

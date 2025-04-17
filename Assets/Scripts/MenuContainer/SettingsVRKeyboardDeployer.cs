using TMPro;
using UnityEngine;

public class SettingsVRKeyboardDeployer : MonoBehaviour
{
    public TMP_InputField xAxisMinUI;
    public TMP_InputField xAxisMaxUI;
    public TMP_InputField yAxisMinUI;
    public TMP_InputField yAxisMaxUI;
    public TMP_InputField zAxisMinUI;
    public TMP_InputField zAxisMaxUI;
    public TMP_InputField xRotationUI;
    public TMP_InputField yRotationUI;
    public TMP_InputField zRotationUI;
    public TMP_InputField stepUI;

    
    public Transform playerHead;
    public GameObject VRKeyboard;

    private const float KB_DISTANCE = 1.5f;
    private const float KB_HEIGHT_OFFSET = 0.75f;

    public void showKeyboard(string inputFieldName) {
        Vector3 kbOffset = playerHead.forward;
        kbOffset.y = 0;
        kbOffset.Normalize();

        VRKeyboard.transform.position = playerHead.TransformPoint(playerHead.position) + (kbOffset * KB_DISTANCE) + (Vector3.down * KB_HEIGHT_OFFSET);
        VRKeyboard.transform.rotation = Quaternion.Euler(45, playerHead.transform.rotation.eulerAngles.y, 0);

        SettingsKeyboardController kbController = VRKeyboard.GetComponent<SettingsKeyboardController>();

        switch (inputFieldName) {
            case "xAxisMinUI": kbController.targetField = xAxisMinUI; break;
            case "xAxisMaxUI": kbController.targetField = xAxisMaxUI; break;
            case "yAxisMinUI": kbController.targetField = yAxisMinUI; break;
            case "yAxisMaxUI": kbController.targetField = yAxisMaxUI; break;
            case "zAxisMinUI": kbController.targetField = zAxisMinUI; break;
            case "zAxisMaxUI": kbController.targetField = zAxisMaxUI; break;
            case "xRotationUI": kbController.targetField = xRotationUI; break;
            case "yRotationUI": kbController.targetField = yRotationUI; break;
            case "zRotationUI": kbController.targetField = zRotationUI; break;
            case "stepUI": kbController.targetField = stepUI; break;
            default:
                Debug.LogWarning($"VRKeyboardDeployer: {inputFieldName} is an invalid name");
                return;
        }

        VRKeyboard.SetActive(true);
    }

    public void hideKeyboard(){
        VRKeyboard.SetActive(false);
    }

}


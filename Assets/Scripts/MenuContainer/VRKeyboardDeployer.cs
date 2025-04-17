using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VRKeyboardDeployer : MonoBehaviour {
    public Transform playerHead;
    public GameObject VRKeyboard;

    public TMP_InputField joinRoomHostnameField;
    public TMP_InputField joinRoomIdField;
    public TMP_InputField createRoomHostnameField;

    private const float KB_DISTANCE = 1.5f;
    private const float KB_HEIGHT_OFFSET = 0.75f; //Down, from the player's head



    public void showKeyboard(string inputFieldName) {
        Vector3 kbOffset = playerHead.forward;
        kbOffset.y = 0;
        kbOffset.Normalize();

        VRKeyboard.transform.position = playerHead.TransformPoint(playerHead.position) + (kbOffset * KB_DISTANCE) + (Vector3.down * KB_HEIGHT_OFFSET);
        VRKeyboard.transform.rotation = Quaternion.Euler(45, playerHead.transform.rotation.eulerAngles.y, 0);

        KeyboardController kbController = VRKeyboard.GetComponent<KeyboardController>();

        if(inputFieldName == "JoinRoomHostname"){
            kbController.targetField = joinRoomHostnameField;
        } else if(inputFieldName == "JoinRoomId") {
            kbController.targetField = joinRoomIdField;
        } else if(inputFieldName == "CreateRoomHostname") {
            kbController.targetField = createRoomHostnameField;
        } else {
            Debug.LogWarning($"VRKeyboardDeployer: {inputFieldName} is an invalid name");
        }

        VRKeyboard.SetActive(true);
    }

    public void hideKeyboard(){
        VRKeyboard.SetActive(false);
    }
}
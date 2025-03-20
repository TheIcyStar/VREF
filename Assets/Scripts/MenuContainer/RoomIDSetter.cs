using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoomIDSetter : MonoBehaviour {
    public GameObject roomInputField;
    public GameObject createRoomButton;
    public GameObject roomsUIContainer; //which holds JoinController
    public GameObject roomIdDisplay;

    private JoinController joinController;

    public void Start() {
        joinController = roomsUIContainer.GetComponent<JoinController>();
    }

    public void setRoomIdWithInput() {
        string roomId = roomInputField.GetComponent<TMP_InputField>().text;
        joinController.roomId = roomId;

        if(roomIdDisplay != null){
            roomIdDisplay.GetComponent<TMP_Text>().text = roomId;
        }
    }

    public void createRoomIdButton() {
        string roomId = "123auto";
        joinController.roomId = roomId;
        createRoomButton.GetComponentInChildren<Button>().interactable = false;

        if(roomIdDisplay != null){
            roomIdDisplay.GetComponent<TMP_Text>().text = "Room code: "+roomId;
        }
    }
}
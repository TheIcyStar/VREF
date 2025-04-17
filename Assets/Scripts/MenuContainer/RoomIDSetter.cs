using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;


public class RoomIDSetter : MonoBehaviour {
    public GameObject roomInputField;
    public GameObject createRoomButton;
    public GameObject roomsUIContainer; //which holds JoinController
    public GameObject roomIdDisplay;
    public GameObject hostInputField;

    private JoinController joinController;
    private TMP_Text buttonText;

    public void Start() {
        joinController = roomsUIContainer.GetComponent<JoinController>();
        if(buttonText != null){
            buttonText = createRoomButton.GetComponentInChildren<TMP_Text>();
        }
    }

    public void setRoomIdWithInput() {
        string roomId = roomInputField.GetComponent<TMP_InputField>().text;
        ServerConnection.instance.roomId = roomId;

        if(roomIdDisplay != null){
            roomIdDisplay.GetComponent<TMP_Text>().text = roomId;
        }
    }

    public async void createRoomIdButton() {
        string hostname = hostInputField.GetComponent<TMP_InputField>().text; //TODO: auto-prepend the url with http:// or https://
        if(hostname == ""){
            buttonText.text = "Need hostname!";
            createRoomButton.GetComponentInChildren<Button>().interactable = true;
            Invoke("resetButtonText", 3);
            return;
        }

        createRoomButton.GetComponentInChildren<Button>().interactable = false;


        using (UnityWebRequest webRequest = UnityWebRequest.Post($"{hostname}/autoCreate", "{}", "application/json")){
            webRequest.SetRequestHeader("InsecureHttpOption", "AlwaysAllowed"); //Todo: Make server run HTTPS and remove later
            try {
                await webRequest.SendWebRequest();
                while(!webRequest.isDone){
                    await Task.Yield();
                }

                if(webRequest.result != UnityWebRequest.Result.Success){
                    buttonText.text = "Host not found!";
                    createRoomButton.GetComponentInChildren<Button>().interactable = true;
                    Invoke("resetButtonText", 3);
                    return;
                }

                API_POST_CreateResult response = JsonConvert.DeserializeObject<API_POST_CreateResult>(webRequest.downloadHandler.text);
                ServerConnection.instance.roomId = response.data.roomId;
                ServerConnection.instance.updateToken = response.data.ownerUpdateToken;
                buttonText.text = "Created";

                if(roomIdDisplay != null){
                    roomIdDisplay.GetComponent<TMP_Text>().text = "Room code: "+response.data.roomId;
                }

            } catch (Exception e) {
                Debug.LogError("Error while creating room: "+e.Message);
                createRoomButton.GetComponentInChildren<Button>().interactable = true;
                buttonText.text = "Host not found!";
                Invoke("resetButtonText", 3);
                return;
            }
        }
    }

    private void resetButtonText() {
        buttonText.text = "Create room";
    }
}
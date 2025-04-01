using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System;


enum ServerResponseStatus {
    OK,
    HOST_UNREACHABLE,
    HOST_OLD_PROTOCOL,
    ROOM_NOT_FOUND
}

public class JoinController : MonoBehaviour {
    public GameObject hostInputField;
    public GameObject joinButton;
    private TMP_Text buttonTextComponent;

    void Start(){
        if(joinButton != null){
            buttonTextComponent = joinButton.GetComponentInChildren<TMP_Text>();
        }
    }

    public async void beginJoin() {
        string hostname = hostInputField.GetComponent<TMP_InputField>().text; //TODO: auto-prepend the url with http:// or https://

        if(hostname == ""){
            buttonTextComponent.text = "Need hostname!";
            Invoke("resetButtonText", 3);
            return;
        }
        if(ServerConnection.instance.roomId == ""){
            buttonTextComponent.text = "Need room ID!";
            Invoke("resetButtonText", 3);
            return;
        }


        //Check the server
        buttonTextComponent.text = "Connecting...";
        ServerResponseStatus status = await checkConnection(hostname);

        if(status == ServerResponseStatus.HOST_UNREACHABLE) {
            buttonTextComponent.text = "Host not found";
            Invoke("resetButtonText", 3);
            return;
        } else if(status == ServerResponseStatus.HOST_OLD_PROTOCOL){
            buttonTextComponent.text = "Old server :(";
            Invoke("resetButtonText", 3);
            return;
        }

        //Server connection OK, check the room
        status = await checkRoom(hostname, ServerConnection.instance.roomId);

        if(status != ServerResponseStatus.OK) {
            buttonTextComponent.text = "Room not found";
            Invoke("resetButtonText", 3);
            return;
        }

        buttonTextComponent.text = "Joining...";
        ServerConnection.instance.hostname = hostname;

        SceneManager.LoadScene(1);
    }

    public void beginJoinSolo() {
        SceneManager.LoadScene(1);
    }

    //Async function for checking if the host and room number are OK
    private async Task<ServerResponseStatus> checkConnection(string inputText) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(inputText)){
            webRequest.SetRequestHeader("InsecureHttpOption", "AlwaysAllowed"); //Todo: Make server run HTTPS and remove later
            try {
                await webRequest.SendWebRequest();


                while(!webRequest.isDone){
                    await Task.Yield();
                }

                if(webRequest.result != UnityWebRequest.Result.Success){
                    return ServerResponseStatus.HOST_UNREACHABLE;
                }

                API_ServerPingResponse parsedJson = JsonUtility.FromJson<API_ServerPingResponse>(webRequest.downloadHandler.text);

                if(parsedJson.protocolVersion != ACCEPTED_PROTOCOL.VERSION){
                    return ServerResponseStatus.HOST_OLD_PROTOCOL;
                }
            } catch(Exception e) {
                Debug.LogError("Error while connecting to server: "+e.Message);
                return ServerResponseStatus.HOST_UNREACHABLE;
            }

            return ServerResponseStatus.OK;
        }
    }

    private async Task<ServerResponseStatus> checkRoom(string host, string room) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(host+"/rooms/"+room)) {
            webRequest.SetRequestHeader("InsecureHttpOption", "AlwaysAllowed"); //Todo: Make server run HTTPS and remove later
            await webRequest.SendWebRequest();

            while(!webRequest.isDone){
                await Task.Yield();
            }

            if(webRequest.result != UnityWebRequest.Result.Success){
                return ServerResponseStatus.HOST_UNREACHABLE;
            }

            if(webRequest.responseCode == 404){
                return ServerResponseStatus.ROOM_NOT_FOUND;
            }

            return ServerResponseStatus.OK;
        }
    }

    private void resetButtonText() {
        buttonTextComponent.text = "Join Room";
    }
}

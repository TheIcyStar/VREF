using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Threading.Tasks;


enum ServerResponseStatus {
    OK,
    HOST_UNREACHABLE,
    HOST_OLD_PROTOCOL,
    ROOM_NOT_FOUND
}

public class JoinController : MonoBehaviour {
    public GameObject hostInputField;
    public GameObject joinButton;
    public string roomId;
    private TMP_Text buttonTextComponent;

    void Start(){
        buttonTextComponent = joinButton.GetComponentInChildren<TMP_Text>();
    }

    public async void beginJoin() {
        string hostname = hostInputField.GetComponent<TMP_InputField>().text; //TODO: auto-prepend the url with http:// or https://

        if(hostname == ""){
            buttonTextComponent.text = "Need hostname!";
            Invoke("resetButtonText", 3);
            return;
        }
        if(roomId == ""){
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

        buttonTextComponent.text = "Checking room...";

        //Server connection OK, check the room
        status = await checkRoom(hostname, roomId);

        if(status != ServerResponseStatus.OK) {
            buttonTextComponent.text = "Room not found";
            Invoke("resetButtonText", 3);
            return;
        }

        buttonTextComponent.text = "Joining...";
        ServerConnection.instance.hostname = hostname;
        ServerConnection.instance.roomId = roomId;
    }

    //Async function for checking if the host and room number are OK
    private async Task<ServerResponseStatus> checkConnection(string inputText) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(inputText)){
            await webRequest.SendWebRequest();


            while(!webRequest.isDone){
                await Task.Yield();
            }

            if(webRequest.result != UnityWebRequest.Result.Success){
                return ServerResponseStatus.HOST_UNREACHABLE;
            }

            API_ServerPingResponse parsedJson = JsonUtility.FromJson<API_ServerPingResponse>(webRequest.downloadHandler.text);
            if(parsedJson.protocolVersion != 0){
                return ServerResponseStatus.HOST_OLD_PROTOCOL;
            }

            return ServerResponseStatus.OK;
        }
    }

    private async Task<ServerResponseStatus> checkRoom(string host, string room) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(host+"/rooms/"+room)) {
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

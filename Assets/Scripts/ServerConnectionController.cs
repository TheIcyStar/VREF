using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;


public class ServerConnection : MonoBehaviour {
    public string hostname;
    public string roomId;
    public string updateToken;
    public static ServerConnection instance {get; private set;}

    private long lastSync = DateTime.Now.Ticks;
    private const int SYNC_FREQUENCY_MS = 5000;

    public void Awake() { //Ensures there's only one ServerConnection object
        if(instance != null & instance != this){
            Destroy(this);
        } else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void Update() {
        if(DateTime.Now.Ticks - lastSync < SYNC_FREQUENCY_MS){ return; }
        lastSync = DateTime.Now.Ticks;

        if(updateToken != null) { // Push to server
            Debug.Log("Push");
        } else { // Get from server
            Debug.Log($"Get");
            _ = getRoomState();
        }
    }

    private async Task<ParseTreeNode> getRoomState() {
        Debug.Log("asdf");
        using (UnityWebRequest webRequest = UnityWebRequest.Get($"{hostname}/rooms/{roomId}")){
            webRequest.SetRequestHeader("InsecureHttpOption", "AlwaysAllowed"); //Todo: Make server run HTTPS and remove later
            webRequest.SetRequestHeader("Content-Type", "application/json");
            await webRequest.SendWebRequest();

            while(!webRequest.isDone){
                await Task.Yield();
            }

            if(webRequest.result != UnityWebRequest.Result.Success){
                return null;
            }

            APIResponse response = JsonUtility.FromJson<APIResponse>(webRequest.downloadHandler.text);
            Debug.Log($"Get response: {response.data.equations[0].ToJSON()}");
            return response.data.equations[0];
        }
    }

    private async Task<Boolean> pushRoomState(ParseTreeNode parseTree) {
        string jsonBody = $"{{\"key\": \"{updateToken}\", \"roomState\": {parseTree.ToJSON()}}}";

        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm($"{hostname}/rooms/{roomId}/updateState", jsonBody)){
            webRequest.SetRequestHeader("Content-Type", "application/json");
            await webRequest.SendWebRequest();
            while(!webRequest.isDone){
                await Task.Yield();
            }

            if(webRequest.result != UnityWebRequest.Result.Success){
                return false;
            }

            return true;
        }
    }

}

public class PushObject {
    public string key;
    public RoomStateData roomState;

}
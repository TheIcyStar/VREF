using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Newtonsoft.Json;


public class ServerConnection : MonoBehaviour {
    public string hostname;
    public string roomId;
    public string updateToken;
    public static ServerConnection instance {get; private set;}

    private long lastSync = DateTime.Now.Ticks;
    private const int SYNC_FREQUENCY_MS = 1000;

    public void Awake() { //Ensures there's only one ServerConnection object
        if(instance != null & instance != this){
            Destroy(this);
        } else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void Update() {
        if(DateTime.Now.Ticks - lastSync < SYNC_FREQUENCY_MS * 10000){ return; }
        lastSync = DateTime.Now.Ticks;

        if(hostname.Length == 0 || roomId.Length == 0){ return; }

        if(updateToken != null && updateToken.Length > 0) { // Push to server
            _ = pushRoomState(GraphManager.instance.GetGraphs(), GraphManager.instance.globalGraphSettings);
        } else { // Get from server
            _ = getAndUseRoomState();
        }
    }

    /// <summary>
    /// Fetches the room state and sets settings and equations appropriately
    /// </summary>
    /// <returns></returns>
    private async Task getAndUseRoomState() {
        using (UnityWebRequest webRequest = UnityWebRequest.Get($"{hostname}/rooms/{roomId}")){
            webRequest.SetRequestHeader("InsecureHttpOption", "AlwaysAllowed"); //Todo: Make server run HTTPS and remove later
            webRequest.SetRequestHeader("Content-Type", "application/json");
            await webRequest.SendWebRequest();

            while(!webRequest.isDone){
                await Task.Yield();
            }

            if(webRequest.result != UnityWebRequest.Result.Success){
                Debug.LogWarning($"Webrequest was not successful: {webRequest.error}");
                return;
            }

            API_GET_RoomState response = JsonConvert.DeserializeObject<API_GET_RoomState>(webRequest.downloadHandler.text);

            GraphManager.instance.globalGraphSettings = response.data.settings;
            GraphManager.instance.BulkOverwriteGraphs(response.data.equations);
        }
    }

    /// <summary>
    /// Pushes the equations (token parse trees) and the room settings to the server
    /// </summary>
    /// <param name="parseTrees"> An array of ParseTreeNodes</param>
    /// <param name="graphSettings"></param>
    /// <returns></returns>
    private async Task<Boolean> pushRoomState(ParseTreeNode[] parseTrees, GraphSettings graphSettings) {
        API_POST_RoomState newPostBody = new API_POST_RoomState{
            key = updateToken,
            roomState = new RoomInfo_RoomState{
                settings = graphSettings,
                equations = parseTrees
            }
        };

        string jsonBody = JsonConvert.SerializeObject(newPostBody);
        using (UnityWebRequest webRequest = UnityWebRequest.Post($"{hostname}/rooms/{roomId}/updatestate", jsonBody, "application/json")){
            await webRequest.SendWebRequest();
            while(!webRequest.isDone){
                await Task.Yield();
            }

            if(webRequest.result != UnityWebRequest.Result.Success){
                Debug.LogWarning($"Webrequest was not successful: {webRequest.error}");
                return false;
            }

            return true;
        }
    }
}
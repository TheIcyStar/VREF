using UnityEngine;

public class ServerConnection : MonoBehaviour {
    public string hostname;
    public string roomId;
    public string updateToken;
    public static ServerConnection instance {get; private set;}

    public void Awake() { //Ensures there's only one ServerConnection object
        if(instance != null & instance != this){
            Destroy(this);
        } else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
